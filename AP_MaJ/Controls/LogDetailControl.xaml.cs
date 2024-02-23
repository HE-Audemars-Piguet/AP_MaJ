using Ch.Hurni.AP_MaJ.Classes;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ch.Hurni.AP_MaJ.Controls
{
    /// <summary>
    /// Interaction logic for PropertyDetailControl.xaml
    /// </summary>
    public partial class LogDetailControl : UserControl, INotifyPropertyChanged
    {
        #region Properties
        public DataRow[] Logs
        {
            get
            {
                return _logs;
            }
            set
            {
                _logs = value;
                NotifyPropertyChanged();
            }
        }
        private DataRow[] _logs = null;
        #endregion


        #region BindableProperties
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(DataRowView), typeof(LogDetailControl), new PropertyMetadata(null, SelectedItemValue_Changed));
        public DataRowView SelectedItem
        {
            get { return (DataRowView)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        private static void SelectedItemValue_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LogDetailControl currentCtrl = (LogDetailControl)d;

            if (e.NewValue == null)
            {
                currentCtrl.LogsGridControl.ItemsSource = null;
                return;
            }

            //currentCtrl.Props.Rows.Clear();

            DataRow SelectedEntityData = (e.NewValue as DataRowView).Row;
            currentCtrl.Logs = SelectedEntityData.GetChildRows("EntityLogs");
            currentCtrl.LogsGridControl.ItemsSource = currentCtrl.Logs;

            //foreach(DataColumn col in SelectedEntityData.Table.Columns)
            //{
            //    DataRow NewPropRow = currentCtrl.Props.NewRow();
            //    NewPropRow["Group"] = "System property";
            //    NewPropRow["Name"] = col.ColumnName;
            //    NewPropRow["Value"] = SelectedEntityData[col.ColumnName].ToString();
            //    currentCtrl.Props.Rows.Add(NewPropRow);
            //}

            //DataRow SelectedEntityProps = SelectedEntityData.GetChildRows("EntityNewProp").FirstOrDefault();
            //foreach (DataColumn col in SelectedEntityProps.Table.Columns.Cast<DataColumn>().Where(x => !x.ColumnName.Equals("EntityId")))
            //{
            //    DataRow NewPropRow = currentCtrl.Props.NewRow();
            //    NewPropRow["Group"] = "User property";
            //    NewPropRow["Name"] = col.ColumnName;
            //    NewPropRow["Value"] = SelectedEntityProps[col.ColumnName].ToString();
            //    currentCtrl.Props.Rows.Add(NewPropRow);
            //}

            //currentCtrl.PropGridControl.ItemsSource = currentCtrl.Props;
        }
        #endregion


        public LogDetailControl()
        {
            InitializeComponent();

            //this.DataContext = this;
        }


        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        private void PropGridControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if ((e.OriginalSource as GridControl).Columns.Count > 0) return;

            //GridColumn newGridCol;

            //foreach (DataColumn dc in Props.Columns)
            //{
            //    if (dc.ColumnName.Equals("Group"))
            //    {
            //        newGridCol = new GridColumn() { Header = dc.ColumnName, FieldName = dc.ColumnName, GroupIndex = 0, Visible = true };
            //    }
            //    else if (dc.ColumnName.Equals("NewValue"))
            //    {
            //        //if (this.Tag.ToString().Equals("BatchEditor") || this.Tag.ToString().Equals("ItemImport"))
            //        //{
            //        //    newGridCol = new GridColumn() { Header = dc.ColumnName, FieldName = dc.ColumnName, Visible = false };
            //        //}
            //        //else
            //        //{
            //            newGridCol = new GridColumn() { Header = dc.ColumnName, FieldName = dc.ColumnName, Visible = true };
            //        //}
            //    }
            //    else
            //    {
            //        newGridCol = new GridColumn() { Header = dc.ColumnName, FieldName = dc.ColumnName, Visible = true };
            //    }

            //    newGridCol.Tag = dc.ColumnName;
            //    newGridCol.UnboundType = DevExpress.Data.UnboundColumnType.String;

            //    (e.OriginalSource as GridControl).Columns.Add(newGridCol);
            //    (e.OriginalSource as GridControl).GroupBy("Group");
            //    (e.OriginalSource as GridControl).AutoExpandAllGroups = true;
            //}
        }

        private void PropGridControl_CustomUnboundColumnData(object sender, DevExpress.Xpf.Grid.GridColumnDataEventArgs e)
        {
            //if (e.IsGetData)
            //{
            //    if (e.Column.Tag != null)
            //    {
            //        string Tag = e.Column.Tag.ToString();
            //        string EnumType = string.Empty;
            //        List<string> Ref = new List<string>();

            //        int index = Tag.IndexOf("]");

            //        if (index > -1)
            //        {
            //            EnumType = Tag.Substring(1, index - 1);
            //            Tag = Tag.Substring(index + 1);
            //        }

            //        Ref = Tag.Split('|').ToList();

            //        DataRow dr = ((GridControl)sender).GetRowByListIndex(e.ListSourceRowIndex) as DataRow;

            //        if (Ref.Count == 1)
            //        {
            //            if (!string.IsNullOrEmpty(EnumType)) e.Value = ConvertToEnum(dr[Ref[0]], EnumType);
            //            else e.Value = dr[Ref[0]];
            //        }
            //        else if (Ref.Count == 2)
            //        {
            //            if (Ref[0].Equals("StringLength")) e.Value = dr[Ref[1]].ToString().Length;
            //            ////else if (Ref[0].Equals("PathDepth")) e.Value = Pri.LongPath.Path.GetDirectoryName(dr[Ref[1]].ToString()).Split(new char[] { '/', '\\' }).Count() - 1;
            //        }
            //        else if (Ref.Count == 3)
            //        {
            //            if (Ref[0].Equals("GetChildRows") && dr.GetChildRows(Ref[1]).Count() > 0)
            //            {
            //                if (string.IsNullOrEmpty(EnumType)) e.Value = dr.GetChildRows(Ref[1]).FirstOrDefault()[Ref[2]];
            //                else e.Value = ConvertToEnum(dr.GetChildRows(Ref[1]).FirstOrDefault()[Ref[2]], EnumType);
            //            }
            //        }
            //    }
            //}
        }

        private string ConvertToEnum(object val, string enumTypeName)
        {
            //if (enumTypeName.Equals("FileStateEnum")) return ((FileStateEnum)Enum.Parse(typeof(FileStateEnum), val.ToString())).ToString();
            //else if (enumTypeName.Equals("FileProviderEnum")) return ((FileProviderEnum)Enum.Parse(typeof(FileProviderEnum), val.ToString())).ToString();
            //else if (enumTypeName.Equals("RefStateEnum")) return ((RefStateEnum)Enum.Parse(typeof(RefStateEnum), val.ToString())).ToString();
            //else if (enumTypeName.Equals("FolderTypeEnum")) return ((FolderTypeEnum)Enum.Parse(typeof(FolderTypeEnum), val.ToString())).ToString();
            //else if (enumTypeName.Equals("FolderStateEnum")) return ((FolderStateEnum)Enum.Parse(typeof(FolderStateEnum), val.ToString())).ToString();
            //else if (enumTypeName.Equals("FileRenameEnum")) return ((FileRenameEnum)Enum.Parse(typeof(FileRenameEnum), val.ToString())).ToString();
            //else if (enumTypeName.Equals("FileSelectionEnum")) return ((FileSelectionEnum)Enum.Parse(typeof(FileSelectionEnum), val.ToString())).ToString();
            //else return val.ToString();
            return val.ToString();
        }
    }
}
