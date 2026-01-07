using Honeywell.UOP.WinTray.BusinessLogic.Adaptors;
using Honeywell.UOP.WinTray.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Honeywell.UOP.WinTray.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Properties.xaml
    /// </summary>
    public partial class RangeProperties : UserControl
    {
        #region Constructor
        public RangeProperties()
        {
            InitializeComponent();
            TrayRangeAda trayRangeAda = new TrayRangeAda();
            DataContext = new TrayRangeViewModel(trayRangeAda);
        }
        #endregion
    }
}
