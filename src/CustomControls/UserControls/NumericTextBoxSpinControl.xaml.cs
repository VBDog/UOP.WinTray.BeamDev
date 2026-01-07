using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UOP.WinTray.UI.CustomControls.UserControls
{
    /// <summary>
    /// Interaction logic for NumericTextBoxSpinControl.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class NumericTextBoxSpinControl : UserControl
    {
        private readonly Regex _numMatch;

        /// <summary>Initializes a new instance of the NumericTextBoxSpinControl class.</summary>
        public NumericTextBoxSpinControl()
        {
            InitializeComponent();

            _numMatch = new Regex(@"^[0-9]*(?:\.[0-9]{0,5})?$");
            Maximum = int.MaxValue;
            Minimum = 0;
            TextBoxValue.Text = "0";
        }

        /// <summary>
        /// Resets the text
        /// </summary>
        /// <param name="tb"></param>
        private void ResetText(TextBox tb)
        {
            tb.Text = 0 < Minimum ? Minimum.ToString() : "0";

            tb.SelectAll();
        }

        /// <summary>
        /// Validates Input text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void value_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var text = tb.Text.Insert(tb.CaretIndex, e.Text);

            e.Handled = !_numMatch.IsMatch(text);
        }        

        /// <summary>
        /// Handler when up arrow is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
        }

        /// <summary>
        /// Handler when down arrow is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
        }

        /// <summary>The Value property represents the TextBoxValue of the control.</summary>
        /// <returns>The current TextBoxValue of the control</returns>      
        public double? Value
        {
            get
            {

                return (double?)GetValue(ValueProperty);
            }
            set
            {
               // TextBoxValue.Text = value.ToString();
                SetValue(ValueProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double?), typeof(NumericTextBoxSpinControl),
              new PropertyMetadata(0.0, new PropertyChangedCallback(OnSomeValuePropertyChanged)));

        private static void OnSomeValuePropertyChanged(
        DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NumericTextBoxSpinControl numericBox = target as NumericTextBoxSpinControl;
            numericBox.TextBoxValue.Text = e.NewValue == null ? string.Empty: e.NewValue.ToString();
        }

        /// <summary>
        /// Maximum value for the Numeric Up Down control
        /// </summary>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumericTextBoxSpinControl), new UIPropertyMetadata(100));

        /// <summary>
        /// Minimum value of the numeric up down conrol.
        /// </summary>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumericTextBoxSpinControl), new UIPropertyMetadata(0));
        public bool IsFocus
        {
            get { return (bool)GetValue(IsFocusProp); }
            set { SetValue(IsFocusProp, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFocusProp =
            DependencyProperty.Register("IsFocus", typeof(bool), typeof(NumericTextBoxSpinControl), new UIPropertyMetadata(false));

        /// <summary>
        /// Step Count value of the numeric up down conrol.
        /// </summary>
        public double StepCount
        {
            get { return (double)GetValue(StepCountProperty); }
            set { SetValue(StepCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StepCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StepCountProperty =
            DependencyProperty.Register("StepCount", typeof(double), typeof(NumericTextBoxSpinControl), new UIPropertyMetadata(0.1));

        // Value changed
        private static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NumericTextBoxSpinControl));

        /// <summary>The ValueChanged event is called when the TextBoxValue of the control changes.</summary>
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        //Increase button clicked
        private static readonly RoutedEvent IncreaseClickedEvent =
            EventManager.RegisterRoutedEvent("IncreaseClicked", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NumericTextBoxSpinControl));

        /// <summary>The IncreaseClicked event is called when the Increase button clicked</summary>
        public event RoutedEventHandler IncreaseClicked
        {
            add { AddHandler(IncreaseClickedEvent, value); }
            remove { RemoveHandler(IncreaseClickedEvent, value); }
        }

        //Increase button clicked
        private static readonly RoutedEvent DecreaseClickedEvent =
            EventManager.RegisterRoutedEvent("DecreaseClicked", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NumericTextBoxSpinControl));

        /// <summary>The DecreaseClicked event is called when the Decrease button clicked</summary>
        public event RoutedEventHandler DecreaseClicked
        {
            add { AddHandler(DecreaseClickedEvent, value); }
            remove { RemoveHandler(DecreaseClickedEvent, value); }
        }


        //TextBox Lost Focus
        private static readonly RoutedEvent TextBoxLostFocusEvent =
            EventManager.RegisterRoutedEvent("TextBoxLostFocus", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NumericTextBoxSpinControl));

        /// <summary>The IncreaseClicked event is called when the Increase button clicked</summary>
        public event RoutedEventHandler TextBoxLostFocus
        {
            add { AddHandler(TextBoxLostFocusEvent, value); }
            remove { RemoveHandler(TextBoxLostFocusEvent, value); }
        }

        public bool IsTextEnabled
        {
            get { return (bool)GetValue(IsTextEnabledProperty); }
            set { SetValue(IsTextEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsTextEnabledProperty =
            DependencyProperty.Register(
                "IsTextEnabled",
                typeof(bool),
                typeof(NumericTextBoxSpinControl),
                new PropertyMetadata(true));

        /// <summary>
        /// Checking for Up and Down events and updating the value accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void value_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.Up && Value < Maximum)
            {
                Value = Value + StepCount;
                RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
            }
            else if (e.IsDown && e.Key == Key.Down && Value > Minimum)
            {
                Value = Value - StepCount;
                RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
            }
        }

        /// <summary>
        /// Handle fired when text box loses focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxValue_LostFocus(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(TextBoxLostFocusEvent));
        }
        private void Got_Focus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}
