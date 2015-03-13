using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace log4net.Raygun.WebApi
{
    public class RaygunHttpRequestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LogicalThreadContext.Properties["log4net.Raygun.WebApi.HttpRequestMessage"] = request;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
