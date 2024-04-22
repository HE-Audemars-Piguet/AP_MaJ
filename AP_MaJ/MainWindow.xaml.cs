using AP_MaJ;
using Ch.Hurni.AP_MaJ.Classes;
using Ch.Hurni.AP_MaJ.Dialogs;
using Ch.Hurni.AP_MaJ.Utilities;
using CH.Hurni.AP_MaJ.Dialogs;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.UI;
using DevExpress.Utils.CommonDialogs;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Editors.Settings;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Layout.Core;
using DevExpress.XtraPrinting.Native;
using Inventor;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Ch.Hurni.AP_MaJ.Classes.ApplicationOptions;

namespace Ch.Hurni.AP_MaJ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow, INotifyPropertyChanged
    {
        private string _rootProjectDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

        private JsonSerializerOptions JsonOptions = new JsonSerializerOptions() { WriteIndented = true };

        #region Properties
        public string ActiveProjectName
        {
            get
            {
                return _activeProjectName;
            }
            set
            {
                if(!System.IO.File.Exists(value))
                {
                    System.IO.File.WriteAllText(value, JsonSerializer.Serialize(new ApplicationOptions(), typeof(ApplicationOptions), JsonOptions));
                }

                _activeProjectName = value;

                OpenProject();

                NotifyPropertyChanged();
            } 
        }
        private string _activeProjectName = string.Empty;

        public string ApplicationDir
        {
            get
            {
                string _applicationDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                return _applicationDirectory;
            }
        }


        public string ActiveProjectDir
        {
            get
            {
                string fileExt = System.IO.Path.GetExtension(ActiveProjectName);
                string dirName = ActiveProjectName.Substring(0, ActiveProjectName.Length - fileExt.Length);

                if (!string.IsNullOrWhiteSpace(dirName) && !System.IO.Directory.Exists(dirName))
                {
                    System.IO.Directory.CreateDirectory(dirName);
                }

                return dirName;
            }
        }

        public string ActiveProjectDataBase
        {
            get
            {
                return System.IO.Path.Combine(ActiveProjectDir, "DataFile.db");
            }
        }

        public string ActiveProjectGridViewDir
        {
            get
            {
                string dirName = System.IO.Path.Combine(ActiveProjectDir, "GridViews");

                if (!string.IsNullOrWhiteSpace(dirName) && !System.IO.Directory.Exists(dirName))
                {
                    System.IO.Directory.CreateDirectory(dirName);
                }

                return dirName;
            }

        }

        public ObservableCollection<string> GridViewList
        {
            get
            {
                return _gridViewList;
            }
            set
            {
                _gridViewList = value;
                NotifyPropertyChanged();
            }
        }
        private ObservableCollection<string> _gridViewList = new ObservableCollection<string>();

        public string ActiveGridView
        {
            get
            {
                return _activeGridView;
            }
            set
            {
                _activeGridView = value;

                string GidViewFile = System.IO.Path.Combine(ActiveProjectGridViewDir, _activeGridView + ".xml");
                if (System.IO.File.Exists(GidViewFile))
                {
                    MainGridControl.RestoreLayoutFromXml(GidViewFile);
                }

                NotifyPropertyChanged();
            } 
        }
        private string _activeGridView = "Default";

        public List<GridColumn> GridColumList
        {
            get
            {
                if (_gridColumnList == null)
                {
                    _gridColumnList = new List<GridColumn>();

                    GridColumn newGridCol;

                    foreach (DataColumn dc in Data.Tables["Entities"].Columns)
                    {
                        if (dc.ColumnName.Equals("Id")) continue;

                        newGridCol = new GridColumn() { Header = dc.ColumnName, FieldName = "Entity_" + dc.ColumnName, Visible = false };

                        if (dc.ColumnName.Equals("Ext"))
                        {
                            newGridCol.Tag = dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.String;
                            newGridCol.FixedWidth = true;
                            newGridCol.Width = new GridColumnWidth(20, GridColumnUnitType.Pixel);
                            //newGridCol.CellTemplate = (currentCtrl.MainGridControl.FindResource("ExtTemplate") as DataTemplate);
                        }
                        else if (dc.ColumnName.Equals("Size"))
                        {
                            newGridCol.Tag = dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                            newGridCol.EditSettings = new TextEditSettings() { DisplayFormat = "{0:N0} Ko", MaskUseAsDisplayFormat = true };
                        }
                        else if (dc.DataType == typeof(DateTime))
                        {
                            newGridCol.Tag = dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.DateTime;
                            newGridCol.EditSettings = new TextEditSettings() { DisplayFormat = "dd.MM.yyyy HH:mm:ss", MaskUseAsDisplayFormat = true };
                        }
                        else if (dc.DataType.IsEnum)
                        {
                            newGridCol.Tag = "[" + dc.DataType.Name + "]" + dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.String;
                        }
                        else if (dc.DataType == typeof(bool))
                        {
                            newGridCol.Tag = dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                        }
                        else if (dc.DataType == typeof(int) || dc.DataType == typeof(double))
                        {
                            newGridCol.Tag = dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                        }
                        else
                        {
                            newGridCol.Tag = dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.String;
                            newGridCol.SortMode = DevExpress.XtraGrid.ColumnSortMode.Custom;
                        }

                        _gridColumnList.Add(newGridCol);
                    }

                    foreach (DataColumn dc in Data.Tables["NewProps"].Columns)
                    {
                        if (dc.ColumnName.Equals("EntityId")) continue;

                        newGridCol = new GridColumn() { Header = dc.ColumnName + " (User property)", FieldName = "NewProp_" + dc.ColumnName, Visible = false };

                        if (dc.DataType == typeof(DateTime))
                        {
                            newGridCol.Tag = dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.DateTime;
                            newGridCol.EditSettings = new TextEditSettings() { DisplayFormat = "dd.MM.yyyy HH:mm:ss", MaskUseAsDisplayFormat = true };
                        }
                        else if (dc.DataType.IsEnum)
                        {
                            newGridCol.Tag = "[" + dc.DataType.Name + "]" + "GetChildRows|EntityNewProp|" + dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.String;
                        }
                        else if (dc.DataType == typeof(bool))
                        {
                            newGridCol.Tag = "GetChildRows|EntityNewProp|" + dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
                        }
                        else if (dc.DataType == typeof(int) || dc.DataType == typeof(double))
                        {
                            newGridCol.Tag = "GetChildRows|EntityNewProp|" + dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.Decimal;
                        }
                        else
                        {
                            newGridCol.Tag = "GetChildRows|EntityNewProp|" + dc.ColumnName;
                            newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.String;
                        }

                        _gridColumnList.Add(newGridCol);
                    }
                }
                return _gridColumnList;
            }
            set
            {
                _gridColumnList = value;
            }
        }
        private List<GridColumn> _gridColumnList = null;

        public bool IsWaitIndicatorVisible
        {
            get
            {
                return _isWaitIndicatorVisible;
            }
            set
            {
                _isWaitIndicatorVisible = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isWaitIndicatorVisible = false;

        public DataSet Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                NotifyPropertyChanged();
            } 
        }
        private DataSet _data = null;

        public ApplicationOptions AppOptions
        {
            get 
            { 
                return _appOptions; 
            }
            set
            {
                _appOptions = value;
                NotifyPropertyChanged();
            }
        }
        private ApplicationOptions _appOptions = null;


        #endregion


        #region Constructors
        public MainWindow()
        {
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            DataContext = this;
            
            InitializeComponent();
        }
        #endregion


        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MainGridControl_CustomUnboundColumnData(object sender, DevExpress.Xpf.Grid.GridColumnDataEventArgs e)
        {
            if (e.IsGetData)
            {
                if (e.Column.Tag != null)
                {
                    string Tag = e.Column.Tag.ToString();
                    string EnumType = string.Empty;
                    List<string> Ref = new List<string>();

                    int index = Tag.IndexOf("]");

                    if (index > -1)
                    {
                        EnumType = Tag.Substring(1, index - 1);
                        Tag = Tag.Substring(index + 1);
                    }

                    Ref = Tag.Split('|').ToList();

                    DataRow dr = (((GridControl)sender).GetRowByListIndex(e.ListSourceRowIndex) as DataRowView).Row as DataRow;

                    if (Ref.Count == 1)
                    {
                        if (!string.IsNullOrEmpty(EnumType)) e.Value = ConvertToEnum(dr[Ref[0]], EnumType);
                        else if (Ref[0].Equals("Name_LC")) e.Value = dr["Name"].ToString().ToLower();
                        else e.Value = dr[Ref[0]];
                    }
                    else if (Ref.Count == 3)
                    {
                        if (Ref[0].Equals("GetChildRows") && dr.GetChildRows(Ref[1]).Count() > 0)
                        {
                            if (string.IsNullOrEmpty(EnumType)) e.Value = dr.GetChildRows(Ref[1]).FirstOrDefault()[Ref[2]];
                            else e.Value = ConvertToEnum(dr.GetChildRows(Ref[1]).FirstOrDefault()[Ref[2]], EnumType);
                        }
                    }
                }
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ActiveProjectDir))
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine("C:\\Temp", "Crash.log"), e.Exception.ToString());
            }
            else
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(ActiveProjectDir, "Crash.log"), e.Exception.ToString());
            }
        }
        #endregion


        #region MainToolbarButtonEvents
        private void NewProject_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            
            if(!string.IsNullOrWhiteSpace(ActiveProjectName)) saveFileDialog.InitialDirectory = new System.IO.FileInfo(ActiveProjectName).Directory.Parent.Name;
            else saveFileDialog.InitialDirectory = _rootProjectDir;
            
            saveFileDialog.DefaultExt = ".maj";
            //saveFileDialog.FileName = "Default.maj";
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Projet de mise à jour (*.maj)|*.maj";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (DeleteProject(saveFileDialog.FileName))
                {
                    ActiveProjectName = saveFileDialog.FileName;

                    OpenProject();
                }
            }
        }

        private void OpenProject_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            if (!string.IsNullOrWhiteSpace(ActiveProjectName)) openFileDialog.InitialDirectory = new System.IO.FileInfo(ActiveProjectName).Directory.Parent.Name;
            else openFileDialog.InitialDirectory = _rootProjectDir;

            openFileDialog.DefaultExt = ".maj";
            //openFileDialog.FileName = "Default.maj";
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Projet de mise à jour (*.maj)|*.maj";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ActiveProjectName = openFileDialog.FileName;
            }
        }

        private void EditProject_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            EditProjectDialog EditProjectDlg = new EditProjectDialog();
            EditProjectDlg.AppOptions = AppOptions;

            EditProjectDlg.Owner = this;
            EditProjectDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            EditProjectDlg.ShowDialog();

            System.IO.File.WriteAllText(ActiveProjectName, JsonSerializer.Serialize(AppOptions, typeof(ApplicationOptions), JsonOptions));
        }

        private void ImportProjectData_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            List<string> ColList = Data.Tables["Entities"].Columns.Cast<DataColumn>().Where(x => !x.ColumnName.EndsWith("Id") && !x.ColumnName.StartsWith("Vault") && !x.ColumnName.Equals("State") && !x.ColumnName.Equals("Task") && !x.ColumnName.Equals("JobSubmitCount") && !x.ColumnName.Equals("VaultProvider") && !x.ColumnName.Equals("VaultLevel")).Select(x => x.ColumnName).ToList();
            ColList = ColList.Concat(Data.Tables["NewProps"].Columns.Cast<DataColumn>().Where(x => !x.ColumnName.Equals("EntityId")).Select(x => x.ColumnName)).ToList();

            ImporExportProjectDataDialog ImportExportDlg = new ImporExportProjectDataDialog(ColList, ActiveProjectName, "Windows-1252", false);
            ImportExportDlg.Owner = this;
            ImportExportDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ImportExportDlg.ShowDialog();

            List<string> EntityColNames = Data.Tables["Entities"].Columns.Cast<DataColumn>().Select(x => x.ColumnName).Intersect(ImportExportDlg.Data.Columns.Cast<DataColumn>().Select(x => x.ColumnName)).ToList();
            List<string> PropColNames = Data.Tables["NewProps"].Columns.Cast<DataColumn>().Select(x => x.ColumnName).Intersect(ImportExportDlg.Data.Columns.Cast<DataColumn>().Select(x => x.ColumnName)).ToList();


            if (ImportExportDlg.DialogResult == true)
            {
                HashSet<string> ExistingEntities = new HashSet<string>();
                HashSet<string> UpdateEntities = new HashSet<string>();
                string Action = "AddNew";

                if (Data.Tables["Entities"].Rows.Count > 0)
                {
                    DataImportMergeOptionDialog MergeOptionDlg = new DataImportMergeOptionDialog("Votre projet contient déjà des données, voulez-vous:");
                    MergeOptionDlg.Owner = this;
                    MergeOptionDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    MergeOptionDlg.ShowDialog();

                    Action = MergeOptionDlg.MergeOption;

                    if (MergeOptionDlg.MergeOption.Equals("Replace"))
                    {
                        Data.Clear();
                    }
                    else if (MergeOptionDlg.MergeOption.Equals("AddNew"))
                    {
                        ExistingEntities = Data.Tables["Entities"].AsEnumerable().Select(x => x.Field<string>("EntityType") + "|" + x.Field<string>("Name")).ToHashSet();
                    }
                    else if (MergeOptionDlg.MergeOption.Equals("UpdateAddNew"))
                    {
                        ExistingEntities = Data.Tables["Entities"].AsEnumerable().Select(x => x.Field<string>("EntityType") + "|" + x.Field<string>("Name")).ToHashSet();
                        UpdateEntities = ImportExportDlg.Data.AsEnumerable().Select(x => x.Field<string>("EntityType") + "|" + x.Field<string>("Name")).ToHashSet();
                    }
                    else
                    {
                        return;
                    }
                }

                foreach (DataRow dr in ImportExportDlg.Data.Rows)
                {
                    if (Action.Equals("AddNew") && ExistingEntities.Contains(dr.Field<string>("EntityType") + "|" + dr.Field<string>("Name")))
                    {
                        continue;
                    }
                    else if (Action.Equals("UpdateAddNew") && ExistingEntities.Contains(dr.Field<string>("EntityType") + "|" + dr.Field<string>("Name")))
                    {
                        DataRow UpdateEntity = Data.Tables["Entities"].AsEnumerable().Where(x => (x.Field<string>("EntityType") + "|" + x.Field<string>("Name")).Equals(dr.Field<string>("EntityType") + "|" + dr.Field<string>("Name"))).FirstOrDefault(); ;
                        UpdateEntity["Task"] = ApplicationOptions.TaskTypeEnum.Validation;
                        UpdateEntity["State"] = ApplicationOptions.StateEnum.Pending;
                        foreach (string ColName in EntityColNames)
                        {
                            UpdateEntity[ColName] = dr[ColName];
                        }

                        DataRow UpdateProp = UpdateEntity.GetChildRows("EntityNewProp").FirstOrDefault();
                        foreach (string ColName in PropColNames)
                        {
                            UpdateProp[ColName] = dr[ColName];
                        }

                        continue;
                    }

                    DataRow NewEntity = Data.Tables["Entities"].NewRow();
                    NewEntity["Task"] = ApplicationOptions.TaskTypeEnum.Validation;
                    NewEntity["State"] = ApplicationOptions.StateEnum.Pending;
                    foreach (string ColName in EntityColNames)
                    {
                        NewEntity[ColName] = dr[ColName];
                    }

                    Data.Tables["Entities"].Rows.Add(NewEntity);

                    DataRow NewProp = Data.Tables["NewProps"].NewRow();
                    NewProp["EntityId"] = NewEntity["Id"];
                    foreach (string ColName in PropColNames)
                    {
                        NewProp[ColName] = dr[ColName];
                    }
                    Data.Tables["NewProps"].Rows.Add(NewProp);
                }



            }

            Data.SaveToSQLite(ActiveProjectDataBase);

            if (string.IsNullOrWhiteSpace(ImportExportDlg.SourceSheetName)) AppOptions.ImportedDataFile = ImportExportDlg.SourceFileName;
            else AppOptions.ImportedDataFile = ImportExportDlg.SourceFileName + " [" + ImportExportDlg.SourceSheetName + "]";

            System.IO.File.WriteAllText(ActiveProjectName, JsonSerializer.Serialize(AppOptions, typeof(ApplicationOptions), JsonOptions));
        }

        private void ProcessProjectData_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            DataUpdateTaskSeletor UpdateTaskDlg = new DataUpdateTaskSeletor(ref _data, ActiveProjectDataBase, AppOptions);

            UpdateTaskDlg.Owner = this;
            UpdateTaskDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            MainGridControl.BeginDataUpdate();

            UpdateTaskDlg.ShowDialog();

            Data = UpdateTaskDlg.Data;

            MainGridControl.EndDataUpdate();
        }

        private void ClearData_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            Data = DataSetUtility.CreateDataSet(AppOptions.VaultPropertyFieldMappings);

            Data.SaveToSQLite(ActiveProjectDataBase);
        }

        private void ExportLog_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            DataTable Report = Data.Tables["logs"].Clone();

            Report.Columns.Add(new DataColumn("EntityType", typeof(string)));
            Report.Columns.Add(new DataColumn("Path", typeof(string)));
            Report.Columns.Add(new DataColumn("Name", typeof(string)));

            foreach(DataRow dr in Data.Tables["Entities"].Rows)
            {
                DataRow[] dataRows = dr.GetChildRows("EntityLogs");

                if(dataRows.Length > 0)
                {
                    foreach (DataRow log in dataRows)
                    {
                        DataRow newRow = Report.NewRow();
                        newRow["EntityType"] = dr["EntityType"];
                        newRow["Path"] = dr["Path"];
                        newRow["Name"] = dr["Name"];
                        newRow["Severity"] = log["Severity"];
                        newRow["Date"] = log["Date"];
                        newRow["Message"] = log["Message"];
                        newRow["EntityId"] = log["EntityId"];
                        Report.Rows.Add(newRow);
                    }
                }
            }

            ImporExportProjectDataDialog ImportExportDlg = new ImporExportProjectDataDialog(Report, ActiveProjectDir);
            ImportExportDlg.Owner = this;
            ImportExportDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ImportExportDlg.ShowDialog();
        }

        private void GridViewSave_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            ThemedMessageBoxParameters msgBoxParam = new ThemedMessageBoxParameters(MessageBoxImage.Information.GetMessageBoxIcon())
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                AllowTextSelection = true
            };

            MessageBoxResult messageBoxResult = ThemedMessageBox.Show("Sauvegarde de la configuration de la grille",
                                                                      "Etes-vous certain de vouloir écraser la configuration de grille '" + ActiveGridView + "' ?",
                                                                      MessageBoxButton.YesNo, MessageBoxResult.Yes, msgBoxParam);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (string.IsNullOrEmpty(ActiveGridView)) ActiveGridView = "Default";

                MainGridControl.SaveLayoutToXml(System.IO.Path.Combine(ActiveProjectGridViewDir, ActiveGridView + ".xml"));
            }
        }

        private void GridViewSaveAs_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            CreatGridViewDialog GridViewDlg = new CreatGridViewDialog(GridViewList);

            GridViewDlg.Owner = this;
            GridViewDlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            GridViewDlg.ShowDialog();

            if(GridViewDlg.DialogResult == true)
            {
                GridViewList.Add(GridViewDlg.NewGridViewNameTextEdit.Text);

                MainGridControl.SaveLayoutToXml(System.IO.Path.Combine(ActiveProjectGridViewDir, GridViewDlg.NewGridViewNameTextEdit.Text + ".xml"));

                ActiveGridView = GridViewDlg.NewGridViewNameTextEdit.Text;
            }
        }
        
        private void ShowHelp_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {

        }
        #endregion


        #region StatusBarButtonEvents
        private void OpenProjectDir_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            Process.Start(ActiveProjectDir);
        }
        #endregion


        #region ContextMenu
        private void CopyRow_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            if (MainGridView.GridMenu.MenuInfo is GridCellMenuInfo menuInfo && menuInfo.Row != null)
            {
                System.Windows.Clipboard.SetText(string.Join(";", (menuInfo.Row.Row as DataRowView).Row.ItemArray.Select(x => x.ToString())));
            }
        }

        private void CopyCell_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            if (MainGridView.GridMenu.MenuInfo is GridCellMenuInfo menuInfo && menuInfo.Row != null)
            {
                System.Windows.Clipboard.SetText(menuInfo.Row.CellData[menuInfo.Column.ActualVisibleIndex].Value.ToString());
            }
        }
        #endregion


        #region PrivateMethod
        private async void OpenProject()
        {
            IsWaitIndicatorVisible = true;

            AppOptions = await Task.Run(() => (ApplicationOptions)JsonSerializer.Deserialize(System.IO.File.ReadAllText(ActiveProjectName), typeof(ApplicationOptions), JsonOptions));
            AppOptions.Parent = this;

            Data = DataSetUtility.CreateDataSet(AppOptions.VaultPropertyFieldMappings);

            if (!System.IO.File.Exists(ActiveProjectDataBase))
            {
                Data.SaveToSQLite(ActiveProjectDataBase);
            }
            else
            {
                Data = await Data.ReadFromSQLiteAsync(ActiveProjectDataBase);
            }

            RefreshGridColumns();

            GridViewList = GetProjectGridList();
            ActiveGridView = "Default";

            IsWaitIndicatorVisible = false;
        }

        private bool DeleteProject(string fileName)
        {
            try
            {
                string fileExt = System.IO.Path.GetExtension(fileName);
                string dirName = fileName.Substring(0, fileName.Length - fileExt.Length);

                if (System.IO.Directory.Exists(dirName))
                {
                    System.IO.Directory.Delete(dirName, true);
                }

                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }

                return true;
            }
            catch (Exception Ex)
            {
                return false;
            }
        }

        private ObservableCollection<string> GetProjectGridList()
        {
            string DefaultGridViewName = System.IO.Path.Combine(ActiveProjectGridViewDir, "Default.xml");
            if (!System.IO.File.Exists(DefaultGridViewName))
            {
                string SourceFile = System.IO.Directory.EnumerateFiles(ApplicationDir, "Default.xml", SearchOption.AllDirectories).FirstOrDefault();
                if(!string.IsNullOrWhiteSpace(SourceFile)) 
                {
                    System.IO.File.Copy(SourceFile, DefaultGridViewName);
                }
                else
                {
                    ThemedMessageBoxParameters msgBoxParam = new ThemedMessageBoxParameters(MessageBoxImage.Error.GetMessageBoxIcon())
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        AllowTextSelection = true
                    };

                    ThemedMessageBox.Show("Erreur de configuration","La vue par défaut de la grille '" + SourceFile + "' n'existe pas!", msgBoxParam);
                }
            }

            return new ObservableCollection<string>(new System.IO.DirectoryInfo(ActiveProjectGridViewDir).EnumerateFiles("*.xml", SearchOption.TopDirectoryOnly).Select(x => System.IO.Path.GetFileNameWithoutExtension(x.Name)).OrderBy(x => x));
        }

        public void RefreshGridColumns()
        {
            List<string> FieldNames = GridColumList.Select(x => x.FieldName).ToList();

            for (int i = MainGridControl.Columns.Count - 1; i > 0; i--)
            {
                if (!FieldNames.Contains(MainGridControl.Columns[i].FieldName)) MainGridControl.Columns.RemoveAt(i);

            }

            foreach (GridColumn c in GridColumList)
            {
                // if (c.FieldName == "Main_Ext") c.CellTemplate = (MainGridControl.FindResource("ExtTemplate") as DataTemplate);

                if (MainGridControl.Columns.Where(x => x.FieldName == c.FieldName).FirstOrDefault() == null)
                {
                    MainGridControl.Columns.Add(c);
                }
            }
        }

        private string ConvertToEnum(object val, string enumTypeName)
        {
            if (enumTypeName.Equals("StateEnum")) return ((StateEnum)Enum.Parse(typeof(StateEnum), val.ToString())).ToString();
            else if (enumTypeName.Equals("TaskTypeEnum")) return ((TaskTypeEnum)Enum.Parse(typeof(TaskTypeEnum), val.ToString())).ToString();
            //else if (enumTypeName.Equals("FileProviderEnum")) return ((FileProviderEnum)Enum.Parse(typeof(FileProviderEnum), val.ToString())).ToString();
            else return val.ToString();
        }
        #endregion
    }
}
