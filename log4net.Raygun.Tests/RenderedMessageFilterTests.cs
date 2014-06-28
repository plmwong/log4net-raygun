using NUnit.Framework;
using log4net.Raygun.Tests.Fakes;
using log4net.Core;

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
}

