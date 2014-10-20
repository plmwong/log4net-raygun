namespace log4net.Raygun
{
    public class RaygunSettings : IRaygunSettings
    {
        public bool ConfiguredFromXml
        {
            get { return Mindscape.Raygun4Net.RaygunSettings.Settings.IsReadOnly(); }
        }

        public bool ThrowOnError
        {
            set { Mindscape.Raygun4Net.RaygunSettings.Settings.ThrowOnError = value; }
            get { return Mindscape.Raygun4Net.RaygunSettings.Settings.ThrowOnError; }
        }
    }
}