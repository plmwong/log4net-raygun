using System;
using log4net.Raygun.Core;
using System.Collections.Generic;

namespace log4net.Raygun.Tests
{
    public class InterceptingTypeActivator : ITypeActivator
    {
        private readonly TypeActivator _normalTypeActivator;

        public InterceptingTypeActivator()
        {
            _normalTypeActivator = new TypeActivator(l => {});
        }

        public KeyValuePair<string, object> RegisteredInstance { get; set; }

        public T Activate<T>(string typeName, Action<string> errorAction, T defaultInstance = null) where T : class
        {
            if (RegisteredInstance.Key == typeName)
            {
                return (T)RegisteredInstance.Value;
            }
            
            return _normalTypeActivator.Activate<T>(typeName, errorAction, defaultInstance);
        }
    }
}

