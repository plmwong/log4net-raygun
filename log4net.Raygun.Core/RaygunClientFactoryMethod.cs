using System;

namespace log4net.Raygun.Core
{
    public class RaygunClientFactoryMethod : IRaygunClientFactory
    {
        private readonly Func<string, IRaygunClient> _raygunFactoryMethod;

        public static RaygunClientFactoryMethod From(Func<string, IRaygunClient> raygunFactoryMethod)
        {
            return new RaygunClientFactoryMethod(raygunFactoryMethod);
        }

        public RaygunClientFactoryMethod(Func<string, IRaygunClient> raygunFactoryMethod)
        {
            _raygunFactoryMethod = raygunFactoryMethod;
        }

        public IRaygunClient Create(string apiKey)
        {
            return _raygunFactoryMethod(apiKey);
        }
    }
}