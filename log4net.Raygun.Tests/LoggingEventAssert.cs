using System.Collections.Generic;
using log4net.Core;
using NUnit.Framework;
using System;

namespace log4net.Raygun.Tests
{
    public static class LoggingEventAssert
    {
        public static EventMappingAssertionBuilder When(Action<LoggingEventDataWrapper> loggingEventDataInitialiser)
        {
            var loggingEventData = new LoggingEventDataWrapper();
            loggingEventDataInitialiser(loggingEventData);

            var loggingEvent = new LoggingEvent(loggingEventData.ToLoggingEventData());
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