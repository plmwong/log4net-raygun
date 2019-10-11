using System.Collections.Generic;
using System.Linq;

namespace log4net.Raygun.Core
{
	public sealed class IgnoredDataSettings
	{		
		public IgnoredDataSettings(string ignoredFormNames = null,
			                         string ignoredHeaderNames = null,
			                         string ignoredCookieNames = null,
			                         string ignoredServerVariableNames = null,
			                         string ignoredQueryParameterNames = null,
			                         string ignoredSensitiveFieldNames = null,
			                         bool isRawDataIgnored = false)
		{
			IgnoredFormNames           = SplitIgnoredSetting(ignoredFormNames);
			IgnoredHeaderNames         = SplitIgnoredSetting(ignoredHeaderNames);
			IgnoredCookieNames         = SplitIgnoredSetting(ignoredCookieNames);
			IgnoredServerVariableNames = SplitIgnoredSetting(ignoredServerVariableNames);
			IgnoredQueryParameterNames = SplitIgnoredSetting(ignoredQueryParameterNames);
			IgnoredSensitiveFieldNames = SplitIgnoredSetting(ignoredSensitiveFieldNames);
      IsRawDataIgnored           = isRawDataIgnored;
		}

		private IEnumerable<string> SplitIgnoredSetting(string setting) 
		{
			return string.IsNullOrEmpty(setting) ? Enumerable.Empty<string>() : setting.Split(',').ToList();
		}

		public IEnumerable<string> IgnoredFormNames { get; set; }

		public IEnumerable<string> IgnoredHeaderNames { get; set; }

		public IEnumerable<string> IgnoredCookieNames { get; set; }

		public IEnumerable<string> IgnoredServerVariableNames { get; set; }
		
		public IEnumerable<string> IgnoredQueryParameterNames { get; set; }
		
		public IEnumerable<string> IgnoredSensitiveFieldNames { get; set; }
		
		public bool IsRawDataIgnored { get; set; }
	}
}

