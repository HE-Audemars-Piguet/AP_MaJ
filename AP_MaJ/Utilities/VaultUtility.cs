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
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using VDF = Autodesk.DataManagement.Client.Framework;
using ACW = Autodesk.Connectivity.WebServices;
using Autodesk.DataManagement.Client.Framework.Vault.Currency.Entities;
using Autodesk.DataManagement.Client.Framework.Interfaces;
using Autodesk.Connectivity.WebServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using DevExpress.Xpf.Core.Native;
using Autodesk.DataManagement.Client.Framework.Internal.ExtensionMethods;

namespace AP_MaJ.Utilities
{
    public class VaultUtility
    {
        public VDF.Vault.Currency.Connections.Connection VaultConnection
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
        private VDF.Vault.Currency.Connections.Connection _vaultConnection = null;

        public VaultConfig VaultConfig
        {
            get
            {
                return _vaultConfig;
            }
            set
            {
                _vaultConfig = value;
            }
        }
        private VaultConfig _vaultConfig = null;

        internal async Task<VDF.Vault.Currency.Connections.Connection> ConnectToVaultAsync(ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken)
        {
            bool ReportProgress = taskProgReport != null;

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Connection au Vault", Timer = "Start" });

            VDF.Vault.Results.LogInResult results = await Task.Run(() => VDF.Vault.Library.ConnectionManager.LogIn(appOptions.VaultServer, appOptions.VaultName, appOptions.VaultUser, appOptions.VaultPassword,
                                                                                                                   VDF.Vault.Currency.Connections.AuthenticationFlags.Standard, null));

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Connection au Vault", Timer = "Stop" });

            if (results.Success) return results.Connection;
            else return null;
        }

        internal async Task<VaultConfig> ReadVaultConfigAsync(ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken)
        {
            bool ReportProgress = taskProgReport != null;

            VaultConfig vltConfig = new VaultConfig();

            if(ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des propriétés des fichiers", Timer = "Start" });
            vltConfig.VaultFilePropertyDefinitionDictionary = await Task.Run(() => VaultConnection.PropertyManager.GetPropertyDefinitions(VDF.Vault.Currency.Entities.EntityClassIds.Files, null, VDF.Vault.Currency.Properties.PropertyDefinitionFilter.IncludeAll));

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des catégories des fichiers" });
            vltConfig.VaultFileCategoryList = await Task.Run(() => VaultConnection.CategoryManager.GetAvailableCategories(VDF.Vault.Currency.Entities.EntityClassIds.Files, true).ToList());

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des propriétés des articles" });
            vltConfig.VaultItemPropertyDefinitionDictionary = await Task.Run(() => VaultConnection.PropertyManager.GetPropertyDefinitions(VDF.Vault.Currency.Entities.EntityClassIds.Items, null, VDF.Vault.Currency.Properties.PropertyDefinitionFilter.IncludeAll));

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des cycles de vie" });
            vltConfig.VaultLifeCycleDefinitionList = await Task.Run(() => VaultConnection.WebServiceManager.LifeCycleService.GetAllLifeCycleDefinitions().ToList());

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des révisions" });
            vltConfig.VaultRevisionDefinitionList = VaultConnection.WebServiceManager.RevisionService.GetAllRevisionDefinitionInfo().RevDefArray.ToList();

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des comportements de fichiers" });
            vltConfig.VaultFileCategoryBehavioursList = await Task.Run(() => VaultConnection.WebServiceManager.CategoryService.GetCategoryConfigurationsByBehaviorNames(VDF.Vault.Currency.Entities.EntityClassIds.Files, true, new string[] { "Category", "UserDefinedProperty", "RevisionScheme", "LifeCycle" }).ToList());

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des comportements de articles" });
            vltConfig.VaultItemCategoryBehavioursList = await Task.Run(() => VaultConnection.WebServiceManager.CategoryService.GetCategoryConfigurationsByBehaviorNames(VDF.Vault.Currency.Entities.EntityClassIds.Items, true, new string[] { "Category", "UserDefinedProperty", "RevisionScheme", "LifeCycle" }).ToList());

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture des transitions d'état autorisées" });
            vltConfig.AllowedStateTransitionIdsList = VaultConnection.WebServiceManager.LifeCycleService.GetAllowedLifeCycleStateTransitionIds().ToList();

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture des définitions de schéma de révision" });
            vltConfig.RevDefInfo = VaultConnection.WebServiceManager.RevisionService.GetAllRevisionDefinitionInfo();

            // TODO get other values from configuration...

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "", Timer = "Stop" });

