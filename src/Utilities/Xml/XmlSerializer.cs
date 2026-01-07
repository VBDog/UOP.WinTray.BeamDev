
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace UOP.WinTray.UI
{
    [Serializable()]
    public class XmlSerializer<T> where T : class
    {
        private static readonly XmlSerializerCache cache = new();

        /// <summary>
        /// Saves the vessel draft.
        /// </summary>
        /// <param name="serializableObject">
        /// The serializable object.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        public static void Serialize(T serializableObject, string path)
        {
            SerializeToDocumentFormat(serializableObject, null, path);
        }


        /// <summary>
        /// Saves the vessel draft.
        /// </summary>
        /// <param name="serializableObject">
        /// The serializable object.
        /// </param>       
        public static XmlDocument Serialize(T serializableObject)
        {
            //SerializeToDocumentFormat(serializableObject, null, path);

            var xmlDocument = new XmlDocument();
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, serializableObject);
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                xmlDocument.Load(stream);
            }

            return xmlDocument;
        }


        /// <summary>
        /// Saves the vessel draft.
        /// </summary>
        /// <param name="serializableObject">
        /// The serializable object.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>


        public static void Serialize(T serializableObject, string path, SerializationContext context)
        {
            switch (context)
            {
                case SerializationContext.Binary:
                    SaveToBinaryFormat(serializableObject, path);
                    break;

                default:
                    SerializeToDocumentFormat(serializableObject, null, path);
                    break;
            }
        }

        /// <summary>
        /// Saves the vessel draft.
        /// </summary>
        /// <param name="serializableObject">
        /// The serializable object.
        /// </param>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="extraTypes">
        /// The extra types.
        /// </param>

        public static void Serialize(T serializableObject, string path, Type[] extraTypes)
        {
            SerializeToDocumentFormat(serializableObject, extraTypes, path);
        }

        /// <summary>
        /// Loads the vessel draft.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <returns>
        /// </returns>
        public static T Deserialize(string path)
        {
            return LoadFromDocumentFormat(null, path);
        }

        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// </returns>
        public static T Deserialize(string path, SerializationContext context)
        {
            try
            {
                switch (context)
                {
                    case SerializationContext.Binary:
                        return LoadFromBinaryFormat(path);

                    default:
                        return LoadFromDocumentFormat(null, path);
                }
            }
            catch 
            {
            }
            return null;
        }

        /// <summary>
        /// Loads the vessel draft.
        /// </summary>
        /// <param name="path">
        /// The path.
        /// </param>
        /// <param name="extraTypes">
        /// The extra types.
        /// </param>
        /// <returns>
        /// </returns>
        public static T Deserialize(string path, Type[] extraTypes)
        {
            return LoadFromDocumentFormat(extraTypes, path);
        }

        /// <summary>
        /// Creates the file stream.
        /// </summary>
        /// <param name="strPath">The STR path.</param>
        /// <returns></returns>
        private static FileStream CreateFileStream(string strPath)
        {
            return new FileStream(strPath, FileMode.OpenOrCreate);
        }

        /// <summary>
        /// Loads from binary format.
        /// </summary>
        /// <param name="strPath">The STR path.</param>
        /// <returns></returns>
        private static T LoadFromBinaryFormat(string strPath)
        {
            try
            {
                using FileStream fileStream = CreateFileStream(strPath);
                return new BinaryFormatter().Deserialize(fileStream) as T;
            }
            catch (Exception)
            {
            }
            return null;
        }

        /// <summary>
        /// Loads from document format.
        /// </summary>
        /// <param name="extraTypes">The extra types.</param>
        /// <param name="strPath">The STR path.</param>
        /// <returns></returns>
        private static T LoadFromDocumentFormat(Type[] extraTypes, string strPath)
        {
            try
            {
                using TextReader textReader = CreateTextReader(strPath);
                return CreateXmlSerializer(extraTypes).Deserialize(textReader) as T;
            }
            catch 
            {
            }
            return null;
        }

        /// <summary>
        /// Creates the text reader.
        /// </summary>
        /// <param name="strPath">The STR path.</param>
        /// <returns></returns>
        private static TextReader CreateTextReader(string strPath)
        {
            return new StreamReader(strPath);
        }

        /// <summary>
        /// Creates the text writer.
        /// </summary>
        /// <param name="strPath">The STR path.</param>
        /// <returns></returns>
        private static TextWriter CreateTextWriter(string strPath)
        {
            return new StreamWriter(strPath);
        }

        /// <summary>
        /// Creates the XML serializer.
        /// </summary>
        /// <param name="extraTypes">
        /// The extra types.
        /// </param>
        /// <returns>
        /// </returns>
        private static XmlSerializer CreateXmlSerializer(Type[] extraTypes)
        {
            try
            {
                XmlSerializerCacheKey key = new();
                key.T = typeof(T);
                key.ExtraTypes = extraTypes;
                XmlSerializer serializer = cache.GetXmlSerializer(key);
                if (null == serializer)
                {
                    serializer = extraTypes != null ? new XmlSerializer(typeof(T), extraTypes) : new XmlSerializer(typeof(T));
                    cache.AddXmlSerializer(key, serializer);
                }
                return serializer;
            }
            catch 
            {
            }
            return null;
        }

        /// <summary>
        /// Saves to document format.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <param name="extraTypes">The extra types.</param>
        /// <param name="strPath">The STR path.</param>
        private static void SerializeToDocumentFormat(T serializableObject, Type[] extraTypes, string strPath)
        {

            try
            {
                using TextWriter textWriter = CreateTextWriter(strPath);
                CreateXmlSerializer(extraTypes).Serialize(textWriter, serializableObject);
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Saves to binary format.
        /// </summary>
        /// <param name="serializableObject">The serializable object.</param>
        /// <param name="strPath">The STR path.</param>
        private static void SaveToBinaryFormat(T serializableObject, string strPath)
        {
            try
            {
                using FileStream fileStream = CreateFileStream(strPath);
                new BinaryFormatter().Serialize(fileStream, serializableObject);
            }
            catch (Exception)
            {
            }
        }
    }
}