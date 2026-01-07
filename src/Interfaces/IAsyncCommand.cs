using System.Threading.Tasks;
using System.Windows.Input;

namespace UOP.WinTray.UI.Interfaces
{
    /// <summary>
    ///  Interface for IAsyncCommand
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();
        bool CanExecute();
    }
}
