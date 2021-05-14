using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public class AIOParty
    {
        public static bool _isRunning;
        public static List<WoWPlayer> Group { get; private set; } = new List<WoWPlayer>();

        public static void DoPartyUpdatePulse(object sender, DoWorkEventArgs args)
        {
            _isRunning = true;
            while (Main.isLaunched && _isRunning)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        List<WoWPlayer> group = Party.GetParty();
                        group.Add(ObjectManager.Me);

                        if (group.Count != Group.Count && group.Count > 1)
                        {
                            string logMessage = "Party detected [";
                            group.ForEach(m => logMessage += m.Name + "-");
                            logMessage = logMessage.Remove(logMessage.Length - 1);
                            logMessage += "]";
                            Logger.Log(logMessage);
                        }

                        Group = group;
                    }
                }
                catch (Exception arg)
                {
                    Logger.LogError("AIOParty -> " + string.Concat(arg));
                }
                Thread.Sleep(3000);
            }
            _isRunning = false;
        }
    }
}
