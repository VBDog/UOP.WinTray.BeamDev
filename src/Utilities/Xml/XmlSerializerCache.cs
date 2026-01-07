using System.Collections.Generic;
using System.Xml.Serialization;

namespace UOP.WinTray.UI
{
    public class XmlSerializerCache
    {
        private static readonly Dictionary<XmlSerializerCacheKey, XmlSerializer> _cache = new();

        public void AddXmlSerializer(XmlSerializerCacheKey t, XmlSerializer serializer)
        {
            if (!_cache.ContainsKey(t))
            {
                _cache.Add(t, serializer);
            }
        }

        public XmlSerializer GetXmlSerializer(XmlSerializerCacheKey t)
        {
            XmlSerializer serializer = null;
            if (_cache.ContainsKey(t))
            {
                serializer = _cache[t];
            }
            return serializer;
        }
    }
}
