using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Rogue
{
    public class RogueCombatParty : Rogue
    {
        public RogueCombatParty(BaseSettings settings) : base(settings)
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
            if (_casterEnemies.Contains(Target.Name))
                _fightingACaster = true;

            // Stealth
            if (!Me.HasAura(Stealth)
                && Target.GetDistance > 15f
                && Target.GetDistance < 25f
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
            if (_shouldBeInterrupted
                && cast.OnTarget(Kick))
                return;

            // Adrenaline Rush
            if (!Me.HasAura(AdrenalineRush)
                && cast.OnSelf(AdrenalineRush))
                return;

            // Blade Flurry
            if (!Me.HasAura(BladeFlurry)
                && cast.OnSelf(BladeFlurry))
                return;

            // Riposte
            if (Riposte.IsSpellUsable
                && (Target.CreatureTypeTarget.Equals("Humanoid") || settings.RiposteAll)
                && cast.OnTarget(Riposte))
                return;

            // Bandage
            if (!Target.IsTargetingMe
                && Me.HealthPercent < 40)
            {
                MovementManager.StopMoveTo(true, 500);
                ItemsManager.UseItemByNameOrId(_myBestBandage);
                Logger.Log("Using " + _myBestBandage);
                Usefuls.WaitIsCasting();
                return;
            }

            // Blind
            if (Me.HealthPercent < 40
                && Target.IsTargetingMe
                && !WTEffects.HasDebuff("Recently Bandaged")
                && _myBestBandage != null
                && settings.UseBlindBandage
                && cast.OnTarget(Blind))
                return;

            // Evasion
            if (Me.HealthPercent < 30
                && !Me.HasAura(Evasion)
                && cast.OnSelf(Evasion))
                return;

            // Cloak of Shadows
            if (Me.HealthPercent < 30
                && !Me.HasAura(CloakOfShadows)
                && Target.HealthPercent > 50
                && cast.OnSelf(CloakOfShadows))
                return;

            // DPS ROTATION

            // Slice and Dice
            if ((!Me.HasAura(SliceAndDice) || WTEffects.BuffTimeLeft("Slice and Dice") < 4)
                && Me.ComboPoint > 0
                && cast.OnTarget(SliceAndDice))
                return;

            // Shiv
            if (Target.HasAura("Deadly Poison")
                && WTEffects.DeBuffTimeLeft("Deadly Poison", "target") < 3
                && cast.OnTarget(Shiv))
                return;

            // Rupture
            if (!Target.HasAura(Rupture)
                && Me.ComboPoint > 1
                && cast.OnTarget(Rupture))
                return;

            // Eviscerate
            if (WTEffects.DeBuffTimeLeft("Rupture", "target") > 5
                && Me.ComboPoint > 3
                && cast.OnTarget(Eviscerate))
                return;

            // GhostlyStrike
            if (Me.ComboPoint < 5
                && cast.OnTarget(GhostlyStrike))
                return;

            // Hemohrrage
            if (Me.ComboPoint < 5
                && cast.OnTarget(Hemorrhage))
                return;

            // Backstab
            if (Me.ComboPoint < 5
                && Me.Energy >= 60
                && BehindTargetCheck
                && cast.OnTarget(Backstab))
                return;

            // Sinister Strike
            if (Me.ComboPoint < 5
                && Me.Energy >= 60
                && cast.OnTarget(SinisterStrike))
                return;
        }
    }
}
