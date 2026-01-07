using UOP.WinTray.UI.ViewModels;
using System;
using System.Windows;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// Auto wireup viewmodel to the view and also setup DataContext property.
    /// 1. Auto wireup will happen, if
    ///     A. Naming conversions of View and ViewModel are XXXView and XXXViewModel
    ///     B. View and ViewModel should be in the namespaces UOP.WinTray.UI.Views
    ///     and UOP.WinTray.UI.ViewModels respectively.
    ///     C. Use below line at View's xaml code
    ///        xmlns:autolocator ="clr-namespace:UOP.WinTray.UI"
    ///        autolocator:AutomaticViewModelLocator.IsAutomaticLocator="True"  
    /// 2. Otherwise need specify ViewModel type explicitly as given below
    ///      xmlns:autolocator ="clr-namespace:UOP.WinTray.UI"
    ///      autolocator:AutomaticViewModelLocator.IsAutomaticLocator="True" 
    ///      autolocator:AutomaticViewModelLocator.ViewModelClassName="Fully Qualified ViewModel name"    
    /// </summary>
    public class ViewModelLocator
    {
        #region Constructor

        public ViewModelLocator() { }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Dependency Get property for auto view model locator
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetIsAutomaticLocator(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAutomaticLocatorProperty);
        }

        /// <summary>
        /// Dependency Set property for auto view model locator
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetIsAutomaticLocator(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAutomaticLocatorProperty, value);
        }

        /// <summary>
        /// Dependency property for auto view model locator
        /// </summary>
        public static readonly DependencyProperty IsAutomaticLocatorProperty =
            DependencyProperty.RegisterAttached("IsAutomaticLocator", typeof(bool), typeof(ViewModelLocator), new PropertyMetadata(false, IsAutomaticLocatorChanged));

        /// <summary>
        /// Dependency Get property for explicilty specifying the view model name
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetViewModelClassName(DependencyObject obj)
        {
            return (string)obj.GetValue(ViewModelClassNameProperty);
        }

        /// <summary>
        /// Dependency Set property for explicilty specifying the view model name
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetViewModelClassName(DependencyObject obj, string value)
        {
            obj.SetValue(ViewModelClassNameProperty, value);
        }

        /// <summary>
        /// Dependency property for explicilty specifying the view model name
        /// </summary>
        public static readonly DependencyProperty ViewModelClassNameProperty =
            DependencyProperty.RegisterAttached("ViewModelClassName", typeof(string), typeof(ViewModelLocator), new PropertyMetadata(null));

        #endregion

        #region Private Methods

        /// <summary>
        /// Create the instance based on dependencyPropertyType and class name
        /// </summary>
        /// <param name="dependencyPropertyType"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private static object GetInstanceOf(Type dependencyPropertyType, string className)
        {
            var classNameDef = GetClassName(dependencyPropertyType, className);
            Type userControlType = Type.GetType(classNameDef);

            if (userControlType == null)
                throw new ArgumentException($"Not exist a type {classNameDef} in the asembly {dependencyPropertyType.Assembly.FullName}");

            var resultado = ApplicationModule.Instance.Resolve(userControlType);

            return resultado;
        }       

        /// <summary>
        /// Creates an instance of specified view model type and assign it to view's DataContext property
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void IsAutomaticLocatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var callOwner = d as FrameworkElement;
            var className = GetViewModelClassName(d);
            var userControl = GetInstanceOf(callOwner.GetType(), className);
            callOwner.DataContext = userControl;

            // Added by CADfx
            if (userControl is ViewModels.MDProjectViewModel)
            {
                WinTrayMainViewModel.WinTrayMainViewModelObj.MDProjectViewModel = userControl as ViewModels.MDProjectViewModel;
            }
            // Added by CADfx
        }

        /// <summary>
        /// Get the fully qualified view model's name
        /// </summary>
        /// <param name="dependencyPropertyType"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private static string GetClassName(Type dependencyPropertyType, string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                return dependencyPropertyType.FullName.Replace("View", "ViewModel");
            }

            return className;
        }        

        #endregion 
    }
}
