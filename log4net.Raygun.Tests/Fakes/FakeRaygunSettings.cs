namespace log4net.Raygun.Tests.Fakes
{
    public class FakeRaygunSettings : IRaygunSettings
    {
        public FakeRaygunSettings(bool configuredFromXml = false)
        {
            ConfiguredFromXml = configuredFromXml;
        }

        public bool ConfiguredFromXml { get; private set; }

        public bool ThrowOnError { get; set; }
    }
}