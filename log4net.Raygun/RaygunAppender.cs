using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net.Appender;
using log4net.Core;
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

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent.Level >= Level.Error)
            {
                Exception exception = null;
                
                var exceptionObject = loggingEvent.ExceptionObject;
                if (exceptionObject != null)
                {
                    exception = exceptionObject.GetBaseException();
                }

				if (exception == null) {
					var messageObject = loggingEvent.MessageObject;
					if (messageObject != null)
					{
						var messageObjectAsException = messageObject as Exception;
						if (messageObjectAsException != null)
						{
							exception = messageObjectAsException;
						}
					}
				}

                if (exception != null)
                {
                    SendExceptionToRaygunInBackground(exception, loggingEvent);
                }
                else
                {
                    ErrorHandler.Error("RaygunAppender: Could not find any exception to log");
                }
            }
        }

		private void SendExceptionToRaygunInBackground(Exception exception, LoggingEvent loggingEvent)
        {
            var userCustomData = _userCustomDataBuilder.Build(loggingEvent);
            var raygunMessage = BuildRaygunExceptionMessage(exception, userCustomData);

			new TaskFactory(_taskScheduler)
                .StartNew(() => Retry.Action(() => SendExceptionToRaygun(raygunMessage), Retries, TimeBetweenRetries))
                .ContinueWith(e =>
                {
                    if (e.Exception != null)
                    {
                        ErrorHandler.Error(string.Format("RaygunAppender: Could not send error to the Raygun API, retried {0} times", Retries), e.Exception);
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
		}

        private void SendExceptionToRaygun(RaygunMessage raygunMessage)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                ErrorHandler.Error("RaygunAppender: API Key is empty");
            }

            var raygunClient = _raygunClientFactory(ApiKey);

            raygunClient.Send(raygunMessage);
        }

        private static RaygunMessage BuildRaygunExceptionMessage(Exception exception, Dictionary<string, string> userCustomData)
        {
			var assemblyResolver = new AssemblyResolver();
			var applicationAssembly = assemblyResolver.GetApplicationAssembly();

            var raygunMessage = RaygunMessageBuilder.New
                .SetExceptionDetails(exception)
                .SetClientDetails()
                .SetEnvironmentDetails()
                .SetMachineName(Environment.MachineName)
				.SetVersion(applicationAssembly != null ? applicationAssembly.GetName().Version.ToString() : null)
                .SetUserCustomData(userCustomData)
                .Build();

            return raygunMessage;
        }
    }
}
