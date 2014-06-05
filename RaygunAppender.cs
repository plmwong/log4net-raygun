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
        private static readonly string ApplicationAssemblyFullName = AssemblyResolver.GetApplicationAssembly().FullName;

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
                var exceptionObject = loggingEvent.ExceptionObject;
                var exception = exceptionObject != null ? exceptionObject.GetBaseException() : null;

                if (exception == null && loggingEvent.MessageObject is Exception)
                {
                    exception = loggingEvent.MessageObject as Exception;
                }

                if (exception != null)
                {
                    new TaskFactory().StartNew(() =>
                        Retry.Action(() => SendExceptionToRaygun(exception), Retries, TimeBetweenRetries));
                }
            }
        }

        private void SendExceptionToRaygun(Exception exception)
        {
            var raygunClient = new RaygunClient(ApiKey);
            var raygunMessage = BuildRaygunExceptionMessage(exception);

            raygunClient.Send(raygunMessage);
        }

        private static RaygunMessage BuildRaygunExceptionMessage(Exception exception)
        {
            var userCustomData = new Dictionary<string, string> { { UserCustomDataKey.AssemblyFulllName, ApplicationAssemblyFullName } };

            var raygunMessage = RaygunMessageBuilder.New
                .SetExceptionDetails(exception)
                .SetClientDetails()
                .SetEnvironmentDetails()
                .SetMachineName(Environment.MachineName)
                .SetVersion(AssemblyResolver.GetApplicationAssembly().GetName().Version.ToString())
                .SetUserCustomData(userCustomData)
                .Build();

            return raygunMessage;
        }

        protected class UserCustomDataKey
        {
            public const string AssemblyFulllName = "Assembly FullName";
        }
    }
}
