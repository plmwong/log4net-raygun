namespace log4net.Raygun
{
    public interface IMessageFilter
    {
        string Filter(string message);
    }
}