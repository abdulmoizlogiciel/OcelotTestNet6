using Ocelot.Errors;

namespace OcelotTestNet6.Models
{
    public class ForbiddenError : Error
    {
        /// <summary>
        /// It will return with the HttpStatusCode: 403 (System.Net.HttpStatusCode.Forbidden)
        /// </summary>
        /// <param name="message"></param>
        public ForbiddenError(string message) : base(message, OcelotErrorCode.UnauthorizedError, 403)
        {
        }
    }
}
