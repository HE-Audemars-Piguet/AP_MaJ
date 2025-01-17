using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DevExpress.Xpf.Grid;
using Ch.Hurni.AP_MaJ.Utilities;
using GenericParsing;
using System.Diagnostics;
using System.Threading.Tasks;
using static DevExpress.Data.Helpers.FindSearchRichParser;
using System.ComponentModel.DataAnnotations;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using static Ch.Hurni.AP_MaJ.Classes.ApplicationOptions;

namespace Ch.Hurni.AP_MaJ.Dialogs
{
    /// <summary>
    /// Interaction logic for ImporExportProjectDataDialog.xaml
    /// </summary>
    public partial class ImporExportProjectDataDialog : ThemedWindow, INotifyPropertyChanged
    {
        #region Properties
        public string SourceFileName
        {
            get
            {
                return _sourceFileName;
            }
            set
            {
                _sourceFileName = value;

                if (ExportGridOption.Visibility == Visibility.Visible)
                {
                    if (!string.IsNullOrWhiteSpace(_sourceFileName))
                    {
                        if (_sourceFileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ExcelSheetName.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ExcelSheetName.Visibility = Visibility.Visible;
                        }
                        ExportButton.IsEnabled = true;
                    }
                    else
                    {
                        ExportButton.IsEnabled = false;
                    }
                }

                if (ImportGridOption.Visibility == Visibility.Visible)
                {
                    ExcelSheetSelectorCombo.SelectedIndex = -1;

                    if (!string.IsNullOrWhiteSpace(_sourceFileName) && System.IO.File.Exists(_sourceFileName))
                    {
                        if (_sourceFileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ExcelSheetSelector.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            string sConnection = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _sourceFileName + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"";

                            OleDbConnection oleExcelConnection = new OleDbConnection(sConnection);
                            try
                            {
                                oleExcelConnection.Open();
                                
                                DataTable dtTablesList = oleExcelConnection.GetSchema("Tables");

                                foreach (string ExcelSheet in dtTablesList.AsEnumerable().Select(x => x.Field<string>("TABLE_NAME")))
                                {
                                    if (ExcelSheet.EndsWith("$")) ExcelSheets.Add(new ExcelSheetName() { DisplayName = ExcelSheet.Substring(0, ExcelSheet.Length - 1), Name = ExcelSheet });
                                    else if (ExcelSheet.StartsWith("'") && ExcelSheet.EndsWith("$'")) ExcelSheets.Add(new ExcelSheetName() { DisplayName = ExcelSheet.Substring(1, ExcelSheet.Length - 3), Name = ExcelSheet });
                                }

                                ExcelSheets = new ObservableCollection<ExcelSheetName>(ExcelSheets.OrderBy(x => x.DisplayName));

                                oleExcelConnection.Close();

                                ExcelSheetSelector.Visibility = Visibility.Visible;

                                if (ExcelSheets.Count > 0)
                                {
                                    ExcelSheetSelectorCombo.SelectedIndex = 0;
                                }
                            }
                            catch(Exception Ex)
                            {
                                ThemedMessageBoxParameters msgBoxParam = new ThemedMessageBoxParameters(MessageBoxImage.Error.GetMessageBoxIcon())
                                {
                                    Owner = this,
                                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                                    AllowTextSelection = true
                                };

                                MessageBoxResult messageBoxResult = ThemedMessageBox.Show("Erreur de lecture du fichier",
                                                                                          Ex.Message.Replace("''.", "'" + _sourceFileName + "'."),
                                                                                          MessageBoxButton.OK, MessageBoxResult.OK, msgBoxParam);
                            }
                        }
                    }
                }

                NotifyPropertyChanged();
            }
        }
        private string _sourceFileName = string.Empty;

        public string SourceSheetName
        {
            get
            {
                return _sourceSheetName;
            }
            set
            {
                _sourceSheetName = value;
                NotifyPropertyChanged();
            }
        }
        private string _sourceSheetName = string.Empty;

        public bool CanImport
        {
            get
            {
                return _canImport;
            }
            set
            {
                _canImport = value;
                NotifyPropertyChanged();
            }
        }
        private bool _canImport = false;

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

