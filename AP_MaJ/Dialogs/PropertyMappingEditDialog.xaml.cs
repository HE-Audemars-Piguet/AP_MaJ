using AP_MaJ.Utilities;
using Ch.Hurni.AP_MaJ.Classes;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Ch.Hurni.AP_MaJ.Dialogs
{
    /// <summary>
    /// Interaction logic for PropertyMappingEditDialog.xaml
    /// </summary>
    public partial class PropertyMappingEditDialog : ThemedWindow
    {
        public ObservableCollection<PropertyFieldMapping> Mappings
        {
            get
            {
                return _mappings;
            }
            set
            {
                _mappings = value;
                //NotifyPropertyChanged();
            }
        }
        private ObservableCollection<PropertyFieldMapping> _mappings = null;

        public PropertyMappingEditDialog(ObservableCollection<Classes.PropertyFieldMapping> PropertyMappings, VaultConfig VltCongif)
        {
            Mappings = new ObservableCollection<Classes.PropertyFieldMapping>();

            Mappings.CollectionChanged += Mappings_CollectionChanged;

            foreach (PropertyFieldMapping cfm in PropertyMappings)
            {
                Mappings.Add(new PropertyFieldMapping() { VaultPropertySet = cfm.VaultPropertySet, VaultPropertyDisplayName = cfm.VaultPropertyDisplayName, VaultPropertyType = cfm.VaultPropertyType, FieldName = cfm.FieldName, IsSelected = true, IsValid = false });
            }
            
            foreach (Autodesk.DataManagement.Client.Framework.Vault.Currency.Properties.PropertyDefinition propDef in VltCongif.VaultFilePropertyDefinitionDictionary.Values)
            {
                if (propDef.Active == false || propDef.IsCalculated == true || propDef.IsSystem == true) continue;

                PropertyFieldMapping mapping = Mappings.Where(x => x.VaultPropertySet == "File" && x.VaultPropertyDisplayName == propDef.DisplayName).FirstOrDefault();

                if (mapping == null)
                {
                    mapping = new PropertyFieldMapping() { VaultPropertySet = "File", VaultPropertyDisplayName = propDef.DisplayName, VaultPropertyType = propDef.DataType.ToString(), IsSelected = false, IsValid = true, FieldName = "" };
                    Mappings.Add(mapping);
                }
                else
                {
                    mapping.IsValid = true;
                }
            }

            foreach (Autodesk.DataManagement.Client.Framework.Vault.Currency.Properties.PropertyDefinition propDef in VltCongif.VaultItemPropertyDefinitionDictionary.Values)
            {
                if (propDef.Active == false || propDef.IsCalculated == true || (propDef.IsSystem == true && !(propDef.SystemName.Equals("Title(Item,CO)") || propDef.SystemName.Equals("Description(Item,CO)")))) continue;

                PropertyFieldMapping mapping = Mappings.Where(x => x.VaultPropertySet == "Item" && x.VaultPropertyDisplayName == propDef.DisplayName).FirstOrDefault();

                if (mapping == null)
                {
                    mapping = new PropertyFieldMapping() { VaultPropertySet = "Item", VaultPropertyDisplayName = propDef.DisplayName, VaultPropertyType = propDef.DataType.ToString(), IsSelected = false, IsValid = true, FieldName = "" };
                    Mappings.Add(mapping);
                }
                else
                {
                    mapping.IsValid = true;
                }
            }

            //ValidateFieldName();

            DataContext = this;

            InitializeComponent();
        }

        private void Mappings_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FieldName" || e.PropertyName == "IsSelected")
            {
                ValidateFieldName(sender as PropertyFieldMapping);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        //private List<string> SystemNames = new List<string>() { "TargetVaultName", "TargetVaultPath", "VaultCatName", "TargetVaultCatName", "TargetVaultLcName", "TempVaultLcsName", "TargetVaultLcsName", "TargetVaultRevSchName", "TargetVaultRevLabel" };
        private void ValidateFieldName(PropertyFieldMapping CurrentItem = null)
        {
            if (CurrentItem != null && !string.IsNullOrEmpty(CurrentItem.FieldName) && !Regex.IsMatch(CurrentItem.FieldName, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                CurrentItem.IsValidFiledName = false;
            }
            else
            {
                //List<string> Names = Mappings.Where(x => x.IsSelected == true && !string.IsNullOrWhiteSpace(x.FieldName)).Select(x => x.FieldName).ToList();

                //foreach (PropertyFieldMapping item in Mappings)
                //{
                //    if (item.IsSelected == true && string.IsNullOrEmpty(item.FieldName))
                //    {
                //        item.IsValidFiledName = null;
                //        continue;
                //    }
                //    //item.IsValidFiledName = !SystemNames.Contains(item.FieldName);
                //}
            }
        }
    }
}
