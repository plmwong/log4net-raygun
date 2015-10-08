using log4net.Core;
using log4net.Raygun.Core;
using log4net.Raygun.Tests.Fakes;
using log4net.Util;
using Mindscape.Raygun4Net.Messages;
using NUnit.Framework;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class AffectedUsersTests
    {
        private RaygunAppenderBase _appender;
        private RaygunMessageBuilder _raygunMessageBuilder;
        private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
        private FakeRaygunClient _fakeRaygunClient;
        private CurrentThreadTaskScheduler _currentThreadTaskScheduler;

        [SetUp]
        public void SetUp()
        {
            _raygunMessageBuilder = new RaygunMessageBuilder(() => FakeHttpContext.For(new FakeHttpApplication()));
            _fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
            _fakeRaygunClient = new FakeRaygunClient();
            _currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
            _appender = new TestRaygunAppender(_fakeUserCustomDataBuilder, _raygunMessageBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
        }

        [Test]
        public void WhenLoggingEventContainsAffectedUserInformationThenRaygunMessageIsBuiltWithRaygunIdentifier()
        {
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });
            var affectedUserInfo = new RaygunIdentifierMessage("I am an affected user");
            loggingEventWithProperties.Properties[RaygunAppenderBase.PropertyKeys.AffectedUser] = affectedUserInfo;

            _appender.DoAppend(loggingEventWithProperties);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.User, Is.EqualTo(affectedUserInfo));
        }

        [Test]
        public void WhenLoggingEventDoesNotContainAffectedUserInformationThenRaygunMessageIsNotBuiltWithRaygunIdentifier()
        {
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });

            _appender.DoAppend(loggingEventWithProperties);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.User, Is.Null);
        }
    }
}