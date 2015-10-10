using System;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun.Core
{
    public abstract class RaygunAppenderBase : AppenderSkeleton
    {
        public static readonly Type DeclaringType = typeof(RaygunAppenderBase);
        public static readonly TimeSpan DefaultTimeBetweenRetries = TimeSpan.FromMilliseconds(5000);
        private TimeSpan _timeBetweenRetries;

        private readonly IUserCustomDataBuilder _userCustomDataBuilder;
        private readonly IRaygunMessageBuilder _raygunMessageBuilder;
        private readonly IRaygunClientFactory _raygunClientFactory;
        private readonly ITypeActivator _typeActivator;
        private readonly TaskScheduler _taskScheduler;

        private bool _sendInBackground;

        protected RaygunAppenderBase(IRaygunMessageBuilder raygunMessageBuilder, IRaygunClientFactory raygunClientFactory)
            : this(new UserCustomDataBuilder(), raygunMessageBuilder, raygunClientFactory,
                new TypeActivator(l => LogLog.Debug(DeclaringType, l)), TaskScheduler.Default)
        {
        }

        protected RaygunAppenderBase(IUserCustomDataBuilder userCustomDataBuilder, IRaygunMessageBuilder raygunMessageBuilder,
            IRaygunClientFactory raygunClientFactory, ITypeActivator typeActivator, TaskScheduler taskScheduler)
        {
            _userCustomDataBuilder = userCustomDataBuilder;
            _raygunMessageBuilder = raygunMessageBuilder;
            _raygunClientFactory = raygunClientFactory;
            _typeActivator = typeActivator;
            _taskScheduler = taskScheduler;

            _sendInBackground = true;
        }

        private IRaygunClientFactory RaygunClientFactory
        {
            get
            {
                var clientFactory = _typeActivator.Activate<IRaygunClientFactory>(CustomRaygunClientFactory, e => ErrorHandler.Error(e), _raygunClientFactory);
                return clientFactory;
            }
        }

        public virtual string ApiKey { get; set; }
        public virtual int Retries { get; set; }

        public virtual TimeSpan TimeBetweenRetries
        {
            get { return _timeBetweenRetries == TimeSpan.Zero ? DefaultTimeBetweenRetries : _timeBetweenRetries; }
            set { _timeBetweenRetries = value; }
        }

        public virtual bool OnlySendExceptions { get; set; }
        // Analysis disable once ConvertToAutoProperty
        public virtual bool SendInBackground
        {
            get { return _sendInBackground; }
            set { _sendInBackground = value; }
        }

        public virtual string IgnoredFormNames { get; set; }
        public virtual string IgnoredHeaderNames { get; set; }
        public virtual string IgnoredCookieNames { get; set; }
        public virtual string IgnoredServerVariableNames { get; set; }
        public virtual bool IsRawDataIgnored { get; set; }
        public virtual string ApplicationVersion { get; set; }

        public virtual string ExceptionFilter { get; set; }
        public virtual string RenderedMessageFilter { get; set; }

        public virtual string CustomRaygunClientFactory { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Received Logging Event with Logging Level '{0}'", loggingEvent.Level));

            Exception exception = ResolveLoggedExceptionObject(loggingEvent);

            if (exception != null || !OnlySendExceptions)
            {
                if (Retries > 0)
                {
                    LogLog.Debug(DeclaringType, "RaygunAppender: Retries are enabled, checking that throw on errors has been enabled, or can be overridden");
                    RaygunThrowOnErrorsMustBeEnabled();
                }

                RaygunMessage raygunMessage = BuildRaygunMessageToSend(exception, loggingEvent);

                if (SendInBackground)
                {
                    SendErrorToRaygunInBackground(raygunMessage);
                }
                else
                {
                    SendErrorToRaygun(raygunMessage);
                }
            }
        }

        private static Exception ResolveLoggedExceptionObject(LoggingEvent loggingEvent)
        {
            var exception = loggingEvent.ExceptionObject;

            if (exception == null)
            {
                var messageObject = loggingEvent.MessageObject;
                var messageObjectAsException = messageObject as Exception;
                if (messageObjectAsException != null)
                {
                    exception = messageObjectAsException;
                    LogLog.Debug(DeclaringType, "RaygunAppender: Setting Exception to MessageObject");
                }
            }

            return exception;
        }

        private RaygunMessage BuildRaygunMessageToSend(Exception exception, LoggingEvent loggingEvent)
        {
            LogLog.Debug(DeclaringType, "RaygunAppender: Building UserCustomData dictionary");
            var userCustomData = _userCustomDataBuilder.Build(loggingEvent);
            LogLog.Debug(DeclaringType, "RaygunAppender: Building Raygun message");

            var exceptionFilter = ActivateInstanceOfMessageFilter(ExceptionFilter);
            var renderedMessageFilter = ActivateInstanceOfMessageFilter(RenderedMessageFilter);
            var ignoredFieldSettings = new IgnoredDataSettings(IgnoredFormNames, IgnoredHeaderNames,
                IgnoredCookieNames, IgnoredServerVariableNames, IsRawDataIgnored);

            RaygunMessage raygunMessage = _raygunMessageBuilder.BuildMessage(exception, loggingEvent, userCustomData,
                exceptionFilter, renderedMessageFilter, ignoredFieldSettings, ApplicationVersion);

            return raygunMessage;
        }

        private void SendErrorToRaygunInBackground(RaygunMessage raygunMessage)
        {
            LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Sending Raygun message in a background task. Retries: '{0}', TimeBetweenRetries: '{1}'", Retries, TimeBetweenRetries));
            new TaskFactory(_taskScheduler)
                .StartNew(() => SendErrorToRaygun(raygunMessage));
        }

        private IMessageFilter ActivateInstanceOfMessageFilter(string filter)
        {
            return _typeActivator.Activate<IMessageFilter>(filter, e => ErrorHandler.Error(e), new DoNothingMessageFilter());
        }

        private void SendErrorToRaygun(RaygunMessage raygunMessage)
        {
            try
            {
                Retry.Action(() =>
                {
                    if (string.IsNullOrEmpty(ApiKey))
                    {
                        ErrorHandler.Error("RaygunAppender: API Key is empty");
                    }

                    var raygunClient = RaygunClientFactory.Create(ApiKey);

                    raygunClient.Send(raygunMessage);

                    LogLog.Debug(DeclaringType, "RaygunAppender: Raygun message sent");
                }, Retries, TimeBetweenRetries);
            }
            catch (Exception ex)
            {
                ErrorHandler.Error(string.Format("RaygunAppender: Could not send error to the Raygun API, retried {0} times", Retries), ex);
            }
        }

        private void RaygunThrowOnErrorsMustBeEnabled()
        {
            if (!RaygunSettings.Settings.ThrowOnError)
            {
                LogLog.Warn(DeclaringType, "RaygunAppender: ThrowOnError was found to be disabled, setting to 'true'");
                RaygunSettings.Settings.ThrowOnError = true;
            }
        }

        public static class PropertyKeys
        {
            public const string Tags = "log4net.Raygun.Tags";
            public const string AffectedUser = "log4net.Raygun.AffectedUser";
        }
    }
}