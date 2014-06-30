using System;
using System.IO;
using System.Linq;
using log4net.Config;
using NUnit.Framework;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class GivenRaygunAppenderPropertiesAreSetInXmlConfiguration
    {
        private RaygunAppender _raygunAppender;

        [SetUp]
        public void WhenRaygunAppenderIsConfiguredFromXml()
        {
            var fileInfo = new FileInfo("AllPropertiesSet.config");
            XmlConfigurator.Configure(fileInfo);

            var appenders = LogManager.GetRepository().GetAppenders();
            _raygunAppender = appenders.Cast<RaygunAppender>().Single();
        }

        [Test]
        public void ThenApiKeyIsSet()
        {
            Assert.That(_raygunAppender.ApiKey, Is.EqualTo("IAmAnAPIKey"));
        }

        [Test]
        public void ThenRetriesIsSet()
        {
            Assert.That(_raygunAppender.Retries, Is.EqualTo(11));
		}

		[Test]
		public void ThenExceptionFilterIsSet()
		{
			Assert.That(_raygunAppender.ExceptionFilter, Is.EqualTo("exceptionFilter"));
		}

		[Test]
		public void ThenRenderedMessageFilterIsSet()
		{
			Assert.That(_raygunAppender.RenderedMessageFilter, Is.EqualTo("messageFilter"));
		}

        [Test]
        public void ThenTimeBetweenRetriesIsSet()
        {
            Assert.That(_raygunAppender.TimeBetweenRetries, Is.EqualTo(TimeSpan.FromSeconds(1)));
        }
    }

    [TestFixture]
    public class GivenRaygunAppenderPropertiesAreNotSetInXmlConfiguration
    {
        private RaygunAppender _raygunAppender;

        [SetUp]
        public void WhenRaygunAppenderIsConfiguredFromXml()
        {
            var fileInfo = new FileInfo("NoPropertiesSet.config");
            XmlConfigurator.Configure(fileInfo);

            var appenders = LogManager.GetRepository().GetAppenders();
            _raygunAppender = appenders.Cast<RaygunAppender>().Single();
        }

        [Test]
        public void ThenApiKeyIsSet()
        {
            Assert.That(_raygunAppender.ApiKey, Is.Null);
        }

        [Test]
        public void ThenRetriesIsSet()
        {
            Assert.That(_raygunAppender.Retries, Is.EqualTo(0));
		}

		[Test]
		public void ThenExceptionFilterIsSet()
		{
			Assert.That(_raygunAppender.ExceptionFilter, Is.Null);
		}

		[Test]
		public void ThenRenderedMessageFilterIsSet()
		{
			Assert.That(_raygunAppender.RenderedMessageFilter, Is.Null);
		}

        [Test]
        public void ThenTimeBetweenRetriesIsSet()
        {
            Assert.That(_raygunAppender.TimeBetweenRetries, Is.EqualTo(RaygunAppender.DefaultTimeBetweenRetries));
        }
    }
}
