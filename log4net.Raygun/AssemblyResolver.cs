using System.Reflection;

namespace log4net.Raygun
{
    public class AssemblyResolver
    {
        private readonly IHttpContext _httpContext;
        private readonly IAssembly _assemblyLoader;
        private const string AspNamespace = "ASP";

        public AssemblyResolver() : this(new HttpContextAdapter(), new AssemblyAdapter())
        {
        }

        internal AssemblyResolver(IHttpContext httpContext, IAssembly assemblyLoader)
        {
            _httpContext = httpContext;
            _assemblyLoader = assemblyLoader;
        }

        public Assembly GetApplicationAssembly()
        {
            var baseWebApplicationAssembly = GetWebApplicationAssembly();

            if (baseWebApplicationAssembly != null)
            {
                return baseWebApplicationAssembly;
            }

            return _assemblyLoader.GetEntryAssembly();
        }

        private Assembly GetWebApplicationAssembly()
        {
            if (_httpContext != null && _httpContext.ApplicationInstance != null)
            {
                var webApplicationType = _httpContext.ApplicationInstance.GetType();

                if (webApplicationType != null)
                {
                    while (webApplicationType.Namespace == AspNamespace)
                    {
                        webApplicationType = webApplicationType.BaseType;
                    }

                    return webApplicationType.Assembly;
                }
            }

            return null;
        }
    }
}