        private List<string> _columnNamesList = new List<string>();
        private string _defaultDir = string.Empty;
        private string _defaultFileName = string.Empty;
        private string _defaultEncodingName = string.Empty;
        private bool _detectEncoding = true;

        public ObservableCollection<ExcelSheetName> ExcelSheets
        {
            get
            {
                return _excelSheets;
            }
            set
            {
                _excelSheets = value;
                NotifyPropertyChanged();
            }
        }
        private ObservableCollection<ExcelSheetName> _excelSheets = new ObservableCollection<ExcelSheetName>();

        public DataTable Data
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
        private DataTable _data = new DataTable();
        #endregion

        #region Attributes

        #endregion

        #region Constructors
        public ImporExportProjectDataDialog(List<string> columnNamesList, string defaultDir = "", string defaultEncodingName = "UTF-8", bool detectEncoding = true)
        {
            _columnNamesList = columnNamesList;
            _defaultDir = defaultDir;
            _defaultEncodingName = defaultEncodingName;
            _detectEncoding = detectEncoding;

            DataContext = this;

            InitializeComponent();

            Title = "Pré-visualisation des données à importer";
            ExportGridOption.Visibility = Visibility.Collapsed;
            ImportGridOption.Visibility = Visibility.Visible;

            ExportButton.Visibility = Visibility.Collapsed;
            ImportButton.Visibility = Visibility.Visible;
        }

        public ImporExportProjectDataDialog(DataTable exportedData, string defaultDir = "", string defaultFileName = "", string defaultEncodingName = "UTF-8")
        {
            _data = exportedData;
            _defaultDir = defaultDir;
            _defaultFileName = defaultFileName;
            _defaultEncodingName = defaultEncodingName;

            DataContext = this;

            InitializeComponent();

            Title = "Pré-visualisation des données à exporter";
            ImportGridOption.Visibility = Visibility.Collapsed;
            ExportGridOption.Visibility = Visibility.Visible;

            ImportButton.Visibility = Visibility.Collapsed;
            ExportButton.Visibility = Visibility.Visible;

            ContentRendered += DataImportExportDialog_ContentRendered;
        }

        #endregion


        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void DataImportExportDialog_ContentRendered(object sender, EventArgs e)
        {
            if (ExportGridOption.Visibility == Visibility.Visible)
            {
                if (!string.IsNullOrWhiteSpace(_defaultDir) && !string.IsNullOrWhiteSpace(_defaultFileName)) SourceFileName = System.IO.Path.Combine(_defaultDir, _defaultFileName + ".xlsx");
                if (string.IsNullOrWhiteSpace(Data.TableName)) Data.TableName = "Sheet1";
            }
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.InitialDirectory = _defaultDir;
            openFileDialog.AddExtension = true;
            openFileDialog.CheckFileExists = false;
            openFileDialog.Multiselect = false;

            openFileDialog.Filter = "Excel file (*.xlsx)|*.xlsx|" +
                                    "Excel file (*.xls)|*.xls|" +
                                    "Csv file (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImportGrid.BeginDataUpdate();
                Data.Clear();
                ExcelSheets.Clear();
                CanImport = false;
                ImportGrid.EndDataUpdate();

                bool oledb12Installed = new System.Data.OleDb.OleDbEnumerator().GetElements().AsEnumerable().Any(x => x.Field<string>("SOURCES_NAME") == "Microsoft.ACE.OLEDB.12.0");

                if (!System.IO.Path.GetExtension(openFileDialog.FileName).Equals(".csv", StringComparison.InvariantCultureIgnoreCase) && !oledb12Installed)
                {
                    MessageBox.Show("To allow Excel file import, you must install 'Microsoft.ACE.OLEDB.12.0' on your computer." + System.Environment.NewLine +
                                    "You can download the installer from the Microsoft web site." + System.Environment.NewLine +
                                    "https://www.microsoft.com/en-us/download/details.aspx?id=54920");

                }
                else
                {
                    SourceFileName = openFileDialog.FileName;
                }

            }
        }

