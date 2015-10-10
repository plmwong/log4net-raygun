using log4net.Raygun.Core;
using log4net.Raygun.Tests.Fakes;

namespace log4net.Raygun.Tests
{
    public class FakeRaygunClientFactory : IRaygunClientFactory
    {
        private readonly FakeRaygunClient _instance;
        
        public FakeRaygunClientFactory() 
        {
            _instance = new FakeRaygunClient();
        }
        
        public FakeRaygunClient Instance
        {
            get
            {
                return _instance;
            }
        }
        
        public IRaygunClient Create(string apiKey)
        {
            return Instance;
        }
    }
}

