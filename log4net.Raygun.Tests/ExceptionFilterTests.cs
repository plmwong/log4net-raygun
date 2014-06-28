using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;
using log4net.Core;
using System;

namespace log4net.Raygun.Tests
{
	[TestFixture]
	public class GivenExceptionFilterIsSet
	{
		private RaygunAppender _appender;
		private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
		private FakeRaygunClient _fakeRaygunClient;
		private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
		private FakeErrorHandler _fakeErrorHandler;

		[SetUp]
		public void SetUp()
		{
			_fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
			_fakeRaygunClient = new FakeRaygunClient();
			_currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
			_appender = new RaygunAppender(_fakeUserCustomDataBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
			_fakeErrorHandler = new FakeErrorHandler();

			_appender.ErrorHandler = _fakeErrorHandler;
			_appender.ExceptionFilter = typeof(FakeExceptionFilter).AssemblyQualifiedName;

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new NullReferenceException(), null);
			_appender.DoAppend(errorLoggingEvent);
		}

		[Test]
		public void ThenExceptionsAreFirstPassedThroughTheExceptionFilter()
		{
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo("log4net.Raygun.Tests.TestException"));
		}
	}

	[TestFixture]
	public class GivenExceptionFilterIsSetToANonExistentType
	{
		private RaygunAppender _appender;
		private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
		private FakeRaygunClient _fakeRaygunClient;
		private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
		private FakeErrorHandler _fakeErrorHandler;

		[SetUp]
		public void SetUp()
		{
			_fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
			_fakeRaygunClient = new FakeRaygunClient();
			_currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
			_appender = new RaygunAppender(_fakeUserCustomDataBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
			_fakeErrorHandler = new FakeErrorHandler();

			_appender.ErrorHandler = _fakeErrorHandler;
			_appender.ExceptionFilter = "not a type";

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new NullReferenceException(), null);
			_appender.DoAppend(errorLoggingEvent);
		}

		[Test]
		public void ThenExceptionsAreNotFilteredAtAll()
		{
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo("System.NullReferenceException"));
		}
	}

	[TestFixture]
	public class GivenExceptionFilterIsSetToANonFilterType
	{
		private RaygunAppender _appender;
		private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
		private FakeRaygunClient _fakeRaygunClient;
		private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
		private FakeErrorHandler _fakeErrorHandler;

		[SetUp]
		public void SetUp()
		{
			_fakeUserCustomDataBuilder = new FakeUserCustomDataBuilder();
			_fakeRaygunClient = new FakeRaygunClient();
			_currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
			_appender = new RaygunAppender(_fakeUserCustomDataBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
			_fakeErrorHandler = new FakeErrorHandler();

			_appender.ErrorHandler = _fakeErrorHandler;
			_appender.ExceptionFilter = typeof(Decimal).AssemblyQualifiedName;

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new NullReferenceException(), null);
			_appender.DoAppend(errorLoggingEvent);
		}

		[Test]
		public void ThenExceptionsAreNotFilteredAtAll()
		{
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.Error.ClassName, Is.EqualTo("System.NullReferenceException"));
		}
	}
}

