using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using UOP.WinTray.Projects.Utilities;

namespace UOP.WinTray.UI
{
    /// <summary>
    /// This is base class and will be derived from ViewModels.
    /// The puspose of this class to provide databinding, object resource release management,
    /// error handing and notification service.
    /// </summary>
    public abstract class BindingObject : BindableBase, IDisposable, IDataErrorInfo, INotifyDataErrorInfo
    {
        #region INotifyPropertyChanged

        public override event PropertyChangedEventHandler PropertyChanged = delegate { };
        

        public virtual bool NotifyPropertyChanged(string propertyName, bool? bNoErrors = null)
        {
            
            try
            {
                if (SuppressEvents) return false;
                if (!bNoErrors.HasValue) bNoErrors = !uopUtils.RunningInIDE;
                // Verify if the property is valid
                if (VerifyPropertyName(propertyName, bNoErrors.Value))
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch { return false; }
        }
        public virtual bool NotifyPropertyChanged(System.Reflection.MethodBase aMethod, bool? bNoErrors = null)
        {
            // Verify if the property is valid
            try
            {
                if (aMethod == null || SuppressEvents) return false;
                string propname =  aMethod.Name.Replace("set_", "");
               return  NotifyPropertyChanged(propname, bNoErrors);
             
            }
            catch {return false; }
        }
        public virtual bool NotifyPropertyChanged(PropertyChangedEventArgs property, bool? bNoErrors = null)
        {
           return NotifyPropertyChanged(property.PropertyName, bNoErrors);
        }
        #endregion INotifyPropertyChanged

        #region Properties
        public bool Disposed => disposedValue;

        private bool _SuppressEvents = false;
        public bool SuppressEvents {get => disposedValue || _SuppressEvents; set=> _SuppressEvents = value; }

        #endregion Properties

        #region Common Methods

        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>

        // [DebuggerStepThrough]
        public bool VerifyPropertyName(string strPropertyName, bool bNoErrors = false)
        {
            // Empty string allows to update all the binding so is valid.
            if (string.IsNullOrWhiteSpace(strPropertyName) || SuppressEvents)
            {
                return false;
            }

            // Verify that the property name matches a real,  
            // public, instance property on this object.
            bool propertExists = TypeDescriptor.GetProperties(this)[strPropertyName] != null;
            if (!propertExists)
            {
                string strMsg = $"Invalid property name: { strPropertyName }";

                if (ThrowOnInvalidPropertyName && !bNoErrors && !SuppressEvents)
                {
                    throw new Exception(strMsg);
                }
                if (!bNoErrors ) Debug.Fail(strMsg);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion Common Methods

        #region IDataErrorInfo

        public string Error => "";//throw new NotImplementedException();
            
       
        public string this[string columnName]
        {
            get
            {
                string errorMessage = string.Empty;
                try
                {
                    errorMessage = GetError(columnName);
                }
                catch (AmbiguousMatchException)
                {
                }
                return errorMessage;
            }
        }
        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <param name="strPropertyName">Name of the STR property.</param>
        /// <returns></returns>
        protected virtual string GetError(string aPropertyName)
        {
            return string.Empty;
        }


        #endregion Properties

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    PropertyChanged = null;
                    disposedValue = true;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BindingObject() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public virtual void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            //GC.SuppressFinalize(this);
        }


        #endregion IDisposable Support

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public List<Tuple< string, string>> ErrorCollection { get; set; } = new List<Tuple<string, string>>();


        public bool HasErrors
        {
            get
            {
                return propertyErrors.Count > 0 || ErrorCollection.Count > 0;
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            lock (propertyErrors)
            {
                if (string.IsNullOrEmpty(propertyName)
                   || !propertyErrors.ContainsKey(propertyName))
                    return null;
                return propertyErrors[propertyName];
            }
        }

        #endregion INotifyDataErroInfo

        #region  HelperMethod For INoifyDataErrorInfo

        readonly Dictionary<string, ICollection<CustomErrorType>> propertyErrors = new();
        private void RaiseErrorChanged(string propertyName)
        {
            if (ErrorsChanged != null)
            {
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public virtual bool ValidateProperty(string propertyName, out ICollection<CustomErrorType> validationErrors)
        {
            validationErrors = new List<CustomErrorType>();
            return true;
        }
        public virtual void ValidateProperty([CallerMemberName] string propertyName= null)
        {
            ICollection<CustomErrorType> validationErrors = null;
            bool IsValid = ValidateProperty(propertyName, out validationErrors);
            if (!IsValid)
            {
                /* Update the collection in the dictionary returned by the GetErrors method */
                propertyErrors[propertyName] = validationErrors;
                /* Raise event to tell WPF to execute the GetErrors method */
                RaiseErrorChanged(propertyName);
            }
            else if (propertyErrors.ContainsKey(propertyName))
            {
                /* Remove all errors for this property */
                propertyErrors.Remove(propertyName);
                /* Raise event to tell WPF to execute the GetErrors method */
                RaiseErrorChanged(propertyName);
            }

        }
        #endregion HelperMethod For INoifyDataErrorInfo
    }
}
