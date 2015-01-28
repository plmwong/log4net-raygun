namespace log4net.Raygun.Core
{
    public class DoNothingMessageFilter : IMessageFilter
    {
        public string Filter(string message)
        {
            return message;
        }
    }
}