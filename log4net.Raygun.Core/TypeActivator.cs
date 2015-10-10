using System;

namespace log4net.Raygun.Core
{
    public class TypeActivator : ITypeActivator
    {
        private readonly Action<string> _loggingAction;

        public TypeActivator(Action<string> loggingAction)
        {
            _loggingAction = loggingAction;
        }

        public T Activate<T>(string typeName, Action<string> errorAction, T defaultInstance = null) where T : class
        {
            if (typeName != null)
            {
                var toActivateType = Type.GetType(typeName);
                var intendedType = typeof(T);

                if (toActivateType != null)
                {
                    _loggingAction(string.Format("RaygunAppender: Activating instance of '{0}'", toActivateType.AssemblyQualifiedName));
                    var activatedType = Activator.CreateInstance(toActivateType) as T;

                    if (activatedType != null)
                    {
                        _loggingAction(string.Format("RaygunAppender: Activated instance '{0}'", toActivateType.AssemblyQualifiedName));

                        return activatedType;
                    }

                    errorAction(string.Format("RaygunAppender: Configured type '{0}' is not intended type '{1}'", toActivateType, intendedType.Name));
                }
                else
                {
                    errorAction(string.Format("RaygunAppender: Configured type '{0}' is not a type", toActivateType));
                }
            }

            return defaultInstance;
        }
    }
}

