using AP_MaJ.Properties;
using Ch.Hurni.AP_MaJ.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using static Ch.Hurni.AP_MaJ.Classes.ApplicationOptions;

namespace Ch.Hurni.AP_MaJ.Utilities
{
    public static class DataSetUtility
    {

        private const string ConnectionString = "Data Source={0}; FailIfMissing=False";
        
        internal static DataSet CreateDataSet(ObservableCollection<PropertyFieldMapping> vaultPropertyFieldMappings)
        {
            DataSet ds = new DataSet();

            DataTable dtEntities = new DataTable("Entities");
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Id", DataType = typeof(long), AutoIncrement = true, AutoIncrementSeed = 1, AutoIncrementStep = 1, AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Task", DataType = typeof(TaskTypeEnum), AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "State", DataType = typeof(StateEnum), AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "JobSubmitCount", DataType = typeof(int), DefaultValue = null, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "EntityType", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = false });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Name", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultMasterId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Path", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultFolderId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultNumSchName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultNumSchId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultPath", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultFolderId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultCatName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultCatId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultCatName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultCatId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcsName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcsId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TempVaultLcsName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TempVaultLcsId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcsName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcsId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevSchName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevSchId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevSchName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevSchId", DataType = typeof(long), DefaultValue = null, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevLabel", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            //dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevLabel", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            //dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.PrimaryKey = new List<DataColumn>() { dtEntities.Columns["Id"] }.ToArray();
            ds.Tables.Add(dtEntities);


            DataTable dtNewProps = new DataTable("NewProps");
            dtNewProps.Columns.Add(new DataColumn() { ColumnName = "EntityId", DataType = typeof(long), AllowDBNull = false });
            foreach (string Field in vaultPropertyFieldMappings.Select(x => x.FieldName).Distinct())
            {
                dtNewProps.Columns.Add(new DataColumn() { ColumnName = Field, DataType = typeof(string), AllowDBNull = true });
            }

            dtNewProps.PrimaryKey = new List<DataColumn>() { dtNewProps.Columns["EntityId"] }.ToArray();
            ds.Tables.Add(dtNewProps);


            DataTable dtLogs = new DataTable("Logs");
            dtLogs.Columns.Add(new DataColumn() { ColumnName = "EntityId", DataType = typeof(long), AllowDBNull = false });
            dtLogs.Columns.Add(new DataColumn() { ColumnName = "Severity", DataType = typeof(string), AllowDBNull = false });
            dtLogs.Columns.Add(new DataColumn() { ColumnName = "Date", DataType = typeof(DateTime), AllowDBNull = false });
            dtLogs.Columns.Add(new DataColumn() { ColumnName = "Message", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = false });
            ds.Tables.Add(dtLogs);


            ds.Relations.Add("EntityNewProp", ds.Tables["Entities"].Columns["Id"], ds.Tables["NewProps"].Columns["EntityId"]);
            ds.Relations.Add("EntityLogs", ds.Tables["Entities"].Columns["Id"], ds.Tables["Logs"].Columns["EntityId"]);

