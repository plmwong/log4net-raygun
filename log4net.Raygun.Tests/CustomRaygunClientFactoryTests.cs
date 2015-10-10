using System.Collections.Generic;
using log4net.Core;
using log4net.Raygun.Core;
using log4net.Raygun.Tests.Fakes;
using NUnit.Framework;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class CustomRaygunClientFactoryTests
    {
        private RaygunAppenderBase _appender;
        private RaygunMessageBuilder _raygunMessageBuilder;
        private UserCustomDataBuilder _userCustomDataBuilder;
        private FakeRaygunClient _fakeRaygunClient;
        private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
        private InterceptingTypeActivator _fakeTypeActivator;
        private FakeErrorHandler _fakeErrorHandler;

        [SetUp]
        public void SetUp()
        {
            _raygunMessageBuilder = new RaygunMessageBuilder(() => FakeHttpContext.For(new FakeHttpApplication()));
            _userCustomDataBuilder = new UserCustomDataBuilder();
            _fakeRaygunClient = new FakeRaygunClient();
            _fakeTypeActivator = new InterceptingTypeActivator();
            _currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
            _appender = new TestRaygunAppender(_userCustomDataBuilder,
                                               _raygunMessageBuilder,
                                               RaygunClientFactoryMethod.From(apiKey => _fakeRaygunClient),
                                               _fakeTypeActivator,
                                               _currentThreadTaskScheduler);
            _fakeErrorHandler = new FakeErrorHandler();

            _appender.ErrorHandler = _fakeErrorHandler;
        }

        [Test]
        public void WhenCustomRaygunClientFactoryIsSetThenCustomRaygunClientIsUsed()
        {
            const string customRaygunFactoryType = "fakeClientFactoryType";
            var otherFakeRaygunClient = new FakeRaygunClient();
            _fakeTypeActivator.RegisteredInstance = new KeyValuePair<string, object>(customRaygunFactoryType, RaygunClientFactoryMethod.From(apiKey => otherFakeRaygunClient));
            _appender.CustomRaygunClientFactory = customRaygunFactoryType;

            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent, Is.Null);
            Assert.That(otherFakeRaygunClient.LastMessageSent, Is.Not.Null);
        }
    }
}