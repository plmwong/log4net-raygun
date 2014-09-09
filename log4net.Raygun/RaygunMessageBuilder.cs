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

        public RaygunMessage BuildMessage(Exception exception, LoggingEvent loggingEvent, Dictionary<string, string> userCustomData, string exceptionFilter = null, string renderedMessageFilter = null, string ignoredFormNames = null)
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
                .SetUserCustomData(renderedMessageFilter != null ? FilterRenderedMessageInUserCustomData(userCustomData, renderedMessageFilter) : userCustomData);

            var raygunMessage = raygunMessageBuilder.Build();

            if (exception != null)
            {
                if (exceptionFilter != null && raygunMessage.Details.Error != null)
                {
                    raygunMessage.Details.Error.Message = string.Format("{0}: {1}", exception.GetType().Name, ApplyFilter(exception.Message, exceptionFilter));
                }
            }
            else
            {
                raygunMessage.Details.Error = new RaygunErrorMessage
                {
                    Message = renderedMessageFilter != null ? ApplyFilter(loggingEvent.RenderedMessage, renderedMessageFilter) : loggingEvent.RenderedMessage,
                    ClassName = loggingEvent.LocationInformation.ClassName
                };
            }

            return raygunMessage;
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

        private Dictionary<string, string> FilterRenderedMessageInUserCustomData(Dictionary<string, string> userCustomData, string renderedMessageFilter)
        {
            if (userCustomData.ContainsKey(UserCustomDataBuilder.UserCustomDataKey.RenderedMessage))
            {
                var oldValue = userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage];
                userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage] = ApplyFilter(oldValue, renderedMessageFilter);
            }

            return userCustomData;
        }

        private string ApplyFilter(string message, string filter)
        {
            var renderedMessageFilterType = Type.GetType(filter);

            if (renderedMessageFilterType != null)
            {
                LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Activating instance of message filter for '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
                var renderedMessageFilter = Activator.CreateInstance(renderedMessageFilterType) as IMessageFilter;

                if (renderedMessageFilter != null)
                {
                    LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Filtering through message filter '{0}'", renderedMessageFilterType.AssemblyQualifiedName));
                    return renderedMessageFilter.Filter(message);
                }

                LogLog.Error(DeclaringType, string.Format("RaygunAppender: Configured message filter '{0}' is not a IMessageFilter", filter));
            }
            else
            {
                LogLog.Error(DeclaringType, string.Format("RaygunAppender: Configured message filter '{0}' is not a type", filter));
            }

            return message;
        }
    }
}