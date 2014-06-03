using System;
using System.Threading;

namespace log4net.Raygun
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
                catch (Exception)
                {
                    if (attempts >= numberOfRetries)
                    {
                        throw;
                    }

                    Thread.Sleep(initialWaitInterval);

                    continue;
                }

                break;
            }
        }
    }
}