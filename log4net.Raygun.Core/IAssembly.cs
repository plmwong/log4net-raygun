using System.Reflection;

namespace log4net.Raygun.Core
{
    public interface IAssembly
    {
        Assembly GetEntryAssembly();
    }
}