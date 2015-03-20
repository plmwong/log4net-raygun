Raygun Appender for log4net
===========================

[![Build status](https://ci.appveyor.com/api/projects/status/72mst5et9y15w1mc/branch/master?svg=true)](https://ci.appveyor.com/project/plmw/log4net-raygun-rb4sk/branch/master)

Intro
-----
A log4net appender which can be used to send logged errors and exceptions to raygun.io. It will also map most of the log4net LoggingEvent fields to the UserCustomData dictionary in the Raygun message sent.

Currently log4net.Raygun supports .NET Target Frameworks 4.0 and 4.5. Releases of log4net.Raygun older than 4.0.0 also support .NET 3.5, which requires an additional NuGet dependency on System.Threading.Tasks.Unofficial.

NuGet
-----
https://www.nuget.org/packages/log4net.Raygun/

or for the log4net 1.2.10 compatible version:

https://www.nuget.org/packages/log4net.1.2.10.Raygun

If you use either Mindscape.Raygun4Net.Mvc or Mindscape.Raygun4Net.WebApi, you can use:

https://www.nuget.org/packages/log4net.Raygun.Mvc/
https://www.nuget.org/packages/log4net.Raygun.WebApi/

Configuration
-------------

* `apiKey` (required) : The API key for accessing your application in raygun.io. The API key can be found under 'Application Settings' of your Raygun app.
* `retries` (optional) : The number of times to try and send the exception raygun message to the raygun.io API before giving up and discarding the message. If this setting is not specified, then retries are *disabled* and the appender will only try to log to raygun once, and discard the message if unsuccessful.
* `timeBetweenRetries` (optional) : A `TimeSpan` of the time to wait between retry attempts. If this setting is not specified, then a default of '00:00:05' (5 seconds) is used.
* `onlySendExceptions` (optional) : Toggle whether to send both exceptions and messages logged to ERROR to raygun, or whether to only send logged events which contain exceptions. If this setting is not specified, then by defauly *both* exceptions and error messages will be sent to raygun.
* `sendInBackground` (optional) : Toggle whether to send messages to raygun in a background task. If set to false then raygun messages will be sent synchronously. By default this is set to true.
* `exceptionFilter` (optional) : The assembly qualified class name for an implementation of `IMessageFilter`. This filter will be called prior to the Raygun message being sent and can be used to filter out sensitive information from an `Exception.Message`.
* `renderedMessageFilter` (optional) : The assembly qualified class name for an implementation of `IMessageFilter`. This filter will be called prior to the Raygun message being sent and can be used to filter out sensitive information from the RenderedMessage in UserCustomData.

The following configuration properties can be used to omit sensitive data from being sent to raygun (as introduced in raygun4net 3.0):

* `ignoredFormNames` (optional) : Comma delimited list of form field names to omit when logging details of an Http Request.
* `ignoredHeaderNames` (optional) : Comma delimited list of header field names to omit when logging details of an Http Request.
* `ignoredCookieNames` (optional) : Comma delimited list of cookie field names to omit when logging details of an Http Request.
* `ignoredServerVariableNames` (optional) : Comma delimited list of server variable field names to omit when logging details of an Http Request.

A log4net `threshold` should be used to filter out logging levels (e.g. everything below ERROR level). By default, all levels of logging will be sent to Raygun.

Configuration Example
---------------------

```
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,log4net" />
  </configSections>
  ...
  <log4net>
    ...
    <appender name="RaygunAppender" type="log4net.Raygun.RaygunAppender, log4net.Raygun">
      <threshold value="ERROR" />
      <apiKey value="8oe2eItifdYUuxVOe4VhqQ==" />
      <!-- Attempt to send errors to raygun 15 times -->
      <retries value="15" />
      <!-- Wait 1 minute between retry attempts -->
      <timeBetweenRetries value="00:01:00" />
	  <!-- Toggles whether to only send exceptions to raygun, or to also send messages logged to ERROR -->
	  <onlySendExceptions value="true" />
      <!-- Optional filters for filtering exceptions and messages before sending to raygun -->
      <exceptionFilter value="SomeOtherAssembly.SensitiveInformationMessageFilter, SomeOtherAssembly" />
      <renderedMessageFilter value="SomeOtherAssembly.SensitiveInformationMessageFilter, SomeOtherAssembly" />
    </appender>
	...
  </log4net>
</configuration>
```

Questions
---------

***My application uses the older 1.2.10 version of log4net, before they went and changed the public key***

You can use the https://www.nuget.org/packages/log4net.1.2.10.Raygun version built against that version of log4net. The source for this is located in the `log4net-1.2.10` branch.

***My application logs sensitive information which we would rather not send to a third-party***

log4net.Raygun now allows you to implement an `IMessageFilter`, and then configure the RaygunAppender to use filters in the appender configuration.
The filters allow for exceptions and/or the rendered log4net message to be sanitized before it is sent to raygun.

E.g.

```
public class SensitiveInformationFilter : IMessageFilter
{
	public string Filter(string message)
	{
		var newMessage = message.Replace("Very Sensitive Information.", "Move Along. Nothing to see here.");
		return newMessage;
	}
}
```

***What about tags?***

As of version 2.1, log4net.Raygun provides a basic mechanism for populating tags in a raygun message.
Tags can be stored as custom data in a pipe-delimited format (e.g. tag1|tag2|tag3) on the `log4net.LogicalThreadContext` or `log4net.GlobalContext` collections, prior to calling a log4net logging method..

```
log4net.LogicalThreadContext.Properties[RaygunAppender.PropertyKeys.Tags] = "important|squirrel-related";
log.Error("Something bad happened to your squirrel!"); 
```

When constructing the raygun message to send to raygun.io, log4net.Raygun will use the tags stored in this collection to populate the raygun message tags.

***I have to use version X of log4net, because of reasons, and it is newer/older than the one log4net.Raygun is built against***

You may need to add a binding redirection to your application configuration file, redirecting to the version of log4net you are using.

E.g. to redirect all versions of log4net older than 1.2.12.0 to use 1.2.12.0:

```
<runtime>
  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
    <dependentAssembly>
      <assemblyIdentity name="log4net" publicKeyToken="669e0ddf0bb1aa2a" culture="neutral" />
      <bindingRedirect oldVersion="0.0.0.0-1.2.12.0" newVersion="1.2.12.0" />
    </dependentAssembly>
  </assemblyBinding>
</runtime>
```

***I use Mindscape.Raygun4Net.Mvc in my Mvc project***

log4net.Raygun is assembly compatible with Mindscape.Raygun4Net.Mvc and should work normally.

If you would rather not have the Mindscape.Raygun4Net package added into your assembly though, you can instead use the log4net.Raygun.Mvc package.

***I use Mindscape.Raygun4Net.WebApi in my WebApi project***

Use log4net.Raygun.Webapi instead.

Note that you will need to store the WebApi `HttpRequestMessage` so that log4net.Raygun appender will be able to record the request details. This can either be done directly in individual Controllers, or
by adding the provided `RaygunHttpRequestHandler` to the WebApi `MessageHandlers`. E.g.

```
public class SomeController : ApiController
{
    public IEnumerable<Thing> GetSomeThings()
    {
        log4net.LogicalThreadContext.Properties["log4net.Raygun.WebApi.HttpRequestMessage"] = Request;
    }
}
```

```
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        config.MessageHandlers.Add(new RaygunHttpRequestHandler());
    }
}
```

