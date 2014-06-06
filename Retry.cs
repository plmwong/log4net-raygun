using System;
using System.Threading;
using System.Diagnostics;

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
				catch (Exception ex)
                {
					Debug.WriteLine(string.Format("Exception on attempt #{0}: {1}", attempts, ex));

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