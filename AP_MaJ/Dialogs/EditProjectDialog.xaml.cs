using AP_MaJ;
using AP_MaJ.Utilities;
using Ch.Hurni.AP_MaJ.Classes;
using CH.Hurni.AP_MaJ.Dialogs;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
    /// Interaction logic for EditProjectDialog.xaml
    /// </summary>
    public partial class EditProjectDialog : ThemedWindow, INotifyPropertyChanged
    {
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

        public EditProjectDialog()
        {
            DataContext = this;

            InitializeComponent();
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private VaultUtility vaultUtility;
        private CancellationTokenSource TaskCancellationTokenSource;

        public Progress<TaskProgressReport> TaskProgReport { get; set; }

        public Progress<ProcessProgressReport> ProcessProgReport { get; set; }

        private async void Mapping_Edit_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            //App.AppProgressSplash.Show(this);


            this.vaultUtility = new VaultUtility();

            TaskCancellationTokenSource = new CancellationTokenSource();
            CancellationToken TaskCancellationToken = TaskCancellationTokenSource.Token;

            vaultUtility.VaultConnection = await vaultUtility.ConnectToVaultAsync(AppOptions, null, TaskCancellationToken);
            vaultUtility.VaultConfig = await vaultUtility.ReadVaultConfigAsync(AppOptions, null, TaskCancellationToken);

            PropertyMappingEditDialog PropMappingEdit = new PropertyMappingEditDialog(AppOptions.VaultPropertyFieldMappings, vaultUtility.VaultConfig);

            //App.AppProgressSplash.Close();
            this.IsEnabled = true;

            PropMappingEdit.Owner = this;
            PropMappingEdit.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            PropMappingEdit.ShowDialog();

            if (PropMappingEdit.DialogResult == true)
            {
                AppOptions.UpdatePropertyMappings(PropMappingEdit.Mappings.Where(x => x.IsSelected == true).ToList());
                

                //ApplicationOptions.RootWindow.BatchEditorControl.RefreshGridColumns(AppOptions.BatchEditor.BatchEditorGridCustomColumns);
            }
        }
    }
}
