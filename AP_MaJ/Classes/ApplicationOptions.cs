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

        public enum PropertySyncModeEnum { PurgeAndAdd, Purge, Add }


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

        [JsonIgnore]
        public List<PropertySyncModeEnum> PropertySyncModes { get; set; } = new List<PropertySyncModeEnum>() { PropertySyncModeEnum.PurgeAndAdd, PropertySyncModeEnum.Purge, PropertySyncModeEnum.Add };

        public PropertySyncModeEnum FilePropertySyncMode
        {
            get
            {
                return _filePropertySyncMode;
            }
            set
            {
                _filePropertySyncMode = value;
                NotifyPropertyChanged();
            }
        }
        private PropertySyncModeEnum _filePropertySyncMode = PropertySyncModeEnum.PurgeAndAdd;

        public int MaxRetryCount
        {
            get
            {
                return _maxRetryCount;
            }
            set
            {
                _maxRetryCount = value;
                NotifyPropertyChanged();
            }
        }
        private int _maxRetryCount = 3;

        public int FileValidationProcess
        {
            get
            {
                return _fileValidationProcess;
            }
            set
            {
                _fileValidationProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _fileValidationProcess = 1;

        public int FileTempChangeStateProcess
        {
            get
            {
                return _fileTempChangeStateProcess;
            }
            set
            {
                _fileTempChangeStateProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _fileTempChangeStateProcess = 1;

        public int FilePurgePropsProcess
        {
            get
            {
                return _filePurgePropsProcess;
            }
            set
            {
                _filePurgePropsProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _filePurgePropsProcess = 1;

        public int FileUpdateProcess
        {
            get
            {
                return _fileUpdateProcess;
            }
            set
            {
                _fileUpdateProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _fileUpdateProcess = 1;

        public int FilePropSyncProcess
        {
            get
            {
                return _filePropSyncProcess;
            }
            set
            {
                _filePropSyncProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _filePropSyncProcess = 1;

        public PropertySyncModeEnum ItemPropertySyncMode
        {
            get
            {
                return _itemPropertySyncMode;
            }
            set
            {
                _itemPropertySyncMode = value;
                NotifyPropertyChanged();
            }
        }
        private PropertySyncModeEnum _itemPropertySyncMode = PropertySyncModeEnum.PurgeAndAdd;

        public int ItemValidationProcess
        {
            get
            {
                return _itemValidationProcess;
            }
            set
            {
                _itemValidationProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _itemValidationProcess = 1;

        public int ItemTempChangeStateProcess
        {
            get
            {
                return _itemTempChangeStateProcess;
            }
            set
            {
                _itemTempChangeStateProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _itemTempChangeStateProcess = 1;

        public int ItemPurgePropsProcess
        {
            get
            {
                return _itemPurgePropsProcess;
            }
            set
            {
                _itemPurgePropsProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _itemPurgePropsProcess = 1;

        public int ItemUpdateProcess
        {
            get
            {
                return _itemUpdateProcess;
            }
            set
            {
                _itemUpdateProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _itemUpdateProcess = 1;

        public int ItemPropSyncProcess
        {
            get
            {
                return _itemPropSyncProcess;
            }
            set
            {
                _itemPropSyncProcess = value;
                NotifyPropertyChanged();
            }
        }
        private int _itemPropSyncProcess = 1;


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

        public int MaxJobSubmitionCount
        {
            get 
            {
                return _maxJobSubmitionCount;
            }
            set
            {
                _maxJobSubmitionCount = value;
            }
        }
        private int _maxJobSubmitionCount = 5;

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

        public int MaxInventorAppCount
        {
            get
            {
                return _maxInventorAppCount;
            }
            set
            {
                _maxInventorAppCount = value;
            }
        }
        private int _maxInventorAppCount = 3;

        public int MaxInventorFileCount
        {
            get
            {
                return _maxInventorFileCount;
            }
            set
            {
                _maxInventorFileCount = value;
            }
        }
        private int _maxInventorFileCount = 100;

        public int? MaxWaitForInventorInstanceInSeconds
        {
            get
            {
                return _maxWaitForInventorInstanceInSeconds;
            }
            set
            {
                _maxWaitForInventorInstanceInSeconds = value;
            }
        }
        private int? _maxWaitForInventorInstanceInSeconds = null;

        public bool IsInventorVisible
        {
            get
            {
                return _isInventorVisible;
            }
            set
            {
                _isInventorVisible = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isInventorVisible = false;

        public bool IsInventorSilent
        {
            get
            {
                return _isInventorSilent;
            }
            set
            {
                _isInventorSilent = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isInventorSilent = true;

        public bool IsInventorInteractionEnable
        {
            get
            {
                return _isInventorInteractionEnable;
            }
            set
            {
                _isInventorInteractionEnable = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isInventorInteractionEnable = false;

        public string SyncPartNumberValue
        {
            get
            {
                return _syncPartNumberValue;
            }
            set
            {
                _syncPartNumberValue = value;
                NotifyPropertyChanged();
            }
        }
        private string _syncPartNumberValue = "SyncPartNumber";

        public string ClearPropValue
        {
            get
            {
                return _clearPropValue;
            }
            set
            {
                _clearPropValue = value;
                NotifyPropertyChanged();
            }
        }
        private string _clearPropValue = "ClearPropValue";

        public bool IsAnyCadFileAnError
        {
            get
            {
                return _isAnyCadFileAnError;
            }
            set
            {
                _isAnyCadFileAnError = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isAnyCadFileAnError = true;

        public List<object> AllAnyCadFileExt
        {
            get
            {
                return _allAnyCadFileExt;
            }
            set
            {
                _allAnyCadFileExt = value;
            }
        }
        private List<object> _allAnyCadFileExt = new List<object>() { ".stp", ".ste", ".step", ".stpz", ".prt", ".sldprt", ".asm", ".sldasm", ".par", ".psm" };

        public List<object> SelectedAnyCadFileExt
        {
            get
            {
                return _selectedAnyCadFileExt;
            }
            set
            {
                _selectedAnyCadFileExt = value;
                NotifyPropertyChanged();
            }
        }
        private List<object> _selectedAnyCadFileExt = new List<object>() { ".stp", ".step", ".prt", ".sldprt", ".asm", ".sldasm" };

        public int CancelAcquireFileAfter
        {
            get
            {
                return _cancelAcquireFileAfter;
            }
            set
            {
                _cancelAcquireFileAfter = value;
            }
        }
        private int _cancelAcquireFileAfter = 5;

        public string InitialLcsValue
        {
            get
            {
                return _initialLcsValue;
            }
            set
            {
                _initialLcsValue = value;
                NotifyPropertyChanged();
            }
        }
        private string _initialLcsValue = "InitialLcsValue";

        public bool SaveHistory
        {
            get
            {
                return _saveHistory;
            }
            set
            {
                _saveHistory = value;
                NotifyPropertyChanged();
            }
        }
        private bool _saveHistory = true;
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

        public bool MustMatchInventorMaterial
        {
            get
            {
                return _mustMatchInventorMaterial;
            }
            set
            {
                _mustMatchInventorMaterial = value;
                NotifyPropertyChanged();
            }
        }
        private bool _mustMatchInventorMaterial = false;

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
                    FieldName = _vaultPropertyDisplayName;
                    //if (string.IsNullOrWhiteSpace(FieldName))
                    //{
                    //    var normalizedString = _vaultPropertyDisplayName.Normalize(NormalizationForm.FormD);
                    //    var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

                    //    for (int i = 0; i < normalizedString.Length; i++)
                    //    {
                    //        char c = normalizedString[i];
                    //        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    //        if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    //        {
                    //            stringBuilder.Append(c);
                    //        }
                    //    }

                    //    FieldName = stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(" ", "");
                    //}
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
