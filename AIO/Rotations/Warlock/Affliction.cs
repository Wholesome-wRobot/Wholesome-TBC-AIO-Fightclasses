using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class Affliction : Warlock
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

            // Amplify Curse
            if (AmplifyCurse.IsSpellUsable
                && AmplifyCurse.KnownSpell)
                AmplifyCurse.Launch();

            // Siphon Life
            if (Me.HealthPercent < 90
                && settings.UseSiphonLife
                && !ObjectManager.Target.HaveBuff("Siphon Life")
                && ObjectManager.Target.GetDistance < _maxRange + 2)
                if (cast.Normal(SiphonLife))
                    return;

            // Unstable Affliction
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !ObjectManager.Target.HaveBuff("Unstable Affliction"))
                if (cast.Normal(UnstableAffliction))
                    return;

            base.Pull();
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
        }
    }
}
