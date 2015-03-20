using System.Collections.Generic;
using log4net.Core;
using log4net.Raygun.Core;
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
        public void LoggingEventDomainIsAddedToUserCustomData()
        {
            LoggingEventAssert.When(l => l.Domain = "TestDomain")
                .Property(l => l.Domain)
                .ShouldMapTo(UserCustomDataBuilder.UserCustomDataKey.Domain);
        }

        [Test]
        public void LoggingEventLevelIsAddedToUserCustomData()
        {
            LoggingEventAssert.When(l => l.Level = Level.Debug)
                .Property(l => l.Level.Name)
                .ShouldMapTo(UserCustomDataBuilder.UserCustomDataKey.Level);
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
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData {Properties = new PropertiesDictionary()});
            loggingEventWithProperties.Properties["TestPropertyKey"] = "TestPropertyValue";

            var userCustomData = _userCustomDataBuilder.Build(loggingEventWithProperties);

            Assert.That(userCustomData, Has.Exactly(1).EqualTo(new KeyValuePair<string, string>("Properties.TestPropertyKey", "TestPropertyValue")));
        }

        [Test]
        public void Log4NetRaygunAssemblyVersionIsAddedToUserCustomData()
        {
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });
            loggingEventWithProperties.Properties["TestPropertyKey"] = "TestPropertyValue";

            var userCustomData = _userCustomDataBuilder.Build(loggingEventWithProperties);

            Assert.That(userCustomData, Has.Exactly(1).EqualTo(new KeyValuePair<string, string>(UserCustomDataBuilder.UserCustomDataKey.Log4NetRaygunVersion, typeof(RaygunAppenderBase).Assembly.GetName().Version.ToString())));
        }
    }
}