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
            _pullFromAfar = false;

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Stealth
            if (!Me.HaveBuff("Stealth")
                && !_pullFromAfar 
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f 
                && settings.StealthApproach && Backstab.KnownSpell
                && (!ToolBox.HasPoisonDebuff() || settings.StealthWhenPoisoned))
                if (cast.Normal(Stealth))
                    return;

            // Stealth approach
            if (Me.HaveBuff("Stealth")
                && ObjectManager.Target.GetDistance > 3f
                && !_isStealthApproching)
            {
                float desiredDistance = RangeManager.GetMeleeRangeWithTarget() - 4f;
                RangeManager.SetRangeToMelee();
                _stealthApproachTimer.Start();
                _isStealthApproching = true;
                if (ObjectManager.Me.IsAlive && ObjectManager.Target.IsAlive)
                {
                    while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && ObjectManager.Target.GetDistance > 2.5f
                    && ObjectManager.Target.GetDistance <= RangeManager.GetMeleeRangeWithTarget()
                    && Fight.InFight
                    && _stealthApproachTimer.ElapsedMilliseconds <= 15000
                    && Me.HaveBuff("Stealth"))
                    {
                        ToggleAutoAttack(false);
                        Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f);
                        MovementManager.MoveTo(position);
                        Thread.Sleep(50);
                        CastOpener();
                    }
                    ToggleAutoAttack(true);
                    _isStealthApproching = false;
                }
            }

            // Auto
            if (ObjectManager.Target.GetDistance < 6f && !Me.HaveBuff("Stealth"))
                ToggleAutoAttack(true);
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();
            bool _inMeleeRange = ObjectManager.Target.GetDistance < 6f;

            WoWUnit Target = ObjectManager.Target;

            // Check Auto-Attacking
            ToggleAutoAttack(true);

            // Check if interruptable enemy is in list
            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                if (!_casterEnemies.Contains(ObjectManager.Target.Name))
                    _casterEnemies.Add(ObjectManager.Target.Name);
            }

            // Kick interrupt
            if (_shouldBeInterrupted
                && cast.Normal(Kick))
                return;

            // Adrenaline Rush
            if (!Me.HaveBuff("Adrenaline Rush")
                && cast.Normal(AdrenalineRush))
                return;

            // Blade Flurry
            if (!Me.HaveBuff("Blade Flurry")
                && cast.Normal(BladeFlurry))
                return;

            // Riposte
            if (Riposte.IsSpellUsable 
                && (Target.CreatureTypeTarget.Equals("Humanoid") || settings.RiposteAll)
                && cast.Normal(Riposte))
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
                && cast.Normal(Blind))
                return;

            // Evasion
            if (Me.HealthPercent < 30 
                && !Me.HaveBuff("Evasion")
                && cast.Normal(Evasion))
                return;

            // Cloak of Shadows
            if (Me.HealthPercent < 30 
                && !Me.HaveBuff("Cloak of Shadows") 
                && Target.HealthPercent > 50
                && cast.Normal(CloakOfShadows))
                return;

            // DPS ROTATION

            // Slice and Dice
            if ((!Me.HaveBuff("Slice and Dice") || ToolBox.BuffTimeLeft("Slice and Dice") < 4)
                && Me.ComboPoint > 0
                && cast.Normal(SliceAndDice))
                return;

            // Shiv
            if (Target.HaveBuff("Deadly Poison")
                && ToolBox.DeBuffTimeLeft("Deadly Poison", "target") < 3
                && cast.Normal(Shiv))
                return;

            // Rupture
            if (!Target.HaveBuff("Rupture")
                && Me.ComboPoint > 1
                && cast.Normal(Rupture))
                return;
            /*
            // Backstab in combat
            if (ToolBox.GetMHWeaponType() == "Daggers"
                && ToolBox.MeBehindTarget()
                && cast.Normal(Backstab))
                return;
            */
            // Eviscerate logic
            if (ToolBox.DeBuffTimeLeft("Rupture", "target") > 5
                && Me.ComboPoint > 3
                && cast.Normal(Eviscerate))
                return;

            // GhostlyStrike
            if (Me.ComboPoint < 5 
                && cast.Normal(GhostlyStrike))
                return;

            // Hemohrrage
            if (Me.ComboPoint < 5 
                && cast.Normal(Hemorrhage))
                return;

            // Sinister Strike / Backstab
            if (Me.ComboPoint < 5
                && Me.Energy >= 60
                && cast.Normal(Backstab)
                && cast.Normal(SinisterStrike))
                return;

            // Sinister Strike
            if (Me.ComboPoint < 5
                && Me.Energy >= 60
                && cast.Normal(SinisterStrike))
                return;
        }
    }
}
