using System;
using System.Diagnostics;
using System.Threading;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Events;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.Collections.Generic;
using wManager.Wow.Bot.Tasks;
using System.ComponentModel;
using System.Linq;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;

namespace WholesomeTBCAIO.Rotations.Rogue
{
    public class Rogue : IClassRotation
    {
        public static RogueSettings settings;

        protected Cast cast;

        protected Stopwatch _pullMeleeTimer = new Stopwatch();
        protected Stopwatch _meleeTimer = new Stopwatch();
        protected Stopwatch _stealthApproachTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected List<string> _casterEnemies = new List<string>();

        protected readonly BackgroundWorker _pulseThread = new BackgroundWorker();

        protected float _pullRange = 25f;
        protected bool _fightingACaster = false;
        protected bool _pullFromAfar = false;
        protected bool _isStealthApproching;
        public static uint MHPoison;
        public static uint OHPoison;
        protected string _myBestBandage = null;

        protected Rogue specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = RogueSettings.Current;
            cast = new Cast(SinisterStrike, settings.ActivateCombatDebug, null);

            this.specialization = specialization as Rogue;
            TalentsManager.InitTalents(settings);

            RangeManager.SetRangeToMelee();
            AddPoisonsToNoSellList();

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            MovementEvents.OnMoveToPulse += MoveToPulseHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
            OthersEvents.OnAddBlackListGuid += BlackListHandler;

