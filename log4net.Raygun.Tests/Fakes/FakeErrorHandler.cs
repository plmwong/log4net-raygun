using System;
using System.Collections.Generic;
using log4net.Core;

namespace log4net.Raygun.Tests.Fakes
{
    public class FakeErrorHandler : IErrorHandler
    {
        private readonly List<string> _errors = new List<string>();
        public List<string> Errors { get { return _errors; }}

        public void Error(string message, Exception e, ErrorCode errorCode)
        {
            _errors.Add(string.Format("{0}|{1}|{2}", message, e, errorCode));
        }

        public void Error(string message, Exception e)
        {
            _errors.Add(string.Format("{0}|{1}", message, e));
        }

        public void Error(string message)
        {
            _errors.Add(message);
        }
    }
}