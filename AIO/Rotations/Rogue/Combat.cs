using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Rogue
{
    public class Combat : Rogue
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();
        }

        protected override void Pull()
        {
            base.Pull();

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull || ToolBox.HasPoisonDebuff(), new List<AIOSpell> { Shoot, Throw }))
            {
                _combatMeleeTimer = new Timer(2000);
                return;
            }

            // Stealth
            if (!Me.HaveBuff("Stealth") 
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f 
                && ToolBox.GetClosestHostileFrom(ObjectManager.Target, 20) == null
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
            if (_shouldBeInterrupted)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(Kick) || cast.OnTarget(Gouge) || cast.OnTarget(KidneyShot))
                    return;
            }

            // Adrenaline Rush
            if ((ObjectManager.GetNumberAttackPlayer() > 1 || Target.IsElite) && !Me.HaveBuff("Adrenaline Rush"))
                if (cast.OnTarget(AdrenalineRush))
                    return;

            // Blade Flurry
            if (ObjectManager.GetNumberAttackPlayer() > 1 && !Me.HaveBuff("Blade Flurry"))
                if (cast.OnTarget(BladeFlurry))
                    return;

            // Riposte
            if (Riposte.IsSpellUsable && (Target.CreatureTypeTarget.Equals("Humanoid") || settings.RiposteAll))
                if (cast.OnTarget(Riposte))
                    return;

            // Bandage
            if (Target.HaveBuff("Blind"))
            {
                MovementManager.StopMoveTo(true, 500);
                ItemsManager.UseItemByNameOrId(_myBestBandage);
                Logger.Log("Using " + _myBestBandage);
                Usefuls.WaitIsCasting();
                return;
            }

            // Blind
            if (Me.HealthPercent < 40 && !ToolBox.HasDebuff("Recently Bandaged") && _myBestBandage != null
                && settings.UseBlindBandage)
                if (cast.OnTarget(Blind))
                    return;

            // Evasion
            if (ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 && !Me.HaveBuff("Evasion") && Target.HealthPercent > 50)
                if (cast.OnTarget(Evasion))
                    return;

            // Cloak of Shadows
            if (Me.HealthPercent < 30 && !Me.HaveBuff("Cloak of Shadows") && Target.HealthPercent > 50)
                if (cast.OnTarget(CloakOfShadows))
                    return;

            // Backstab in combat
            if (IsTargetStunned() && ToolBox.GetMHWeaponType().Equals("Daggers"))
                if (cast.OnTarget(Backstab))
                    return;

            // Slice and Dice
            if (!Me.HaveBuff("Slice and Dice") && Me.ComboPoint > 1 && Target.HealthPercent > 40)
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
