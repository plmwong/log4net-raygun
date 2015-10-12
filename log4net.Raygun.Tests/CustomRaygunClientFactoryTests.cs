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
        private FakeTypeActivator _fakeTypeActivator;
        private FakeErrorHandler _fakeErrorHandler;

        [SetUp]
        public void SetUp()
        {
            _raygunMessageBuilder = new RaygunMessageBuilder(() => FakeHttpContext.For(new FakeHttpApplication()));
            _userCustomDataBuilder = new UserCustomDataBuilder();
            _fakeRaygunClient = new FakeRaygunClient();
            _fakeTypeActivator = new FakeTypeActivator();
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
            var otherFakeRaygunClient = new FakeRaygunClient();

            UseACustomRaygunClientFactory(RaygunClientFactoryMethod.From(apiKey => otherFakeRaygunClient));

            Assert.That(otherFakeRaygunClient.LastMessageSent, Is.Not.Null);
        }

        private void UseACustomRaygunClientFactory(IRaygunClientFactory raygunClientFactory)
        {
            const string customRaygunFactoryType = "CustomRaygunClientFactoryType";
            _fakeTypeActivator.FakedType = new KeyValuePair<string, object>(customRaygunFactoryType, raygunClientFactory);
            _appender.CustomRaygunClientFactory = customRaygunFactoryType;

            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
            _appender.DoAppend(errorLoggingEvent);
        }

        [Test]
        public void WhenCustomRaygunClientFactoryIsSetThenDefaultRaygunClientIsNotUsed()
        {
            var otherFakeRaygunClient = new FakeRaygunClient();

            UseACustomRaygunClientFactory(RaygunClientFactoryMethod.From(apiKey => otherFakeRaygunClient));

            Assert.That(_fakeRaygunClient.LastMessageSent, Is.Null);
        }
    }
}