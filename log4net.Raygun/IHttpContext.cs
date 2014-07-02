using System.Web;

namespace log4net.Raygun
{
	public interface IHttpContext
	{
        HttpContext Instance { get; }
		HttpApplication ApplicationInstance { get; }
	}
}

