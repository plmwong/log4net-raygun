using log4net.Raygun.Core;
using Mindscape.Raygun4Net.WebApi;

namespace log4net.Raygun.WebApi
{
    public class RaygunAppender : RaygunAppenderBase
    {
        public RaygunAppender() : base(new RaygunWebApiMessageBuilder(), RaygunClientFactoryMethod.From(apiKey => new RaygunWebApiClientAdapter(new RaygunWebApiClient(apiKey))))
        {
        }
    }
}