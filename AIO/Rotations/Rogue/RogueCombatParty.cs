using robotManager.Helpful;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Rogue
{
    public class RogueCombatParty : Rogue
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

            // Stealth
            if (!Me.HaveBuff("Stealth")
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f 
                && settings.StealthApproach 
                && Backstab.KnownSpell
                && (!ToolBox.HasPoisonDebuff() || settings.StealthWhenPoisoned)
                && cast.OnSelf(Stealth))
                return;

            // Stealth approach
            if (Me.HaveBuff("Stealth")
                && ObjectManager.Target.GetDistance > 3f
                && !_isStealthApproching)
                StealthApproach();

            // Auto
            if (ObjectManager.Target.GetDistance < 6f && !Me.HaveBuff("Stealth"))
                ToggleAutoAttack(true);
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();

            WoWUnit Target = ObjectManager.Target;

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
                if (!_casterEnemies.Contains(ObjectManager.Target.Name))
                    _casterEnemies.Add(ObjectManager.Target.Name);
            }

            // Kick interrupt
            if (_shouldBeInterrupted
                && cast.OnTarget(Kick))
                return;

            // Adrenaline Rush
            if (!Me.HaveBuff("Adrenaline Rush")
                && cast.OnSelf(AdrenalineRush))
                return;

            // Blade Flurry
            if (!Me.HaveBuff("Blade Flurry")
                && cast.OnSelf(BladeFlurry))
                return;

            // Riposte
            if (Riposte.IsSpellUsable 
                && (Target.CreatureTypeTarget.Equals("Humanoid") || settings.RiposteAll)
                && cast.OnTarget(Riposte))
                return;

            // Bandage
            if (!ObjectManager.Target.IsTargetingMe
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
                && !ToolBox.HasDebuff("Recently Bandaged") 
                && _myBestBandage != null
                && settings.UseBlindBandage
                && cast.OnTarget(Blind))
                return;

            // Evasion
            if (Me.HealthPercent < 30 
                && !Me.HaveBuff("Evasion")
                && cast.OnSelf(Evasion))
                return;

            // Cloak of Shadows
            if (Me.HealthPercent < 30 
                && !Me.HaveBuff("Cloak of Shadows") 
                && Target.HealthPercent > 50
                && cast.OnSelf(CloakOfShadows))
                return;

            // DPS ROTATION

            // Slice and Dice
            if ((!Me.HaveBuff("Slice and Dice") || ToolBox.BuffTimeLeft("Slice and Dice") < 4)
                && Me.ComboPoint > 0
                && cast.OnTarget(SliceAndDice))
                return;

            // Shiv
            if (Target.HaveBuff("Deadly Poison")
                && ToolBox.DeBuffTimeLeft("Deadly Poison", "target") < 3
                && cast.OnTarget(Shiv))
                return;

            // Rupture
            if (!Target.HaveBuff("Rupture")
                && Me.ComboPoint > 1
                && cast.OnTarget(Rupture))
                return;

            // Eviscerate
            if (ToolBox.DeBuffTimeLeft("Rupture", "target") > 5
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
