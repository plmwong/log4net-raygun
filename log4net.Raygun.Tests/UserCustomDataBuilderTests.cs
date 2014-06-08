using System.Collections.Generic;
using log4net.Core;
using log4net.Util;
using NUnit.Framework;

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
		public void LoggingEventPropertiesAreAddedToUserCustomData()
		{
			var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });
			loggingEventWithProperties.Properties["TestPropertyKey"] = "TestPropertyValue";

			var userCustomData = _userCustomDataBuilder.Build(loggingEventWithProperties);

			Assert.That(userCustomData, Has.Exactly(1).EqualTo(new KeyValuePair<string, string>("Properties.TestPropertyKey", "TestPropertyValue")));
		}
    }
}