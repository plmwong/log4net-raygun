namespace log4net.Raygun.Core
{
    public interface IRaygunClientFactory
    {
        IRaygunClient Create(string apiKey);
    }
}