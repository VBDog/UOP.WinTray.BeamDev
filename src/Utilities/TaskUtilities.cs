using UOP.WinTray.UI.Logger;
using System;
using System.Threading.Tasks;

namespace UOP.WinTray.UI.Utilities
{
    /// <summary>
    /// Extends functionalities of the type Task
    /// </summary>
    public static class TaskUtilities
    {
        public static async Task FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task.ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                ApplicationLogger.Instance.LogError(ex);
            }
        }
    }
}
