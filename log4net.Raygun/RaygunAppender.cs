using System;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun
{
    public class RaygunAppender : AppenderSkeleton
    {
        public static readonly Type DeclaringType = typeof(RaygunAppender);
        public static readonly TimeSpan DefaultTimeBetweenRetries = TimeSpan.FromMilliseconds(5000);
        private TimeSpan _timeBetweenRetries;

        private readonly IUserCustomDataBuilder _userCustomDataBuilder;
        private readonly IRaygunMessageBuilder _raygunMessageBuilder;
        private readonly Func<string, IRaygunClient> _raygunClientFactory;
        private readonly TaskScheduler _taskScheduler;

        public RaygunAppender()
            : this(new UserCustomDataBuilder(), new RaygunMessageBuilder(() => new HttpContextAdapter()), apiKey => new RaygunClientAdapter(new RaygunClient(apiKey)), TaskScheduler.Default)
        {
        }

        internal RaygunAppender(IUserCustomDataBuilder userCustomDataBuilder, IRaygunMessageBuilder raygunMessageBuilder, Func<string, IRaygunClient> raygunClientFactory, TaskScheduler taskScheduler)
        {
            _userCustomDataBuilder = userCustomDataBuilder;
            _raygunMessageBuilder = raygunMessageBuilder;
            _raygunClientFactory = raygunClientFactory;
            _taskScheduler = taskScheduler;

            RaygunSettings.Settings.ThrowOnError = true;
        }

        public virtual string ApiKey { get; set; }
        public virtual int Retries { get; set; }

        public virtual TimeSpan TimeBetweenRetries
        {
            get { return _timeBetweenRetries == TimeSpan.Zero ? DefaultTimeBetweenRetries : _timeBetweenRetries; }
            set { _timeBetweenRetries = value; }
        }

        public virtual bool OnlySendExceptions { get; set; }

		public virtual string IgnoredFormNames { get; set; }
		public virtual string IgnoredHeaderNames { get; set; }
		public virtual string IgnoredCookieNames { get; set; }
		public virtual string IgnoredServerVariableNames { get; set; }

        public virtual string ExceptionFilter { get; set; }
        public virtual string RenderedMessageFilter { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Received Logging Event with Logging Level '{0}'", loggingEvent.Level));

            Exception exception = ResolveLoggedExceptionObject(loggingEvent);

            if (exception != null || !OnlySendExceptions)
            {
                RaygunMessage raygunMessage = BuildRaygunMessageToSend(exception, loggingEvent);

                SendErrorToRaygunInBackground(raygunMessage);
            }
        }

        private static Exception ResolveLoggedExceptionObject(LoggingEvent loggingEvent)
        {
            Exception exception = null;

            var exceptionObject = loggingEvent.ExceptionObject;
            if (exceptionObject != null)
            {
                exception = exceptionObject.GetBaseException();
                LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Setting Exception to BaseException of LoggingEvent.ExceptionObject"));
            }

            if (exception == null)
            {
                var messageObject = loggingEvent.MessageObject;
                if (messageObject != null)
                {
                    var messageObjectAsException = messageObject as Exception;
                    if (messageObjectAsException != null)
                    {
                        exception = messageObjectAsException;
                        LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Setting Exception to MessageObject"));
                    }
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
			var ignoredFieldSettings = new IgnoredFieldSettings(IgnoredFormNames, IgnoredHeaderNames, IgnoredCookieNames, IgnoredServerVariableNames);

            RaygunMessage raygunMessage = _raygunMessageBuilder.BuildMessage(exception, loggingEvent, userCustomData,
				exceptionFilter, renderedMessageFilter, ignoredFieldSettings);

            return raygunMessage;
        }

        private void SendErrorToRaygunInBackground(RaygunMessage raygunMessage)
        {
            LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Sending Raygun message in a background task. Retries: '{0}', TimeBetweenRetries: '{1}'", Retries, TimeBetweenRetries));
            new TaskFactory(_taskScheduler)
                .StartNew(() =>
                {
                    try
                    {
                        Retry.Action(() => SendErrorToRaygun(raygunMessage), Retries, TimeBetweenRetries);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.Error(string.Format("RaygunAppender: Could not send error to the Raygun API, retried {0} times", Retries), ex);
                    }
                });
        }

        private IMessageFilter ActivateInstanceOfMessageFilter(string filter)
        {
            if (filter != null)
            {
                var renderedMessageFilterType = Type.GetType(filter);

                if (renderedMessageFilterType != null)
                {
                    LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Activating instance of message filter for '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
                    var renderedMessageFilter = Activator.CreateInstance(renderedMessageFilterType) as IMessageFilter;

                    if (renderedMessageFilter != null)
                    {
                        LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Activated instance of message filter '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
                        return renderedMessageFilter;
                    }

                    ErrorHandler.Error(string.Format("RaygunAppender: Configured message filter '{0}' is not a IMessageFilter", filter));
                }
                else
                {
                    ErrorHandler.Error(string.Format("RaygunAppender: Configured message filter '{0}' is not a type", filter));
                }
            }

            return new DoNothingMessageFilter();
        }

        private void SendErrorToRaygun(RaygunMessage raygunMessage)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                ErrorHandler.Error("RaygunAppender: API Key is empty");
            }

            var raygunClient = _raygunClientFactory(ApiKey);

            raygunClient.Send(raygunMessage);

            LogLog.Debug(DeclaringType, "RaygunAppender: Raygun message sent");
        }

        public static class PropertyKeys
        {
            public const string Tags = "log4net.Raygun.Tags";
        }
    }
}