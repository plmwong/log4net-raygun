using System;
using log4net.Raygun.Filters;

namespace log4net.Raygun.Tests
{
	public class FakeExceptionFilter : IExceptionFilter
	{
		public Exception Filter(Exception exception)
		{
			return new TestException();
		}
	}
}

