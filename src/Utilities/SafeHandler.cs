using System.Runtime.InteropServices;

namespace UOP.WinTray.UI.Utilities
{
    public class SafeHandler
    {
        /// <summary>
        /// Release the com components safely
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comObject"></param>
        public static void ReleaseReference<T>(ref T comObject) where T : class
        {
            T anonymousObject = comObject;
            comObject = default(T);
            if (null != anonymousObject && Marshal.IsComObject(anonymousObject))
            {
                ReleaseReference(anonymousObject);
            }
        }
        
        /// <summary>
        /// keep one COM object reference count
        /// </summary>
        /// <param name="comObject"></param>
        public static void ReleaseReference(object comObject,
            int count = 0)
        {
            int refCount;
            if (Marshal.IsComObject(comObject))
            {
                do
                {
                    refCount = Marshal.ReleaseComObject(comObject);
                }
                while (refCount > count);
            }
        }
        
        /// <summary>
        /// keep at one COM reference and dispose rest of the COM objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comObject"></param>
        public static void ReleaseReferenceHoldOne<T>(ref T comObject) where T : class
        {
            T anonymousObject = comObject;
            comObject = default(T);
            if (null != anonymousObject && Marshal.IsComObject(anonymousObject))
            {
                ReleaseReference(anonymousObject,1);
            }
            comObject = anonymousObject;
        }
    }
}
