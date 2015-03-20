using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun.Core
{
    public interface IRaygunClient
    {
        void Send(RaygunMessage raygunMessage);
    }
}