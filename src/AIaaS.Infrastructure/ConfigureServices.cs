﻿using AIaaS.Domain.Interfaces;
using AIaaS.Infrastructure;
using AIaaS.Infrastructure.AWS;
using AIaaS.Infrastructure.Data;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        var connectionString = configuration["DATABASE_CONNECTIONSTRING"];

        //Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<EfContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(IInfrastructureMarker).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<EfContext>());
        services.AddScoped<ApplicationDbContextInitialiser>();
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IS3Service, S3Service>();
        //services
        //    .AddDefaultIdentity<ApplicationUser>()
        //    .AddRoles<IdentityRole>()
        //    .AddEntityFrameworkStores<ApplicationDbContext>();

        //services.AddTransient<IDateTime, DateTimeService>();
        //services.AddTransient<IIdentityService, IdentityService>();

        //services.AddAuthorization(options =>
        //    options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        return services;
    }
}
