using System.Net;

namespace productTestApi.Exceptions
{
    public class UnauthorizedException : CustomException
    {
        public List<string>? ErrorMessages { get; }

        public HttpStatusCode StatusCode { get; }

        public UnauthorizedException(string message, List<string>? errors = default, HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
            : base(message)
        {
            ErrorMessages = errors;
            StatusCode = statusCode;
        }
    }
}
