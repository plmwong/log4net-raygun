using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using log4net.Appender;
using log4net.Core;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun
{
    public class RaygunAppender : AppenderSkeleton
    {
        private const int MaximumNumberOfAttemptsToLogToRaygun = 10;
        private readonly TimeSpan _initialIntervalBetweenRetries = TimeSpan.FromMilliseconds(5000);
        private static readonly string AssemblyFullName = GetAssemblyFullName();

        private static string GetAssemblyFullName()
        {
            if (HttpContext.Current != null)
            {
                var baseWebApplicationType = HttpContext.Current.ApplicationInstance.GetType().BaseType;

                if (baseWebApplicationType != null)
                {
                    return baseWebApplicationType.Assembly.FullName;
                }
            }

            return Assembly.GetEntryAssembly() != null ? Assembly.GetEntryAssembly().FullName : null;
        }

        protected class UserCustomDataKey
        {
            public const string AssemblyFulllName = "Assembly FullName";
        }

        public virtual string ApiKey { get; set; }

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
                        Retry.Action(() => SendExceptionToRaygun(exception), MaximumNumberOfAttemptsToLogToRaygun, _initialIntervalBetweenRetries));
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
            var userCustomData = new Dictionary<string, string> {{UserCustomDataKey.AssemblyFulllName, AssemblyFullName}};

            var raygunMessage = RaygunMessageBuilder.New
                .SetExceptionDetails(exception)
                .SetClientDetails()
                .SetEnvironmentDetails()
                .SetMachineName(Environment.MachineName)
                .SetVersion(null)
                .SetUserCustomData(userCustomData)
                .Build();

            return raygunMessage;
        }
    }
}
