using UOP.WinTray.UI.CustomControls.UserControls;
using UOP.WinTray.UI.ViewModels;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace UOP.WinTray.UI.Behaviors
{
    [ExcludeFromCodeCoverage]
    //Takes care of executing corresponding save functionality while pressing enter 
    public static class EnterKeyHelpers
    {
        public static ICommand GetEnterKeyCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(EnterKeyCommandProperty);
        }

        public static void SetEnterKeyCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(EnterKeyCommandProperty, value);
        }

        public static readonly DependencyProperty EnterKeyCommandProperty =
            DependencyProperty.RegisterAttached(
                "EnterKeyCommand",
                typeof(ICommand),
                typeof(EnterKeyHelpers),
                new PropertyMetadata(null, OnEnterKeyCommandChanged));

        static void OnEnterKeyCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ICommand command = (ICommand)e.NewValue;
            FrameworkElement fe = (FrameworkElement)target;
            Control control = (Control)target;
            control.KeyDown += (s, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    // make sure the textbox binding updates its source first
                    BindingExpression b = control.GetBindingExpression(TextBox.TextProperty);
                    if (b != null)
                    {
                        b.UpdateSource();
                    }
                    else if (control is NumericTextBoxSpinControl)
                    {
                        var source = target as NumericTextBoxSpinControl;
                        var sourceVM = source.DataContext as Edit_SpoutGroup_ViewModel;
                        var textValue = source.TextBoxValue.Text;
                        sourceVM.SetConstraintWhenLostFocus(source.Name.ToUpper(), ref textValue);
                        source.TextBoxValue.Text = textValue;
                    }         
                    //command.Execute(null);
                }
            };
        }
    }
}
