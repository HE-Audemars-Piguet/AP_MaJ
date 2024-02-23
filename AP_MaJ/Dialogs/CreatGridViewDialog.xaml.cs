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
    public partial class CreatGridViewDialog : ThemedWindow
    {
        public ObservableCollection<string> GridViewList;

        public CreatGridViewDialog(ObservableCollection<string> ExistingGridViewList)
        {
            GridViewList = ExistingGridViewList;
            InitializeComponent();
        }

        private void TextEdit_Validate(object sender, ValidationEventArgs e)
        {
            if(e.Value == null || string.IsNullOrWhiteSpace(e.Value.ToString()) || GridViewList.Contains(e.Value.ToString()))
            {
                e.IsValid = false;
            }
            SaveNewGridViewButton.IsEnabled = e.IsValid;
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
    }
}
