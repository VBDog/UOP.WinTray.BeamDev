using UOP.WinTray.Projects;
using UOP.WinTray.Projects.Enums;
using UOP.WinTray.Projects.Utilities;
using UOP.WinTray.UI.CustomControls;
using UOP.WinTray.UI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace UOP.WinTray.UI.ViewModels
{



    public class PropertyControlViewModel : BindingObject
    {

        public delegate void PropertyValueChangeHandler(uopProperty aProperty);
        public event PropertyValueChangeHandler eventPropertyValueChangeHandler;

        public delegate void GotFocusEventHandler(PropertyControlViewModel aProperty);
        public event GotFocusEventHandler eventGotFocusEventHandler;


        public delegate void DoubleClickEventHandler(PropertyControlViewModel aProperty);
        public event DoubleClickEventHandler eventDoubleClickEventHandler;

        public delegate void ChoiceValueChangeEventHandler(PropertyControlViewModel aProperty);
        public event ChoiceValueChangeEventHandler eventChoiceValueChangeEventHandler;

        public delegate void YesNoValueChangeEventHandler(PropertyControlViewModel aProperty);
        public event YesNoValueChangeEventHandler eventYesNoValueChangeEventHandler;


        #region Constructors

        public PropertyControlViewModel()
        {
            Property = new uopProperty("PropertyControl", "Value") { DisplayName = "Caption :" };
        }
        public PropertyControlViewModel(uopProperty aProperty, bool? bReadOnly = null, bool? bFreeForm = null, Visibility? aVisibility = null , PropertyControlContainer aContainer = null)
        {
            if (!bReadOnly.HasValue) bReadOnly = aProperty.Protected;
            Property = aProperty;
            IsReadOnly = bReadOnly.Value;
            if (bFreeForm.HasValue) FreeForm = bFreeForm.Value;
            if (aVisibility.HasValue) Visibility = aVisibility.Value;
            Container = aContainer;
        }


        #endregion Constructors

        #region Properties

        private WeakReference<PropertyControlContainer> _Container;
        public PropertyControlContainer Container 
        { 
            get 
            {
                if (_Container == null) 
                    return null;

                _Container.TryGetTarget( out PropertyControlContainer _rVal);
                if (_rVal == null) _Container = null;
                return _rVal;
            }
            set 
            { 
                if(value == null)
                {
                    _Container = null;
                    return;
                }
                value.RegisterPropertyControl(this);
                _Container = new WeakReference<PropertyControlContainer>(value);
            } 
        }

        private uopProperty _Property;
        public uopProperty Property { get => _Property;
            set
            {
                if (_Property != null) _Property.PropertyChanged -= Property_PropertyChanged;

                value ??= new uopProperty("PropertyControl", "Value") { DisplayName = "Caption" };
                _Property = value;
                _Property.PropertyChanged += Property_PropertyChanged;
                NotifyPropertyChanged("Property");
                IsYesNo = _Property.VariableTypeName == "BOOL";
                if (IsYesNo)
                {
            
                    YesNo = Property.ValueB;
          

                }
                else
                {
                    Visibility_YesNo = Visibility.Collapsed;

                }

            }
        }

        public uppPartTypes PartType => Property != null ? Property.PartType : uppPartTypes.Undefined;
        public string DisplayName { get => Property.DisplayName; set { Property.DisplayName = value; NotifyPropertyChanged("DisplayName"); UpdateInputControl(null, true); } }

        private string _Seperator = " : ";
        public string Seperator { get => _Seperator; set { value ??= ""; _Seperator = value; NotifyPropertyChanged("Seperator"); Visibility_Seperator = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible; } }
        public int DisplayUnitPrecision
        {
            get => Property.DisplayUnitPrecision;
            set { Property.DisplayUnitPrecision = value; NotifyPropertyChanged("DisplayUnitPrecision"); UpdateInputControl( null, true); }
        }

        private string _DisplayUnitLabel = "";
        public string DisplayUnitLabel { get => Property.HasUnits && string.IsNullOrWhiteSpace(_DisplayUnitLabel) ? Property.DisplayUnitLabel : _DisplayUnitLabel; set { value ??= ""; _DisplayUnitLabel = value; NotifyPropertyChanged("DisplayUnitLabel"); } }

        private string _Tag = "";
        public string Tag { get => string.IsNullOrWhiteSpace(_Tag) ? Property.Name : _Tag; set { value ??= ""; _Tag = value; NotifyPropertyChanged("Tag"); } }

        public bool YesNo
        {
            get => Property != null && Property.ValueB; 
            set { if (Property != null) Property.SetValue(value); NotifyPropertyChanged("YesNo"); }
        }
        private bool _IsYesNo = false;
        public bool IsYesNo { 
            get => _IsYesNo; 
            set 
            {   _IsYesNo = value; 
                NotifyPropertyChanged("IsYesNo");
                if (_IsYesNo)
                {
                    
                    Visibility_YesNo = Visibility.Visible;
                }
            } 
        }

        private bool _FreeForm = false;
        public bool FreeForm { get => _FreeForm;
            set
            {
                _FreeForm = value;
                UpdateInputControl( null, true);
                NotifyPropertyChanged("FreeForm");
            }
        }

        private bool _HighlightOnFocus = false;
        public bool HighlightOnFocus
        {
            get => _HighlightOnFocus;
            set
            {
                _HighlightOnFocus = value;
                UpdateInputControl( null, true);
                NotifyPropertyChanged("HighlightOnFocus");
            }
        }
        private string _ToolTip = "";
        public string ToolTip { get => string.IsNullOrWhiteSpace(_ToolTip) ? Property.Name : _ToolTip; set { value ??= ""; _ToolTip = value; NotifyPropertyChanged("ToolTip"); } }

        private bool _ZerosAsNullString = false;
        public bool ZerosAsNullString
        {
            get => _ZerosAsNullString;
            set
            {
                _ZerosAsNullString = value;
                UpdateInputControl( null, true);
                NotifyPropertyChanged("ZerosAsNullString");
            }
        }

        private string _NullValueReplacer = null;
        public string NullValueReplacer { get => _NullValueReplacer; set { value ??= ""; _NullValueReplacer = value; NotifyPropertyChanged("NullValueReplacer"); UpdateInputControl( null, true); } }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility { get => _Visibility; set { _Visibility = value; NotifyPropertyChanged("Visibility"); } }

        private bool _IsEnabled = true;
        public bool IsEnabled { get => _IsEnabled; set { IsReadOnly = !value; } }

        private bool _IsReadOnly = false;
        public bool IsReadOnly { get => _IsReadOnly;
            set
            {
                _IsReadOnly = value;
                UpdateInputControl( null, true);
                NotifyPropertyChanged("IsReadOnly");
                Visibility_Input = value ? Visibility.Collapsed : Visibility.Visible;
                Visibility_ReadOnly = !value ? Visibility.Collapsed : Visibility.Visible;
                _IsEnabled = !value;
                NotifyPropertyChanged("IsEnabled");
            }
        }

        private Visibility _Visibility_ChoiceList = Visibility.Collapsed;
        public Visibility Visibility_ChoiceList { get => _Visibility_ChoiceList; set { _Visibility_ChoiceList = value; NotifyPropertyChanged("Visibility_ChoiceList"); } }

        private Visibility _Visibility_Input = Visibility.Collapsed;
        public Visibility Visibility_Input { get => _IsYesNo ? Visibility.Collapsed : _Visibility_Input; set { _Visibility_Input = value; NotifyPropertyChanged("Visibility_Input");  } }

        private Visibility _Visibility_YesNo = Visibility.Collapsed;
        public Visibility Visibility_YesNo 
            { get => _Visibility_YesNo; 
            set 
            {
                bool newval = _Visibility_YesNo != value;
              
                if(value == Visibility.Visible && newval)
                {
                    Visibility_ChoiceList = Visibility.Collapsed;
                    Visibility_Input = Visibility.Collapsed;
                    Visibility_ReadOnly = Visibility.Collapsed;
                   
                }
                _Visibility_YesNo = value;
                NotifyPropertyChanged("Visibility_YesNo");

            } 
        }



        private Visibility _Visibility_ReadOnly = Visibility.Visible;
        public Visibility Visibility_ReadOnly { get => _Visibility_ReadOnly; set { _Visibility_ReadOnly = value; NotifyPropertyChanged("Visibility_ReadOnly"); } }

        private Visibility _Visibility_Seperator = Visibility.Visible;
        public Visibility Visibility_Seperator { get => _Visibility_Seperator; set { _Visibility_Seperator = value; NotifyPropertyChanged("Visibility_Seperator"); } }

        public uppUnitFamilies DisplayUnits { get => Property.DisplayUnits; set { Property.DisplayUnits = value; } }
        private int _MaxLength = 50;
        public int MaxLength { get => _MaxLength; set { _MaxLength = mzUtils.LimitedValue(value, 0, 50); NotifyPropertyChanged("MaxLength"); UpdateInputControl(null, true); } }

        public int ChoiceCount => _Choices == null ? 0 : _Choices.Count;
        private List<string> _Choices = new();
        public List<string> Choices 
        { get => _Choices; 
            set 
            { 
                value ??= new List<string>(); 
                _Choices = value; 
                NotifyPropertyChanged("Choices");
                if(value.Count > 0)
                {
                    Visibility_Input = Visibility.Collapsed;
                    Visibility_ReadOnly = Visibility.Collapsed;
                    Visibility_ChoiceList = Visibility.Visible;

                }
            } 
        }

        private int _ChoiceIndex = 0;
        public int ChoiceIndex
        {
            get => _ChoiceIndex;
            set
            {
                _ChoiceIndex = value;
                NotifyPropertyChanged("ChoiceIndex");
                if (InputChoiceList != null) UpdateInputControl(aCombo: InputChoiceList);
            }
        }

        private string _ChoiceSelectedValue = "";
        public string ChoiceSelectedValue
        {
            get => DisplayUnitValueString;
            set
            {
                value ??= "";
                string svalue = value.ToString();
                _ChoiceSelectedValue = svalue;
                _ChoiceIndex = Choices.FindIndex(x => string.Compare(x, svalue, true) == 0);
                NotifyPropertyChanged("ChoiceIndex");
                NotifyPropertyChanged("ChoiceSelectedValue");

                DisplayUnitValueString = value;
                //if (InputChoiceList != null) UpdateInputControl(aCombo: InputChoiceList);
            }
        }

        private WeakReference<NumericTextBox> _ReadOnlyBox;
        public NumericTextBox ReadOnlyBox
        {
            get
            {
                if (_ReadOnlyBox == null) return null;
                _ReadOnlyBox.TryGetTarget(out NumericTextBox _rVal);
                if (_rVal == null) _ReadOnlyBox = null;
                return _rVal;
            }
            set
            {

                _ReadOnlyBox = value != null ? new WeakReference<NumericTextBox>(value) : null;
                if (value != null) UpdateInputControl(value);
            }
        }

        private WeakReference<NumericTextBox> _InputBox;
        public NumericTextBox InputBox 
        { 
            get 
            {
                if (_InputBox == null) return null;
                _InputBox.TryGetTarget(out NumericTextBox _rVal);
                if (_rVal == null)  _InputBox = null;
                return _rVal;
            } 
            set 
            {

                _InputBox = value != null ? new WeakReference<NumericTextBox>(value) : null;
                if (value != null) UpdateInputControl(value);
           
            }
        }

        private WeakReference<ComboBox> _InputChoiceList;
        public ComboBox InputChoiceList
        {
            get
            {
                if (_InputChoiceList == null) return null;
                _InputChoiceList.TryGetTarget(out ComboBox _rVal);
                if (_rVal == null) _InputChoiceList = null;
                return _rVal;
            }
            set
            {

                _InputChoiceList = value != null ? new WeakReference<ComboBox>(value) : null;
                if (value != null) UpdateInputControl(aCombo: value);

            }
        }

        private WeakReference<CheckBox> _YesNoCheckBox;
        public CheckBox YesNoCheckBox
        {
            get
            {
                if (_YesNoCheckBox == null) return null;
                _YesNoCheckBox.TryGetTarget(out CheckBox _rVal);
                if (_rVal == null) _YesNoCheckBox = null;
                return _rVal;
            }
            set
            {

                _YesNoCheckBox = value != null ? new WeakReference<CheckBox>(value) : null;
                if (value != null) UpdateInputControl(aCheckBox: value);

            }
        }

        public Control InputControl(PropertyControl myview = null)
        {
            if(myview != null) 
            {
                if (IsYesNo) return myview.PropertyCheckBox;
                if (ChoiceCount > 0) return myview.InputChoiceList;

                if (IsReadOnly) return myview.ReadOnlyBox;
                return myview.InputBox;

            }
            else
            {
                if (IsYesNo) return YesNoCheckBox;
                if (ChoiceCount > 0) return InputChoiceList;

                if (IsReadOnly) return ReadOnlyBox;
                return InputBox;

            }

        }

        public string Name => Property.Name;

        public string DisplayUnitValueString
        {
            get 
            {
                if (Property == null) return "";
                if (Property.IsNullValue) return "";

                if(Property.VariableTypeName == "BOOL")
                {
                    if (Property.ValueB)
                        return "Yes";
                    else
                        return "No";
                }
                if (Property.HasUnits && Property.ValueD == 0 && ZerosAsNullString) return "";
                string _rVal = "";
                if (Property.HasUnits)
                {
                   
                    _rVal = Property.UnitValueString(DisplayUnits, ZerosAsNullString, ZerosAsNullString, bIncludeThousandsSeps: IsReadOnly, bYesNoForBool: IsReadOnly);
                }
                else
                {
                    _rVal = Property.DisplayValueString;
                }
                return _rVal;
            }
            set
            {
                if (Property == null|| IsReadOnly) return;

                if (Property.VariableTypeName == "BOOL")
                {
                   bool bvalue = mzUtils.VarToBoolean(value, Property.ValueB);
                    Property.SetValue(bvalue);
                    return;
                }

                value ??= "";
                value = value.Trim();
                if (Property.HasUnits && string.IsNullOrWhiteSpace(value)) 
                       value = Property.HasNullValue ? Property.NullValue.ToString() :  "0";
                Property.DisplayValueString = value;
            }
        }

        public bool IsChoiceList => Visibility_ChoiceList == Visibility.Visible;
        #endregion Properties

        #region Methods
        private void Property_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_Property.DisplayUnits)) NotifyPropertyChanged("DisplayUnits");
            if (e.PropertyName == nameof(_Property.Value))
            {
                eventPropertyValueChangeHandler?.Invoke(Property);
            }


        }
        
        public void RespondToGotFocus()
        {
            eventGotFocusEventHandler?.Invoke(this);
            if (Container != null) Container.SelectedControlName = Tag;
        }

        public void RespondToDoubleClick()
        {
            if (Container == null)
            {
                eventDoubleClickEventHandler?.Invoke(this);
                return;
            }
            else
            {
                Container.SelectedControlName = Tag;
                Container.RespondToPropertyDoubleClick(this);

            }

        }

        public void RespondToChoiceValueChange()
        {
            eventChoiceValueChangeEventHandler?.Invoke(this);
            if (Container == null) return;
            Container.SelectedControlName = Tag;
            Container.RespondToChoiceValueChange(this);
        }
        public void RespondToYesNoValueChange()
        {
            eventYesNoValueChangeEventHandler?.Invoke(this);
            if (Container == null) return;
            Container.SelectedControlName = Tag;
            Container.RespondToYesNoValueChange(this);
        }
        public override string ToString() => $"PropertyControlViewModel [ {Name} ]";
        public void ExectuteLostFocus()
        {
            NumericTextBox tbox = IsReadOnly ? ReadOnlyBox : InputBox;
            if (tbox != null)
                tbox.ExecuteLostFocus();
            //else
            //{ Console.WriteLine($"Text BOX '{Name}' Not Found"); }
        }
        public void NotifyPropertyChanges()
        {

            try
            {
               
                var props = this.GetType().GetProperties();
                foreach (var item in props)
                {
                    //System.Diagnostics.Debug.Print(item.Name);

                    //if (item.CanWrite)
                    //{
                    //System.Diagnostics.Debug.Print(item.Name);
                    try
                    {
                        NotifyPropertyChanged(item.Name, true);
                    }
                    catch
                    {
                        Console.WriteLine($"{item.Name} CAUSED AN ERROR HERE");
                    }
                    //}

                }
           

            }
            catch { }



        }

        private void UpdateInputControl(NumericTextBox aBox = null, bool bDoLostFocus = false, ComboBox aCombo = null, CheckBox aCheckBox = null)
        {
            aBox ??= IsReadOnly ? ReadOnlyBox : InputBox;
            if (aBox == null) return;
            bool updated = false;
            if(IsYesNo )
            if(aCheckBox == null && IsYesNo && Visibility_YesNo == Visibility.Visible)

                {
                    aCheckBox = YesNoCheckBox;


                }

            if(aCheckBox != null)
            {
                aCheckBox.Tag = Tag;
                if (aCheckBox.IsEnabled != IsEnabled) { updated = true; aCheckBox.IsEnabled = IsEnabled; }
                if(!aCheckBox.IsChecked.HasValue ) { updated = true; aCheckBox.IsChecked = YesNo; }
                if (!aCheckBox.IsChecked != YesNo) { updated = true; aCheckBox.IsChecked = YesNo; }

            }
            if (aCombo != null)
            {
                aCombo.Tag = Tag;
                if (aCombo.IsReadOnly != IsReadOnly) { updated = true; aCombo.IsReadOnly = IsReadOnly; }
                if (aCombo.SelectedIndex != ChoiceIndex) { updated = true; aCombo.SelectedIndex = ChoiceIndex; }
                if (aCombo.IsEnabled != !IsReadOnly) { updated = true; aCombo.IsEnabled = !IsReadOnly; }

            }
            else
            {
                aBox.Tag = Tag;
                aBox.PropertyName = Property.Name;
                if (aBox.IsReadOnly != IsReadOnly) { updated = true; aBox.IsReadOnly = IsReadOnly; }
                if (aBox.FreeForm != FreeForm) { updated = true; aBox.FreeForm = FreeForm; }
                if (aBox.MaxLength != MaxLength) { updated = true; aBox.MaxLength = MaxLength; }
                if (aBox.FreeForm != FreeForm) { updated = true; aBox.FreeForm = FreeForm; }
                if (aBox.ZerosAsNullString != ZerosAsNullString) { updated = true; aBox.ZerosAsNullString = ZerosAsNullString; }
                if (aBox.NullValueReplacer != NullValueReplacer) { updated = true; aBox.NullValueReplacer = NullValueReplacer; }
                if (aBox.PrecisionLimit != DisplayUnitPrecision) { updated = true; aBox.PrecisionLimit = DisplayUnitPrecision; }
                if (bDoLostFocus && updated)
                    aBox.ExecuteLostFocus();
            }

         

        }
        #endregion Methods

       
    }
}
