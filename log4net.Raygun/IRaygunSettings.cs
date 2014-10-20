namespace log4net.Raygun
{
    public interface IRaygunSettings
    {
        bool ConfiguredFromXml { get; }
        bool ThrowOnError { get; set; }
    }
}