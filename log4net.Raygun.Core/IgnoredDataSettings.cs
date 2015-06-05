using System.Collections.Generic;
using System.Linq;

namespace log4net.Raygun.Core
{
	public sealed class IgnoredDataSettings
	{
		public IgnoredDataSettings(string ignoredFormNames = null, string ignoredHeaderNames = null,
            string ignoredCookieNames = null, string ignoredServerVariableNames = null, bool isRawDataIgnored = false)
		{
			IgnoredFormNames = SplitIgnoredSetting(ignoredFormNames);
			IgnoredHeaderNames = SplitIgnoredSetting(ignoredHeaderNames);
			IgnoredCookieNames = SplitIgnoredSetting(ignoredCookieNames);
			IgnoredServerVariableNames = SplitIgnoredSetting(ignoredServerVariableNames);
            IsRawDataIgnored = isRawDataIgnored;
		}

		private IEnumerable<string> SplitIgnoredSetting(string setting) 
		{
			return string.IsNullOrEmpty(setting) ? Enumerable.Empty<string>() : setting.Split(',').ToList();
		}

		public IEnumerable<string> IgnoredFormNames { get; private set; }

		public IEnumerable<string> IgnoredHeaderNames { get; private set; }

		public IEnumerable<string> IgnoredCookieNames { get; private set; }

		public IEnumerable<string> IgnoredServerVariableNames { get; private set; }

        public bool IsRawDataIgnored { get; private set; }
	}
}

