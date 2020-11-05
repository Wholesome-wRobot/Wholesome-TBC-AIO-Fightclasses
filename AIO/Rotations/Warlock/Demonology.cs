using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class Demonology : Warlock
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();
        }

        protected override void Pull()
        {
            // Pet attack
            if (ObjectManager.Pet.Target != ObjectManager.Me.Target)
                Lua.LuaDoString("PetAttack();", false);

            // Life Tap
            if (Me.HealthPercent > Me.ManaPercentage
                && !Me.IsMounted
                && settings.UseLifeTap)
                if (cast.Normal(LifeTap))
                    return;

            base.Pull();
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
        }
    }
}
