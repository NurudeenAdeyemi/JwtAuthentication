using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using productTestApi.Exceptions;

namespace productTestApi.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _request;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public ExceptionMiddleware(RequestDelegate request, IHostEnvironment environment, ILogger<ExceptionMiddleware> logger)
    {
        _request = request;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _request(context);
        }
        catch (ValidationException exception)
        {
            _logger.LogInformation("A validation exception has occurred while executing the request: {ErrorMessage}",
                exception.Message);
            var errors = exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(failureGroup => failureGroup.Key,
                    failureGroup => failureGroup.Select(f => f.ErrorMessage).ToArray());
            var problemDetail = new ValidationProblemDetails(errors)
            {
                Title = "ValidationFailure",
                Detail = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };
            await Results.Json(problemDetail,
                _jsonSerializerOptions,
                statusCode: StatusCodes.Status400BadRequest
                ).ExecuteAsync(context);

        }
        catch (CustomException exception)
        {
            _logger.LogWarning(message: "A domain exception has occurred while executing the request.\n{ErrorMessage}", exception.Message);
            var error = _errors[(int)exception.StatusCode];
            var problemDetail = new ProblemDetails
            {
                Title = error.Title,
                Detail = exception.Message,
                Status = (int)exception.StatusCode,
                Instance = context.Request.Path,
                Type = error.Type
            };
            await Results.Json(problemDetail,
                statusCode: (int)exception.StatusCode,
                options: _jsonSerializerOptions
            ).ExecuteAsync(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception: exception, "An unhandled exception has occurred while executing the request.");
            var error = _errors[500];
            var problem = new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = _environment.IsDevelopment() ? exception.Message : error.Title,
                Status = StatusCodes.Status500InternalServerError,
                Instance = context.Request.Path,
                Type = error.Type
            };
            await Results.Json(
                problem,
                options: _jsonSerializerOptions,
                statusCode: StatusCodes.Status500InternalServerError
            ).ExecuteAsync(context);
        }
    }

    private readonly Dictionary<int, (string Type, string Title)> _errors = new()
    {
        [400] =
        (
            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            "Bad Request"
        ),

        [401] =
        (
            "https://tools.ietf.org/html/rfc7235#section-3.1",
            "Unauthorized"
        ),

        [403] =
        (
            "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            "Forbidden"
        ),

        [404] =
        (
            "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            "Not Found"
        ),

        [406] =
        (
            "https://tools.ietf.org/html/rfc7231#section-6.5.6",
            "Not Acceptable"
        ),

        [409] =
        (
            "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            "Conflict"
        ),

        [415] =
        (
            "https://tools.ietf.org/html/rfc7231#section-6.5.13",
            "Unsupported Media Type"
        ),

        [422] =
        (
            "https://tools.ietf.org/html/rfc4918#section-11.2",
            "Unprocessable Entity"
        ),

        [500] =
        (
            "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            "An error occurred while processing your request."
        ),
    };
}