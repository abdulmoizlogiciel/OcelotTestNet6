using Microsoft.Extensions.Caching.Memory;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.Values;
using OcelotTestNet6.Utils;

namespace OcelotTest.Handlers
{
    public class CustomLoadBalancer : ILoadBalancer
    {
        private readonly IDictionary<string, List<OcelotTestNet6.Models.ServiceHostAndPort>> _customerWiseServices;
        private readonly IMemoryCache _memoryCache;
        private readonly object _lock = new();

        private int _last;

        public CustomLoadBalancer(IDictionary<string, List<OcelotTestNet6.Models.ServiceHostAndPort>> services, IMemoryCache memoryCache)
        {
            _customerWiseServices = services;
            _memoryCache = memoryCache;
        }

        public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
        {
            var services = _customerWiseServices[httpContext.User.Claims.First(x => x.Type.Equals("customer_id")).Value];

            lock (_lock)
            {
                if (_last >= services.Count)
                {
                    _last = 0;
                }

                OcelotTestNet6.Models.ServiceHostAndPort service = services[_last];

                // handeling websocket and subscription requests:
                var upstreamScheme = httpContext.Request.Scheme;
                if (upstreamScheme.StartsWith("ws") || upstreamScheme.Contains("subscribe", StringComparison.InvariantCultureIgnoreCase)) // either ws or wss
                {
                    // TODO: again try to do claims to header transformation:
                    string userIdentifier = httpContext.User.UserIdentifier();
                    if (_memoryCache.TryGetValue(userIdentifier, out int index))
                    {
                        service = services[index];
                        return Task.FromResult<Response<ServiceHostAndPort>>(new OkResponse<ServiceHostAndPort>(new ServiceHostAndPort(service.Host, service.Port)));
                    }

                    _memoryCache.Set(userIdentifier, _last);
                }

                _last++;

                // https://stackoverflow.com/a/34005587
                return Task.FromResult<Response<ServiceHostAndPort>>(new OkResponse<ServiceHostAndPort>(new ServiceHostAndPort(service.Host, service.Port)));
            }
        }

        public void Release(ServiceHostAndPort hostAndPort)
        {
        }
    }
}
