using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.Behaviors
{
    /// <summary>
    /// This is a custom property which helps to set the focus to binded control.
    /// </summary>
    public static class FocusExtension
    {
        public static DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusExtension), new UIPropertyMetadata(false, OnIsFocusedChanged));

        /// <summary>
        /// Get value isFocused.
        /// </summary>
        public static bool GetIsFocused(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(IsFocusedProperty);
        }

        /// <summary>
        /// Set value isFocused.
        /// </summary>
        public static void SetIsFocused(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(IsFocusedProperty, value);
        }

        /// <summary>
        /// Event when isFocused is changed.
        /// </summary>
        public static void OnIsFocusedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            TextBox textBox = dependencyObject as TextBox;
            if (textBox != null && !textBox.IsFocused) textBox.Focus();
        }
    }
}
