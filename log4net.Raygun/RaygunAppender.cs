using log4net.Raygun.Core;
using Mindscape.Raygun4Net;

namespace log4net.Raygun
{
    public class RaygunAppender : RaygunAppenderBase
    {
        public RaygunAppender() : base(new RaygunMessageBuilder(() => new HttpContextAdapter()), RaygunClientFactoryMethod.From(apiKey => new RaygunClientAdapter(new RaygunClient(apiKey))))
        {
        }
    }
}