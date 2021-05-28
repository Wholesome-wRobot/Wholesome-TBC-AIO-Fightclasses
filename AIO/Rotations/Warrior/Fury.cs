using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class Fury : Warrior
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();
        }

        protected override void Pull()
        {
            base.Pull();

            // Check stance
            if (!InBattleStance()
                && ObjectManager.Me.Rage < 10
                && !_pullFromAfar
                && !settings.AlwaysPull)
                cast.OnSelf(BattleStance);

            // Charge Battle Stance
            if (InBattleStance()
                && ObjectManager.Target.GetDistance > 9f
                && ObjectManager.Target.GetDistance < 24f
                && !_pullFromAfar
                && cast.OnTarget(Charge))
                return;

            // Charge Berserker Stance
            if (InBerserkStance()
                && ObjectManager.Target.GetDistance > 9f
                && ObjectManager.Target.GetDistance < 24f
                && !_pullFromAfar
                && cast.OnTarget(Intercept))
                return;

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { Shoot, Throw }))
            {
                _combatMeleeTimer = new Timer(2000);
                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;
            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();
            bool _inMeleeRange = Target.GetDistance < 6f;
            bool _saveRage = Cleave.KnownSpell
                && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && settings.UseCleave
                || Execute.KnownSpell && Target.HealthPercent < 40
                || Bloodthirst.KnownSpell && ObjectManager.Me.Rage < 40 && Target.HealthPercent > 50;

            // Force melee
            if (_combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check if we need to interrupt
            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
            }

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Intercept
            if (InBerserkStance()
                && ObjectManager.Target.GetDistance > 12f
                && ObjectManager.Target.GetDistance < 24f
                && cast.OnTarget(Intercept))
                return;

            // Battle stance
            if (InBerserkStance()
                && Me.Rage < 10
                && !settings.PrioritizeBerserkStance 
                && ObjectManager.GetNumberAttackPlayer() > 1
                && !_fightingACaster
                && cast.OnSelf(BattleStance))
                return;

            // Berserker stance
            if (settings.PrioritizeBerserkStance
                && !InBerserkStance()
                && ObjectManager.GetNumberAttackPlayer() < 2
                && cast.OnSelf(BerserkerStance))
                return;

            // Fighting a caster
            if (_fightingACaster
                && !InBerserkStance()
                && Me.Rage < 20
                && ObjectManager.GetNumberAttackPlayer() < 2
                && cast.OnSelf(BerserkerStance))
                return;

            // Interrupt
            if (_shouldBeInterrupted 
                && InBerserkStance())
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(Pummel))
                    return;
            }

            // Victory Rush
            if (cast.OnTarget(VictoryRush))
                return;

            // Rampage
            if ((!Me.HaveBuff("Rampage") || Me.HaveBuff("Rampage") && ToolBox.BuffTimeLeft("Rampage") < 10)
                && cast.OnTarget(Rampage))
                return;

            // Berserker Rage
            if (InBerserkStance()
                && Target.HealthPercent > 70
                && cast.OnSelf(BerserkerRage))
                return;

            // Execute
            if (cast.OnTarget(Execute))
                return;

            // Overpower
            if (Overpower.IsSpellUsable)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(Overpower))
                    return;
            }

            // Bloodthirst
            if (_inMeleeRange
                && cast.OnTarget(Bloodthirst))
                return;

            // Whirlwind
            if (_inMeleeRange
                && InBerserkStance()
                && Me.Rage > 30
                && cast.OnTarget(Whirlwind))
                return;

            // Sweeping Strikes
            if (_inMeleeRange
                && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && cast.OnTarget(SweepingStrikes))
                return;

            // Retaliation
            if (_inMeleeRange && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.GetNbEnemiesClose(15f) > 1)
                if (cast.OnTarget(Retaliation) && (!SweepingStrikes.IsSpellUsable || !SweepingStrikes.KnownSpell))
                    return;

            // Cleave
            if (_inMeleeRange
                && ObjectManager.GetNumberAttackPlayer() > 1
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

            // Rend
            if (!Target.HaveBuff("Rend")
                && _inMeleeRange
                && settings.UseRend
                && Target.HealthPercent > 25
                && cast.OnTarget(Rend))
                return;

            // Demoralizing Shout
            if (settings.UseDemoralizingShout
                && !Target.HaveBuff("Demoralizing Shout")
                && !Target.HaveBuff("Demoralizing Roar")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || ToolBox.GetNbEnemiesClose(15f) <= 0) 
                && _inMeleeRange
                && cast.OnSelf(DemoralizingShout))
                return;

            // Heroic Strike (after whirlwind)
            if (_inMeleeRange
                && Whirlwind.KnownSpell
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
