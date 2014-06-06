using log4net.Core;
using System.Collections.Generic;

namespace log4net.Raygun.Tests
{
	public class FakeUserCustomDataBuilder : IUserCustomDataBuilder
	{
		public Dictionary<string, string> Build(LoggingEvent loggingEvent)
		{
			return new Dictionary<string, string>();
		}
	}
}

