using Ocelot.Errors;

namespace OcelotTestNet6.Models
{
    public class InternalError : Error
    {
        /// <summary>
        /// It will return with the HttpStatusCode: 500 (System.Net.HttpStatusCode.InternalServerError)
        /// </summary>
        /// <param name="message"></param>
        public InternalError(string message) : base(message, OcelotErrorCode.UnauthorizedError, 500)
        {
        }
    }
}
