using MvvmDialogs;
using System;
using UOP.WinTray.UI.Commands;

namespace UOP.WinTray.UI.ViewModels
{
    /// <summary>
    /// About view model class
    /// </summary>
    public class AboutViewModel : ViewModel_Base, IModalDialogViewModel
    {
        #region Varibles

        static readonly Version app = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private readonly string dllVersion = "{0}.{1}.{2}.{3}";
        protected bool? dialogResult;
        protected DelegateCommand okCommand;

        #endregion

        #region Properties 

        private string version;

        public string Version
        {
            get { return version; }
            set 
            {
                if (version != value)
                {
                    version = value;
                    NotifyPropertyChanged("Version");
                }
            }
        }

        public bool? DialogResult
        {
            get
            {
                return this.dialogResult;
            }
            set
            {
                this.dialogResult = value;
                NotifyPropertyChanged("DialogResult");
            }
        }

        #endregion

        #region Constructor

        public AboutViewModel()
        {
            Version = string.Format(dllVersion,app.Major.ToString() , app.Minor,app.Build,app.Revision);
        }

        #endregion

        #region Method

        /// <summary>
        /// Ok button action to close the popup 
        /// </summary>
        public DelegateCommand OkCommand
        {
            get
            {
                if(okCommand==null)
                {
                    okCommand = new DelegateCommand(param => Ok());
                }
                return okCommand;
            }
        }

        /// <summary>
        /// Close the popup
        /// </summary>
        private void Ok()
        {
            this.DialogResult = true;
        }

        #endregion
    }
}
