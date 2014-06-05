using System.Reflection;
using System.Web;

namespace log4net.Raygun
{
    public class AssemblyResolver
    {
        public static Assembly GetApplicationAssembly()
        {
            var baseWebApplicationAssembly = GetWebApplicationAssembly();

            if (baseWebApplicationAssembly != null)
            {
                return baseWebApplicationAssembly;
            }

            return Assembly.GetEntryAssembly();
        }

        private static Assembly GetWebApplicationAssembly()
        {
            if (HttpContext.Current != null)
            {
                var baseWebApplicationType = HttpContext.Current.ApplicationInstance.GetType().BaseType;

                if (baseWebApplicationType != null)
                {
                    return baseWebApplicationType.Assembly;
                }
            }

            return null;
        }
    }
}