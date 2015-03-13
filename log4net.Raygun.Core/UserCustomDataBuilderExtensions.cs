using System;

namespace log4net.Raygun.Core
{
    internal static class UserCustomDataBuilderExtensions
    {
        internal const string NotSupplied = "Not supplied.";

        internal static string NotSuppliedIfNullOrEmpty(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return NotSupplied;
            }

            return value;
        }
    }
}