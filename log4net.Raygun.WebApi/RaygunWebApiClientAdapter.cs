using log4net.Raygun.Core;
using Mindscape.Raygun4Net.Messages;
using Mindscape.Raygun4Net.WebApi;

namespace log4net.Raygun.WebApi
{
    public class RaygunWebApiClientAdapter : IRaygunClient
    {
        private readonly RaygunWebApiClient _raygunClient;

        public RaygunWebApiClientAdapter(RaygunWebApiClient raygunClient)
        {
            _raygunClient = raygunClient;
        }

        public void Send(RaygunMessage raygunMessage)
        {
            _raygunClient.Send(raygunMessage);
        }
    }
}