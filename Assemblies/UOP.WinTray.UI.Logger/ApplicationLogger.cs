using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UOP.WinTray.UI.Logger
{
    /// <summary>
    /// Class file for Logging error/warning/information
    /// </summary>
    public class ApplicationLogger : IApplicationLogger
    {
        private const string LOGGER = "\\AppLogger.config";
        private const string LOGSTRING = " LogText::{0} ||FilePath::{1} ||MethodName::{2} ||SourceLineNumber::{3}";
        #region Private variables

        private static readonly ILog logger = LogManager.GetLogger(typeof(ApplicationLogger));

        private static ApplicationLogger instance = null;
        private static readonly object padlock = new object();
        #endregion

        #region Public Properties
        /// <summary>
        /// Class file for Logging error/warning/information
        /// </summary>
        public static ApplicationLogger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null) instance = new ApplicationLogger();
                   
                    return instance;
                }
            }
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Class file for Logging error/warning/information
        /// </summary>
        public ApplicationLogger()
        {
            FileInfo configFile = new System.IO.FileInfo(AppDomain.CurrentDomain.BaseDirectory + LOGGER);
            log4net.Config.XmlConfigurator.ConfigureAndWatch(configFile);
            var rootAppender = ((Hierarchy)LogManager.GetRepository()).Root.Appenders.OfType<FileAppender>().FirstOrDefault();
            string filename = rootAppender != null ? rootAppender.File : string.Empty;
            if (!string.IsNullOrEmpty(filename))
            {
                var PathName = Path.GetDirectoryName(filename);
                if (!Directory.Exists(PathName))
                    Directory.CreateDirectory(PathName);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Log debug messages
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="methodName"></param>
        /// <param name="filePath"></param>
        /// <param name="sourceLineNumber"></param>
        public void LogDebug(string logText, [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(logText))
            {
                return;
            }
            logText = string.Format(LOGSTRING, logText, filePath, methodName, sourceLineNumber);
            Task.Factory.StartNew(() => logger.Debug(logText), TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// Log information messages
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="methodName"></param>
        /// <param name="filePath"></param>
        /// <param name="sourceLineNumber"></param>
        public void LogInfo(string logText, [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(logText))
            {
                return;
            }
            Task.Factory.StartNew(() => logger.Info(logText), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Log Error messages
        /// </summary>
        /// <param name="logText"></param>
        /// <param name="ex"></param>
        /// <param name="methodName"></param>
        /// <param name="filePath"></param>
        /// <param name="sourceLineNumber"></param>
        public void LogError(string logText, Exception ex = null, [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (ex == null)
            {
                return;
            }
            logText = string.Format(LOGSTRING, logText, filePath, methodName, sourceLineNumber);
            Task.Factory.StartNew(() => logger.Error(logText, ex), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Log Error messages 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logText"></param>
        /// <param name="methodName"></param>
        /// <param name="filePath"></param>
        /// <param name="sourceLineNumber"></param>
        public void LogError(Exception ex, string logText = "", [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (ex == null)
            {
                return;
            }
            logText = string.Format(LOGSTRING, logText, filePath, methodName, sourceLineNumber);
            Task.Factory.StartNew(() => logger.Error(logText, ex), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Log entry
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="ex"></param>
        /// <param name="methodName"></param>
        /// <param name="filePath"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="logText"></param>
        /// <param name="args"></param>
        public void LogEntry(LogLevel logLevel = LogLevel.Info, Exception ex = null,
             [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0,
             string logText = "", params object[] args)
        {
            if (ex == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(logText) && args != null && args.Length > 0 && args.Length <= logText.Count(f => f == '{'))
            {
                logText = string.Format(logText, args);
            }
            Task.Factory.StartNew(() => logger.Error(logText, ex), TaskCreationOptions.LongRunning);
        }

        #endregion
    }
}
