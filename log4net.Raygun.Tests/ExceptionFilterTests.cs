using System;
using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;
using log4net.Core;

namespace log4net.Raygun.Tests
{
	[TestFixture]
	public class ExceptionFilterTests
	{
		private RaygunAppender _appender;
		private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
		private FakeRaygunClient _fakeRaygunClient;
		private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
		private FakeErrorHandler _fakeErrorHandler;

		[SetUp]
		public void WhenExceptionFilterIsSet()
		{
			_fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
			_fakeRaygunClient = new FakeRaygunClient();
			_currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
			_appender = new RaygunAppender(_fakeUserCustomDataBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
			_fakeErrorHandler = new FakeErrorHandler();

			_appender.ErrorHandler = _fakeErrorHandler;
			_appender.ExceptionFilter = typeof(FakeExceptionFilter).AssemblyQualifiedName;

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
			_appender.DoAppend(errorLoggingEvent);
		}

		[Test]
		public void ThenExceptionsAreFirstPassedThroughTheExceptionFilter()
		{
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo("log4net.Raygun.Tests.TestException"));
		}
	}
}

