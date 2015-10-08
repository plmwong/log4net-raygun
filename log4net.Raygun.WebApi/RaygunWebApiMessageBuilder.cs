using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using log4net.Core;
using log4net.Raygun.Core;
using log4net.Util;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun.WebApi
{
    public class RaygunWebApiMessageBuilder : Core.IRaygunMessageBuilder
    {
        public static readonly Type DeclaringType = typeof(RaygunAppenderBase);

        public RaygunMessage BuildMessage(Exception exception, LoggingEvent loggingEvent, Dictionary<string, string> userCustomData,
            IMessageFilter exceptionFilter, IMessageFilter renderedMessageFilter, IgnoredDataSettings ignoredFieldSettings, string customApplicationVersion)
        {
            var raygunMessageBuilder = Mindscape.Raygun4Net.WebApi.RaygunWebApiMessageBuilder.New;

            var httpRequestMessage = ResolveHttpRequestMessageFromLog4NetProperties(loggingEvent.Properties);
            if (httpRequestMessage != null)
            {
                LogLog.Debug(DeclaringType, "RaygunAppender: Setting http details on the raygun message from http context");

                var messageOptions = new RaygunRequestMessageOptions(
                    ignoredFieldSettings.IgnoredFormNames,
                    ignoredFieldSettings.IgnoredHeaderNames,
                    ignoredFieldSettings.IgnoredCookieNames,
                    ignoredFieldSettings.IgnoredServerVariableNames)
                {
                    IsRawDataIgnored = ignoredFieldSettings.IsRawDataIgnored
                };

                raygunMessageBuilder.SetHttpDetails(httpRequestMessage, messageOptions);
            }

            raygunMessageBuilder
                .SetExceptionDetails(exception)
                .SetClientDetails()
                .SetTags(ExtractTags(loggingEvent.Properties))
                .SetUser(ExtractAffectedUser(loggingEvent.Properties))
                .SetEnvironmentDetails()
                .SetMachineName(Environment.MachineName)
                .SetVersion(GetApplicationVersion(customApplicationVersion))
                .SetUserCustomData(FilterRenderedMessageInUserCustomData(userCustomData, renderedMessageFilter));

            var raygunMessage = raygunMessageBuilder.Build();

            if (exception != null)
            {
                if (raygunMessage.Details.Error != null)
                {
                    raygunMessage.Details.Error.Message = exceptionFilter.ApplyTo(exception.Message);
                }
            }
            else
            {
                LogLog.Debug(DeclaringType, "RaygunAppender: No exception object found in error, creating raygun error message from the rendered message and calling class");
                raygunMessage.Details.Error = new RaygunErrorMessage
                {
                    Message = renderedMessageFilter.ApplyTo(loggingEvent.RenderedMessage),
                    ClassName = loggingEvent.LocationInformation.ClassName
                };
            }

            return raygunMessage;
        }

        private string GetApplicationVersion(string customApplicationVersion)
        {
            if (!string.IsNullOrEmpty(customApplicationVersion))
            {
                LogLog.Debug(DeclaringType, "RaygunAppender: Using custom application version " + customApplicationVersion);
                return customApplicationVersion;
            }

            LogLog.Debug(DeclaringType, "RaygunAppender: Resolving application assembly");
            var assemblyResolver = new AssemblyResolver();
            var applicationAssembly = assemblyResolver.GetApplicationAssembly();
            return applicationAssembly != null ? applicationAssembly.GetName().Version.ToString() : null;
        }

        private IList<string> ExtractTags(ReadOnlyPropertiesDictionary loggingEventProperties)
        {
            var rawTags = ResolveTagsFromLog4NetProperties(loggingEventProperties);

            if (!string.IsNullOrEmpty(rawTags))
            {
                LogLog.Debug(DeclaringType, string.Format("RaygunAppender: Found '{0}' property key, extracting raygun tags from '{1}'", RaygunAppenderBase.PropertyKeys.Tags, rawTags));

                return rawTags.Split('|').ToList();
            }

            return null;
        }

        private string ResolveTagsFromLog4NetProperties(ReadOnlyPropertiesDictionary loggingEventProperties)
        {
            return ResolveFromLog4NetProperties<string>(loggingEventProperties, RaygunAppenderBase.PropertyKeys.Tags);
        }

        private HttpRequestMessage ResolveHttpRequestMessageFromLog4NetProperties(ReadOnlyPropertiesDictionary loggingEventProperties)
        {
            return ResolveFromLog4NetProperties<HttpRequestMessage>(loggingEventProperties, PropertyKeys.HttpRequestMessage);
        }

        private T ResolveFromLog4NetProperties<T>(ReadOnlyPropertiesDictionary loggingEventProperties, string key)
            where T : class
        {
            return (loggingEventProperties[key] ?? LogicalThreadContext.Properties[key]
                ?? ThreadContext.Properties[key] ?? GlobalContext.Properties[key]) as T;
        }

        private Dictionary<string, string> FilterRenderedMessageInUserCustomData(Dictionary<string, string> userCustomData, IMessageFilter renderedMessageFilter)
        {
            if (userCustomData.ContainsKey(UserCustomDataBuilder.UserCustomDataKey.RenderedMessage))
            {
                var oldValue = userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage];
                userCustomData[UserCustomDataBuilder.UserCustomDataKey.RenderedMessage] = renderedMessageFilter.ApplyTo(oldValue);
            }

            return userCustomData;
        }

        private RaygunIdentifierMessage ExtractAffectedUser(ReadOnlyPropertiesDictionary loggingEventProperties)
        {
            return ResolveFromLog4NetProperties<RaygunIdentifierMessage>(loggingEventProperties, RaygunAppenderBase.PropertyKeys.AffectedUser);
        }

        internal static class PropertyKeys
        {
            public const string HttpRequestMessage = "log4net.Raygun.WebApi.HttpRequestMessage";
        }
    }
}
