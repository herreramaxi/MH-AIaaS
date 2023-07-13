using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace AIaaS.WebAPI.PipelineBehaviours
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);
            var validators = _validators.Select(v => v.ValidateAsync(context, cancellationToken));
            var validationResults = await Task.WhenAll(validators);
            var failures = validationResults
                .SelectMany(x => x.AsErrors())
                .ToList();

            if (failures.Count > 0)
            {
                var responseType = typeof(TResponse);

                if (responseType.IsGenericType)
                {
                    var genericType = responseType.GetGenericArguments().FirstOrDefault();
                    if (genericType is null || responseType.GetGenericArguments().Length > 1)
                    {
                        return Result<TResponse>.Invalid(failures);
                    }                                 

                    Type resultType = typeof(Result<>).MakeGenericType(genericType);
                    MethodInfo invalidMethod = resultType.GetMethod("Invalid", new[] { typeof(List<ValidationError>) });
                    
                    if (invalidMethod != null)
                    {
                        object result = invalidMethod.Invoke(null, new object[] { failures });
                     
                        return (TResponse)result;
                    }                    
                }

                return Result<TResponse>.Invalid(failures);
            }

            return await next();
        }
    }

}
