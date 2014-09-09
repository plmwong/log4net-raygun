using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using log4net.Util;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun
{
    public class RaygunMessageBuilder : IRaygunMessageBuilder
    {
        public static readonly Type DeclaringType = typeof(RaygunAppender);

        private readonly Func<IHttpContext> _httpContextFactory;

        public RaygunMessageBuilder(Func<IHttpContext> httpContextFactory)
        {
            _httpContextFactory = httpContextFactory;
        }

        public RaygunMessage BuildMessage(Exception exception, LoggingEvent loggingEvent, Dictionary<string, string> userCustomData, IMessageFilter exceptionFilter, IMessageFilter renderedMessageFilter, string ignoredFormNames)
        {
            LogLog.Debug(DeclaringType, "RaygunAppender: Resolving application assembly");
            var assemblyResolver = new AssemblyResolver();
            var applicationAssembly = assemblyResolver.GetApplicationAssembly();
            var raygunMessageBuilder = Mindscape.Raygun4Net.RaygunMessageBuilder.New;

            var httpContext = _httpContextFactory();
            if (httpContext != null && httpContext.Instance != null)
            {
                LogLog.Debug(DeclaringType, "RaygunAppender: Setting http details on the raygun message from http context");

                var messageOptions = new RaygunRequestMessageOptions(string.IsNullOrEmpty(ignoredFormNames) ? Enumerable.Empty<string>() : ignoredFormNames.Split(',').ToList(),
                                                                     Enumerable.Empty<string>(),
                                                                     Enumerable.Empty<string>(),
                                                                     Enumerable.Empty<string>());

                raygunMessageBuilder.SetHttpDetails(httpContext.Instance, messageOptions);
            }

            raygunMessageBuilder
                .SetExceptionDetails(exception)
                .SetClientDetails()
                .SetTags(ExtractTagsFromLoggingEventProperties(loggingEvent.Properties))
                .SetEnvironmentDetails()
                .SetMachineName(Environment.MachineName)
                .SetVersion(applicationAssembly != null ? applicationAssembly.GetName().Version.ToString() : null)
                .SetUserCustomData(FilterRenderedMessageInUserCustomData(userCustomData, renderedMessageFilter));

            var raygunMessage = raygunMessageBuilder.Build();

            if (exception != null)
            {
                if (raygunMessage.Details.Error != null)
                {
                    raygunMessage.Details.Error.Message = string.Format("{0}: {1}", exception.GetType().Name, ApplyFilter(exception.Message, exceptionFilter));
                }
            }
            else
            {
                raygunMessage.Details.Error = new RaygunErrorMessage
                {
                    Message = ApplyFilter(loggingEvent.RenderedMessage, renderedMessageFilter),
                    ClassName = loggingEvent.LocationInformation.ClassName
                };
            }

            return raygunMessage;
        }

        private string ApplyFilter(string message, IMessageFilter exceptionFilter)
        {
            if (exceptionFilter != null)
            {
                return exceptionFilter.Filter(message);
            }

            return message;
        }

        private IList<string> ExtractTagsFromLoggingEventProperties(ReadOnlyPropertiesDictionary loggingEventProperties)
        {
            if (loggingEventProperties.Contains(RaygunAppender.PropertyKeys.Tags))
            {
                var rawTags = loggingEventProperties[RaygunAppender.PropertyKeys.Tags] as string;

                if (!string.IsNullOrEmpty(rawTags))
                {
                    return rawTags.Split('|').ToList();
                }
            }

            return null;
        }

        private Dictionary<string, string> FilterRenderedMessageInUserCustomData(Dictionary<string, string> userCustomData, IMessageFilter renderedMessageFilter)
        {
            if (userCustomData.ContainsKey(UserCustomDataBuilder.UserCustomDataKey.RenderedMessage))
            {
                var oldValue = userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage];
                userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage] = ApplyFilter(oldValue, renderedMessageFilter);
            }

            return userCustomData;
        }
    }
}