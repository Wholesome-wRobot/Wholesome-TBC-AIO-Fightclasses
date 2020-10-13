using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.ObjectManager;

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

            // Check if surrounding enemies
            if (ObjectManager.Target.GetDistance < _pullRange && !_pullFromAfar)
                _pullFromAfar = ToolBox.CheckIfEnemiesOnPull(ObjectManager.Target, _pullRange);

            // Check stance
            if (!InBattleStance()
                && ObjectManager.Me.Rage < 10
                && !_pullFromAfar
                && !settings.AlwaysPull)
                Cast(BattleStance);

            // Pull from afar
            if (_pullFromAfar
                && _pullMeleeTimer.ElapsedMilliseconds < 5000 || settings.AlwaysPull
                && ObjectManager.Target.GetDistance < 24f)
            {
                Spell pullMethod = null;

                if (Shoot.IsSpellUsable
                    && Shoot.KnownSpell)
                    pullMethod = Shoot;

                if (Throw.IsSpellUsable
                    && Throw.KnownSpell)
                    pullMethod = Throw;

                if (pullMethod == null)
                {
                    Logger.Log("Can't pull from distance. Please equip a ranged weapon in order to Throw or Shoot.");
                    _pullFromAfar = false;
                }
                else
                {
                    if (Me.IsMounted)
                        MountTask.DismountMount();

                    RangeManager.SetRange(_pullRange);
                    if (Cast(pullMethod))
                        Thread.Sleep(2000);
                }
            }

            // Melee ?
            if (_pullMeleeTimer.ElapsedMilliseconds <= 0
                && ObjectManager.Target.GetDistance <= _pullRange + 3)
                _pullMeleeTimer.Start();

            if (_pullMeleeTimer.ElapsedMilliseconds > 5000)
            {
                Logger.LogDebug("Going in Melee range");
                RangeManager.SetRangeToMelee();
                _pullMeleeTimer.Reset();
            }

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Charge Battle Stance
            if (InBattleStance()
                && ObjectManager.Target.GetDistance > 9f
                && ObjectManager.Target.GetDistance < 24f
                && !_pullFromAfar)
                if (Cast(Charge))
                    return;

            // Charge Berserker Stance
            if (InBerserkStance()
                && ObjectManager.Target.GetDistance > 9f
                && ObjectManager.Target.GetDistance < 24f
                && !_pullFromAfar)
                if (Cast(Intercept))
                    return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;
            bool _shouldBeInterrupted = ToolBox.EnemyCasting();
            bool _inMeleeRange = Target.GetDistance < 6f;
            bool _saveRage = Cleave.KnownSpell
                && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.CheckIfEnemiesClose(15f)
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

            // Melee ?
            if (_pullMeleeTimer.ElapsedMilliseconds > 0)
                _pullMeleeTimer.Reset();

            if (_meleeTimer.ElapsedMilliseconds <= 0
                && _pullFromAfar)
                _meleeTimer.Start();

            if ((_shouldBeInterrupted || _meleeTimer.ElapsedMilliseconds > 5000)
                && !RangeManager.CurrentRangeIsMelee())
            {
                Logger.LogDebug("Going in Melee range 2");
                RangeManager.SetRangeToMelee();
                _meleeTimer.Stop();
            }

            // Intercept
            if (InBerserkStance()
                && ObjectManager.Target.GetDistance > 12f
                && ObjectManager.Target.GetDistance < 24f)
                if (Cast(Intercept))
                    return;

            // Battle stance
            if (InBerserkStance()
                && Me.Rage < 10
                && (!settings.PrioritizeBerserkStance || ObjectManager.GetNumberAttackPlayer() > 1)
                && !_fightingACaster)
                if (Cast(BattleStance))
                    return;

            // Berserker stance
            if (settings.PrioritizeBerserkStance
                && !InBerserkStance()
                && BerserkerStance.KnownSpell
                && Me.Rage < 15
                && ObjectManager.GetNumberAttackPlayer() < 2)
                if (Cast(BerserkerStance))
                    return;

            // Fighting a caster
            if (_fightingACaster
                && !InBerserkStance()
                && BerserkerStance.KnownSpell
                && Me.Rage < 20
                && ObjectManager.GetNumberAttackPlayer() < 2)
            {
                if (Cast(BerserkerStance))
                    return;
            }

            // Interrupt
            if (_shouldBeInterrupted 
                && InBerserkStance())
            {
                Thread.Sleep(Main.humanReflexTime);
                if (Cast(Pummel))
                    return;
            }

            // Victory Rush
            if (VictoryRush.KnownSpell)
                if (Cast(VictoryRush))
                    return;

            // Rampage
            if (Rampage.KnownSpell
                && (!Me.HaveBuff("Rampage") || Me.HaveBuff("Rampage") && ToolBox.BuffTimeLeft("Rampage") < 10))
                if (Cast(Rampage))
                    return;

            // Berserker Rage
            if (InBerserkStance()
                && Target.HealthPercent > 70)
                if (Cast(BerserkerRage))
                    return;

            // Execute
            if (Target.HealthPercent < 20)
                if (Cast(Execute))
                    return;

            // Overpower
            if (Overpower.IsSpellUsable)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (Cast(Overpower))
                    return;
            }

            // Bloodthirst
            if (_inMeleeRange)
                if (Cast(Bloodthirst))
                    return;

            // Whirlwind
            if (_inMeleeRange
                && InBerserkStance()
                && Me.Rage > 30)
                if (Cast(Whirlwind))
                    return;

            // Sweeping Strikes
            if (_inMeleeRange
                && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.CheckIfEnemiesClose(15f))
                if (Cast(SweepingStrikes))
                    return;

            // Retaliation
            if (_inMeleeRange && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.CheckIfEnemiesClose(15f))
                if (Cast(Retaliation) && (!SweepingStrikes.IsSpellUsable || !SweepingStrikes.KnownSpell))
                    return;

            // Cleave
            if (_inMeleeRange
                && ObjectManager.GetNumberAttackPlayer() > 1
                && ToolBox.CheckIfEnemiesClose(15f) &&
                (!SweepingStrikes.IsSpellUsable || !SweepingStrikes.KnownSpell) && ObjectManager.Me.Rage > 40
                && settings.UseCleave)
                if (Cast(Cleave))
                    return;

            // Blood Rage
            if (settings.UseBloodRage
                && Me.HealthPercent > 90)
                if (Cast(BloodRage))
                    return;

            // Hamstring
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && _inMeleeRange
                && settings.UseHamstring
                && Target.HealthPercent < 40
                && !Target.HaveBuff("Hamstring"))
                if (Cast(Hamstring))
                    return;

            // Commanding Shout
            if (!Me.HaveBuff("Commanding Shout")
                && settings.UseCommandingShout
                && CommandingShout.KnownSpell)
                if (Cast(CommandingShout))
                    return;

            // Battle Shout
            if (!Me.HaveBuff("Battle Shout")
                && (!settings.UseCommandingShout || !CommandingShout.KnownSpell))
                if (Cast(BattleShout))
                    return;

            // Rend
            if (!Target.HaveBuff("Rend")
                && ToolBox.CanBleed(Target)
                && _inMeleeRange
                && settings.UseRend
                && Target.HealthPercent > 25)
                if (Cast(Rend))
                    return;

            // Demoralizing Shout
            if (settings.UseDemoralizingShout
                && !Target.HaveBuff("Demoralizing Shout")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || !ToolBox.CheckIfEnemiesClose(15f)) && _inMeleeRange)
                if (Cast(DemoralizingShout))
                    return;

            // Heroic Strike (after whirlwind)
            if (_inMeleeRange
                && Whirlwind.KnownSpell
                && !HeroicStrikeOn()
                &&  Me.Rage > 60)
                if (Cast(HeroicStrike))
                    return;

            // Heroic Strike (before whirlwind)
            if (_inMeleeRange
                && !Whirlwind.KnownSpell
                && !HeroicStrikeOn()
                && (!_saveRage || Me.Rage > 60))
                if (Cast(HeroicStrike))
                    return;
        }
    }
}
