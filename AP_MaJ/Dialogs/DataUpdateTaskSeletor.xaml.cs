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

            MaJTask VaultConnectTask = new MaJTask() { TaskGroup="Vault", Name = "VaultConnect", DisplayName = "Connexion au Vault", IsChecked = true, Index = 0, IsIndeterminate = true };
            MaJTasks.Add(VaultConnectTask);

            MaJTask ReadVaultConfigTask = new MaJTask() { TaskGroup = "Vault", Name = "ReadVaultConfig", DisplayName = "Lecture de la configuration Vault", IsChecked = true, Index = 1, IsIndeterminate = true };
            MaJTasks.Add(ReadVaultConfigTask);

            MaJTask CreateVaultFolderTask = new MaJTask() { TaskGroup = "Vault", Name = "CreateVaultFolder", DisplayName = "Création des nouveaux dossiers Vault", IsChecked = true, Index = 2, IsIndeterminate = true };
            MaJTasks.Add(CreateVaultFolderTask);

            MaJTask ReadInventorConfigTask = new MaJTask() { TaskGroup = "Inventor", Name = "ReadInventorConfig", DisplayName = "Lecture de la configuration Inventor", IsChecked = true, Index = 10, IsIndeterminate = true };
            MaJTasks.Add(ReadInventorConfigTask);

            MaJTask FileTask = new MaJTask() { IsGroup = true, TaskGroup = "File", Name = "File", DisplayName = "Tâches de mise à jour des fichiers", IsChecked = null, IsTaskCheckBoxEnable = true, Index = 20 };
            FileTask.SubTasks = new ObservableCollection<MaJTask>();
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 21});
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 22 });
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "PurgeProps", DisplayName = "Ajout/suppression des propriétés", IsChecked = false, IsTaskCheckBoxEnable = true, Index = 23 });
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "Update", DisplayName = "Mise à jour", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 24 });
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "WaitForBomBlob", DisplayName = "Attendre et forcer la création des BOM blob", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 25 });
            MaJTasks.Add(FileTask);

            MaJTask InventorCloseTask = new MaJTask() { TaskGroup = "Inventor", Name = "InventorClose", DisplayName = "Fermeture des sessions Inventor", IsChecked = true, Index = 30, IsIndeterminate = true };
            MaJTasks.Add(InventorCloseTask);

            MaJTask ItemTask = new MaJTask() { IsGroup = true, TaskGroup = "Item", Name = "Item", DisplayName = "Tâches de mise à jour des articles", IsChecked = false, IsTaskCheckBoxEnable = true, Index = 40 };
            ItemTask.SubTasks = new ObservableCollection<MaJTask>();
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = false, IsTaskCheckBoxEnable = true, Index = 41 });
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = false, IsTaskCheckBoxEnable = true, Index = 42 });
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "PurgeProps", DisplayName = "Ajout/suppression des propriétés", IsChecked = false, IsTaskCheckBoxEnable = true, Index = 43 });
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "Update", DisplayName = "Mise à jour", IsChecked = false, IsTaskCheckBoxEnable = true, Index = 44 });
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

            ToDoList.ItemsSource = CollectSelectedTasks(MaJTasks).ToList();

            //ToDoList.ItemsSource = MaJTasks.SelectMany(x=>x.SubTasks).Where(y => y.IsChecked == true).ToList();
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

                foreach (MaJTask t in CollectSelectedTasks(MaJTasks).OrderBy(x => x.Index).ToList())
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

                    if (currentTask.Name.Equals("VaultConnect"))
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
                    }                    
                    else if (currentTask.Name.Equals("CreateVaultFolder"))
                    {
                        vaultUtility.VaultConfig.FolderPathToFolderDico = await vaultUtility.GetTargetVaultFoldersAsync(_data, TaskProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.Name.Equals("ReadInventorConfig"))
                    {
                        vaultUtility.VaultConfig.InventorMaterials = await vaultUtility.GetInventorMaterialAsync(appOptions, TaskProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.TaskGroup.Equals("File"))
                    {
                        _data = await vaultUtility.ProcessFilesAsync(currentTask.Name, _data, appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.Name.Equals("InventorClose"))
                    {
                        await vaultUtility.CloseAllInventorAsync(TaskProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.TaskGroup.Equals("Item"))
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




                //foreach (MaJTask t in MaJTasks.SelectMany(x => x.SubTasks).Where(y => y.IsChecked == true).OrderBy(y => y.Index))
                //{
                //    currentTask = t;
                //    currentTask.ProcessFeedback.Clear();

                //    if (TaskCancellationToken.IsCancellationRequested)
                //    {
                //        currentTask.ProcessingState = StateEnum.Canceled;
                //        continue;
                //    }

                //    TaskProgReport.ProgressChanged += ShowTaskProgress;
                //    ProcessProgReport.ProgressChanged += ShowProcessProgress;

                //    currentTask.ProcessingState = StateEnum.Processing;

                //    if (currentTask.Parent.Name.Equals("Vault") ||currentTask.Parent.Name.Equals("Inventor"))
                //    {
                //        currentTask.IsIndeterminate = true;
                //        if (currentTask.Name.Equals("Connect"))
                //        {
                //            vaultUtility.VaultConnection = await vaultUtility.ConnectToVaultAsync(appOptions, TaskProgReport, TaskCancellationToken);
                //            if (vaultUtility.VaultConnection == null)
                //            {
                //                currentTask.ProcessingState = StateEnum.Error;
                //                return;
                //            }
                //            else
                //            {
                //                currentTask.ProcessingState = StateEnum.Completed;
                //            }
                //        }
                //        else if (currentTask.Name.Equals("ReadVaultConfig"))
                //        {
                //            vaultUtility.VaultConfig = await vaultUtility.ReadVaultConfigAsync(appOptions, TaskProgReport, TaskCancellationToken);
                //            vaultUtility.VaultConfig.FolderPathToFolderDico = await vaultUtility.GetTargetVaultFoldersAsync(_data, TaskProgReport, TaskCancellationToken);
                //        }
                //        else if (currentTask.Name.Equals("ReadInventorConfig"))
                //        {
                //            vaultUtility.VaultConfig.InventorMaterials = await vaultUtility.GetInventorMaterialAsync(appOptions, TaskProgReport, TaskCancellationToken);
                //        }
                //        currentTask.IsIndeterminate = false;
                //    }
                //    else if (currentTask.TaskGroup.Equals("File"))
                //    {
                //        _data = await vaultUtility.ProcessFilesAsync(currentTask.Name, _data, appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                //    }
                //    else if (currentTask.TaskGroup.Equals("File"))
                //    {
                //        _data = await vaultUtility.ProcessFilesAsync(currentTask.Name, _data, appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                //    }
                //    else if (currentTask.TaskGroup.Equals("Item"))
                //    {
                //        _data = await vaultUtility.ProcessItemsAsync(currentTask.Name, _data, appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                //    }

                //    if (TaskCancellationToken.IsCancellationRequested) currentTask.ProcessingState = StateEnum.Canceled;
                //    else if (currentTask.ElementErrorCount > 0) currentTask.ProcessingState = StateEnum.Error;
                //    else currentTask.ProcessingState = StateEnum.Completed;


                //    if (appOptions.ProcessingBehaviour == ProcessingBehaviourEnum.FinishTask && currentTask.ProcessingState == StateEnum.Error)
                //    {
                //        TaskCancellationTokenSource.Cancel();
                //    }

                //    ProcessProgReport.ProgressChanged -= ShowProcessProgress;
                //    TaskProgReport.ProgressChanged -= ShowTaskProgress;
                //}
            }
            else
            {
                TaskCancellationTokenSource.Cancel();
            }

            Page1.Visibility = Visibility.Collapsed;
            Page2.Visibility = Visibility.Collapsed;
            Page3.Visibility = Visibility.Visible;
            
            _data.SaveToSQLite(_dbFileName);

            DoneList.ItemsSource = CollectSelectedTasks(MaJTasks).OrderBy(x => x.Index).ToList();
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
            foreach (MaJTask t in CollectSelectedTasks(MaJTasks).OrderBy(x => x.Index))
            {

                Report += Environment.NewLine + t.DisplayName + ";" + t.ProcessingState + ";" + t.ElementCount + ";" + t.TotalElementCount + ";" + t.ElementDoneCount + ";" + t.ElementErrorCount + ";" + t.TaskDuration;
            }

            System.IO.File.WriteAllText(ReportName, Report);

            Close();
        }


        IEnumerable<MaJTask> CollectSelectedTasks(ObservableCollection<MaJTask> tasks)
        {
            if (tasks != null)
            {
                foreach (MaJTask task in tasks)
                {
                    if (task.IsChecked == true && !task.IsGroup) yield return task;

                    foreach (MaJTask subtask in CollectSelectedTasks(task.SubTasks))
                        if (subtask.IsChecked == true) yield return subtask;
                }
            }
        }
    }

    public class MaJTask : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string TaskGroup { get; set; }
        public bool IsGroup { get; set; } = false;
        public bool IsTaskCheckBoxEnable { get; set; } = false; 
        public string DisplayName { get; set; }
        public bool? IsChecked 
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
        private bool? _isChecked = false;

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


        public ObservableCollection<MaJTask> SubTasks { get; set; }

        public bool IsIndeterminate
        {
            get
            {
                return _isIndeterminate;
            }
            set
            {
                _isIndeterminate = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isIndeterminate = false;

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
