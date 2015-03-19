using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace log4net.Raygun.WebApi.Tests
{
    public class RaygunHttpRequestHandlerWrapper : RaygunHttpRequestHandler
    {
        public RaygunHttpRequestHandlerWrapper() : base(new NoOpHandler())
        {
        }

        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }
}