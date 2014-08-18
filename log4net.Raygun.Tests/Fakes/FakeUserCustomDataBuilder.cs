using log4net.Core;
using System.Collections.Generic;

namespace log4net.Raygun.Tests.Fakes
{
    public class FakeUserCustomDataBuilder : IUserCustomDataBuilder
    {
        private readonly Dictionary<string, string> _userCustomData = new Dictionary<string, string>();

        public Dictionary<string, string> UserCustomData
        {
            get { return _userCustomData; }
        }

        public Dictionary<string, string> Build(LoggingEvent loggingEvent)
        {
            return _userCustomData;
        }
    }
}