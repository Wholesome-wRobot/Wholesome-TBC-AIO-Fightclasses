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
                && ObjectManager.Me.Target > 0UL
                && ObjectManager.Target.IsAttackable
                && ObjectManager.Target.IsAlive
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
                && !Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly;
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
