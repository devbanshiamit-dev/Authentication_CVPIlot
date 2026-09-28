using JWTAuthenticationAPI.Exceptions;

namespace JWTAuthenticationAPI.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                var (statusCode, message) = exception switch
                {
                    ApiException apiException => (apiException.StatusCode, apiException.Message),
                    _ => (StatusCodes.Status500InternalServerError,
                        "An unexpected error occurred. Please try again later.")
                };

                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(exception,
                        "Unhandled exception for {Method} {Path}",
                        context.Request.Method, context.Request.Path);
                }
                else
                {
                    _logger.LogWarning(
                        "{ExceptionType} on {Method} {Path}: {Message}",
                        exception.GetType().Name,
                        context.Request.Method, context.Request.Path, message);
                }

                context.Response.Clear();
                context.Response.StatusCode = statusCode;

                await context.Response.WriteAsJsonAsync(new
                {
                    statusCode,
                    message
                });
            }
        }
    }
}
