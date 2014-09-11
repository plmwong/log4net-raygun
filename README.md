Raygun Appender for log4net
===========================

[![Build status](https://ci.appveyor.com/api/projects/status/l1vkoo634ylqnvep)](https://ci.appveyor.com/project/plmw/log4net-raygun)

Intro
-----
A basic log4net appender which can be used to send logged exceptions to raygun.io. It will also map most of the log4net LoggingEvent fields to the UserCustomData dictionary in the Raygun message sent.

Currently log4net.Raygun supports .NET Target Frameworks 3.5, 4.0 and 4.5 (.NET 3.5 requires an additional NuGet dependency on System.Threading.Tasks).

NuGet
-----
https://www.nuget.org/packages/log4net.Raygun/

or for the log4net 1.2.10 compatible version:

https://www.nuget.org/packages/log4net.1.2.10.Raygun

Configuration
-------------

* `apiKey` (required) : The API key for accessing your application in raygun.io. The API key can be found under 'Application Settings' of your Raygun app.
* `retries` (optional) : The number of times to try and send the exception raygun message to the raygun.io API before giving up and discarding the message. If this setting is not specified, then retries are *disabled* and the appender will only try to log to raygun once, and discard the message if unsuccessful.
* `timeBetweenRetries` (optional) : A `TimeSpan` of the time to wait between retry attempts. If this setting is not specified, then a default of '00:00:05' (5 seconds) is used.
* `onlySendExceptions` (optional) : Toggle whether to send both exceptions and messages logged to ERROR to raygun, or whether to only send logged events which contain exceptions. If this setting is not specified, then by defauly *both* exceptions and error messages will be sent to raygun.
* `exceptionFilter` (optional) : The assembly qualified class name for an implementation of `IMessageFilter`. This filter will be called prior to the Raygun message being sent and can be used to filter out sensitive information from an `Exception.Message`.
* `renderedMessageFilter` (optional) : The assembly qualified class name for an implementation of `IMessageFilter`. This filter will be called prior to the Raygun message being sent and can be used to filter out sensitive information from the RenderedMessage in UserCustomData.

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

You need to use the https://www.nuget.org/packages/log4net.1.2.10.Raygun version built against that version of log4net. The source for this is located in the `log4net-1.2.10` branch.

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

***I have to use version X of log4net, because of reasons***

You might need to add a binding redirection to your application configuration file, redirecting to the version of log4net you are using.

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