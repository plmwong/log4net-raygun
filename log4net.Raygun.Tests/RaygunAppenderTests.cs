using NUnit.Framework;
using log4net.Core;
using System;

namespace log4net.Raygun.Tests
{
	[TestFixture]
	public class RaygunAppenderTests
	{
		private RaygunAppender _appender;
		private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
		private FakeRaygunClient _fakeRaygunClient;
		private CurrentThreadTaskScheduler _currentThreadTaskScheduler;

		[SetUp]
		public void SetUp()
		{
			_fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
			_fakeRaygunClient = new FakeRaygunClient();
			_currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
			_appender = new RaygunAppender(_fakeUserCustomDataBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
		}

		[Test]
		public void WhenLoggingEventIsNotAnErrorDoNothing()
		{
			var debugLoggingEvent = new LoggingEvent(new LoggingEventData { Level = Level.Debug });
			_appender.DoAppend(debugLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent, Is.Null);
		}

		[Test]
		public void WhenLoggingEventIsAnErrorSendRaygunMessage()
		{
			var errorLoggingEvent = new LoggingEvent(GetType(), null, "RaygunAppenderTests", Level.Error, null, new TestException());
			_appender.DoAppend(errorLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent, Is.Not.Null);
		}

		[Test]
		public void WhenLoggingEventIsFatalSendRaygunMessage()
		{
			var fatalLoggingEvent = new LoggingEvent(GetType(), null, "RaygunAppenderTests", Level.Fatal, null, new TestException());

			_appender.DoAppend(fatalLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent, Is.Not.Null);
		}

		[Test]
		public void WhenLoggingErrorEventWithoutExceptionDataDoNothing()
		{
			var fatalLoggingEvent = new LoggingEvent(GetType(), null, "RaygunAppenderTests", Level.Error, null, null);

			_appender.DoAppend(fatalLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent, Is.Null);
		}

		[Test]
		public void WhenLoggingErrorEventWithExceptionInMessageDataSendRaygunMessage()
		{
			var fatalLoggingEvent = new LoggingEvent(GetType(), null, "RaygunAppenderTests", Level.Error, new TestException(), null);

			_appender.DoAppend(fatalLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent, Is.Not.Null);
		}
	}
}

