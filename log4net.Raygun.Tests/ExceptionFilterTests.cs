using log4net.Raygun.Core;
using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;
using log4net.Core;
using System;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class GivenExceptionFilterIsSet
    {
        private RaygunAppenderBase _appender;
        private RaygunMessageBuilder _raygunMessageBuilder;
        private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
        private FakeRaygunClient _fakeRaygunClient;
        private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
        private FakeErrorHandler _fakeErrorHandler;

        [SetUp]
        public void SetUp()
        {
            _raygunMessageBuilder = new RaygunMessageBuilder(() => FakeHttpContext.For(new FakeHttpApplication()));
            _fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
            _fakeRaygunClient = new FakeRaygunClient();
            _currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
            _appender = new TestRaygunAppender(_fakeUserCustomDataBuilder,
                                               _raygunMessageBuilder,
                                               RaygunClientFactoryMethod.From(apiKey => _fakeRaygunClient),
                                               new TypeActivator(l => { }),
                                               _currentThreadTaskScheduler);
            _fakeErrorHandler = new FakeErrorHandler();

            _appender.ErrorHandler = _fakeErrorHandler;
        }

        [Test]
        public void WhenFilterIsSetThenExceptionsAreFirstPassedThroughTheExceptionFilter()
        {
            _appender.ExceptionFilter = typeof (FakeMessageFilter).AssemblyQualifiedName;

            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new NullReferenceException(), null);
            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo("System.NullReferenceException"));
            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.Message, Is.EqualTo("I changed your message!"));
        }

        [Test]
        public void WhenFilterIsSetToNonTypeThenExceptionsAreNotFilteredAtAll()
        {
            _appender.ExceptionFilter = "not a type";

            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new NullReferenceException(), null);
            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo("System.NullReferenceException"));
#if __MonoCS__
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.Message, Is.EqualTo("A null value was found where an object instance was required."));
#else
            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.Message, Is.EqualTo("Object reference not set to an instance of an object."));
#endif
        }

        [Test]
        public void WhenFilterIsSetToNonFilterTypeThenExceptionsAreNotFilteredAtAll()
        {
            _appender.ExceptionFilter = typeof (Int32).AssemblyQualifiedName;

            var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new NullReferenceException(), null);
            _appender.DoAppend(errorLoggingEvent);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo("System.NullReferenceException"));
#if __MonoCS__
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.Message, Is.EqualTo("A null value was found where an object instance was required."));
#else
            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.Message, Is.EqualTo("Object reference not set to an instance of an object."));
#endif
        }
    }
}