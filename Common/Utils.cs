using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Common
{
    public static class Utils
    {

        private static readonly Dictionary<byte, string> Hex0 = new Dictionary<byte, string>
                                                    {
                                                        {0, "0"},
                                                        {1, "1"},
                                                        {2, "2"},
                                                        {3, "3"},
                                                        {4, "4"},
                                                        {5, "5"},
                                                        {6, "6"},
                                                        {7, "7"},
                                                        {8, "8"},
                                                        {9, "9"},
                                                        {10, "A"},
                                                        {11, "B"},
                                                        {12, "C"},
                                                        {13, "D"},
                                                        {14, "E"},
                                                        {15, "F"},
                                                    };

        private static readonly Dictionary<char, byte> Decimal1 = new Dictionary<char, byte>
                                                    {
                                                        {'0',0},
                                                        {'1',1},
                                                        {'2',2},
                                                        {'3',3},
                                                        {'4',4},
                                                        {'5',5},
                                                        {'6',6},
                                                        {'7',7},
                                                        {'8',8},
                                                        {'9',9},
                                                        {'A',10},
                                                        {'B',11},
                                                        {'C',12},
                                                        {'D',13},
                                                        {'E',14},
                                                        {'F',15},
                                                        {'a',10},
                                                        {'b',11},
                                                        {'c',12},
                                                        {'d',13},
                                                        {'e',14},
                                                        {'f',15},
                                                    };

        private static readonly Dictionary<char, byte> Decimal0 = new Dictionary<char, byte>
                                                    {
                                                        {'0',0},
                                                        {'1',16},
                                                        {'2',32},
                                                        {'3',48},
                                                        {'4',64},
                                                        {'5',80},
                                                        {'6',96},
                                                        {'7',112},
                                                        {'8',128},
                                                        {'9',144},
                                                        {'A',160},
                                                        {'B',176},
                                                        {'C',192},
                                                        {'D',208},
                                                        {'E',224},
                                                        {'F',240},
                                                        {'a',160},
                                                        {'b',176},
                                                        {'c',192},
                                                        {'d',208},
                                                        {'e',224},
                                                        {'f',240},
                                                    };


        public const string IsoDateTimeMask = "yyyy-MM-dd HH:mm:ss";

        public const string IsoDateMask = "yyyy-MM-dd";
        public const string RussianDateMask = "dd.MM.yyyy";

        public static string ByteToHex(byte src)
        {
            var d2 = (byte)(src * 0.0625);
            src = (byte)(src - d2*16);

            return Hex0[d2] + Hex0[src];

        }
        public static bool Mod10Check(string creditCardNumber)
        {
            //// check whether input string is null or empty
            if (string.IsNullOrEmpty(creditCardNumber))
            {
                return false;
            }

            //// 1.	Starting with the check digit double the value of every other digit 
            //// 2.	If doubling of a number results in a two digits number, add up
            ///   the digits to get a single digit number. This will results in eight single digit numbers                    
            //// 3. Get the sum of the digits
            int sumOfDigits = creditCardNumber.Where((e) => e >= '0' && e <= '9')
                            .Reverse()
                            .Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2))
                            .Sum((e) => e / 10 + e % 10);


            //// If the final sum is divisible by 10, then the credit card number
            //   is valid. If it is not divisible by 10, the number is invalid.            
            return sumOfDigits % 10 == 0;
        }

        public static string ToHexString(this ICollection<byte> src)
        {
            var sb = new StringBuilder(src.Count * 2);

            foreach (var b in src)
                sb.Append(ByteToHex(b));

            return sb.ToString();
        }

        public static byte HexToByte(string src)
        {
            if (src.Length == 0)
                throw new Exception("Can not convert empty string to byte");

            if (src.Any(b => !Decimal0.ContainsKey(b)))
                throw new Exception("Inapropriate hex string ["+src+"]");

            var d0 = src.Length == 1 ? '0' : src[0];
            var d1 = src.Length == 1 ? src[0] : src[1];


            return (byte)(Decimal0[d0] + Decimal1[d1]);

        }

        public static byte[] HexToArray(string src)
        {
           if (src.Length % 2 != 0)
               throw new Exception("Дилна строки ["+src+"] не делится нацело на 2");

            var result = new byte[src.Length /2];
            int ri = 0;

            for (var i = 0; i < src.Length; i+=2)
                result[ri++] = HexToByte(src.Substring(i, 2));

            return result;

        }

        public static byte[] GenerateRandomBytes(int length)
        {
            var result = new byte[length];

            var random = new Random();
            random.NextBytes(result);
            return result;
        }


        public static T ParseEnum<T>(this string value)
        {
            return (T) Enum.Parse(typeof (T), value, true);
        }


        public static T ParseEnum<T>(this string value, T defaultValue)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static IEnumerable<T> Enumerate<T>()
        {
            return Enum.GetNames(typeof(T)).Select(ParseEnum<T>);
        }


        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "zero";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            var words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number <= 0) return words;

            if (words != "")
                words += "and ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }

            return words;
        }


        private static readonly Dictionary<int, string> RuDigitsMa = new Dictionary<int, string>
        {
            {0, " ноль"},
            {1, " один"},
            {2, " два"},
            {3, " три"},
            {4, " четыре"},
            {5, " пять"},
            {6, " шесть"},
            {7, " семь"},
            {8, " восемь"},
            {9, " девять"},
            {10, " десять"},
            {11, " одиннадцать"},
            {12, " двенадцать"},
            {13, " тринадцать"},
            {14, " четырнадцать"},
            {15, " пятнадцать"},
            {16, " шестнадцать"},
            {17, " семнадцать"},
            {18, " восемнадцать"},
            {19, " девятнадцать"},
        };

        private static readonly Dictionary<int, string> RuDigitsFe = new Dictionary<int, string>
        {
            {0, " ноль"},
            {1, " одна"},
            {2, " две"},
            {3, " три"},
            {4, " четыре"},
            {5, " пять"},
            {6, " шесть"},
            {7, " семь"},
            {8, " восемь"},
            {9, " девять"},
            {10, " десять"},
            {11, " одиннадцать"},
            {12, " двенадцать"},
            {13, " тринадцать"},
            {14, " четырнадцать"},
            {15, " пятнадцать"},
            {16, " шестнадцать"},
            {17, " семнадцать"},
            {18, " восемнадцать"},
            {19, " девятнадцать"},
        };

        private static readonly Dictionary<int, string> RuDigitsHundreds = new Dictionary<int, string>
        {
            {1, " сто"},
            {2, " двести"},
            {3, " триста"},
            {4, " четыреста"},
            {5, " пятьсот"},
            {6, " шестьсот"},
            {7, " семьсот"},
            {8, " восемьсот"},
            {9, " девятьсот"},
        };


        private static readonly Dictionary<int, string> RuTens = new Dictionary<int, string>
        {
            {2, " двадцать"},
            {3, " тридцать"},
            {4, " сорок"},
            {5, " пятьдесят"},
            {6, " шестдесят"},
            {7, " семьдесят"},
            {8, " восемьдесят"},
            {9, " девяносто"},
        };



        private static void WriteMillionsRu(int amount, StringBuilder sb)
        {
            var thous = GenderAmount(amount, " миллион", " миллиона", " миллионов");
            sb.Append(NumberToWordsRus(amount, true) + thous);
        }


        private static int GenderAmount(int amount)
        {
            if (amount < 20)
                return amount;

            if (amount < 100)
                return (int)(amount*0.1);

            return amount % 10;

        }

        public static string GenderAmount(int amount, string oneWord, string twoWord, string fiveOrZerosWord)
        {
            var genderAmount = GenderAmount(amount);


            switch (genderAmount)
            {
                case 1:
                    return oneWord;

                case 2:
                    return twoWord;

                case 3:
                    return twoWord;

                case 4:
                    return twoWord;
            }
            return fiveOrZerosWord;
        }

        private static void WriteThousantsRu(int amount, StringBuilder sb)
        {
            var thous = GenderAmount(amount, " тысяча", " тысячи", " тысяч");
           sb.Append(NumberToWordsRus(amount, false) + thous);
        }

        private static void WriteHoundredsRu(int amount, StringBuilder sb)
        {
            sb.Append(RuDigitsHundreds[amount]);
        }

        private static void WriteTensRu(int amount, StringBuilder sb)
        {
            sb.Append(RuTens[amount]);
        }


        public static string NumberToWordsRus(int amount, bool ismale)
        {
            if (amount == 0)
            {
                return ismale
                    ? RuDigitsMa[amount]
                    : RuDigitsFe[amount];

            }

            var sb = new StringBuilder();

            var millions = (int)(amount * 0.000001);
            {
                if (millions > 0)
                {
                    WriteMillionsRu(millions, sb);
                    amount -= millions * 1000000;
                }
            }

            var thousants = (int)(amount * 0.001);
            {
                if (thousants > 0)
                {
                    WriteThousantsRu(thousants, sb);
                    amount -= thousants * 1000;
                }
            }

            var hundreds = (int)(amount * 0.01);
            {
                if (hundreds > 0)
                {
                    WriteHoundredsRu(hundreds, sb);
                    amount -= hundreds * 100;
                }
            }


            if (amount < 20 && amount >0)
            {
                sb.Append(ismale
                    ? RuDigitsMa[amount]
                    : RuDigitsFe[amount]);

            }
            else
            {
                var tens = (int)(amount * 0.1);
                if (tens > 0)
                {
                    WriteTensRu(tens, sb);
                    amount -= tens * 10;
                }

                if (amount > 0)
                {
                    sb.Append(ismale
                        ? RuDigitsMa[amount]
                        : RuDigitsFe[amount]);

                }
            }

            return sb.ToString();

        }





        public static string PutLastSymbol(string src, char symbol)
        {
            if (src == null)
                return null;

            if (src == string.Empty)
                return src;

            if (src[src.Length - 1] == symbol)
                return src;

            return src + symbol;
        }

        public static string StringToHex(this string src)
        {
            var array = Encoding.UTF8.GetBytes(src);
            return ToHexString(array);
        }

        public static string HexToString(string value)
        {
            var array = HexToArray(value);
            return Encoding.UTF8.GetString(array, 0, array.Length);
        }

        public static string ParamsToString(object obj)
        {
            var result = new StringBuilder();
            foreach (var propInfo in obj.GetType().GetProperties())
            {
                var value = propInfo.GetValue(obj, null);
                if (value !=null)
                  result.Append(propInfo.Name + "=[" + value + "];");
            }

            return result.ToString();
        }


        /// <summary>
        /// Взять следующую дату, указав день недели. Сегодняшняя дата считается
        /// </summary>
        /// <param name="nowDateTime">текущая дата</param>
        /// <param name="dayOfWeek">день недели</param>
        /// <returns>Дата дня недели</returns>
        public static DateTime GetNextDateByDayOfTheWeek(DateTime nowDateTime, DayOfWeek dayOfWeek)
        {
            var days = (int)dayOfWeek - (int)nowDateTime.DayOfWeek;
            if (days < 0)
                days += 7;
            return nowDateTime.AddDays(days);
        }

        public static DateTime SetNewTime(DateTime dateTime, string time)
        {
            var timedata = time.Split(':').Select(int.Parse).ToArray();
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timedata[0], timedata[1], timedata[2]);
        }

        public static string CombineString(IEnumerable<string> strings, char separator)
        {
            var result = new StringBuilder();
            foreach (var s in strings)
            {
                if (result.Length > 0)
                    result.Append(separator);
                result.Append(s);
            }

            return result.ToString();
        }

        private static bool InList<T>(T element, IEnumerable<T> list,  Func<T, T, bool> equal)
        {
            return list.Any(itm => equal(element, itm));
        }

        public static bool ListsAreSame<T>(T[] oldOne, T[] newOne, Func<T, T, bool> equal )
        {

            if (oldOne == null && newOne == null)
                return true;

            if (oldOne == null && newOne != null)
                return false;

            if (oldOne != null && newOne == null)
                return false;


            if (oldOne.Length != newOne.Length)
                return false;

            if (newOne.Any(itm => !InList(itm, oldOne, equal)))
                return false;

            if (oldOne.Any(itm => !InList(itm, newOne, equal)))
                return false;

            return true;
        }

        public static string ToBase64(this string src)
        {
            var bytes = Encoding.UTF8.GetBytes(src);
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64(this byte[] src)
        {
            return Convert.ToBase64String(src);
        }


        public static string Base64ToString(this string src)
        {
            var bytes = Convert.FromBase64String(src);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }


        public static double ParseAnyDouble(this string amount)
        {
            amount = amount.Replace(',', '.');
            return Double.Parse(amount, CultureInfo.InvariantCulture);
        }

        public static int[] GenerateYearsTillNow(int fromYear)
        {
            var result = new List<int>();
            var nowYear = DateTime.UtcNow.Year;
            for (var year = fromYear; year <= nowYear; year++)
                result.Add(year);

            return result.ToArray();
        }


        public static IEnumerable<IEnumerable<T>> ToPieces<T>(this IEnumerable<T> src, int countInPicese)
        {
            var result = new List<T>();

            foreach (var itm in src)
            {
                result.Add(itm);
                if (result.Count >= countInPicese)
                {
                    yield return result;
                    result = new List<T>();
                }
            }

            if (result.Count>0)
                yield return result;
        }


        public static IEnumerable<T> Limit<T>(this IEnumerable<T> src, int limit)
        {
            var no = 0;
            foreach (var itm in src)
            {
                yield return itm;
                no++;
                if (no >= limit)
                    break;
            }
        }

        public static IEnumerable<int> GenerateInts(int from, int to)
        {
            for (var i = from; i <= to; i++)
                yield return i;
        }


        public static Dictionary<TKey, TValue> CloneDictionary<TKey, TValue>(this Dictionary<TKey, TValue> src)
        {
            var result = new Dictionary<TKey, TValue>();
            foreach (var itm in src)
                result.Add(itm.Key, itm.Value);
            return result;
        }


        public static Dictionary<TKey, Dictionary<TKey2, TValue>> CloneDoubleDictionary<TKey,TKey2, TValue>(this Dictionary<TKey, Dictionary<TKey2, TValue>> src)
        {
            var result = new Dictionary<TKey, Dictionary<TKey2, TValue>>();
            foreach (var itm1 in src)
            {
                var subDictionary = new Dictionary<TKey2, TValue>();
                result.Add(itm1.Key, subDictionary);

                foreach (var itm2 in itm1.Value)
                    subDictionary.Add(itm2.Key, itm2.Value);
            }
            return result;
        }


        public static string GeneratePassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(this IEnumerable<TValue> src, Func<TValue, TKey> getKey)
        {
            var result = new SortedDictionary<TKey, TValue>();

            foreach (var itm in src)
                result.Add(getKey(itm), itm);

            return result;
        }

        public static byte ToByte(this bool src)
        {
            return src ? (byte)1 : (byte)0;
        }


        private static readonly Dictionary<Type, Type> SimpleTypes = new Dictionary<Type, Type>
        {
            {typeof(bool), typeof(bool)},
            {typeof(byte), typeof(byte)},
            {typeof(short), typeof(short)},
            {typeof(ushort), typeof(ushort)},
            {typeof(int), typeof(int)},
            {typeof(uint), typeof(uint)},
            {typeof(long), typeof(long)},
            {typeof(ulong), typeof(ulong)},
            {typeof(double), typeof(double)},
            {typeof(float), typeof(float)},

        }; 


        public static bool IsSimpleType(this Type type)
        {
            return SimpleTypes.ContainsKey(type);
        }

        public static MemoryStream ToStream(this string src)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(src))
            {
                Position = 0
            };
        }

        public static byte[] ToBytes(this Stream src)
        {

            var memoryStream = src as MemoryStream;

            if (memoryStream != null)
                return memoryStream.ToArray();


            src.Position = 0;
            var result = new MemoryStream();

            src.CopyTo(result);
            return result.ToArray();
        }

        public static async Task<byte[]> ToBytesAsync(this Stream src)
        {

            var memoryStream = src as MemoryStream;

            if (memoryStream != null)
                return memoryStream.ToArray();


            var result = new MemoryStream();
            await src.CopyToAsync(result);
            return result.ToArray();
        }

        public static byte[] ToUtf8Bytes(this string src)
        {
            return Encoding.UTF8.GetBytes(src);
        }

        public static Stream ToStream(this byte[] src)
        {
            if (src == null)
                return null;

            return new MemoryStream(src) {Position = 0};
        }


        /// <summary>
        /// Проверить то, что последовательность левая начиная от indexOf равна последовательности search
        /// </summary>
        /// <returns></returns>
        public static bool AreSame(List<byte> src, byte[] search, int indexOf)
        {
            if (search.Length > src.Count - indexOf)
                return false;

            for (var i = 0; i < search.Length; i++)
            {
                if (src[indexOf++] != search[i])
                    return false;
            }

            return true;
        }


        public static int IndexOf(this List<byte> src, byte[] search)
        {
            if (search.Length == 0)
                return -1;

            for (var i = 0; i < src.Count ; i++)
            {
                if (src[i] != search[0]) continue;
                return AreSame(src, search, i) ? i : -1; 
            }

            return -1;
        }



        public static IEnumerable<T> CutFrom<T>(this IEnumerable<T> src, int from, int length)
        {
            var i = 0;

            var indexTo = from + length;
            
            foreach (var itm in src)
            {
                if (i >= indexTo)
                    yield break;

                if (i >= from && i < indexTo)
                    yield return itm;

                i++;
            }
        }

        public static IEnumerable<IEnumerable<T>> ToChunks<T>(this IEnumerable<T> src, int chunkSize)
        {
            var chunk = new List<T>();

            foreach (var item in src)
            {
                chunk.Add(item);

                if (chunk.Count >= chunkSize)
                {
                    yield return chunk.ToArray();
                    chunk.Clear();
                }

            }

            if (chunk.Count > 0)
                yield return chunk;
        }

    }

}
