using System.Collections.Generic;


namespace Common.Json
{
    public static class JsonUtils
    {

        private static string ReadBetween(this string json, char symbol, ref int handle)
        {
            handle = json.IndexOf(symbol, handle) + 1;
            if (handle < 0)
                return null;

            var iTo = json.IndexOf(symbol, handle);

            if (iTo < 0)
                return null;

            var result = json.Substring(handle-1, iTo - handle+2);
            handle = iTo;

            return result;
        }


        private static string ReadKey(this string json, ref int handle)
        {
            handle = json.IndexOf('"', handle);
            if (handle < 0)
                return null;

            handle = json.IndexOf('"', handle) + 1;
            if (handle < 0)
                return null;

            var iTo = json.IndexOf('"', handle);

            if (iTo < 0)
                return null;

            var result = json.Substring(handle, iTo - handle);
            handle = iTo;

            return result;
        }

        private static string ReadSubJson(this string json, char openSymbol, char closeSymbol, ref int handle)
        {
            var insideQuotes = false;

            var level = 0;
            var from = handle;
            int i; 

            for (i = handle; i < json.Length; i++)
            {

                if (json[i] == '"')
                {
                    insideQuotes = !insideQuotes;
                    continue;
                }

                if (insideQuotes)
                {
                    if (json[i] == '\\')
                        handle++;
                    continue;
                }
        
                    
                if (json[i] == openSymbol)
                    level++;
                else
                if (json[i] == closeSymbol)
                    level--;

                if (level == 0)
                    break;

            }

            handle = i;
            return json.Substring(from, handle - from+1);
        }

        private static string ReadNumber(this string json, ref int handle)
        {
            var from = handle;
            var index = json.IndexOfNotAny(handle, '0', '1', '2', '3','4', '5', '6', '7', '8', '9', 't', 'r','u','U','e','f','a','l','L','s', 'n','N');

            if (index < 0)
                return json.Substring(handle, index);

            handle = index;
            return json.Substring(from, index - from);

        }

        private static string ReadValue(this string json, ref int handle)
        {
            handle = json.FindFirstNonSpaceSymbolIndex(handle);
            if (handle < 0)
                return null;

            if (json[handle] == '"')
                return json.ReadBetween('"', ref handle);

            if (json[handle] == '[')
                return json.ReadSubJson('[',']', ref handle);

            if (json[handle] == '{')
                return json.ReadSubJson('{', '}', ref handle);

            return json.ReadNumber(ref handle);
        }


        public static Dictionary<string, string> GetSubJsons(this string json, bool keyLowerKeys = false)
        {
            if (json == null)
                return null;

            var handle = json.FindFirstNonSpaceSymbolIndex();

            if (handle < 0)
                return null;

            if (json[handle] != '{')
                return null;

            var result = new Dictionary<string, string>();
            
            while (true)
            {


                var key = json.ReadKey(ref handle);

                if (key == null)
                    break;

                handle = json.IndexOf(':', handle);
                if (handle <0)
                    break;
                handle++;
                var value = json.ReadValue(ref handle);
                if (value == null)
                    break;

                if (keyLowerKeys)
                    key = key.ToLower();

                result.Add(key, value);

                handle = json.IndexOf(',', handle);
                if (handle < 0)
                    break;

            }



            return result;
        }


        private static bool IsJsonValueNull(this string value)
        {
            return value == "null";
        }

        public static string CleanUpJsonValue(this string value)
        {
            if (value == null)
                return null;

            if (value.Length < 2)
                return value;

            if ((value[0] == '"' || value[0] == '\'') &&
                (value[value.Length - 1] == '"' || value[value.Length - 1] == '\''))
                return value.Substring(1, value.Length - 2);

            return value;
        }
        
        public static T DeserializeJson<T>(this string data){
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }

        
        public static string ToJson(this object value){
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }

    }

}
