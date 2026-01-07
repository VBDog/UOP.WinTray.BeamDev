using MvvmDialogs;
using UOP.WinTray.UI.Events.EventAggregator;

namespace UOP.WinTray.UI.ViewModels
{
    public class UOPPropertyGridViewModel : ViewModel_Base 
    {

        internal UOPPropertyGridViewModel(IEventAggregator eventAggregator, IDialogService dialogService) : base(eventAggregator: eventAggregator, dialogService:dialogService){ }



    }
}
