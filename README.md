Raygun Appender for log4net
=======

Usage
-----

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

