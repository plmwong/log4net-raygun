using System.Web;

namespace log4net.Raygun
{
	public interface IHttpContext
	{
		HttpApplication ApplicationInstance { get; }
	}
}

