using UOP.WinTray.Projects.Enums;
using UOP.WinTray.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using UOP.WinTray.Projects;

namespace UOP.WinTray.UI.Views
{
    /// <summary>
    /// Interaction logic for UOPPropertyListView.xaml
    /// </summary>
    public partial class UOPPropertyListView : UserControl
    {

        public UOPPropertyListView()
        {
            InitializeComponent();
            var VM = DataContext as UOPPropertyListViewModel;
            VM.PropertyListView = PropertyList;
            ListFooter = "";
          
        }

        private uppPartTypes _PartType;
        public uppPartTypes PartType
        {
            get => _PartType;
            set
            {
                _PartType = value;
                UOPPropertyListViewModel VM = DataContext as UOPPropertyListViewModel;
                VM.PartType = value;
            }
        }

        public uopProperties Properties
        {
            get
            {
                var VM = DataContext as UOPPropertyListViewModel;
                return VM.Properties;
                //return (uopProperties)GetValue(PropertiesProperty);
            }
            set
            {
                //SetValue(PropertiesProperty, value);
                var VM = DataContext as UOPPropertyListViewModel;
                VM.Properties = value;
            }
        }




        // Using a DependencyProperty as the backing store for Properties.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesProperty = DependencyProperty.Register("Properties", typeof(uopProperties), typeof(UOPPropertyListView), new PropertyMetadata(null));



        public uopProperties PropertySource
        {
            get { return (uopProperties)GetValue(PropertySourceProperty); }
            set 
            { 
                SetValue(PropertySourceProperty, value); 
            }
        }

        // Using a DependencyProperty as the backing store for PropertySouce.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertySourceProperty =
            DependencyProperty.Register("PropertySource", typeof(uopProperties), typeof(UOPPropertyListView), new PropertyMetadata(PropertySourceChangedCallback));

        private static void PropertySourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UOPPropertyListView usercntrl = (UOPPropertyListView)d;
            usercntrl.PropertySource = e.NewValue as uopProperties;
        }


        public string ListCaption
        {
            get => (string)GetValue(ListCaptionProperty);
            set
            {
                SetValue(ListCaptionProperty, value);
                lblCaption.Content = value;
            }

        }

        public static readonly DependencyProperty ListCaptionProperty = DependencyProperty.RegisterAttached(
            name: nameof(ListCaption),
            propertyType: typeof(string),
            ownerType: typeof(UOPPropertyListView),
            new  FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault )
            );

        public string ListFooter
        {
            get => (string)GetValue(ListFooterProperty);
            set
            {
                SetValue(ListFooterProperty, value);
                lblFooter.Content = value;
            }

        }



        public static readonly DependencyProperty ListFooterProperty = DependencyProperty.RegisterAttached(
            name: "ListFooter",
            propertyType: typeof(string),
            ownerType: typeof(UOPPropertyListView),
            new UIPropertyMetadata( defaultValue: "" )
            );

        public string SelectedName =>  (SelectedProperty == null) ? "" : SelectedProperty.Name;

        public string SelectedDisplayName => (SelectedProperty == null) ? "" : SelectedProperty.DisplayName;

        public string SelectedValue =>  (SelectedProperty == null) ? "" : SelectedProperty.DisplayValueStringFormatted; 

        public uopProperty SelectedProperty => (PropertyList == null) ? null : (uopProperty)PropertyList.SelectedItem; 
       

        private void PropertyList_Loaded(object sender, RoutedEventArgs e)
        {
            UOPPropertyListViewModel VM = (UOPPropertyListViewModel)DataContext;
            VM.PropertyListView = PropertyList;
        }

        private void propertyList_Unloaded(object sender, RoutedEventArgs e)
        {
            {
                UOPPropertyListViewModel VM = (UOPPropertyListViewModel)DataContext;
                VM.PropertyListView = null;
            }
        }
    }
}
