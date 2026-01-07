namespace UOP.WinTray.UI
{
    /// <summary>
    /// provide custom validation message
    /// </summary>
    public class CustomErrorType
    {
        public CustomErrorType(string validationMessage, Severity severity)
        {
            this.ValidationMessage = validationMessage;
            this.Severity = severity;
        }
        public string ValidationMessage { get; private set; }
        public Severity Severity { get; private set; }
    }
    public enum Severity
    {
        WARNING,
        ERROR
    }
}