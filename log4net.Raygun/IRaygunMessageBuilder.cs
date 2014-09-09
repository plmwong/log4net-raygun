using System;
using System.Collections.Generic;
using log4net.Core;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun
{
    public interface IRaygunMessageBuilder
    {
        RaygunMessage BuildMessage(Exception exception, LoggingEvent loggingEvent, Dictionary<string, string> userCustomData, string exceptionFilter = null, string renderedMessageFilter = null, string ignoredFormNames = null);
    }
}