using AIaaS.Application.SignalR;
using AIaaS.Infrastructure.Data;
using AIaaS.WebAPI.Infrastructure;
using Amazon.CloudWatchLogs;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;
using System.Reflection;

const int MAX_BODY_SIZE = 250 * 1024 * 1024;

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

    if (builder.Environment.IsProduction())
    {
        configuration.WriteTo.AmazonCloudWatch(
          new CloudWatchSinkOptions
          {
              LogGroupName = builder.Configuration.GetValue<string>("AWS_LOG_GROUP_NAME"),
              LogStreamNameProvider = new DefaultLogStreamProvider(),
              TextFormatter = new JsonFormatter(renderMessage: true),
              BatchSizeLimit = 100,
              QueueSizeLimit = 10000,
              RetryAttempts = 5,
              CreateLogGroup = true,
          },
            new AmazonCloudWatchLogsClient(
                new BasicAWSCredentials(
                    builder.Configuration.GetValue<string>("AWS_ACCESS_KEY_ID"),
                    builder.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY"))));
    }
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.Limits.MaxRequestBodySize = MAX_BODY_SIZE;
});

builder.Services.Configure<FormOptions>(x =>
{
    //x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = MAX_BODY_SIZE; // if don't set default value is: 128 MB
    //x.MultipartHeadersLengthLimit = int.MaxValue;
});

var awsOptions = new AWSOptions
{
    Credentials = new BasicAWSCredentials(builder.Configuration.GetValue<string>("AWS_ACCESS_KEY_ID"), builder.Configuration.GetValue<string>("AWS_SECRET_ACCESS_KEY"))
};
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddScoped<ICustomAuthService, CustomAuthService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<EfContext>();

builder.Services.AddSingleton<IJwtValidationService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var issuer = configuration["JWT_ISSUER"];
    var audience = configuration["AUTH0_AUDIENCE"];
    //var secret = configuration["JWT_SECRET"];
    var jwksEndpoint = configuration["AUTH0_JWKS"];
    return new JwtValidationService(issuer, audience, jwksEndpoint, provider.GetService<ILogger<IJwtValidationService>>());
});

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

builder.Services.AddSignalR();

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


//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssemblyContaining<IWebApiMarker>();
//builder.Services.AddAutoMapper(typeof(IWebApiMarker));
//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IWebApiMarker>());
//builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

await app.InitialiseDatabaseAsync();

app.MapHealthChecks();

app.MapPost("testsr", async (string message, IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowCLient> context) =>
{
    await context.Clients.All.ReceiveMessage(message);
    return Results.NoContent();
});

app.MapHub<SignalRWorkflowRunHistoryHub>("/workflow-run-history-hub");

var requiredVars =
    new string[] {
          "PORT_WEBAPI",
          "CLIENT_ORIGIN_URL",
          "AUTH0_DOMAIN",
          "AUTH0_AUDIENCE",
          "AUTH0_JWKS",
          "JWT_ISSUER",
          "DATABASE_CONNECTIONSTRING",
          "AWS_LOG_GROUP_NAME",
          "AWS_ACCESS_KEY_ID",
          "AWS_SECRET_ACCESS_KEY",
          "AWS_BUCKET_NAME"
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