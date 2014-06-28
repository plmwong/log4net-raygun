using System;

namespace log4net.Raygun.Filters
{
	public interface IExceptionFilter
	{
		Exception Filter(Exception exception);
	}
}

