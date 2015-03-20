namespace log4net.Raygun.Core
{
    public interface IMessageFilter
    {
        string Filter(string message);
    }

    public static class MessageFilterExtensions
    {
        public static string ApplyTo(this IMessageFilter messageFilter, string message)
        {
            if (messageFilter != null)
            {
                return messageFilter.Filter(message);
            }

            return message;
        }
    }
}