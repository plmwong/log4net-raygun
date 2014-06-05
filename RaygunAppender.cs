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
        private static readonly string ApplicationAssemblyFullName = AssemblyResolver.GetApplicationAssembly() != null ? AssemblyResolver.GetApplicationAssembly().FullName : "Not supplied.";

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
                string loggedMessage = null;

                var exceptionObject = loggingEvent.ExceptionObject;
                if (exceptionObject != null)
                {
                    exception = exceptionObject.GetBaseException();
                }

                var messageObject = loggingEvent.MessageObject;
                if (messageObject != null)
                {
                    var messageObjectAsException = messageObject as Exception;
                    if (exception == null && messageObjectAsException != null)
                    {
                        exception = messageObjectAsException;
                    }
                    else
                    {
                        loggedMessage = messageObject.ToString();
                    }
                }

                if (exception != null)
                {
                    new TaskFactory().StartNew(() =>
                        Retry.Action(() => SendExceptionToRaygun(exception, loggedMessage), Retries, TimeBetweenRetries));
                }
            }
        }

        private void SendExceptionToRaygun(Exception exception, string loggedMessage)
        {
            var raygunClient = new RaygunClient(ApiKey);
            var raygunMessage = BuildRaygunExceptionMessage(exception, loggedMessage);

            raygunClient.Send(raygunMessage);
        }

        private static RaygunMessage BuildRaygunExceptionMessage(Exception exception, string loggedMessage)
        {
            var userCustomData = new Dictionary<string, string>
            {
                { UserCustomDataKey.AssemblyFulllName, ApplicationAssemblyFullName },
                { UserCustomDataKey.LoggedMessage, loggedMessage ?? "Not supplied."}
            };

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
            public const string LoggedMessage = "Logged Message";
        }
    }
}
