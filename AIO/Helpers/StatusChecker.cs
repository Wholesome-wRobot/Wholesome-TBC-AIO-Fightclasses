using robotManager.Products;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public static class StatusChecker
    {
        public static bool InCombat()
        {
            return BasicConditions()
                && (!ObjectManager.Me.IsMounted || ObjectManager.Me.HaveBuff("Ghost Wolf"))
                && ObjectManager.Me.Target > 0UL
                && ObjectManager.Target.IsAttackable
                && ObjectManager.Target.IsAlive
                && ObjectManager.Me.InCombatFlagOnly;
        }
        public static bool InCombatNoTarget()
        {
            return BasicConditions()
                && (!ObjectManager.Me.IsMounted || ObjectManager.Me.HaveBuff("Ghost Wolf"))
                && !ObjectManager.Me.HasTarget
                && ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool InPull()
        {
            return BasicConditions()
                && Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool OutOfCombat()
        {
            return BasicConditions()
                && !ObjectManager.Me.IsMounted
                && !ObjectManager.Me.IsCast
                && !Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly
                && (!ObjectManager.Me.HaveBuff("Drink") || ObjectManager.Me.ManaPercentage >= 95)
                && (!ObjectManager.Me.HaveBuff("Food") || ObjectManager.Me.HealthPercent >= 95)
                && !MovementManager.InMovement;
        }

        public static bool OOCMounted()
        {
            return BasicConditions()
                && ObjectManager.Me.IsMounted
                && !Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool BasicConditions()
        {
            return Conditions.InGameAndConnectedAndProductStartedNotInPause
                && ObjectManager.Me.IsAlive
                && Main.isLaunched
                && !Main.HMPrunningAway;
        }
    }
}
