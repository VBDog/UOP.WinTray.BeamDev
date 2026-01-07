using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UOP.WinTray.UI.CustomControls
{
    public class NumericTextBox : TextBox
    {
        #region Constants

        private const string REG_EXP_REMOVE_SPACE = @"\s+";
        private const string REG_EXP_ALLOW_ONLY_NUMBERS = @"^[0-9]*$";

        #endregion

        public NumericTextBox()
        {
            base.Text = string.Empty;
      
        }

        /// <summary>
        /// Preview Text Event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (IsReadOnly) return;
            TextBox textBox = (TextBox)e.Source;
            
            string newText = textBox.SelectedText == textBox.Text ? e.Text
                : textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength).Insert(textBox.CaretIndex, e.Text);

            if (!string.IsNullOrEmpty(newText) && !FreeForm)
            {
                e.Handled = !IsTextAllowed(newText);
            }
            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// Hanlder for Lost Focus event
        /// </summary>
        /// <param name="e"></param>                               
        protected override void OnLostFocus(RoutedEventArgs e)
        {
           

            var txtBox = e.Source as TextBox;

            if (null != txtBox) 
            {
                ExecuteLostFocus(txtBox);
                e.Source = txtBox;
                //e.Handled = true;
            }
            try { base.OnLostFocus(e); } catch { }
            
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            
            base.OnTextChanged(e);
        }
        public void ExecuteLostFocus(TextBox txtBox = null)
        {

            txtBox ??= this;
          
            if (FreeForm)
            {
                txtBox.Text = txtBox.Text.Trim();
            }
            else
            {
                txtBox.Text = Regex.Replace(txtBox.Text, REG_EXP_REMOVE_SPACE, string.Empty);
                if (string.IsNullOrEmpty(txtBox.Text)) txtBox.Text = "0";
                double.TryParse(txtBox.Text, out double dVal);
                if (dVal == 0)
                {
                    if (ZerosAsNullString)
                    {
                        txtBox.Text = "";
                        if (!string.IsNullOrWhiteSpace(NullValueReplacer))
                            txtBox.Text = NullValueReplacer;
                    }
                }

            }
            if (string.IsNullOrWhiteSpace(txtBox.Text) && !string.IsNullOrWhiteSpace(NullValueReplacer)) txtBox.Text = NullValueReplacer;

            txtBox.SelectionLength = 0;
            txtBox.CaretBrush = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            
          
        }
        /// <summary>
        /// Select all text content on getting forcus
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync((e.Source as TextBox).SelectAll);
            var txtBox = e.Source as TextBox;
            txtBox.CaretBrush = null;

                base.OnGotFocus(e);
            if (!HighlightOnFocus) base.Background = null;
        }
        /// <summary>
        /// Check if user entered any letter which is not number
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsTextAllowed(string text)
        {
            //Match the regex to check decimal precision
            string expr = REG_EXP_ALLOW_ONLY_NUMBERS;
            if (!IsWholeNumber)
            {
               
                expr = @"^[0-9]{0," + PreDecimalLengthLimitStr + "}" + @"(?:\.[0-9]{0," + PrecisionLimitStr + "})?$";
               
            }


            var regEx = new Regex(expr);
            var decimalpoint = regEx.IsMatch(text);
            return regEx.IsMatch(text); 

        }
        private string FormatString
        {
            get
            {
                string prefix = "{0:0" ;
                if (IsWholeNumber) return prefix + "}";
                if (PrecisionLimit <= 0) return prefix + ".0000}";
                if (PrecisionLimit == 1) return prefix + ".0}";
                return prefix + ".0" + new string('#',PrecisionLimit-1) + "}";
            }
        }

        private string PrecisionLimitStr => (PrecisionLimit > 0) ? PrecisionLimit.ToString() : "";
     
        private string PreDecimalLengthLimitStr => (PreDecimalLengthLimit > 0) ? PreDecimalLengthLimit.ToString() : "";
            
        /// <summary>
        /// Precision limit value for text box
        /// </summary>
        public int PrecisionLimit { get => (int)GetValue(PrecisionLimitProperty); set => SetValue(PrecisionLimitProperty, value);  }

        /// <summary>
        /// Precision limit value for text box
        /// </summary>
        public bool IsWholeNumber { get => (bool)GetValue(IsWholeNumberProperty);  set => SetValue(IsWholeNumberProperty, value);  }

        /// <summary>
        /// Pre Decimal length limit value for text box
        /// </summary>
        public int PreDecimalLengthLimit { get => (int)GetValue(PreDecimalLengthLimitProperty); set => SetValue(PreDecimalLengthLimitProperty, value); }

        public override string ToString() => $"NumericTextBox[{PropertyName}]";
        

        public bool ZerosAsNullString
        {
            get { return (bool)GetValue(ZerosAsNullStringProperty); }
            set { SetValue(ZerosAsNullStringProperty, value); }
        }



      
        public string NullValueReplacer
        {
            get { return (string)GetValue(NullValueReplacerProperty); }
            set { SetValue(NullValueReplacerProperty, value); }
        }



        public bool FreeForm
        {
            get { return (bool)GetValue(FreeFormProperty); }
            set { SetValue(FreeFormProperty, value); }
        }


        public bool HighlightOnFocus
        {
            get { return (bool)GetValue(HighlightOnFocusProperty); }
            set { SetValue(HighlightOnFocusProperty, value); }
        }



        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(NumericTextBox), new PropertyMetadata(""));


        // Using a DependencyProperty as the backing store for HighlightOnFocus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightOnFocusProperty =
            DependencyProperty.Register("HighlightOnFocus", typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true));



        // Using a DependencyProperty as the backing store for FreeForm.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FreeFormProperty =
            DependencyProperty.Register("FreeForm", typeof(bool), typeof(NumericTextBox), new PropertyMetadata(false));



        // Using a DependencyProperty as the backing store for NullValueReplacer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NullValueReplacerProperty =
            DependencyProperty.Register("NullValueReplacer", typeof(string), typeof(NumericTextBox), new PropertyMetadata(""));



        // Using a DependencyProperty as the backing store for ZerosAsNullString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZerosAsNullStringProperty =
            DependencyProperty.Register("ZerosAsNullString", typeof(bool), typeof(NumericTextBox));



        // Using a DependencyProperty as the backing store for Pre Decimal length Limit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreDecimalLengthLimitProperty =
            DependencyProperty.Register("PreDecimalLengthLimit", typeof(int), typeof(NumericTextBox));

        // Using a DependencyProperty as the backing store for PrecisionLimit.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrecisionLimitProperty =
            DependencyProperty.Register("PrecisionLimit", typeof(int), typeof(NumericTextBox));
        // Using a DependencyProperty as the backing store for IsWholeNumber.
        public static readonly DependencyProperty IsWholeNumberProperty =
            DependencyProperty.Register("IsWholeNumber", typeof(bool), typeof(NumericTextBox));
    }
}
