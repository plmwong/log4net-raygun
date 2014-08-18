using System.Reflection;

namespace log4net.Raygun.Tests.Fakes
{
    public class FakeAssemblyLoader : IAssembly
    {
        private readonly Assembly _assembly;

        public FakeAssemblyLoader(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Assembly GetEntryAssembly()
        {
            return _assembly;
        }
    }
}