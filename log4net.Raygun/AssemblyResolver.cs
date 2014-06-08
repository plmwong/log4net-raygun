using System.Reflection;

namespace log4net.Raygun
{
	public class AssemblyResolver
    {
		private readonly IHttpContext _httpContext;
		private const string AspNamespace = "ASP";

		public AssemblyResolver() : this(new HttpContextAdapter())
		{
		}

		internal AssemblyResolver(IHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

        public Assembly GetApplicationAssembly()
        {
            var baseWebApplicationAssembly = GetWebApplicationAssembly();

            if (baseWebApplicationAssembly != null)
            {
                return baseWebApplicationAssembly;
            }

            return Assembly.GetEntryAssembly();
        }

        private Assembly GetWebApplicationAssembly()
        {
			if (_httpContext != null && _httpContext.ApplicationInstance != null)
            {
				var webApplicationType = _httpContext.ApplicationInstance.GetType();

				if (webApplicationType != null)
				{
					while (webApplicationType.Namespace == AspNamespace) {
						webApplicationType = webApplicationType.BaseType;
					}

                    return webApplicationType.Assembly;
                }
            }

            return null;
        }
    }
}