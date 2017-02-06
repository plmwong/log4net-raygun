using System.Collections.Generic;
using log4net.Core;
using log4net.Util;

namespace log4net.Raygun.Core
{
    public class UserCustomDataBuilder : IUserCustomDataBuilder
    {
        public Dictionary<string, string> Build(LoggingEvent loggingEvent)
        {
            var assemblyResolver = new AssemblyResolver();
            var applicationAssembly = assemblyResolver.GetApplicationAssembly();
            var applicationAssemblyFullName = applicationAssembly != null ? applicationAssembly.FullName.NotSuppliedIfNullOrEmpty() : UserCustomDataBuilderExtensions.NotSupplied;

            var userCustomData = new Dictionary<string, string>
            {
                {UserCustomDataKey.AssemblyFullName, applicationAssemblyFullName},
                {UserCustomDataKey.Domain, loggingEvent.Domain.NotSuppliedIfNullOrEmpty()},
                {UserCustomDataKey.Level, loggingEvent.Level != null ? loggingEvent.Level.Name : UserCustomDataBuilderExtensions.NotSupplied},
                {UserCustomDataKey.Identity, loggingEvent.Identity.NotSuppliedIfNullOrEmpty()},
                {
                    UserCustomDataKey.LocationInfo, loggingEvent.LocationInformation != null
                        ? loggingEvent.LocationInformation.FullInfo.NotSuppliedIfNullOrEmpty()
                        : UserCustomDataBuilderExtensions.NotSupplied
                },
                {UserCustomDataKey.ThreadName, loggingEvent.ThreadName.NotSuppliedIfNullOrEmpty()},
                {UserCustomDataKey.RenderedMessage, loggingEvent.RenderedMessage.NotSuppliedIfNullOrEmpty()},
                {UserCustomDataKey.TimeStamp, loggingEvent.TimeStamp.ToString("O")},
                {UserCustomDataKey.UserName, loggingEvent.UserName.NotSuppliedIfNullOrEmpty()},
                {UserCustomDataKey.Log4NetRaygunVersion, typeof(RaygunAppenderBase).Assembly.GetName().Version.ToString() }
            };

            AddCustomProperties(loggingEvent, userCustomData);

            return userCustomData;
        }

        private static void AddCustomProperties(LoggingEvent loggingEvent, Dictionary<string, string> userCustomData)
        {
            var properties = loggingEvent.GetProperties() ?? new PropertiesDictionary();
            foreach (var propertyKey in properties.GetKeys())
            {
                var propertyValue = properties[propertyKey].ToString();
                userCustomData.Add($"{UserCustomDataKey.PropertiesPrefix}.{propertyKey}", propertyValue);
            }
        }

        public static class UserCustomDataKey
        {
            public const string AssemblyFullName = "Assembly FullName";
            public const string Domain = "Domain";
            public const string Level = "Level";
            public const string Identity = "Identity";
            public const string LocationInfo = "Location Info";
            public const string RenderedMessage = "Rendered Message";
            public const string ThreadName = "Thread Name";
            public const string TimeStamp = "Time Stamp";
            public const string UserName = "User Name";
            public const string PropertiesPrefix = "Properties";
            public const string Log4NetRaygunVersion = "log4net.Raygun Version";
        }
    }
}