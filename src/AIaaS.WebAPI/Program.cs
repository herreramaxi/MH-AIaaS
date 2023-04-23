using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Infrastructure;
using AIaaS.WebAPI.Middlewares;
using AIaaS.WebAPI.Services;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((configBuilder) =>
{
    configBuilder.Sources.Clear();
    DotEnv.Load();
    configBuilder.AddEnvironmentVariables();
});

builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration
    .Enrich.WithThreadId()
    .WriteTo.Console();

});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
});

builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDbContext<AIaaSContext>(options =>
                options.UseSqlServer(builder.Configuration.GetValue<string>("DATABASE_CONNECTIONSTRING")));

//builder.Services.AddScoped<AIaaSContext, AIaaSContext>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AIaaSContext>();

builder.Services.AddScoped<IMessageService, MessageService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetValue<string>("CLIENT_ORIGIN_URL"))
            //.WithHeaders(new string[] {
            //    HeaderNames.ContentType,
            //    HeaderNames.Authorization,
            //})
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            //.WithMethods("GET")
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var audience =
                  builder.Configuration.GetValue<string>("AUTH0_AUDIENCE");

            options.Authority =
                  $"https://{builder.Configuration.GetValue<string>("AUTH0_DOMAIN")}/";
            options.Audience = audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuerSigningKey = true
            };
        });

builder.Services.AddAuthorization(o =>
{

    o.AddPolicy("Administrator", p => p.
        RequireAuthenticatedUser().
        RequireRole("Administrator"));
});

//TODO: Check if disable on prod or not
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapHealthChecks();

app.CreateDbIfNotExists();

var requiredVars =
    new string[] {
          "PORT_WEBAPI",
          "CLIENT_ORIGIN_URL",
          "AUTH0_DOMAIN",
          "AUTH0_AUDIENCE",
          "DATABASE_CONNECTIONSTRING"
    };

foreach (var key in requiredVars)
{
    var value = app.Configuration.GetValue<string>(key);

    if (string.IsNullOrEmpty(value))
    {
        throw new Exception($"Config variable missing: {key}.");
    }
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapGet("/test", () =>
{
    return "hello world";
});


app.Urls.Add($"http://+:{app.Configuration.GetValue<string>("PORT_WEBAPI")}");

app.UseErrorHandler();
//app.UseSecureHeaders();
app.MapControllers()
    .RequireAuthorization();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
//app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.Run();