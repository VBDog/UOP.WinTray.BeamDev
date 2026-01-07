using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UOP.WinTray.UI.Behaviors
{  
    /// <summary>
    /// This is a custom behavior which helps to handle the mouse double click event
    /// </summary>
    public sealed class HandleDoubleClickBehavior
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command", typeof(ICommand), typeof(HandleDoubleClickBehavior), new PropertyMetadata(default(ICommand), OnComandChanged));

        public static void SetCommand(DependencyObject element, ICommand value)
        {
            element.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject element)
        {
            return (ICommand)element.GetValue(CommandProperty);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter", typeof(object), typeof(HandleDoubleClickBehavior), new PropertyMetadata(default(object)));

        public static void SetCommandParameter(DependencyObject element, object value)
        {
            element.SetValue(CommandParameterProperty, value);
        }

        public static object GetCommandParameter(DependencyObject element)
        {
            return (object)element.GetValue(CommandParameterProperty);
        }

        private static void OnComandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null) return;
            if(d is Control)
            {
                var c = d as Control;
                if (c == null)
                    throw new InvalidOperationException($"can only be attached to {nameof(d)}");
                c.MouseDoubleClick -= OnDoubleClick;
                if (GetCommand(c) != null)
                    c.MouseDoubleClick += OnDoubleClick;
            }
           
        }

        private static void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var d = sender as DependencyObject;
            if (d == null)
                return;
            var command = GetCommand(d);
            if (command == null)
                return;
            var parameter = GetCommandParameter(d);
            if (!command.CanExecute(parameter))
                return;
            command.Execute(parameter);
        }
    }
}
