using AP_MaJ.Properties;
using Ch.Hurni.AP_MaJ.Classes;
using DevExpress.Mvvm.Native;
using DevExpress.XtraReports;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Ch.Hurni.AP_MaJ.Classes.ApplicationOptions;

namespace AP_MaJ.Utilities
{
    public class VaultUtility
    {
        public string VaultConnection
        {
            get
            {
                return _vaultConnection;
            }
            set
            {
                _vaultConnection = value;
            }
        }
        private string _vaultConnection = null;
        



        public async Task<DataSet> ProcessFilesAsync(string FileTaskName, DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            if (FileTaskName.Equals("Validate")) return await ValidateFilesAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            else if (FileTaskName.Equals("ChangeState")) return await TempChangeStateFilesAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            else if (FileTaskName.Equals("PurgeProps")) return data;
            else if (FileTaskName.Equals("Update")) return data;
            else if (FileTaskName.Equals("PropSync")) return data;
            else if (FileTaskName.Equals("CreateBomBlob")) return data;
            else if (FileTaskName.Equals("WaitForBomBlob")) return data;
            else return data;
        }

        private async Task<string> ConnectToVault(ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Connection au Vault" });

            await Task.Run(() => System.Threading.Thread.Sleep(500));

            return "tttt";
        }



        private async Task<DataSet> ValidateFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la configuration Vault" });
            await Task.Run(() => System.Threading.Thread.Sleep(500));

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File")));

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });

            List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)>> TaskList = new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)>>();
            for (int i = 0; i < appOptions.SimultaneousValidationProcess; i++)
            {
                int ProcessId = i;
                DataRow PopEntity = EntitiesStack.Pop();
                TaskList.Add(Task.Run(() => ValidateFile(ProcessId, PopEntity, processProgReport)));

                if (EntitiesStack.Count == 0) break;
            }


            while (TaskList.Any())
            {
                Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)> finished = await Task.WhenAny(TaskList);

                int ProcessId = finished.Result.processId;

                if (finished.Result.State == StateEnum.Completed)
                {
                    finished.Result.entity["State"] = StateEnum.Completed;
                    finished.Result.entity["Name"] = finished.Result.Result["Name"];
                }
                else
                {
                    finished.Result.entity["State"] = finished.Result.State;
                }

                TaskList.Remove(finished);

                if (EntitiesStack.Count > 0 && !taskCancellationToken.IsCancellationRequested)
                {
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => ValidateFile(ProcessId, PopEntity, processProgReport)));
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State)> ValidateFile(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport)
        {
            StateEnum returnState = StateEnum.Error;

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            if (dr == null)
            {
                return (processId, dr, updatedValues, StateEnum.Error);
            }

            processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ProcessFeedbackMessage = dr.Field<string>("Name"), ProcessHasError = null });

            await Task.Run(() => System.Threading.Thread.Sleep(10));

            bool State = RandomValue();

            if (!State)
            {
                updatedValues.Add("Name", "New name " + dr.Field<string>("Name"));
                returnState = StateEnum.Completed;
            }

            processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ProcessFeedbackMessage = dr.Field<string>("Name"), ProcessHasError = State });
            return (processId, dr, updatedValues, returnState);
        }


        private async Task<DataSet> TempChangeStateFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy(); 

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File")));
            
            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Changement d'état temporaire des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });


            List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)>> TaskList = new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)>>();
            for (int i = 0; i < appOptions.SimultaneousChangeStateProcess; i++)
            {
                int ProcessId = i;
                DataRow PopEntity = EntitiesStack.Pop();
                TaskList.Add(Task.Run(() => ValidateFile(ProcessId, PopEntity, processProgReport)));

                if (EntitiesStack.Count == 0) break;
            }


            while (TaskList.Any())
            {
                Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)> finished = await Task.WhenAny(TaskList);

                int ProcessId = finished.Result.processId;

                if (finished.Result.State == StateEnum.Completed)
                {
                    finished.Result.entity["State"] = StateEnum.Completed;
                    finished.Result.entity["Name"] = finished.Result.Result["Name"];
                }
                else
                {
                    finished.Result.entity["State"] = finished.Result.State;
                }

                TaskList.Remove(finished);

                if (EntitiesStack.Count > 0 && !taskCancellationToken.IsCancellationRequested)
                {
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => ValidateFile(ProcessId, PopEntity, processProgReport)));
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Changement d'état temporaire des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State)> TempChangeStateFile(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport)
        {
            StateEnum returnState = StateEnum.Error;

            Dictionary<string, object> updatedValues = new Dictionary<string, object>();

            if(dr == null)
            {
                return (processId, dr, updatedValues, StateEnum.Error);
            }

            processProgReport.Report(new ProcessProgressReport() {ProcessIndex = processId, ProcessFeedbackMessage = dr.Field<string>("Name"), ProcessHasError = null });

            await Task.Run(() => System.Threading.Thread.Sleep(10));
            
            bool State = RandomValue();

            if(!State)
            { 
                updatedValues.Add("Name", "New name " + dr.Field<string>("Name"));
                returnState = StateEnum.Completed;
            }

            processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ProcessFeedbackMessage = dr.Field<string>("Name"), ProcessHasError = State });
            return (processId, dr, updatedValues, returnState);
        }

        public bool RandomValue()
        {
            Random random = new Random();
            int RandomNumber = random.Next(0, 100);

            return RandomNumber < 2;
        }
    }
}
