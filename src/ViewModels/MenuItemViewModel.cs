using UOP.WinTray.UI.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Media;
using Dynamitey;
using System.Windows;

namespace UOP.WinTray.UI
{
    public class MenuItemViewModel : HierarchicalViewModel<MenuItemViewModel>
    {

        #region Contructors

        public MenuItemViewModel() { }
      
        public MenuItemViewModel(string header, string tooltip = "", ICommand command = null, string imagesource = "", bool isenabled = true, object param = null) 
        { Header = header; ToolTip = tooltip; Command = command; ImageSource = imagesource; Enabled = isenabled; CommandParameter = param; }


        #endregion Constructors

        private string _Header;
        public string Header
        {
            get => _Header; 
            set { _Header = value; NotifyPropertyChanged("Header"); }
        }

        private bool _IsNodeEditable;
        public bool IsNodeEditable
        {
            get => _IsNodeEditable; 
            set { _IsNodeEditable = value; NotifyPropertyChanged("IsNodeEditable"); }
        }

        private string _ToolTip;
        public string ToolTip
        {
            get =>  _ToolTip; 
            set { _ToolTip = value; NotifyPropertyChanged("ToolTip"); }
        }

        private string _InputGestureText;
        public string InputGestureText
        {
            get => _InputGestureText; 
            set { _InputGestureText = value; NotifyPropertyChanged("InputGestureText"); }
        }

        private string _ImageSource = "";
        public string ImageSource
        {
            get => _ImageSource; 
            set {
                value ??= string.Empty;
                _ImageSource = value;
                NotifyPropertyChanged("ImageSource");
                Visibility_Icon = string.IsNullOrEmpty(value) ?  Visibility.Collapsed : Visibility.Visible;
            }
        }

        private Visibility _Visibility_Icon;
        public Visibility Visibility_Icon
        {
            get => _Visibility_Icon;
            set { _Visibility_Icon = value; NotifyPropertyChanged("Visibility_Icon"); }
        }

        private Visibility _Visibility_CheckBox;
        public Visibility Visibility_CheckBox
        {
            get => _Visibility_CheckBox;
            set { _Visibility_CheckBox = value; NotifyPropertyChanged("Visibility_CheckBox"); }
        }

        private bool _Enabled = true;
        public bool Enabled
        {
            get => _Enabled; 
            set { _Enabled = value; 
                NotifyPropertyChanged("Enabled"); 
                if (value == false)
                    System.Diagnostics.Debug.WriteLine("Menu '" + Header + "'Disabled");  }
        }

        private bool _StaysOpen = true;
        public bool StaysOpen
        {
            get => _StaysOpen; 
            set { _StaysOpen = value; NotifyPropertyChanged("StaysOpen"); }
        }

        private ICommand _Command;
        public ICommand Command
        {
            get => _Command; 
            set { _Command = value; NotifyPropertyChanged("Command"); }
        }

        private object _CommandParam; 
        public object CommandParameter
        {
            get => _CommandParam; 
            set { _CommandParam = value; NotifyPropertyChanged("CommandParameter"); }
        }

        private System.Windows.Visibility _Visible;
        public System.Windows.Visibility Visible
        {
            get => _Visible; 
            set { _Visible = value; NotifyPropertyChanged("Visible"); }
        }

        private bool _IsCheckable;
        public bool IsCheckable
        {
            get => _IsCheckable; 
            set { _IsCheckable = value; NotifyPropertyChanged("IsCheckable"); }
        }

        private bool _IsChecked;
        public bool IsChecked
        {
            get => _IsChecked; 
            set { _IsChecked = value; NotifyPropertyChanged("IsChecked"); }
        }

        public MenuItemViewModel AddChildMenuItem(string header, ICommand command)
        {
       
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command);
            Children.Add(_rVal);

            return _rVal;
        }
        public string MenuType { get; set; }


        public MenuItemViewModel AddChildMenuItem(string header, ICommand command, string imageSource)
        {
            if (imageSource == null) imageSource = "";
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command, imageSource);
            Children.Add(_rVal);
            return _rVal;
        }
        
        public MenuItemViewModel AddChildMenuItem(string header, ICommand command, string imageSource, bool isenabled)
        {
            if (imageSource == null) imageSource = "";
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command, imageSource, isenabled);
            Children.Add(_rVal);
            return _rVal;

        }


        public MenuItemViewModel AddChildMenuItem(string strHeader, string strToolTip, ICommand command, string imageSource)
        {
            if (imageSource == null) imageSource = "";
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = strHeader.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(strHeader, strToolTip, command, imageSource);
            Children.Add(_rVal);
            return _rVal;
        }


        public MenuItemViewModel AddChildMenuItem(string header, ICommand command, object commandParameter, string imageSource)
        {
            if (imageSource == null) imageSource = "";
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command, imageSource);
            Children.Add(_rVal);
            return _rVal;
        }
       
        public MenuItemViewModel AddChildMenuItem(string header, ICommand command, object commandParameter, string imageSource, bool isenabled)
        {
            if (imageSource == null) imageSource = "";
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command, imageSource, isenabled, commandParameter);
            Children.Add(_rVal);
            return _rVal;
        }


        public MenuItemViewModel AddChildMenuItem(string header, ICommand command, object commandParameter, string imageSource, string inputGestureText)
        {
            if (imageSource == null) imageSource = "";
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command, imageSource) { InputGestureText = inputGestureText, CommandParameter = commandParameter };
            Children.Add(_rVal);
            return _rVal;

        }

        public MenuItemViewModel AddChildViewMenuItem(string header, ICommand command, object commandParameter, string imageSource, string inputGestureText, bool isEnabled)
        {
            if (imageSource == null) imageSource = "";
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command, imageSource, isEnabled) { InputGestureText = inputGestureText, CommandParameter = commandParameter };
            Children.Add(_rVal);
            return _rVal;
        }

        public MenuItemViewModel AddChildViewMenuItem(string header, ICommand command, object commandParameter, string inputGestureText, bool isEnabled, bool isChecked)
        {
            Children ??= new ObservableCollection<HierarchicalViewModel<MenuItemViewModel>>();
            MenuItemViewModel _rVal = header.Equals("separator") ? new SeparatorViewModel() : new MenuItemViewModel(header, header, command, isenabled: isEnabled, param: commandParameter) { IsChecked = isChecked, InputGestureText = inputGestureText };
            Children.Add(_rVal);
            return _rVal;
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(Header) ? $"MenuItem[{Header}]" : "MenuItem[-]";
        }
    }

    public class HierarchicalViewModel<T> : ViewModel_Base
    {
        private static readonly PropertyChangedEventArgs _childrenChanged = new("Children");

        private ObservableCollection<HierarchicalViewModel<T>> _children;

        public ObservableCollection<HierarchicalViewModel<T>> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                NotifyPropertyChanged(_childrenChanged);
            }
        }
    }

    public class SeparatorViewModel : MenuItemViewModel
    {
    }
}
