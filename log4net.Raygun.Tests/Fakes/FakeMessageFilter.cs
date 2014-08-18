namespace log4net.Raygun.Tests.Fakes
{
    public class FakeMessageFilter : IMessageFilter
    {
        public static string ReplacementMessage
        {
            get { return "I changed your message!"; }
        }

        public string Filter(string message)
        {
            return ReplacementMessage;
        }
    }
}