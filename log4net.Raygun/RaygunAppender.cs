using System;
using System.Collections.Generic;
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
        public static readonly TimeSpan DefaultTimeBetweenRetries = TimeSpan.FromMilliseconds(5000);
        private TimeSpan _timeBetweenRetries;

		private readonly IUserCustomDataBuilder _userCustomDataBuilder;
		private readonly Func<string, IRaygunClient> _raygunClientFactory;
		private readonly TaskScheduler _taskScheduler;

		public RaygunAppender() 
			: this(new UserCustomDataBuilder(), apiKey => new RaygunClientAdapter(new RaygunClient(apiKey)), TaskScheduler.Default)
		{
		}

		internal RaygunAppender(IUserCustomDataBuilder userCustomDataBuilder, Func<string, IRaygunClient> raygunClientFactory, TaskScheduler taskScheduler)
		{
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

		public virtual string ExceptionFilter { get; set; }
		public virtual string RenderedMessageFilter { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            LogLog.Debug(string.Format("RaygunAppender: Received Logging Event with Logging Level '{0}'", loggingEvent.Level));
        
			Exception exception = null;
        
			var exceptionObject = loggingEvent.ExceptionObject;
			if (exceptionObject != null) 
			{
				exception = exceptionObject.GetBaseException();
                LogLog.Debug(string.Format("RaygunAppender: Setting Exception to BaseException of LoggingEvent.ExceptionObject"));
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
                        LogLog.Debug(string.Format("RaygunAppender: Setting Exception to MessageObject"));
					}
				}
			}

			if (exception != null) 
			{
				SendExceptionToRaygunInBackground(exception, loggingEvent);
			} 
			else 
			{
				ErrorHandler.Error("RaygunAppender: Could not find any Exception to log. Doing nothing.");
			}
        }

		private void SendExceptionToRaygunInBackground(Exception exception, LoggingEvent loggingEvent)
        {
            LogLog.Debug("RaygunAppender: Building UserCustomData dictionary");
            var userCustomData = _userCustomDataBuilder.Build(loggingEvent);
            LogLog.Debug("RaygunAppender: Building Raygun message");
            var raygunMessage = BuildRaygunExceptionMessage(exception, userCustomData);

            LogLog.Debug(string.Format("RaygunAppender: Sending Raygun message in a background task. Retries: '{0}', TimeBetweenRetries: '{1}'", Retries, TimeBetweenRetries));
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

            LogLog.Debug("RaygunAppender: Raygun message sent");
        }

        private RaygunMessage BuildRaygunExceptionMessage(Exception exception, Dictionary<string, string> userCustomData)
        {
            LogLog.Debug("RaygunAppender: Resolving application assembly");
			var assemblyResolver = new AssemblyResolver();
			var applicationAssembly = assemblyResolver.GetApplicationAssembly();

            var raygunMessage = RaygunMessageBuilder.New
				.SetExceptionDetails(exception)
                .SetClientDetails()
                .SetEnvironmentDetails()
                .SetMachineName(Environment.MachineName)
				.SetVersion(applicationAssembly != null ? applicationAssembly.GetName().Version.ToString() : null)
				.SetUserCustomData(RenderedMessageFilter != null ? FilterRenderedMessage(userCustomData) : userCustomData)
                .Build();

            if (ExceptionFilter != null && raygunMessage.Details.Error != null)
            {
                raygunMessage.Details.Error.Message = FilterException(exception.Message);
            }

            return raygunMessage;
        }

		private string FilterException(string exceptionMessage)
		{
			var exceptionFilterType = Type.GetType(ExceptionFilter);

			if (exceptionFilterType != null) 
			{
				LogLog.Debug(string.Format("RaygunAppender: Activating instance of exception filter for '{0}'", exceptionFilterType.AssemblyQualifiedName));
				var exceptionFilter = Activator.CreateInstance(exceptionFilterType) as IMessageFilter;

				if (exceptionFilter != null) 
				{
					LogLog.Debug(string.Format("RaygunAppender: Filtering through exception filter '{0}'", exceptionFilterType.AssemblyQualifiedName));
                    return exceptionFilter.Filter(exceptionMessage);
				}
			
                ErrorHandler.Error(string.Format("RaygunAppender: Configured exception filter '{0}' is not an IExceptionFilter", ExceptionFilter));
			}
			else 
			{
				ErrorHandler.Error(string.Format("RaygunAppender: Configured exception filter '{0}' is not a type", ExceptionFilter));
			}

            return exceptionMessage;
		}

		private Dictionary<string, string> FilterRenderedMessage(Dictionary<string, string> userCustomData)
		{
			var renderedMessageFilterType = Type.GetType(RenderedMessageFilter);

			if (renderedMessageFilterType != null)
			{
				LogLog.Debug(string.Format("RaygunAppender: Activating instance of rendered message filter for '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
				var renderedMessageFilter = Activator.CreateInstance(renderedMessageFilterType) as IMessageFilter;

				if (renderedMessageFilter != null && userCustomData.ContainsKey(UserCustomDataBuilder.UserCustomDataKey.RenderedMessage))
				{
					LogLog.Debug(string.Format("RaygunAppender: Filtering through rendered message filter '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
					var oldValue = userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage];
					userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage] = renderedMessageFilter.Filter(oldValue);
				}
				else 
				{
					ErrorHandler.Error(string.Format("RaygunAppender: Configured rendered message filter '{0}' is not an IRenderedMessageFilter", RenderedMessageFilter));
				}
			} 
			else 
			{
				ErrorHandler.Error(string.Format("RaygunAppender: Configured rendered message filter '{0}' is not a type", RenderedMessageFilter));
			}

			return userCustomData;
		}
    }
}
