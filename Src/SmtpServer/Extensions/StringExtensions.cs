using System;

namespace SmtpServer
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns a value indicating whether or not the given string is equal on a case insensitive comparisson.
        /// </summary>
        /// <param name="str">The string to compare.</param>
        /// <param name="test">The string to test against.</param>
        /// <returns>true if the strings are equal on a case insensitive comparisson.</returns>
        public static bool CaseInsensitiveEquals(this string str, string test)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return String.Equals(str, test, StringComparison.OrdinalIgnoreCase);
        }
    }
}