            Rotation();
        }

        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            MovementEvents.OnMoveToPulse -= MoveToPulseHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            OthersEvents.OnAddBlackListGuid -= BlackListHandler;
            cast.Dispose();
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();
                }
                catch (Exception arg)
                {
                    Logging.WriteError("ERROR: " + arg, true);
                }
                Thread.Sleep(ToolBox.GetLatency() + settings.ThreadSleepCycle);
            }
            Logger.Log("Stopped.");
        }

        protected virtual void BuffRotation()
        {
            if (!Me.IsMounted && !Me.IsCast)
            {
                PoisonWeapon();

                // Sprint
                if (settings.SprintWhenAvail && Me.HealthPercent > 80 && MovementManager.InMovement && Sprint.IsSpellUsable
                    && Sprint.KnownSpell)
                    Sprint.Launch();
            }
        }

        protected virtual void Pull()
        {
            // Check if surrounding enemies
            if (ObjectManager.Target.GetDistance < _pullRange && !_pullFromAfar)
                _pullFromAfar = ToolBox.CheckIfEnemiesAround(ObjectManager.Target, _pullRange);

            // Pull from afar
            if (_pullFromAfar && _pullMeleeTimer.ElapsedMilliseconds < 5000 || settings.AlwaysPull
                && ObjectManager.Target.GetDistance <= _pullRange)
            {
                Spell pullMethod = null;

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

        protected virtual void CombatRotation()
        {
            bool _shouldBeInterrupted = ToolBox.EnemyCasting();
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
                Me.Energy > ToolBox.GetSpellCost("Ghostly Strike") + ToolBox.GetSpellCost("Kick")))
                if (cast.Normal(GhostlyStrike))
                    return;

            // Hemohrrage
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > ToolBox.GetSpellCost("Hemorrhage") + ToolBox.GetSpellCost("Kick")))
                if (cast.Normal(Hemorrhage))
                    return;

            // Sinister Strike
            if (Me.ComboPoint < 5 && !IsTargetStunned() &&
                (!_fightingACaster || !Kick.KnownSpell ||
                Me.Energy > ToolBox.GetSpellCost("Sinister Strike") + ToolBox.GetSpellCost("Kick")))
                if (cast.Normal(SinisterStrike))
                    return;
        }

        protected Spell Attack = new Spell("Attack");
        protected Spell Shoot = new Spell("Shoot");
        protected Spell Throw = new Spell("Throw");
        protected Spell Eviscerate = new Spell("Eviscerate");
        protected Spell SinisterStrike = new Spell("Sinister Strike");
        protected Spell Stealth = new Spell("Stealth");
        protected Spell Backstab = new Spell("Backstab");
        protected Spell Gouge = new Spell("Gouge");
        protected Spell Evasion = new Spell("Evasion");
        protected Spell Kick = new Spell("Kick");
        protected Spell Garrote = new Spell("Garrote");
        protected Spell SliceAndDice = new Spell("Slice and Dice");
        protected Spell Vanish = new Spell("Vanish");
        protected Spell CheapShot = new Spell("Cheap Shot");
        protected Spell Riposte = new Spell("Riposte");
        protected Spell BladeFlurry = new Spell("Blade Flurry");
        protected Spell AdrenalineRush = new Spell("Adrenaline Rush");
        protected Spell Sprint = new Spell("Sprint");
        protected Spell CloakOfShadows = new Spell("Cloak of Shadows");
        protected Spell Blind = new Spell("Blind");
        protected Spell KidneyShot = new Spell("Kidney Shot");
        protected Spell Hemorrhage = new Spell("Hemorrhage");
        protected Spell GhostlyStrike = new Spell("Ghostly Strike");

        private void CastOpener()
        {
            // Opener
            if (ToolBox.MeBehindTarget())
            {
                if (settings.UseGarrote)
                    if (cast.Normal(Garrote))
                        return;
                if (cast.Normal(Backstab))
                    return;
                if (cast.Normal(CheapShot))
                    return;
                if (cast.Normal(Hemorrhage) || cast.Normal(SinisterStrike))
                    return;
            }
            else
            {
                if (CheapShot.KnownSpell)
                    if (cast.Normal(CheapShot))
                        return;
                else if (HaveDaggerInMH() && Gouge.KnownSpell)
                    if (cast.Normal(Gouge))
                        return;
                else
                    if (cast.Normal(Hemorrhage) || cast.Normal(SinisterStrike))
                        return;
            }
        }

        protected List<string> Bandages()
        {
            return new List<string>
            {
                "Linen Bandage",
                "Heavy Linen Bandage",
                "Wool Bandage",
                "Heavy Wool Bandage",
                "Silk Bandage",
                "Heavy Silk Bandage",
                "Mageweave Bandage",
                "Heavy Mageweave Bandage",
                "Runecloth Bandage",
                "Heavy Runecloth Bandage",
                "Netherweave Bandage",
                "Heavy Netherweave Bandage"
            };
        }

        protected void ToggleAutoAttack(bool activate)
        {
            bool _autoAttacking = Lua.LuaDoString<bool>("isAutoRepeat = false; if IsCurrentSpell('Attack') " +
                "then isAutoRepeat = true end", "isAutoRepeat");

            if (!_autoAttacking && activate && !ObjectManager.Target.HaveBuff("Gouge")
                && (!ObjectManager.Target.HaveBuff("Blind") || ToolBox.HasDebuff("Recently Bandaged")))
            {
                Logger.Log("Turning auto attack ON");
                ToolBox.CheckAutoAttack(Attack);
            }

            if (!activate && _autoAttacking)
            {
                Logger.Log("Turning auto attack OFF");
                Attack.Launch();
            }
        }

        protected bool IsTargetStunned()
        {
            return ObjectManager.Target.HaveBuff("Gouge") || ObjectManager.Target.HaveBuff("Cheap Shot");
        }

        protected bool HaveDaggerInMH()
        {
            return ToolBox.GetMHWeaponType().Equals("Daggers");
        }

        protected void PoisonWeapon()
        {
            bool hasMainHandEnchant = Lua.LuaDoString<bool>
                (@"local hasMainHandEnchant, _, _, _, _, _, _, _, _ = GetWeaponEnchantInfo()
            if (hasMainHandEnchant) then 
               return '1'
            else
               return '0'
            end");

            bool hasOffHandEnchant = Lua.LuaDoString<bool>
                (@"local _, _, _, _, hasOffHandEnchant, _, _, _, _ = GetWeaponEnchantInfo()
            if (hasOffHandEnchant) then 
               return '1'
            else
               return '0'
            end");

            bool hasoffHandWeapon = Lua.LuaDoString<bool>(@"local hasWeapon = OffhandHasWeapon()
            return hasWeapon");

            if (!hasMainHandEnchant)
            {
                IEnumerable<uint> DP = DeadlyPoisonDictionary
                    .Where(i => i.Key <= Me.Level 
                    && ItemsManager.HasItemById(i.Value))
                    .OrderByDescending(i => i.Key)
                    .Select(i => i.Value);

                IEnumerable<uint> IP = InstantPoisonDictionary
                    .Where(i => i.Key <= Me.Level 
                    && ItemsManager.HasItemById(i.Value))
                    .OrderByDescending(i => i.Key)
                    .Select(i => i.Value);

                if (DP.Any() || IP.Any())
                {
                    MovementManager.StopMoveTo(true, 1000);
                    MHPoison = DP.Any() ? DP.First() : IP.First();
                    ItemsManager.UseItem(MHPoison);
                    Thread.Sleep(10);
                    Lua.RunMacroText("/use 16");
                    Usefuls.WaitIsCasting();
                    return;
                }
            }
            if (!hasOffHandEnchant && hasoffHandWeapon)
            {

                IEnumerable<uint> IP = InstantPoisonDictionary
                    .Where(i => i.Key <= Me.Level 
                    && ItemsManager.HasItemById(i.Value))
                    .OrderByDescending(i => i.Key)
                    .Select(i => i.Value);

                if (IP.Any())
                {
                    MovementManager.StopMoveTo(true, 1000);
                    OHPoison = IP.First();
                    ItemsManager.UseItem(OHPoison);
                    Thread.Sleep(10);
                    Lua.RunMacroText("/use 17");
                    Usefuls.WaitIsCasting();
                    return;
                }
            }
        }

        protected Dictionary<int, uint> InstantPoisonDictionary = new Dictionary<int, uint>
        {
            { 20, 6947 },
            { 28, 6949 },
            { 36, 6950 },
            { 44, 8926 },
            { 52, 8927 },
            { 60, 8928 },
            { 68, 21927 },
            { 73, 43230 },
            { 79, 43231 },
        };

        protected Dictionary<int, uint> DeadlyPoisonDictionary = new Dictionary<int, uint>
        {
            { 30, 2892 },
            { 38, 2893 },
            { 46, 8984 },
            { 54, 8985 },
            { 60, 20844 },
            { 62, 22053 },
            { 70, 22054 },
            { 76, 43232 },
            { 80, 43233 },
        };

        private void AddPoisonsToNoSellList()
        {
            ToolBox.AddToDoNotSellList("Instant Poison");
            ToolBox.AddToDoNotSellList("Instant Poison II");
            ToolBox.AddToDoNotSellList("Instant Poison III");
            ToolBox.AddToDoNotSellList("Instant Poison IV");
            ToolBox.AddToDoNotSellList("Instant Poison V");
            ToolBox.AddToDoNotSellList("Instant Poison VI");
            ToolBox.AddToDoNotSellList("Instant Poison VII");

            ToolBox.AddToDoNotSellList("Deadly Poison");
            ToolBox.AddToDoNotSellList("Deadly Poison II");
            ToolBox.AddToDoNotSellList("Deadly Poison III");
            ToolBox.AddToDoNotSellList("Deadly Poison IV");
            ToolBox.AddToDoNotSellList("Deadly Poison V");
            ToolBox.AddToDoNotSellList("Deadly Poison VI");
            ToolBox.AddToDoNotSellList("Deadly Poison VII");
        }

        // EVENT HANDLERS
        private void BlackListHandler(ulong guid, int timeInMilisec, bool isSessionBlacklist, CancelEventArgs cancelable)
        {
            if (Me.HaveBuff("Stealth") && !_pullFromAfar)
            {
                Logger.LogDebug("BL : " + guid + " ms : " + timeInMilisec + " is session: " + isSessionBlacklist);
                Logger.Log("Cancelling Blacklist event");
                cancelable.Cancel = true;
            }
        }

        private void FightEndHandler(ulong guid)
        {
            _meleeTimer.Reset();
            _pullMeleeTimer.Reset();
            _stealthApproachTimer.Reset();
            _fightingACaster = false;
            _pullFromAfar = false;
            _isStealthApproching = false;
            _myBestBandage = null;
            RangeManager.SetRangeToMelee();
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            _myBestBandage = ToolBox.GetBestMatchingItem(Bandages());
            if (_myBestBandage != null)
                Logger.LogDebug("Found best bandage : " + _myBestBandage);
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (IsTargetStunned()
            && !MovementManager.InMovement
            && Me.IsAlive
            && !Me.IsCast
            && ObjectManager.Target.IsAlive)
            {
                Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f);
                MovementManager.Go(PathFinder.FindPath(position), false);

                while (MovementManager.InMovement
                && StatusChecker.BasicConditions()
                && IsTargetStunned())
                {
                    // Wait follow path
                    Thread.Sleep(200);
                }
            }
        }

        private void MoveToPulseHandler(Vector3 point, CancelEventArgs cancelable)
        {
            if (_isStealthApproching &&
            !point.ToString().Equals(ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f).ToString()))
                cancelable.Cancel = true;
        }
    }
}