using NUnit.Framework;
using System;

namespace log4net.Raygun.Tests
{
	[TestFixture]
	public class RetryTests
	{
		private bool _didActionSucceed;
		private int _timesActionCalled;
		private const int NumberOfRetries = 15;
		private TimeSpan TimeBetweenRetries = TimeSpan.FromMilliseconds(1);

		[SetUp]
		public void SetUp()
		{
			_didActionSucceed = false;
			_timesActionCalled = 0;
		}

		[Test]
		public void WhenActionAttemptsExceedsNumberOfRetriesThenExceptionIsThrown()
		{
			Assert.Throws<TestException>(() => Retry.Action(AlwaysFails, NumberOfRetries, TimeBetweenRetries));
		}

		private void AlwaysFails()
		{
			throw new TestException();
		}

		[Test]
		public void WhenActionSucceedsThenActionIsPerformed()
		{
			Assert.DoesNotThrow(() => Retry.Action(Success, NumberOfRetries, TimeBetweenRetries));
			Assert.That(_didActionSucceed, Is.True);
		}

		private void Success()
		{
			_didActionSucceed = true;
		}

		[Test]
		public void WhenActionSucceedsEventuallyThenActionIsPerformed()
		{
			Assert.DoesNotThrow(() => Retry.Action(EventualSuccess, NumberOfRetries, TimeBetweenRetries));
			Assert.That(_didActionSucceed, Is.True);
		}

		private void EventualSuccess()
		{
			_timesActionCalled++;

			if (_timesActionCalled < 15)
			{
				throw new TestException();
			}

			_didActionSucceed = true;
		}
	}
}

