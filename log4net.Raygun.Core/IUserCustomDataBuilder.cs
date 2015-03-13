using System.Collections.Generic;
using log4net.Core;

namespace log4net.Raygun.Core
{
    public interface IUserCustomDataBuilder
    {
        Dictionary<string, string> Build(LoggingEvent loggingEvent);
    }
}