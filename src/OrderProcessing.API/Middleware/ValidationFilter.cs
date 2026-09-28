using System.ComponentModel.DataAnnotations;

namespace OrderProcessing.API.Middleware; // <-- Updated namespace

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arg = context.Arguments.OfType<T>().FirstOrDefault();

        if (arg is not null)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(arg);

            if (!Validator.TryValidateObject(arg, validationContext, validationResults, true))
            {
                var errors = validationResults
                    .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(r => r.ErrorMessage ?? "Invalid value").ToArray()
                    );

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}