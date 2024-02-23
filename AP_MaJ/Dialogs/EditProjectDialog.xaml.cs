using Ch.Hurni.AP_MaJ.Classes;
using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        private void Mapping_Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FieldName_Validate(object sender, DevExpress.Xpf.Grid.GridCellValidationEventArgs e)
        {

        }
    }
}
