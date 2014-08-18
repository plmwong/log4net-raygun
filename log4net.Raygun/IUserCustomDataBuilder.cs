using log4net.Core;
using System.Collections.Generic;

namespace log4net.Raygun
{
    public interface IUserCustomDataBuilder
    {
        Dictionary<string, string> Build(LoggingEvent loggingEvent);
    }
}