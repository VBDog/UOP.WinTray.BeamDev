using System.Windows;
using System.Windows.Controls;


namespace UOP.WinTray.UI.CustomControls
{
    /// <summary>
    /// Class that is derived from TextBox control to provide select all text on focus functionality fron non numeric textbox control
    /// </summary>
    public class CustomTextBox : TextBox
    {
        /// <summary>
        /// Select all text content on getting forcus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Application.Current?.Dispatcher.InvokeAsync((e.Source as TextBox).SelectAll);
            base.OnGotFocus(e);
        }
    }
}
