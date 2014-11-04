using System;
using NUnit.Framework;
using log4net.Core;
using log4net.Raygun.Tests.Fakes;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class RaygunAppenderTests
    {
        private RaygunAppender _appender;
        private RaygunMessageBuilder _raygunMessageBuilder;
        private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
        private FakeRaygunClient _fakeRaygunClient;
        private CurrentThreadTaskScheduler _currentThreadTaskScheduler;

        protected Level[] LoggingLevels = { Level.Debug, Level.Info, Level.Warn, Level.Error, Level.Fatal };

        [SetUp]
        public void SetUp()
        {
            _raygunMessageBuilder = new RaygunMessageBuilder(() => FakeHttpContext.For(new FakeHttpApplication()));
            _fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
            _fakeRaygunClient = new FakeRaygunClient();
            _currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
            _appender = new RaygunAppender(_fakeUserCustomDataBuilder, _raygunMessageBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
        }

        [Test]
        [TestCaseSource("LoggingLevels")]
        public void WhenLoggingEventContainsAnExceptionThenSendRaygunMessage(Level loggingLevel)
        {
            var loggingEvent = new LoggingEvent(GetType(), null, GetType().Name, loggingLevel, null, new TestException());
            _appender.DoAppend(loggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent, Is.Not.Null);
        }

        [Test]
        [TestCaseSource("LoggingLevels")]
        public void WhenLoggingEventWithoutExceptionDataThenSendRaygunMessage(Level loggingLevel)
        {
            var loggingEvent = new LoggingEvent(GetType(), null, GetType().Name, loggingLevel, null, null);

            _appender.DoAppend(loggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent, Is.Not.Null);
        }

        [Test]
        [TestCaseSource("LoggingLevels")]
        public void WhenLoggingEventWithoutExceptionDataThenRaygunMessageContainsLoggedMessage(Level loggingLevel)
        {
            const string loggedMessage = "Logged Message";
            var loggingEvent = new LoggingEvent(GetType(), null, GetType().Name, loggingLevel, loggedMessage, null);

            _appender.DoAppend(loggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.Message, Is.EqualTo(loggedMessage));
        }

        [Test]
        [TestCaseSource("LoggingLevels")]
        public void WhenLoggingEventWithoutExceptionDataThenRaygunMessageContainsCallerClass(Level loggingLevel)
        {
            const string loggedMessage = "Logged Message";
            var loggingEvent = new LoggingEvent(GetType(), null, GetType().Name, loggingLevel, loggedMessage, null);

            _appender.DoAppend(loggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo(loggingEvent.LocationInformation.ClassName));
        }

        [Test]
        [TestCaseSource("LoggingLevels")]
        public void WhenLoggingEventWithoutExceptionDataAndAppenderIsConfiguredToOnlySendExceptionsThenNothingIsSent(Level loggingLevel)
        {
            const string loggedMessage = "Logged Message";
            var loggingEvent = new LoggingEvent(GetType(), null, GetType().Name, loggingLevel, loggedMessage, null);

            _appender.OnlySendExceptions = true;
            _appender.DoAppend(loggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent, Is.Null);
        }

        [Test]
        public void WhenBuildingRaygunMessageToSendThenSetTheHttpDetailsFromHttpContext()
        {
            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);

            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Request.HostName, Is.EqualTo(FakeHttpContext.FakeHostName));
        }

        [Test]
        public void WhenBuildingRaygunMessageToSendThenSetTheUserCustomDataFromBuilder()
        {
            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);

            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.UserCustomData, Is.SameAs(_fakeUserCustomDataBuilder.UserCustomData));
        }

        [Test]
        public void WhenBuildingRaygunMessageToSendThenTheMachineNameIsSet()
        {
            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);

            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.MachineName, Is.EqualTo(Environment.MachineName));
        }
    }
}