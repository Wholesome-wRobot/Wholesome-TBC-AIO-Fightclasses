using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache
{
    public class UnitCache : IUnitCache
    {
        private object _unitCacheLock = new object();
        private bool _isRunning;
        private List<WoWUnit> _allUnits = new List<WoWUnit>();
        private List<WoWUnit> _closeUnits = new List<WoWUnit>();
        private readonly BackgroundWorker _unitCacheThread = new BackgroundWorker();

        public List<WoWUnit> UnitsTargetingMe
        {
            get
            {
                lock (_unitCacheLock)
                {
                    return _closeUnits.FindAll(u => u.IsTargetingMe);
                }
            }
        }

        public List<WoWUnit> AllUnits
        {
            get
            {
                lock (_unitCacheLock)
                {
                    return _allUnits;
                }
            }
        }

        public List<WoWUnit> CloseUnitsTargetingMe
        {
            get
            {
                lock (_unitCacheLock)
                {
                    return _closeUnits.FindAll(e => e.IsTargetingMe);
                }
            }
        }

        public UnitCache()
        {

        }

        public void Initialize()
        {
            _isRunning = true;
            _unitCacheThread.DoWork += Pulse;
            _unitCacheThread.RunWorkerAsync();
        }

        public void Dispose()
        {
            _unitCacheThread.DoWork -= Pulse;
            _unitCacheThread.Dispose();
            _isRunning = false;
        }

        private void Pulse(object sender, DoWorkEventArgs args)
        {
            _isRunning = true;
            while (Main.IsLaunched && _isRunning)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        lock (_unitCacheLock)
                        {
                            _allUnits = ObjectManager.GetObjectWoWUnit();
                            _closeUnits = _allUnits.FindAll(e => e.GetDistance < 50);
                        }
                    }
                }
                catch (Exception arg)
                {
                    Logger.LogError("UnitCache -> " + string.Concat(arg));
                }
                Thread.Sleep(3000);
            }
            _isRunning = false;
        }
    }
}
