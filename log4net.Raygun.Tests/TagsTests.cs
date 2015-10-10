using System.Linq;
using log4net.Core;
using log4net.Raygun.Core;
using log4net.Raygun.Tests.Fakes;
using log4net.Util;
using NUnit.Framework;

namespace log4net.Raygun.Tests
{
    [TestFixture]
    public class TagsTests
    {
        private RaygunAppenderBase _appender;
        private RaygunMessageBuilder _raygunMessageBuilder;
        private FakeUserCustomDataBuilder _fakeUserCustomDataBuilder;
        private FakeRaygunClient _fakeRaygunClient;
        private CurrentThreadTaskScheduler _currentThreadTaskScheduler;

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
        }

        [Test]
        public void WhenLoggingEventContainsTagsThenRaygunMessageIsBuiltWithTags()
        {
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });
            const string tags = "tag1|tag2|tag3";
            loggingEventWithProperties.Properties[RaygunAppenderBase.PropertyKeys.Tags] = tags;

            _appender.DoAppend(loggingEventWithProperties);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Tags, Is.EquivalentTo(tags.Split('|').ToList()));
        }

        [Test]
        public void WhenLoggingEventContainsOneTagThenRaygunMessageIsBuiltWithOneTag()
        {
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });
            const string tags = "tag1";
            loggingEventWithProperties.Properties[RaygunAppenderBase.PropertyKeys.Tags] = tags;

            _appender.DoAppend(loggingEventWithProperties);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Tags, Is.EquivalentTo(tags.Split('|').ToList()));
        }

        [Test]
        public void WhenLoggingEventContainsEmptyTagsThenRaygunMessageHasNoTags()
        {
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });
            loggingEventWithProperties.Properties[RaygunAppenderBase.PropertyKeys.Tags] = string.Empty;

            _appender.DoAppend(loggingEventWithProperties);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Tags, Is.Null);
        }

        [Test]
        public void WhenLoggingEventDoesNotContainAnyTagsThenRaygunMessageHasNoTags()
        {
            var loggingEventWithProperties = new LoggingEvent(new LoggingEventData { Properties = new PropertiesDictionary() });

            _appender.DoAppend(loggingEventWithProperties);

            Assert.That(_fakeRaygunClient.LastMessageSent.Details.Tags, Is.Null);
        }
    }
}