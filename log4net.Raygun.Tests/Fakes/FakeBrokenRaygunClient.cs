using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun.Tests.Fakes
{
    public class FakeBrokenRaygunClient : IRaygunClient
    {
        public void Send(RaygunMessage raygunMessage)
        {
            throw new TestException();
        }
    }
}