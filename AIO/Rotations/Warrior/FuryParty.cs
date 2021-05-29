using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class FuryParty : Warrior
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();
        }

        protected override void Pull()
        {
            base.Pull();

            RangeManager.SetRangeToMelee();

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Charge Berserker Stance
            if (InBerserkStance()
                && ObjectManager.Target.GetDistance > 9f
                && ObjectManager.Target.GetDistance < 24f
                && ObjectManager.Target.HealthPercent < 90
                && cast.OnTarget(Intercept))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;
            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();
            bool _inMeleeRange = Target.GetDistance < RangeManager.GetMeleeRangeWithTarget();
            bool _saveRage = Cleave.KnownSpell
                && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && settings.UseCleave
                || Execute.KnownSpell && Target.HealthPercent < 40
                || Bloodthirst.KnownSpell && ObjectManager.Me.Rage < 40 && Target.HealthPercent > 50;

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Check if we need to interrupt
            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
            }

            // Intercept
            if (ObjectManager.Target.GetDistance > 12f
                && ObjectManager.Target.HealthPercent < 90
                && ObjectManager.Target.GetDistance < 24f
                && cast.OnTarget(Intercept))
                return;

            // Berserker stance
            if (!InBerserkStance()
                && cast.OnSelf(BerserkerStance))
                return;

            // Interrupt
            if (_shouldBeInterrupted
                && cast.OnTarget(Pummel))
                return;

            // Victory Rush
            if (cast.OnTarget(VictoryRush))
                return;

            // Rampage
            if (!Me.HaveBuff("Rampage") || Me.HaveBuff("Rampage") && ToolBox.BuffTimeLeft("Rampage") < 10)
                if (cast.OnTarget(Rampage))
                    return;

            // Execute
            if (cast.OnTarget(Execute))
                return;

            // Overpower
            if (cast.OnTarget(Overpower))
                return;

            // Bloodthirst
            if (_inMeleeRange
                && cast.OnTarget(Bloodthirst))
                return;

            // Whirlwind
            if (_inMeleeRange
                && Me.Rage > 30
                && cast.OnTarget(Whirlwind))
                return;

            // Sweeping Strikes
            if (_inMeleeRange
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && cast.OnTarget(SweepingStrikes))
                return;

            // Cleave
            if (_inMeleeRange
                && ToolBox.GetNbEnemiesClose(15f) > 1 
                && (!SweepingStrikes.IsSpellUsable || !SweepingStrikes.KnownSpell) 
                && ObjectManager.Me.Rage > 40
                && settings.UseCleave
                && cast.OnTarget(Cleave))
                return;

            // Blood Rage
            if (settings.UseBloodRage
                && Me.HealthPercent > 90
                && cast.OnSelf(BloodRage))
                return;

            // Hamstring
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && _inMeleeRange
                && settings.UseHamstring
                && Target.HealthPercent < 40
                && !Target.HaveBuff("Hamstring")
                && cast.OnTarget(Hamstring))
                return;

            // Commanding Shout
            if (!Me.HaveBuff("Commanding Shout")
                && settings.UseCommandingShout
                && cast.OnSelf(CommandingShout))
                return;

            // Battle Shout
            if (!Me.HaveBuff("Battle Shout")
                && (!settings.UseCommandingShout || !CommandingShout.KnownSpell)
                && cast.OnSelf(BattleShout))
                return;

            // Heroic Strike (after whirlwind)
            if (_inMeleeRange
                && !HeroicStrikeOn()
                &&  Me.Rage > 60
                && cast.OnTarget(HeroicStrike))
                return;

            // Heroic Strike (before whirlwind)
            if (_inMeleeRange
                && !Whirlwind.KnownSpell
                && !HeroicStrikeOn()
                && (!_saveRage || Me.Rage > 60)
                && cast.OnTarget(HeroicStrike))
                return;
        }
    }
}
