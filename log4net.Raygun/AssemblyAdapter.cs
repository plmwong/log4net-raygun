using System.Reflection;

namespace log4net.Raygun
{
    public class AssemblyAdapter : IAssembly
    {
        public Assembly GetEntryAssembly()
        {
            return Assembly.GetEntryAssembly();
        }
    }
}