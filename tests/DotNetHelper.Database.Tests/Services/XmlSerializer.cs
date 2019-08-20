using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DotNetHelper.Serialization.Abstractions.Interface;

namespace DotNetHelper.Database.Tests.Services
{
    public class DataSourceXML : ISerializer
    {

     

        public DataSourceXML()
        {

        }



        public void SerializeToStream<T>(T obj, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false) where T : class
        {
            throw new NotImplementedException();
        }

        public void SerializeToStream(object obj, Type type, Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false)
        {
            throw new NotImplementedException();
        }

        public Stream SerializeToStream<T>(T obj, int bufferSize = 1024) where T : class
        {
            throw new NotImplementedException();
        }

        public Stream SerializeToStream(object obj, Type type, int bufferSize = 1024)
        {
            throw new NotImplementedException();
        }

        public string SerializeToString(object obj)
        {
            var xmlSerializer = new XmlSerializer(obj.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, obj);
                return textWriter.ToString();
            }
        }

        public string SerializeToString<T>(T obj) where T : class
        {
            var xmlSerializer = new XmlSerializer(obj.GetType());

            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, obj);
                return textWriter.ToString();
            }
        }

        public List<dynamic> DeserializeToList(string content)
        {
            throw new NotImplementedException();
        }

        public List<dynamic> DeserializeToList(Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false)
        {
            throw new NotImplementedException();
        }

        public List<T> DeserializeToList<T>(string content) where T : class
        {
            throw new NotImplementedException();
        }

        public List<T> DeserializeToList<T>(Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false) where T : class
        {
            throw new NotImplementedException();
        }

        public List<object> DeserializeToList(string content, Type type)
        {
            throw new NotImplementedException();
        }

        public dynamic Deserialize(string content)
        {
            throw new NotImplementedException();
        }

        public dynamic Deserialize(Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(string content) where T : class
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(Stream stream, int bufferSize = 1024, bool leaveStreamOpen = false) where T : class
        {
            throw new NotImplementedException();
        }

        public object Deserialize(string content, Type type)
        {
            var ser = new XmlSerializer(type);

            using (var sr = new StringReader(content))
            {
                return ser.Deserialize(sr);
            }
        }

        public object Deserialize(Stream stream, Type type, int bufferSize = 1024, bool leaveStreamOpen = false)
        {
            throw new NotImplementedException();
        }
    }
}
