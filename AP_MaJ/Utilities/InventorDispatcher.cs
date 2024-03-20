using Ch.Hurni.AP_MaJ.Classes;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using DevExpress.Xpf.Core.ReflectionExtensions.Internal;
using Inventor;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ch.Hurni.AP_MaJ.Utilities
{
    public class InventorInstance
    {
        public Inventor.Application InvApp
        {
            get
            {
                if(_invApp == null)
                {
                    Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");

                    _invApp = System.Activator.CreateInstance(inventorAppType) as Inventor.Application;
                    _invApp.Visible = true;
                    _invApp.SilentOperation = true;
                    _invApp.UserInterfaceManager.UserInteractionDisabled = true;

                    while (_invApp != null && !_invApp.Ready)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }

                return _invApp;
            }
            set
            {
                _invApp = value;
            } 
        }
        private Inventor.Application _invApp = null;

        public InventorInstance() { }
    }

    public class InventorDispatcher
    {
        private List<InventorInstance> _invInstances = new List<InventorInstance>();
        private List<string> _mutexIds = new List<string>();

        public InventorDispatcher(int MaxInventorInstance)
        {
           for (int i = 0; i < MaxInventorInstance; i++)
            {
                _invInstances.Add(new InventorInstance());
            }
        }

        public InventorInstance GetInventorInstance()
        {
            while (true)
            {
                InventorInstance result = GetFreeInstance();
                if (result != null)
                    return result;
                else
                    System.Threading.Thread.Sleep(1000);
            }
        }

        private InventorInstance GetFreeInstance()
        {
            foreach (InventorInstance invInst in _invInstances)
            {
                if (Monitor.TryEnter(invInst))
                {
                    return invInst;
                }
            }

            return null;
        }


        public void ReleaseInventorInstance(InventorInstance invInst)
        {
            if (Monitor.IsEntered(invInst)) Monitor.Exit(invInst);
        }

        public void CloseAllInventor()
        {
            foreach(InventorInstance invInst in _invInstances)
            {
                if (Monitor.TryEnter(invInst))
                {
                    invInst.InvApp.Quit();
                    invInst.InvApp = null;
                    Monitor.Exit(invInst);
                }
            }
        }
    }

    //public class InventorDispatcher1
    //{
    //    private List<Inventor.Application> _inventors = new List<Inventor.Application>();

    //    public int MaxInventorAppCount
    //    {
    //        get
    //        {
    //            return _maxInventorAppCount;
    //        }
    //        set
    //        {
    //            _maxInventorAppCount = value;
    //        }
    //    }
    //    private int _maxInventorAppCount = 1;

    //    public InventorDispatcher1()
    //    {

    //    }

    //    private bool Tutu = true;

    //    public Inventor.Application GetInventorInstanceAsync()
    //    {

    //        //_globalMutex.WaitOne();

    //        Inventor.Application result = GetFreeInstance();
    //        if (result != null)
    //        {
    //            //Monitor.Exit(this);
    //            //_globalMutex.ReleaseMutex();
    //            return result;
    //        }

    //        bool res = false;

    //        while(!res) 
    //        {
    //            Monitor.Enter(Tutu, ref res);
    //            if (res)
    //            {
    //                result = CreateInstance();
    //                Monitor.Exit(Tutu);
    //            }
    //            else
    //            {
    //                Thread.Sleep(1000);
    //            }
    //        }
            
    //        if (result != null)
    //        {
    //            //Monitor.Exit(this);
    //            //_globalMutex.ReleaseMutex();
    //            return result;
    //        }

    //        //_globalMutex.ReleaseMutex();
    //        while (true)
    //        {
    //            result = GetFreeInstance();
    //            if (result != null)
    //                return result;
    //            else
    //               System.Threading.Thread.Sleep(1000);
    //        }
    //    }


    //    private Inventor.Application GetFreeInstance()
    //    {
    //        foreach (Inventor.Application invApp in _inventors)
    //        {
    //            if (Monitor.TryEnter(invApp))
    //            {
    //                return invApp;
    //            }
    //        }

    //        return null;
    //    }

    //    private Inventor.Application CreateInstance()
    //    {
    //        if (_inventors.Count >= MaxInventorAppCount) return null;

    //        // Monitor.Enter(this);

    //        Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");

    //        Inventor.Application inv = System.Activator.CreateInstance(inventorAppType) as Inventor.Application;
    //        inv.Visible = true;
    //        inv.SilentOperation = true;
    //        inv.WindowState = WindowsSizeEnum.kNormalWindow;
    //        inv.UserInterfaceManager.UserInteractionDisabled = true;

    //        while (inv != null && !inv.Ready)
    //        {
    //            System.Threading.Thread.Sleep(1000);
    //        }

    //        Monitor.TryEnter(inv);

    //        _inventors.Add(inv);

    //        // Monitor.Exit(this);

    //        return inv;
    //    }

    //    private async Task<Inventor.Application> CreateInstanceAsync()
    //    {
    //        if (_inventors.Count >= MaxInventorAppCount) return null;


    //        Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");

    //        Inventor.Application inv = System.Activator.CreateInstance(inventorAppType) as Inventor.Application;
    //        inv.Visible = true;
    //        inv.SilentOperation = true;
    //        inv.WindowState = WindowsSizeEnum.kNormalWindow;
    //        inv.UserInterfaceManager.UserInteractionDisabled = true;

    //        while (inv != null && !inv.Ready)
    //        {
    //            await Task.Delay(1000);
    //        }

    //        Monitor.TryEnter(inv);

    //        _inventors.Add(inv);
           
    //        return inv;
    //    }

    //    public void ReleaseInventorInstance(Inventor.Application invApp)
    //    {
    //        if(Monitor.IsEntered(invApp)) Monitor.Exit(invApp);
    //    }

    //    public async Task CloseAllInventorAsync()
    //    {
    //        do
    //        {
    //            for (int i = _inventors.Count - 1 ; i >= 0; i--)
    //            {
    //                if (Monitor.TryEnter(_inventors[i]))
    //                {
    //                    _inventors[i].Quit();
    //                    _inventors.RemoveAt(i);
    //                }
    //            }
            
    //            if (_inventors.Count > 0) await Task.Delay(1000);

    //        } while (_inventors.Count != 0);
    //    }
    //}

    //public class InventorDispatcher2
    //{
    //    private Dictionary<string, Inventor.Application> _inventors = new Dictionary<string, Inventor.Application>();

    //    public int MaxInventorAppCount
    //    {
    //        get
    //        {
    //            return _maxInventorAppCount;
    //        }
    //        set
    //        {
    //            _maxInventorAppCount = value;
    //        }
    //    }
    //    private int _maxInventorAppCount = 1;

    //    private Mutex _globalMutex = new Mutex();

    //    public InventorDispatcher2()
    //    {

    //    }

    //    private bool Tutu = true;


    //    public async Task<(string mutexId, Inventor.Application invApp)> GetInventorInstanceAsync()
    //    {
    //        _globalMutex.WaitOne();

    //        (string mutexId, Inventor.Application invApp) result = (null, null);

    //        result = GetFreeInstance();

    //        if (result != (null, null))
    //        {
    //            _globalMutex.ReleaseMutex();
    //            return result;
    //        }
    //        else if (_inventors.Count < MaxInventorAppCount)
    //        {
    //            result = await CreateInstanceAsync();
    //            return result;
    //        }
    //        else
    //        {
    //            _globalMutex.ReleaseMutex();
    //            while (true)
    //            {
    //                result = GetFreeInstance();
    //                if (result != (null, null))
    //                    return result;
    //                else
    //                    await Task.Delay(1000);
    //            }
    //        }

    //        ///if (result != (null, null))
    //        ///{
    //        ///    return result;
    //        ///}
    //        ///else if (_inventors.Count < _parent.MaxInventorAppCount)
    //        ///{
    //        ///    mutexId = Guid.NewGuid().ToString();
    //        ///    _inventors.Add(mutexId, null);
    //        ///}
    //        ///globalMutex.ReleaseMutex();
    //        ///if (!string.IsNullOrEmpty(mutexId))
    //        ///{
    //        ///    return await CreateInstanceAsync(mutexId);
    //        ///}
    //        ///else
    //        ///{
    //        ///    while (true)
    //        ///    {
    //        ///        result = GetFreeInstance();
    //        ///        if (result != (null, null))
    //        ///            return result;
    //        ///        else
    //        ///            await Task.Delay(1000);
    //        ///    }
    //        ///}
    //        /// }
    //       /// return (null, null);
    //        ///(string mutexId, Inventor.Application invApp) result = GetFreeInstance();
    //        ///if (result != (null, null))
    //        ///{
    //        ///    return result;
    //        ///}
    //        ///else if (_inventors.Count < _parent.MaxInventorAppCount)
    //        ///{
    //        ///    return await CreateInstanceAsync();
    //        ///}
    //        ///else
    //        ///{
    //        ///    while (true)
    //        ///    {
    //        ///        result = GetFreeInstance();
    //        ///        if (result != (null, null))
    //        ///            return result;
    //        ///        else
    //        ///            await Task.Delay(1000);
    //        ///    }
    //        ///}
    //    }


    //    private (string mutexId, Inventor.Application invApp) GetFreeInstance()
    //    {
    //        foreach (string mutexId in _inventors.Keys)
    //        {
    //            Mutex mutex = new Mutex(false, mutexId);

    //            if (mutex.WaitOne())
    //            {
    //                return (mutexId, _inventors[mutexId]);
    //            }
    //        }

    //        return (null, null);
    //    }

    //    private async Task<(string mutexId, Inventor.Application invApp)> CreateInstanceAsync()
    //    {
    //        string mutexId = Guid.NewGuid().ToString();
    //        Mutex mutex = new Mutex(true, mutexId);

    //        _inventors.Add(mutexId, null);

    //        _globalMutex.ReleaseMutex();

    //        Type inventorAppType = System.Type.GetTypeFromProgID("Inventor.Application");

    //        Inventor.Application inv = System.Activator.CreateInstance(inventorAppType) as Inventor.Application;
    //        inv.Visible = true;
    //        inv.SilentOperation = true;
    //        inv.UserInterfaceManager.UserInteractionDisabled = true;

    //        while (inv != null && !inv.Ready)
    //        {
    //            await Task.Delay(1000);
    //        }

    //        _inventors[mutexId] = inv;

    //        return (mutexId, _inventors[mutexId]);
    //    }


    //    public void ReleaseInventorInstance(string mutexId)
    //    {
    //        Mutex mutex = new Mutex(false, mutexId);
    //        mutex.ReleaseMutex();
    //    }

    //    public async Task CloseAllInventorAsync()
    //    {
    //        do
    //        {
    //            foreach (string mutexId in _inventors.Keys)
    //            {
    //                Mutex mutex = new Mutex(false, mutexId);

    //                if (mutex.WaitOne(0))
    //                {
    //                    _inventors[mutexId].Quit();
    //                    _inventors.Remove(mutexId);
    //                }
    //            }

    //            if (_inventors.Count > 0) await Task.Delay(1000);

    //        } while (_inventors.Count != 0);
    //    }
    //}
}
