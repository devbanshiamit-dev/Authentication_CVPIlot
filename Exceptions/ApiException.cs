namespace JWTAuthenticationAPI.Exceptions
{
    public abstract class ApiException : Exception
    {
        public int StatusCode { get; }

        protected ApiException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public sealed class BadRequestException(string message)
        : ApiException(StatusCodes.Status400BadRequest, message);

    public sealed class UnauthorizedException(string message)
        : ApiException(StatusCodes.Status401Unauthorized, message);

    public sealed class NotFoundException(string message)
        : ApiException(StatusCodes.Status404NotFound, message);

    public sealed class ConflictException(string message)
        : ApiException(StatusCodes.Status409Conflict, message);
}
