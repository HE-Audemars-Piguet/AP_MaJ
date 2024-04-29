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
using Autodesk.DataManagement.Client.Framework.Currency;
using DevExpress.ClipboardSource.SpreadsheetML;
using System.Collections;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using Autodesk.DataManagement.Client.Framework.Vault.Currency.Properties;
using DevExpress.Xpf.Editors.Helpers;
using System.Runtime.InteropServices.ComTypes;
using DevExpress.Xpf.Docking;
using System.Xml;
using DevExpress.Data.Svg;
using Inventor;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using Autodesk.DataManagement.Client.Framework.Vault.Results;
using DevExpress.Xpf.Core.DragDrop.Native;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Ch.Hurni.AP_MaJ.Utilities
{
    public class VaultUtility
    {
        public List<string> BooleanTrueValues = new List<string>() { "Oui", "Vrai", "Yes", "True", "1" };
        public List<string> BooleanFalseValues = new List<string>() { "Non", "Faux", "No", "False", "0" };

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

        private InventorDispatcher _invDispatcher;

        public VaultUtility()
        {
            
        }

        public VaultUtility(InventorDispatcher dispatcher)
        {
            _invDispatcher = dispatcher;
        }

        #region VaultConnection
        internal async Task<VDF.Vault.Currency.Connections.Connection> ConnectToVaultAsync(ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken, string confirmedUser = null, string confirmedPwd = null)
        {
            bool ReportProgress = taskProgReport != null;

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Connection au Vault...", Timer = "Start" });
            await Task.Delay(50);

            string user = appOptions.VaultUser;
            if (confirmedUser != null) user = confirmedUser;

            string pwd = appOptions.VaultPassword;
            if (confirmedPwd != null) pwd = confirmedPwd;

            VDF.Vault.Results.LogInResult results = VDF.Vault.Library.ConnectionManager.LogIn(appOptions.VaultServer, appOptions.VaultName, user, pwd, VDF.Vault.Currency.Connections.AuthenticationFlags.Standard, null);

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "", Timer = "Stop" });
            await Task.Delay(50);

            if (results.Success) return results.Connection;
            else return null;
        }

        internal VDF.Vault.Currency.Connections.Connection ConnectToVault(ApplicationOptions appOptions, string confirmedUser = null, string confirmedPwd = null)
        {
            string user = appOptions.VaultUser;
            if (confirmedUser != null) user = confirmedUser;

            string pwd = appOptions.VaultPassword;
            if (confirmedPwd != null) pwd = confirmedPwd;

            VDF.Vault.Results.LogInResult results = VDF.Vault.Library.ConnectionManager.LogIn(appOptions.VaultServer, appOptions.VaultName, user, pwd,
                                                                                              VDF.Vault.Currency.Connections.AuthenticationFlags.Standard, null);

            if (results.Success) return results.Connection;
            else return null;
        }

        internal VDF.Vault.Currency.Connections.Connection ConnectToVault(ApplicationOptions appOptions)
        {
            VDF.Vault.Results.LogInResult results = VDF.Vault.Library.ConnectionManager.LogIn(appOptions.VaultServer, appOptions.VaultName, appOptions.VaultUser, appOptions.VaultPassword,
                                                                                                                   VDF.Vault.Currency.Connections.AuthenticationFlags.Standard, null);

            if (results.Success) return results.Connection;
            else return null;
        }
        #endregion


        #region ReadVaultInventorConfig
        internal async Task<VaultConfig> ReadVaultConfigAsync(ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken)
        {
            bool ReportProgress = taskProgReport != null;

            VaultConfig vltConfig = new VaultConfig();

            if(ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des propriétés des fichiers...", Timer = "Start" });
            vltConfig.VaultFilePropertyDefinitionDictionary = await Task.Run(() => VaultConnection.PropertyManager.GetPropertyDefinitions(VDF.Vault.Currency.Entities.EntityClassIds.Files, null, VDF.Vault.Currency.Properties.PropertyDefinitionFilter.IncludeAll | VDF.Vault.Currency.Properties.PropertyDefinitionFilter.OnlyActive));

            vltConfig.ProviderPropId = vltConfig.VaultFilePropertyDefinitionDictionary["Provider"].Id;
            vltConfig.CompliancePropId = vltConfig.VaultFilePropertyDefinitionDictionary["Compliance"].Id;
            vltConfig.ItemAssignablePropId = vltConfig.VaultFilePropertyDefinitionDictionary["ItemAssignable"].Id;

            vltConfig.VaultFilePropertyMapping = await Task.Run(() => GetPropertyMapping(VDF.Vault.Currency.Entities.EntityClassIds.Files, 
                vltConfig.VaultFilePropertyDefinitionDictionary.Where(x => (!x.Value.IsSystem || x.Value.SystemName.Equals("Revision")) && !x.Value.IsDynamic).Select(x => x.Value).ToList()));

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des catégories des fichiers" });
            vltConfig.VaultFileCategoryList = await Task.Run(() => VaultConnection.CategoryManager.GetAvailableCategories(VDF.Vault.Currency.Entities.EntityClassIds.Files, true).ToList());

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des propriétés des articles" });
            vltConfig.VaultItemPropertyDefinitionDictionary = await Task.Run(() => VaultConnection.PropertyManager.GetPropertyDefinitions(VDF.Vault.Currency.Entities.EntityClassIds.Items, null, VDF.Vault.Currency.Properties.PropertyDefinitionFilter.IncludeAll | VDF.Vault.Currency.Properties.PropertyDefinitionFilter.OnlyActive));
            vltConfig.VaultItemPropertyMapping = await Task.Run(() => GetPropertyMapping(VDF.Vault.Currency.Entities.EntityClassIds.Items, 
                vltConfig.VaultItemPropertyDefinitionDictionary.Where(x => (!x.Value.IsSystem || x.Value.SystemName.Equals("Title(Item,CO)") || x.Value.SystemName.Equals("Description(Item,CO)")) && !x.Value.IsDynamic).Select(x => x.Value).ToList()));

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la définition des catégories des articles" });
            vltConfig.VaultItemCategoryList = await Task.Run(() => VaultConnection.CategoryManager.GetAvailableCategories(VDF.Vault.Currency.Entities.EntityClassIds.Items, true).ToList());


            //propDef.Active == false || propDef.IsCalculated == true || (propDef.IsSystem == true && !(propDef.SystemName.Equals("Title(Item,CO)") || propDef.SystemName.Equals("Description(Item,CO)")))


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

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture des définitions de schéma de numérotation" });
            vltConfig.VaultFileNumberingSchemes = VaultConnection.WebServiceManager.NumberingService.GetNumberingSchemes(VDF.Vault.Currency.Entities.EntityClassIds.Files, NumSchmType.Activated)?.ToList() ?? null;
            vltConfig.VaultItemNumberingSchemes = VaultConnection.WebServiceManager.NumberingService.GetNumberingSchemes(VDF.Vault.Currency.Entities.EntityClassIds.Items, NumSchmType.Activated)?.ToList() ?? null;

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "", Timer = "Stop" });

            return vltConfig;
        }

        internal async Task<Dictionary<FolderPathAbsolute, VDF.Vault.Currency.Entities.Folder>> GetTargetVaultFoldersAsync(DataSet data, IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken)
        {
            bool ReportProgress = taskProgReport != null;
            
            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Création des dossiers de destination manquants dans Vault...", Timer = "Start" });
            
            Dictionary<FolderPathAbsolute, VDF.Vault.Currency.Entities.Folder> dictionary = null;

            List<string> Folders = data.Tables["Entities"].AsEnumerable().Where(x => !string.IsNullOrWhiteSpace(x.Field<string>("TargetVaultPath"))).Select(x => x.Field<string>("TargetVaultPath").TrimEnd(new char[] { '/' })).ToList();
            
            if(Folders.Count > 0)
            {
                List<FolderPathAbsolute> VaultFolders = Folders.Select(x => new FolderPathAbsolute(x)).ToList();

                dictionary = await Task.Run(() => VaultConnection.FolderManager.EnsureFolderPathsExist(VaultFolders));            
            }

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "", Timer = "Stop" });

            return dictionary;
        }

        internal async Task<List<string>> GetInventorMaterialAsync(ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken)
        {
            bool ReportProgress = taskProgReport != null;

            List<string> MaterialList = new List<string>();

            if(appOptions.VaultPropertyFieldMappings.Where(x => x.MustMatchInventorMaterial).Count() > 0)
            {
                if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Attend une instance d'Inventor libre...", Timer = "Start" });
                await Task.Delay(10);

                InventorInstance invInst = _invDispatcher.GetInventorInstance(0);

                try
                {
                    if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Démarrage ou redémarrage d'Inventor..." });
                    await invInst.StartOrRestartInventorAsync();

                    if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "Lecture de la liste des matières disponible dans Inventor..." });
                    await Task.Delay(10);

                    invInst.InventorFileCount++;
                    Inventor.Document invDoc = await Task.Run(() => invInst.InvApp.Documents.Add(Inventor.DocumentTypeEnum.kPartDocumentObject));

                    foreach (Inventor.MaterialAsset assetMaterial in invInst.InvApp.ActiveMaterialLibrary.MaterialAssets)
                    {
                        MaterialList.Add(assetMaterial.DisplayName);
                    }

                    await Task.Run(() => invDoc.Close(true));
                }
                catch
                {
                    await Task.Run(() => invInst.ForceCloseInventor());
                }

                _invDispatcher.ReleaseInventorInstance(0);
            }

            if (ReportProgress) taskProgReport.Report(new TaskProgressReport() { Message = "", Timer = "Stop" });

            return MaterialList;
        }
        #endregion
        

        #region FileProcessing
        internal async Task<DataSet> ProcessFilesAsync(string FileTaskName, DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            if (FileTaskName.Equals("Validate"))
            {
                return await ValidateFilesAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            }
            else if (FileTaskName.Equals("ChangeState"))
            {
                return await TempChangeStateFilesAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            }
            else if (FileTaskName.Equals("PurgeProps"))
            {
                return await PurgePropertyFilesAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            }
            else if (FileTaskName.Equals("Update"))
            {
                return await UpdateFilesAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            }
            else if (FileTaskName.Equals("WaitForBomBlob"))
            {
                return await ForceAndWaitForBomBlobCreationFilesAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            }
            else
            {
                return null;
            }
        }

        #region FileValidation
        internal async Task<DataSet> ValidateFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File")));

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });

            List<string> FieldsMustMatchInventorMaterial = appOptions.VaultPropertyFieldMappings.Where(x => x.MustMatchInventorMaterial).Select(x => x.FieldName).ToList();

            if (TotalCount > 0)
            {
                List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
                new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

                for (int i = 0; i < appOptions.FileValidationProcess; i++)
                {
                    int ProcessId = i;
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => ValidateFileAsync(ProcessId, PopEntity, appOptions.InitialLcsValue, FieldsMustMatchInventorMaterial, appOptions, processProgReport)));

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
                        TaskList.Add(Task.Run(() => ValidateFileAsync(ProcessId, PopEntity, appOptions.InitialLcsValue, FieldsMustMatchInventorMaterial, appOptions, processProgReport)));
                    }
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> ValidateFileAsync(int processId, DataRow dr, string initialLcsValue, List<string> fieldsMustMatchInventorMaterial, ApplicationOptions appOptions, IProgress<ProcessProgressReport> processProgReport)
        {
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
                
                processProgReport.Report(new ProcessProgressReport() { Message = FullVaultName, ProcessIndex = processId, TotalCountInc = 1 });

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
                    if(FullVaultName.StartsWith("$"))
                    {
                        VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.FindLatestFilesByPaths(new string[] { FullVaultName }).FirstOrDefault());
                    }
                    else
                    {
                        string bookmark = string.Empty;
                        ACW.SrchStatus status = null;

                        List<SrchCond> SearchList = new List<SrchCond>();
                        SearchList.Add(new SrchCond()
                        {
                            PropDefId = VaultConfig.VaultFilePropertyDefinitionDictionary["Name"].Id,
                            PropTyp = PropertySearchType.SingleProperty,
                            SrchOper = 3,
                            SrchRule = SearchRuleType.Must,
                            SrchTxt = dr.Field<string>("Name")
                        });


                        ACW.File[] files = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.FindFilesBySearchConditions(SearchList.ToArray(), null, null, true, true, ref bookmark, out status));
                        if (files == null)
                        {
                            resultLogs.Add(CreateLog("Error", "Le fichier '" + FullVaultName + "' n'existe pas dans le Vault."));
                            resultState = StateEnum.Error;
                        }
                        else if (files.Length == 1)
                        {
                            VaultFile = files.FirstOrDefault();
                        }
                        else
                        {
                            resultLogs.Add(CreateLog("Error", "Il existe " + files.Length + " fichiers ayant le nom '" + FullVaultName + "' , impossible d'identifier le fichier à traiter."));
                            resultState = StateEnum.Error;
                        }
                    }
                }
                catch (VaultServiceErrorException VltEx)
                {
                    resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes(VltEx) + " à été retourné lors de l'accès au fichier '" + FullVaultName + "'."));
                    resultState = StateEnum.Error;
                }

                if(resultState != StateEnum.Error)
                {
                    if(VaultFile.MasterId != -1)
                    {
                        resultValues.Add("VaultMasterId", VaultFile.MasterId);
                        resultValues.Add("VaultFolderId", VaultFile.FolderId);

                        if(!string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultPath")))
                        {
                            FolderPathAbsolute targetVaultPath = new FolderPathAbsolute(dr.Field<string>("TargetVaultPath").TrimEnd('/'));
                            if(VaultConfig.FolderPathToFolderDico.ContainsKey(targetVaultPath))
                            {
                                resultValues.Add("TargetVaultFolderId", VaultConfig.FolderPathToFolderDico[targetVaultPath].Id);
                            }
                        }

                        ValidateFileAccessInfo(VaultFile, FullVaultName, resultValues, ref resultState, resultLogs);

                        if (resultState != StateEnum.Error)
                        {
                            ValidateFileSystemProperties(VaultFile, dr, resultValues, ref resultState, resultLogs, appOptions);
                            ValidateFileMaterials(fieldsMustMatchInventorMaterial, dr, resultValues, ref resultState, resultLogs);
                            ValidateFileNumberingSch(VaultFile, dr, resultValues, ref resultState, resultLogs);
                            ValidateFileCategoryInfo(VaultFile, dr, resultValues, ref resultState, resultLogs);
                            ValidateFileLifeCycleInfo(VaultFile, dr, resultValues, ref resultState, resultLogs);
                            ValidateFileLifeCycleStateInfo(VaultFile, dr, initialLcsValue, resultValues, ref resultState, resultLogs);
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

            if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;

            if(resultState == StateEnum.Error) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ErrorInc = 1 });
            else processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, DoneInc = 1 });

            return (processId, dr, resultValues, resultState, resultLogs);
        }

        private void ValidateFileAccessInfo(ACW.File vaultFile, string fullVaultName, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            if (vaultFile.Cloaked)
            {
                resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'a pas les droits nécessaires pour accéder au fichier, il ne peut pas être traité.."));
                resultState = StateEnum.Error;
                return;
            }
            if (vaultFile.CheckedOut)
            {
                try
                {
                    ACW.User user = VaultConnection.WebServiceManager.AdminService.GetUserByUserId(vaultFile.CkOutUserId);
                    resultLogs.Add(CreateLog("Error", "Le fichier est extrait par l'utilisateur '" + user.Name + "', il ne peut pas être traité."));
                }
                catch (VaultServiceErrorException VltEx)
                {
                    resultLogs.Add(CreateLog("Error", "Le fichier est extrait par un utilisateur, il ne peut pas être traité."));
                }

                
                resultState = StateEnum.Error;
                return;
            }
        }

        private void ValidateFileSystemProperties(ACW.File vaultFile, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions)
        {
            bool IsAnyCad = false;
            resultValues.Add("VaultLevel", GetFileLevel(vaultFile.MasterId, out IsAnyCad, appOptions.SelectedAnyCadFileExt.Select(x => x.ToString()).ToList()));

            if(IsAnyCad)
            {
                if (appOptions.IsAnyCadFileAnError && appOptions.LogError)
                {
                    resultLogs.Add(CreateLog("Error", "Le fichier contient des références AnyCAD."));
                }
                else if (!appOptions.IsAnyCadFileAnError && appOptions.LogWarning)
                {
                    resultLogs.Add(CreateLog("Warning", "Le fichier contient des références AnyCAD."));
                }
            }
            else
            {
                if(appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le fichier ne contient pas de référence AnyCAD."));
            }
           
            PropInst[] propInsts = VaultConnection.WebServiceManager.PropertyService.GetProperties(VDF.Vault.Currency.Entities.EntityClassIds.Files,
                                        new long[] { vaultFile.Id }, new long[] { VaultConfig.ProviderPropId, VaultConfig.ItemAssignablePropId, VaultConfig.CompliancePropId});

            string ProviderPropName = VaultConfig.VaultFilePropertyDefinitionDictionary.Values.Where(x => x.Id == VaultConfig.ProviderPropId).FirstOrDefault().DisplayName;

            PropInst Provider = propInsts.Where(x => x.PropDefId == VaultConfig.ProviderPropId).FirstOrDefault();
            if (Provider != null && Provider.Val != null)
            {
                resultLogs.Add(CreateLog("Info", ProviderPropName + " = '" + Provider.Val.ToString() + "'."));
                resultValues.Add("VaultProvider", Provider.Val.ToString());
            }
            else
            {
                resultLogs.Add(CreateLog("Error", "Provider incorrecte."));
            }

            string ItemAssignablePropName = VaultConfig.VaultFilePropertyDefinitionDictionary.Values.Where(x => x.Id == VaultConfig.ItemAssignablePropId).FirstOrDefault().DisplayName;

            PropInst ItemAssignable = propInsts.Where(x => x.PropDefId == VaultConfig.ItemAssignablePropId).FirstOrDefault();
            if (ItemAssignable != null && ItemAssignable.Val != null)
            {
                if ((bool)ItemAssignable.Val == true) resultLogs.Add(CreateLog("Info", ItemAssignablePropName + " = '" + ItemAssignable.Val.ToString() + "'."));
                else resultLogs.Add(CreateLog("Warning", ItemAssignablePropName + " = '" + ItemAssignable.Val.ToString() + "'."));
            }

            string CompliancePropName = VaultConfig.VaultFilePropertyDefinitionDictionary.Values.Where(x => x.Id == VaultConfig.CompliancePropId).FirstOrDefault().DisplayName;

            PropInst Compliance = propInsts.Where(x => x.PropDefId == VaultConfig.CompliancePropId).FirstOrDefault();
            if (Compliance != null && Compliance.Val != null)
            {
                if ((double)Compliance.Val == 0) resultLogs.Add(CreateLog("Info", CompliancePropName + " = 'True' (" + Compliance.Val.ToString() + ")."));
                else resultLogs.Add(CreateLog("Warning", CompliancePropName + " = 'False' (" + Compliance.Val.ToString() + ")."));
            }
        }

        private void ValidateFileMaterials(List<string> fieldsMustMatchInventorMaterial, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            foreach (string MustMatchFieldName in fieldsMustMatchInventorMaterial)
            {
                string MaterialToCheck = dr.GetChildRows("EntityNewProp").FirstOrDefault().Field<string>(MustMatchFieldName);
                if (!string.IsNullOrWhiteSpace(MaterialToCheck))
                {
                    if (VaultConfig.InventorMaterials.Contains(MaterialToCheck))
                    {
                        resultLogs.Add(CreateLog("Info", "La matière est valide."));
                    }
                    else
                    {
                        resultLogs.Add(CreateLog("Error", "La matière '" + MaterialToCheck + "' n'existe pas dans la bibliothèque de matières d'Inventor par défaut."));
                        resultState = StateEnum.Error;
                    }
                }
            }
        }

        private void ValidateFileNumberingSch(ACW.File vaultFile, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            string targetVaultNumSch = dr.Field<string>("TargetVaultNumSchName");

            if (!string.IsNullOrWhiteSpace(targetVaultNumSch))
            {
                NumSchm numSchm = VaultConfig.VaultFileNumberingSchemes.Where(x => x.Name.Equals(targetVaultNumSch)).FirstOrDefault();
                if (numSchm != null)
                {
                    resultValues.Add("TargetVaultNumSchId", numSchm.SchmID);
                }
                else
                {
                    resultLogs.Add(CreateLog("Error", "Le schéma de numérotation cible '" + targetVaultNumSch + "' n'existe pas dans le Vault."));
                    resultState = StateEnum.Error;
                }
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

        private void ValidateFileLifeCycleStateInfo(ACW.File vaultFile, DataRow dr, string initialLcsValue, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            resultValues.Add("VaultLcsName", vaultFile.FileLfCyc.LfCycStateName);
            resultValues.Add("VaultLcsId", vaultFile.FileLfCyc.IterLfCycStateId);

            LfCycState TempLcs = null;
            
            string tempVaultlifeCycleStateName = dr.Field<string>("TempVaultLcsName");
            if (!string.IsNullOrWhiteSpace(tempVaultlifeCycleStateName))
            {
                LfCycDef CurrentLcDef = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == vaultFile.FileLfCyc.LfCycDefId).FirstOrDefault();
                
                TempLcs = CurrentLcDef?.StateArray.Where(x => x.DispName == tempVaultlifeCycleStateName).FirstOrDefault() ?? null;
                if(TempLcs == null)
                {
                    resultLogs.Add(CreateLog("Error", "L'état de cycle de vie temporaire '" + tempVaultlifeCycleStateName + "' n'existe pas dans le cycle de vie '" + (CurrentLcDef?.DispName ?? "Base") + "'."));
                    resultState = StateEnum.Error;
                    return;
                }

                resultValues.Add("TempVaultLcsId", TempLcs.Id);
                if (vaultFile.FileLfCyc.IterLfCycStateId != TempLcs.Id)
                {
                    LfCycTrans TempTrans = CurrentLcDef.TransArray.Where(x => x.FromId == vaultFile.FileLfCyc.IterLfCycStateId && x.ToId == TempLcs.Id).FirstOrDefault();
                    if (TempTrans == null)
                    {
                        resultLogs.Add(CreateLog("Error", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + tempVaultlifeCycleStateName + "' n'est pas possible dans le cycle de vie '" + CurrentLcDef.DispName + "'."));
                        resultState = StateEnum.Error;
                        return;
                    }

                    if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TempTrans.Id))
                    {
                        resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + tempVaultlifeCycleStateName + "' dans le cycle de vie '" + CurrentLcDef.DispName + "'."));
                        resultState = StateEnum.Error;
                        return;
                    }

                    if (TempTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                    {
                        resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + tempVaultlifeCycleStateName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise a jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
                    }
                }
            }

            LfCycDef TargetLcDef = null;
            LfCycState TargetLcs = null;

            string targetVaultlifeCycleStateName = dr.Field<string>("TargetVaultLcsName");

            if (targetVaultlifeCycleStateName.Equals(initialLcsValue)) targetVaultlifeCycleStateName = vaultFile.FileLfCyc.LfCycStateName;

            if (!string.IsNullOrWhiteSpace(targetVaultlifeCycleStateName))
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
                    resultLogs.Add(CreateLog("Error", "L'état de cycle de vie cible '" + targetVaultlifeCycleStateName + "' n'existe pas dans le cycle de vie ''."));
                    resultState = StateEnum.Error;
                    return;
                }

                TargetLcs = TargetLcDef.StateArray.Where(x => x.DispName == targetVaultlifeCycleStateName).FirstOrDefault();
                if (TargetLcs == null)
                {
                    resultLogs.Add(CreateLog("Error", "L'état de cycle de vie cible '" + targetVaultlifeCycleStateName + "' n'existe pas dans le cycle de vie '" + TargetLcDef.DispName + "'."));
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
                            resultLogs.Add(CreateLog("Error", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + targetVaultlifeCycleStateName + "' n'est pas possible dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TargetTrans.Id))
                        {
                            resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + targetVaultlifeCycleStateName + "' dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (TargetTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                        {
                            resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + vaultFile.FileLfCyc.LfCycStateName + "' vers l'état '" + targetVaultlifeCycleStateName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise à jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
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
                            resultLogs.Add(CreateLog("Error", "La transition de l'état '" + tempVaultlifeCycleStateName + "' vers l'état '" + targetVaultlifeCycleStateName + "' n'est pas possible dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TargetTrans.Id))
                        {
                            resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + tempVaultlifeCycleStateName + "' vers l'état '" + targetVaultlifeCycleStateName + "' dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (TargetTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                        {
                            resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + tempVaultlifeCycleStateName + "' vers l'état '" + targetVaultlifeCycleStateName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise à jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
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

                if(vaultFile.FileRev.Label.Equals(targetVaultRevisionName))
                {
                    resultLogs.Add(CreateLog("Warning", "Aucun changement de révision passage de '" + vaultFile.FileRev.Label + "' vers '" + targetVaultRevisionName + "'."));

                }
                else if (!ValidateRevisionLabel(vaultFile.FileRev.Label, targetVaultRevisionName, CurrentRevDef))
                {
                    string RevDefName = "Base";
                    if (CurrentRevDef != null) RevDefName = CurrentRevDef.DispName;

                    resultLogs.Add(CreateLog("Error", "Le schéma de révision '" + RevDefName + "' n'autorise pas le passage de la révision '" + vaultFile.FileRev.Label + "' vers '" + targetVaultRevisionName + "'."));
                    resultState = StateEnum.Error;
                }
            }
        }
        #endregion

        #region FileTempChangeState
        internal async Task<DataSet> TempChangeStateFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File") &&
                                                                                                              (x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.Validation && x.Field<StateEnum>("State") == StateEnum.Completed) &&
                                                                                                              x.Field<long?>("VaultMasterId") != null &&
                                                                                                              (!string.IsNullOrWhiteSpace(x.Field<string>("TempVaultLcsName")) && x.Field<long?>("TempVaultLcsId") != null)));

            foreach(DataRow dr in EntitiesStack)
            {
                dr["Task"] = TaskTypeEnum.TempChangeState;
                dr["State"] = StateEnum.Pending;
            }

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Changement d'état temporaire des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });

            if (TotalCount > 0)
            {

                List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
                new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

                for (int i = 0; i < appOptions.FileTempChangeStateProcess; i++)
                {
                    int ProcessId = i;
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => TempChangeStateFileAsync(ProcessId, PopEntity, processProgReport, appOptions)));

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
                        TaskList.Add(Task.Run(() => TempChangeStateFileAsync(ProcessId, PopEntity, processProgReport, appOptions)));
                    }
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Changement d'état temporaire des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> TempChangeStateFileAsync(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport, ApplicationOptions appOptions, int RetryCount = 0)
        {
            RetryCount++;

            Dictionary<string, object> resultValues = new Dictionary<string, object>();
            List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

            StateEnum resultState = StateEnum.Processing;

            if (dr == null)
            {
                if(appOptions.LogError) resultLogs.Add(CreateLog("Error", "La ligne de base de données est 'null', impossible de traiter l'élément."));
                RetryCount = appOptions.MaxRetryCount;
                resultState = StateEnum.Error;
            }

            string FullVaultName = string.Empty;
            ACW.File VaultFile = null;

            if (resultState != StateEnum.Error)
            {
                FullVaultName = dr.Field<string>("Path");
                if (string.IsNullOrWhiteSpace(FullVaultName) || FullVaultName.EndsWith("/")) FullVaultName += dr.Field<string>("Name");
                else FullVaultName += "/" + dr.Field<string>("Name");

                processProgReport.Report(new ProcessProgressReport() { Message = FullVaultName, ProcessIndex = processId, TotalCountInc = 1 });

                if (string.IsNullOrWhiteSpace(FullVaultName))
                {
                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le nom de fichier est vide, impossible de traiter l'élément."));
                    RetryCount = appOptions.MaxRetryCount;
                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error)
            {
                try
                {
                    VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileLifeCycleStates(new long[] { dr.Field<long>("VaultMasterId") }, new long[] { dr.Field<long>("TempVaultLcsId") }, "MaJ - Changement d'état").FirstOrDefault());
                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Changement d'état temporaire de '" + dr.Field<string>("VaultLcsName") + "' à '" + dr.Field<string>("TempVaultLcsName") + "'."));
                }
                catch (VaultServiceErrorException VltEx)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes(VltEx) + "' à été retourné lors du changement d'état du fichier '" + FullVaultName + "' vers l'état '" + dr.Field<string>("TempVaultLcsName") + "'."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;


            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                (int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs) Result = await TempChangeStateFileAsync(processId, dr, processProgReport, appOptions, RetryCount);
                resultValues = resultValues.Concat(Result.Result).ToDictionary<string,object>();
                resultState = Result.State;
                resultLogs = resultLogs.Concat(Result.ResultLogs).ToList<Dictionary<string, object>>();
            }
            else
            {
                if (resultState == StateEnum.Error) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ErrorInc = 1 });
                else if (resultState == StateEnum.Completed) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, DoneInc = 1 });
            }

            return (processId, dr, resultValues, resultState, resultLogs);
        }
        #endregion

        #region FilePurgeProperties
        internal async Task<DataSet> PurgePropertyFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File") &&
                                                                                                             (x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.Validation || x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.TempChangeState) && 
                                                                                                              x.Field<StateEnum>("State") == StateEnum.Completed &&
                                                                                                              x.Field<long?>("VaultMasterId") != null));

            foreach (DataRow dr in EntitiesStack)
            {
                dr["Task"] = TaskTypeEnum.PurgeProps;
                dr["State"] = StateEnum.Pending;
            }

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Purge des propriétés des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });

            if (TotalCount > 0)
            {
                List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
                new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

                for (int i = 0; i < appOptions.FilePurgePropsProcess; i++)
                {
                    int ProcessId = i;
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => PurgePropertyFileAsync(ProcessId, PopEntity, processProgReport, appOptions)));

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
                        TaskList.Add(Task.Run(() => PurgePropertyFileAsync(ProcessId, PopEntity, processProgReport, appOptions)));
                    }
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Purge des propriétés des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> PurgePropertyFileAsync(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport, ApplicationOptions appOptions)
        {
            Dictionary<string, object> resultValues = new Dictionary<string, object>();
            List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

            StateEnum resultState = StateEnum.Processing;

            if (dr == null)
            {
                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "La ligne de base de données est 'null', impossible de traiter l'élément."));
                resultState = StateEnum.Error;
            }

            string FullVaultName = string.Empty;
            ACW.File VaultFile = null;

            if (resultState != StateEnum.Error)
            {
                FullVaultName = dr.Field<string>("Path");
                if (string.IsNullOrWhiteSpace(FullVaultName) || FullVaultName.EndsWith("/")) FullVaultName += dr.Field<string>("Name");
                else FullVaultName += "/" + dr.Field<string>("Name");

                processProgReport.Report(new ProcessProgressReport() { Message = FullVaultName, ProcessIndex = processId, TotalCountInc = 1 });

                if (string.IsNullOrWhiteSpace(FullVaultName))
                {
                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le nom de fichier est vide, impossible de traiter l'élément."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error)
            {
                try
                {
                    VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId")));

                    List<long> VaultFilePropIds = await Task.Run(() => VaultConnection.WebServiceManager.PropertyService.GetPropertiesByEntityIds(VDF.Vault.Currency.Entities.EntityClassIds.Files, new long[] { VaultFile.Id }).Select(x => x.PropDefId).ToList());
                    List<long> VaultFileUdpIds = VaultFilePropIds.Where(x => VaultConfig.VaultFilePropertyDefinitionDictionary.Where(y => !y.Value.IsSystem && !y.Value.IsCalculated).Select(y => y.Value.Id).Contains(x)).ToList();

                    long CatId = dr.Field<long>("VaultCatId");
                    if(dr.Field<long?>("TargetVaultCatId") != null) CatId = dr.Field<long>("TargetVaultCatId");

                    CatCfg catCfg = VaultConfig.VaultFileCategoryBehavioursList.Where(x => x.Cat.Id == CatId).FirstOrDefault();
                    
                    List<long> CatPropIds = new List<long>();
                    BhvCfg BhvUdps = null;
                    if (catCfg != null)
                    {
                        BhvUdps = catCfg.BhvCfgArray.Where(x => x.Name.Equals("UserDefinedProperty")).FirstOrDefault();
                        if(BhvUdps != null) CatPropIds = BhvUdps.BhvArray.Select(x => x).Select(x => x.Id).ToList();
                    }

                    List<long> RemoveUdpIds = VaultFileUdpIds.Except(CatPropIds).ToList();
                    List<string> RemoveUdpNames = VaultConfig.VaultFilePropertyDefinitionDictionary.Where(x => RemoveUdpIds.Contains(x.Value.Id)).Select(y => y.Value.DisplayName).ToList();
                    
                    List<long> AddUdpIds = CatPropIds.Except(VaultFileUdpIds).ToList();
                    List<string> AddUdpNames = VaultConfig.VaultFilePropertyDefinitionDictionary.Where(x => AddUdpIds.Contains(x.Value.Id)).Select(y => y.Value.DisplayName).ToList();

                    long[] RemoveUdpIdsArray = null;
                    long[] AddUdpIdsArray = null;

                    if(appOptions.FilePropertySyncMode == PropertySyncModeEnum.Purge)
                    {
                        if (RemoveUdpIds.Count > 0) RemoveUdpIdsArray = RemoveUdpIds.ToArray();
                    }
                    else if(appOptions.FilePropertySyncMode == PropertySyncModeEnum.Add)
                    {
                        if (AddUdpIds.Count > 0) AddUdpIdsArray = AddUdpIds.ToArray();
                    }
                    else if(appOptions.FilePropertySyncMode == PropertySyncModeEnum.PurgeAndAdd)
                    {
                        if (RemoveUdpIds.Count > 0) RemoveUdpIdsArray = RemoveUdpIds.ToArray();
                        if (AddUdpIds.Count > 0) AddUdpIdsArray = AddUdpIds.ToArray();
                    }

                    if (RemoveUdpIdsArray != null || AddUdpIdsArray != null)
                    {
                        await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.UpdateFilePropertyDefinitions(new long[] { VaultFile.MasterId }, AddUdpIdsArray, RemoveUdpIdsArray, "Purge properties"));

                        if (appOptions.LogInfo && AddUdpIdsArray != null) resultLogs.Add(CreateLog("Info", "Les propriétés '" + string.Join("', '", AddUdpNames) + "' ont été ajoutées."));
                        if (appOptions.LogInfo && RemoveUdpIdsArray != null) resultLogs.Add(CreateLog("Info", "Les propriétés '" + string.Join("', '", RemoveUdpNames) + "' ont été purgées."));
                    }
                    else
                    {
                        if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Pas de purge/ajout de propriétés nécessaire."));
                    }
                }
                catch (VaultServiceErrorException VltEx)
                {
                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes(VltEx) + "' à été retourné lors de a purge des propriété du fichier '" + FullVaultName + "'."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;

            if (resultState == StateEnum.Error) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ErrorInc = 1 });
            else processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, DoneInc = 1 });

            return (processId, dr, resultValues, resultState, resultLogs);
        }
        #endregion

        #region FileUpdate
        internal async Task<DataSet> UpdateFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            List<DataRow> Entities = ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File") &&
                                                                               (x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.Validation || x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.TempChangeState || x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.PurgeProps) &&
                                                                                x.Field<StateEnum>("State") == StateEnum.Completed &&
                                                                                x.Field<long?>("VaultMasterId") != null).ToList(); ;

            foreach (DataRow dr in Entities)
            {
                dr["Task"] = TaskTypeEnum.Update;
                dr["State"] = StateEnum.Pending;
            }

            int TotalCount = Entities.Count;
            int currentLevel = 1;

            int maxLevel = currentLevel;
            if (TotalCount > 0) maxLevel = Entities.Max(x => x.Field<int>("VaultLevel"));

            taskProgReport.Report(new TaskProgressReport() { Message = "Mise à jour des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });
            
            List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
                                            new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

            while (currentLevel <= maxLevel)
            {
                taskProgReport.Report(new TaskProgressReport() { Message = "Mise à jour des fichiers de niveau " + currentLevel + " sur " + maxLevel, TotalEntityCount = TotalCount });
                List<DataRow> CurrentlevelEntities = Entities.Where(x => x.Field<int>("VaultLevel") == currentLevel).ToList();

                while (CurrentlevelEntities.Count > 0)
                {
                    Stack<DataRow> BatchStack = new Stack<DataRow>(CurrentlevelEntities.Take(1000));
                    CurrentlevelEntities.RemoveRange(0, BatchStack.Count);

                    for (int i = 0; i < appOptions.FileUpdateProcess; i++)
                    {
                        int ProcessId = i;
                        DataRow PopEntity = BatchStack.Pop();
                        TaskList.Add(Task.Run(() => UpdateFileAsync(ProcessId, PopEntity, processProgReport, appOptions)));

                        if (BatchStack.Count == 0) break;
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

                        if (BatchStack.Count > 0 && !taskCancellationToken.IsCancellationRequested)
                        {
                            DataRow PopEntity = BatchStack.Pop();
                            TaskList.Add(Task.Run(() => UpdateFileAsync(ProcessId, PopEntity, processProgReport, appOptions)));
                        }
                    }
                }

                currentLevel++;
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Mise à jour des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> UpdateFileAsync(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport, ApplicationOptions appOptions)
        {
            Dictionary<string, object> resultValues = new Dictionary<string, object>();
            List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

            StateEnum resultState = StateEnum.Processing;

            string FullVaultName = string.Empty;

            FullVaultName = dr.Field<string>("Path");
            if (string.IsNullOrWhiteSpace(FullVaultName) || FullVaultName.EndsWith("/")) FullVaultName += dr.Field<string>("Name");
            else FullVaultName += "/" + dr.Field<string>("Name");

            processProgReport.Report(new ProcessProgressReport() { Message = FullVaultName , ProcessIndex = processId, TotalCountInc = 1 });

            if (resultState != StateEnum.Error) resultState = await UpdateFileCategoryAsync(FullVaultName, dr, resultState, resultLogs, appOptions);
            if (resultState != StateEnum.Error) resultState =await MoveFileAsync(FullVaultName, dr, resultState, resultLogs, appOptions);
            if (resultState != StateEnum.Error) resultState = await RenameFileAsync(FullVaultName, dr, resultState, resultLogs, appOptions);
            if (resultState != StateEnum.Error) resultState = await UpdateFileLifeCycleAsync(FullVaultName, dr, resultState, resultLogs, appOptions);
            if (resultState != StateEnum.Error) resultState = await UpdateFileRevisionAsync(FullVaultName, dr, resultState, resultLogs, appOptions);
            if (resultState != StateEnum.Error) resultState = await UpdateFilePropertyAsync(FullVaultName, dr, resultState, resultLogs, appOptions, processId, processProgReport);
            if (resultState != StateEnum.Error) resultState = await UpdateFileLifeCycleStateAsync(FullVaultName, dr, resultState, resultLogs, appOptions);
            if (resultState != StateEnum.Error) resultState = await CreateBomBlobJobAsync(FullVaultName, dr, resultValues, resultState, resultLogs, appOptions);

            if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;

            if (resultState == StateEnum.Error) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ErrorInc = 1 });
            else processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, DoneInc = 1 });

            return (processId, dr, resultValues, resultState, resultLogs);
        }

        private async Task<StateEnum> UpdateFileCategoryAsync(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 0)
        {
            RetryCount++;

            if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultCatId") != null && dr.Field<long?>("VaultCatId") != dr.Field<long?>("TargetVaultCatId"))
            {
                try
                {
                    ACW.File VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileCategories(new long[] { dr.Field<long>("VaultMasterId") }, new long[] { dr.Field<long>("TargetVaultCatId") }, "MaJ - Changement de catégorie").FirstOrDefault());
                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "La catégorie '" + dr.Field<string>("TargetVaultCatName") + "' a été appliqué au fichier."));
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if(RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning)) 
                            resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) + 
                                                                    "' à été retourné lors du changement de catégorie du fichier (essai " + 
                                                                    RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors du changement de catégorie du fichier (essai " + 
                                                                    RetryCount + "/" + appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }
            
            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await UpdateFileCategoryAsync(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
            }

            return resultState;
        }

        private async Task<StateEnum> MoveFileAsync(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 0)
        {            
            RetryCount++;

            if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultFolderId") != null && dr.Field<long?>("VaultFolderId") != dr.Field<long?>("TargetVaultFolderId"))
            {
                try
                {
                    await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.MoveFile(dr.Field<long>("VaultMasterId"), dr.Field<long>("VaultFolderId"), dr.Field<long>("TargetVaultFolderId")));
                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le fichier a été déplacé de '" + dr.Field<string>("Path") + "' vers '" + dr.Field<string>("TargetVaultPath") + "'."));
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning)) 
                            resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                    "' à été retourné lors du déplacement du fichier (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning)) 
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors du déplacement du fichier (essai " + RetryCount + "/" +
                                                                    appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }


            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await MoveFileAsync(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
            }

            return resultState;
        }

        private async Task<StateEnum> RenameFileAsync(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        {
            //TODO globalize retry and Error/Warning logs...

            string Ext = System.IO.Path.GetExtension(dr.Field<string>("Name"));
            string NewName = string.Empty;

            if (dr.Field<long?>("TargetVaultNumSchId") == null && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultName")))
            {
                NewName = dr.Field<string>("TargetVaultName");
                if (!NewName.EndsWith(Ext, StringComparison.InvariantCultureIgnoreCase)) NewName += Ext;
            }
            else if (dr.Field<long?>("TargetVaultNumSchId") != null && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultName")) && dr.Field<string>("TargetVaultName").Equals("NextNumber"))
            {
                try
                {
                    NewName = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GenerateFileNumber(dr.Field<long>("TargetVaultNumSchId"), null));
                    NewName += Ext;
                }
                catch (Exception Ex)
                {
                    if (Ex is VaultServiceErrorException)
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                                   "' à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") + 
                                                                                   "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "."));
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") +
                                                                                   "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "." + System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }
            else if (dr.Field<long?>("TargetVaultNumSchId") != null && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultName")) && dr.Field<string>("TargetVaultName").StartsWith("NextNumber="))
            {
                string NumSchInput = dr.Field<string>("TargetVaultName").Substring("NextNumber=".Length).Trim();
                try
                {
                    NewName = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GenerateFileNumber(dr.Field<long>("TargetVaultNumSchId"), NumSchInput.Split('|')));
                    NewName += Ext;
                }
                catch (Exception Ex)
                {
                    if (Ex is VaultServiceErrorException)
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                                   "' à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") + 
                                                                                   "' et les paramètres '" +  NumSchInput + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "."));
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") +
                                                                                   "' et les paramètres '" + NumSchInput + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "." + 
                                                                                   System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error && !string.IsNullOrWhiteSpace(NewName) && NewName != dr.Field<string>("Name"))
            {            
                FileRenameRestric[] fileRenameRestrics = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GetFileRenameRestrictionsByMasterId(dr.Field<long>("VaultMasterId"), NewName));

                try
                {
                    VDF.Vault.Settings.AcquireFilesSettings AcquireSettings = new VDF.Vault.Settings.AcquireFilesSettings(VaultConnection);

                    AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeAttachments = false;
                    AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeChildren = false;
                    AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeLibraryContents = false;
                    AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeParents = false;
                    AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeRelatedDocumentation = false;
                    AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.VersionGatheringOption = VDF.Vault.Currency.VersionGatheringOption.Actual;

                    ACW.File File = VaultConnection.WebServiceManager.DocumentService.GetLatestFilesByMasterIds(new long[] { dr.Field<long>("VaultMasterId") }).FirstOrDefault();

                    AcquireSettings.AddFileToAcquire(new VDF.Vault.Currency.Entities.FileIteration(VaultConnection, File), VDF.Vault.Settings.AcquireFilesSettings.AcquisitionOption.Checkout);

                    VDF.Vault.Results.AcquireFilesResults AcquireResults = await VaultConnection.FileManager.AcquireFilesAsync(AcquireSettings);
                    
                    if(!AcquireResults.IsCancelled && AcquireResults.FileResults.FirstOrDefault().Status == VDF.Vault.Results.FileAcquisitionResult.AcquisitionStatus.Success)
                    {
                        FileIteration AcquiredFile = AcquireResults.FileResults.FirstOrDefault().File;
                     
                        ACW.File NewFileVer = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.CheckinUploadedFile(dr.Field<long>("VaultMasterId"), "MaJ - Renommage du fichier", false, DateTime.Now,
                                                                        GetFileAssocParamByMasterId(dr.Field<long>("VaultMasterId")), null, false, NewName,
                                                                        AcquiredFile.FileClassification, AcquiredFile.IsHidden, null));

                        if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le fichier a été renommé de '" + dr.Field<string>("Name") + "' en '" + NewName + "'."));
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le renommage n'est pas possible car le fichier ne peut être extrait."));
                        resultState = StateEnum.Error;
                    }
                }
                catch (Exception Ex)
                {
                    if (Ex is VaultServiceErrorException)
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                                   "' à été retourné lors du renommage du fichier '" + dr.Field<string>("Name") + "' en '" + NewName + 
                                                                                   "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors du renommage du fichier '" + dr.Field<string>("Name") + "' en '" + NewName +
                                                                                   "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
                    }

                    try
                    {
                        ACW.ByteArray downloadTicket;
                        await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.UndoCheckoutFile(dr.Field<long>("VaultMasterId"), out downloadTicket));
                    }
                    catch (Exception UndoEx)
                    {
                        if (Ex is VaultServiceErrorException)
                        {
                            if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)UndoEx) +
                                                                                       "' à été retourné lors de l'annulation de l'extraction suite a une erreur de renommage" +
                                                                                       " (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                        }
                        else
                        {
                            if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'annulation de l'extraction suite a une erreur de renommage" +
                                                                                       " (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." + System.Environment.NewLine + UndoEx.ToString()));
                        }
                    }

                    resultState = StateEnum.Error;
                }
            }

            RetryCount++;
            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                resultState = await RenameFileAsync(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
            }

            return resultState;
        }

        private async Task<StateEnum> UpdateFilePropertyAsync(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int processId, IProgress<ProcessProgressReport> processProgReport)
        {
            //TODO globalize retry and Error/Warning logs...
            string NewInventorMaterialName = string.Empty;

            if (resultState != StateEnum.Error && appOptions.VaultPropertyFieldMappings.Count > 0)
            {
                try
                {
                    ACW.File file = VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId"));

                    string FileProviderName = dr.Field<string>("VaultProvider");

                    ContentSourceProvider Provider = VaultConnection.ConfigurationManager.GetContentSourceProviders().Where(x => x.DisplayName == FileProviderName).FirstOrDefault();

                    List<ACW.PropInstParam> UpdateUdps = new List<ACW.PropInstParam>();
                    List<string> UdpNames = new List<string>();
                    List<ACW.PropWriteReq> UpdateFileProps = new List<ACW.PropWriteReq>();
                    List<string> FilePropNames = new List<string>();

                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "Start processing file '" + dr.Field<string>("Name") + "'" + System.Environment.NewLine);

                    CatCfg catCfg = VaultConfig.VaultFileCategoryBehavioursList.Where(x => x.Cat.Id == file.Cat.CatId).FirstOrDefault();
                    if (catCfg != null)
                    {
                        BhvCfg bhvCfg = catCfg.BhvCfgArray.Where(x => x.Name.Equals("UserDefinedProperty")).FirstOrDefault();
                        if (bhvCfg != null)
                        {
                            foreach (PropertyFieldMapping fMapping in appOptions.VaultPropertyFieldMappings.Where(x => x.VaultPropertySet.Equals("File")))
                            {
                                PropertyDefinition pDef = VaultConfig.VaultFilePropertyDefinitionDictionary.Values.Where(x => x.DisplayName.Equals(fMapping.VaultPropertyDisplayName)).FirstOrDefault();

                                if (bhvCfg.BhvArray.Select(x => x.Id).Contains(pDef.Id))
                                {
                                    string stringVal = dr.GetChildRows("EntityNewProp").FirstOrDefault().Field<string>(fMapping.FieldName);

                                    if (!string.IsNullOrEmpty(stringVal))
                                    {
                                        object objectVal = ToObject(stringVal, pDef.ManagedDataType, file.Name, appOptions.ClearPropValue, appOptions.SyncPartNumberValue);

                                        UpdateUdps.Add(new ACW.PropInstParam() { PropDefId = pDef.Id, Val = objectVal });
                                        UdpNames.Add(pDef.DisplayName);

                                        if (VaultConfig.VaultFilePropertyMapping.ContainsKey(pDef.SystemName) && VaultConfig.VaultFilePropertyMapping[pDef.SystemName].ContainsKey(Provider.SystemName))
                                        {
                                            ContentSourcePropertyMapping cSourceMappings = VaultConfig.VaultFilePropertyMapping[pDef.SystemName][Provider.SystemName].FirstOrDefault();

                                            if (cSourceMappings.ContentPropertyDefinition.Moniker.Equals("Material!{32853F0F-3444-11D1-9E93-0060B03C1CA6}!nvarchar"))
                                            {
                                                NewInventorMaterialName = stringVal;
                                                UpdateUdps.Remove(UpdateUdps.LastOrDefault());
                                                UdpNames.Remove(UdpNames.LastOrDefault());
                                            }
                                            else
                                            {
                                                UpdateFileProps.Add(new ACW.PropWriteReq()
                                                {
                                                    CanCreate = cSourceMappings.ContentPropertyDefinition.SupportCreate,
                                                    Moniker = cSourceMappings.ContentPropertyDefinition.Moniker,
                                                    Val = objectVal
                                                });
                                                FilePropNames.Add(pDef.DisplayName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (VaultConfig.VaultFilePropertyMapping.ContainsKey("Revision") && VaultConfig.VaultFilePropertyMapping["Revision"].ContainsKey(Provider.SystemName))
                    {
                        ContentSourcePropertyMapping cSourceMappings = VaultConfig.VaultFilePropertyMapping["Revision"][Provider.SystemName].FirstOrDefault();

                        UpdateFileProps.Add(new ACW.PropWriteReq()
                        {
                            CanCreate = cSourceMappings.ContentPropertyDefinition.SupportCreate,
                            Moniker = cSourceMappings.ContentPropertyDefinition.Moniker,
                            Val = file.FileRev.Label
                        });
                        FilePropNames.Add(VaultConfig.VaultFilePropertyDefinitionDictionary["Revision"].DisplayName);
                    }

                    if (UpdateUdps.Count > 0 || UpdateFileProps.Count > 0)
                    {
                        // Checkout
                        ACW.ByteArray downloadTicket = null;
                        file = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.CheckoutFile(new FileIteration(VaultConnection, file).EntityIterationId,
                                                    ACW.CheckoutFileOptions.Master, System.Environment.MachineName, "", "MaJ - Mise à jour des propriétés", out downloadTicket));

                        System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Checkout" + System.Environment.NewLine);

                        if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Extraction du fichier pour mise à jour."));

                        // update Vault UDPs
                        if (UpdateUdps.Count > 0)
                        {
                            await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.UpdateFileProperties(new long[] { dr.Field<long>("VaultMasterId") },
                                                 new ACW.PropInstParamArray[] { new ACW.PropInstParamArray() { Items = UpdateUdps.ToArray() } }));

                            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Update UDP" + System.Environment.NewLine);

                            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Mise à jour des propriétés du fichier dans Vault:" + System.Environment.NewLine +
                                                                             string.Join(System.Environment.NewLine, UpdateUdps.Select(x => "   - " + UdpNames[UpdateUdps.IndexOf(x)] + " = " + (x.Val?.ToString() ?? "")))));
                        }

                        // Update file properties
                        ByteArray uploadTicket = null;
                        if (UpdateFileProps.Count > 0)
                        {
                            ACW.PropWriteResults PropUpdateresults;
                            uploadTicket = await Task.Run(() => VaultConnection.WebServiceManager.FilestoreService.CopyFile(downloadTicket.Bytes, System.IO.Path.GetExtension(file.Name).TrimStart('.'),
                                                                true, new PropWriteRequests() { Requests = UpdateFileProps.ToArray() }, out PropUpdateresults).ToByteArray());

                            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Update Properties" + System.Environment.NewLine);

                            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Mise à jour des propriétés du fichier:" + System.Environment.NewLine +
                                                                             string.Join(System.Environment.NewLine, UpdateFileProps.Select(x => "   - " + FilePropNames[UpdateFileProps.IndexOf(x)] + " = " + (x.Val?.ToString() ?? "")))));
                        }

                        // Checkin
                        if (resultState != StateEnum.Error) resultState = await RetryCheckInFileAsync(file, "MaJ - Mise à jour des propriétés", uploadTicket, resultState, resultLogs, appOptions, processId);

                        if (resultState != StateEnum.Error && !string.IsNullOrWhiteSpace(NewInventorMaterialName) && System.IO.Path.GetExtension(fullVaultName).Equals(".ipt", StringComparison.InvariantCultureIgnoreCase))
                        {
                            resultState = await UpdateInventorMaterialAsync(fullVaultName, NewInventorMaterialName, dr, resultState, resultLogs, appOptions, processId, processProgReport);
                        }

                        System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "End processing file '" + dr.Field<string>("Name") + "'" + System.Environment.NewLine);
                    }
                }
                catch (Exception Ex)
                {
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > UpdateFileProperty ERROR" + System.Environment.NewLine);
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", Ex.ToString() + System.Environment.NewLine);

                    if (Ex is VaultServiceErrorException)
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                                   "' à été retourné lors de la mise à jour des propriétés du fichier (essais multiples non implémenté)."));
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de la mise à jour des propriétés du fichier (essais multiples non implémenté)." +
                                                                                   System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            return resultState;
        }

        private async Task<StateEnum> UpdateInventorMaterialAsync(string fullVaultName, string newInventorMaterialName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int processId, IProgress<ProcessProgressReport> processProgReport)
        {
            //TODO globalize retry and Error/Warning logs...
            FileAcquisitionResult AcquiredFile = null;
            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Update Inventor material" + System.Environment.NewLine);

            try
            {
                VDF.Vault.Settings.AcquireFilesSettings AcquireSettings = new VDF.Vault.Settings.AcquireFilesSettings(VaultConnection);

                AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeAttachments = false;
                AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeChildren = true;
                AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.RecurseChildren = true;
                AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeLibraryContents = true;
                AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeParents = false;
                AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeRelatedDocumentation = false;
                AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.VersionGatheringOption = VDF.Vault.Currency.VersionGatheringOption.Actual;

                AcquireSettings.OptionsResolution.OverwriteOption = VDF.Vault.Settings.AcquireFilesSettings.AcquireFileResolutionOptions.OverwriteOptions.ForceOverwriteAll;
                AcquireSettings.OptionsResolution.SyncWithRemoteSiteSetting = VDF.Vault.Settings.AcquireFilesSettings.SyncWithRemoteSite.Always;

                AcquireSettings.OptionsThreading.CancellationToken = new CancellationTokenSource();


                ACW.File File = VaultConnection.WebServiceManager.DocumentService.GetLatestFilesByMasterIds(new long[] { dr.Field<long>("VaultMasterId") }).FirstOrDefault();

                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - file with master ID '" + dr.Field<long>("VaultMasterId") + "' in db has masterId '" + File.MasterId + "' in Vault." + System.Environment.NewLine);
                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - file with master ID '" + dr.Field<long>("VaultMasterId") + "' is checked out '" + File.CheckedOut.ToString() + "'." + System.Environment.NewLine);

                AcquireSettings.AddFileToAcquire(new VDF.Vault.Currency.Entities.FileIteration(VaultConnection, File), VDF.Vault.Settings.AcquireFilesSettings.AcquisitionOption.Checkout | VDF.Vault.Settings.AcquireFilesSettings.AcquisitionOption.Download);

                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - Ready to get files..." + System.Environment.NewLine);
                
                int RetryCount = 0;


                AcquireFilesResults AcquireResults = null;

                do
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    try
                    {
                        Task<AcquireFilesResults> acq = VaultConnection.FileManager.AcquireFilesAsync(AcquireSettings);
                        AcquireSettings.OptionsThreading.CancellationToken.CancelAfter(appOptions.CancelAcquireFileAfter * 1000);
                        await acq;

                        AcquireResults = acq.Result;

                        if (AcquireResults == null || AcquireResults.IsCancelled)
                        {
                            if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                                resultLogs.Add(CreateLog(ErrorLogLevel, "Annulation de l'opération de téléchargement des fichiers après " + appOptions.CancelAcquireFileAfter +
                                                                        " secondes (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));

                            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "      - Download canceled after " + appOptions.CancelAcquireFileAfter + " sec" + System.Environment.NewLine);
                            
                            RetryCount++;
                            AcquireResults = null;
                            await Task.Delay(100);
                        }
                        else
                        {
                            string AcqResult = string.Empty;
                            foreach (FileAcquisitionResult FileAcqRes in AcquireResults.FileResults)
                            {
                                AcqResult += System.Environment.NewLine + " > " + FileAcqRes.LocalPath.FullPath + "(State=" + FileAcqRes.Status.ToString() + ", Options=" + FileAcqRes.AcquisitionOption.ToString() + ")";
                                if (FileAcqRes.Status == FileAcquisitionResult.AcquisitionStatus.Exception)
                                {
                                    AcqResult += System.Environment.NewLine + "   " + FileAcqRes.Exception.ToString().Replace("\n", "\n   ");
                                }
                                else if (FileAcqRes.Status == FileAcquisitionResult.AcquisitionStatus.Restriction)
                                {
                                    AcqResult += System.Environment.NewLine + "   " + FileAcqRes.Restrictions.ToString();
                                }
                            }

                            if (AcquireResults.FileResults.Where(x => x.Status != FileAcquisitionResult.AcquisitionStatus.Success).Count() == 0)
                            {
                                if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Téléchargement des fichiers pour mise à jour de la matière:" + AcqResult));
                                RetryCount = appOptions.MaxRetryCount;
                            }
                            else
                            {
                                if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                                    resultLogs.Add(CreateLog(ErrorLogLevel, "Impossible de télécharger tous les fichiers (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." + AcqResult));

                                RetryCount++;
                                AcquireResults = null;
                                await Task.Delay(100);
                            }
                        }
                    }
                    catch (Exception Ex)
                    {
                        if (Ex is VaultServiceErrorException)
                        {
                            if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                                resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                        "' à été retourné lors du téléchargement des fichiers (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                        }
                        else
                        {
                            if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                                resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors du téléchargement des fichiers (essai " + RetryCount + "/" +
                                                                        appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString().Replace("\n", "\n   ")));
                        }


                        System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - Get files ERROR (Essai " + RetryCount + "/" + appOptions.MaxRetryCount + " ." + System.Environment.NewLine + Ex.ToString() + System.Environment.NewLine);
                        RetryCount++;
                        AcquireResults = null;
                        await Task.Delay(100);
                    }
                } while (RetryCount < appOptions.MaxRetryCount);

                if (AcquireResults != null)
                {
                    AcquiredFile = AcquireResults.FileResults.Where(x => x.File.EntityMasterId == File.MasterId).FirstOrDefault();

                    processProgReport.Report(new ProcessProgressReport() { Message = fullVaultName + " - Attend une instance d'Inventor libre...", ProcessIndex = processId });
                    await Task.Delay(10);
                }
                else
                {
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > UpdateInventorMaterial (AcquireFile) result is Null" + System.Environment.NewLine);
                    resultState = StateEnum.Error;
                }
            }
            catch (Exception Ex)
            {
                if (Ex is VaultServiceErrorException)
                {
                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                               "' à été retourné lors de l'extraction du fichier pour changement de la matière (essais multiples non implémenté)."));
                }
                else
                {
                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'extraction du fichier pour changement de la matière (essais multiples non implémenté)." +
                                                                               System.Environment.NewLine + Ex.ToString()));
                }

                resultState = StateEnum.Error;
            }

            if (resultState != StateEnum.Error && AcquiredFile != null)
            {
                InventorInstance invInst = null;
                try
                {
                    //processProgReport.Report(new ProcessProgressReport() { Message = fullVaultName + " (niveau " + dr.Field<int>("VaultLevel").ToString() + ")" + " - Attend une instance d'Inventor libre...", ProcessIndex = processId });
                    processProgReport.Report(new ProcessProgressReport() { Message = fullVaultName + " - Attend une instance d'Inventor libre...", ProcessIndex = processId });
                    await Task.Delay(10);

                    invInst = _invDispatcher.GetInventorInstance(processId);

                    if(invInst != null)
                    {
                        processProgReport.Report(new ProcessProgressReport() { Message = fullVaultName + " - Démarrage ou redémarrage d'Inventor...", ProcessIndex = processId });
                        System.Threading.Thread.Sleep(10);
                        await Task.Delay(10);
                        await invInst.StartOrRestartInventorAsync();

                        processProgReport.Report(new ProcessProgressReport() { Message = fullVaultName + " - Mise à jour de la matière Inventor...", ProcessIndex = processId });
                        await Task.Delay(10);

                        invInst.InventorFileCount++;
                        Inventor.PartDocument invPartDoc = invInst.InvApp.Documents.Open(AcquiredFile.LocalPath.FullPath) as Inventor.PartDocument;
                        System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - File open" + System.Environment.NewLine);
                        if(invPartDoc.IsModifiable == false)
                        {
                            if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le fichier est de type 'Content Center file' il ne peut pas être moodifié."));
                            resultState = StateEnum.Error;
                        }
                        else
                        {
                            invPartDoc.ActiveMaterial = invInst.InvApp.ActiveMaterialLibrary.MaterialAssets[newInventorMaterialName] as Inventor.Asset;
                            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - Material updated" + System.Environment.NewLine);

                            invPartDoc.Save2(false);
                            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - File saved" + System.Environment.NewLine);

                            invInst.InvApp.Documents.CloseAll();
                            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - File closed" + System.Environment.NewLine);
                            
                            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "La matière du fichier Inventor a été changée pour '" + newInventorMaterialName + "'."));
                        }

                        processProgReport.Report(new ProcessProgressReport() { Message = fullVaultName, ProcessIndex = processId });
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Impossible d'optenir une instance d'Inventor."));
                        resultState = StateEnum.Error;
                    }
                }
                catch (Exception Ex)
                {
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > UpdateInventorMaterial (Update Inventor material) ERROR" + System.Environment.NewLine);

                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors du changement de la matière." +
                                                                                System.Environment.NewLine + Ex.ToString()));

                    if (invInst != null) await Task.Run(() => invInst.ForceCloseInventor());

                    resultState = StateEnum.Error;
                }

                try
                {
                    _invDispatcher.ReleaseInventorInstance(processId);
                }
                catch(Exception Ex)
                {
                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error && AcquiredFile != null)
            { 
                try
                {
                    if (resultState != StateEnum.Error) resultState = await RetryCheckInFileAsync(AcquiredFile.File, "MaJ - Changement matière Inventor", AcquiredFile.LocalPath.FullPath, resultState, resultLogs, appOptions, processId);
                }
                catch (Exception Ex)
                {
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > UpdateInventorMaterial (Checkin) ERROR" + System.Environment.NewLine);
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", Ex.ToString() + System.Environment.NewLine);

                    if (Ex is VaultServiceErrorException)
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                                   "' à été retourné lors de l'archivage du fichier après changement de la matière (essais multiples non implémenté)."));
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'archivage du fichier après changement de la matière (essais multiples non implémenté)." +
                                                                                   System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Error && AcquiredFile != null)
            {
                try
                {
                    ACW.ByteArray downloadTicket;
                    await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.UndoCheckoutFile(dr.Field<long>("VaultMasterId"), out downloadTicket));

                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "    - File checkout canceled" + System.Environment.NewLine);

                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "L'extraction a été annulée suite a une erreur lors de la mise a jour de la matière."));
                }
                catch (Exception Ex)
                {
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > UpdateInventorMaterial (UndoCheckout) ERROR" + System.Environment.NewLine);
                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", Ex.ToString() + System.Environment.NewLine);

                    if (Ex is VaultServiceErrorException)
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                                   "' à été retourné lors de l'annulation de l'extraction du fichier suite au changement de la matière (essais multiples non implémenté)."));
                    }
                    else
                    {
                        if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'annulation de l'extraction du fichier suite au changement de la matière (essais multiples non implémenté)." +
                                                                                   System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            return resultState;
        }

        private async Task<StateEnum> UpdateFileLifeCycleAsync(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 0)
        {
            RetryCount++;

            if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultLcId") != null)
            {
                try
                {
                    ACW.File VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId")));

                    if (VaultFile != null & dr.Field<long?>("TargetVaultLcId") != VaultFile.FileLfCyc.LfCycDefId) 
                    {
                        LfCycState DefaultLcs = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == dr.Field<long>("TargetVaultLcId")).FirstOrDefault().StateArray.Where(y => y.IsDflt).FirstOrDefault();

                        await Task.Run(() => VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileLifeCycleDefinitions(new long[] { dr.Field<long>("VaultMasterId") },
                                                new long[] { dr.Field<long>("TargetVaultLcId") }, new long[] { DefaultLcs.Id }, "MaJ - Changement de cycle de vie"));

                        if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le cycle de vie '" + dr.Field<string>("TargetVaultLcName") + "' et l'état '" + DefaultLcs.DispName + "' ont été appliqués au fichier."));
                    }
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if ((Ex as VaultServiceErrorException).ErrorCode == 3109)
                        {
                            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le fichier est déjà dans le cycle de vie '" + dr.Field<string>("TargetVaultLcName") + "'."));
                            RetryCount = appOptions.MaxRetryCount;
                        }
                        else
                        {
                            if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning)) 
                                resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                        "' à été retourné lors du changement de cycle de vie du fichier vers '" + dr.Field<string>("TargetVaultLcName") +
                                                                        "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                        }
                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors du changement de cycle de vie du fichier vers '" + dr.Field<string>("TargetVaultLcName") +
                                                                    "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await UpdateFileLifeCycleAsync(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
            }

            return resultState;
        }

        private async Task<StateEnum> UpdateFileRevisionAsync(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 0)
        {
            RetryCount++;

            if (resultState != StateEnum.Error && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
            {
                string Label = dr.Field<string>("TargetVaultRevLabel");
                if (Label.Equals("NextPrimary") || Label.Equals("NextSecondary") || Label.Equals("NextTertiary"))
                {
                    StringArray revArray = await Task.Run(() => VaultConnection.WebServiceManager.RevisionService.GetNextRevisionNumbersByMasterIds(new long[] { dr.Field<long>("VaultMasterId") }, 
                                                                new long[] { dr.Field<long>("TargetVaultRevSchId") }).FirstOrDefault());

                    if (Label.Equals("NextPrimary")) Label = revArray.Items[0];
                    else if (Label.Equals("NextSecondary")) Label = revArray.Items[1];
                    else if (Label.Equals("NextTertiary")) Label = revArray.Items[2];
                }

                try
                {
                    ACW.File VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId")));
                    
                    if (dr.Field<long?>("TargetVaultRevSchId") != null && dr.Field<long>("TargetVaultRevSchId") != VaultFile.FileRev.RevDefId)
                    {
                        await Task.Run(() => VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateRevisionDefinitionAndNumbers(new long[] { VaultFile.Id }, 
                                             new long[] { dr.Field<long>("TargetVaultRevSchId") }, new string[] { Label }, "MaJ - Changement de révision"));
                    }
                    else if (dr.Field<long>("TargetVaultRevSchId") == VaultFile.FileRev.RevDefId && Label != VaultFile.FileRev.Label)
                    {
                        await Task.Run(() => VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileRevisionNumbers(new long[] { VaultFile.Id }, 
                                             new string[] { Label }, "MaJ - Changement de révision"));
                    }

                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "La révison a été changée pour '" + Label + "'."));
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning)) 
                            resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                    "' à été retourné lors de la mise à jour de la révision du fichier (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." +
                                                                    System.Environment.NewLine + "TargetVaultRevSchName = " + dr.Field<string>("TargetVaultRevSchName") +
                                                                    System.Environment.NewLine + "VaultRevSchName = " + dr.Field<string>("VaultRevSchName") +
                                                                    System.Environment.NewLine + "Label = " + Label));

                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning)) 
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors de la mise à jour de la révision du fichier (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." +
                                                                    System.Environment.NewLine + "TargetVaultRevSchName = " + dr.Field<string>("TargetVaultRevSchName") +
                                                                    System.Environment.NewLine + "VaultRevSchName = " + dr.Field<string>("VaultRevSchName") +
                                                                    System.Environment.NewLine + "Label = " + Label +
                                                                    System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await UpdateFileRevisionAsync(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
            }

            return resultState;
        }

        private async Task<StateEnum> UpdateFileLifeCycleStateAsync(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 0)
        {
            RetryCount++;

            ACW.File VaultFile = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId")));

            if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultLcsId") != null && dr.Field<long?>("TargetVaultLcsId") != VaultFile.FileLfCyc.LfCycStateId)
            {
                try
                {
                    await Task.Run(() => VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileLifeCycleStates(new long[] { dr.Field<long>("VaultMasterId") }, 
                                         new long[] { dr.Field<long>("TargetVaultLcsId") }, "MaJ - Changement d'état"));

                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "L'état de cycle de vie '" + dr.Field<string>("TargetVaultLcsName") + "' à été appliqué au fichier."));
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                    "' à été retourné lors du changement d'état de cycle de vie du fichier vers '" +
                                                                    dr.Field<string>("TargetVaultLcsName") + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors du changement d'état de cycle de vie du fichier vers '" +
                                                                    dr.Field<string>("TargetVaultLcsName") + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." +
                                                                    System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            
            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await UpdateFileLifeCycleStateAsync(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
            }

            return resultState;
        }

        private async Task<StateEnum> CreateBomBlobJobAsync(string fullVaultName, DataRow dr, Dictionary<string, object> resultValues, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 0)
        {
            RetryCount++;

            if (resultState != StateEnum.Error)
            {
                try
                {
                    string Ext = System.IO.Path.GetExtension(dr.Field<string>("Name"));

                    if (Ext.Equals(".ipt", StringComparison.InvariantCultureIgnoreCase) || Ext.Equals(".iam", StringComparison.InvariantCultureIgnoreCase))
                    {
                        int Count = 1;
                        if (dr.Field<int?>("JobSubmitCount") != null) Count = dr.Field<int>("JobSubmitCount") + 1;

                        resultValues.Add("JobSubmitCount", Count++);

                        JobParam param1 = new JobParam();
                        param1.Name = "EntityClassId";
                        param1.Val = "File";

                        JobParam param2 = new JobParam();
                        param2.Name = "FileMasterId";
                        param2.Val = dr.Field<long>("VaultMasterId").ToString();

                        Job job = await Task.Run(() => VaultConnection.WebServiceManager.JobService.AddJob("autodesk.vault.extractbom.inventor", "HE-CreateBomBlob: " + fullVaultName, 
                                                       new JobParam[] { param1, param2 }, 10));

                        if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le job de création du BOM Blob a été soumis."));
                    }                    
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if((Ex as VaultServiceErrorException).ErrorCode == 237)
                        {
                            if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                                resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                        "' à été retourné lors de la soumission du job de création du BOM Blob." + System.Environment.NewLine +
                                                                        "Le job est déjà présent dans la queue du job processeur!"));

                            RetryCount = appOptions.MaxRetryCount;
                        }
                        else
                        {
                            if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                                resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
                                                                        "' à été retourné lors de la soumission du job de création du BOM Blob (essai " + 
                                                                        RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                        }

                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors de la soumission du job de création du BOM Blob (essai " + 
                                                                    RetryCount + "/" + appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
                    }

                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await CreateBomBlobJobAsync(fullVaultName, dr, resultValues, StateEnum.Processing, resultLogs, appOptions, RetryCount);
            }

            return resultState;
        }
        #endregion

        #region FileWaitForBomBlob
        internal async Task<DataSet> ForceAndWaitForBomBlobCreationFilesAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });

            DataSet ds = data.Copy();

            List<DataRow> Entities = ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File") &&
                                                                                          x.Field<int?>("JobSubmitCount") != null &&
                                                                                          x.Field<long?>("VaultMasterId") != null).ToList();

            foreach (DataRow dr in Entities)
            {
                dr["Task"] = TaskTypeEnum.WaitForBomBlob;
                dr["State"] = StateEnum.Pending;
            }
            int TotalCount = Entities.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Attente des information de nomenclature des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });

            List<long> PendingJobMasterIds = new List<long>();
            List<long> RunningJobMasterIds = new List<long>();
            List<long> ErrorJobMasterIds = new List<long>();
            List<long> DoneJobMasterIds = new List<long>();

            TimeSpan from = new TimeSpan(24, 0, 0);
            
            while (Entities.Count > 0)
            {
                processProgReport.Report(new ProcessProgressReport() { Message = "Contrôle de la file d'attente du job processeur...", ProcessIndex = 0 });
                Job[] AllBomBlobJobs = await Task.Run(() => VaultConnection.WebServiceManager.JobService.GetJobsByDate(10000, DateTime.Now.Subtract(from)));

                if (AllBomBlobJobs != null)
                {
                    PendingJobMasterIds = AllBomBlobJobs.Where(x => x.Typ.Equals("autodesk.vault.extractbom.inventor") && x.StatusCode == JobStatus.Ready).Select(x => long.Parse(x.ParamArray[1].Val)).ToList();
                    RunningJobMasterIds = AllBomBlobJobs.Where(x => x.Typ.Equals("autodesk.vault.extractbom.inventor") && x.StatusCode == JobStatus.Running).Select(x => long.Parse(x.ParamArray[1].Val)).ToList();
                    ErrorJobMasterIds = AllBomBlobJobs.Where(x => x.Typ.Equals("autodesk.vault.extractbom.inventor") && x.StatusCode == JobStatus.Failure).Select(x => long.Parse(x.ParamArray[1].Val)).ToList();
                }
                else
                {
                    PendingJobMasterIds.Clear();
                    RunningJobMasterIds.Clear();
                    ErrorJobMasterIds.Clear();
                }
                DoneJobMasterIds = Entities.Select(x => x.Field<long>("VaultMasterId")).Except(PendingJobMasterIds).Except(RunningJobMasterIds).Except(ErrorJobMasterIds).ToList();


                foreach(long MasterId in ErrorJobMasterIds)
                {
                    DataRow dr = Entities.Where(x => x.Field<long>("VaultMasterId") == MasterId).FirstOrDefault();

                    if(dr != null)
                    {
                        if(dr.Field<int>("JobSubmitCount") < appOptions.MaxJobSubmitionCount)
                        {
                            dr["JobSubmitCount"] = dr.Field<int>("JobSubmitCount") + 1;
                            dr["State"] = StateEnum.Pending;
                            await Task.Run(() => VaultConnection.WebServiceManager.JobService.ResubmitJob(AllBomBlobJobs.Where(x => long.Parse(x.ParamArray[1].Val) == MasterId).FirstOrDefault().Id));
                        }
                        else
                        {
                            dr["State"] = StateEnum.Error;
                            processProgReport.Report(new ProcessProgressReport() { ProcessIndex = 0, TotalCountInc = 1, ErrorInc = 1 });
                        }
                    }
                }

                if(DoneJobMasterIds.Count > 0)
                {
                    List<ACW.File> files = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.GetLatestFilesByMasterIds(DoneJobMasterIds.ToArray()).ToList());

                    List<bool> HasBomBlob = VaultConnection.WebServiceManager.PropertyService.GetProperties(EntityClassIds.Files, files.Select(x => x.Id).ToArray(),
                                                new long[] { VaultConfig.VaultFilePropertyDefinitionDictionary["ItemAssignable"].Id }).Select(x => (bool)x.Val).ToList();

                    for (int i = 0; i < files.Count; i++)
                    {
                        DataRow dr = Entities.Where(x => x.Field<long>("VaultMasterId") == files[i].MasterId).FirstOrDefault();

                        if (dr != null)
                        {
                            if (HasBomBlob[i])
                            {
                                dr["State"] = StateEnum.Completed;
                                processProgReport.Report(new ProcessProgressReport() { ProcessIndex = 0, TotalCountInc = 1, DoneInc = 1 });
                            }
                            else
                            {
                                if (dr.Field<int>("JobSubmitCount") < appOptions.MaxJobSubmitionCount)
                                {
                                    dr["JobSubmitCount"] = dr.Field<int>("JobSubmitCount") + 1;
                                    dr["State"] = StateEnum.Pending;

                                    JobParam param1 = new JobParam();
                                    param1.Name = "EntityClassId";
                                    param1.Val = "File";

                                    JobParam param2 = new JobParam();
                                    param2.Name = "FileMasterId";
                                    param2.Val = dr.Field<long>("VaultMasterId").ToString();

                                    VaultConnection.WebServiceManager.JobService.AddJob("autodesk.vault.extractbom.inventor", "HE-CreateBomBlob: " + dr.Field<string>("Name"), new JobParam[] { param1, param2 }, 10);
                                }
                                else
                                {
                                    dr["State"] = StateEnum.Error;
                                    processProgReport.Report(new ProcessProgressReport() { ProcessIndex = 0, TotalCountInc = 1, ErrorInc = 1 });
                                }
                            }
                        }
                    }
                }


                Entities = Entities.Where(x => x.Field<StateEnum>("State") == StateEnum.Pending).ToList();

                if (Entities.Count > 0 && !taskCancellationToken.IsCancellationRequested)
                {
                    processProgReport.Report(new ProcessProgressReport() { ProcessIndex = 0 });
                    await Task.Delay(5000);
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Attente des information de nomenclature des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }
        #endregion
        #endregion


        #region ItemProcessing
        internal async Task<DataSet> ProcessItemsAsync(string FileTaskName, DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            if (FileTaskName.Equals("Validate"))
            {
                return await ValidateItemsAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            }
            else if (FileTaskName.Equals("ChangeState"))
            {
                return await TempChangeStateItemsAsync(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
            }
            else if (FileTaskName.Equals("PurgeProps"))
            {
                return data;
            }
            else if (FileTaskName.Equals("Update"))
            {
                return data;
            }
            else
            {
                return data;
            }
        }

        #region ItemValidation
        internal async Task<DataSet> ValidateItemsAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("Item")));

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des articles", TotalEntityCount = TotalCount, Timer = "Start" });

            List<long> FileMasterIds = ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<long?>("VaultMasterId") != null).Select(x => x.Field<long>("VaultMasterId")).ToList();

            List<string> FieldsMustMatchInventorMaterial = appOptions.VaultPropertyFieldMappings.Where(x => x.MustMatchInventorMaterial).Select(x => x.FieldName).ToList();

            if (TotalCount > 0)
            {
                List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, List<Dictionary<string, object>> ResultLinks, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
                new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, List<Dictionary<string, object>> ResultLinks, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

                for (int i = 0; i < appOptions.ItemValidationProcess; i++)
                {
                    int ProcessId = i;
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => ValidateItem(ProcessId, PopEntity, FieldsMustMatchInventorMaterial, processProgReport)));

                    if (EntitiesStack.Count == 0) break;
                }


                while (TaskList.Any())
                {
                    Task<(int processId, DataRow entity, Dictionary<string, object> Result, List<Dictionary<string, object>> ResultLinks, StateEnum State, List<Dictionary<string, object>> ResultLogs)> finished = await Task.WhenAny(TaskList);

                    int ProcessId = finished.Result.processId;

                    finished.Result.entity["State"] = finished.Result.State;

                    foreach (KeyValuePair<string, object> kvp in finished.Result.Result)
                    {
                        finished.Result.entity[kvp.Key] = kvp.Value;
                    }

                    foreach (Dictionary<string, object> link in finished.Result.ResultLinks)
                    {
                        DataRow drLink = ds.Tables["Links"].NewRow();
                        drLink["EntityId"] = finished.Result.entity["Id"];

                        foreach (KeyValuePair<string, object> kvp in link)
                        {
                            drLink[kvp.Key] = kvp.Value;
                        }

                        drLink["FoundInEntities"] = FileMasterIds.Contains(drLink.Field<long>("LinkMasterId"));

                        ds.Tables["Links"].Rows.Add(drLink);
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
                        TaskList.Add(Task.Run(() => ValidateItem(ProcessId, PopEntity, FieldsMustMatchInventorMaterial, processProgReport)));
                    }
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Validation des articles", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, List<Dictionary<string, object>> ResultLinks, StateEnum State, List<Dictionary<string, object>> ResultLogs)> ValidateItem(int processId, DataRow dr, List<string> fieldsMustMatchInventorMaterial, IProgress<ProcessProgressReport> processProgReport)
        {
            Dictionary<string, object> resultValues = new Dictionary<string, object>();
            List<Dictionary<string, object>> resultLinks = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

            StateEnum resultState = StateEnum.Processing;

            if (dr == null)
            {
                resultLogs.Add(CreateLog("Error", "La ligne de base de données est 'null', impossible de traiter l'élément."));
                resultState = StateEnum.Error;
            }

            string VaultItemNumber = dr.Field<string>("Name");
            ACW.Item VaultItem = null;

            if (resultState != StateEnum.Error)
            {
                if (string.IsNullOrWhiteSpace(VaultItemNumber))
                {
                    resultLogs.Add(CreateLog("Error", "Le numero d'article est vide, impossible de traiter l'élément."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error)
            {
                try
                {
                    VaultItem = await Task.Run(() => VaultConnection.WebServiceManager.ItemService.GetLatestItemByItemNumber(VaultItemNumber));
                }
                catch (VaultServiceErrorException VltEx)
                {
                    if (VltEx.ErrorCode == 1350)
                    {
                        resultLogs.Add(CreateLog("Error", "L'article '" + VaultItemNumber + "' n'existe pas dans le Vault."));
                    }
                    else if (VltEx.ErrorCode == 303)
                    {
                        resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'a pas les droits nécessaires pour accéder à l'article '" + VaultItemNumber + "'."));
                    }
                    else
                    {
                        resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes(VltEx) + " à été retourné lors de l'accès à l'article '" + VaultItemNumber + "'."));
                    }
                    resultState = StateEnum.Error;
                    VaultItem = null;
                }

                if (resultState != StateEnum.Error)
                {
                    if (VaultItem.MasterId != -1)
                    {
                        resultValues.Add("VaultMasterId", VaultItem.MasterId);

                        ValidateItemAccessInfo(VaultItem, VaultItemNumber, resultValues, ref resultState, resultLogs);

                        if (resultState != StateEnum.Error)
                        {
                            ValidateItemSystemProperties(VaultItem, dr, resultValues, ref resultState, resultLogs);
                            ValidateItemMaterials(fieldsMustMatchInventorMaterial, dr, resultValues, ref resultState, resultLogs);
                            ValidateItemNumberingSch(VaultItem, dr, resultValues, ref resultState, resultLogs);
                            ValidateItemCategoryInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);
                            ValidateItemLifeCycleInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);
                            ValidateItemLifeCycleStateInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);
                            ValidateItemRevisionInfo(VaultItem, dr, resultValues, ref resultState, resultLogs);

                            CollectItemFileLinks(VaultItem, dr, resultLinks, ref resultState, resultLogs);
                        }
                    }
                    else
                    {
                        resultLogs.Add(CreateLog("Error", "Le fichier '" + VaultItemNumber + "' n'existe pas dans le Vault."));
                        resultState = StateEnum.Error;
                    }
                }
            }

            if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;

            if (resultState == StateEnum.Error) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ErrorInc = 1 });
            else processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, DoneInc = 1 });

            return (processId, dr, resultValues, resultLinks, resultState, resultLogs);
        }

        private void ValidateItemAccessInfo(ACW.Item vaultItem, string vaultItemNumber, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            if (vaultItem.IsCloaked)
            {
                resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'a pas les droits nécessaires pour accéder à l'article '" + vaultItemNumber + "', il ne peut pas être traité.."));
                resultState = StateEnum.Error;
                return;
            }

            if (vaultItem.Locked)
            {
                resultLogs.Add(CreateLog("Error", "L'article '" + vaultItemNumber + "' est vérouillé, il ne peut pas être traité."));
                resultState = StateEnum.Error;
                return;
            }
        }

        private void ValidateItemSystemProperties(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            resultValues.Add("VaultLevel", GetItemLevel(vaultItem.Id));

            PropInst[] propInsts = VaultConnection.WebServiceManager.PropertyService.GetProperties(VDF.Vault.Currency.Entities.EntityClassIds.Items,
                                        new long[] { vaultItem.Id }, new long[] { VaultConfig.ProviderPropId });

            PropInst Provider = propInsts.Where(x => x.PropDefId == VaultConfig.ProviderPropId).FirstOrDefault();
            if (Provider != null && Provider.Val != null)
            {
                resultLogs.Add(CreateLog("Info", "Provider '" + Provider.Val.ToString() + "'."));
                resultValues.Add("VaultProvider", Provider.Val.ToString());
            }
            else
            {
                resultLogs.Add(CreateLog("Error", "Provider incorrecte."));
            }
        }

        private void ValidateItemMaterials(List<string> fieldsMustMatchInventorMaterial, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            foreach (string MustMatchFieldName in fieldsMustMatchInventorMaterial)
            {
                string MaterialToCheck = dr.GetChildRows("EntityNewProp").FirstOrDefault().Field<string>(MustMatchFieldName);
                if (!string.IsNullOrWhiteSpace(MaterialToCheck))
                {
                    if (VaultConfig.InventorMaterials.Contains(MaterialToCheck))
                    {
                        resultLogs.Add(CreateLog("Info", "La matière '" + MaterialToCheck + "' est valide."));
                    }
                    else
                    {
                        resultLogs.Add(CreateLog("Error", "La matière '" + MaterialToCheck + "' n'existe pas dans la bibliothèque de matières d'Inventor par défaut."));
                        resultState = StateEnum.Error;
                    }
                }
            }
        }

        private void ValidateItemNumberingSch(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            string targetVaultNumSch = dr.Field<string>("TargetVaultNumSchName");

            if (!string.IsNullOrWhiteSpace(targetVaultNumSch))
            {
                NumSchm numSchm = VaultConfig.VaultItemNumberingSchemes.Where(x => x.Name.Equals(targetVaultNumSch)).FirstOrDefault();
                if (numSchm != null)
                {
                    resultValues.Add("TargetVaultNumSchId", numSchm.SchmID);
                }
                else
                {
                    resultLogs.Add(CreateLog("Error", "Le schéma de numérotation cible '" + targetVaultNumSch + "' n'existe pas dans le Vault."));
                    resultState = StateEnum.Error;
                }
            }
        }

        private void ValidateItemCategoryInfo(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            resultValues.Add("VaultCatName", vaultItem.Cat.CatName);
            resultValues.Add("VaultCatId", vaultItem.Cat.CatId);

            string targetVaultCatName = dr.Field<string>("TargetVaultCatName");

            if (!string.IsNullOrWhiteSpace(targetVaultCatName))
            {
                EntityCategory TargetCat = VaultConfig.VaultItemCategoryList.Where(x => x.Name.Equals(targetVaultCatName)).FirstOrDefault();
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
            resultValues.Add("VaultLcsName", VaultConfig.VaultLifeCycleDefinitionList.SelectMany(x => x.StateArray).Where(y => y.Id == vaultItem.LfCycStateId).FirstOrDefault().DispName);
            resultValues.Add("VaultLcsId", vaultItem.LfCycStateId);
            
            LfCycState TempLcs = null;

            string tempVaultlifeCycleStatName = dr.Field<string>("TempVaultLcsName");
            if (!string.IsNullOrWhiteSpace(tempVaultlifeCycleStatName))
            {
                LfCycDef CurrentLcDef = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == vaultItem.LfCyc.LfCycDefId).FirstOrDefault();

                TempLcs = CurrentLcDef?.StateArray.Where(x => x.DispName == tempVaultlifeCycleStatName).FirstOrDefault() ?? null;
                if (TempLcs == null)
                {
                    resultLogs.Add(CreateLog("Error", "L'état de cycle de vie temporaire '" + tempVaultlifeCycleStatName + "' n'existe pas dans le cycle de vie '" + (CurrentLcDef?.DispName ?? "Base") + "'."));
                    resultState = StateEnum.Error;
                    return;
                }

                resultValues.Add("TempVaultLcsId", TempLcs.Id);
                if (vaultItem.LfCycStateId != TempLcs.Id)
                {
                    LfCycTrans TempTrans = CurrentLcDef.TransArray.Where(x => x.FromId == vaultItem.LfCycStateId && x.ToId == TempLcs.Id).FirstOrDefault();
                    if (TempTrans == null)
                    {
                        resultLogs.Add(CreateLog("Error", "La transition de l'état '" + resultValues["VaultLcsName"] + "' vers l'état '" + tempVaultlifeCycleStatName + "' n'est pas possible dans le cycle de vie '" + CurrentLcDef.DispName + "'."));
                        resultState = StateEnum.Error;
                        return;
                    }

                    if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TempTrans.Id))
                    {
                        resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + resultValues["VaultLcsName"] + "' vers l'état '" + tempVaultlifeCycleStatName + "' dans le cycle de vie '" + CurrentLcDef.DispName + "'."));
                        resultState = StateEnum.Error;
                        return;
                    }

                    if (TempTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                    {
                        resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + resultValues["VaultLcsName"] + "' vers l'état '" + tempVaultlifeCycleStatName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise a jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
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
                    TargetLcDef = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == vaultItem.LfCycStateId).FirstOrDefault();
                }

                if (TargetLcDef == null)
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
                    long FromId = vaultItem.LfCycStateId;
                    if (FromId == 0) FromId = TargetLcDef.StateArray.Where(x => x.IsDflt).FirstOrDefault().Id;

                    if (FromId != TargetLcs.Id)
                    {
                        LfCycTrans TargetTrans = TargetLcDef.TransArray.Where(x => x.FromId == FromId && x.ToId == TargetLcs.Id).FirstOrDefault();
                        if (TargetTrans == null)
                        {
                            resultLogs.Add(CreateLog("Error", "La transition de l'état '" + resultValues["VaultLcsName"] + "' vers l'état '" + targetVaultlifeCycleStatName + "' n'est pas possible dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (!VaultConfig.AllowedStateTransitionIdsList.Contains(TargetTrans.Id))
                        {
                            resultLogs.Add(CreateLog("Error", "L'utilisateur '" + VaultConnection.UserName + "' n'est pas autorisé à effectuer la transition de l'état '" + resultValues["VaultLcsName"] + "' vers l'état '" + targetVaultlifeCycleStatName + "' dans le cycle de vie '" + TargetLcDef.DispName + "'."));
                            resultState = StateEnum.Error;
                            return;
                        }

                        if (TargetTrans.Bump != BumpRevisionEnum.None && !String.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
                        {
                            resultLogs.Add(CreateLog("Warning", "La transition de l'état '" + resultValues["VaultLcsName"] + "' vers l'état '" + targetVaultlifeCycleStatName + "' incrémente la révision. Cela peut rentrer en conflit avec l'option de mise à jour de la révision '" + dr.Field<string>("TargetVaultRevLabel") + "'."));
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

        private void ValidateItemRevisionInfo(ACW.Item vaultItem, DataRow dr, Dictionary<string, object> resultValues, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            long RevSchId = VaultConnection.WebServiceManager.RevisionService.GetRevisionDefinitionIdsByMasterIds(new long[] { vaultItem.MasterId }).FirstOrDefault();
            RevDef CurrentRevDef = VaultConfig.VaultRevisionDefinitionList.Where(x => x.Id == RevSchId).FirstOrDefault();

            resultValues.Add("VaultRevSchId", RevSchId);
            resultValues.Add("VaultRevSchName", VaultConfig.VaultRevisionDefinitionList.Where(x => x.Id == RevSchId).FirstOrDefault().DispName);

            string targetVaultRevisionSchName = dr.Field<string>("TargetVaultRevSchName");
            if (!string.IsNullOrWhiteSpace(targetVaultRevisionSchName))
            {
                long CatId = vaultItem.Cat.CatId;
                string CatName = vaultItem.Cat.CatName;

                if (resultValues.ContainsKey("TargetVaultCatId"))
                {
                    CatId = (long)resultValues["TargetVaultCatId"];
                    CatName = dr.Field<string>("TargetVaultCatName");
                }

                CatCfg catCfg = VaultConfig.VaultItemCategoryBehavioursList.Where(x => x.Cat.Id == CatId).FirstOrDefault();
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

            resultValues.Add("VaultRevLabel", vaultItem.RevNum);

            string targetVaultRevisionName = dr.Field<string>("TargetVaultRevLabel");
            if (!string.IsNullOrWhiteSpace(targetVaultRevisionName) && !targetVaultRevisionName.Equals("NextPrimary") && !targetVaultRevisionName.Equals("NextSecondary") && !targetVaultRevisionName.Equals("NextTertiary"))
            {

                if (resultValues.ContainsKey("TargetVaultRevSchId"))
                {
                    CurrentRevDef = VaultConfig.VaultRevisionDefinitionList.Where(x => x.Id == (long)resultValues["TargetVaultRevSchId"]).FirstOrDefault();
                }

                if (vaultItem.RevNum.Equals(targetVaultRevisionName))
                {
                    resultLogs.Add(CreateLog("Warning", "Aucun changement de révision passage de '" + vaultItem.RevNum + "' vers '" + targetVaultRevisionName + "'."));

                }
                else if (!ValidateRevisionLabel(vaultItem.RevNum, targetVaultRevisionName, CurrentRevDef))
                {
                    string RevDefName = "Base";
                    if (CurrentRevDef != null) RevDefName = CurrentRevDef.DispName;

                    resultLogs.Add(CreateLog("Error", "Le schéma de révision '" + RevDefName + "' n'autorise pas le passage de la révision '" + vaultItem.RevNum + "' vers '" + targetVaultRevisionName + "'."));
                    resultState = StateEnum.Error;
                }
            }
        }

        private void CollectItemFileLinks(Item vaultItem, DataRow dr, List<Dictionary<string, object>> resultLinks, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs)
        {
            ItemFileAssoc[] itemFileAssocs = VaultConnection.WebServiceManager.ItemService.GetItemFileAssociationsByItemIds(new long[] { vaultItem.Id }, ItemFileLnkTypOpt.Primary | ItemFileLnkTypOpt.Secondary | ItemFileLnkTypOpt.Tertiary);
            ItemAttmt itemAttmt = VaultConnection.WebServiceManager.ItemService.GetAttachmentsByItemIds(new long[] { vaultItem.Id }).FirstOrDefault();

            List<long> FileIds = new List<long>();

            if (itemFileAssocs != null)
            {
                FileIds = itemFileAssocs.Select(x => x.CldFileId).ToList();
            }

            if (itemAttmt != null && itemAttmt.AttmtArray != null)
            {
                FileIds = FileIds.Concat(itemAttmt.AttmtArray.Select(x => x.FileId)).ToList();
            }

            if (FileIds.Count > 0)
            {
                ACW.File[] files = VaultConnection.WebServiceManager.DocumentService.GetFilesByIds(FileIds.ToArray());

                Dictionary<string, object> Link = new Dictionary<string, object>();

                foreach (ItemFileAssoc itemFileAssoc in itemFileAssocs)
                {
                    Link = new Dictionary<string, object>();
                    Link.Add("LinkType", itemFileAssoc.Typ.ToString());
                    Link.Add("LinkMasterId", files[FileIds.IndexOf(itemFileAssoc.CldFileId)].MasterId);
                    Link.Add("LinkName", files[FileIds.IndexOf(itemFileAssoc.CldFileId)].Name);
                    resultLinks.Add(Link);
                }

                if (itemAttmt != null && itemAttmt.AttmtArray != null)
                {
                    foreach (Attmt fileAttmt in itemAttmt.AttmtArray)
                    {
                        Link = new Dictionary<string, object>();
                        if (fileAttmt.Pin) Link.Add("LinkType", "PinAttachment");
                        else Link.Add("LinkType", "UnPinAttachment");
                        Link.Add("LinkMasterId", files[FileIds.IndexOf(fileAttmt.FileId)].MasterId);
                        Link.Add("LinkName", files[FileIds.IndexOf(fileAttmt.FileId)].Name);
                        resultLinks.Add(Link);
                    }
                }
            }

        }
        #endregion

        #region ItemTempChangeState
        internal async Task<DataSet> TempChangeStateItemsAsync(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        {
            taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
            DataSet ds = data.Copy();

            Stack<DataRow> EntitiesStack = new Stack<DataRow>(ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("Item") &&
                                                                                                              (x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.Validation && x.Field<StateEnum>("State") == StateEnum.Completed) &&
                                                                                                              x.Field<long?>("VaultMasterId") != null &&
                                                                                                              (!string.IsNullOrWhiteSpace(x.Field<string>("TempVaultLcsName")) && x.Field<long?>("TempVaultLcsId") != null)));

            foreach (DataRow dr in EntitiesStack)
            {
                dr["Task"] = TaskTypeEnum.TempChangeState;
                dr["State"] = StateEnum.Pending;
            }

            int TotalCount = EntitiesStack.Count;

            taskProgReport.Report(new TaskProgressReport() { Message = "Changement d'état temporaire des articles", TotalEntityCount = TotalCount, Timer = "Start" });

            if (TotalCount > 0)
            {

                List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
                new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

                for (int i = 0; i < appOptions.FileTempChangeStateProcess; i++)
                {
                    int ProcessId = i;
                    DataRow PopEntity = EntitiesStack.Pop();
                    TaskList.Add(Task.Run(() => TempChangeStateItem(ProcessId, PopEntity, processProgReport, appOptions)));

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
                        TaskList.Add(Task.Run(() => TempChangeStateItem(ProcessId, PopEntity, processProgReport, appOptions)));
                    }
                }
            }

            taskProgReport.Report(new TaskProgressReport() { Message = "Changement d'état temporaire des articles", TotalEntityCount = TotalCount, Timer = "Stop" });

            return ds;
        }

        private async Task<(int processId, DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)> TempChangeStateItem(int processId, DataRow dr, IProgress<ProcessProgressReport> processProgReport, ApplicationOptions appOptions)
        {
            Dictionary<string, object> resultValues = new Dictionary<string, object>();
            List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

            StateEnum resultState = StateEnum.Processing;

            if (dr == null)
            {
                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "La ligne de base de données est 'null', impossible de traiter l'élément."));
                resultState = StateEnum.Error;
            }

            string VaultItemName = string.Empty;
            ACW.Item VaultItem = null;

            if (resultState != StateEnum.Error)
            {
                VaultItemName = dr.Field<string>("Name");

                processProgReport.Report(new ProcessProgressReport() { Message = VaultItemName, ProcessIndex = processId, TotalCountInc = 1 });

                if (string.IsNullOrWhiteSpace(VaultItemName))
                {
                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le nom d'article est vide, impossible de traiter l'élément."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState != StateEnum.Error)
            {
                try
                {
                    VaultItem = await Task.Run(() => VaultConnection.WebServiceManager.ItemService.UpdateItemLifeCycleStates(new long[] { dr.Field<long>("VaultMasterId") }, new long[] { dr.Field<long>("TempVaultLcsId") }, "MaJ - Changement d'état").FirstOrDefault());
                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Changement d'état temporaire de '" + dr.Field<string>("VaultLcsName") + "' à '" + dr.Field<string>("TempVaultLcsName") + "'."));
                }
                catch (VaultServiceErrorException VltEx)
                {
                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes(VltEx) + "' à été retourné lors du changement d'état de l'article '" + VaultItemName + "' vers l'état '" + dr.Field<string>("TempVaultLcsName") + "'."));
                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;

            if (resultState == StateEnum.Error) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, ErrorInc = 1 });
            else processProgReport.Report(new ProcessProgressReport() { ProcessIndex = processId, DoneInc = 1 });

            return (processId, dr, resultValues, resultState, resultLogs);
        }
        #endregion

        #endregion


        #region CloseInventor
        internal async Task CloseAllInventorAsync(IProgress<TaskProgressReport> taskProgReport, CancellationToken taskCancellationToken)
        {
            bool ReportProgress = taskProgReport != null;
            taskProgReport.Report(new TaskProgressReport() { Message = "Fermeture des instances Inventor...", Timer = "Start" });

            await Task.Run(() => _invDispatcher.CloseAllInventor());

            taskProgReport.Report(new TaskProgressReport() { Message = "", Timer = "Stop" });
            await Task.Delay(10);
        }
        #endregion


        #region PrivateMethods
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

        public object ToObject(string inputString, Type OutputTypeObject, string fileName = "", string ClearValue = "ClearPropValue", string SyncPartNumber = "SyncPartNumber")
        {
            
            if (inputString.Equals(ClearValue)) return null;
            
            if (inputString.Equals(SyncPartNumber)) inputString = System.IO.Path.GetFileNameWithoutExtension(fileName);

            if (OutputTypeObject == typeof(string))
            {
                return inputString;
            }
            else if (OutputTypeObject == typeof(double) && !string.IsNullOrEmpty(inputString))
            {
                double doubleVal;

                if (double.TryParse(inputString, out doubleVal))
                {
                    return doubleVal;
                }
                else
                {
                    if (inputString.Contains("."))
                    {
                        inputString = inputString.Replace(".", ",");
                        if (double.TryParse(inputString, out doubleVal))
                        {
                            return doubleVal;
                        }
                    }
                    else if (inputString.Contains(","))
                    {
                        inputString = inputString.Replace(",", ".");
                        if (double.TryParse(inputString, out doubleVal))
                        {
                            return doubleVal;
                        }
                    }
                }

            }
            else if (OutputTypeObject == typeof(bool) && !string.IsNullOrEmpty(inputString))
            {
                bool boolVal;

                if (bool.TryParse(inputString, out boolVal))
                {
                    return boolVal;
                }
                else
                {
                    if (BooleanTrueValues.Contains(inputString, StringComparer.InvariantCultureIgnoreCase)) return true;
                    else if (BooleanFalseValues.Contains(inputString, StringComparer.InvariantCultureIgnoreCase)) return false;
                }
            }
            else if (OutputTypeObject == typeof(DateTime) && !string.IsNullOrEmpty(inputString))
            {
                DateTime datetimeVal;

                if (DateTime.TryParse(inputString, out datetimeVal))
                    return datetimeVal;
            }

            return null;
        }

        private Dictionary<string, Dictionary<string, IList<ContentSourcePropertyMapping>>> GetPropertyMapping(string entityClassIdFilter, List<PropertyDefinition> propertyDefinitions)
        {
            Dictionary<string, Dictionary<string, IList<ContentSourcePropertyMapping>>> result = new Dictionary<string, Dictionary<string, IList<ContentSourcePropertyMapping>>>();

            foreach (PropertyDefinition pDef in propertyDefinitions)
            {
                if (pDef.Mappings.HasMappings)
                {
                    result.Add(pDef.SystemName, pDef.Mappings.GetContentSourcePropertyMappingsGroupedByProvider(entityClassIdFilter, PropertyMappings.DesiredMappingTypes.Both));
                }
            }
            
            return result;
        }

        private FileAssocParam[] GetFileAssocParamByMasterId(long masterId)
        {
            try
            {
                ArrayList fileAssocParams = new ArrayList();
                FileAssocArray ThisFileAssocArray = VaultConnection.WebServiceManager.DocumentService.GetLatestFileAssociationsByMasterIds(
                                                        new long[] { masterId }, FileAssociationTypeEnum.None, false, FileAssociationTypeEnum.All, false, false, true, false).FirstOrDefault();

                if (ThisFileAssocArray != null && ThisFileAssocArray.FileAssocs != null)
                {
                    foreach (FileAssoc assoc in ThisFileAssocArray.FileAssocs)
                    {
                        FileAssocParam param1 = new FileAssocParam();

                        param1.CldFileId = assoc.CldFile.Id;
                        param1.RefId = assoc.RefId;
                        param1.Source = assoc.Source;
                        param1.Typ = assoc.Typ;
                        param1.ExpectedVaultPath = assoc.ExpectedVaultPath;

                        fileAssocParams.Add(param1);
                    }
                    return (FileAssocParam[])fileAssocParams.ToArray(typeof(FileAssocParam));
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                //MessageBox.Show("Error - Unable to update file assocs." + Environment.NewLine +
                //                "File \"" + DocService.GetFileById(entityIterationId).Name + "\" has lost references." + Environment.NewLine +
                //                "It MUST be open and checked-in from the Source application.", "!!! ERROR !!!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private string GetSubExceptionCodes(VaultServiceErrorException vltEx)
        {
            string result = vltEx.ErrorCode.ToString();
            List<string> restrictionCodes = new List<string>();

            if (vltEx.ErrorCode == 1092 || vltEx.ErrorCode == 1387 || vltEx.ErrorCode == 1633)
            {
                try
                {
                    XmlNodeList nodes = vltEx.Detail["sl:sldetail"]["sl:restrictions"].ChildNodes;
                    foreach (XmlNode node in nodes)
                    {
                        if (node.Name == "sl:restriction")
                        {
                            XmlElement element = node as XmlElement;
                            if (element != null && element.HasAttribute("sl:code") && node.ChildNodes.Count >= 3)
                                restrictionCodes.Add(element.GetAttribute("sl:code") + " (sur enfant '" + node.ChildNodes[2].InnerText?.ToString() ?? "" + "').");
                        }
                    }
                }
                catch
                {
                    restrictionCodes.Add("Impossible d'obtenir les codes de restriction");
                }
               

                if (restrictionCodes.Count > 0)
                {
                    result += " (codes de restriction: " + string.Join(", ", restrictionCodes) + ")";
                }
            }
            
            return result;
        }

        private int GetFileLevel(long masterId, out bool isAnyCad, List<string> anyCadFileExt = null)
        {
            isAnyCad = false;
            try
            {
                FileAssocArray AllFileAssocArray = VaultConnection.WebServiceManager.DocumentService.GetLatestFileAssociationsByMasterIds(new long[] { masterId },
                                                        FileAssociationTypeEnum.None, false, FileAssociationTypeEnum.Dependency, true, false, false, false).FirstOrDefault();

                int Level = 1;
                if (AllFileAssocArray == null || AllFileAssocArray.FileAssocs == null || AllFileAssocArray.FileAssocs.Length == 0)
                {
                    return Level;
                }

                List<FileAssoc> FileAssocsList = AllFileAssocArray.FileAssocs.ToList();
                List<long> CurrentLevelMasterIds = new List<long>() { masterId };

                while(FileAssocsList.Count > 0)
                {
                    Level++;
                    List<long> NextLevelMasterIds = new List<long>();
                    List<int> FileAssocIndexToRemove = new List<int>();

                    foreach (FileAssoc fAssoc in FileAssocsList.Where(x => CurrentLevelMasterIds.Contains(x.ParFile.MasterId)))
                    {
                        FileAssocIndexToRemove.Add(FileAssocsList.IndexOf(fAssoc));

                        if(Level == 2 && anyCadFileExt != null)
                        {
                            if (anyCadFileExt.Contains(fAssoc.CldFile.Name.ToLower())) isAnyCad = true;
                        }

                        if (!NextLevelMasterIds.Contains(fAssoc.CldFile.MasterId))
                        {
                            NextLevelMasterIds.Add(fAssoc.CldFile.MasterId);
                        }
                    }

                    FileAssocsList.RemoveMultiple(FileAssocIndexToRemove.ToArray());
                    CurrentLevelMasterIds = NextLevelMasterIds;

                    if(FileAssocIndexToRemove.Count == 0 || NextLevelMasterIds.Count == 0)
                    {
                        Level = -666;
                        break;
                    }
                }

                return Level;
            }
            catch(Exception Ex)
            {
                return 0;
            }
        }

        private int GetItemLevel(long id)
        {
            return -1;


            try
            {
                ItemAssoc AllItemAssoc = VaultConnection.WebServiceManager.ItemService.GetItemBOMAssociationsByItemIds(new long[] { id },true).FirstOrDefault();

                int Level = 1;

                return Level;
            }
            catch
            {
                return 0;
            }

        }

        private async Task<StateEnum> RetryCheckInFileAsync(ACW.File file, string comment, ByteArray uploadTicket, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int processId, int RetryCount = 0)
        {
            RetryCount++;

            if (resultState != StateEnum.Error)
            {
                try
                {
                    await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.CheckinUploadedFile(file.MasterId, comment, false,
                                            DateTime.Now, GetFileAssocParamByMasterId(file.MasterId), null, true, file.Name, file.FileClass, file.Hidden, uploadTicket));

                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Checkin" + System.Environment.NewLine);

                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Archivage du fichier après mise à jour."));
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((Ex as VaultServiceErrorException)) +
                                                                    "' à été retourné lors de l'archivage du fichier (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors de l'archivage du fichier (essai " + RetryCount + "/" +
                                                                    appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
                    }
                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await RetryCheckInFileAsync(file, comment, uploadTicket, StateEnum.Processing, resultLogs, appOptions, processId, RetryCount);
            }

            return resultState;
        }

        private async Task<StateEnum> RetryCheckInFileAsync(FileIteration file, string comment, string fullFileName, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int processId, int RetryCount = 1)
        {
            RetryCount++;

            if (resultState != StateEnum.Error)
            {
                try
                {
                    using (System.IO.StreamReader stream = new System.IO.StreamReader(fullFileName))
                    {
                        await Task.Run(() => VaultConnection.FileManager.CheckinFile(file, comment, false,
                                            DateTime.Now, GetFileAssocParamByMasterId(file.EntityMasterId), null, false, file.EntityName, file.FileClassification, file.IsHidden, stream.BaseStream));
                    }

                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Checkin" + System.Environment.NewLine);

                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Archivage du fichier après mise à jour."));
                }
                catch (Exception Ex)
                {
                    string ErrorLogLevel = "Warning";
                    if (RetryCount >= appOptions.MaxRetryCount) ErrorLogLevel = "Error";

                    if (Ex is VaultServiceErrorException)
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "Le code d'erreur Vault '" + GetSubExceptionCodes((Ex as VaultServiceErrorException)) +
                                                                    "' à été retourné lors de l'archivage du fichier (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
                    }
                    else
                    {
                        if ((ErrorLogLevel == "Error" && appOptions.LogError) || (ErrorLogLevel == "Warning" && appOptions.LogWarning))
                            resultLogs.Add(CreateLog(ErrorLogLevel, "L'erreur suivante à été retourné lors de l'archivage du fichier (essai " + RetryCount + "/" +
                                                                    appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
                    }
                    resultState = StateEnum.Error;
                }
            }

            if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
            {
                await Task.Delay(100);
                resultState = await RetryCheckInFileAsync(file, comment, fullFileName, StateEnum.Processing, resultLogs, appOptions, processId, RetryCount);
            }

            return resultState;
        }
        #endregion


        #region OldMethodes
        //internal DataSet ProcessFiles(string FileTaskName, DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        //{
        //    if (FileTaskName.Equals("Update"))
        //    {
        //        return UpdateFiles(data, appOptions, taskProgReport, processProgReport, taskCancellationToken);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}


        //internal DataSet UpdateFiles(DataSet data, ApplicationOptions appOptions, IProgress<TaskProgressReport> taskProgReport, IProgress<ProcessProgressReport> processProgReport, CancellationToken taskCancellationToken)
        //{
        //    taskProgReport.Report(new TaskProgressReport() { Message = "Initialisation" });
        //    DataSet ds = data.Copy();

        //    List<DataRow> Entities = ds.Tables["Entities"].AsEnumerable().Where(x => x.Field<string>("EntityType").Equals("File") &&
        //                                                                       (x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.Validation || x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.TempChangeState || x.Field<TaskTypeEnum>("Task") == TaskTypeEnum.PurgeProps) &&
        //                                                                        x.Field<StateEnum>("State") == StateEnum.Completed &&
        //                                                                        x.Field<long?>("VaultMasterId") != null).ToList(); ;

        //    foreach (DataRow dr in Entities)
        //    {
        //        dr["Task"] = TaskTypeEnum.Update;
        //        dr["State"] = StateEnum.Pending;
        //    }

        //    int TotalCount = Entities.Count;
        //    int currentLevel = 1;

        //    int maxLevel = currentLevel;
        //    if (TotalCount > 0) maxLevel = Entities.Max(x => x.Field<int>("VaultLevel"));

        //    taskProgReport.Report(new TaskProgressReport() { Message = "Mise à jour des fichiers", TotalEntityCount = TotalCount, Timer = "Start" });

        //    List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>> TaskList =
        //                                    new List<Task<(int processId, DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs)>>();

        //    while (currentLevel <= maxLevel)
        //    {
        //        Stack<DataRow> EntitiesStack = new Stack<DataRow>(Entities.Where(x => x.Field<int>("VaultLevel") == currentLevel));

        //        while (EntitiesStack.Count > 0)
        //        {
        //            DataRow PopEntity = EntitiesStack.Pop();
        //            (DataRow entity, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs) result = UpdateFile(PopEntity, processProgReport, appOptions);

        //            result.entity["State"] = result.State;

        //            foreach (KeyValuePair<string, object> kvp in result.Result)
        //            {
        //                result.entity[kvp.Key] = kvp.Value;
        //            }

        //            foreach (Dictionary<string, object> log in result.ResultLogs)
        //            {
        //                DataRow drLog = ds.Tables["Logs"].NewRow();
        //                drLog["EntityId"] = result.entity["Id"];

        //                foreach (KeyValuePair<string, object> kvp in log)
        //                {
        //                    drLog[kvp.Key] = kvp.Value;
        //                }

        //                ds.Tables["Logs"].Rows.Add(drLog);
        //            }
        //        }

        //        currentLevel++;
        //    }

        //    taskProgReport.Report(new TaskProgressReport() { Message = "Mise à jour des fichiers", TotalEntityCount = TotalCount, Timer = "Stop" });

        //    return ds;
        //}

        //private (DataRow dr, Dictionary<string, object> Result, StateEnum State, List<Dictionary<string, object>> ResultLogs) UpdateFile(DataRow dr, IProgress<ProcessProgressReport> processProgReport, ApplicationOptions appOptions)
        //{
        //    Dictionary<string, object> resultValues = new Dictionary<string, object>();
        //    List<Dictionary<string, object>> resultLogs = new List<Dictionary<string, object>>();

        //    StateEnum resultState = StateEnum.Processing;

        //    string FullVaultName = string.Empty;

        //    FullVaultName = dr.Field<string>("Path");
        //    if (string.IsNullOrWhiteSpace(FullVaultName) || FullVaultName.EndsWith("/")) FullVaultName += dr.Field<string>("Name");
        //    else FullVaultName += "/" + dr.Field<string>("Name");

        //    processProgReport.Report(new ProcessProgressReport() { Message = FullVaultName + " (niveau " + dr.Field<int>("VaultLevel").ToString() + ")", ProcessIndex = 0, TotalCountInc = 1 });

        //    if (resultState != StateEnum.Error) resultState = UpdateFileCategory(FullVaultName, dr, resultState, resultLogs, appOptions);
        //    if (resultState != StateEnum.Error) resultState = MoveFile(FullVaultName, dr, resultState, resultLogs, appOptions);
        //    if (resultState != StateEnum.Error) resultState = RenameFile(FullVaultName, dr, resultState, resultLogs, appOptions);
        //    if (resultState != StateEnum.Error) resultState = UpdateFileLifeCycle(FullVaultName, dr, resultState, resultLogs, appOptions);
        //    if (resultState != StateEnum.Error) resultState = UpdateFileRevision(FullVaultName, dr, resultState, resultLogs, appOptions);
        //    if (resultState != StateEnum.Error) resultState = UpdateFileProperty(FullVaultName, dr, resultState, resultLogs, appOptions, 0, processProgReport);
        //    if (resultState != StateEnum.Error) resultState = UpdateFileLifeCycleState(FullVaultName, dr, resultState, resultLogs, appOptions);
        //    if (resultState != StateEnum.Error) resultState = CreateBomBlobJob(FullVaultName, dr, resultValues, resultState, resultLogs, appOptions);

        //    if (resultState == StateEnum.Processing) resultState = StateEnum.Completed;

        //    if (resultState == StateEnum.Error) processProgReport.Report(new ProcessProgressReport() { ProcessIndex = 0, ErrorInc = 1 });
        //    else processProgReport.Report(new ProcessProgressReport() { ProcessIndex = 0, DoneInc = 1 });

        //    return (dr, resultValues, resultState, resultLogs);
        //}

        //private StateEnum UpdateFileCategory(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        //{
        //    if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultCatId") != null && dr.Field<long?>("VaultCatId") != dr.Field<long?>("TargetVaultCatId"))
        //    {
        //        try
        //        {
        //            ACW.File VaultFile = VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileCategories(new long[] { dr.Field<long>("VaultMasterId") }, new long[] { dr.Field<long>("TargetVaultCatId") }, "MaJ - Changement de catégorie").FirstOrDefault();
        //            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "La catégorie '" + dr.Field<string>("TargetVaultCatName") + "' a été appliqué au fichier."));
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors du changement de catégorie du fichier (essai " + RetryCount + "/" +
        //                                                                           appOptions.MaxRetryCount + ")."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors du changement de catégorie du fichier (essai " + RetryCount + "/" +
        //                                                                           appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    RetryCount++;
        //    if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
        //    {
        //        resultState = UpdateFileCategory(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
        //    }

        //    return resultState;
        //}

        //private StateEnum MoveFile(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        //{
        //    if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultFolderId") != null && dr.Field<long?>("VaultFolderId") != dr.Field<long?>("TargetVaultFolderId"))
        //    {
        //        try
        //        {
        //            VaultConnection.WebServiceManager.DocumentService.MoveFile(dr.Field<long>("VaultMasterId"), dr.Field<long>("VaultFolderId"), dr.Field<long>("TargetVaultFolderId"));
        //            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le fichier a été déplacé de '" + dr.Field<string>("Path") + "' vers '" + dr.Field<string>("TargetVaultPath") + "'."));
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors du déplacement du fichier (essai " + RetryCount + "/" +
        //                                                                           appOptions.MaxRetryCount + ")."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors du déplacement du fichier (essai " + RetryCount + "/" +
        //                                                                           appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    RetryCount++;
        //    if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
        //    {
        //        resultState = MoveFile(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
        //    }

        //    return resultState;
        //}

        //private StateEnum RenameFile(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        //{
        //    string Ext = System.IO.Path.GetExtension(dr.Field<string>("Name"));
        //    string NewName = string.Empty;

        //    if (dr.Field<long?>("TargetVaultNumSchId") == null && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultName")))
        //    {
        //        NewName = dr.Field<string>("TargetVaultName");
        //        if (!NewName.EndsWith(Ext, StringComparison.InvariantCultureIgnoreCase)) NewName += Ext;
        //    }
        //    else if (dr.Field<long?>("TargetVaultNumSchId") != null && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultName")) && dr.Field<string>("TargetVaultName").Equals("NextNumber"))
        //    {
        //        try
        //        {
        //            NewName = VaultConnection.WebServiceManager.DocumentService.GenerateFileNumber(dr.Field<long>("TargetVaultNumSchId"), null);
        //            NewName += Ext;
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") +
        //                                                                           "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") +
        //                                                                           "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "." + System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }
        //    else if (dr.Field<long?>("TargetVaultNumSchId") != null && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultName")) && dr.Field<string>("TargetVaultName").StartsWith("NextNumber="))
        //    {
        //        string NumSchInput = dr.Field<string>("TargetVaultName").Substring("NextNumber=".Length).Trim();
        //        try
        //        {
        //            NewName = VaultConnection.WebServiceManager.DocumentService.GenerateFileNumber(dr.Field<long>("TargetVaultNumSchId"), NumSchInput.Split('|'));
        //            NewName += Ext;
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") +
        //                                                                           "' et les paramètres '" + NumSchInput + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'optention du nom de fichier avec le schéma '" + dr.Field<string>("TargetVaultNumSchName") +
        //                                                                           "' et les paramètres '" + NumSchInput + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + "." +
        //                                                                           System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    if (resultState != StateEnum.Error && !string.IsNullOrWhiteSpace(NewName) && NewName != dr.Field<string>("Name"))
        //    {
        //        FileRenameRestric[] fileRenameRestrics = VaultConnection.WebServiceManager.DocumentService.GetFileRenameRestrictionsByMasterId(dr.Field<long>("VaultMasterId"), NewName);

        //        try
        //        {
        //            VDF.Vault.Settings.AcquireFilesSettings AcquireSettings = new VDF.Vault.Settings.AcquireFilesSettings(VaultConnection);

        //            AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeAttachments = false;
        //            AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeChildren = false;
        //            AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeLibraryContents = false;
        //            AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeParents = false;
        //            AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.IncludeRelatedDocumentation = false;
        //            AcquireSettings.OptionsRelationshipGathering.FileRelationshipSettings.VersionGatheringOption = VDF.Vault.Currency.VersionGatheringOption.Actual;

        //            ACW.File File = VaultConnection.WebServiceManager.DocumentService.GetLatestFilesByMasterIds(new long[] { dr.Field<long>("VaultMasterId") }).FirstOrDefault();

        //            AcquireSettings.AddFileToAcquire(new VDF.Vault.Currency.Entities.FileIteration(VaultConnection, File), VDF.Vault.Settings.AcquireFilesSettings.AcquisitionOption.Checkout);

        //            VDF.Vault.Results.AcquireFilesResults AcquireResults = VaultConnection.FileManager.AcquireFiles(AcquireSettings);

        //            if (!AcquireResults.IsCancelled && AcquireResults.FileResults.FirstOrDefault().Status == VDF.Vault.Results.FileAcquisitionResult.AcquisitionStatus.Success)
        //            {
        //                FileIteration AcquiredFile = AcquireResults.FileResults.FirstOrDefault().File;

        //                ACW.File NewFileVer = VaultConnection.WebServiceManager.DocumentService.CheckinUploadedFile(dr.Field<long>("VaultMasterId"), "MaJ - Renommage du fichier", false, DateTime.Now,
        //                                                                GetFileAssocParamByMasterId(dr.Field<long>("VaultMasterId")), null, false, NewName,
        //                                                                AcquiredFile.FileClassification, AcquiredFile.IsHidden, null);

        //                if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le fichier a été renommé de '" + dr.Field<string>("Name") + "' en '" + NewName + "'."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le renommage n'est pas possible car le fichier ne peut être extrait."));
        //                resultState = StateEnum.Error;
        //            }
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors du renommage du fichier '" + dr.Field<string>("Name") + "' en '" + NewName +
        //                                                                           "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors du renommage du fichier '" + dr.Field<string>("Name") + "' en '" + NewName +
        //                                                                           "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
        //            }

        //            try
        //            {
        //                ACW.ByteArray downloadTicket;
        //                VaultConnection.WebServiceManager.DocumentService.UndoCheckoutFile(dr.Field<long>("VaultMasterId"), out downloadTicket);
        //            }
        //            catch (Exception UndoEx)
        //            {
        //                if (Ex is VaultServiceErrorException)
        //                {
        //                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)UndoEx) +
        //                                                                               "' à été retourné lors de l'annulation de l'extraction suite a une erreur de renommage" +
        //                                                                               " (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
        //                }
        //                else
        //                {
        //                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de l'annulation de l'extraction suite a une erreur de renommage" +
        //                                                                               " (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." + System.Environment.NewLine + UndoEx.ToString()));
        //                }
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    RetryCount++;
        //    if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
        //    {
        //        resultState = RenameFile(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
        //    }

        //    return resultState;
        //}

        //private StateEnum UpdateFileProperty(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int processId, IProgress<ProcessProgressReport> processProgReport)
        //{
        //    string NewInventorMaterialName = string.Empty;


        //    if (resultState != StateEnum.Error && appOptions.VaultPropertyFieldMappings.Count > 0)
        //    {
        //        try
        //        {
        //            ACW.File file = VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId"));

        //            string FileProviderName = dr.Field<string>("VaultProvider");

        //            ContentSourceProvider Provider = VaultConnection.ConfigurationManager.GetContentSourceProviders().Where(x => x.DisplayName == FileProviderName).FirstOrDefault();

        //            List<ACW.PropInstParam> UpdateUdps = new List<ACW.PropInstParam>();
        //            List<string> UdpNames = new List<string>();
        //            List<ACW.PropWriteReq> UpdateFileProps = new List<ACW.PropWriteReq>();
        //            List<string> FilePropNames = new List<string>();

        //            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "Start processing file '" + dr.Field<string>("Name") + "'" + System.Environment.NewLine);

        //            CatCfg catCfg = VaultConfig.VaultFileCategoryBehavioursList.Where(x => x.Cat.Id == file.Cat.CatId).FirstOrDefault();
        //            if (catCfg != null)
        //            {
        //                BhvCfg bhvCfg = catCfg.BhvCfgArray.Where(x => x.Name.Equals("UserDefinedProperty")).FirstOrDefault();
        //                if (bhvCfg != null)
        //                {
        //                    foreach (PropertyFieldMapping fMapping in appOptions.VaultPropertyFieldMappings.Where(x => x.VaultPropertySet.Equals("File")))
        //                    {
        //                        PropertyDefinition pDef = VaultConfig.VaultFilePropertyDefinitionDictionary.Values.Where(x => x.DisplayName.Equals(fMapping.VaultPropertyDisplayName)).FirstOrDefault();

        //                        if (bhvCfg.BhvArray.Select(x => x.Id).Contains(pDef.Id))
        //                        {
        //                            string stringVal = dr.GetChildRows("EntityNewProp").FirstOrDefault().Field<string>(fMapping.FieldName);

        //                            if (!string.IsNullOrEmpty(stringVal))
        //                            {
        //                                object objectVal = ToObject(stringVal, pDef.ManagedDataType, file.Name, appOptions.ClearPropValue, appOptions.SyncPartNumberValue);

        //                                UpdateUdps.Add(new ACW.PropInstParam() { PropDefId = pDef.Id, Val = objectVal });
        //                                UdpNames.Add(pDef.DisplayName);

        //                                if (VaultConfig.VaultFilePropertyMapping.ContainsKey(pDef.SystemName) && VaultConfig.VaultFilePropertyMapping[pDef.SystemName].ContainsKey(Provider.SystemName))
        //                                {
        //                                    ContentSourcePropertyMapping cSourceMappings = VaultConfig.VaultFilePropertyMapping[pDef.SystemName][Provider.SystemName].FirstOrDefault();

        //                                    if (cSourceMappings.ContentPropertyDefinition.Moniker.Equals("Material!{32853F0F-3444-11D1-9E93-0060B03C1CA6}!nvarchar"))
        //                                    {
        //                                        NewInventorMaterialName = stringVal;
        //                                        UpdateUdps.Remove(UpdateUdps.LastOrDefault());
        //                                        UdpNames.Remove(UdpNames.LastOrDefault());
        //                                    }
        //                                    else
        //                                    {
        //                                        UpdateFileProps.Add(new ACW.PropWriteReq()
        //                                        {
        //                                            CanCreate = cSourceMappings.ContentPropertyDefinition.SupportCreate,
        //                                            Moniker = cSourceMappings.ContentPropertyDefinition.Moniker,
        //                                            Val = objectVal
        //                                        });
        //                                        FilePropNames.Add(pDef.DisplayName);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            if (VaultConfig.VaultFilePropertyMapping.ContainsKey("Revision") && VaultConfig.VaultFilePropertyMapping["Revision"].ContainsKey(Provider.SystemName))
        //            {
        //                ContentSourcePropertyMapping cSourceMappings = VaultConfig.VaultFilePropertyMapping["Revision"][Provider.SystemName].FirstOrDefault();

        //                UpdateFileProps.Add(new ACW.PropWriteReq()
        //                {
        //                    CanCreate = cSourceMappings.ContentPropertyDefinition.SupportCreate,
        //                    Moniker = cSourceMappings.ContentPropertyDefinition.Moniker,
        //                    Val = file.FileRev.Label
        //                });
        //                FilePropNames.Add(VaultConfig.VaultFilePropertyDefinitionDictionary["Revision"].DisplayName);
        //            }

        //            if (UpdateUdps.Count > 0 || UpdateFileProps.Count > 0)
        //            {
        //                /*
        //                // Checkout
        //                ACW.ByteArray downloadTicket = null;
        //                file = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.CheckoutFile(new FileIteration(VaultConnection, file).EntityIterationId,
        //                                            ACW.CheckoutFileOptions.Master, System.Environment.MachineName, "", "MaJ - Mise à jour des propriétés", out downloadTicket));

        //                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Checkout" + System.Environment.NewLine);

        //                if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Extraction du fichier pour mise à jour."));

        //                // update Vault UDPs
        //                if (UpdateUdps.Count > 0)
        //                {
        //                    await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.UpdateFileProperties(new long[] { dr.Field<long>("VaultMasterId") },
        //                                         new ACW.PropInstParamArray[] { new ACW.PropInstParamArray() { Items = UpdateUdps.ToArray() } }));

        //                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Update UDP" + System.Environment.NewLine);

        //                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Mise à jour des propriétés du fichier dans Vault:" + System.Environment.NewLine +
        //                                                                     string.Join(System.Environment.NewLine, UpdateUdps.Select(x => "   - " + UdpNames[UpdateUdps.IndexOf(x)] + " = " + (x.Val?.ToString() ?? "")))));
        //                }

        //                // Update file properties
        //                ByteArray uploadTicket = null;
        //                if (UpdateFileProps.Count > 0)
        //                {
        //                    ACW.PropWriteResults PropUpdateresults;
        //                    uploadTicket = await Task.Run(() => VaultConnection.WebServiceManager.FilestoreService.CopyFile(downloadTicket.Bytes, System.IO.Path.GetExtension(file.Name).TrimStart('.'),
        //                                                        true, new PropWriteRequests() { Requests = UpdateFileProps.ToArray() }, out PropUpdateresults).ToByteArray());

        //                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Update Properties" + System.Environment.NewLine);

        //                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Mise à jour des propriétés du fichier:" + System.Environment.NewLine +
        //                                                                     string.Join(System.Environment.NewLine, UpdateFileProps.Select(x => "   - " + FilePropNames[UpdateFileProps.IndexOf(x)] + " = " + (x.Val?.ToString() ?? "")))));
        //                }

        //                // Checkin
        //                if (resultState != StateEnum.Error) resultState = RetryCheckInFile(file, "MaJ - Mise à jour des propriétés", uploadTicket, resultState, resultLogs, appOptions, processId);
        //                */

        //                if (resultState != StateEnum.Error && !string.IsNullOrWhiteSpace(NewInventorMaterialName) && System.IO.Path.GetExtension(fullVaultName).Equals(".ipt", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    // resultState = await UpdateInventorMaterialAsync(fullVaultName, NewInventorMaterialName, dr, resultState, resultLogs, appOptions, processId, processProgReport);
        //                }

        //                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "End processing file '" + dr.Field<string>("Name") + "'" + System.Environment.NewLine);
        //            }
        //        }
        //        catch (Exception Ex)
        //        {
        //            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > UpdateFileProperty ERROR" + System.Environment.NewLine);
        //            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", Ex.ToString() + System.Environment.NewLine);

        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors de la mise à jour des propriétés du fichier (essais multiples non implémenté)."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de la mise à jour des propriétés du fichier (essais multiples non implémenté)." +
        //                                                                           System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    return resultState;
        //}

        //private async Task<StateEnum> Old_UpdateFileProperty(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int processId, IProgress<ProcessProgressReport> processProgReport)
        //{
        //    string NewInventorMaterialName = string.Empty;

        //    if (resultState != StateEnum.Error && appOptions.VaultPropertyFieldMappings.Count > 0)
        //    {
        //        try
        //        {
        //            ACW.File file = VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId"));

        //            string FileProviderName = dr.Field<string>("VaultProvider");

        //            ContentSourceProvider Provider = VaultConnection.ConfigurationManager.GetContentSourceProviders().Where(x => x.DisplayName == FileProviderName).FirstOrDefault();

        //            List<ACW.PropInstParam> UpdateUdps = new List<ACW.PropInstParam>();
        //            List<string> UdpNames = new List<string>();
        //            List<ACW.PropWriteReq> UpdateFileProps = new List<ACW.PropWriteReq>();
        //            List<string> FilePropNames = new List<string>();

        //            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "Start processing file '" + dr.Field<string>("Name") + "'" + System.Environment.NewLine);

        //            CatCfg catCfg = VaultConfig.VaultFileCategoryBehavioursList.Where(x => x.Cat.Id == file.Cat.CatId).FirstOrDefault();
        //            if (catCfg != null)
        //            {
        //                BhvCfg bhvCfg = catCfg.BhvCfgArray.Where(x => x.Name.Equals("UserDefinedProperty")).FirstOrDefault();
        //                if (bhvCfg != null)
        //                {
        //                    foreach (PropertyFieldMapping fMapping in appOptions.VaultPropertyFieldMappings.Where(x => x.VaultPropertySet.Equals("File")))
        //                    {
        //                        PropertyDefinition pDef = VaultConfig.VaultFilePropertyDefinitionDictionary.Values.Where(x => x.DisplayName.Equals(fMapping.VaultPropertyDisplayName)).FirstOrDefault();

        //                        if (bhvCfg.BhvArray.Select(x => x.Id).Contains(pDef.Id))
        //                        {
        //                            string stringVal = dr.GetChildRows("EntityNewProp").FirstOrDefault().Field<string>(fMapping.FieldName);

        //                            if (!string.IsNullOrEmpty(stringVal))
        //                            {
        //                                object objectVal = ToObject(stringVal, pDef.ManagedDataType, file.Name, appOptions.ClearPropValue, appOptions.SyncPartNumberValue);

        //                                UpdateUdps.Add(new ACW.PropInstParam() { PropDefId = pDef.Id, Val = objectVal });
        //                                UdpNames.Add(pDef.DisplayName);

        //                                if (VaultConfig.VaultFilePropertyMapping.ContainsKey(pDef.SystemName) && VaultConfig.VaultFilePropertyMapping[pDef.SystemName].ContainsKey(Provider.SystemName))
        //                                {
        //                                    ContentSourcePropertyMapping cSourceMappings = VaultConfig.VaultFilePropertyMapping[pDef.SystemName][Provider.SystemName].FirstOrDefault();

        //                                    if (cSourceMappings.ContentPropertyDefinition.Moniker.Equals("Material!{32853F0F-3444-11D1-9E93-0060B03C1CA6}!nvarchar"))
        //                                    {
        //                                        NewInventorMaterialName = stringVal;
        //                                        UpdateUdps.Remove(UpdateUdps.LastOrDefault());
        //                                        UdpNames.Remove(UdpNames.LastOrDefault());
        //                                    }
        //                                    else
        //                                    {
        //                                        UpdateFileProps.Add(new ACW.PropWriteReq()
        //                                        {
        //                                            CanCreate = cSourceMappings.ContentPropertyDefinition.SupportCreate,
        //                                            Moniker = cSourceMappings.ContentPropertyDefinition.Moniker,
        //                                            Val = objectVal
        //                                        });
        //                                        FilePropNames.Add(pDef.DisplayName);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            if (VaultConfig.VaultFilePropertyMapping.ContainsKey("Revision") && VaultConfig.VaultFilePropertyMapping["Revision"].ContainsKey(Provider.SystemName))
        //            {
        //                ContentSourcePropertyMapping cSourceMappings = VaultConfig.VaultFilePropertyMapping["Revision"][Provider.SystemName].FirstOrDefault();

        //                UpdateFileProps.Add(new ACW.PropWriteReq()
        //                {
        //                    CanCreate = cSourceMappings.ContentPropertyDefinition.SupportCreate,
        //                    Moniker = cSourceMappings.ContentPropertyDefinition.Moniker,
        //                    Val = file.FileRev.Label
        //                });
        //                FilePropNames.Add(VaultConfig.VaultFilePropertyDefinitionDictionary["Revision"].DisplayName);
        //            }

        //            if (UpdateUdps.Count > 0 || UpdateFileProps.Count > 0)
        //            {
        //                // Checkout
        //                ACW.ByteArray downloadTicket = null;
        //                file = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.CheckoutFile(new FileIteration(VaultConnection, file).EntityIterationId,
        //                                            ACW.CheckoutFileOptions.Master, System.Environment.MachineName, "", "MaJ - Mise à jour des propriétés", out downloadTicket));

        //                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Checkout" + System.Environment.NewLine);

        //                if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Extraction du fichier pour mise à jour."));

        //                // update Vault UDPs
        //                if (UpdateUdps.Count > 0)
        //                {
        //                    await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.UpdateFileProperties(new long[] { dr.Field<long>("VaultMasterId") },
        //                                         new ACW.PropInstParamArray[] { new ACW.PropInstParamArray() { Items = UpdateUdps.ToArray() } }));

        //                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Update UDP" + System.Environment.NewLine);

        //                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Mise à jour des propriétés du fichier dans Vault:" + System.Environment.NewLine +
        //                                                                     string.Join(System.Environment.NewLine, UpdateUdps.Select(x => "   - " + UdpNames[UpdateUdps.IndexOf(x)] + " = " + (x.Val?.ToString() ?? "")))));
        //                }

        //                // Update file properties
        //                ByteArray uploadTicket = null;
        //                if (UpdateFileProps.Count > 0)
        //                {
        //                    ACW.PropWriteResults PropUpdateresults;
        //                    uploadTicket = await Task.Run(() => VaultConnection.WebServiceManager.FilestoreService.CopyFile(downloadTicket.Bytes, System.IO.Path.GetExtension(file.Name).TrimStart('.'),
        //                                                        true, new PropWriteRequests() { Requests = UpdateFileProps.ToArray() }, out PropUpdateresults).ToByteArray());

        //                    System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Update Properties" + System.Environment.NewLine);

        //                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Mise à jour des propriétés du fichier:" + System.Environment.NewLine +
        //                                                                     string.Join(System.Environment.NewLine, UpdateFileProps.Select(x => "   - " + FilePropNames[UpdateFileProps.IndexOf(x)] + " = " + (x.Val?.ToString() ?? "")))));
        //                }

        //                // Checkin
        //                file = await Task.Run(() => VaultConnection.WebServiceManager.DocumentService.CheckinUploadedFile(dr.Field<long>("VaultMasterId"), "MaJ - Mise à jour des propriétés", false,
        //                                             DateTime.Now, GetFileAssocParamByMasterId(dr.Field<long>("VaultMasterId")), null, true, file.Name, file.FileClass, file.Hidden, uploadTicket));

        //                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > Checkin" + System.Environment.NewLine);

        //                if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Archivage du fichier après mise à jour."));

        //                if (resultState != StateEnum.Error && !string.IsNullOrWhiteSpace(NewInventorMaterialName) && System.IO.Path.GetExtension(fullVaultName).Equals(".ipt", StringComparison.InvariantCultureIgnoreCase))
        //                {
        //                    resultState = await UpdateInventorMaterial(fullVaultName, NewInventorMaterialName, dr, resultState, resultLogs, appOptions, processId, processProgReport);
        //                }

        //                System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", "End processing file '" + dr.Field<string>("Name") + "'" + System.Environment.NewLine);
        //            }
        //        }
        //        catch (Exception Ex)
        //        {
        //            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", " > UpdateFileProperty ERROR" + System.Environment.NewLine);
        //            System.IO.File.AppendAllText(@"C:\Temp\Process" + processId + ".log", Ex.ToString() + System.Environment.NewLine);

        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors de la mise à jour des propriétés du fichier (essais multiples non implémenté)."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de la mise à jour des propriétés du fichier (essais multiples non implémenté)." +
        //                                                                           System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    return resultState;
        //}

        //private StateEnum UpdateFileLifeCycle(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        //{
        //    if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultLcId") != null)
        //    {
        //        try
        //        {
        //            ACW.File VaultFile = VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId"));

        //            if (VaultFile != null & dr.Field<long?>("TargetVaultLcId") != VaultFile.FileLfCyc.LfCycDefId)
        //            {
        //                LfCycState DefaultLcs = VaultConfig.VaultLifeCycleDefinitionList.Where(x => x.Id == dr.Field<long>("TargetVaultLcId")).FirstOrDefault().StateArray.Where(y => y.IsDflt).FirstOrDefault();

        //                VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileLifeCycleDefinitions(new long[] { dr.Field<long>("VaultMasterId") },
        //                                        new long[] { dr.Field<long>("TargetVaultLcId") }, new long[] { DefaultLcs.Id }, "MaJ - Changement de cycle de vie");

        //                if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le cycle de vie '" + dr.Field<string>("TargetVaultLcName") + "' et l'état '" + DefaultLcs.DispName + "' ont été appliqués au fichier."));
        //            }
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if ((Ex as VaultServiceErrorException).ErrorCode == 3109)
        //                {
        //                    if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le fichier est déjà dans le cycle de vie '" + dr.Field<string>("TargetVaultLcName") + "'."));
        //                    RetryCount = appOptions.MaxRetryCount;
        //                }
        //                else
        //                {
        //                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                      "' à été retourné lors du changement de cycle de vie du fichier vers '" + dr.Field<string>("TargetVaultLcName") +
        //                                                                      "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
        //                }
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors du changement de cycle de vie du fichier vers '" + dr.Field<string>("TargetVaultLcName") +
        //                                                                           "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." +
        //                                                                           System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    RetryCount++;
        //    if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
        //    {
        //        resultState = UpdateFileLifeCycle(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
        //    }

        //    return resultState;
        //}

        //private StateEnum UpdateFileRevision(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        //{
        //    if (resultState != StateEnum.Error && !string.IsNullOrWhiteSpace(dr.Field<string>("TargetVaultRevLabel")))
        //    {
        //        string Label = dr.Field<string>("TargetVaultRevLabel");
        //        if (Label.Equals("NextPrimary") || Label.Equals("NextSecondary") || Label.Equals("NextTertiary"))
        //        {
        //            StringArray revArray = VaultConnection.WebServiceManager.RevisionService.GetNextRevisionNumbersByMasterIds(new long[] { dr.Field<long>("VaultMasterId") },
        //                                                        new long[] { dr.Field<long>("TargetVaultRevSchId") }).FirstOrDefault();

        //            if (Label.Equals("NextPrimary")) Label = revArray.Items[0];
        //            else if (Label.Equals("NextSecondary")) Label = revArray.Items[1];
        //            else if (Label.Equals("NextTertiary")) Label = revArray.Items[2];
        //        }

        //        try
        //        {
        //            ACW.File VaultFile = VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId"));

        //            if (dr.Field<long?>("TargetVaultRevSchId") != null && dr.Field<long>("TargetVaultRevSchId") != VaultFile.FileRev.RevDefId)
        //            {
        //                VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateRevisionDefinitionAndNumbers(new long[] { VaultFile.Id },
        //                                     new long[] { dr.Field<long>("TargetVaultRevSchId") }, new string[] { Label }, "MaJ - Changement de révision");
        //            }
        //            else if (dr.Field<long>("TargetVaultRevSchId") == VaultFile.FileRev.RevDefId && Label != VaultFile.FileRev.Label)
        //            {
        //                VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileRevisionNumbers(new long[] { VaultFile.Id },
        //                                     new string[] { Label }, "MaJ - Changement de révision");
        //            }

        //            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "La révison a été changée pour '" + Label + "'."));
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors de la mise à jour de la révision du fichier (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." +
        //                                                                           System.Environment.NewLine + "TargetVaultRevSchName = " + dr.Field<string>("TargetVaultRevSchName") +
        //                                                                           System.Environment.NewLine + "VaultRevSchName = " + dr.Field<string>("VaultRevSchName") +
        //                                                                           System.Environment.NewLine + "Label = " + Label));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de la mise à jour de la révision du fichier (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." +
        //                                                                           System.Environment.NewLine + "TargetVaultRevSchName = " + dr.Field<string>("TargetVaultRevSchName") +
        //                                                                           System.Environment.NewLine + "VaultRevSchName = " + dr.Field<string>("VaultRevSchName") +
        //                                                                           System.Environment.NewLine + "Label = " + Label +
        //                                                                           System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    RetryCount++;
        //    if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
        //    {
        //        resultState = UpdateFileRevision(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
        //    }

        //    return resultState;
        //}

        //private StateEnum UpdateFileLifeCycleState(string fullVaultName, DataRow dr, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        //{
        //    ACW.File VaultFile = VaultConnection.WebServiceManager.DocumentService.GetLatestFileByMasterId(dr.Field<long>("VaultMasterId"));

        //    if (resultState != StateEnum.Error && dr.Field<long?>("TargetVaultLcsId") != null && dr.Field<long?>("TargetVaultLcsId") != VaultFile.FileLfCyc.LfCycStateId)
        //    {
        //        try
        //        {
        //            VaultConnection.WebServiceManager.DocumentServiceExtensions.UpdateFileLifeCycleStates(new long[] { dr.Field<long>("VaultMasterId") },
        //                                 new long[] { dr.Field<long>("TargetVaultLcsId") }, "MaJ - Changement d'état");

        //            if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "L'état de cycle de vie '" + dr.Field<string>("TargetVaultLcsName") + "' à été appliqué au fichier."));
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                           "' à été retourné lors du changement d'état de cycle de vie du fichier vers '" +
        //                                                                           dr.Field<string>("TargetVaultLcsName") + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")."));
        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors du changement d'état de cycle de vie du fichier vers '" +
        //                                                                           dr.Field<string>("TargetVaultLcsName") + "' (essai " + RetryCount + "/" + appOptions.MaxRetryCount + ")." +
        //                                                                           System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    RetryCount++;
        //    if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
        //    {
        //        resultState = UpdateFileLifeCycleState(fullVaultName, dr, StateEnum.Processing, resultLogs, appOptions, RetryCount);
        //    }

        //    return resultState;
        //}

        //private void SyncFileProperties(string fullVaultName, DataRow dr, ref StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions)
        //{
        //    if (resultState != StateEnum.Error)
        //    {
        //        try
        //        {
        //            resultLogs.Add(CreateLog("System", "La synchro des propriétés de fichier n'est pas encore possible..."));
        //        }
        //        catch (VaultServiceErrorException VltEx)
        //        {
        //            if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes(VltEx) + "' à été retourné lors de la mise à jour des propriétés du fichier '" + fullVaultName + "'."));
        //            resultState = StateEnum.Error;
        //        }
        //    }
        //}

        //private StateEnum CreateBomBlobJob(string fullVaultName, DataRow dr, Dictionary<string, object> resultValues, StateEnum resultState, List<Dictionary<string, object>> resultLogs, ApplicationOptions appOptions, int RetryCount = 1)
        //{
        //    if (resultState != StateEnum.Error)
        //    {
        //        try
        //        {
        //            string Ext = System.IO.Path.GetExtension(dr.Field<string>("Name"));

        //            if (Ext.Equals(".ipt", StringComparison.InvariantCultureIgnoreCase) || Ext.Equals(".iam", StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                int Count = 1;
        //                if (dr.Field<int?>("JobSubmitCount") != null) Count = dr.Field<int>("JobSubmitCount") + 1;

        //                resultValues.Add("JobSubmitCount", Count++);

        //                JobParam param1 = new JobParam();
        //                param1.Name = "EntityClassId";
        //                param1.Val = "File";

        //                JobParam param2 = new JobParam();
        //                param2.Name = "FileMasterId";
        //                param2.Val = dr.Field<long>("VaultMasterId").ToString();

        //                Job job = VaultConnection.WebServiceManager.JobService.AddJob("autodesk.vault.extractbom.inventor", "HE-CreateBomBlob: " + fullVaultName,
        //                                               new JobParam[] { param1, param2 }, 10);

        //                if (appOptions.LogInfo) resultLogs.Add(CreateLog("Info", "Le job de création du BOM Blob a été soumis."));
        //            }
        //        }
        //        catch (Exception Ex)
        //        {
        //            if (Ex is VaultServiceErrorException)
        //            {
        //                if ((Ex as VaultServiceErrorException).ErrorCode == 237)
        //                {
        //                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                               "' à été retourné lors de la soumission du job de création du BOM Blob." +
        //                                                                               "Le job est déjà présent dans la queue du job processeur!"));
        //                    RetryCount = appOptions.MaxRetryCount;
        //                }
        //                else
        //                {
        //                    if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "Le code d'erreur Vault '" + GetSubExceptionCodes((VaultServiceErrorException)Ex) +
        //                                                                               "' à été retourné lors de la soumission du job de création du BOM Blob (essai " + RetryCount + "/" +
        //                                                                               appOptions.MaxRetryCount + ")."));
        //                }

        //            }
        //            else
        //            {
        //                if (appOptions.LogError) resultLogs.Add(CreateLog("Error", "L'erreur suivante à été retourné lors de la soumission du job de création du BOM Blob (essai " + RetryCount + "/" +
        //                                                                           appOptions.MaxRetryCount + ")." + System.Environment.NewLine + Ex.ToString()));
        //            }

        //            resultState = StateEnum.Error;
        //        }
        //    }

        //    RetryCount++;
        //    if (resultState == StateEnum.Error && RetryCount <= appOptions.MaxRetryCount)
        //    {
        //        resultState = CreateBomBlobJob(fullVaultName, dr, resultValues, StateEnum.Processing, resultLogs, appOptions, RetryCount);
        //    }

        //    return resultState;
        //}
        #endregion
    }
}
