using Ch.Hurni.AP_MaJ.Utilities;
using DevExpress.ClipboardSource.SpreadsheetML;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Mapping;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Ch.Hurni.AP_MaJ.Classes
{
    public class ApplicationOptions : INotifyPropertyChanged
    {
        public enum StateEnum { Pending = 0, Processing = 1, Completed = 2, Error = 3, Canceled = 4 }

        public enum TaskTypeEnum { Validation = 0, TempChangeState = 1, PurgeProps = 2, Update = 3, SyncProps = 4, PublishBomBlob = 5, WaitForBomBlob = 6, None = 7 }

        public enum SeverityEnum { Info, Warning, Error }

        public enum ProcessingBehaviourEnum { Stop, FinishTask, Continue }

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

        public bool LogError
        {
            get
            {
                return _logError;
            }
            set
            {
                _logError = value;
                NotifyPropertyChanged();
            }
        }
        private bool _logError = true;

        public bool LogWarning
        {
            get
            {
                return _logWarning;
            }
            set
            {
                _logWarning = value;
                NotifyPropertyChanged();
            }
        }
        private bool _logWarning = true;

        public bool LogInfo
        {
            get
            {
                return _logInfo;
            }
            set
            {
                _logInfo = value;
                NotifyPropertyChanged();
            }
        }
        private bool _logInfo = true;

        [JsonIgnore]
        public List<ProcessingBehaviourEnum> AvailableProcessingBehaviours { get; set; } = new List<ProcessingBehaviourEnum>() { ProcessingBehaviourEnum.Stop, ProcessingBehaviourEnum.FinishTask , ProcessingBehaviourEnum.Continue };

        public ProcessingBehaviourEnum ProcessingBehaviour
        {
            get
            {
                return _processingBehaviour;
            }
            set
            {
                _processingBehaviour = value;
                NotifyPropertyChanged();
            }
        }
        private ProcessingBehaviourEnum _processingBehaviour = ProcessingBehaviourEnum.FinishTask;

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

        [JsonConverter(typeof(JsonStringEncription))]
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

        public ObservableCollection<PropertyFieldMapping> VaultPropertyFieldMappings
        {
            get
            {
                if (_vaultPropertyFieldMappings == null)
                {
                    _vaultPropertyFieldMappings = new ObservableCollection<PropertyFieldMapping>();
                }
                return _vaultPropertyFieldMappings;
            }
            set
            {
                _vaultPropertyFieldMappings = value;
                NotifyPropertyChanged();
            }
        }
        private ObservableCollection<PropertyFieldMapping> _vaultPropertyFieldMappings = null;

        [JsonIgnore]
        public DevExpress.Xpf.Core.ThemedWindow Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
            }
        }
        private DevExpress.Xpf.Core.ThemedWindow _parent = null;
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

        internal void UpdatePropertyMappings(List<PropertyFieldMapping> newPropertyFieldMappings)
        {
            List<string> AddedProperties = new List<string>();
            List<string> DeletedProperties = new List<string>();

            //if (!System.IO.File.Exists(BatchEditorDataBasePath)) Data.SaveToSQLite(BatchEditorDataBasePath);

            bool NeedDbSave = false;

            DeletedProperties = VaultPropertyFieldMappings.Select(x => x.FieldName).Distinct().ToList().Except(newPropertyFieldMappings.Select(x => x.FieldName).Distinct()).ToList();
            foreach(string Field in DeletedProperties)
            {
                NeedDbSave = true;

                DataSetUtility.DeleteColumn(Field, (Parent as MainWindow).ActiveProjectDataBase);
                (Parent as MainWindow).Data.Tables["NewProps"].Columns.Remove(Field);
            }


            AddedProperties = newPropertyFieldMappings.Select(x => x.FieldName).Distinct().ToList().Except(VaultPropertyFieldMappings.Select(x => x.FieldName).Distinct()).ToList();
            foreach (string Field in AddedProperties)
            {
                NeedDbSave = true;

                DataSetUtility.AddNewColumn(Field, typeof(string), (Parent as MainWindow).ActiveProjectDataBase);
                (Parent as MainWindow).Data.Tables["NewProps"].Columns.Add(new DataColumn() { ColumnName = Field, DataType = typeof(string), AllowDBNull = true });
            }

            VaultPropertyFieldMappings = new ObservableCollection<PropertyFieldMapping>(newPropertyFieldMappings);
        }

        #endregion
    }

    [Serializable]
    public class PropertyFieldMapping : INotifyPropertyChanged
    {
        #region Properties
        public string FieldName
        {
            get
            {
                return _fieldName;
            }
            set
            {
                _fieldName = value;
                NotifyPropertyChanged();
            }
        }
        private string _fieldName;

        public string VaultPropertySet
        {
            get
            {
                return _vaultPropertySet;
            }
            set
            {
                _vaultPropertySet = value;
                NotifyPropertyChanged();
            }
        }
        private string _vaultPropertySet;

        public string VaultPropertyDisplayName
        {
            get
            {
                return _vaultPropertyDisplayName;
            }
            set
            {
                _vaultPropertyDisplayName = value;
                NotifyPropertyChanged();
            }
        }
        private string _vaultPropertyDisplayName;

        public string VaultPropertyType
        {
            get
            {
                return _vaultPropertyType;
            }
            set
            {
                _vaultPropertyType = value;
                NotifyPropertyChanged();
            }
        }
        private string _vaultPropertyType;

        [JsonIgnore]
        public bool? IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;

                if (value == true)
                {
                    if (string.IsNullOrWhiteSpace(FieldName))
                    {
                        var normalizedString = _vaultPropertyDisplayName.Normalize(NormalizationForm.FormD);
                        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

                        for (int i = 0; i < normalizedString.Length; i++)
                        {
                            char c = normalizedString[i];
                            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                            {
                                stringBuilder.Append(c);
                            }
                        }

                        FieldName = stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(" ", "");
                    }
                }

                NotifyPropertyChanged();
            }
        }
        private bool? _isSelected = null;

        [JsonIgnore]
        public bool? IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
                NotifyPropertyChanged();
            }
        }
        private bool? _isValid = null;

        [JsonIgnore]
        public bool? IsValidFiledName
        {
            get
            {
                return _isValidFiledName;
            }
            set
            {
                _isValidFiledName = value;
                NotifyPropertyChanged();
            }
        }
        private bool? _isValidFiledName = null;
        #endregion


        #region Constructors
        public PropertyFieldMapping()
        {
        }
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
