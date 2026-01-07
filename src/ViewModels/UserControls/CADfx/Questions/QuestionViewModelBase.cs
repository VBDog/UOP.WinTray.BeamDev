using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Web.UI.WebControls;
using System.Windows;
using iText.StyledXmlParser.Jsoup.Nodes;

namespace UOP.WinTray.UI.ViewModels.UserControls.CADfx.Questions
{
    public abstract class QuestionViewModelBase
    {
        private WeakReference<Control> _FocusElement_Ref;

        public Control FocusElement 
        {
            get
            {
                if(_FocusElement_Ref == null) return null;
                if (!_FocusElement_Ref.TryGetTarget(out Control element)) { _FocusElement_Ref = null; return null;  };
                return element;
            }

            set
            {
                Control control = null;
                if (_FocusElement_Ref != null)  _FocusElement_Ref.TryGetTarget(out control);
                
                _FocusElement_Ref = value != null ? new WeakReference<Control>(value) : null;
                if(value != null)
                {
                    if (value is System.Windows.Controls.TextBox)  //.GetType() == typeof(System.Windows.Controls.TextBox))
                    {
                        System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)value;
                        value.GotFocus += GotFocus; 
                    }

                }
                else
                {
                    if(control != null) control.GotFocus -= GotFocus;
                }
            }
            
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            Control element = FocusElement;
            if (element == null) return;
            if (!element.IsEnabled) return;
            try
            {

                element.Focus();
                if (element is System.Windows.Controls.TextBox)  //.GetType() == typeof(System.Windows.Controls.TextBox))
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)element;
                    tb.SelectionStart = 0;
                    tb.SelectAll();
                }
            }
            catch { }
        }

        public void SetFocus()
        {
            Control element = FocusElement;
            if (element == null) return;
            if (!element.IsEnabled) return;
            try
            {

                element.Focus();
                if ( element   is System.Windows.Controls.TextBox)  //.GetType() == typeof(System.Windows.Controls.TextBox))
                {
                    System.Windows.Controls.TextBox tb = (System.Windows.Controls.TextBox)element;
                    tb.SelectionStart = 0;
                    tb.SelectAll();
                }
            }
            catch { }
        }


        
    }
}
