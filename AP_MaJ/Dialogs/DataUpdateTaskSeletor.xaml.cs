using Ch.Hurni.AP_MaJ.Utilities;
using Ch.Hurni.AP_MaJ.Classes;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using DevExpress.Xpf.Controls;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI.Internal;
using DevExpress.XtraReports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Ch.Hurni.AP_MaJ.Classes.ApplicationOptions;
using static DevExpress.Utils.SafeXml;


namespace CH.Hurni.AP_MaJ.Dialogs
{
    /// <summary>
    /// Interaction logic for DataUpdateTaskSeletor.xaml
    /// </summary>
    public partial class DataUpdateTaskSeletor : ThemedWindow, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public ObservableCollection<MaJTask> MaJTasks { get; set; }

        public ObservableCollection<string> MaJToDoTasks { get; set; }

        public Progress<TaskProgressReport> TaskProgReport { get; set; }

        public Progress<ProcessProgressReport> ProcessProgReport { get; set; }

        public bool IsCloseButtonEnable
        {
            get
            {
                return _isCloseButtonEnable;
            }
            set
            {
                _isCloseButtonEnable = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isCloseButtonEnable = true;

        public DataSet Data
        {
            get
            {
                return _data;
            }
        }
        private DataSet _data;
        private string _dbFileName = string.Empty;

        private ApplicationOptions appOptions;
        private VaultUtility vaultUtility;
        private MaJTask currentTask = null;
        private DispatcherTimer dTimer;
        private DateTime dTimerStartTime;
        private CancellationTokenSource TaskCancellationTokenSource;

        public InventorDispatcher InvDispatcher
        {
            get
            {
                return _invDispatcher;
            }
            set
            {
                _invDispatcher = value;
            }
        }
        private InventorDispatcher _invDispatcher = null;

        public DataUpdateTaskSeletor(ref DataSet data, string dbFileName, ApplicationOptions appOptions)
        {
            _data = data;
            _dbFileName = dbFileName;
            _invDispatcher = new InventorDispatcher(appOptions.MaxInventorAppCount);
            
            this.appOptions = appOptions;
            this.vaultUtility = new VaultUtility(_invDispatcher);

            MaJTasks = new ObservableCollection<MaJTask>();
            MaJTask VaultTask = new MaJTask() { Name = "VaultInventor", DisplayName = "Lecture des configurations Vault et Inventor", IsChecked = true, Index = 0 };
            VaultTask.SubTasks = new ObservableCollection<MaJTask>();
            VaultTask.SubTasks.Add(new MaJTask() { Name = "Connect", DisplayName = "Connexion au Vault", IsChecked = true, Index = 1, Parent = VaultTask });
            VaultTask.SubTasks.Add(new MaJTask() { Name = "ReadVaultConfig", DisplayName = "Lecture de la configuration Vault", IsChecked = true, Index = 2, Parent = VaultTask });
            VaultTask.SubTasks.Add(new MaJTask() { Name = "ReadInventorConfig", DisplayName = "Lecture de la configuration Inventor", IsChecked = true, Index = 2, Parent = VaultTask });
            MaJTasks.Add(VaultTask);


            MaJTask FileTask = new MaJTask() { Name = "File", DisplayName = "Tâches de mise à jour des fichiers", IsChecked = true, Index = 100 };
            FileTask.SubTasks = new ObservableCollection<MaJTask>();
            FileTask.SubTasks.Add(new MaJTask() { Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = true, Index = 101, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = true, Index = 102, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "PurgeProps", DisplayName = "Ajout/suppression des propriétés", IsChecked = true, Index = 103, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "Update", DisplayName = "Mise à jour", IsChecked = true, Index = 104, Parent = FileTask });
            //FileTask.SubTasks.Add(new MaJTask() { Name = "PropSync", DisplayName = "Synchronisation des propriétés", IsChecked = false, Index = 105, Parent = FileTask });
            //FileTask.SubTasks.Add(new MaJTask() { Name = "CreateBomBlob", DisplayName = "Créer les BOM blob", IsChecked = false, Index = 106, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "WaitForBomBlob", DisplayName = "Attendre et forcer la création des BOM blob", IsChecked = false, Index = 105, Parent = FileTask });
            MaJTasks.Add(FileTask);

            MaJTask ItemTask = new MaJTask() { Name = "Item", DisplayName = "Tâches de mise à jour des articles", IsChecked = false, Index = 200 };
            ItemTask.SubTasks = new ObservableCollection<MaJTask>();
            ItemTask.SubTasks.Add(new MaJTask() { Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = false,Index = 201, Parent = ItemTask });
            ItemTask.SubTasks.Add(new MaJTask() { Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = false, Index = 202, Parent = ItemTask });
            ItemTask.SubTasks.Add(new MaJTask() { Name = "PurgeProps", DisplayName = "Ajout/suppression des propriétés", IsChecked = false, Index = 203, Parent = ItemTask });
            ItemTask.SubTasks.Add(new MaJTask() { Name = "Update", DisplayName = "Mise à jour", IsChecked = false, Index = 204, Parent = ItemTask });
            //ItemTask.SubTasks.Add(new MaJTask() { Name = "PropSync", DisplayName = "Synchronisation des propriétés", IsChecked = false, Index = 205, Parent = ItemTask });
            MaJTasks.Add(ItemTask);

            MaJToDoTasks = new ObservableCollection<string>();

            DataContext = this;
            InitializeComponent();
        }

        private void ShowTaskProgress(object sender, TaskProgressReport e)
        {
            if(e.Timer != null)
            {
                if (e.Timer.Equals("Start"))
                {
                    dTimerStartTime = DateTime.Now;
                    currentTask.TaskDuration = "00:00";                    
                    
                    dTimer = new DispatcherTimer();
                    dTimer.Interval = TimeSpan.FromSeconds(0.1);
                    dTimer.Tick += timer_Tick;
                    
                    dTimer.Start();                   
                }
                else if (e.Timer.Equals("Stop"))
                {
                    dTimer.Stop();

                    dTimer.Tick -= timer_Tick;

                    currentTask.TaskDuration = currentTask.FormatTimeSpan(DateTime.Now.Subtract(dTimerStartTime));
                }
            }

            currentTask.TotalElementCount = e.TotalEntityCount;
            currentTask.TaskDetail = e.Message;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            currentTask.TaskDuration = currentTask.FormatTimeSpan(DateTime.Now.Subtract(dTimerStartTime));
        }

        private void ShowProcessProgress(object sender, ProcessProgressReport e)
        {
            if(currentTask.ProcessFeedback.Count <= e.ProcessIndex)
            {
                for (int i = currentTask.ProcessFeedback.Count; i <= e.ProcessIndex; i++)
                {
                    currentTask.ProcessFeedback.Add("");
                }
            }

            currentTask.ProcessFeedback[e.ProcessIndex] = e.Message;

            currentTask.ElementCount = currentTask.ElementCount + e.TotalCountInc;
            currentTask.ElementDoneCount = currentTask.ElementDoneCount + e.DoneInc;

            if(e.ErrorInc > 0)
            {
                currentTask.ElementErrorCount = currentTask.ElementErrorCount + e.ErrorInc;
                if (appOptions.ProcessingBehaviour == ProcessingBehaviourEnum.Stop) TaskCancellationTokenSource.Cancel();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Page1.Visibility = Visibility.Collapsed;
            Page2.Visibility = Visibility.Visible;
            Page3.Visibility = Visibility.Collapsed;

            ToDoList.ItemsSource = MaJTasks.SelectMany(x=>x.SubTasks).Where(y => y.IsChecked == true).ToList();
        }

        private async void ExecutButton_Click(object sender, RoutedEventArgs e)
        {
            if(ExecutButton.Tag.ToString().Equals("Run"))
            {
                BackButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                CancelButton.IsDefault = false;

                ExecutButton.Tag = "Stop";
                ExecutButton.Content = "Stop";
                ExecutButton.IsDefault = false;

                TaskCancellationTokenSource = new CancellationTokenSource();
                CancellationToken TaskCancellationToken = TaskCancellationTokenSource.Token;
                
                TaskProgReport = new Progress<TaskProgressReport>();
                ProcessProgReport = new Progress<ProcessProgressReport>();

                foreach (MaJTask t in MaJTasks.SelectMany(x => x.SubTasks).Where(y => y.IsChecked == true).OrderBy(y => y.Index))
                {
                    currentTask = t;
                    currentTask.ProcessFeedback.Clear();

                    if (TaskCancellationToken.IsCancellationRequested)
                    {
                        currentTask.ProcessingState = StateEnum.Canceled;
                        continue;
                    }

                    TaskProgReport.ProgressChanged += ShowTaskProgress;
                    ProcessProgReport.ProgressChanged += ShowProcessProgress;

                    currentTask.ProcessingState = StateEnum.Processing;

                    if (currentTask.Parent.Name.Equals("VaultInventor"))
                    {
                        if (currentTask.Name.Equals("Connect"))
                        {
                            vaultUtility.VaultConnection = await vaultUtility.ConnectToVaultAsync(appOptions, TaskProgReport, TaskCancellationToken);
                            if (vaultUtility.VaultConnection == null)
                            {
                                currentTask.ProcessingState = StateEnum.Error;
                                return;
                            }
                            else
                            {
                                currentTask.ProcessingState = StateEnum.Completed;
                            }
                        }
                        else if (currentTask.Name.Equals("ReadVaultConfig"))
                        {
                            vaultUtility.VaultConfig = await vaultUtility.ReadVaultConfigAsync(appOptions, TaskProgReport, TaskCancellationToken);
                            vaultUtility.VaultConfig.FolderPathToFolderDico = await vaultUtility.GetTargetVaultFoldersAsync(_data, TaskProgReport, TaskCancellationToken);
                        }
                        else if (currentTask.Name.Equals("ReadInventorConfig"))
                        {
                            vaultUtility.VaultConfig.InventorMaterials = await vaultUtility.GetInventorMaterialAsync(appOptions, TaskProgReport, TaskCancellationToken);
                        }
                    }
                    else if (currentTask.Parent.Name.Equals("File"))
                    {
                        _data = await vaultUtility.ProcessFilesAsync(currentTask.Name, _data, appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.Parent.Name.Equals("Item"))
                    {
                        _data = await vaultUtility.ProcessItemsAsync(currentTask.Name, _data, appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                    }

                    if (TaskCancellationToken.IsCancellationRequested) currentTask.ProcessingState = StateEnum.Canceled;
                    else if (currentTask.ElementErrorCount > 0) currentTask.ProcessingState = StateEnum.Error;
                    else currentTask.ProcessingState = StateEnum.Completed;


                    if (appOptions.ProcessingBehaviour == ProcessingBehaviourEnum.FinishTask && currentTask.ProcessingState == StateEnum.Error)
                    {
                        TaskCancellationTokenSource.Cancel();
                    }

                    ProcessProgReport.ProgressChanged -= ShowProcessProgress;
                    TaskProgReport.ProgressChanged -= ShowTaskProgress;
                }
            }
            else
            {
                TaskCancellationTokenSource.Cancel();
            }

            Page1.Visibility = Visibility.Collapsed;
            Page2.Visibility = Visibility.Collapsed;
            Page3.Visibility = Visibility.Visible;
            
            _data.SaveToSQLite(_dbFileName);

            DoneList.ItemsSource = MaJTasks.SelectMany(x => x.SubTasks).Where(y => y.IsChecked == true).ToList();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Page1.Visibility = Visibility.Visible;
            Page2.Visibility = Visibility.Collapsed;
            Page3.Visibility = Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            string ReportName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_dbFileName), "Report " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".log");
            string Report = "DisplayName;ProcessingState;ElementCount;TotalElementCount;ElementDoneCount;ElementErrorCount;TaskDuration";
            foreach (MaJTask t in MaJTasks.SelectMany(x => x.SubTasks).Where(y => y.IsChecked == true).OrderBy(y => y.Index))
            {

                Report += Environment.NewLine + t.DisplayName + ";" + t.ProcessingState + ";" + t.ElementCount + ";" + t.TotalElementCount + ";" + t.ElementDoneCount + ";" + t.ElementErrorCount + ";" + t.TaskDuration;
            }

            System.IO.File.WriteAllText(ReportName, Report);

            Close();
        }
    }

    public class MaJTask : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IsChecked 
        { 
            get
            {
                return _isChecked;
            }
            set 
            {
                _isChecked = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isChecked = false;

        public StateEnum ProcessingState
        {
            get
            {
                return _processingState;
            }
            set
            {
                _processingState = value;
                NotifyPropertyChanged();
            }
        }
        private StateEnum _processingState = StateEnum.Pending;

        public MaJTask Parent { get; set; }

        public ObservableCollection<MaJTask> SubTasks { get; set; }

        public long TotalElementCount
        {
            get
            {
                return _totalElementCount;
            }
            set
            {
                _totalElementCount = value;
                NotifyPropertyChanged();
            }
        }
        private long _totalElementCount = -1;
        
        public long ElementCount
        {
            get
            {
                return _elementCount;
            }
            set
            {
                _elementCount = value;
                NotifyPropertyChanged();
            }
        }
        private long _elementCount = 0;

        public long ElementDoneCount
        {
            get
            {
                return _elementDoneCount;
            }
            set
            {
                _elementDoneCount = value;
                NotifyPropertyChanged();
            }
        }
        private long _elementDoneCount = 0;

        public long ElementErrorCount
        {
            get
            {
                return _elementErrorCount;
            }
            set
            {
                _elementErrorCount = value;
                NotifyPropertyChanged();
            }
        }
        private long _elementErrorCount = 0;

        public string TaskDuration
        {
            get
            {
                return _taskDuration;
            }
            set
            { 
                _taskDuration = value;
                NotifyPropertyChanged();
            }
        }
        private string _taskDuration = "00:00";

        public string TaskDetail
        {
            get
            {
                return _taskDetail;
            }
            set
            {
                _taskDetail = value;
                NotifyPropertyChanged();
            }
        }
        private string _taskDetail = string.Empty;

        public ObservableCollection<string> ProcessFeedback { get; set; } = new ObservableCollection<string>();


        public MaJTask() { }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        public string FormatTimeSpan(TimeSpan ts)
        {
            var sb = new StringBuilder();

            if ((int)ts.TotalHours > 0)
            {
                sb.Append((int)ts.TotalHours);
                sb.Append(":");
            }

            sb.Append(ts.Minutes.ToString("00"));
            sb.Append(":");
            sb.Append(ts.Seconds.ToString("00"));
            //sb.Append(".");
            //sb.Append(ts.Milliseconds.ToString("00"));

            return sb.ToString();
        }
    }
}
