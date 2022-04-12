using Ocelot.Middleware;
using OcelotTestNet6.Models;

namespace OcelotTest.Handlers
{
    public class CustomOcelotPipelineConfig : OcelotPipelineConfiguration
    {
        private readonly ILogger<CustomOcelotPipelineConfig> _logger;

        public CustomOcelotPipelineConfig(IApplicationBuilder applicationBuilder)
        {
            _logger = applicationBuilder.ApplicationServices.GetRequiredService<ILogger<CustomOcelotPipelineConfig>>();
            base.PreAuthorizationMiddleware = PreAuthorizationMiddleware;
        }

        private new Task PreAuthorizationMiddleware(HttpContext httpContext, Func<Task> nextService)
        {
            var defaultHttpContext = httpContext as DefaultHttpContext;
            if (defaultHttpContext.User.Claims.Any(claim => claim.Type.Equals("customer_id")))
            {
                // call the underline service.
                return nextService();
            }

            _logger.LogInformation("User does not have the customer_id Claim.");

            httpContext.Items.SetError(new ForbiddenError("Forbidden"));
            return Task.CompletedTask;
        }
    }
}
