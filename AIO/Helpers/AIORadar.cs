using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public class AIORadar
    {
        private static object _radarLock = new object();
        public static bool _isRunning;
        private static List<WoWUnit> _allUnits = new List<WoWUnit>();
        private static List<WoWUnit> _closeUnits = new List<WoWUnit>();

        public static void Pulse(object sender, DoWorkEventArgs args)
        {
            _isRunning = true;
            while (Main.isLaunched && _isRunning)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        lock (_radarLock)
                        {
                            _allUnits = ObjectManager.GetObjectWoWUnit();
                            _closeUnits = _allUnits.FindAll(e => e.GetDistance < 50);
                        }
                    }
                }
                catch (Exception arg)
                {
                    Logger.LogError("AIORadar -> " + string.Concat(arg));
                }
                Thread.Sleep(3000);
            }
            _isRunning = false;
        }

        public static List<WoWUnit> UnitsTargetingMe
        {
            get
            {
                lock (_radarLock)
                {
                    return _closeUnits.FindAll(u => u.IsTargetingMe);
                }
            }
        }

        public static List<WoWUnit> AllUnits
        {
            get
            {
                lock (_radarLock)
                {
                    return _allUnits;
                }
            }
        }

        public static List<WoWUnit> CloseUnitsTargetingMe
        {
            get
            {
                lock (_radarLock)
                {
                    return _closeUnits.FindAll(e => e.IsTargetingMe);
                }
            }
        }
    }
}
