using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Json
{

    public interface IJsonReader
    {
        JsonObjectBase ReadNext();
    }

    public class JsonObjectBase
    {
        public string Name { get; internal set; }
    }

    public class JsonObjectSimple : JsonObjectBase
    {
        public string Value { get; internal set; }
    }

    public class JsonObjectClass : JsonObjectBase
    {
        public IJsonReader Class { get; internal set; }
    }

    public class JsonArray : JsonObjectBase
    {
        public IJsonReader Array { get; internal set; } 
    }






    public class JsonEngine : IJsonReader
    {

        private readonly List<JsonEngine> _engines = new List<JsonEngine>(255); 

        private readonly string _jsonSrc;

        internal int CurrentPos;

        private readonly char _openTag;
        private readonly char _closeTag;
        private readonly string _objectName;

        public JsonEngine(string objectName, string src, int pos,char openTag, char closeTag, List<JsonEngine> engines )
        {
            _objectName = objectName;
            _engines = engines;
            _openTag = openTag;
            _closeTag = closeTag;
            _jsonSrc = src;

            Init(pos, objectName);

            _engines.Add(this);

        }

        internal int FindNextSpaceOrSymbol(int pos, string symbols)
        {
            for (int i = pos; i < _jsonSrc.Length; i++)
            {
                var b = (byte)_jsonSrc[i];
                if (b <= 32) return i;

                if (symbols.Any(symbol => _jsonSrc[i] == symbol))
                    return i;
            }

            return _jsonSrc.Length;
        }


        internal int FineNextNoSpace(int pos)
        {
            for (int i = pos; i < _jsonSrc.Length; i++)
            {
                var b = (byte)_jsonSrc[i];

                if (b > 32) return i;
            }

            return _jsonSrc.Length;
        }

        internal int FineNextSpace(int pos)
        {
            for (int i = pos; i < _jsonSrc.Length; i++)
            {
                var b = (byte)_jsonSrc[i];

                if (b <= 32) return i;
            }

            return _jsonSrc.Length;

        }


        private void Init(int startPos, string objectName)
        {
            CurrentPos = FineNextNoSpace(startPos);

            if (_jsonSrc[CurrentPos] != _openTag)
                throw new Exception("Error parsing json. " + objectName +
                                    "  element should start with should start from [" + _openTag + "] symbol");

            CurrentPos = FineNextNoSpace(CurrentPos + 1);

            EndOfDocument = (_jsonSrc[CurrentPos] == _closeTag);
        }

        internal void FindNextJsonItem()
        {
            CurrentPos = FineNextNoSpace(CurrentPos);

            if (_jsonSrc[CurrentPos] == _closeTag)
            {
                EndOfDocument = true;
                return;
            }
                
            if (_jsonSrc[CurrentPos] != ',')
                throw new Exception("Fileds should be divided by [,]");

            CurrentPos = FineNextNoSpace(CurrentPos + 1);

        }

        private string ReadEndOfQuoting()
        {
            var posQuoting = _jsonSrc.IndexOf('"', CurrentPos + 1);
            var result = _jsonSrc.Substring(CurrentPos + 1, posQuoting - CurrentPos - 1);
            CurrentPos = posQuoting+1;
            return result;
        }

        private string ReadToEndWithourQuting()
        {
            var i = FindNextSpaceOrSymbol(CurrentPos, "," + _closeTag);
            var value = _jsonSrc.Substring(CurrentPos, i - CurrentPos);

            if (value == "null")
                value = null;

            CurrentPos = i;

            return value;
        }

        private readonly JsonObjectSimple _jsonObjectSimple = new JsonObjectSimple();

        private bool _endOfDocument;

        public JsonObjectBase ReadNext()
        {
            if (EndOfDocument)
                return null;



            string name;

            if (_jsonSrc[CurrentPos] == '{')
            {
                var jsc = new JsonObjectClass
                {
                    Name = null,
                    Class = new JsonEngine("AnonymusClass[" + _objectName + "]", _jsonSrc, CurrentPos, '{', '}', _engines)
                };

                return jsc; 
            }

            if (_jsonSrc[CurrentPos] == '[')
                throw new Exception("Array can not be anonimus. Error in object "+_objectName);


            if (_jsonSrc[CurrentPos] == '"')
                name = ReadEndOfQuoting();
            else
            {
                _jsonObjectSimple.Name = null;
                _jsonObjectSimple.Value = ReadToEndWithourQuting();
                FindNextJsonItem();
                return _jsonObjectSimple;  
            }


            CurrentPos = FineNextNoSpace(CurrentPos);

            if (_jsonSrc[CurrentPos] == ',' || _jsonSrc[CurrentPos] == _closeTag)
            {

                _jsonObjectSimple.Value = name; 
                _jsonObjectSimple.Name = null;
                FindNextJsonItem();
                return _jsonObjectSimple;    
            }

            if (_jsonSrc[CurrentPos] != ':')
                throw new Exception("Error parsing json. Symbol [" + ':' + "] after [" + name + "]");


            CurrentPos = FineNextNoSpace(CurrentPos + 1);

            switch (_jsonSrc[CurrentPos])
            {
                case '"':
                    _jsonObjectSimple.Name = name;
                    _jsonObjectSimple.Value = ReadEndOfQuoting();
                    FindNextJsonItem();
                    return _jsonObjectSimple;

                case '{':
                    var jsc = new JsonObjectClass
                                  {
                                      Name = name,
                                      Class = new JsonEngine(name, _jsonSrc, CurrentPos, '{', '}', _engines)
                                  };

                    return jsc;

                case '[':
                    var jsa = new JsonArray
                    {
                        Name = name,
                        Array = new JsonEngine(name, _jsonSrc, CurrentPos,  '[', ']', _engines)
                    };

                    return jsa;


                default:

                    ReadToEndWithourQuting();

                    _jsonObjectSimple.Name = name;
                    _jsonObjectSimple.Value = null;
                    FindNextJsonItem();
                    return _jsonObjectSimple;

            }


        }

        public bool EndOfDocument
        {
            get { return _endOfDocument; }
            private set
            {
                if (_endOfDocument == value)
                    return;

                if (_endOfDocument)
                    return;

                _endOfDocument = value;
                 
                _engines.RemoveAt(_engines.Count-1);

                if (_engines.Count > 0)
                {
                    _engines[_engines.Count-1].CurrentPos = CurrentPos + 1;
                    _engines[_engines.Count - 1].FindNextJsonItem();
                }

            }
        }
    }


    public class JsonReader : IJsonReader
    {
        private readonly string _jsonSrc;

        private readonly List<JsonEngine> _engines = new List<JsonEngine>(255);

        public JsonReader(string json)
        {
            _jsonSrc = json;
            new JsonEngine("Root", _jsonSrc, 0, '{', '}', _engines);
        }

        public JsonObjectBase ReadNext()
        {

            if (_engines.Count == 0)
                return null;

            var result = _engines[_engines.Count - 1].ReadNext();

            return result;

        }
    }
}
