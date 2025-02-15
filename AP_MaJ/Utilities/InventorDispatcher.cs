﻿using Ch.Hurni.AP_MaJ.Classes;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using DevExpress.Xpf.Core.ReflectionExtensions.Internal;
using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ch.Hurni.AP_MaJ.Utilities
{
    public class InventorInstance
    {
        [DllImport("user32")]
        private static extern int GetWindowThreadProcessId(int hwnd, ref int lpdwProcessId);

        public Inventor.Application InvApp
        {
            get
            {
                return _invApp;
            }
            set
            {
                _invApp = value;
            } 
        }
        private Inventor.Application _invApp = null;

        public int? UsedByProcessId
        {
            get
            {
                return _usedByProcessId;
            }
            set
            {
                _usedByProcessId = value;
            }
        }
        private int? _usedByProcessId = null;

        public int InventorFileCount
        {
            get
            {
                return _inventorFileCount;
            }
            set
            {
                _inventorFileCount = value;
            }
        }
        private int _inventorFileCount = 0;

        public int MaxInventorFileCount
        {
            get
            {
                return _maxInventorFileCount;
            }
            set
            {
                _maxInventorFileCount = value;
            }
        }
        private int _maxInventorFileCount = 100;


        /// <summary>
        /// Value in Mb
        /// </summary>
        public int MaxInventorMemory
        {
            get
            {
                return _maxInventorMemory;
            }
            set
            {
                _maxInventorMemory = value;
            }
        }
        private int _maxInventorMemory = 5000;


        public bool IsInventorVisible
        {
            get
            {
                return _isInventorVisible;
            }
            set
            {
                _isInventorVisible = value;
            }
        }
        private bool _isInventorVisible = false;

        public bool IsInventorSilent
        {
            get
            {
                return _isInventorSilent;
            }
            set
            {
                _isInventorSilent = value;
            }
        }
        private bool _isInventorSilent = true;

        public bool IsInventorInteractionEnable
        {
            get
            {
                return _isInventorInteractionEnable;
            }
            set
            {
                _isInventorInteractionEnable = value;
            }
        }
        private bool _isInventorInteractionEnable = false;

        private int _inventorProcessId = -1;

        public InventorInstance() { }

        internal async Task StartOrRestartInventorAsync()
        {
            int memUsed = 0;
            if(_inventorProcessId != -1 && Process.GetProcesses().Where(x => x.Id == _inventorProcessId).Count() > 0)
            {
                Process thisproc = Process.GetProcessById(_inventorProcessId);
                memUsed = (int)(thisproc.WorkingSet64 / (1024 * 1024));
            }

            if (_invApp != null && (InventorFileCount >= MaxInventorFileCount || memUsed > 4000))
            {
                await Task.Run(() => ForceCloseInventor());
            }

            if (_invApp == null)
            {
                Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");

                _invApp = await Task.Run(() => System.Activator.CreateInstance(inventorAppType)) as Inventor.Application;
                
                GetWindowThreadProcessId(_invApp.MainFrameHWND, ref _inventorProcessId);

                _invApp.Visible = IsInventorVisible; //false;
                _invApp.SilentOperation = IsInventorSilent; //true;
                _invApp.UserInterfaceManager.UserInteractionDisabled = !IsInventorInteractionEnable; //true;

                while (_invApp != null && !_invApp.Ready)
                {
                    await Task.Delay(1000);
                }
            }
        }

        internal void ForceCloseInventor()
        {
            try
            {
                _invApp.Documents.CloseAll();
                _invApp.Quit();
            }
            catch (Exception Ex)
            {
                System.IO.File.AppendAllText(@"C:\Temp\DispatcherLog.txt", "Inventor is not running anymore..." + System.Environment.NewLine);
            }

            if (_inventorProcessId != -1 && Process.GetProcesses().Where(x => x.Id == _inventorProcessId).Count() > 0)
            {
                System.IO.File.AppendAllText(@"C:\Temp\DispatcherLog.txt", "Kill Inventor process Id '" + _inventorProcessId + "'..." + System.Environment.NewLine);
                try
                {
                    Process thisproc = Process.GetProcessById(_inventorProcessId);

                    if (!thisproc.CloseMainWindow()) thisproc.Kill();
                }
                catch(Exception Ex)
                {
                    System.IO.File.AppendAllText(@"C:\Temp\DispatcherLog.txt", "Inventor process Id '" + _inventorProcessId + "' not found..." + System.Environment.NewLine);
                }
            }

            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            _invApp = null;
            _inventorFileCount = 0;
        }
    }

    public class InventorDispatcher
    {
        private List<InventorInstance> _invInstances = new List<InventorInstance>();
        private List<string> _mutexIds = new List<string>();

        public int? MaxWaitForInventorInstanceInSeconds
        {
            get
            {
                return _maxWaitForInventorInstanceInSeconds;
            }
            set
            {
                _maxWaitForInventorInstanceInSeconds = value;
            }
        }
        private int? _maxWaitForInventorInstanceInSeconds = null;

        public InventorDispatcher(ApplicationOptions appOptions)
        {
            for (int i = 0; i < appOptions.MaxInventorAppCount; i++)
            {
                _invInstances.Add(new InventorInstance() 
                { 
                    MaxInventorFileCount = appOptions.MaxInventorFileCount,
                    MaxInventorMemory = appOptions.MaxInventorMemory,
                    IsInventorSilent = appOptions.IsInventorSilent,
                    IsInventorVisible = appOptions.IsInventorVisible,
                    IsInventorInteractionEnable = appOptions.IsInventorInteractionEnable
                });
            }

            MaxWaitForInventorInstanceInSeconds = appOptions.MaxWaitForInventorInstanceInSeconds;
        }

        public InventorInstance GetInventorInstance(int ProcessId)
        {
            InventorInstance result = null;
            int retryCount = 0;

            while (true)
            {
                Monitor.Enter(_invInstances);
                try
                {
                    result = _invInstances.Where(x => x.UsedByProcessId == null).FirstOrDefault();
                    if (result != null)
                    {
                        result.UsedByProcessId = ProcessId;
                        System.IO.File.AppendAllText(@"C:\Temp\DispatcherLog.txt", "Inventor instance acquired by process '" + ProcessId + "'" + System.Environment.NewLine);
                    }
                    else
                    {
                        System.IO.File.AppendAllText(@"C:\Temp\DispatcherLog.txt", "Process '" + ProcessId + "' waiting for inventor instance" + System.Environment.NewLine);
                    }
                }
                finally
                {
                    Monitor.Exit(_invInstances);
                }

                if (result != null)
                {
                    return result;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    retryCount++;

                    if (MaxWaitForInventorInstanceInSeconds != null && retryCount > MaxWaitForInventorInstanceInSeconds)
                    {
                        Monitor.Enter(_invInstances);
                        try
                        {
                            System.IO.File.AppendAllText(@"C:\Temp\DispatcherLog.txt", "Le process '" + ProcessId + "' n'a pas obtenu d'instance Inventor après 5 minutes d'attente..." + System.Environment.NewLine);
                        }
                        finally 
                        { 
                            Monitor.Exit(_invInstances); 
                        }

                        return result;
                    }
                }
            }
        }

        public void ReleaseInventorInstance(int ProcessId)
        {
            Monitor.Enter(_invInstances);
            try
            {
                InventorInstance result = _invInstances.Where(x => x.UsedByProcessId == ProcessId).FirstOrDefault();
                if (result != null)
                {
                    System.IO.File.AppendAllText(@"C:\Temp\DispatcherLog.txt", "Inventor instance released by process '" + ProcessId + "'" + System.Environment.NewLine);
                    result.UsedByProcessId = null;
                }
            }
            finally
            {
                Monitor.Exit(_invInstances);
            }
        }

        public void CloseAllInventor()
        {
            Monitor.Enter(_invInstances);
            try
            {
                foreach (InventorInstance invInstance in _invInstances.Where(x => x.UsedByProcessId == null && x.InvApp != null))
                {
                    invInstance.ForceCloseInventor();
                }
            }
            finally
            {
                Monitor.Exit(_invInstances);
            }
        }

        //public InventorInstance GetInventorInstance()
        //{
        //    int retryCount = 0;

        //    while (true)
        //    {
        //        InventorInstance result = GetFreeInstance();
        //        if (result != null)
        //        {
        //            return result;
        //        }
        //        else
        //        {
        //            System.Threading.Thread.Sleep(1000);
        //            retryCount++;

        //            if (retryCount >= 20) return null;
        //        }
        //    }
        //}

        //private InventorInstance GetFreeInstance()
        //{
        //    foreach (InventorInstance invInst in _invInstances)
        //    {
        //        if (Monitor.TryEnter(invInst))
        //        {
        //            return invInst;
        //        }
        //    }

        //    return null;
        //}


        //public void ReleaseInventorInstance(InventorInstance invInst)
        //{
        //    if (invInst != null && Monitor.IsEntered(invInst))
        //    {
        //        Monitor.Exit(invInst);
        //    }
        //}

        //public void CloseAllInventor(IProgress<TaskProgressReport> taskProgReport = null)
        //{
        //    int TotalInventorCount = _invInstances.Where(x => x.InvApp != null).Count();
        //    int ClosedInventorCount = 0;

        //    do
        //    {
        //        foreach (InventorInstance invInst in _invInstances.Where(x => x.InvApp != null))
        //        {
        //            if (Monitor.TryEnter(invInst))
        //            {
        //                ClosedInventorCount++;
                        
        //                if (taskProgReport != null)
        //                {
        //                    taskProgReport.Report(new TaskProgressReport() { Message = "Fermeture des instances Inventor " + ClosedInventorCount + " sur " + TotalInventorCount + "..." });
        //                }

        //                invInst.InvApp.Quit();
        //                invInst.InvApp = null;
        //                Monitor.Exit(invInst);
        //            }
        //        }

        //        if (_invInstances.Where(x => x.InvApp != null).Count() > 0) System.Threading.Thread.Sleep(1000);

        //    } while (_invInstances.Where(x => x.InvApp != null).Count() > 0);
        //}
    }
}
