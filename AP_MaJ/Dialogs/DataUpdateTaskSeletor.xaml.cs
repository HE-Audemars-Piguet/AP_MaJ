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
using System.IO.Compression;
using System.Data.SQLite;
using System.Text.Json;
using Ch.Hurni.AP_MaJ;

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

        public ApplicationOptions AppOptions
        {
            get
            {
                return _appOptions;
            }
        }
        private ApplicationOptions _appOptions;
        
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
            _invDispatcher = new InventorDispatcher(appOptions);
            
            _appOptions = appOptions;
            vaultUtility = new VaultUtility(_invDispatcher);
            
            MaJTasks = new ObservableCollection<MaJTask>();

            MaJTask VaultConnectTask = new MaJTask() { TaskGroup="Vault", Name = "VaultConnect", DisplayName = "Connexion au Vault", IsChecked = true, Index = 0, IsIndeterminate = true };
            MaJTasks.Add(VaultConnectTask);

            MaJTask ReadVaultConfigTask = new MaJTask() { TaskGroup = "Vault", Name = "ReadVaultConfig", DisplayName = "Lecture de la configuration Vault", IsChecked = true, Index = 1, IsIndeterminate = true };
            MaJTasks.Add(ReadVaultConfigTask);

            MaJTask CreateVaultFolderTask = new MaJTask() { TaskGroup = "Vault", Name = "CreateVaultFolder", DisplayName = "Création des nouveaux dossiers Vault", IsChecked = true, Index = 2, IsIndeterminate = true };
            MaJTasks.Add(CreateVaultFolderTask);

            MaJTask ReadInventorConfigTask = new MaJTask() { TaskGroup = "Inventor", Name = "ReadInventorConfig", DisplayName = "Lecture de la configuration Inventor", IsChecked = true, Index = 10, IsIndeterminate = true };
            MaJTasks.Add(ReadInventorConfigTask);

            MaJTask FileTask = new MaJTask() { IsGroup = true, TaskGroup = "File", Name = "File", DisplayName = "Tâches de mise à jour des fichiers", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 20 };
            FileTask.SubTasks = new ObservableCollection<MaJTask>();
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 21});
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 22 });
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "PurgeProps", DisplayName = "Ajout/suppression des propriétés", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 23 });
            FileTask.SubTasks.Add(new MaJTask() { TaskGroup = "File", Name = "Update", DisplayName = "Mise à jour", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 24 });
            MaJTasks.Add(FileTask);

            MaJTask InventorCloseTask = new MaJTask() { TaskGroup = "Inventor", Name = "InventorClose", DisplayName = "Fermeture des sessions Inventor", IsChecked = true, Index = 30, IsIndeterminate = true };
            MaJTasks.Add(InventorCloseTask);

            MaJTask WaitForBomBlob = new MaJTask() { TaskGroup = "File", Name = "WaitForBomBlob", DisplayName = "Attendre et forcer la création des BOM blob", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 40 };
            MaJTasks.Add(WaitForBomBlob);

            MaJTask ItemTask = new MaJTask() { IsGroup = true, TaskGroup = "Item", Name = "Item", DisplayName = "Tâches de mise à jour des articles", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 50 };
            ItemTask.SubTasks = new ObservableCollection<MaJTask>();
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 51 });
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 52 });
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "PurgeProps", DisplayName = "Ajout/suppression des propriétés", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 53 });
            ItemTask.SubTasks.Add(new MaJTask() { TaskGroup = "Item", Name = "Update", DisplayName = "Mise à jour", IsChecked = true, IsTaskCheckBoxEnable = true, Index = 54 });
            MaJTasks.Add(ItemTask);

            if(AppOptions.LastTaskUsed.Count > 0)
            {
                foreach(LastTaskOption lastOpt in AppOptions.LastTaskUsed)
                {
                    MaJTask maJTask = MaJTasks.Select(x => x).Union(MaJTasks.Where(x => x.SubTasks != null).SelectMany(x => x.SubTasks)).Where(x => x.Index == lastOpt.Index).FirstOrDefault();
                    if(maJTask != null)
                    { 
                        maJTask.IsChecked = lastOpt.IsChecked;
                        maJTask.IsTaskCheckBoxEnable = false;
                    }
                }
            }

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

        DateTime lastExection = DateTime.MinValue;
        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            if((now - lastExection).TotalSeconds >= 1)
            {
                currentTask.TaskDuration = currentTask.FormatTimeSpan(now.Subtract(dTimerStartTime));
                lastExection = now;
            }            
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
                if (_appOptions.ProcessingBehaviour == ProcessingBehaviourEnum.Stop) TaskCancellationTokenSource.Cancel();
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
                AppOptions.LastTaskUsed.Clear();
                foreach (MaJTask majt in MaJTasks.Select(x => x).Union(MaJTasks.Where(x => x.SubTasks != null).SelectMany(x => x.SubTasks)))
                {
                    AppOptions.LastTaskUsed.Add(new LastTaskOption() { Index = majt.Index, IsChecked = majt.IsChecked });
                }
                System.IO.File.WriteAllText((Owner as MainWindow).ActiveProjectName, JsonSerializer.Serialize(AppOptions, typeof(ApplicationOptions), (Owner as MainWindow).JsonOptions));
                
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

                    IProgress<TaskProgressReport> taskProgReport = TaskProgReport as IProgress<TaskProgressReport>;

                    if (currentTask.Name.Equals("VaultConnect"))
                    {
                        while(true)
                        {
                            VaultUserPasswordCheckDialog pwdCheckDialog = new VaultUserPasswordCheckDialog(AppOptions.VaultServer, AppOptions.VaultName, AppOptions.VaultUser);
                            pwdCheckDialog.Owner = this;
                            pwdCheckDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                            pwdCheckDialog.ShowDialog();

                            if (pwdCheckDialog.DialogResult == true)
                            {
                                currentTask.ProcessingState = StateEnum.Processing;
                                await Task.Delay(100);
                                taskProgReport.Report(new TaskProgressReport() { Message = "Connection au Vault...", Timer = "Start" });
                                await Task.Delay(100);

                                vaultUtility.VaultConnection = vaultUtility.ConnectToVault(_appOptions, pwdCheckDialog.User, pwdCheckDialog.Password);
                                if (vaultUtility.VaultConnection != null)
                                {
                                    currentTask.ProcessingState = StateEnum.Completed;
                                    await Task.Delay(100);
                                    taskProgReport.Report(new TaskProgressReport() { Message = "Connection au Vault...", Timer = "Stop" });
                                    await Task.Delay(100);

                                    break;
                                }
                                else
                                {
                                    currentTask.ProcessingState = StateEnum.Error;
                                    TaskCancellationTokenSource.Cancel();
                                    await Task.Delay(100);
                                    taskProgReport.Report(new TaskProgressReport() { Message = "Connection au Vault...", Timer = "Stop" });
                                    await Task.Delay(100);
                                    
                                    break;
                                }
                            }
                            else
                            {
                                currentTask.ProcessingState = StateEnum.Canceled;
                                TaskCancellationTokenSource.Cancel();

                                break;
                            }

                            //if (pwdCheckDialog.DialogResult == true)
                            //{
                            //    currentTask.ProcessingState = StateEnum.Processing;
                            //    vaultUtility.VaultConnection = await vaultUtility.ConnectToVaultAsync(_appOptions, TaskProgReport, TaskCancellationToken, pwdCheckDialog.User, pwdCheckDialog.Password);

                            //    if (vaultUtility.VaultConnection != null)
                            //    {
                            //        currentTask.ProcessingState = StateEnum.Completed;
                            //        break;
                            //    }
                            //    else
                            //    {
                            //        currentTask.ProcessingState = StateEnum.Error;
                            //        TaskCancellationTokenSource.Cancel();
                            //        break;
                            //    }
                            //}
                            //else
                            //{
                            //    currentTask.ProcessingState = StateEnum.Error;
                            //    TaskCancellationTokenSource.Cancel();
                            //    break;
                            //}
                        }
                    }
                    else if (currentTask.Name.Equals("ReadVaultConfig"))
                    {
                        currentTask.ProcessingState = StateEnum.Processing;
                        vaultUtility.VaultConfig = await vaultUtility.ReadVaultConfigAsync(_appOptions, TaskProgReport, TaskCancellationToken);
                    }                    
                    else if (currentTask.Name.Equals("CreateVaultFolder"))
                    {
                        currentTask.ProcessingState = StateEnum.Processing;
                        vaultUtility.VaultConfig.FolderPathToFolderDico = await vaultUtility.GetTargetVaultFoldersAsync(_data, TaskProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.Name.Equals("ReadInventorConfig"))
                    {
                        currentTask.ProcessingState = StateEnum.Processing;
                        vaultUtility.VaultConfig.InventorMaterials = await vaultUtility.GetInventorMaterialAsync(_appOptions, TaskProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.TaskGroup.Equals("File"))
                    {
                        currentTask.ProcessingState = StateEnum.Processing;
                        _data = await vaultUtility.ProcessFilesAsync(currentTask.Name, _data, _appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.Name.Equals("InventorClose"))
                    {
                        currentTask.ProcessingState = StateEnum.Processing;
                        await vaultUtility.CloseAllInventorAsync(TaskProgReport, TaskCancellationToken);
                    }
                    else if (currentTask.TaskGroup.Equals("Item"))
                    {
                        currentTask.ProcessingState = StateEnum.Processing;
                        _data = await vaultUtility.ProcessItemsAsync(currentTask.Name, _data, _appOptions, TaskProgReport, ProcessProgReport, TaskCancellationToken);
                    }

                    if (TaskCancellationToken.IsCancellationRequested && currentTask.ProcessingState == StateEnum.Error) currentTask.ProcessingState = StateEnum.Error;
                    else if (TaskCancellationToken.IsCancellationRequested && currentTask.ProcessingState != StateEnum.Error) currentTask.ProcessingState = StateEnum.Canceled;
                    else if (currentTask.ElementErrorCount > 0) currentTask.ProcessingState = StateEnum.Error;
                    else currentTask.ProcessingState = StateEnum.Completed;


                    if (_appOptions.ProcessingBehaviour == ProcessingBehaviourEnum.FinishTask && currentTask.ProcessingState == StateEnum.Error)
                    {
                        TaskCancellationTokenSource.Cancel();
                    }

                    if(currentTask.TaskGroup.Equals("File") || currentTask.TaskGroup.Equals("Item")) _data.SaveToSQLite(_dbFileName);

                    await Task.Delay(100);

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
            string lastTaskName = MaJTasks.Where(x => x.Name.Equals("Item")).FirstOrDefault().SubTasks.Where(x => x.IsChecked == true).OrderBy(x => x.Index).LastOrDefault()?.Name ?? string.Empty;
            TaskTypeEnum lastTaskType = (TaskTypeEnum)Enum.Parse(typeof(TaskTypeEnum), lastTaskName);

            foreach (DataRow dr in _data.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("Item") && x.Field<TaskTypeEnum>("Task") == lastTaskType && x.Field<StateEnum>("State") == StateEnum.Completed))
            {
                dr["State"] = StateEnum.Finished;
            }

            lastTaskName = MaJTasks.Where(x => x.Name.Equals("File")).FirstOrDefault().SubTasks.Where(x => x.IsChecked == true).OrderBy(x => x.Index).LastOrDefault()?.Name ?? string.Empty;
            lastTaskType = (TaskTypeEnum)Enum.Parse(typeof(TaskTypeEnum), lastTaskName);

            bool WaitForBomBlob = MaJTasks.Where(x => x.Name.Equals("WaitForBomBlob")).FirstOrDefault()?.IsChecked ?? false;
            StringComparison sComp = StringComparison.CurrentCultureIgnoreCase;

            foreach (DataRow dr in _data.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File") && x.Field<StateEnum>("State") == StateEnum.Completed))
            {
                if ((WaitForBomBlob && dr.Field<TaskTypeEnum>("Task") == TaskTypeEnum.WaitForBomBlob && (dr.Field<string>("Name").EndsWith(".ipt", sComp) || dr.Field<string>("Name").EndsWith(".iam", sComp))) ||
                   (!WaitForBomBlob && dr.Field<TaskTypeEnum>("Task") == lastTaskType && !(dr.Field<string>("Name").EndsWith(".ipt", sComp) || dr.Field<string>("Name").EndsWith(".iam", sComp))))
                {
                    dr["State"] = StateEnum.Finished;
                }
            }

            string ReportName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_dbFileName), "Statistics.log");

            string Report = "DisplayName;ProcessingState;ElementCount;TotalElementCount;ElementDoneCount;ElementErrorCount;TaskDuration";
            foreach (MaJTask t in CollectSelectedTasks(MaJTasks).OrderBy(x => x.Index))
            {
                Report += Environment.NewLine + t.DisplayName + ";" + t.ProcessingState + ";" + t.ElementCount + ";" + t.TotalElementCount + ";" + t.ElementDoneCount + ";" + t.ElementErrorCount + ";" + t.TaskDuration;
            }
            System.IO.File.WriteAllText(ReportName, Report);

            if(SaveHistory.IsChecked == true)
            {
                string ZipName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_dbFileName), "Archive " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".zip");

                using (System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.Open(ZipName, System.IO.Compression.ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(ReportName, "Statistics.log");
                    
                    int SaveTryCount = 0;
                    while (true)
                    {
                        try
                        {
                            archive.CreateEntryFromFile(_dbFileName, System.IO.Path.GetFileName(_dbFileName));
                            break;
                        }
                        catch 
                        {
                            if(SaveTryCount >= 10)
                            {
                                MessageBox.Show("Impossible de sauver l'historique de la base de données");
                                break;
                            }
                            System.Threading.Thread.Sleep(500);
                            SaveTryCount++;
                        }
                    }
                    
                    archive.CreateEntryFromFile(System.IO.Path.GetDirectoryName(_dbFileName) + ".maj", System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(_dbFileName) + ".maj"));
                }
                System.IO.File.Delete(ReportName);
            }
            
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
