using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;

namespace Ch.Hurni.AP_MaJ.Classes
{
    public class ApplicationOptions : INotifyPropertyChanged
    {
        public enum StateEnum { Pending = 0, Processing = 1, Completed = 2, Error = 3 }

        public enum TaskTypeEnum { Validation = 0, TempChangeState = 1, PurgeProps = 2, Update = 3, SyncProps = 4, PublishBomBlob = 5, WaitForBomBlob = 6, None = 7 }

        public enum SeverityEnum { Info, Warning, Error }

        #region Properties
        public string ImportedDataFile
        {
            get
            {
                return _importedDataFile;
            }
            set
            {
                _importedDataFile = value;
                NotifyPropertyChanged();
            }
        }
        private string _importedDataFile = string.Empty;

        public int SimultaneousValidationProcess
        {
            get
            {
                return _simultaneousValidationProcess;
            }
            set
            {
                _simultaneousValidationProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _simultaneousValidationProcess = 1;

        public int SimultaneousChangeStateProcess
        {
            get
            {
                return _simultaneousChangeStateProcess;
            }
            set
            {
                _simultaneousChangeStateProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _simultaneousChangeStateProcess = 1;

        public int SimultaneousPurgePropsProcess
        {
            get
            {
                return _simultaneousPurgePropsProcess;
            }
            set
            {
                _simultaneousPurgePropsProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _simultaneousPurgePropsProcess = 1;

        public int SimultaneousUpdateProcess
        {
            get
            {
                return _simultaneousUpdateProcess;
            }
            set
            {
                _simultaneousUpdateProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _simultaneousUpdateProcess = 1;

        public int SimultaneousPropSyncProcess
        {
            get
            {
                return _simultaneousPropSyncProcess;
            }
            set
            {
                _simultaneousPropSyncProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _simultaneousPropSyncProcess = 1;

        public string VaultServer
        {
            get 
            { 
                return _vaultServer; 
            }
            set
            {
                _vaultServer = value;
                NotifyPropertyChanged();
            }
        }
        private string _vaultServer = string.Empty;

        public string VaultName
        {
            get
            {
                return _vaultName;
            }
            set
            {
                _vaultName = value;
                NotifyPropertyChanged();
            }
        }
        private string _vaultName = string.Empty;

        public string VaultUser
        {
            get
            {
                return _vaultuserr;
            }
            set
            {
                _vaultuserr = value;
                NotifyPropertyChanged();
            }
        }
        private string _vaultuserr = string.Empty;

        public string VaultPassword
        {
            get
            {
                return _vaultPassword;
            }
            set
            {
                _vaultPassword = value;
                NotifyPropertyChanged();
            }
        }
        private string _vaultPassword = string.Empty;
        #endregion

        #region Constructors
        public ApplicationOptions() { }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
