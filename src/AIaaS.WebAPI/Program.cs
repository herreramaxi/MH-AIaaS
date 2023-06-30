using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Infrastructure;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models.CustomAttributes;
using AIaaS.WebAPI.Services;
using AIaaS.WebAPI.Services.PredictionService;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

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
    serverOptions.Limits.MaxRequestBodySize = 60 * 1024 * 1024;
});

builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDbContext<EfContext>(options =>
                options.UseSqlServer(builder.Configuration.GetValue<string>("DATABASE_CONNECTIONSTRING")));

//builder.Services.AddScoped<AIaaSContext, AIaaSContext>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<EfContext>();

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IPredictionService, PredictionService>();
builder.Services.AddScoped<IPredictionBuilderDirector, PredictionBuilderDirector>();
builder.Services.AddScoped<ICustomAuthService, CustomAuthService>();

builder.Services.AddSingleton<IMyCustomService, MyCustomService>();
builder.Services.AddSingleton<IJwtValidationService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var issuer = configuration["JWT_ISSUER"]; // Replace with your Auth0 issuer
    var audience = configuration["AUTH0_AUDIENCE"]; // Replace with your Auth0 audience
    //var secret = configuration["JWT_SECRET"]; // Replace with your Auth0 secret
    var jwksEndpoint = configuration["AUTH0_JWKS"]; // Replace with your Auth0 secret
    return new JwtValidationService(issuer, audience, jwksEndpoint);
});

var workflowOperatorTypes = typeof(IWebApiMarker).Assembly
    .GetTypes()
    .Where(x => typeof(IWorkflowOperator).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);

foreach (var workflowType in workflowOperatorTypes)
{
    var serviceDescriptor = new ServiceDescriptor(typeof(IWorkflowOperator), workflowType, ServiceLifetime.Scoped);
    builder.Services.Add(serviceDescriptor);
}

var predictServiceBUilderTypes = typeof(IWebApiMarker).Assembly
    .GetTypes()
    .Where(x => typeof(IPredictServiceParameterBuilder).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
    .OrderBy(x => x.GetCustomAttribute<PredictServiceParameterBuilderAttribute>()?.Order ?? int.MaxValue);

foreach (var builderType in predictServiceBUilderTypes)
{
    var serviceDescriptor = new ServiceDescriptor(typeof(IPredictServiceParameterBuilder), builderType, ServiceLifetime.Scoped);
    builder.Services.Add(serviceDescriptor);
}

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AIaaS Api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
      {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
              {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
              },
              Scheme = "oauth2",
              Name = "Bearer",
              In = ParameterLocation.Header,

            },
            new List<string>()
          }
        });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();
app.MapHealthChecks();

app.CreateDbIfNotExists();

var requiredVars =
    new string[] {
          "PORT_WEBAPI",
          "CLIENT_ORIGIN_URL",
          "AUTH0_DOMAIN",
          "AUTH0_AUDIENCE",
          "AUTH0_JWKS",
          "JWT_ISSUER",
          "JWT_SECRET",
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

//app.UseErrorHandler();
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