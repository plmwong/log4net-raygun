namespace log4net.Raygun.Filters
{
	public interface IRenderedMessageFilter
	{
		string Filter(string renderedMessage);
	}
}

