using System;
using System.Runtime.CompilerServices;

namespace UOP.WinTray.UI.Logger
{
    /// <summary>
    /// Class file for Logging error/warning/information
    /// </summary>
    public interface IApplicationLogger
    {
        /// <summary>
        /// Class file for Logging error/warning/information
        /// </summary>
        void LogDebug(string logText, [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0);
        /// <summary>
        /// Class file for Logging error/warning/information
        /// </summary>
        void LogInfo(string logText, [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0);
        /// <summary>
        /// Class file for Logging error/warning/information
        /// </summary>
        void LogError(string logText, Exception ex = null, [CallerMemberName] string methodName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0);
        /// <summary>
        /// Class file for Logging error/warning/information
        /// </summary>
        void LogError(Exception ex, string logText = "", [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0);
        /// <summary>
        /// Class file for Logging error/warning/information
        /// </summary>
        void LogEntry(LogLevel logLevel = LogLevel.Info, Exception ex = null, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int sourceLineNumber = 0,
            string logText = "", params object[] args);

    }
}
