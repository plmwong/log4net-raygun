using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Func<IHttpContext> _httpContextFactory;
        private readonly IUserCustomDataBuilder _userCustomDataBuilder;
        private readonly Func<string, IRaygunClient> _raygunClientFactory;
        private readonly TaskScheduler _taskScheduler;

        public RaygunAppender()
            : this(() => new HttpContextAdapter(), new UserCustomDataBuilder(), apiKey => new RaygunClientAdapter(new RaygunClient(apiKey)), TaskScheduler.Default)
        {
        }

        internal RaygunAppender(Func<IHttpContext> httpContextFactory, IUserCustomDataBuilder userCustomDataBuilder, Func<string, IRaygunClient> raygunClientFactory, TaskScheduler taskScheduler)
        {
            _httpContextFactory = httpContextFactory;
            _userCustomDataBuilder = userCustomDataBuilder;
            _raygunClientFactory = raygunClientFactory;
            _taskScheduler = taskScheduler;
        }

        public virtual string ApiKey { get; set; }
        public virtual int Retries { get; set; }

        public virtual TimeSpan TimeBetweenRetries
        {
            get { return _timeBetweenRetries == TimeSpan.Zero ? DefaultTimeBetweenRetries : _timeBetweenRetries; }
            set { _timeBetweenRetries = value; }
        }

        public virtual string IgnoredFormNames { get; set; }
        public virtual string ExceptionFilter { get; set; }
        public virtual string RenderedMessageFilter { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Received Logging Event with Logging Level '{0}'", loggingEvent.Level));

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

            SendExceptionToRaygunInBackground(exception, loggingEvent);
        }

        private void SendExceptionToRaygunInBackground(Exception exception, LoggingEvent loggingEvent)
        {
            LogLog.Debug(DeclaringType, "RaygunAppender: Building UserCustomData dictionary");
            var userCustomData = _userCustomDataBuilder.Build(loggingEvent);
            LogLog.Debug(DeclaringType, "RaygunAppender: Building Raygun message");

            RaygunMessage raygunMessage = BuildRaygunMessage(exception, loggingEvent, userCustomData);

            LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Sending Raygun message in a background task. Retries: '{0}', TimeBetweenRetries: '{1}'", Retries, TimeBetweenRetries));
            new TaskFactory(_taskScheduler)
                .StartNew(() => Retry.Action(() =>
                {
                    try
                    {
                        SendExceptionToRaygun(raygunMessage);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.Error(string.Format("RaygunAppender: Could not send error to the Raygun API, retried {0} times", Retries), ex);
                    }
                }, Retries, TimeBetweenRetries));
        }

        private void SendExceptionToRaygun(RaygunMessage raygunMessage)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                ErrorHandler.Error("RaygunAppender: API Key is empty");
            }

            var raygunClient = _raygunClientFactory(ApiKey);

            raygunClient.Send(raygunMessage);

            LogLog.Debug(DeclaringType, "RaygunAppender: Raygun message sent");
        }

        private RaygunMessage BuildRaygunMessage(Exception exception, LoggingEvent loggingEvent, Dictionary<string, string> userCustomData)
        {
            LogLog.Debug(DeclaringType, "RaygunAppender: Resolving application assembly");
            var assemblyResolver = new AssemblyResolver();
            var applicationAssembly = assemblyResolver.GetApplicationAssembly();

            var raygunMessageBuilder = RaygunMessageBuilder.New;

            var httpContext = _httpContextFactory();
            if (httpContext != null && httpContext.Instance != null)
            {
                LogLog.Debug(DeclaringType, "RaygunAppender: Setting http details on the raygun message from http context");

                var messageOptions = new RaygunRequestMessageOptions(string.IsNullOrEmpty(IgnoredFormNames) ? Enumerable.Empty<string>() : IgnoredFormNames.Split(',').ToList(),
                                                                     Enumerable.Empty<string>(),
                                                                     Enumerable.Empty<string>(),
                                                                     Enumerable.Empty<string>());

                raygunMessageBuilder.SetHttpDetails(httpContext.Instance, messageOptions);
            }

            raygunMessageBuilder
                .SetExceptionDetails(exception)
                .SetClientDetails()
                .SetEnvironmentDetails()
                .SetMachineName(Environment.MachineName)
                .SetVersion(applicationAssembly != null ? applicationAssembly.GetName().Version.ToString() : null)
                .SetUserCustomData(RenderedMessageFilter != null ? FilterRenderedMessageInUserCustomData(userCustomData) : userCustomData);

            var raygunMessage = raygunMessageBuilder.Build();

            if (exception != null)
            {
                if (ExceptionFilter != null && raygunMessage.Details.Error != null)
                {
                    raygunMessage.Details.Error.Message = string.Format("{0}: {1}", exception.GetType().Name, ApplyFilter(exception.Message, ExceptionFilter));
                }
            }
            else
            {
                raygunMessage.Details.Error = new RaygunErrorMessage
                {
                    Message = RenderedMessageFilter != null ? ApplyFilter(loggingEvent.RenderedMessage, RenderedMessageFilter) : loggingEvent.RenderedMessage,
                    ClassName = loggingEvent.LocationInformation.ClassName
                };
            }

            return raygunMessage;
        }

        private Dictionary<string, string> FilterRenderedMessageInUserCustomData(Dictionary<string, string> userCustomData)
        {
            if (userCustomData.ContainsKey(UserCustomDataBuilder.UserCustomDataKey.RenderedMessage))
            {
                var oldValue = userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage];
                userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage] = ApplyFilter(oldValue, RenderedMessageFilter);
            }

            return userCustomData;
        }

        private string ApplyFilter(string message, string filter)
        {
            var renderedMessageFilterType = Type.GetType(filter);

            if (renderedMessageFilterType != null)
            {
                LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Activating instance of message filter for '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
                var renderedMessageFilter = Activator.CreateInstance(renderedMessageFilterType) as IMessageFilter;

                if (renderedMessageFilter != null)
                {
                    LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Filtering through message filter '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
                    return renderedMessageFilter.Filter(message);
                }

                ErrorHandler.Error(string.Format("RaygunAppender: Configured message filter '{0}' is not a IMessageFilter", filter));
            }
            else
            {
                ErrorHandler.Error(string.Format("RaygunAppender: Configured message filter '{0}' is not a type", filter));
            }

            return message;
        }
    }
}