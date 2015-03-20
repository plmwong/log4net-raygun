using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace log4net.Raygun.WebApi.Tests
{
    public class NoOpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}