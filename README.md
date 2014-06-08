Raygun Appender for log4net
===========================

[![Build status](https://ci.appveyor.com/api/projects/status/l1vkoo634ylqnvep)](https://ci.appveyor.com/project/plmw/log4net-raygun)

Intro
-----
A basic log4net appender which can be used to send ERROR/FATAL level exceptions to raygun.io. It will also map most of the log4net LoggingEvent fields to the UserCustomData dictionary in the Raygun message sent.

NuGet
-----
https://www.nuget.org/packages/log4net.Raygun/

Configuration
-------------

* apiKey : The API key for accessing your application in raygun.io. Can be found under 'Application Settings' of your Raygun app.
* retries : The number of times to try and send the exception raygun message to the raygun.io API before giving up and discarding the message.
* timeBetweenRetries : The time to wait between retry attempts. If none is specified then a default of 5 seconds is used.

Example
-------

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
      <apiKey value="<raygun.io API key>" />
      <retries value="<number of times to retry sending to raygun.io>" />
      <timeBetweenRetries value="<time to wait between retries>" />
    </appender>
	...
  </log4net>
</configuration>
```

Questions
---------

*I have to use version X of log4net, because of reasons*

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