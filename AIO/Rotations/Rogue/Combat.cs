using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Rogue
{
    public class Combat : Rogue
    {
        public Combat(BaseSettings settings) : base(settings)
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

            // Check if caster in list
            if (_casterEnemies.Contains(Target.Name))
                _fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull || WTEffects.HasPoisonDebuff(), new List<AIOSpell> { Shoot, Throw }, unitCache))
            {
                _combatMeleeTimer = new Timer(2000);
                return;
            }

            // Stealth
            if (!Me.HasAura(Stealth)
                && Target.GetDistance > 15f
                && Target.GetDistance < 25f
                && unitCache.GetClosestHostileFrom(Target, 20) == null
                && settings.StealthApproach
                && Backstab.KnownSpell
                && (!WTEffects.HasPoisonDebuff() || settings.StealthWhenPoisoned)
                && cast.OnSelf(Stealth))
                return;

            // Stealth approach
            if (Me.HasAura(Stealth)
                && Target.GetDistance > 3f
                && !_isStealthApproching)
                StealthApproach();

            // Auto
            if (Target.GetDistance < 6f && !Me.HasAura(Stealth))
                ToggleAutoAttack(true);
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool _shouldBeInterrupted = WTCombat.TargetIsCasting();

            // Force melee
            if (_combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Check if interruptable enemy is in list
            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
            }

            // Kick interrupt
            if (_shouldBeInterrupted)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(Kick) || cast.OnTarget(Gouge) || cast.OnTarget(KidneyShot))
                    return;
            }

            // Adrenaline Rush
            if ((unitCache.EnemiesAttackingMe.Count > 1 || Target.IsElite) && !Me.HasAura(AdrenalineRush))
                if (cast.OnTarget(AdrenalineRush))
                    return;

            // Blade Flurry
            if (unitCache.EnemiesAttackingMe.Count > 1 && !Me.HasAura(BladeFlurry))
                if (cast.OnTarget(BladeFlurry))
                    return;

            // Riposte
            if (Riposte.IsSpellUsable && (Target.CreatureTypeTarget.Equals("Humanoid") || settings.RiposteAll))
                if (cast.OnTarget(Riposte))
                    return;

            // Bandage
            if (Target.HasAura(Blind))
            {
                MovementManager.StopMoveTo(true, 500);
                ItemsManager.UseItemByNameOrId(_myBestBandage);
                Logger.Log("Using " + _myBestBandage);
                Usefuls.WaitIsCasting();
                return;
            }

            // Blind
            if (Me.HealthPercent < 40
                && !WTEffects.HasDebuff("Recently Bandaged")
                && _myBestBandage != null
                && settings.UseBlindBandage)
                if (cast.OnTarget(Blind))
                    return;

            // Evasion
            if (unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 30 && !Me.HasAura(Evasion) && Target.HealthPercent > 50)
                if (cast.OnTarget(Evasion))
                    return;

            // Cloak of Shadows
            if (Me.HealthPercent < 30
                && !Me.HasAura(CloakOfShadows)
                && Target.HealthPercent > 50)
                if (cast.OnTarget(CloakOfShadows))
                    return;

            // Backstab in combat
            if (IsTargetStunned()
                && WTGear.GetMainHandWeaponType().Equals("Daggers"))
                if (cast.OnTarget(Backstab))
                    return;

            // Slice and Dice
            if (!Me.HasAura(SliceAndDice)
                && Me.ComboPoint > 1
                && Target.HealthPercent > 40)
                if (cast.OnTarget(SliceAndDice))
                    return;

            // Eviscerate logic
            if (Me.ComboPoint > 0 && Target.HealthPercent < 30
                || Me.ComboPoint > 1 && Target.HealthPercent < 45
                || Me.ComboPoint > 2 && Target.HealthPercent < 60
                || Me.ComboPoint > 3 && Target.HealthPercent < 70)
                if (cast.OnTarget(Eviscerate))
                    return;

            // GhostlyStrike
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > GhostlyStrike.Cost + Kick.Cost))
                if (cast.OnTarget(GhostlyStrike))
                    return;

            // Hemohrrage
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > Hemorrhage.Cost + Kick.Cost))
                if (cast.OnTarget(Hemorrhage))
                    return;

            // Sinister Strike
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > SinisterStrike.Cost + Kick.Cost))
                if (cast.OnTarget(SinisterStrike))
                    return;
        }
    }
}
