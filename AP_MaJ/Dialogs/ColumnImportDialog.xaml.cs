using DevExpress.Xpf.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
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
    /// Interaction logic for TaskAndProgressWizard.xaml
    /// </summary>
    public partial class ColumnImportDialog : ThemedWindow, INotifyPropertyChanged
    {
        public static readonly DependencyProperty AppBackgroundBrushProperty = DependencyProperty.Register("AppBackgroundBrush", typeof(Brush), typeof(ColumnImportDialog));

        public Brush AppBackgroundBrush
        {
            get
            {
                return (Brush)GetValue(AppBackgroundBrushProperty);
            }
            set
            {
                SetValue(AppBackgroundBrushProperty, value);
            }
        }

        public static readonly DependencyProperty AppForegroundBrushProperty = DependencyProperty.Register("AppForegroundBrush", typeof(Brush), typeof(ColumnImportDialog));

        public Brush AppForegroundBrush
        {
            get
            {
                return (Brush)GetValue(AppForegroundBrushProperty);
            }
            set
            {
                SetValue(AppForegroundBrushProperty, value);
            }
        }

        public DataSet Data
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
        private DataSet _data;


        public List<DataColumn> ColumnList
        {
            get
            {
                return _columnList;
            }
            set
            {
                _columnList = value;
                NotifyPropertyChanged();
            }
        }
        private List<DataColumn> _columnList = null;


        public ColumnImportDialog(/*DataSet ds,*/Brush Foreground, Brush Background, string subTitle = "", ImageSource ico = null)
        {
            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            if (ico != null) Icon = ico;
            else Icon = Application.Current.MainWindow.Icon;

            if (string.IsNullOrWhiteSpace(subTitle)) Title = Application.Current.MainWindow.Title;
            else Title = Application.Current.MainWindow.Title + " - " + subTitle;

            ShowInTaskbar = false;
            AppForegroundBrush = Foreground;
            AppBackgroundBrush = Background;

            //Data = ds;
            DataContext = this;

            this.ContentRendered += ColumnImportDialog_ContentRendered;

            InitializeComponent();
        }

        private void ColumnImportDialog_ContentRendered(object sender, EventArgs e)
        {
            if(Data != null && Data.Tables.Count > 0) 
            {
                TableSelector.EditValue = Data.Tables[0];
            }
        }


        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void Import_Click(object sender, DevExpress.Xpf.Bars.ItemClickEventArgs e)
        {
            ColumnList = (TableSelector.EditValue as DataTable).Columns.Cast<DataColumn>().ToList();
            Close();
        }
    }

    public class TabDataItem
    {
        public string PageText { get; set; }
        public string HeaderText { get; set; }
    }
}
