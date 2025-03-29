using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FloppyShelf.UserManagement.Services
{
    public class UniqueUsernameGenerator
    {/// <summary>
     /// Dictionary that holds replacement rules for certain characters or substrings.
     /// </summary>
        private readonly Dictionary<string, string> _replacementRules;

        /// <summary>
        /// Regular expression to match invalid characters that are not alphanumeric.
        /// </summary>
        private readonly Regex _invalidCharsRegex;

        /// <summary>
        /// The minimum allowed length for a generated username.
        /// </summary>
        private const int MinUsernameLength = 6;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsernameGenerator"/> class with optional replacement rules.
        /// </summary>
        /// <param name="replacementRules">A dictionary of replacement rules for certain characters (optional).</param>
        public UniqueUsernameGenerator(Dictionary<string, string> replacementRules = null)
        {
            // If no replacement rules are provided, use default rules.
            _replacementRules = replacementRules ?? new Dictionary<string, string>
            {
                { "Sch", "S" }, // Replace "Sch" with "S"
                { "sch", "s" }, // Replace "sch" with "s"
                { "Ä", "Ae" },  // Replace "Ä" with "Ae"
                { "Ö", "Oe" },  // Replace "Ö" with "Oe"
                { "Ü", "Ue" },  // Replace "Ü" with "Ue"
                { "ä", "ae" },  // Replace "ä" with "ae"
                { "ö", "oe" },  // Replace "ö" with "oe"
                { "ü", "ue" },  // Replace "ü" with "ue"
                { "ß", "ss" },  // Replace "ß" with "ss"
            };

            // Regex that matches any character that is not alphanumeric.
            _invalidCharsRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
        }

        /// <summary>
        /// Generates a unique username based on the first and last name, ensuring it falls within the specified length limits.
        /// </summary>
        /// <param name="firstName">The first name used to generate the username.</param>
        /// <param name="lastName">The last name used to generate the username.</param>
        /// <param name="minLength">The minimum allowed length for the username.</param>
        /// <param name="maxLength">The maximum allowed length for the username.</param>
        /// <param name="existingUsernames">A set of already existing usernames to avoid conflicts.</param>
        /// <returns>A unique username that fits within the specified length range.</returns>
        /// <exception cref="ArgumentException">Thrown if the first name or last name is empty or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the minimum length is less than 6 or if max length is less than min length.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no valid username can be generated.</exception>
        public string GenerateUniqueUsername(string firstName, string lastName, int minLength, int maxLength, HashSet<string> existingUsernames)
        {
            // Validate input arguments.
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentException("First name cannot be empty or whitespace.", nameof(firstName));
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                throw new ArgumentException("Last name cannot be empty or whitespace.", nameof(lastName));
            }

            if (minLength < MinUsernameLength)
            {
                throw new ArgumentOutOfRangeException(nameof(minLength), $"Minimum length must be at least {MinUsernameLength} characters.");
            }

            if (maxLength < minLength)
            {
                throw new ArgumentOutOfRangeException(nameof(maxLength), "Maximum length must be greater than or equal to minimum length.");
            }

            // Try generating a username in normal or reversed order.
            foreach (var reverseOrder in new[] { false, true })
            {
                // Iterate through potential lengths from minLength to maxLength.
                for (int length = minLength; length <= maxLength; length++)
                {
                    // Attempt to generate a base username without a numeric suffix.
                    string baseUsername = BuildBaseUsername(firstName, lastName, length, "", reverseOrder);
                    if (!existingUsernames.Contains(baseUsername))
                    {
                        return baseUsername; // Return if the generated username is unique.
                    }

                    // Generate numeric suffixes if base username already exists.
                    int maxSuffixDigits = Math.Max(1, length / 3);
                    int maxSuffixNumber = (int)Math.Pow(10, maxSuffixDigits) - 1;

                    for (int suffixNum = 1; suffixNum <= maxSuffixNumber; suffixNum++)
                    {
                        // Ensure the suffix doesn't make the username too short.
                        string suffix = suffixNum.ToString($"D{maxSuffixDigits}");
                        int availableLength = length - suffix.Length;

                        if (availableLength < 2)
                        {
                            continue; // Skip if the remaining length is too short for a valid username.
                        }

                        // Attempt to generate a username with a numeric suffix.
                        string username = BuildBaseUsername(firstName, lastName, availableLength, suffix, reverseOrder);
                        if (!existingUsernames.Contains(username))
                        {
                            return username; // Return if the username with suffix is unique.
                        }
                    }
                }
            }

            // If no unique username is found, throw an exception.
            throw new InvalidOperationException("Unable to generate a unique username within the specified length limits.");
        }

        /// <summary>
        /// Builds a base username by applying replacements and truncating the first name and last name.
        /// </summary>
        /// <param name="firstName">The first name to build the username from.</param>
        /// <param name="lastName">The last name to build the username from.</param>
        /// <param name="totalLength">The total desired length of the username.</param>
        /// <param name="suffix">A suffix to append to the username (if any).</param>
        /// <param name="reverseOrder">Indicates whether to reverse the order of the first and last names.</param>
        /// <returns>A base username that fits the specified length and rules.</returns>
        private string BuildBaseUsername(string firstName, string lastName, int totalLength, string suffix, bool reverseOrder)
        {
            // Clean and apply replacements to the first and last names.
            string cleanedFirstName = ApplyReplacementsAndClean(firstName);
            string cleanedLastName = ApplyReplacementsAndClean(lastName);

            // Calculate half length for dividing the username between the first and last name parts.
            int halfLength = totalLength / 2;
            int minPartLength = 2; // Ensure that each part has at least 2 characters.

            // Determine the first part of the username (either first or last name based on reverseOrder).
            string part1 = TakeFirstNCharacters(reverseOrder ? cleanedLastName : cleanedFirstName, Math.Max(halfLength, minPartLength));

            // Determine the second part of the username.
            string part2 = TakeFirstNCharacters(reverseOrder ? cleanedFirstName : cleanedLastName, totalLength - part1.Length);

            // Return the final username, combining both parts and the suffix.
            return part1 + part2 + suffix;
        }

        /// <summary>
        /// Applies the replacement rules to the input string and removes invalid characters.
        /// </summary>
        /// <param name="input">The input string to clean and apply replacements to.</param>
        /// <returns>A cleaned version of the input string with applied replacements and no invalid characters.</returns>
        private string ApplyReplacementsAndClean(string input)
        {
            // Return an empty string if the input is null or empty.
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Iterate through all replacement rules and apply them to the input string.
            foreach (var rule in _replacementRules)
            {
                input = input.Replace(rule.Key, rule.Value);
            }

            // Remove any characters that are not alphanumeric using the regex.
            return _invalidCharsRegex.Replace(input, string.Empty);
        }

        /// <summary>
        /// Takes the first 'n' characters from the input string.
        /// </summary>
        /// <param name="input">The input string to take characters from.</param>
        /// <param name="n">The number of characters to take.</param>
        /// <returns>The first 'n' characters of the input string, or the whole string if it is shorter than 'n'.</returns>
        private string TakeFirstNCharacters(string input, int n)
        {
            // If the string is shorter than 'n', return the entire string. Otherwise, return the first 'n' characters.
            return input.Length <= n ? input : input.Substring(0, n);
        }
    }
}
