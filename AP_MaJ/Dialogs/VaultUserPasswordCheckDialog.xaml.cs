using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


namespace CH.Hurni.AP_MaJ.Dialogs
{
    /// <summary>
    /// Interaction logic for CreatGridViewDialog.xaml
    /// </summary>
    public partial class VaultUserPasswordCheckDialog : ThemedWindow
    {
        public string Server { get; set; } = string.Empty;
        public string Vault { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public VaultUserPasswordCheckDialog(string vaultServer, string vaultName, string vaultUser)
        {
            Server = vaultServer;
            
            Vault = vaultName;
            User = vaultUser;
            Password = string.Empty;

            DataContext = this;

            InitializeComponent();
            
            if (Server.ToLower().Contains("vltp")) WarningIcon.Visibility = Visibility.Visible;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MaxHeight = ActualHeight;
        }
    }
}
