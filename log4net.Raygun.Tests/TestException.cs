using System;

namespace log4net.Raygun.Tests
{
    public class TestException : Exception
    {
        public TestException(Exception innerException) : base(string.Empty, innerException)
        {
        }

        public TestException()
        {
        }
    }
}