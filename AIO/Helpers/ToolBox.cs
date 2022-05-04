using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeToolbox;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public class ToolBox
    {
        #region Combat
        // Pull
        public static bool Pull(Cast cast, bool alwaysPull, List<AIOSpell> spells, IUnitCache unitCache)
        {
            AIOSpell pullSpell = spells.Find(s => s != null && s.IsSpellUsable && s.KnownSpell);
            if (pullSpell == null)
            {
                RangeManager.SetRangeToMelee();
                return false;
            }

            IWoWUnit closestHostileFromTarget = unitCache.GetClosestHostileFrom(unitCache.Target, 20);
            if (closestHostileFromTarget == null && !alwaysPull)
            {
                RangeManager.SetRangeToMelee();
                return false;
            }

            float pullRange = pullSpell.MaxRange;

            if (ObjectManager.Target.GetDistance > pullRange - 2
                || ObjectManager.Target.GetDistance < 6
                || TraceLine.TraceLineGo(ObjectManager.Target.Position))
            {
                RangeManager.SetRangeToMelee();
                return false;
            }

            if (closestHostileFromTarget != null && RangeManager.GetRange() < pullRange)
                Logger.Log($"Pulling from distance (hostile unit {closestHostileFromTarget.Name} is too close)");

            if (ObjectManager.Me.IsMounted)
                MountTask.DismountMount();

            RangeManager.SetRange(pullRange - 1);
            Thread.Sleep(300);

            if (cast.OnTarget(pullSpell))
            {
                Thread.Sleep(500);
                if (pullSpell.GetCurrentCooldown > 0)
                {
                    Usefuls.WaitIsCasting();
                    if (pullSpell.Name == "Shoot" || pullSpell.Name == "Throw" || pullSpell.Name == "Avenger's Shield")
                        Thread.Sleep(1500);
                    return true;
                }
            }

            return false;
        }

        // Reactivates auto attack if it's off. Must pass the Attack spell as argument
        public static void CheckAutoAttack(AIOSpell attack)
        {
            if (!WTCombat.IsSpellActive("Attack") && ObjectManager.Target.IsAlive)
            {
                Logger.LogDebug("Re-activating attack");
                attack.Launch();
            }
        }

        // Waits for GlobalCooldown to be off, must pass the most basic spell avalailable at lvl1 (ex: Smite for priest)
        public static void WaitGlobalCoolDown(Spell s)
        {
            int c = 0;
            while (!s.IsSpellUsable)
            {
                c += 50;
                Thread.Sleep(50);
                if (c >= 2000)
                    return;
            }
            Logger.LogDebug("Waited for GCD : " + c);
        }

        #endregion

        #region Misc

        public static void ClearCursor()
        {
            Lua.LuaDoString("ClearCursor();");
        }

        // Returns whether units, hostile or not, are close to the player. Distance must be passed as argument
        public static int GetNbEnemiesClose(float distance)
        {
            List<WoWUnit> surroundingEnemies = ObjectManager.GetObjectWoWUnit();
            int result = 0;
            foreach (WoWUnit unit in surroundingEnemies)
            {
                if (unit.IsAlive
                    && !unit.IsTapDenied
                    && unit.IsValid
                    && !unit.IsTaggedByOther
                    && !unit.PlayerControlled
                    && unit.IsAttackable
                    && unit.GetDistance < distance)
                    result++;
            }
            return result;
        }

        // Returns the latency
        public static int GetLatency()
        {
            int worldLatency = Lua.LuaDoString<int>($"local down, up, lagHome, lagWorld = GetNetStats(); return lagWorld");
            int homeLatency = Lua.LuaDoString<int>($"local down, up, lagHome, lagWorld = GetNetStats(); return lagHome");
            return worldLatency + homeLatency;
        }

        #endregion

        #region Items

        // Get item ID in bag from a list passed as argument (good to check CD)
        public static int GetItemEntry(List<string> list)
        {
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
                if (list.Contains(item.Name))
                    return item.Entry;
            return 0;
        }

        // Get item Cooldown from list (must pass item list as arg)
        public static int GetItemCooldown(List<string> itemList)
        {
            int entry = GetItemEntry(itemList);
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
                if (entry == item.Entry)
                    return Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + entry + "); " +
                        "return duration - (GetTime() - startTime)");

            Logger.Log("Couldn't find item");
            return 0;
        }

        public static void UseConsumableToSelfHeal()
        {
            List<string> consumables = new List<string> {
                "Master Healthstone",
                "Major Healthstone",
                "Greater Healthstone",
                "Healthstone",
                "Lesser Healthstone"
            };
            UseFirstMatchingItem(consumables);
        }

        // Uses the first item found in your bags that matches any element from the list
        public static void UseFirstMatchingItem(List<string> list)
        {
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
            {
                if (list.Contains(item.Name))
                {
                    ItemsManager.UseItemByNameOrId(item.Name);
                    Logger.Log("Using " + item.Name);
                    return;
                }
            }
        }

        // Returns the item found in your bags that matches the latest element from the list
        public static string GetBestMatchingItem(List<string> list)
        {
            string _bestItem = null;
            int index = 0;

            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
            {
                if (list.Contains(item.Name))
                {
                    int itemIndex = list.IndexOf(item.Name);
                    if (itemIndex >= index)
                        _bestItem = item.Name;
                }
            }
            return _bestItem;
        }

        #endregion

        #region Movement

        // Determines if me is behind the Target
        public static bool MeBehindTarget()
        {
            float Pi = (float)System.Math.PI;
            bool backLeft = false;
            bool backRight = false;
            float target_x = ObjectManager.Target.Position.X;
            float target_y = ObjectManager.Target.Position.Y;
            float target_r = ObjectManager.Target.Rotation;
            float player_x = ObjectManager.Me.Position.X;
            float player_y = ObjectManager.Me.Position.Y;
            float d = (float)System.Math.Atan2((target_y - player_y), (target_x - player_x));
            float r = d - target_r;

            if (r < 0) r = r + (Pi * 2);
            if (r > 1.5 * Pi) backLeft = true;
            if (r < 0.5 * Pi) backRight = true;
            if (backLeft || backRight) return true; else return false;
        }

        // Move behind Target
        public static bool StandBehindTargetCombat(IUnitCache unitCache)
        {
            if (unitCache.Me.IsAlive
                && !unitCache.Me.IsCast
                && unitCache.Target.IsAlive
                && unitCache.Target.HasTarget
                && !unitCache.Target.IsTargetingMe
                && !MovementManager.InMovement)
            {
                int limit = 5;
                Vector3 position = WTSpace.BackOfUnit(unitCache.Target.WowUnit, 2f);
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && unitCache.Me.PositionWithoutType.DistanceTo(position) > 1
                    && limit >= 0)
                {
                    position = WTSpace.BackOfUnit(unitCache.Target.WowUnit, 2f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    // Wait follow path
                    Thread.Sleep(500);
                    limit--;
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}