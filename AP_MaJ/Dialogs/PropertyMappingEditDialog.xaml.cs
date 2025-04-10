﻿using Ch.Hurni.AP_MaJ.Utilities;
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
using System.Data;
using DevExpress.Mvvm;
using DevExpress.ClipboardSource.SpreadsheetML;
using System.Runtime.CompilerServices;
using DevExpress.Mvvm.Native;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Ch.Hurni.AP_MaJ.Dialogs
{
    /// <summary>
    /// Interaction logic for PropertyMappingEditDialog.xaml
    /// </summary>
    public partial class PropertyMappingEditDialog : ThemedWindow, INotifyPropertyChanged
        {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ObservableCollection<PropertyFieldMapping> Mappings
        {
            get
            {
                return _mappings;
            }
            set
            {
                _mappings = value;
            }
        }
        private ObservableCollection<PropertyFieldMapping> _mappings = null;

        public List<string> AllFieldNames
        {
            get
            {
                return _allFieldNames;
            }
            set
            {
                _allFieldNames = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("CanAutoImport");
            }
        }
        private List<string> _allFieldNames = new List<string>();

        public bool CanAutoImport
        {
            get
            {
                return AllFieldNames.Count > 0;
            }
        }

        public PropertyMappingEditDialog(ObservableCollection<Classes.PropertyFieldMapping> PropertyMappings, VaultConfig VltCongif/*, List<string> excludedFields = null*/)
        {
            Mappings = new ObservableCollection<Classes.PropertyFieldMapping>();

            Mappings.CollectionChanged += Mappings_CollectionChanged;

            foreach (PropertyFieldMapping cfm in PropertyMappings)
            {
                Mappings.Add(new PropertyFieldMapping() { VaultPropertySet = cfm.VaultPropertySet, VaultPropertyDisplayName = cfm.VaultPropertyDisplayName, VaultPropertyType = cfm.VaultPropertyType, MustMatchInventorMaterial = cfm.MustMatchInventorMaterial, FieldName = cfm.FieldName, IsSelected = true, IsValid = false });
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
            //if (e.PropertyName == "FieldName" || e.PropertyName == "IsSelected")
            //{
            //    ValidateFieldName(sender as PropertyFieldMapping);
            //}
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ValidateFieldName(PropertyFieldMapping CurrentItem = null)
        {
            //if (CurrentItem != null && !string.IsNullOrEmpty(CurrentItem.FieldName) && !Regex.IsMatch(CurrentItem.FieldName, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
            //{
            //    CurrentItem.IsValidFiledName = false;
            //}
            //else
            //{
            //    //List<string> Names = Mappings.Where(x => x.IsSelected == true && !string.IsNullOrWhiteSpace(x.FieldName)).Select(x => x.FieldName).ToList();

            //    //foreach (PropertyFieldMapping item in Mappings)
            //    //{
            //    //    if (item.IsSelected == true && string.IsNullOrEmpty(item.FieldName))
            //    //    {
            //    //        item.IsValidFiledName = null;
            //    //        continue;
            //    //    }
            //    //    //item.IsValidFiledName = !SystemNames.Contains(item.FieldName);
            //    //}
            //}
        }

        private void ImportColumn_Click(object sender, RoutedEventArgs e)
        {
            string importFile = string.Empty;
            DataSet ds = new DataSet();

            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Title = "Sélectionner le fichier dont les colonnes doivent être importées.";
            openFileDialog.InitialDirectory = "";
            openFileDialog.AddExtension = true;
            openFileDialog.CheckFileExists = false;
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = ".xlsx";

            openFileDialog.Filter = "Csv file (*.csv)|*.csv|" +
                        "Excel file (*.xlsx)|*.xlsx|" +
                        "Excel file (*.xls)|*.xls";

            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                importFile = openFileDialog.FileName;
            }



            DXSplashScreenViewModel dXSplashScreenViewModel = new DXSplashScreenViewModel()
            {
                Status = "Chargement du fichier..."
            };

            SplashScreenManager.CreateWaitIndicator(dXSplashScreenViewModel).Show(Application.Current.MainWindow, WindowStartupLocation.CenterOwner);

            ds.ReadFromFile(importFile);

            if (ds != null)
            {
                ColumnImportDialog cImport = new ColumnImportDialog(/*ds, */this.Foreground, this.Background, "Import column names", Icon);
                cImport.Data = ds;

                SplashScreenManager.CloseAll();

                cImport.ShowDialog();

                if(cImport != null && cImport.ColumnList != null)
                {
                    AllFieldNames = cImport.ColumnList.Select(x => x.ColumnName).OrderBy(x => x).ToList();
                    if(AllFieldNames != null )
                    {
                        if (AllFieldNames.Contains("EntityType")) AllFieldNames.Remove("EntityType");
                        if (AllFieldNames.Contains("Path")) AllFieldNames.Remove("Path");
                        if (AllFieldNames.Contains("Name")) AllFieldNames.Remove("Name");
                        if (AllFieldNames.Contains("TargetVaultPath")) AllFieldNames.Remove("TargetVaultPath");
                        if (AllFieldNames.Contains("TargetVaultNumSchName")) AllFieldNames.Remove("TargetVaultNumSchName");
                        if (AllFieldNames.Contains("TargetVaultName")) AllFieldNames.Remove("TargetVaultName");
                        if (AllFieldNames.Contains("TempVaultLcsName")) AllFieldNames.Remove("TempVaultLcsName");
                        if (AllFieldNames.Contains("TargetVaultCatName")) AllFieldNames.Remove("TargetVaultCatName");
                        if (AllFieldNames.Contains("TargetVaultLcName")) AllFieldNames.Remove("TargetVaultLcName");
                        if (AllFieldNames.Contains("TargetVaultLcsName")) AllFieldNames.Remove("TargetVaultLcsName");
                        if (AllFieldNames.Contains("TargetVaultRevSchName")) AllFieldNames.Remove("TargetVaultRevSchName");
                        if (AllFieldNames.Contains("TargetVaultRevLabel")) AllFieldNames.Remove("TargetVaultRevLabel");
                    }

                }
                else
                {
                    AllFieldNames.Clear();
                }
            }
            else
            {
                SplashScreenManager.CloseAll();
            }
        }

        private void AutoMappe_Click(object sender, RoutedEventArgs e)
        {
            foreach (PropertyFieldMapping mapping in Mappings.Where(x => x.IsSelected == true))
            {
                mapping.IsSelected = false;
            }

            foreach (PropertyFieldMapping mapping in Mappings.Where(x => x.IsSelected != true))
            {
                if (AllFieldNames.Contains(mapping.VaultPropertyDisplayName)) mapping.IsSelected = true;
            }
        }

        private void DeleteAllMappe_Click(object sender, RoutedEventArgs e)
        {
            foreach (PropertyFieldMapping mapping in Mappings.Where(x => x.IsSelected == true))
            {
                mapping.IsSelected = false;
            }
        }
    }
}
