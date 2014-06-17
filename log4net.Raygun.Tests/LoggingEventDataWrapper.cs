using log4net.Core;
using log4net.Util;
using System;

namespace log4net.Raygun.Tests
{
	public class LoggingEventDataWrapper 
	{
		public string LoggerName;

		public string Domain;

		public string ExceptionString;

		public string Identity;

		public string UserName;

		public LocationInfo LocationInfo;

		public DateTime TimeStamp;

		public string ThreadName;

		public string Message;

		public Level Level;

		public PropertiesDictionary Properties;

		public LoggingEventData ToLoggingEventData()
		{
			return new LoggingEventData 
			{
				LoggerName = LoggerName,
				Domain = Domain,
				ExceptionString = ExceptionString,
				Identity = Identity,
				UserName = UserName,
				LocationInfo = LocationInfo,
				TimeStamp = TimeStamp,
				ThreadName = ThreadName,
				Message = Message,
				Level = Level,
				Properties = Properties
			};
		}
	}
}