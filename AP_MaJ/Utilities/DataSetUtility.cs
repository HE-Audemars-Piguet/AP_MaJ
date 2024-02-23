using AP_MaJ.Properties;
using System;
using System.Collections.Generic;
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
        internal static DataSet CreateDataSet()
        {
            DataSet ds = new DataSet();

            DataTable dtEntities = new DataTable("Entities");
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Id", DataType = typeof(long), AutoIncrement = true, AutoIncrementSeed = 1, AutoIncrementStep = 1, AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Task", DataType = typeof(TaskTypeEnum), AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "State", DataType = typeof(StateEnum), AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultMasterId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "EntityType", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = false });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Name", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "Path", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultPath", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultCatName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultCatId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultCatName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultCatId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcsName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultLcsId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TempVaultLcsName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TempVaultLcsId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcsName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = false });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultLcsId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevSchName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevSchId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevSchName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevSchId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "VaultRevId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevName", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });
            dtEntities.Columns.Add(new DataColumn() { ColumnName = "TargetVaultRevId", DataType = typeof(string), DefaultValue = string.Empty, AllowDBNull = true });

            dtEntities.PrimaryKey = new List<DataColumn>() { dtEntities.Columns["Id"] }.ToArray();
            ds.Tables.Add(dtEntities);


            DataTable dtNewProps = new DataTable("NewProps");
            dtNewProps.Columns.Add(new DataColumn() { ColumnName = "EntityId", DataType = typeof(long), AllowDBNull = false });
            dtNewProps.Columns.Add(new DataColumn() { ColumnName = "NoPiece", DataType = typeof(string), AllowDBNull = true });
            dtNewProps.Columns.Add(new DataColumn() { ColumnName = "Matiere", DataType = typeof(string), AllowDBNull = true });
            dtNewProps.Columns.Add(new DataColumn() { ColumnName = "CompositionMatiere", DataType = typeof(string), AllowDBNull = true });
            dtNewProps.Columns.Add(new DataColumn() { ColumnName = "DureteMatiere", DataType = typeof(string), AllowDBNull = true });
            dtNewProps.Columns.Add(new DataColumn() { ColumnName = "Description", DataType = typeof(string), AllowDBNull = true });
            //foreach (FieldMapping fm in Settings.InventorFieldMappings.Where(x => x.MappingDirection != FieldMappingDirectionEnum.Read))
            //{
            //    dtNewProps.Columns.Add(new DataColumn() { ColumnName = fm.Name, DataType = typeof(string), AllowDBNull = true });
            //}

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


        private const string ConnectionString = "Data Source={0}; FailIfMissing=False";

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
                //SELECT name FROM sqlite_master WHERE type='table' AND name='{table_name}'

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

        //public static async Task ReadFromSQLiteAsync(this DataSet ds, string dbFilePath)
        //{
        //    ds = await Task.Run(() => Read(ds, dbFilePath));

        //    ds.AcceptChanges();
        //}

        //private static DataSet Read(DataSet ds, string dbFilePath)
        //{
        //    DataSet readDs = ds.Clone();

        //    using (SQLiteConnection Conn = new SQLiteConnection(string.Format(ConnectionString, dbFilePath)))
        //    {
        //        Conn.Open();
        //        //SELECT name FROM sqlite_master WHERE type='table' AND name='{table_name}'

        //        SQLiteDataAdapter DtA = new SQLiteDataAdapter();

        //        foreach (DataTable dt in readDs.Tables)
        //        {
        //            SQLiteCommand Cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='" + dt.TableName + "'", Conn);
        //            if (Cmd.ExecuteScalar() != null)
        //            {
        //                DtA.SelectCommand = new SQLiteCommand("SELECT * FROM [" + dt.TableName + "]", Conn);
        //                DtA.Fill(dt);
        //            }
        //        }

        //        Conn.Close();
        //    }

        //    return readDs;
        //}

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
    }
}
