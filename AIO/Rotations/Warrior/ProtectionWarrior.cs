using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class ProtectionWarrior : Warrior
    {
        public ProtectionWarrior(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Tank;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Defensive Stance
            if (InBattleStance() || !Taunt.IsSpellUsable)
                cast.OnSelf(DefensiveStance);
        }

        protected override void Pull()
        {
            base.Pull();

            // Defensive Stance
            if (InBattleStance()
                && cast.OnSelf(DefensiveStance))
                return;

            // Check if caster in list
            if (casterEnemies.Contains(Target.Name))
                fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { Shoot, Throw }, unitCache))
            {
                combatMeleeTimer = new Timer(2000);
                return;
            }
        }

        protected override void CombatNoTarget()
        {
            base.CombatNoTarget();

            if (settings.PartyTankSwitchTarget)
                partyManager.SwitchTarget(cast, settings.PartyUseIntervene ? Intervene : null);
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            bool shouldBeInterrupted = WTCombat.TargetIsCasting();
            bool inMeleeRange = Target.GetDistance < RangeManager.GetMeleeRangeWithTarget();

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

            if (settings.PartyTankSwitchTarget)
                partyManager.SwitchTarget(cast, settings.PartyUseIntervene ? Intervene : null);

            // Defensive Stance
            if (InBattleStance())
                cast.OnTarget(DefensiveStance);

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Taunt
            if (inMeleeRange
                && !Target.IsTargetingMe
                && Target.Target > 0
                && cast.OnTarget(Taunt))
                return;

            // Cleave
            List<IWoWUnit> closeEnemies = unitCache.EnemiesFighting
                .FindAll(e => e.GetDistance < 10);
            if (inMeleeRange
                && closeEnemies.Count > 1
                && Me.Rage > 70)
                cast.OnTarget(Cleave);

            // Heroic Strike
            if (inMeleeRange
                && !WTCombat.IsSpellActive("Heroic Strike")
                && Me.Rage > 90)
                cast.OnTarget(HeroicStrike);

            // Last Stand
            if (Me.HealthPercent < 20
                && cast.OnSelf(LastStand))
                return;

            // Shied Bash
            if (shouldBeInterrupted
                && cast.OnTarget(ShieldBash))
                return;

            // Demoralizing Shout
            if (settings.UseDemoralizingShout
                && !Target.HasAura(DemoralizingShout)
                && !Target.HasAura("Demoralizing Roar")
                && inMeleeRange
                && cast.OnSelf(DemoralizingShout))
                return;

            // Thunderclap
            if (inMeleeRange
                && !Target.HasAura(ThunderClap)
                && cast.OnSelf(ThunderClap))
                return;

            // Shield Slam
            if (inMeleeRange
                && Me.Rage > 70
                && ShieldSlam.IsSpellUsable
                && cast.OnTarget(ShieldSlam))
                return;

            // Revenge
            if (inMeleeRange
                && cast.OnTarget(Revenge))
                return;

            // Devastate
            if (inMeleeRange
                && cast.OnTarget(Devastate))
                return;

            // Sunder Armor
            if (inMeleeRange
                && cast.OnTarget(SunderArmor))
                return;

            // Shield Block
            if (Me.HealthPercent < 50
                && cast.OnSelf(ShieldBlock))
                return;

            // Spell Reflection
            if (shouldBeInterrupted
                && cast.OnSelf(SpellReflection))
                return;

            // Commanding Shout
            if (!Me.HasAura(CommandingShout)
                && settings.UseCommandingShout
                && cast.OnSelf(CommandingShout))
                return;
        }
    }
}
