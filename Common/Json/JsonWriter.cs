using System;
using System.Text;

namespace Common.Json
{

    public interface IJsonWriter : IDisposable
    {
        void Write(string name, string value);
        void Write(string value);

        IJsonWriter WriteArray(string arrayName);
        IJsonWriter WriteClass(string className);

    }
 
    public class JsonWriterEngine : IJsonWriter
    {
        private readonly StringBuilder _sb;
        private readonly char _close;
        private int _itemNo;

        public JsonWriterEngine(string arrayName, StringBuilder sb, char open, char close)
        {
            _sb = sb;
            _close = close;
            WriteOpenInfo(arrayName, open);


        }

        internal void WriteObjectName(string name)
        {

            _sb.Append('"');
            _sb.Append(name);
            _sb.Append("\":");
        }


        private void WriteOpenInfo(string arrayName, char open)
        {
            if (arrayName != null)
                WriteObjectName(arrayName);

            _sb.Append(open);

        }


        private void WriteValue(string value)
        {

            if (value == null)
                _sb.Append("null");
            else
            {
                _sb.Append('"');
                _sb.Append(value);
                _sb.Append('"');
            }   
        }

        public void Write(string name, string value)
        {
            if (_itemNo++ > 0)
                _sb.Append(',');

            WriteObjectName(name);
            WriteValue(value);

        }

        public void Write(string value)
        {
            if (_itemNo++ > 0)
                _sb.Append(',');

            WriteValue(value);

        }

        public IJsonWriter WriteArray(string arrayName)
        {

            if (_itemNo++ > 0)
                _sb.Append(',');


            return new JsonWriterEngine(arrayName, _sb, '[', ']');

        }

        public IJsonWriter WriteClass(string className)
        {
            if (_itemNo++ > 0)
                _sb.Append(',');

            return new JsonWriterEngine(className, _sb, '{', '}');
        }

        public void Dispose()
        {
            _sb.Append(_close);
        }
    }

    public class JsonWriter : IJsonWriter
    {
        private readonly StringBuilder _sb = new StringBuilder();

        private JsonWriterEngine _jsonWriterEngine;

        public JsonWriter()
        {
            Reset();
        }

        public void Reset()
        {
            _sb.Clear();
            _jsonWriterEngine = new JsonWriterEngine(null, _sb, '{', '}');
        }
        
        public void Write(string name, string value)
        {

            _jsonWriterEngine.Write(name, value);
        }

        public void Write(string value)
        {

            _jsonWriterEngine.Write(value);
        }

        public IJsonWriter WriteArray(string arrayName)
        {
            return _jsonWriterEngine.WriteArray(arrayName);
        }

        public IJsonWriter WriteClass(string className)
        {
            return _jsonWriterEngine.WriteClass(className);
        }

        public string Json
        {
            get
            {
                Dispose();
                return _sb.ToString();
            }
        }


        private bool _disposed;
        public void Dispose()
        {
            if (_disposed)
                return;
            _jsonWriterEngine.Dispose();
            _disposed = true;
        }
    }
}