            return ds;
        }

        public static void ReadFromSQLite(this DataSet ds, string dbFilePath)
        {
            using (SQLiteConnection Conn = new SQLiteConnection(string.Format(ConnectionString, dbFilePath)))
            {
                Conn.Open();
                //SELECT name FROM sqlite_master WHERE type='table' AND name='{table_name}'

                SQLiteDataAdapter DtA = new SQLiteDataAdapter();

                foreach (DataTable dt in ds.Tables)
                {
                    SQLiteCommand Cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='" + dt.TableName + "'", Conn);
                    if (Cmd.ExecuteScalar() != null)
                    {
                        DtA.SelectCommand = new SQLiteCommand("SELECT * FROM [" + dt.TableName + "]", Conn);
                        DtA.Fill(dt);
                    }
                }

                Conn.Close();
            }

            ds.AcceptChanges();
        }

        public static async Task<DataSet> ReadFromSQLiteAsync(this DataSet ds, string dbFilePath)
        {
            DataSet ReturnDataSet = ds.Clone();

            using (SQLiteConnection Conn = new SQLiteConnection(string.Format(ConnectionString, dbFilePath)))
            {
                Conn.Open();

                SQLiteDataAdapter DtA = new SQLiteDataAdapter();

                foreach (DataTable dt in ReturnDataSet.Tables)
                {
                    SQLiteCommand Cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='" + dt.TableName + "'", Conn);
                    if (Cmd.ExecuteScalar() != null)
                    {
                        DtA.SelectCommand = new SQLiteCommand("SELECT * FROM [" + dt.TableName + "]", Conn);
                        await Task.Run(() => DtA.Fill(dt));
                    }
                }

                Conn.Close();
            }

            ReturnDataSet.AcceptChanges();

            return ReturnDataSet;
        }

        public static void SaveToSQLite(this DataSet ds, string dbFilePath)
        {
            using (SQLiteConnection Conn = new SQLiteConnection(string.Format(ConnectionString, dbFilePath)))
            {
                Conn.Open();
                using (SQLiteTransaction Transaction = Conn.BeginTransaction())
                {
                    SQLiteCommand Cmd = new SQLiteCommand();

                    foreach (DataTable dt in ds.Tables)
                    {

                        Cmd = new SQLiteCommand("DROP TABLE IF EXISTS [" + dt.TableName + "]", Conn);
                        Cmd.ExecuteNonQuery();

                        string CreateTableQuery = "CREATE TABLE IF NOT EXISTS [" + dt.TableName + "] (";
                        List<string> ColNames = new List<string>();
                        List<DbType> ColTypes = new List<DbType>();
                        List<string> CreateTableFields = new List<string>();
                        List<string> PrimaryFields = new List<string>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            string Field = "[" + col.ColumnName + "] " + ConvertToSQLiteDbType(col.DataType);
                            //if (dt.PrimaryKey.Contains<DataColumn>(col)) Field += " PRIMARY KEY";
                            if (!col.AllowDBNull) Field += " NOT NULL";

                            ColNames.Add(col.ColumnName);
                            ColTypes.Add(ConvertToSQLiteParamType(col.DataType));
                            CreateTableFields.Add(Field);
                        }

                        foreach (DataColumn PrimaryCol in dt.PrimaryKey)
                        {
                            PrimaryFields.Add(PrimaryCol.ColumnName);
                        }
                        string Primary = "";
                        if (PrimaryFields.Count > 0)
                        {
                            Primary = ", PRIMARY KEY (" + string.Join(",", PrimaryFields) + ")";
                        }

                        Cmd = new SQLiteCommand("CREATE TABLE IF NOT EXISTS [" + dt.TableName + "] (" + String.Join(",", CreateTableFields) + Primary + ")", Conn);
                        Cmd.ExecuteNonQuery();

                        if (dt.Rows.Count > 0)
                        {
                            Cmd = new SQLiteCommand("INSERT INTO [" + dt.TableName + "] (" + String.Join(", ", ColNames.Select(x => "[" + x + "]")) + ") VALUES (" + String.Join(", ", ColNames.Select(x => "@Param" + ColNames.IndexOf(x))) + ")", Conn);
                            foreach (DataRow dr in dt.Rows)
                            {
                                for (int i = 0; i < ColNames.Count; i++)
                                {
                                    Cmd.Parameters.Add(new SQLiteParameter()
                                    {
                                        ParameterName = "@Param" + i,
                                        DbType = ColTypes[i],
                                        SourceColumn = ColNames[i],
                                        Value = dr[ColNames[i]]
                                    });
                                }
                                Cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    Transaction.Commit();
                }
                Conn.Close();

                ds.AcceptChanges();
            }
        }

        public static string ConvertToSQLiteDbType(Type dataType)
        {
            if (dataType == typeof(Boolean)) return "INTEGER";
            else if (dataType == typeof(Byte)) return "INTEGER";
            else if (dataType == typeof(Byte[])) return "BLOB";
            else if (dataType == typeof(Char)) return "TEXT";
            //else if (dataType == typeof(DateOnly)) return "TEXT";
            else if (dataType == typeof(DateTime)) return "TEXT";
            else if (dataType == typeof(DateTimeOffset)) return "TEXT";
            else if (dataType == typeof(Decimal)) return "TEXT";
            else if (dataType == typeof(Double)) return "REAL";
            else if (dataType == typeof(Guid)) return "TEXT";
            else if (dataType == typeof(Int16)) return "INTEGER";
            else if (dataType == typeof(Int32)) return "INTEGER";
            else if (dataType == typeof(Int64)) return "INTEGER";
            else if (dataType == typeof(SByte)) return "INTEGER";
            else if (dataType == typeof(Single)) return "REAL";
            else if (dataType == typeof(String)) return "TEXT";
            //else if (dataType == typeof(TimeOnly)) return "TEXT";
            else if (dataType == typeof(TimeSpan)) return "TEXT";
            else if (dataType == typeof(UInt16)) return "INTEGER";
            else if (dataType == typeof(UInt32)) return "INTEGER";
            else if (dataType == typeof(UInt64)) return "INTEGER";
            else if (dataType.IsEnum) return "INTEGER";

            return "TEXT";
        }

        public static DbType ConvertToSQLiteParamType(Type dataType)
        {
            if (dataType == typeof(Boolean)) return DbType.Boolean;
            else if (dataType == typeof(Byte)) return DbType.Byte;
            //else if (dataType == typeof(Byte[])) return DbType;
            //else if (dataType == typeof(Char)) return DbType;
            //else if (dataType == typeof(DateOnly)) return "TEXT";
            else if (dataType == typeof(DateTime)) return DbType.DateTime;
            else if (dataType == typeof(DateTimeOffset)) return DbType.DateTimeOffset;
            else if (dataType == typeof(Decimal)) return DbType.Decimal;
            else if (dataType == typeof(Double)) return DbType.Double;
            else if (dataType == typeof(Guid)) return DbType.Guid;
            else if (dataType == typeof(Int16)) return DbType.Int16;
            else if (dataType == typeof(Int32)) return DbType.Int32;
            else if (dataType == typeof(Int64)) return DbType.Int64;
            else if (dataType == typeof(SByte)) return DbType.SByte;
            else if (dataType == typeof(Single)) return DbType.Single;
            else if (dataType == typeof(String)) return DbType.String;
            //else if (dataType == typeof(TimeOnly)) return "TEXT";
            else if (dataType == typeof(TimeSpan)) return DbType.Time;
            else if (dataType == typeof(UInt16)) return DbType.UInt16;
            else if (dataType == typeof(UInt32)) return DbType.UInt32;
            else if (dataType == typeof(UInt64)) return DbType.UInt64;
            else if (dataType.IsEnum) return DbType.Int32;
            return DbType.Object;
        }

        public static void AddNewColumn(string ColumnName, Type dataType, string dbFilePath)
        {
            using (SQLiteConnection Conn = new SQLiteConnection(string.Format(ConnectionString, dbFilePath)))
            {
                string SqlQuery = "PRAGMA foreign_keys = 0;" + Environment.NewLine;

                SqlQuery += "ALTER TABLE [NewProps] ADD COLUMN [" + ColumnName + "] " + DataSetUtility.ConvertToSQLiteDbType(dataType) + " NULL;" + Environment.NewLine;

                SqlQuery += "PRAGMA foreign_keys = 1";

                Conn.Open();
                using (SQLiteTransaction Transaction = Conn.BeginTransaction())
                {
                    SQLiteCommand Cmd = new SQLiteCommand(SqlQuery, Conn);
                    Cmd.ExecuteNonQuery();

                    Transaction.Commit();
                }
                Conn.Close();
            }
        }

        public static void DeleteColumn(string DeletedColumnName, string dbFilePath)
        {
            List<string> PropsFieldNames = new List<string>();
            List<string> PropsFieldNamesAndType = new List<string>();
            List<(int index, string col)> PropsPrimaryFields = new List<(int index, string col)>();
            List<string> NewPropsFieldNames = new List<string>();
            List<string> NewPropsFieldNamesAndType = new List<string>();
            List<(int index, string col)> NewPropsPrimaryFields = new List<(int index, string col)>();

            using (SQLiteConnection Conn = new SQLiteConnection(string.Format(ConnectionString, dbFilePath)))
            {
                Conn.Open();

                string SqlQuery = "";
                SQLiteCommand Cmd;

                SqlQuery = "PRAGMA table_info([NewProps]);";

                Cmd = new SQLiteCommand(SqlQuery, Conn);
                SQLiteDataReader Reader = Cmd.ExecuteReader();
                while (Reader.Read())
                {
                    if (Reader["name"].ToString().Equals(DeletedColumnName)) continue;

                    PropsFieldNames.Add(Reader["name"].ToString());

                    string Val = Reader["name"].ToString() + " " + Reader["type"].ToString();
                    if (Reader["notnull"].ToString().Equals("1")) Val += " NOT NULL";

                    if ((long)Reader["pk"] > 0) PropsPrimaryFields.Add((int.Parse(Reader["pk"].ToString()), Reader["name"].ToString()));

                    PropsFieldNamesAndType.Add(Val);
                }
                Reader.Close();

                string PropsPrimary = "";
                if (PropsPrimaryFields.Count > 0) PropsPrimary = ", PRIMARY KEY (" + string.Join(",", PropsPrimaryFields.OrderBy(x => x.index).Select(x => x.col)) + ")";


                SqlQuery = "PRAGMA table_info([NewProps]);";
                Cmd = new SQLiteCommand(SqlQuery, Conn);
                Reader = Cmd.ExecuteReader();
                while (Reader.Read())
                {
                    if (Reader["name"].ToString().Equals(DeletedColumnName)) continue;

                    NewPropsFieldNames.Add(Reader["name"].ToString());

                    string Val = Reader["name"].ToString() + " " + Reader["type"].ToString();
                    if (Reader["notnull"].ToString().Equals("1")) Val += " NOT NULL";

                    if ((long)Reader["pk"] > 0) NewPropsPrimaryFields.Add((int.Parse(Reader["pk"].ToString()), Reader["name"].ToString()));

                    NewPropsFieldNamesAndType.Add(Val);
                }
                Reader.Close();

                string NewPropsPrimary = "";
                if (NewPropsPrimaryFields.Count > 0) NewPropsPrimary = ", PRIMARY KEY (" + string.Join(",", NewPropsPrimaryFields.OrderBy(x => x.index).Select(x => x.col)) + ")";


                SqlQuery = "PRAGMA foreign_keys = 0;" + Environment.NewLine;

                SqlQuery += "CREATE TABLE dtk_temp_table_NewProps AS SELECT * FROM NewProps;" + Environment.NewLine;
                SqlQuery += "DROP TABLE NewProps;" + Environment.NewLine;
                SqlQuery += "CREATE TABLE NewProps (" + string.Join(",", NewPropsFieldNamesAndType) + NewPropsPrimary + ");" + Environment.NewLine;
                SqlQuery += "INSERT INTO NewProps (" + string.Join(",", NewPropsFieldNames) + ") SELECT " + string.Join(",", NewPropsFieldNames) + " FROM dtk_temp_table_NewProps;" + Environment.NewLine;
                SqlQuery += "DROP TABLE dtk_temp_table_NewProps;" + Environment.NewLine;
                    
                SqlQuery += "PRAGMA foreign_keys = 1;";

                using (SQLiteTransaction Transaction = Conn.BeginTransaction())
                {
                    Cmd = new SQLiteCommand(SqlQuery, Conn);
                    Cmd.ExecuteNonQuery();

                    Transaction.Commit();
                }
                Conn.Close();
            }
        }

    }
}
