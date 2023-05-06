using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class FuryParty : Warrior
    {
        public FuryParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();
        }

        protected override void Pull()
        {
            base.Pull();

            RangeManager.SetRangeToMelee();

            // Check if caster in list
            if (casterEnemies.Contains(Target.Name))
                fightingACaster = true;

            // Charge Berserker Stance
            if (InBerserkStance()
                && Target.GetDistance > 9f
                && Target.GetDistance < 24f
                && Target.HealthPercent < 90
                && cast.OnTarget(Intercept))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            bool shouldBeInterrupted = WTCombat.TargetIsCasting();
            bool inMeleeRange = Target.GetDistance < RangeManager.GetMeleeRangeWithTarget();
            bool saveRage = Cleave.KnownSpell
                && unitCache.EnemiesAttackingMe.Count > 1
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && settings.PFU_UseCleave
                || Execute.KnownSpell && Target.HealthPercent < 40
                || Bloodthirst.KnownSpell && Me.Rage < 40 && Target.HealthPercent > 50;

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Check if we need to interrupt
            if (shouldBeInterrupted)
            {
                fightingACaster = true;
                if (!casterEnemies.Contains(Target.Name))
                    casterEnemies.Add(Target.Name);
            }

            // Intercept
            if (Target.GetDistance > 12f
                && Target.HealthPercent < 90
                && Target.GetDistance < 24f
                && cast.OnTarget(Intercept))
                return;

            // Berserker stance
            if (!InBerserkStance()
                && cast.OnSelf(BerserkerStance))
                return;

            // Interrupt
            if (shouldBeInterrupted
                && cast.OnTarget(Pummel))
                return;

            // Victory Rush
            if (cast.OnTarget(VictoryRush))
                return;

            // Rampage
            if (!Me.HasAura(Rampage) || Me.HasAura(Rampage) && WTEffects.BuffTimeLeft("Rampage") < 10)
                if (cast.OnTarget(Rampage))
                    return;

            // Execute
            if (cast.OnTarget(Execute))
                return;

            // Overpower
            if (cast.OnTarget(Overpower))
                return;

            // Bloodthirst
            if (inMeleeRange
                && cast.OnTarget(Bloodthirst))
                return;

            // Whirlwind
            if (inMeleeRange
                && Me.Rage > 30
                && cast.OnTarget(Whirlwind))
                return;

            // Sweeping Strikes
            if (inMeleeRange
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && cast.OnTarget(SweepingStrikes))
                return;

            // Cleave
            if (inMeleeRange
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && (!SweepingStrikes.IsSpellUsable || !SweepingStrikes.KnownSpell)
                && Me.Rage > 40
                && settings.PFU_UseCleave
                && cast.OnTarget(Cleave))
                return;

            // Blood Rage
            if (settings.PFU_UseBloodRage
                && Me.HealthPercent > 90
                && cast.OnSelf(BloodRage))
                return;

            // Hamstring
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && inMeleeRange
                && settings.PFU_UseHamstring
                && Target.HealthPercent < 40
                && !Target.HasAura(Hamstring)
                && cast.OnTarget(Hamstring))
                return;

            // Commanding Shout
            if (!Me.HasAura(CommandingShout)
                && settings.PFU_UseCommandingShout
                && cast.OnSelf(CommandingShout))
                return;

            // Battle Shout
            if (!Me.HasAura(BattleShout)
                && (!settings.PFU_UseCommandingShout || !CommandingShout.KnownSpell)
                && cast.OnSelf(BattleShout))
                return;

            // Heroic Strike (after whirlwind)
            if (inMeleeRange
                && !WTCombat.IsSpellActive("Heroic Strike")
                && Me.Rage > 60
                && cast.OnTarget(HeroicStrike))
                return;

            // Heroic Strike (before whirlwind)
            if (inMeleeRange
                && !Whirlwind.KnownSpell
                && !WTCombat.IsSpellActive("Heroic Strike")
                && (!saveRage || Me.Rage > 60)
                && cast.OnTarget(HeroicStrike))
                return;
        }
    }
}
