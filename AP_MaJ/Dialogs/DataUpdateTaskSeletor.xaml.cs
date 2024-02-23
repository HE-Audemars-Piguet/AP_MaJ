using AP_MaJ.Utilities;
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
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
    public partial class DataUpdateTaskSeletor : ThemedWindow
    {
        public ObservableCollection<MaJTask> MaJTasks { get; set; }

        public ObservableCollection<string> MaJToDoTasks { get; set; }

        public Progress<TaskProgressReport> TaskProgReport { get; set; }

        public Progress<ProcessProgressReport> ProcessProgReport { get; set; }

        public DataSet Data
        {
            get
            {
                return _data;
            }
        }
        private DataSet _data;


        private ApplicationOptions appOptions;
        private VaultUtility vaultUtility;
        private MaJTask currentTask = null;
        private DispatcherTimer dTimer;
        private DateTime dTimerStartTime;

        public DataUpdateTaskSeletor(ref DataSet data, ApplicationOptions appOptions)
        {
            _data = data;

            this.appOptions = appOptions;
            this.vaultUtility = new VaultUtility();

            MaJTasks = new ObservableCollection<MaJTask>();

            MaJTask FileTask = new MaJTask() { Name = "File", DisplayName = "Taches de mise à jour des fichiers", IsChecked = true, Index = 0 };
            FileTask.SubTasks = new ObservableCollection<MaJTask>();
            FileTask.SubTasks.Add(new MaJTask() { Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = true, Index = 1, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = false, Index = 2, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "PurgeProps", DisplayName = "Purge des propriétés", IsChecked = false, Index = 3, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "Update", DisplayName = "Mise à jour", IsChecked = false, Index = 4, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "PropSync", DisplayName = "Synchronisation des propriétés", IsChecked = false, Index = 5, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "CreateBomBlob", DisplayName = "Créer les BOM blob", IsChecked = false, Index = 6, Parent = FileTask });
            FileTask.SubTasks.Add(new MaJTask() { Name = "WaitForBomBlob", DisplayName = "Attendre et forcer la création des BOM blob", IsChecked = false, Index = 7, Parent = FileTask });
            MaJTasks.Add(FileTask);

            MaJTask ItemTask = new MaJTask() { Name = "Item", DisplayName = "Taches de mise à jour des articles", IsChecked = false, Index = 100 };
            ItemTask.SubTasks = new ObservableCollection<MaJTask>();
            ItemTask.SubTasks.Add(new MaJTask() { Name = "Validate", DisplayName = "Validation des données dans Vault", IsChecked = false,Index = 101, Parent = ItemTask });
            ItemTask.SubTasks.Add(new MaJTask() { Name = "ChangeState", DisplayName = "Changement d'état vers l'état temporaire", IsChecked = false, Index = 102, Parent = ItemTask });
            ItemTask.SubTasks.Add(new MaJTask() { Name = "PurgeProps", DisplayName = "Purge des propriétés", IsChecked = false, Index = 103, Parent = ItemTask });
            ItemTask.SubTasks.Add(new MaJTask() { Name = "Update", DisplayName = "Mise à jour", IsChecked = false, Index = 104, Parent = ItemTask });
            ItemTask.SubTasks.Add(new MaJTask() { Name = "PropSync", DisplayName = "Synchronisation des propriétés", IsChecked = false, Index = 105, Parent = ItemTask });
            MaJTasks.Add(ItemTask);

            MaJToDoTasks = new ObservableCollection<string>();

            DataContext = this;
            InitializeComponent();

            TaskProgReport = new Progress<TaskProgressReport>();
            TaskProgReport.ProgressChanged += ShowTaskProgress;

            ProcessProgReport = new Progress<ProcessProgressReport>();
            ProcessProgReport.ProgressChanged += ShowProcessProgress;
        }

        private void ShowTaskProgress(object sender, TaskProgressReport e)
        {
            if(e.Timer != null)
            {
                if (e.Timer.Equals("Start"))
                {
                    dTimer = new DispatcherTimer();
                    dTimer.Interval = TimeSpan.FromSeconds(1);
                    dTimer.Tick += timer_Tick;
                    
                    dTimerStartTime = DateTime.Now;
                    currentTask.TaskDuration = "00:00";

                    dTimer.Start();                   
                }
                else if (e.Timer.Equals("Stop"))
                {
                    dTimer.Stop();

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

            currentTask.ProcessFeedback[e.ProcessIndex] = e.ProcessFeedbackMessage;

            if (e.ProcessHasError == null) currentTask.ElementCount++;

            if (e.ProcessHasError == false) currentTask.ElementDoneCount++;
            else if (e.ProcessHasError == true) currentTask.ElementErrorCount++;
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Page1.Visibility = Visibility.Collapsed;
            Page2.Visibility = Visibility.Visible;
            Page3.Visibility = Visibility.Collapsed;

            NextButton.Visibility = Visibility.Collapsed;
            ExecutButton.Visibility = Visibility.Visible;
            ExecutButton.IsDefault = true;
            BackButton.Visibility = Visibility.Visible;
            FinishButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Visible;

            ToDoList.ItemsSource = MaJTasks.SelectMany(x=>x.SubTasks).Where(y => y.IsChecked == true).ToList();
        }

        private async void ExecutButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonsStackPanel.IsEnabled = false;

            foreach (MaJTask t in MaJTasks.SelectMany(x => x.SubTasks).Where(y => y.IsChecked == true).OrderBy(y => y.Index))
            {
                currentTask = t;
                currentTask.ProcessFeedback.Clear();
                //for (int i = 0; i < appOptions.SimultaniousValidationProcess; i++) currentTask.ProcessFeedback.Add("");

                currentTask.ProcessingState = StateEnum.Processing;

                if (currentTask.Parent.Name.Equals("File")) _data = await vaultUtility.ProcessFilesAsync(currentTask.Name, _data, appOptions, TaskProgReport, ProcessProgReport);

                currentTask.ProcessingState = StateEnum.Completed;
            }

            ButtonsStackPanel.IsEnabled = true;

            Page1.Visibility = Visibility.Collapsed;
            Page2.Visibility = Visibility.Collapsed;
            Page3.Visibility = Visibility.Visible;

            NextButton.Visibility = Visibility.Collapsed;
            ExecutButton.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Collapsed;
            FinishButton.Visibility = Visibility.Visible;
            FinishButton.IsDefault = true;
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Page1.Visibility = Visibility.Visible;
            Page2.Visibility = Visibility.Collapsed;
            Page3.Visibility = Visibility.Collapsed;

            NextButton.Visibility = Visibility.Visible;
            NextButton.IsDefault = true;
            ExecutButton.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Collapsed;
            FinishButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
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
        private long _totalElementCount = 0;
        
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

            return sb.ToString();
        }
    }
}
