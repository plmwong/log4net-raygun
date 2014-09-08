using System.Reflection;

namespace log4net.Raygun
{
    public interface IAssembly
    {
        Assembly GetEntryAssembly();
    }
}