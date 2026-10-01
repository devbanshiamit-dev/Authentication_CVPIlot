using JWTAuthenticationAPI.JWT;

namespace JWTAuthenticationAPI.Middlewares
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _request;

        public ValidationMiddleware(RequestDelegate request)
        {
            _request = request;
        }
        public async Task InvokeAsync(HttpContext context, JWTService jwt)
        {
            var Method = context.Request.Method;
            var Path = context.Request.Path;

            if (Method == HttpMethods.Post &&
                (Path.Equals("/api/user/login", StringComparison.OrdinalIgnoreCase) 
                || Path.Equals("/api/user/register",StringComparison.OrdinalIgnoreCase)))
            {
                await _request(context);
                return;
            }
            else
            {
                var authHeader = context.Request.Headers.Authorization
                    .ToString();

                if (!authHeader.StartsWith(
                    "Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    await UnauthorizeAsync(context);
                    return;
                }

                var token = authHeader["Bearer ".Length..].Trim();

                if (string.IsNullOrWhiteSpace(token))
                {
                    await UnauthorizeAsync(context);
                    return;
                }

                var principal = jwt.ValidateAccessToken(token);

                if(principal == null)
                {
                    await UnauthorizeAsync(context);
                    return;
                }

                context.User = principal;

                await _request(context);
            }
        }
        private async Task UnauthorizeAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Unauthorized"
            });
        }
    }
}
