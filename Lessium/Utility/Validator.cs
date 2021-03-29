using System.Linq;
using System.Text.RegularExpressions;

namespace Lessium.Utility
{
    public static class Validator
    {
        private static readonly Regex onlyDigitsRegex = new Regex("\\d");
        private static readonly Regex validNumericRegex = new Regex(@"^[1-9]\d*$");

        /// <summary>
        /// Checks if given string contains only numbers.
        /// </summary>
        /// <param name="text">string to check</param>
        /// <returns>True if only digits present, false otherwise</returns>
        public static bool IsOnlyDigits(string text)
        {
            return onlyDigitsRegex.IsMatch(text);
        }

        public static string RemoveNonDigits(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }

        public static bool IsValidNumeric(string text)
        {
            return validNumericRegex.IsMatch(text);
        }
    }
}
