using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;
using log4net.Core;
using System;

namespace log4net.Raygun.Tests
{
	[TestFixture]
	public class RenderedMessageFilterTests
	{
		private RaygunAppender _appender;
		private UserCustomDataBuilder _userCustomDataBuilder;
		private FakeRaygunClient _fakeRaygunClient;
		private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
		private FakeErrorHandler _fakeErrorHandler;

		[SetUp]
		public void SetUp()
		{
			_userCustomDataBuilder = new UserCustomDataBuilder();
			_fakeRaygunClient = new FakeRaygunClient();
			_currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
			_appender = new RaygunAppender(_userCustomDataBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
			_fakeErrorHandler = new FakeErrorHandler();

			_appender.ErrorHandler = _fakeErrorHandler;
		}

		[Test]
		public void WhenFilterIsSetThenRenderedMessageIsFirstPassedThroughRenderedMessageFilter()
		{
			_appender.RenderedMessageFilter = typeof(FakeMessageFilter).AssemblyQualifiedName;

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
			_appender.DoAppend(errorLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent.Details.UserCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage], Is.EqualTo(FakeMessageFilter.ReplacementMessage));
		}

		[Test]
		public void WhenFilterIsSetToNonTypeThenExceptionsAreNotFilteredAtAll()
		{
			_appender.RenderedMessageFilter = "not a type";

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
			_appender.DoAppend(errorLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent.Details.UserCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage], 
				Is.EqualTo("log4net.Raygun.Tests.TestException: Exception of type 'log4net.Raygun.Tests.TestException' was thrown."));
		}


		[Test]
		public void WhenFilterIsSetToNonFilterTypeThenExceptionsAreNotFilteredAtAll()
		{
			_appender.RenderedMessageFilter = typeof(Decimal).AssemblyQualifiedName;

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
			_appender.DoAppend(errorLoggingEvent);

			Assert.That(_fakeRaygunClient.LastMessageSent.Details.UserCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage], 
				Is.EqualTo("log4net.Raygun.Tests.TestException: Exception of type 'log4net.Raygun.Tests.TestException' was thrown."));
		}
	}
}

