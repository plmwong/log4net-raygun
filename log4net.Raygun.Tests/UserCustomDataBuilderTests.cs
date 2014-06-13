using System.Collections.Generic;
using log4net.Core;
using log4net.Util;
using NUnit.Framework;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class UserCustomDataBuilderTests
    {
        private UserCustomDataBuilder _userCustomDataBuilder;

        [SetUp]
        public void SetUp()
        {
            _userCustomDataBuilder = new UserCustomDataBuilder();
        }

		[Test]
		public void LoggingEventDomainIsAddedToUserCustomData()
		{
			LoggingEventAssert.When(l => l.Domain = "TestDomain")
				.Property(l => l.Domain)
				.ShouldMapTo(UserCustomDataBuilder.UserCustomDataKey.Domain);
		}

		[Test]
		public void LoggingEventIdentityIsAddedToUserCustomData()
		{
			LoggingEventAssert.When(l => l.Identity = "TestIdentity")
				.Property(l => l.Identity)
				.ShouldMapTo(UserCustomDataBuilder.UserCustomDataKey.Identity);
		}

		[Test]
		public void LoggingEventThreadNameIsAddedToUserCustomData()
		{
			LoggingEventAssert.When(l => l.ThreadName = "TestsThreadName")
				.Property(l => l.ThreadName)
				.ShouldMapTo(UserCustomDataBuilder.UserCustomDataKey.ThreadName);
		}

		[Test]
		public void LoggingEventUserNameIsAddedToUserCustomData()
		{
			LoggingEventAssert.When(l => l.UserName = "TestUserName")
				.Property(l => l.UserName)
				.ShouldMapTo(UserCustomDataBuilder.UserCustomDataKey.UserName);
		}

		[Test]
		public void LoggingEventPropertiesAreAddedToUserCustomData()
		{
			var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });
			loggingEventWithProperties.Properties["TestPropertyKey"] = "TestPropertyValue";

			var userCustomData = _userCustomDataBuilder.Build(loggingEventWithProperties);

			Assert.That(userCustomData, Has.Exactly(1).EqualTo(new KeyValuePair<string, string>("Properties.TestPropertyKey", "TestPropertyValue")));
		}
    }

	public static class LoggingEventAssert
	{
		public static EventMappingAssertionBuilder When(Action<LoggingEventData> loggingEventDataInitialiser)
		{
			var loggingEventData = new LoggingEventData();
			loggingEventDataInitialiser(loggingEventData);
			var loggingEvent = new LoggingEvent(loggingEventData);
			var userCustomDataBuilder = new UserCustomDataBuilder();
			var userCustomData = userCustomDataBuilder.Build(loggingEvent);

			return new EventMappingAssertionBuilder(loggingEvent, userCustomData);
		}
	}

	public class EventMappingAssertionBuilder
	{
		private readonly LoggingEvent _loggingEvent;
		private readonly Dictionary<string, string> _userCustomData;

		public EventMappingAssertionBuilder(LoggingEvent loggingEvent, Dictionary<string, string> userCustomData)
		{
			_loggingEvent = loggingEvent;
			_userCustomData = userCustomData;
		}

		public EventMappingAssertionPropertyBuilder Property(Func<LoggingEvent, string> propertySelector)
		{
			var loggingEventPropertyValue = propertySelector(_loggingEvent);

		    loggingEventPropertyValue = loggingEventPropertyValue.NotSuppliedIfNullOrEmpty();

			return new EventMappingAssertionPropertyBuilder(loggingEventPropertyValue, _userCustomData);
		}
	}

	public class EventMappingAssertionPropertyBuilder
	{
		private readonly string _loggingEventValue;
		private readonly Dictionary<string, string> _userCustomData;

		public EventMappingAssertionPropertyBuilder(string loggingEventValue, Dictionary<string, string> userCustomData)
		{
			_loggingEventValue = loggingEventValue;
			_userCustomData = userCustomData;
		}

		public EventMappingAssertionPropertyBuilder ShouldMapTo(string key)
		{
			var userCustomDataValue = _userCustomData[key];
			Assert.That(userCustomDataValue, Is.EqualTo(_loggingEventValue));

			return this;
		}
	}
}