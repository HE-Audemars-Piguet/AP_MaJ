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
            //if (!System.IO.File.Exists(BatchEditorDataBasePath)) Data.SaveToSQLite(BatchEditorDataBasePath);

            //for (int i = VaultPropertyFieldMappings.Count - 1; i >= 0; i--)
            //{
            //    PropertyFieldMapping EditMapping = newPropertyFieldMappings.Where(x => x.VaultPropertySet == VaultPropertyFieldMappings[i].VaultPropertySet &&
            //                                                           x.VaultPropertyDisplayName == VaultPropertyFieldMappings[i].VaultPropertyDisplayName).FirstOrDefault();

            //    if (EditMapping == null)
            //    {
            //        //SQLiteUtility.DeleteColumn(VaultPropertyFieldMappings[i].FieldName, BatchEditorDataBasePath);

            //        Data.Tables["Props"].Columns.Remove(VaultPropertyFieldMappings[i].Name);

            //        VaultPropertyFieldMappings.RemoveAt(i);
            //    }
            //    else if (EditMapping.MappingDirection == VaultPropertyFieldMappings[i].MappingDirection && EditMapping.Name != VaultPropertyFieldMappings[i].Name)
            //    {
            //        SQLiteUtility.RenameColumn(VaultPropertyFieldMappings[i].Name, EditMapping.Name, BatchEditorDataBasePath, EditMapping.MappingDirection);
            //        Data.Tables["Props"].Columns[VaultPropertyFieldMappings[i].Name].ColumnName = EditMapping.Name;

            //        VaultPropertyFieldMappings[i].Name = EditMapping.Name;
            //        VaultPropertyFieldMappings[i].Property.PropertyTypeName = EditMapping.Property.PropertyTypeName;

            //        newPropertyFieldMappings.Remove(EditMapping);
            //    }
            //    else
            //    {
            //        newPropertyFieldMappings.Remove(EditMapping);
            //    }
            //}

            //foreach (PropertyFieldMapping fm in newPropertyFieldMappings)
            //{
            //    SQLiteUtility.AddNewColumn(fm.Name, Type.GetType(fm.Property.PropertyTypeName), BatchEditorDataBasePath, fm.MappingDirection);

            //    Data.Tables["Props"].Columns.Add(new DataColumn() { ColumnName = fm.Name, DataType = Type.GetType(fm.Property.PropertyTypeName), AllowDBNull = true });

            //    VaultPropertyFieldMappings.Add(new FieldMapping()
            //    {
            //        Name = fm.Name,
            //        Property = new SourceProperty()
            //        {
            //            PropertySet = fm.Property.PropertySet,
            //            PropertyName = fm.Property.PropertyName,
            //            PropertyTypeName = fm.Property.PropertyTypeName
            //        },
            //        MappingDirection = fm.MappingDirection,
            //    });
            //}


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
