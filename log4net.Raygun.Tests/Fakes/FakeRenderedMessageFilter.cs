using log4net.Raygun.Filters;

namespace log4net.Raygun.Tests
{
	public class FakeRenderedMessageFilter : IRenderedMessageFilter
	{
		public static string ReplacementMessage { 
			get { return "I changed your message!"; }
		}

		public string Filter(string renderedMessage)
		{
			return ReplacementMessage;
		}
	}
}

