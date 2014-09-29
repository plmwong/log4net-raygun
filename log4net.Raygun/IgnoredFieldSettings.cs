using System.Collections.Generic;
using System.Linq;

namespace log4net.Raygun
{
	public sealed class IgnoredFieldSettings
	{
		public IgnoredFieldSettings(string ignoredFormNames = null, string ignoredHeaderNames = null, string ignoredCookieNames = null, string ignoredServerVariableNames = null)
		{
			IgnoredFormNames = SplitIgnoredSetting(ignoredFormNames);
			IgnoredHeaderNames = SplitIgnoredSetting(ignoredHeaderNames);
			IgnoredCookieNames = SplitIgnoredSetting(ignoredCookieNames);
			IgnoredServerVariableNames = SplitIgnoredSetting(ignoredServerVariableNames);
		}

		private IEnumerable<string> SplitIgnoredSetting(string setting) 
		{
			return string.IsNullOrEmpty(setting) ? Enumerable.Empty<string>() : setting.Split(',').ToList();
		}

		public IEnumerable<string> IgnoredFormNames { get; private set; }

		public IEnumerable<string> IgnoredHeaderNames { get; private set; }

		public IEnumerable<string> IgnoredCookieNames { get; private set; }

		public IEnumerable<string> IgnoredServerVariableNames { get; private set; }
	}
}

