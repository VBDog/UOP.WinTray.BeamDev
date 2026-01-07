using System;
using System.IO;
using System.Text;

namespace UOP.WinTray.UI.Utilities
{
    /// <summary>
    /// Class which would log exceptions in Current active directory. 
    /// It will read folder name from App.config file
    /// </summary>
    public sealed class Logger
    {

        #region Constant declaration and log File path varriable

        private const string DATE = "DateTime: ";
        private const string CLASS_NAME = "ClassName: ";
        private const string METHOD_NAME = "MethodName: ";
        private const string EXCEPTION = "Exception: ";
        public static string DefaultLocation = string.Empty;
        public static string LogFolderName = string.Empty;
        public static string LogPath = string.Empty;
        private const string LOGFILENAME = "LoggerLog.txt"; // Log file name 
        private const string APPDATA  = "APPDATA";
        private const string LOGFOLDER = "LogFolder";

        #endregion

        /// <summary>
        /// This method will generate a string with error message and will log it to current active location in Folder which is mentioned in App.config file
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="methodName"></param>
        /// <param name="objClassName"></param>
        public static void HandleException(Exception exception, string methodName, object objClassName)
        {
            string strClassName = string.Empty;
            string strMethodName = string.Empty;
            if (objClassName != null)
            {
                strClassName = objClassName.ToString();
            }

            if (!string.IsNullOrEmpty(methodName))
            {
                strMethodName = methodName;
            }

            StringBuilder errorMessage = new();
            errorMessage.Append(DATE + DateTime.Now + Environment.NewLine);
            errorMessage.Append(CLASS_NAME + strClassName + Environment.NewLine);
            errorMessage.Append(METHOD_NAME + strMethodName + Environment.NewLine);
            if (exception != null)
            {
                errorMessage.Append(EXCEPTION + exception.Message + Environment.NewLine);
                errorMessage.Append("\t " + exception.ToString() + Environment.NewLine);
            }
            WriteInfo(errorMessage.ToString());
        }

        /// <summary>
        /// Log the error
        /// </summary>
        /// <param name="errorMessage"></param>
        public static void WriteInfo(string errorMessage)
        {
            StreamWriter streamWriter = null;
            try
            {
                System.IO.Directory.CreateDirectory(LogPath);
                streamWriter = new StreamWriter(LogPath+"\\"+LOGFILENAME, true);
                streamWriter.WriteLine(errorMessage);      
            }
            catch (ObjectDisposedException exception) { WriteInfo(errorMessage+" "+exception.ToString()); }
            catch(Exception exception) { WriteInfo(errorMessage+" "+ exception.ToString()); }
            finally { streamWriter.Flush(); streamWriter.Close();}
        }

        /// <summary>
        /// Set default Location to create log file
        /// </summary>
        public static void SetDefaultLocation()
        {
             DefaultLocation=System.Environment.GetEnvironmentVariable(APPDATA);
             LogPath = DefaultLocation + "\\" + System.Configuration.ConfigurationManager.AppSettings[LOGFOLDER];
        }
    }
}
