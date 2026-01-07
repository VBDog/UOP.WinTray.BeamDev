using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Themes.Resources.Classes
{
    public class SeparatorStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is SeparatorViewModel)
            {
                return (Style)((FrameworkElement)container).FindResource("separatorStyle");
            }

            //if (item is ContextMenuSeparatorViewModel)
            //{
            //    return (Style)((FrameworkElement)container).FindResource("separatorStyle");
            //}

            return base.SelectStyle(item, container);
        }
    }

    //public class ContextMenuSeparatorViewModel : ContextMenuItemViewModel
    //{
    //}
}
