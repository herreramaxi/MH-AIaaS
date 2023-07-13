using AIaaS.Application.Common.Models.CustomAttributes;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.PipelineBehaviours;
using AIaaS.WebAPI.Services;
using AIaaS.WebAPI.Services.PredictionService;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            //cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });


        services.AddScoped<IOperatorService, OperatorService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IPredictionService, PredictionService>();
        services.AddScoped<IPredictionBuilderDirector, PredictionBuilderDirector>();

        var workflowOperatorTypes = typeof(IWebApiMarker).Assembly
            .GetTypes()
            .Where(x => typeof(IWorkflowOperator).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);

        foreach (var workflowType in workflowOperatorTypes)
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IWorkflowOperator), workflowType, ServiceLifetime.Scoped);
            services.Add(serviceDescriptor);
        }

        var predictServiceBUilderTypes = typeof(IWebApiMarker).Assembly
            .GetTypes()
            .Where(x => typeof(IPredictServiceParameterBuilder).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
            .OrderBy(x => x.GetCustomAttribute<PredictServiceParameterBuilderAttribute>()?.Order ?? int.MaxValue);

        foreach (var builderType in predictServiceBUilderTypes)
        {
            var serviceDescriptor = new ServiceDescriptor(typeof(IPredictServiceParameterBuilder), builderType, ServiceLifetime.Scoped);
            services.Add(serviceDescriptor);
        }

        return services;
    }
}
