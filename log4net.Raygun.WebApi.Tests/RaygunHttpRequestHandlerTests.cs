using System.Net.Http;
using System.Threading;
using NUnit.Framework;

namespace log4net.Raygun.WebApi.Tests
{
    [TestFixture]
    public class RaygunHttpRequestHandlerTests
    {
        [Test]
        public void WhenAnHttpRequestMessageIsReceivedThenStoreTheRequestInTheLogicalThreadContext()
        {
            var handler = new RaygunHttpRequestHandlerWrapper();

            var httpRequestMessage = new HttpRequestMessage();
            handler.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.That(LogicalThreadContext.Properties["log4net.Raygun.WebApi.HttpRequestMessage"], Is.EqualTo(httpRequestMessage));
        }
    }
}