        private async void FileContentLoad_Click(object sender, RoutedEventArgs e)
        {
            IsWaitIndicatorVisible = true;

            if (ExcelSheetSelectorCombo.SelectedItem != null)
            {
                SourceSheetName = (ExcelSheetSelectorCombo.SelectedItem as ExcelSheetName).Name;
            }

            ImportGrid.BeginDataUpdate();
            (DataTable ResultDataTable, SeverityEnum Severity, string Message) Result = await Task.Run(() => ReadFileContent(SourceSheetName));

            if (Result.Severity == SeverityEnum.Info)
            {
                Data = Result.ResultDataTable;
            }
            else if (Result.Severity == SeverityEnum.Warning)
            {
                ThemedMessageBoxParameters msgBoxParam = msgBoxParam = new ThemedMessageBoxParameters(MessageBoxImage.Warning.GetMessageBoxIcon())
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    AllowTextSelection = true
                };

                MessageBoxResult messageBoxResult = ThemedMessageBox.Show("Problème lors de la lecture du fichier", Result.Message +"\n\nVoulez vous tout de même importer ces données?",
                                                                          MessageBoxButton.YesNo, MessageBoxResult.Yes, msgBoxParam);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Data = Result.ResultDataTable;
                }
            }
            else if(Result.Severity == SeverityEnum.Error)
            {
                ThemedMessageBoxParameters msgBoxParam = msgBoxParam = new ThemedMessageBoxParameters(MessageBoxImage.Error.GetMessageBoxIcon())
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    AllowTextSelection = true
                };

                MessageBoxResult messageBoxResult = ThemedMessageBox.Show("Erreur lors de la lecture du fichier", Result.Message + "\n\nCes données ne sont pas valide et ne seront pas importées!",
                                                                          MessageBoxButton.OK, MessageBoxResult.OK, msgBoxParam);
            }

            if(Data.AsEnumerable().Select(x =>x.Field<string>("Name")).Distinct().Count() != Data.Rows.Count)
            {
                ThemedMessageBoxParameters msgBoxParam = msgBoxParam = new ThemedMessageBoxParameters(MessageBoxImage.Warning.GetMessageBoxIcon())
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    AllowTextSelection = true
                };

                MessageBoxResult messageBoxResult = ThemedMessageBox.Show("Problème lors de la lecture du fichier", "Le fichier importé contient des noms ('Name') dupliqués.\nLes duplicatas seront ignorés\n\nVoulez vous tout de même importer ces données?",
                                                                          MessageBoxButton.YesNo, MessageBoxResult.Yes, msgBoxParam);

                if (messageBoxResult == MessageBoxResult.No)
                {
                    Data.Clear();
                }
            }

            ImportGrid.EndDataUpdate();

            CanImport = Data.Rows.Count > 0;
            IsWaitIndicatorVisible = false;
        }

        private (DataTable ResultDataTable, SeverityEnum Severity, string Message) ReadFileContent(string ExcelSheetName = "")
        {
            DataTable dt = new DataTable();
            string msg = string.Empty;
            SeverityEnum severity = SeverityEnum.Info;


            if (SourceFileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
            {
                Encoding encoding = Encoding.GetEncoding(_defaultEncodingName);
                if (_detectEncoding) encoding = EncodingUtility.DetectFileEncoding(SourceFileName, encoding);

                using (GenericParserAdapter parser = new GenericParserAdapter(SourceFileName, encoding))
                {
                    parser.ColumnDelimiter = ';';
                    parser.FirstRowHasHeader = true;
                    parser.FirstRowSetsExpectedColumnCount = true;
                    parser.SkipEmptyRows = true;
                    parser.TextQualifier = '"';
                    parser.TrimResults = true;

                    try
                    {
                        dt = parser.GetDataTable();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
            {
                string sConnection = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + SourceFileName + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\"";

                using (OleDbConnection oleExcelConnection = new OleDbConnection(sConnection))
                {
                    oleExcelConnection.Open();

                    OleDbDataAdapter da = new OleDbDataAdapter("Select * From [" + ExcelSheetName + "]", oleExcelConnection);
                    da.Fill(dt);

                    oleExcelConnection.Close();
                }
            }

            foreach(DataColumn dc in dt.Columns)
            {
                if (dc.ColumnName.Contains("#")) dc.ColumnName = dc.ColumnName.Replace("#", ".");
            }

            List<string> dtColumnList = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

            List<string> CommonColNames = dtColumnList.Intersect(_columnNamesList).ToList();
            List<string> MissingColNames = _columnNamesList.Except(dtColumnList).ToList();
            List<string> CriticalMissingColNames = new List<string> { "EntityType", "Name", "TempVaultLcsName", "TargetVaultLcsName" }.Except(dtColumnList).ToList();

            if (CriticalMissingColNames.Count > 0)
            {
                severity = SeverityEnum.Error;
                msg = string.Format("Le fichier '{0}' n'est pas valide.\n\nLes colonnes obligatoires suivantes manquent:\n{1}", SourceFileName, "- " + string.Join(System.Environment.NewLine + "- ", CriticalMissingColNames));
                return (null, severity, msg);
            }
            else if (MissingColNames.Count > 0)
            {
                severity = SeverityEnum.Warning;
                msg = string.Format("Le fichier '{0}' est incomplet.\n\nLes colonnes suivantes manquent:\n{1}", SourceFileName, "- " + string.Join(System.Environment.NewLine + "- ", MissingColNames));
            }

            dt = dt.DefaultView.ToTable(false, CommonColNames.ToArray()).Copy();

            if (dt.Columns.Cast<DataColumn>().Where(x => x.DataType != typeof(string)).Count() > 0)
            {
                DataTable clonedDt = dt.Clone();
                foreach (DataColumn dc in clonedDt.Columns)
                {
                    if (dc.DataType != typeof(string)) dc.DataType = typeof(string);
                }
                foreach (DataRow row in dt.Rows)
                {
                    clonedDt.ImportRow(row);
                }

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < row.ItemArray.Length; i++)
                    {
                        if (row.ItemArray[i] == null || row.ItemArray[i] is DBNull) row.ItemArray[i] = string.Empty;
                    }
                }

                dt = clonedDt;
            }

            foreach(string MissingCol in MissingColNames)
            {
                dt.Columns.Add(new DataColumn(MissingCol, typeof(string)) { DefaultValue = string.Empty });
            }

            return (dt, severity, msg);
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        { 
            if (Data.AsEnumerable().Select(x => x.Field<string>("Name")).Distinct().Count() != Data.Rows.Count)
            {
                Data = Data.AsEnumerable().GroupBy(x => x.Field<string>("Name")).Select(g => g.First()).CopyToDataTable();
            }

            DialogResult = true;
            Close();
        }

        private void FileSave_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.InitialDirectory = _defaultDir;
            saveFileDialog.FileName = _defaultFileName + ".xlsx";
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Excel file (*.xlsx)|*.xlsx|" +
                                    "Excel file (*.xls)|*.xls|" +
                                    "Csv file (*.csv)|*.csv";

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceFileName = saveFileDialog.FileName;
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (SourceFileName.EndsWith(".xlsx", StringComparison.InvariantCultureIgnoreCase)) ImportGrid.View.ExportToXlsx(SourceFileName, new DevExpress.XtraPrinting.XlsxExportOptions() { SheetName = Data.TableName });
            else if (SourceFileName.EndsWith(".xls", StringComparison.InvariantCultureIgnoreCase)) ImportGrid.View.ExportToXls(SourceFileName, new DevExpress.XtraPrinting.XlsExportOptions() { SheetName = Data.TableName });
            else if (SourceFileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase)) ImportGrid.View.ExportToCsv(SourceFileName);

            DialogResult = true;
            Close();
        }

        bool lockEvents;
        private void MainGridView_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (lockEvents)
                return;
            lockEvents = true;
            SetSelectedCellsValues(e.Value);
            lockEvents = false;
        }

        private void SetSelectedCellsValues(object value)
        {
            List<GridCell> GridCells = (ImportGrid.View as TableView).GetSelectedCells().ToList();

            foreach (GridCell cell in GridCells)
            {
                int rowHandle = cell.RowHandle;
                GridColumn column = cell.Column;

                ImportGrid.SetCellValue(rowHandle, column, value);
            }
        }
    }

    public class ExcelSheetName
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
