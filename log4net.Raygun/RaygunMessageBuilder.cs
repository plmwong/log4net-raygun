﻿using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using log4net.Raygun.Core;
using log4net.Util;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;
using IRaygunMessageBuilder = log4net.Raygun.Core.IRaygunMessageBuilder;

namespace log4net.Raygun
{
    public class RaygunMessageBuilder : IRaygunMessageBuilder
    {
        public static readonly Type DeclaringType = typeof(RaygunAppenderBase);

        private readonly Func<IHttpContext> _httpContextFactory;

        public RaygunMessageBuilder(Func<IHttpContext> httpContextFactory)
        {
            _httpContextFactory = httpContextFactory;
        }

        public RaygunMessage BuildMessage(Exception exception, LoggingEvent loggingEvent, Dictionary<string, string> userCustomData,
            IMessageFilter exceptionFilter, IMessageFilter renderedMessageFilter, IgnoredDataSettings ignoredFieldSettings, string customApplicationVersion)
        {
            var raygunMessageBuilder = Mindscape.Raygun4Net.RaygunMessageBuilder.New;

            var httpContext = _httpContextFactory();
            if (httpContext != null && httpContext.Instance != null)
            {
                LogLog.Debug(DeclaringType, "RaygunAppender: Setting http details on the raygun message from http context");

                var messageOptions = new RaygunRequestMessageOptions(
                    ignoredFieldSettings.IgnoredSensitiveFieldNames,
                    ignoredFieldSettings.IgnoredQueryParameterNames,
                    ignoredFieldSettings.IgnoredFormNames,
                    ignoredFieldSettings.IgnoredHeaderNames,
                    ignoredFieldSettings.IgnoredCookieNames,
                    ignoredFieldSettings.IgnoredServerVariableNames)
                {
                    IsRawDataIgnored = ignoredFieldSettings.IsRawDataIgnored
                };

                raygunMessageBuilder.SetHttpDetails(httpContext.Instance, messageOptions);
            }

            raygunMessageBuilder
                .SetExceptionDetails(exception)
                .SetClientDetails()
                .SetTags(ExtractTags(loggingEvent.GetProperties()))
                .SetUser(ExtractAffectedUser(loggingEvent.GetProperties()))
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
            return loggingEventProperties[RaygunAppenderBase.PropertyKeys.Tags] as string;
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
            return loggingEventProperties[RaygunAppenderBase.PropertyKeys.AffectedUser] as RaygunIdentifierMessage;
        }
    }
}
