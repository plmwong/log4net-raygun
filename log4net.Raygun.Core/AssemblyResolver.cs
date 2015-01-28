using System.Reflection;

namespace log4net.Raygun.Core
{
    public class AssemblyResolver
    {
        private readonly IAssembly _assemblyLoader;

        public AssemblyResolver() : this(new AssemblyAdapter())
        {
        }

        internal AssemblyResolver(IAssembly assemblyLoader)
        {
            _assemblyLoader = assemblyLoader;
        }

        public Assembly GetApplicationAssembly()
        {
            return _assemblyLoader.GetEntryAssembly();
        }
    }
}