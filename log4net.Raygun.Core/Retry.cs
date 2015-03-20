using System;
using System.Threading;
using log4net.Util;

namespace log4net.Raygun.Core
{
    public static class Retry
    {
        public static void Action(Action action, int numberOfRetries, TimeSpan initialWaitInterval)
        {
            int attempts = 0;

            while (true)
            {
                attempts++;

                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    LogLog.Debug(string.Format("RaygunAppender: Exception on retry attempt #{0}: {1}", attempts, ex));

                    if (attempts >= numberOfRetries)
                    {
                        throw;
                    }

                    LogLog.Debug(string.Format("RaygunAppender: Waiting {0} until next attempt.", initialWaitInterval));
                    Thread.Sleep(initialWaitInterval);

                    continue;
                }

                break;
            }
        }
    }
}