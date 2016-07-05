using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    public static class StringUtils
    {
        /// <summary>
        /// Check if email is valid
        /// </summary>
        /// <param name="email">Email string to check</param>
        /// <returns>true - if email string is valid</returns>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            email = email.Trim();

            var lines = email.Split('@');

            if (lines.Length != 2)
                return false;

            if (lines[0].Trim() == "" || lines[1].Trim() == "")
                return false;

            if (lines[0].Contains(' ') || lines[1].Contains(' '))
                return false;

            var lines2 = lines[1].Split('.');

            return lines2.Length >= 2;
        }

        public static int IndexOfFromEnd(this string src, char c, int? from = null)
        {
            if (from == null)
                from = src.Length - 1;

            for (var i = from.Value; i >= 0; i--)
                if (src[i] == c)
                    return i;

            return -1;
        }

        public static int FindIndexBeforeTheStatement(this string src, string statement, char c)
        {
            for (var i = src.Length - 1 - statement.Length; i >= 0; i--)
            {
                var index = src.IndexOf(statement, i, StringComparison.Ordinal);
                if (index >= 0)
                    return src.IndexOfFromEnd(c, i);
            }


            return -1;

        }


        private static readonly Dictionary<char, char> OnlyDigits = new Dictionary<char, char> { { '0', '0' }, { '1', '1' }, { '2', '2' }, { '3', '3' }, { '4', '4' }, { '5', '5' }, { '6', '6' }, { '7', '7' }, { '8', '8' }, { '9', '9' }, }; 
        /// <summary>
        /// Проверить на то что строка состоит только из цифр
        /// </summary>
        /// <param name="data">строка</param>
        /// <returns>true - в строке только цифры</returns>
        public static bool IsOnlyDigits(this string data)
        {
            return data.All(ch => OnlyDigits.ContainsKey(ch));
        }

        public static bool IsDigit(this char c)
        {
            return OnlyDigits.ContainsKey(c);
        }


        private static readonly Regex IsGuidRegex =
           new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        /// <summary>
        /// проверить на то что строка в фрмате GUID
        /// </summary>
        /// <param name="src">исходная строка</param>
        /// <returns>Строка соответствует формату GUID</returns>
        public static bool IsGuid(this string src)
        {
            return !string.IsNullOrEmpty(src) && IsGuidRegex.IsMatch(src);
        }

        public static string RemoveCountryPhonePrefix(this string src)
        {
            if (src == null)
                return null;

            if (src.Length < 5)
                return src;


            if (src[0] == '+')
                return src.Substring(2, src.Length - 2);

            if (src.StartsWith("00"))
                return src.Substring(3, src.Length - 3);

            if (src[0] == '8' && src.Length>=11)
                return src.Substring(1, src.Length - 1);

            return src;

        }
        public static string AddFirstSymbolIfNotExists(this string src, char symbol)
        {
            if (string.IsNullOrEmpty(src))
                return symbol+"";

            return src[0] == symbol ? src : symbol+src;
        }

        public static string AddLastSymbolIfNotExists(this string src, char symbol)
        {
            if (string.IsNullOrEmpty(src))
                return ""+symbol;

            return src[src.Length - 1] == symbol ? src : src+symbol;
        }

        public static string RemoveLastSymbolIfExists(this string src, char symbol)
        {
            if (string.IsNullOrEmpty(src))
                return src;

            return src[src.Length - 1] == symbol ? src.Substring(0, src.Length - 1) : src;
        }

        public static string RemoveFirstSymbolIfExists(this string src, char symbol)
        {
            if (String.IsNullOrEmpty(src))
                return src;

            return src[0] == symbol ? src.Substring(1, src.Length - 1) : src;
        }


        public static string SubstringTillSymbol(this string src, int from, char c)
        {
            var indexOf = src.IndexOf(c);

            if (indexOf < 0)
                return @from == 0 ? src : src.Substring(@from, src.Length - @from);


            return src.Substring(@from, indexOf - @from);
        }

        public static string OneLineViaSeparator(this IEnumerable<string> src, char separator)
        {
            var result = new StringBuilder();

            foreach (var s in src)
            {
                if (result.Length > 0)
                    result.Append(separator);

                result.Append(s);
            }

            return result.ToString();
        }

        public static string SubstringExt(this string src, int from, int to)
        {
            return src.Substring(@from, to - @from+1);
        }





        /// <summary>
        /// Get Substring between chars
        /// </summary>
        /// <param name="src">source string</param>
        /// <param name="from">from char</param>
        /// <param name="to">to chat</param>
        /// <param name="skipFrames"></param>
        /// <returns></returns>
        public static string SubstringBetween(this string src, char from, char to, int skipFrames = 0)
        {
            var fromIndex = 0;
            var toIndex = -1;

            for (var i = 0; i <= skipFrames; i++)
            {
                toIndex++;
                fromIndex = src.IndexOf(@from, toIndex) + 1;

                if (fromIndex == 0)
                    return null;

                toIndex = src.IndexOf(to, fromIndex);
            }

            if (toIndex == 0)
                toIndex = src.Length;

            return SubstringExt(src, fromIndex, toIndex-1);
        }

        /// <summary>
        /// Get substring right after the char from
        /// </summary>
        /// <param name="src">source string</param>
        /// <param name="from">from char</param>
        /// <param name="skipCount">how many to skip chars first</param>
        /// <returns></returns>
        public static string SubstringFromChar(this string src, char from, int skipCount = 0)
        {
            var fromIndex = 0;

            for (var i = 0; i <= skipCount; i++)
            {
                fromIndex = src.IndexOf(@from, fromIndex) + 1;

                if (fromIndex == 0)
                    return null;
            }


            return SubstringExt(src, fromIndex, src.Length - 1);
        }

        public static string SubstringFromString(this string src, string from, int skipCount = 0)
        {
            var fromIndex = 0;

            for (var i = 0; i <= skipCount; i++)
            {
                fromIndex = src.IndexOf(@from, fromIndex, StringComparison.Ordinal) + @from.Length;

                if (fromIndex == 0)
                    return null;
            }


            return SubstringExt(src, fromIndex, src.Length - 1);
        }

        public static string ToLowCase(this string src)
        {
            return src?.ToLower();
        }

        public static Tuple<string, string> GetFirstNameAndLastName(this string src)
        {

            if (string.IsNullOrEmpty(src))
                return new Tuple<string, string>(null, null);

            var fl = src.Split(' ');

            if (fl.Length == 1)
                return new Tuple<string, string>(fl[0], null);

            return new Tuple<string, string>(fl[0], fl[1]);

        }

        public static string GenerateId()
        {
            return Guid.NewGuid().ToString().ToLower();
        }



        public static string FirstLetterLowCase(this string src)
        {

            if (string.IsNullOrEmpty(src))
                return src;

            var firstLetter = char.ToLower(src[0]);

            if (firstLetter == src[0])
                return src;

            return firstLetter + src.Substring(1, src.Length - 1);

        }

        public static string ToStringViaSeparator<T>(this IEnumerable<T> str, string separator)
        {
            if (str == null)
                return null;

            var result = new StringBuilder();
            foreach (var s in str)
            {
                if (result.Length > 0)
                    result.Append(separator);
                result.Append(s);
            }

            return result.ToString();
        }

        public static IEnumerable<string> FromStringViaSeparator(this string str, char separator)
        {
            if (string.IsNullOrEmpty(str)) yield break;

            foreach (var s in str.Split(separator))
                yield return s;
        }


        public static string ExtractWebSiteAndPath(this string src)
        {
            if (src == null)
                return null;

            var qIndex = src.IndexOf('?');

            return qIndex < 0 ? src : src.Substring(0, qIndex);
        }

        public static string ExtractWebSiteDomain(this string src)
        {
            if (src == null)
                return null;

            var indexFrom = src.IndexOf(@"//", StringComparison.Ordinal);
            if (indexFrom < 0)
                return null;

            var indexTo = src.IndexOf(@"/", indexFrom+2, StringComparison.Ordinal);

            if (indexTo < 0)
                return src.Substring(indexFrom + 2, src.Length - indexFrom - 2);


            return src.Substring(indexFrom + 2, indexTo - indexFrom - 2);
        }


        public static string ExtractWebSiteRoot(this string src)
        {
            if (src == null)
                return null;

            var indexFrom = src.IndexOf(@"//", StringComparison.Ordinal);
            if (indexFrom < 0)
                return null;

            var indexTo = src.IndexOf(@"/", indexFrom + 2, StringComparison.Ordinal);

            if (indexTo < 0)
                return src.Substring(indexFrom + 2, src.Length - indexFrom - 2);


            return src.Substring(0, indexTo);
        }

        public static int FindFirstNonSpaceSymbolIndex(this string src, int from = 0)
        {
            if (src == null)
                return -1;

            for(var i= from; i<src.Length; i++)
                if (src[i] > ' ')
                    return i;

            return -1;
        }

        public static int IndexOfNotAny(this string src, int startIndex, params char[] symbols)
        {
            for(var i=startIndex; i< src.Length; i++)
            {
                if (!symbols.Any(c => c==src[i]))
                    return i;
            }

            return -1;
        }

        public static string RightSubstring(this string src, int length)
        {
            if (src == null)
                return null;

            if (src.Length < length)
                return src;


            return src.Substring(src.Length - length, length);

        }
    

        public static byte[] ToUtf8ByteArray(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }


    }
}
