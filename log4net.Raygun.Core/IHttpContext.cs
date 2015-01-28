using System.Web;

namespace log4net.Raygun.Core
{
    public interface IHttpContext
    {
        HttpContext Instance { get; }
        HttpApplication ApplicationInstance { get; }
    }
}