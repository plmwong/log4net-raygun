namespace log4net.Raygun
{
    public class DoNothingMessageFilter : IMessageFilter
    {
        public string Filter(string message)
        {
            return message;
        }
    }
}