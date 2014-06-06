using Mindscape.Raygun4Net.Messages;

namespace log4net.Raygun
{
	public interface IRaygunClient
	{
		void Send(RaygunMessage raygunMessage);
	}
}

