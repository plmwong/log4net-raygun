using System;
using log4net.Raygun.Core;
using System.Collections.Generic;

namespace log4net.Raygun.Tests.Fakes
{
    public class FakeTypeActivator : ITypeActivator
    {
        private readonly ITypeActivator _normalTypeActivator;

        public FakeTypeActivator(ITypeActivator normalTypeActivator = null)
        {
            _normalTypeActivator = normalTypeActivator ?? new TypeActivator(l => {});
        }

        public KeyValuePair<string, object> FakedType { get; set; }

        public T Activate<T>(string typeName, Action<string> errorAction, T defaultInstance = null) where T : class
        {
            if (FakedType.Key == typeName)
            {
                Console.WriteLine("Requested type to activate matches the configured faked type, returning the faked instance");

                return (T)FakedType.Value;
            }

            Console.WriteLine("Requested type to activate does not match, forwarding call to the regular type activator");

            return _normalTypeActivator.Activate<T>(typeName, errorAction, defaultInstance);
        }
    }
}

