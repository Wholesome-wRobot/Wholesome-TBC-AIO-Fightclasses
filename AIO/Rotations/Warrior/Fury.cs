using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class Fury : Warrior
    {
        public Fury(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

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
                && !pullFromAfar
                && !settings.AlwaysPull)
                cast.OnSelf(BattleStance);

            // Charge Battle Stance
            if (InBattleStance()
                && ObjectManager.Target.GetDistance > 9f
                && ObjectManager.Target.GetDistance < 24f
                && !pullFromAfar
                && cast.OnTarget(Charge))
                return;

            // Charge Berserker Stance
            if (InBerserkStance()
                && ObjectManager.Target.GetDistance > 9f
                && ObjectManager.Target.GetDistance < 24f
                && !pullFromAfar
                && cast.OnTarget(Intercept))
                return;

            // Check if caster in list
            if (casterEnemies.Contains(ObjectManager.Target.Name))
                fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { Shoot, Throw }, unitCache))
            {
                combatMeleeTimer = new Timer(2000);
                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool shouldBeInterrupted = WTCombat.TargetIsCasting();
            bool inMeleeRange = Target.GetDistance < 6f;
            bool saveRage = Cleave.KnownSpell
                && unitCache.EnemiesAttackingMe.Count > 1
                && ToolBox.GetNbEnemiesClose(15f) > 1
                && settings.UseCleave
                || Execute.KnownSpell && Target.HealthPercent < 40
                || Bloodthirst.KnownSpell && ObjectManager.Me.Rage < 40 && Target.HealthPercent > 50;

            // Force melee
            if (combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check if we need to interrupt
            if (shouldBeInterrupted)
            {
                fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!casterEnemies.Contains(Target.Name))
                    casterEnemies.Add(Target.Name);
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
                && unitCache.EnemiesAttackingMe.Count > 1
                && !fightingACaster
                && cast.OnSelf(BattleStance))
                return;

            // Berserker stance
            if (settings.PrioritizeBerserkStance
                && !InBerserkStance()
                && unitCache.EnemiesAttackingMe.Count < 2
                && cast.OnSelf(BerserkerStance))
                return;

            // Fighting a caster
            if (fightingACaster
                && !InBerserkStance()
                && Me.Rage < 20
                && unitCache.EnemiesAttackingMe.Count < 2
                && cast.OnSelf(BerserkerStance))
                return;

            // Interrupt
            if (shouldBeInterrupted
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
            if ((!Me.HasAura(Rampage) || Me.HasAura(Rampage) && WTEffects.BuffTimeLeft("Rampage") < 10)
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
            if (inMeleeRange
                && cast.OnTarget(Bloodthirst))
                return;

            // Whirlwind
            if (inMeleeRange
                && InBerserkStance()
                && Me.Rage > 30
                && cast.OnTarget(Whirlwind))
                return;

            int nbEnemiesCLose = ToolBox.GetNbEnemiesClose(15f);
            // Sweeping Strikes
            if (inMeleeRange
                && unitCache.EnemiesAttackingMe.Count > 1
                && nbEnemiesCLose > 1
                && cast.OnTarget(SweepingStrikes))
                return;

            // Retaliation
            if (inMeleeRange && unitCache.EnemiesAttackingMe.Count > 1
                && nbEnemiesCLose > 1)
                if (cast.OnTarget(Retaliation) && (!SweepingStrikes.IsSpellUsable || !SweepingStrikes.KnownSpell))
                    return;

            // Cleave
            if (inMeleeRange
                && unitCache.EnemiesAttackingMe.Count > 1
                && nbEnemiesCLose > 1
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
                && inMeleeRange
                && settings.UseHamstring
                && Target.HealthPercent < 40
                && !Target.HasAura(Hamstring)
                && cast.OnTarget(Hamstring))
                return;

            // Commanding Shout
            if (!Me.HasAura(CommandingShout)
                && settings.UseCommandingShout
                && cast.OnSelf(CommandingShout))
                return;

            // Battle Shout
            if (!Me.HasAura(BattleShout)
                && (!settings.UseCommandingShout || !CommandingShout.KnownSpell)
                && cast.OnSelf(BattleShout))
                return;

            // Rend
            if (!Target.HasAura(Rend)
                && inMeleeRange
                && settings.UseRend
                && Target.HealthPercent > 25
                && cast.OnTarget(Rend))
                return;

            // Demoralizing Shout
            if (settings.UseDemoralizingShout
                && !Target.HasAura(DemoralizingShout)
                && !Target.HasBuff("Demoralizing Roar")
                && (unitCache.EnemiesAttackingMe.Count > 1 || ToolBox.GetNbEnemiesClose(15f) <= 0)
                && inMeleeRange
                && cast.OnSelf(DemoralizingShout))
                return;

            // Heroic Strike (after whirlwind)
            if (inMeleeRange
                && Whirlwind.KnownSpell
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
