using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeTBCAIO.Helpers.Enums;

namespace WholesomeTBCAIO.Helpers
{
    public static class StatusChecker
    {
        public static bool InCombat()
        {
            return BasicConditions()
                && (!ObjectManager.Me.IsMounted || ObjectManager.Me.HaveBuff("GhostWolf"))
                && ObjectManager.Me.Target > 0
                && ObjectManager.Target.IsAttackable
                && ObjectManager.Target.IsAlive
                && ObjectManager.Me.InCombatFlagOnly;
        }
        public static bool InCombatNoTarget()
        {
            return BasicConditions()
                && (!ObjectManager.Me.IsMounted || ObjectManager.Me.HaveBuff("GhostWolf"))
                && (!ObjectManager.Me.HasTarget || ObjectManager.Target.IsDead || !ObjectManager.Target.IsValid || !ObjectManager.Target.IsAttackable)
                && ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool InPull()
        {
            return BasicConditions()
                && Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly;
        }

        public static bool OutOfCombat(RotationRole rotationRole)
        {
            if (BasicConditions()
                && !ObjectManager.Me.IsMounted
                && !ObjectManager.Me.IsCast
                && !Fight.InFight
                && !ObjectManager.Me.InCombatFlagOnly
                && (!ObjectManager.Me.HaveBuff("Drink") || ObjectManager.Me.ManaPercentage >= 95)
                && (!ObjectManager.Me.HaveBuff("Food") || ObjectManager.Me.HealthPercent >= 95)
                /*&& !MovementManager.InMovement*/)
            {
                // Remove Earth Shield if not tank
                if (rotationRole != RotationRole.Tank
                    && rotationRole != RotationRole.None
                    && ObjectManager.Me.HaveBuff("Earth Shield"))
                    WTEffects.TBCCancelPlayerBuff("Earth Shield");

                return true;
            }
            return false;
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
                && Main.IsLaunched
                && !Main.HMPrunningAway;
        }
    }
}
