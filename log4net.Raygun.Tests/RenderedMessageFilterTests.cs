using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;
using log4net.Core;
using System;

namespace log4net.Raygun.Tests
{
	[TestFixture]
	public class GivenRenderedMessageFilterIsSet
	{
		private RaygunAppender _appender;
		private UserCustomDataBuilder _userCustomDataBuilder;
		private FakeRaygunClient _fakeRaygunClient;
		private CurrentThreadTaskScheduler _currentThreadTaskScheduler;
		private FakeErrorHandler _fakeErrorHandler;

		[SetUp]
		public void WhenExceptionFilterIsSet()
		{
			_userCustomDataBuilder = new UserCustomDataBuilder();
			_fakeRaygunClient = new FakeRaygunClient();
			_currentThreadTaskScheduler = new CurrentThreadTaskScheduler();
			_appender = new RaygunAppender(_userCustomDataBuilder, apiKey => _fakeRaygunClient, _currentThreadTaskScheduler);
			_fakeErrorHandler = new FakeErrorHandler();

			_appender.ErrorHandler = _fakeErrorHandler;
			_appender.RenderedMessageFilter = typeof(FakeRenderedMessageFilter).AssemblyQualifiedName;

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
			_appender.DoAppend(errorLoggingEvent);
		}

		[Test]
		public void ThenRenderedMessageIsFirstPassedThroughRenderedMessageFilter()
		{
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.UserCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage], Is.EqualTo(FakeRenderedMessageFilter.ReplacementMessage));
		}
	}

	[TestFixture]
	public class GivenRenderedMessageFilterIsSetToANonExistentType
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
			_appender.RenderedMessageFilter = "not a type";

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
			_appender.DoAppend(errorLoggingEvent);
		}

		[Test]
		public void ThenExceptionsAreNotFilteredAtAll()
		{
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.UserCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage], Is.Null);
		}
	}

	[TestFixture]
	public class GivenRenderedMessageFilterIsSetToANonFilterType
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
			_appender.RenderedMessageFilter = typeof(Decimal).AssemblyQualifiedName;

			var errorLoggingEvent = new LoggingEvent(GetType(), null, GetType().Name, Level.Error, new TestException(), null);
			_appender.DoAppend(errorLoggingEvent);
		}

		[Test]
		public void ThenExceptionsAreNotFilteredAtAll()
		{
			Assert.That(_fakeRaygunClient.LastMessageSent.Details.UserCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage], Is.Null);
		}
	}
}

