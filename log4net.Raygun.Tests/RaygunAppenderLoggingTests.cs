using log4net.Core;
using log4net.Raygun.Tests.Fakes;
using NUnit.Framework;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class RaygunAppenderLoggingTests
    {
        private RaygunAppender _appender;
        private RaygunMessageBuilder _raygunMessageBuilder;
        private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
        private FakeRaygunClient _fakeRaygunClient;
        private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
        private FakeErrorHandler _fakeErrorHandler;

        [SetUp]
        public void SetUp()
        {
            _fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
            _raygunMessageBuilder = new RaygunMessageBuilder(() => FakeHttpContext.For(new FakeHttpApplication()));
            _fakeRaygunClient = new FakeRaygunClient();
            _currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
            _appender = new RaygunAppender(_fakeUserCustomDataBuilder, _raygunMessageBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
            _fakeErrorHandler = new FakeErrorHandler();

            _appender.ErrorHandler = _fakeErrorHandler;
        }

        [Test]
        public void WhenNoApiKeyIsConfiguredInTheAppenderThenThatIsLogged()
        {
            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, null, new TestException());
            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeErrorHandler.Errors, Has.Exactly(1).EqualTo("RaygunAppender: API Key is empty"));
        }

        [Test]
        public void WhenSendingErrorToRaygunFailsThenThatIsLogged()
        {
            var appender = new RaygunAppender(_fakeUserCustomDataBuilder, _raygunMessageBuilder, apiKey => new FakeBrokenRaygunClient(), _currentThreadTaskScheduler);
            var fakeErrorHandler = new FakeErrorHandler();

            appender.ErrorHandler = fakeErrorHandler;

            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, null, new TestException());
            appender.DoAppend(errorLoggingEvent);

            Assert.That(fakeErrorHandler.Errors, Has.Exactly(1).StartsWith("RaygunAppender: Could not send error to the Raygun API, retried 0 times"));
        }
    }
}