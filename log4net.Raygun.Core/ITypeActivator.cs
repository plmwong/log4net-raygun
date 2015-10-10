using System;

namespace log4net.Raygun.Core
{
    public interface ITypeActivator
    {
        T Activate<T>(string typeName, Action<string> errorAction, T defaultInstance = null) where T : class;
    }
}

