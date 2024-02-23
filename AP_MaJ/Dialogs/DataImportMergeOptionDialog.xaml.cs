using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Ch.Hurni.AP_MaJ.Dialogs
{
    /// <summary>
    /// Interaction logic for DataImportMergeOptionDialog.xaml
    /// </summary>
    public partial class DataImportMergeOptionDialog : ThemedWindow
    {
        #region Property
        public string MergeOption { get; set; } = "Replace";
        #endregion
        public DataImportMergeOptionDialog(string Text)
        {
            InitializeComponent();

            Message.Text = Text;
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            MergeOption = "Replace";
            Close();
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            MergeOption = "AddNew";
            Close();
        }

        private void UpdateAddNew_Click(object sender, RoutedEventArgs e)
        {
            MergeOption = "UpdateAddNew";
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MergeOption = "Cancel";
            Close();
        }
    }
}