            return vltConfig;
        }

        #region FileProcessing
        internal async Task<DataSet> ProcessFilesAsync(string FileTaskName, DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
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

        #region FileValidation
        internal async Task<DataSet> ValidateFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File")));

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });

            List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList = 
                new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

            for (int i = 0; i < appOptions.SimultaneousValidationProcess; i++)
            {
                int ProcessId = i;
                DataRow PopEntity = EntitiesStack.Pop();
                TaskList.Add(Task.Run(() => ValidateFile(ProcessId, PopEntity, processProgReport)));

                if (EntitiesStack.Count == 0) break;
            }


            while (TaskList.Any())
            {
                Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> finished = await Task.WhenAny(TaskList);

                int ProcessId = finished.Result.processId;

                finished.Result.entity["State"] = finished.Result.State;

                foreach (KeyValuePair<string, object> kvp in finished.Result.Result)
                {
                    finished.Result.entity[kvp.Key] = kvp.Value;
                }

                foreach (Dictionary<string, object> log in finished.Result.ResultLogs)
                { 
                    DataRow drLog = ds.Tables["Logs"].NewRow();
                    drLog["EntityId"] = finished.Result.entity["Id"];

                    foreach (KeyValuePair<string, object> kvp in log)
                    {
                        drLog[kvp.Key] = kvp.Value;
                    }

                    ds.Tables["Logs"].Rows.Add(drLog);
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

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> ValidateFile(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport)
        {
            ProcessProgressReport pProgressReport = new ProcessProgressReport() { ProcessIndex = processId, ProcessHasError = null };
            
            Dictionary<string, object> resultValues = new Dictionary<string, object>();
            List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

            StateEnum resultState = StateEnum.Processing;
            
            if (dr == null)
            {
                resultLogs.Add(CreateLog("Error", "La ligne de base de données est 'null', impossible de traiter l'élément."));
                resultState = StateEnum.Error;
            }

            string FullVaultName = string.Empty;
            ACW.File VaultFile = null;

            if (resultState != StateEnum.Error)
            {
                FullVaultName = dr.Field<string>("Path");
                if (string.IsNullOrWhiteSpace(FullVaultName) || FullVaultName.EndsWith("/")) FullVaultName += dr.Field<string>("Name");
                else FullVaultName += "/" + dr.Field<string>("Name");
                
                pProgressReport.ProcessFeedbackMessage = FullVaultName;
                processProgReport.Report(pProgressReport);

                if (string.IsNullOrWhiteSpace(FullVaultName))
                {
                    resultLogs.Add(CreateLog("Error", "Le nom de fichier est vide, impossible de traiter l'élément."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error)
            {
                try
                {
                    VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.FindLatestFilesByPaths(new string[] { FullVaultName }).FirstOrDefault());
                }
                catch (VaultServiceErrorException VltEx)
                {
                    resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + VltEx.ErrorCode + " à été retourné lors de l'accès au fichier '" + FullVaultName + "'."));
                    resultState = StateEnum.Error;
                }

                if(resultState != StateEnum.Error)
                {
                    if(VaultFile.MasterId != -1)
                    {
                        resultValues.Add("VaultMasterId", VaultFile.MasterId);

                        ValidateFileAccessInfo(VaultFile, FullVaultName, resultValues, ref resultState, resultLogs);

                        if (resultState != StateEnum.Error)
                        {
                            ValidateFileCategoryInfo(VaultFile, dr, resultValues, ref resultState, resultLogs);
                            ValidateFileLifeCycleInfo(VaultFile, dr, resultValues, ref resultState, resultLogs);
                            ValidateFileLifeCycleStateInfo(VaultFile, dr, resultValues, ref resultState, resultLogs);
                            ValidateFileRevisionInfo(VaultFile, dr, resultValues, ref resultState, resultLogs); 
                        }
                    }
                    else
                    {
                        resultLogs.Add(CreateLog("Error", "Le fichier '" + FullVaultName + "' n'existe pas dans le Vault."));
                        resultState = StateEnum.Error;
                    }
                }
            }

            pProgressReport.ProcessHasError = resultState == StateEnum.Error;
            processProgReport.Report(pProgressReport);

            if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;

            return (processId, dr, resultValues, resultState, resultLogs);
        }

        private void ValidateFileAccessInfo(ACW.File vaultFile, string fullVaultName, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            if (vaultFile.Cloaked)
            {
                resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'a pas les droits nécessaires pour accéder au fichier '" + fullVaultName + "', il ne peut pas être traité.."));
                resultState = StateEnum.Error;
                return;
            }
            if (vaultFile.CheckedOut)
            {
                try
                {
                    ACW.User user = VaultConnection.WebServiceManager.AdminService.GetUserByUserId(vaultFile.CkOutUserId);
                    resultLogs.Add(CreateLog("Error", "Le fichier '" + fullVaultName + "' est extrait par l'utilisateur '" + user.Name + "', il ne peut pas être traité."));
                }
                catch (VaultServiceErrorException VltEx)
                {
                    resultLogs.Add(CreateLog("Error", "Le fichier '" + fullVaultName + "' est extrait par un utilisateur, il ne peut pas être traité."));
                }

                
                resultState = StateEnum.Error;
                return;
            }
        }

        private void ValidateFileCategoryInfo(ACW.File vaultFile, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            resultValues.Add("VaultCatName", vaultFile.Cat.CatName);
            resultValues.Add("VaultCatId", vaultFile.Cat.CatId);

            string targetVaultCatName = dr.Field<string>("TargetVaultCatName");

            if (!string.IsNullOrWhiteSpace(targetVaultCatName))
            {
                EntityCategory TargetCat = VaultConfig.VaultFileCategoryList.Where(x => x.Name.Equals(targetVaultCatName)).FirstOrDefault();
                if (TargetCat != null)
                {
                    resultValues.Add("TargetVaultCatId", TargetCat.ID);
                }
                else
                {
                    resultLogs.Add(CreateLog("Error", "La catégorie cible '" + targetVaultCatName + "' n'existe pas dans le Vault."));
                    resultState = StateEnum.Error;
                }
            }
        }

        private void ValidateFileLifeCycleInfo(ACW.File vaultFile, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            if (vaultFile.FileLfCyc.LfCycDefId > 0) resultValues.Add("VaultLcName", VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == vaultFile.FileLfCyc.LfCycDefId).FirstOrDefault().DispName);
            resultValues.Add("VaultLcId", vaultFile.FileLfCyc.LfCycDefId);

            string targetVaultlifeCycleName = dr.Field<string>("TargetVaultLcName");
            if (!string.IsNullOrWhiteSpace(targetVaultlifeCycleName))
            {
                long CatId = vaultFile.Cat.CatId;
                string CatName = vaultFile.Cat.CatName;

                if (resultValues.ContainsKey("TargetVaultCatId"))
                {
                    CatId = (long)resultValues["TargetVaultCatId"];
                    CatName = dr.Field<string>("TargetVaultCatName");
                }

                CatCfg catCfg = VaultConfig.VaultFileCategoryBehavioursList.Where(x => x.Cat.Id == CatId).FirstOrDefault();
                Bhv TargetLcBhv = catCfg.BhvCfgArray.Where(x => x.Name.Equals("LifeCycle")).FirstOrDefault().BhvArray.Where(x => x.DisplayName.Equals(targetVaultlifeCycleName)).FirstOrDefault();

                if (TargetLcBhv != null)
                {
                    resultValues.Add("TargetVaultLcId", TargetLcBhv.Id);
                }
                else
                {
                    resultLogs.Add(CreateLog("Error", "Le cycle de vie cible '" + targetVaultlifeCycleName + "' n'existe pas dans le Vault ou n'est pas associé à la catégorie '" + CatName + "'."));
                    resultState = StateEnum.Error;
                }
            }
        }

        private void ValidateFileLifeCycleStateInfo(ACW.File vaultFile, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            resultValues.Add("VaultLcsName", vaultFile.FileLfCyc.LfCycStateName);
            resultValues.Add("VaultLcsId", vaultFile.FileLfCyc.IterLfCycStateId);

            LfCycState TempLcs = null;
            
            string tempVaultlifeCycleStatName = dr.Field<string>("TempVaultLcsName");
            if (!string.IsNullOrWhiteSpace(tempVaultlifeCycleStatName))
            {
                LfCycDef CurrentLcDef = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == vaultFile.FileLfCyc.LfCycDefId).FirstOrDefault();
                
                TempLcs = CurrentLcDef.StateArray.Where(x => x.DispName == tempVaultlifeCycleStatName).FirstOrDefault();
                if(TempLcs == null)
                {
                    resultLogs.Add(CreateLog("Error", "L'état de cycle de vie temporaire '" + tempVaultlifeCycleStatName + "' n'existe pas dans le cycle de vie '" + CurrentLcDef.DispName + "'."));
                    resultState = StateEnum.Error;
                    return;
                }

                resultValues.Add("TempVaultLcsId", TempLcs.Id);
                if (vaultFile.FileLfCyc.IterLfCycStateId != TempLcs.Id)
                {
                    LfCycTrans TempTrans = CurrentLcDef.TransArray.Where(x => x.FromId == vaultFile.FileLfCyc.IterLfCycStateId && x.ToId == TempLcs.Id).FirstOrDefault();
                    if (TempTrans == null)
                    {
                        resultLogs.Add(CreateLog("Error", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + tempVaultlifeCycleStatName + "' n'est pas possible dans le cycle de vie '" + CurrentLcDef.DispName + "'."));
                        resultState = StateEnum.Error;
                        return;
                    }

                    if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TempTrans.Id))
                    {
                        resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + tempVaultlifeCycleStatName + "' dans le cycle de vie '" + CurrentLcDef.DispName + "'."));
                        resultState = StateEnum.Error;
                        return;
                    }

                    if (TempTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                    {
                        resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + tempVaultlifeCycleStatName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise a jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
                    }
                }
            }

            LfCycDef TargetLcDef = null;
            LfCycState TargetLcs = null;

            string targetVaultlifeCycleStatName = dr.Field<string>("TargetVaultLcsName");
            if (!string.IsNullOrWhiteSpace(targetVaultlifeCycleStatName))
            {
                if (resultValues.ContainsKey("TargetVaultLcId"))
                {
                    TargetLcDef = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == (long)resultValues["TargetVaultLcId"]).FirstOrDefault();
                }
                else
                {
                    TargetLcDef = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == vaultFile.FileLfCyc.LfCycDefId).FirstOrDefault();
                }

                if(TargetLcDef == null)
                {
                    resultLogs.Add(CreateLog("Error", "L'état de cycle de vie cible '" + targetVaultlifeCycleStatName + "' n'existe pas dans le cycle de vie ''."));
                    resultState = StateEnum.Error;
                    return;
                }

                TargetLcs = TargetLcDef.StateArray.Where(x => x.DispName == targetVaultlifeCycleStatName).FirstOrDefault();
                if (TargetLcs == null)
                {
                    resultLogs.Add(CreateLog("Error", "L'état de cycle de vie cible '" + targetVaultlifeCycleStatName + "' n'existe pas dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                    resultState = StateEnum.Error;
                    return;
                }

                resultValues.Add("TargetVaultLcsId", TargetLcs.Id);

                if (TempLcs == null)
                {
                    long FromId = vaultFile.FileLfCyc.IterLfCycStateId;
                    if (FromId == 0) FromId = TargetLcDef.StateArray.Where(x => x.IsDflt).FirstOrDefault().Id;

                    if (FromId != TargetLcs.Id)
                    {
                        LfCycTrans TargetTrans = TargetLcDef.TransArray.Where(x => x.FromId == FromId && x.ToId == TargetLcs.Id).FirstOrDefault();
                        if (TargetTrans == null)
                        {
                            resultLogs.Add(CreateLog("Error", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + targetVaultlifeCycleStatName + "' n'est pas possible dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TargetTrans.Id))
                        {
                            resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + targetVaultlifeCycleStatName + "' dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (TargetTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                        {
                            resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + targetVaultlifeCycleStatName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise à jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
                        }
                    }
                }
                else
                {
                    if (TempLcs.Id != TargetLcs.Id)
                    {
                        LfCycTrans TargetTrans = TargetLcDef.TransArray.Where(x => x.FromId == TempLcs.Id && x.ToId == TargetLcs.Id).FirstOrDefault();
                        if (TargetTrans == null)
                        {
                            resultLogs.Add(CreateLog("Error", "La transition de l'état '" + tempVaultlifeCycleStatName + "' vers l'état '" + targetVaultlifeCycleStatName + "' n'est pas possible dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TargetTrans.Id))
                        {
                            resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + tempVaultlifeCycleStatName + "' vers l'état '" + targetVaultlifeCycleStatName + "' dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (TargetTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                        {
                            resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + tempVaultlifeCycleStatName + "' vers l'état '" + targetVaultlifeCycleStatName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise à jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
                        }
                    }
                }
            }
        }

        private void ValidateFileRevisionInfo(ACW.File vaultFile, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            RevDef CurrentRevDef = VaultConfig.VaultRevisionDefinitionList.Where(x => x.Id == vaultFile.FileRev.RevDefId).FirstOrDefault();

            if (vaultFile.FileRev.RevDefId > 0) resultValues.Add("VaultRevSchName", CurrentRevDef.DispName);
            resultValues.Add("VaultRevSchId", vaultFile.FileRev.RevDefId);

            string targetVaultRevisionSchName = dr.Field<string>("TargetVaultRevSchName");
            if (!string.IsNullOrWhiteSpace(targetVaultRevisionSchName))
            {
                long CatId = vaultFile.Cat.CatId;
                string CatName = vaultFile.Cat.CatName;

                if (resultValues.ContainsKey("TargetVaultCatId"))
                {
                    CatId = (long)resultValues["TargetVaultCatId"];
                    CatName = dr.Field<string>("TargetVaultCatName");
                }

                CatCfg catCfg = VaultConfig.VaultFileCategoryBehavioursList.Where(x => x.Cat.Id == CatId).FirstOrDefault();
                Bhv TargetRevBhv = catCfg.BhvCfgArray.Where(x => x.Name.Equals("RevisionScheme")).FirstOrDefault().BhvArray.Where(x => x.DisplayName.Equals(targetVaultRevisionSchName)).FirstOrDefault();

                if (TargetRevBhv != null)
                {
                    resultValues.Add("TargetVaultRevSchId", TargetRevBhv.Id);
                }
                else
                {
                    resultLogs.Add(CreateLog("Error", "Le schéma de révision '" + targetVaultRevisionSchName + "' n'existe pas dans le Vault ou n'est pas associé à la catégorie '" + CatName + "'."));
                    resultState = StateEnum.Error;
                    return;
                }
            }

            resultValues.Add("VaultRevLabel", vaultFile.FileRev.Label);

            string targetVaultRevisionName = dr.Field<string>("TargetVaultRevLabel");
            if(!string.IsNullOrWhiteSpace(targetVaultRevisionName) && !targetVaultRevisionName.Equals("NextPrimary") && !targetVaultRevisionName.Equals("NextSecondary") && !targetVaultRevisionName.Equals("NextTertiary"))
            {

                if (resultValues.ContainsKey("TargetVaultRevSchId"))
                {
                    CurrentRevDef = VaultConfig.VaultRevisionDefinitionList.Where(x => x.Id == (long)resultValues["TargetVaultRevSchId"]).FirstOrDefault();
                }

                if(!ValidateRevisionLabel(vaultFile.FileRev.Label, targetVaultRevisionName, CurrentRevDef))
                {
                    string RevDefName = "Base";
                    if (CurrentRevDef != null) RevDefName = CurrentRevDef.DispName;

                    resultLogs.Add(CreateLog("Error", "Le schéma de révision '" + RevDefName + "' n'autorise pas le passage de la révision '" + vaultFile.FileRev.Label + "' vers '" + targetVaultRevisionName + "'."));
                    resultState = StateEnum.Error;
                }
            }
        }
        #endregion


        internal async Task<DataSet> TempChangeStateFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy(); 

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File")));
            
            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Changement d'état temporaire des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });


            ////List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)>> TaskList = new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)>>();
            ////for (int i = 0; i < appOptions.SimultaneousChangeStateProcess; i++)
            ////{
            ////    int ProcessId = i;
            ////    DataRow PopEntity = EntitiesStack.Pop();
            ////    TaskList.Add(Task.Run(() => ValidateFile(ProcessId, PopEntity, processProgReport)));

            ////    if (EntitiesStack.Count == 0) break;
            ////}


            ////while (TaskList.Any())
            ////{
            ////    Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State)> finished = await Task.WhenAny(TaskList);

            ////    int ProcessId = finished.Result.processId;

            ////    if (finished.Result.State == StateEnum.Completed)
            ////    {
            ////        finished.Result.entity["State"] = StateEnum.Completed;
            ////        finished.Result.entity["Name"] = finished.Result.Result["Name"];
            ////    }
            ////    else
            ////    {
            ////        finished.Result.entity["State"] = finished.Result.State;
            ////    }

            ////    TaskList.Remove(finished);

            ////    if (EntitiesStack.Count > 0 && !taskCancellationToken.IsCancellationRequested)
            ////    {
            ////        DataRow PopEntity = EntitiesStack.Pop();
            ////        TaskList.Add(Task.Run(() => ValidateFile(ProcessId, PopEntity, processProgReport)));
            ////    }
            ////}

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
            
            bool State = true;

            if(!State)
            { 
                updatedValues.Add("Name", "New name " + dr.Field<string>("Name"));
                returnState = StateEnum.Completed;
            }

            processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ProcessFeedbackMessage = dr.Field<string>("Name"), ProcessHasError = State });
            return (processId, dr, updatedValues, returnState);
        }

        #endregion


        #region ItemProcessing
        internal async Task<DataSet> ProcessItemsAsync(string FileTaskName, DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            if (FileTaskName.Equals("Validate")) return await ValidateItemsAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            else if (FileTaskName.Equals("ChangeState")) return data;
            else if (FileTaskName.Equals("PurgeProps")) return data;
            else if (FileTaskName.Equals("Update")) return data;
            else if (FileTaskName.Equals("PropSync")) return data;
            else if (FileTaskName.Equals("CreateBomBlob")) return data;
            else if (FileTaskName.Equals("WaitForBomBlob")) return data;
            else return data;
        }

        #region ItemValidation
        internal async Task<DataSet> ValidateItemsAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("Item")));

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des articles", TotalEntityCount = TotalCount, Timer = "Start" });

            List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
                new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

            for (int i = 0; i < appOptions.SimultaneousValidationProcess; i++)
            {
                int ProcessId = i;
                DataRow PopEntity = EntitiesStack.Pop();
                TaskList.Add(Task.Run(() => ValidateItem(ProcessId, PopEntity, processProgReport)));

                if (EntitiesStack.Count == 0) break;
            }


            while (TaskList.Any())
            {
                Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> finished = await Task.WhenAny(TaskList);

                int ProcessId = finished.Result.processId;

                finished.Result.entity["State"] = finished.Result.State;

                foreach (KeyValuePair<string, object> kvp in finished.Result.Result)
                {
                    finished.Result.entity[kvp.Key] = kvp.Value;
                }

                foreach (Dictionary<string, object> log in finished.Result.ResultLogs)
                {
                    DataRow drLog = ds.Tables["Logs"].NewRow();
                    drLog["EntityId"] = finished.Result.entity["Id"];

                    foreach (KeyValuePair<string, object> kvp in log)
                    {
                        drLog[kvp.Key] = kvp.Value;
                    }

                    ds.Tables["Logs"].Rows.Add(drLog);
                }

                TaskList.Remove(finished);

                if (EntitiesStack.Count > 0 && !taskCancellationToken.IsCancellationRequested)
                {
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => ValidateItem(ProcessId, PopEntity, processProgReport)));
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des articles", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> ValidateItem(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport)
        {
            ProcessProgressReport pProgressReport = new ProcessProgressReport() { ProcessIndex = processId, ProcessHasError = null };

            Dictionary<string, object> resultValues = new Dictionary<string, object>();
            List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

            StateEnum resultState = StateEnum.Processing;

            if (dr == null)
            {
                resultLogs.Add(CreateLog("Error", "La ligne de base de données est 'null', impossible de traiter l'article."));
                resultState = StateEnum.Error;
            }

            string FullVaultName = string.Empty;
            ACW.Item VaultItem = null;

            if (resultState != StateEnum.Error)
            {
                FullVaultName = dr.Field<string>("Name");

                pProgressReport.ProcessFeedbackMessage = FullVaultName;
                processProgReport.Report(pProgressReport);

                if (string.IsNullOrWhiteSpace(FullVaultName))
                {
                    resultLogs.Add(CreateLog("Error", "Le nom de l'article est vide, impossible de traiter l'élément."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error)
            {
                try
                {
                    VaultItem = await Task.Run(() => VaultConnection.WebServiceManager.ItemService.GetLatestItemByItemNumber(FullVaultName));
                }
                catch(VaultServiceErrorException VltEx)
                { 
                    if(VltEx.ErrorCode == 1350)
                    {
                        resultLogs.Add(CreateLog("Error", "L'article '" + FullVaultName + "' n'existe pas dans le Vault."));
                    }
                    else if (VltEx.ErrorCode == 303)
                    {
                        resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'a pas les droits nécessaires pour accéder à l'article '" + FullVaultName + "'."));
                    }
                    else
                    {
                        resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + VltEx.ErrorCode + " à été retourné lors de l'accès à l'article '" + FullVaultName + "'."));
                    }
                    resultState = StateEnum.Error;
                    VaultItem = null;
                }

                if (VaultItem != null && VaultItem.MasterId != -1)
                {
                    resultValues.Add("VaultMasterId", VaultItem.MasterId);

                    ValidateItemAccessInfo(VaultItem, FullVaultName, resultValues, ref resultState, resultLogs);

                    if (resultState != StateEnum.Error)
                    {
                        ValidateItemCategoryInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);
                        ValidateItemLifeCycleInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);
                        ValidateItemLifeCycleStateInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);
                        ValidateItemRevisionInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);
                    }
                }
            }

            pProgressReport.ProcessHasError = resultState == StateEnum.Error;
            processProgReport.Report(pProgressReport);

            if(resultState == StateEnum.Processing) resultState = StateEnum.Completed; 
            
            return (processId, dr, resultValues, resultState, resultLogs);
        }

        private void ValidateItemAccessInfo(ACW.Item vaultItem, string fullVaultName, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            if (vaultItem.IsCloaked)
            {
                resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'a pas les droits nécessaires pour accéder à l'article '" + fullVaultName + "', il ne peut pas être traité.."));
                resultState = StateEnum.Error;
                return;
            }
            if (vaultItem.Locked)
            {
                resultLogs.Add(CreateLog("Error", "L'article '" + fullVaultName + "' est vérouillé, il ne peut pas être traité."));
                resultState = StateEnum.Error;
                return;
            }
        }


        private void ValidateItemCategoryInfo(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            resultValues.Add("VaultCatName", vaultItem.Cat.CatName);
            resultValues.Add("VaultCatId", vaultItem.Cat.CatId);

            string targetVaultCatName = dr.Field<string>("TargetVaultCatName");

            if (!string.IsNullOrWhiteSpace(targetVaultCatName))
            {
                EntityCategory TargetCat = VaultConfig.VaultFileCategoryList.Where(x => x.Name.Equals(targetVaultCatName)).FirstOrDefault();
                if (TargetCat != null)
                {
                    resultValues.Add("TargetVaultCatId", TargetCat.ID);
                }
                else
                {
                    resultLogs.Add(CreateLog("Error", "La catégorie cible '" + targetVaultCatName + "' n'existe pas dans le Vault."));
                    resultState = StateEnum.Error;
                }
            }
        }

        private void ValidateItemLifeCycleInfo(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            if (vaultItem.LfCyc.LfCycDefId > 0) resultValues.Add("VaultLcName", VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == vaultItem.LfCyc.LfCycDefId).FirstOrDefault().DispName);
            resultValues.Add("VaultLcId", vaultItem.LfCyc.LfCycDefId);

            string targetVaultlifeCycleName = dr.Field<string>("TargetVaultLcName");

            if (!string.IsNullOrWhiteSpace(targetVaultlifeCycleName))
            {
                long CatId = vaultItem.Cat.CatId;
                string CatName = vaultItem.Cat.CatName;

                if (resultValues.ContainsKey("TargetVaultCatId"))
                {
                    CatId = (long)resultValues["TargetVaultCatId"];
                    CatName = dr.Field<string>("TargetVaultCatName");
                }

                CatCfg catCfg = VaultConfig.VaultItemCategoryBehavioursList.Where(x => x.Cat.Id == CatId).FirstOrDefault();
                Bhv TargetLcBhv = catCfg.BhvCfgArray.Where(x => x.Name.Equals("LifeCycle")).FirstOrDefault().BhvArray.Where(x => x.DisplayName.Equals(targetVaultlifeCycleName)).FirstOrDefault();
                
                if (TargetLcBhv != null)
                {
                    resultValues.Add("TargetVaultLcId", TargetLcBhv.Id);
                }
                else
                {
                    resultLogs.Add(CreateLog("Error", "Le cycle de vie cible '" + targetVaultlifeCycleName + "' n'existe pas dans le Vault ou n'est pas associé à la catégorie '" + CatName + "'."));
                    resultState = StateEnum.Error;
                }
            }
        }

        private void ValidateItemLifeCycleStateInfo(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            resultValues.Add("VaultLcsName", vaultItem.LfCyc.LfCycStateId);
            resultValues.Add("VaultLcsId", vaultItem.LfCyc.LfCycStateId);

            string tempVaultlifeCycleStatName = dr.Field<string>("TempVaultLcsName");
            string targetVaultlifeCycleStatName = dr.Field<string>("TargetVaultLcsName");

            //if (!string.IsNullOrWhiteSpace(targetVaultlifeCycleName))
            //{
            //    EntityCategory TargetCat = VaultConfig.VaultFileCategoryList.Where(x => x.Name.Equals(targetVaultlifeCycleName)).FirstOrDefault();
            //    if (TargetCat != null)
            //    {
            //        resultValues.Add("TargetVaultLcId", TargetCat.ID);
            //    }
            //    else
            //    {
            //        Dictionary<string, object> log = new Dictionary<string, object>();
            //        log.Add("Severity", "Error");
            //        log.Add("Date", DateTime.Now);
            //        log.Add("Message", "Le cycle de vie cible '" + targetVaultlifeCycleName + "' n'existe pas dans le Vault.");
            //        resultLogs.Add(log);

            //        resultValues.Add("TargetVaultLcId", -1);

            //        resultState = StateEnum.Error;
            //    }
            //}
        }

        private void ValidateItemRevisionInfo(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            //if (vaultFile.FileRev.RevDefId > 0) resultValues.Add("VaultRevSchName", VaultConfig.VaultRevisionDefinitionList.Where(x => x.Id == vaultFile.FileRev.RevDefId).FirstOrDefault().DispName);
            //resultValues.Add("VaultRevSchId", vaultFile.FileRev.RevDefId);

            resultValues.Add("VaultRevLabel", vaultItem.RevNum);
            resultValues.Add("VaultRevId", vaultItem.RevId);

            string targetVaultRevisionSchName = dr.Field<string>("TargetVaultRevSchName");
            string targetVaultRevisionName = dr.Field<string>("TargetVaultRevLabel");

            //if (!string.IsNullOrWhiteSpace(targetVaultlifeCycleName))
            //{
            //    EntityCategory TargetCat = VaultConfig.VaultFileCategoryList.Where(x => x.Name.Equals(targetVaultlifeCycleName)).FirstOrDefault();
            //    if (TargetCat != null)
            //    {
            //        resultValues.Add("TargetVaultLcId", TargetCat.ID);
            //    }
            //    else
            //    {
            //        Dictionary<string, object> log = new Dictionary<string, object>();
            //        log.Add("Severity", "Error");
            //        log.Add("Date", DateTime.Now);
            //        log.Add("Message", "Le cycle de vie cible '" + targetVaultlifeCycleName + "' n'existe pas dans le Vault.");
            //        resultLogs.Add(log);

            //        resultValues.Add("TargetVaultLcId", -1);

            //        resultState = StateEnum.Error;
            //    }
            //}
        }
        #endregion


        #endregion



        private Dictionary<string, object> CreateLog(string Severity, string Message)
        {
            Dictionary<string, object> log = new Dictionary<string, object>();
            log.Add("Severity", Severity);
            log.Add("Date", DateTime.Now);
            log.Add("Message", Message);

            return log;
        }

        private bool ValidateRevisionLabel(string CurrentRev, string NewRev, RevDef RevDef)
        {
            if(RevDef == null) return false;

            int CurrentPrimaryIndex = -1;
            int CurrentSecondaryIndex = -1;
            int CurrentTertiaryIndex = -1;

            int NewPrimaryIndex = -1;
            int NewSecondaryIndex = -1;
            int NewTertiaryIndex = -1;

            List<string> CurrentRevSeq = CurrentRev.Split(RevDef.Delim).ToList();
            List<string> NewRevSeq = NewRev.Split(RevDef.Delim).ToList();

            List<string> PrimaryLabelList = VaultConfig.RevDefInfo.RevSeqArray.Where(x => x.Id == RevDef.PriSchmId).FirstOrDefault().Label.LabelArray.ToList();
            List<string> SecondaryLabelList = VaultConfig.RevDefInfo.RevSeqArray.Where(x => x.Id == RevDef.SecSchmId).FirstOrDefault().Label.LabelArray.ToList();
            List<string> TertiaryLabelList = VaultConfig.RevDefInfo.RevSeqArray.Where(x => x.Id == RevDef.TerSchmId).FirstOrDefault().Label.LabelArray.ToList();

            if (CurrentRevSeq.Count > 0) CurrentPrimaryIndex = PrimaryLabelList.IndexOf(CurrentRevSeq[0]);
            if (CurrentRevSeq.Count > 1) CurrentSecondaryIndex = SecondaryLabelList.IndexOf(CurrentRevSeq[1]);
            if (CurrentRevSeq.Count > 2) CurrentTertiaryIndex = TertiaryLabelList.IndexOf(CurrentRevSeq[2]);

            if (NewRevSeq.Count > 0) NewPrimaryIndex = PrimaryLabelList.IndexOf(NewRevSeq[0]);
            if (NewRevSeq.Count > 1) NewSecondaryIndex = SecondaryLabelList.IndexOf(NewRevSeq[1]);
            if (NewRevSeq.Count > 2) NewTertiaryIndex = TertiaryLabelList.IndexOf(NewRevSeq[2]);

            if((NewPrimaryIndex > CurrentPrimaryIndex) || (NewPrimaryIndex == CurrentPrimaryIndex && NewSecondaryIndex > CurrentSecondaryIndex) || (NewPrimaryIndex == CurrentPrimaryIndex && NewSecondaryIndex == CurrentSecondaryIndex && NewTertiaryIndex > CurrentTertiaryIndex))
            {
                return true;
            }
           
            return false;
        }
    }
}
