using log4net.Raygun.Core;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun.Mvc
{
    public class RaygunClientAdapter : IRaygunClient
    {
        private readonly RaygunClient _raygunClient;

        public RaygunClientAdapter(RaygunClient raygunClient)
        {
            _raygunClient = raygunClient;
        }

        public void Send(RaygunMessage raygunMessage)
        {
            _raygunClient.Send(raygunMessage);
        }
    }
}