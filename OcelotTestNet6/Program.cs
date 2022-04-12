using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ocelot.Middleware;
using Ocelot.DependencyInjection;
using OcelotTest.Handlers;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Configuration;
using OcelotTestNet6.Models;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

string authority = builder.Configuration["Identity:Authority"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(x =>
                {
                    x.Authority = authority;
                    x.RequireHttpsMetadata = authority.StartsWith("https");
                    x.ApiName = builder.Configuration["Identity:ApiName"];
                    x.JwtValidationClockSkew = TimeSpan.Zero;
                    x.TokenRetriever = new Func<HttpRequest, string>(req =>
                    {
                        var fromHeader = TokenRetrieval.FromAuthorizationHeader();
                        var fromQuery = TokenRetrieval.FromQueryString();
                        return fromHeader(req) ?? fromQuery(req);
                    });
                });

builder.Services.AddAuthorization();


var configuration = new ConfigurationBuilder().AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", false, false).Build();
builder.Services
    .AddOcelot(configuration)
    .AddCustomLoadBalancer(InitializerForCustomLoadBalancer);

builder.Services.AddMemoryCache();


var app = builder.Build();
// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    //app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", context => context.Response.WriteAsync("Ocelot Running!"));
});

app.UseWebSockets();

app.UseOcelot(new CustomOcelotPipelineConfig(app)).Wait();

app.Run();


static CustomLoadBalancer InitializerForCustomLoadBalancer(IServiceProvider serviceProvider, DownstreamRoute route, IServiceDiscoveryProvider serviceDiscoveryProvider)
{
    var webHost = serviceProvider.GetRequiredService<IWebHostEnvironment>();

    string serializedJson = File.ReadAllText($"./ocelot.services.{webHost.EnvironmentName}.json");
    var apiServices = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, IDictionary<string, List<ServiceHostAndPort>>>>(serializedJson);

    return new CustomLoadBalancer(apiServices[route.RequestIdKey], serviceProvider.GetRequiredService<IMemoryCache>());
}