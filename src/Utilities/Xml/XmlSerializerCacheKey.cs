using System;
using System.Collections.Generic;

namespace UOP.WinTray.UI
{
    public class XmlSerializerCacheKey
    {
        public override bool Equals(object obj)
        {
            bool isEqual = false;
            XmlSerializerCacheKey key = obj as XmlSerializerCacheKey;
            if (null != key)
            {
                if (null != T)
                {
                    isEqual = T.Equals(key.T);
                }
                if (null != ExtraTypes && null != key.ExtraTypes)
                {
                    List<Type> thisTypes = new(ExtraTypes);
                    List<Type> otherTypes = new(key.ExtraTypes);
                    foreach (Type type in thisTypes)
                    {
                        if (!otherTypes.Contains(type))
                        {
                            isEqual = false;
                            break;
                        }
                    }
                }
                else
                {
                    isEqual = null == ExtraTypes && null == key.ExtraTypes;
                }
            }
            return isEqual;
        }

        public override int GetHashCode()
        {
            int hasCode = T.GetHashCode();
            if (null != ExtraTypes)
            {
                foreach (Type type in ExtraTypes)
                {
                    hasCode = hasCode & type.GetHashCode();
                }
            }
            return hasCode;
        }

        public Type T;

        public Type[] ExtraTypes;
    }
}