using robotManager.Helpful;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

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

            // Check if surrounding enemies
            if (ObjectManager.Target.GetDistance < _pullRange && !_pullFromAfar)
                _pullFromAfar = ToolBox.CheckIfEnemiesAround(ObjectManager.Target, _pullRange);

            // Pull from afar
            if (_pullFromAfar && _pullMeleeTimer.ElapsedMilliseconds < 5000 || settings.AlwaysPull
                && ObjectManager.Target.GetDistance <= _pullRange)
            {
                AIOSpell pullMethod = null;

                if (Shoot.IsSpellUsable && Shoot.KnownSpell)
                    pullMethod = Shoot;

                if (Throw.IsSpellUsable && Throw.KnownSpell)
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
                    if (cast.Normal(pullMethod))
                        Thread.Sleep(2000);
                }
            }

            // Melee ?
            if (_pullMeleeTimer.ElapsedMilliseconds <= 0 && ObjectManager.Target.GetDistance <= _pullRange + 3)
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

            // Stealth
            if (!Me.HaveBuff("Stealth") && !_pullFromAfar && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f && settings.StealthApproach && Backstab.KnownSpell
                && (!ToolBox.HasPoisonDebuff() || settings.StealthWhenPoisoned))
                if (cast.Normal(Stealth))
                    return;

            // Un-Stealth
            if (Me.HaveBuff("Stealth") && _pullFromAfar && ObjectManager.Target.GetDistance > 15f)
                if (cast.Normal(Stealth))
                    return;

            // Stealth approach
            if (Me.HaveBuff("Stealth")
                && ObjectManager.Target.GetDistance > 3f
                && !_isStealthApproching
                && !_pullFromAfar)
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
                    && !ToolBox.CheckIfEnemiesAround(ObjectManager.Target, _pullRange)
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

                    if (ToolBox.CheckIfEnemiesAround(ObjectManager.Target, _pullRange)
                        && Me.HaveBuff("Stealth"))
                    {
                        _pullFromAfar = true;
                        if (cast.Normal(Stealth))
                            return;
                    }

                    if (_stealthApproachTimer.ElapsedMilliseconds > 15000)
                    {
                        Logger.Log("_stealthApproachTimer time out");
                        _pullFromAfar = true;
                    }

                    //ToggleAutoAttack(true);
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

            // Melee ?
            if (_pullMeleeTimer.ElapsedMilliseconds > 0)
                _pullMeleeTimer.Reset();

            if (_meleeTimer.ElapsedMilliseconds <= 0 && _pullFromAfar)
                _meleeTimer.Start();

            if ((_shouldBeInterrupted || _meleeTimer.ElapsedMilliseconds > 5000)
                && !RangeManager.CurrentRangeIsMelee())
            {
                Logger.LogDebug("Going in Melee range 2");
                RangeManager.SetRangeToMelee();
                _meleeTimer.Stop();
            }

            // Kick interrupt
            if (_shouldBeInterrupted)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.Normal(Kick) || cast.Normal(Gouge) || cast.Normal(KidneyShot))
                    return;
            }

            // Adrenaline Rush
            if ((ObjectManager.GetNumberAttackPlayer() > 1 || Target.IsElite) && !Me.HaveBuff("Adrenaline Rush"))
                if (cast.Normal(AdrenalineRush))
                    return;

            // Blade Flurry
            if (ObjectManager.GetNumberAttackPlayer() > 1 && !Me.HaveBuff("Blade Flurry"))
                if (cast.Normal(BladeFlurry))
                    return;

            // Riposte
            if (Riposte.IsSpellUsable && (Target.CreatureTypeTarget.Equals("Humanoid") || settings.RiposteAll))
                if (cast.Normal(Riposte))
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
                if (cast.Normal(Blind))
                    return;

            // Evasion
            if (ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 && !Me.HaveBuff("Evasion") && Target.HealthPercent > 50)
                if (cast.Normal(Evasion))
                    return;

            // Cloak of Shadows
            if (Me.HealthPercent < 30 && !Me.HaveBuff("Cloak of Shadows") && Target.HealthPercent > 50)
                if (cast.Normal(CloakOfShadows))
                    return;

            // Backstab in combat
            if (IsTargetStunned() && ToolBox.GetMHWeaponType().Equals("Daggers"))
                if (cast.Normal(Backstab))
                    return;

            // Slice and Dice
            if (!Me.HaveBuff("Slice and Dice") && Me.ComboPoint > 1 && Target.HealthPercent > 40)
                if (cast.Normal(SliceAndDice))
                    return;

            // Eviscerate logic
            if (Me.ComboPoint > 0 && Target.HealthPercent < 30
                || Me.ComboPoint > 1 && Target.HealthPercent < 45
                || Me.ComboPoint > 2 && Target.HealthPercent < 60
                || Me.ComboPoint > 3 && Target.HealthPercent < 70)
                if (cast.Normal(Eviscerate))
                    return;

            // GhostlyStrike
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > GhostlyStrike.Cost + Kick.Cost))
                if (cast.Normal(GhostlyStrike))
                    return;

            // Hemohrrage
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > Hemorrhage.Cost + Kick.Cost))
                if (cast.Normal(Hemorrhage))
                    return;

            // Sinister Strike
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > SinisterStrike.Cost + Kick.Cost))
                if (cast.Normal(SinisterStrike))
                    return;
        }
    }
}
