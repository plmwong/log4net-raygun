using System.Web;

namespace log4net.Raygun
{
    public class HttpContextAdapter : IHttpContext
    {
        private readonly HttpContext _httpContext;

        public HttpContextAdapter()
        {
            _httpContext = HttpContext.Current;
        }

        public HttpContext Instance
        {
            get { return _httpContext; }
        }

        public HttpApplication ApplicationInstance
        {
            get { return _httpContext != null ? _httpContext.ApplicationInstance : null; }
        }
    }
}