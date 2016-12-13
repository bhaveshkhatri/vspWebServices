using System;

namespace VspWS.Common
{
    public static class Utils
    {
        public static DateTime Now()
        {
            return DateTime.Now;
        }

        public static T ParseEnum<T>(string stringValue) where T: struct, IConvertible
        {
            T result;
            if (!Enum.TryParse<T>(stringValue, out result))
            {
                result = new T();
            }
            return result;
        }
    }
}
