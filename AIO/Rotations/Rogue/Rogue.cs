using System;
using System.Threading;
using robotManager.Helpful;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Rogue
{
    public class Rogue : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static RogueSettings settings;

        protected Cast cast;
        protected Rogue specialization;

        protected List<string> _casterEnemies = new List<string>();

        protected readonly BackgroundWorker _pulseThread = new BackgroundWorker();

        protected bool _fightingACaster = false;
        protected bool _isStealthApproching;
        public static uint MHPoison;
        public static uint OHPoison;
        protected string _myBestBandage = null;
        protected WoWLocalPlayer Me = ObjectManager.Me;

        private Timer _moveBehindTimer = new Timer();
        protected Timer _combatMeleeTimer = new Timer();
        protected Timer _behindTargetTimer = new Timer();

        public void Initialize(IClassRotation specialization)
        {
            settings = RogueSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);
            cast = new Cast(SinisterStrike, null, settings);

            this.specialization = specialization as Rogue;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            RangeManager.SetRangeToMelee();
            AddPoisonsToNoSellList();

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            MovementEvents.OnMoveToPulse += MoveToPulseHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
            OthersEvents.OnAddBlackListGuid += BlackListHandler;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsWithArgsHandler;

            Rotation();
        }

        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            MovementEvents.OnMoveToPulse -= MoveToPulseHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            OthersEvents.OnAddBlackListGuid -= BlackListHandler;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsWithArgsHandler;
            cast.Dispose();
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.OutOfCombat(RotationRole))
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
            PoisonWeapon();

            // Sprint
            if (settings.SprintWhenAvail
                && Me.HealthPercent >
                80 && MovementManager.InMovement
                && cast.OnSelf(Sprint))
                return;
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
        }

        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell Shoot = new AIOSpell("Shoot");
        protected AIOSpell Throw = new AIOSpell("Throw");
        protected AIOSpell Eviscerate = new AIOSpell("Eviscerate");
        protected AIOSpell SinisterStrike = new AIOSpell("Sinister Strike");
        protected AIOSpell Stealth = new AIOSpell("Stealth");
        protected AIOSpell Backstab = new AIOSpell("Backstab");
        protected AIOSpell Gouge = new AIOSpell("Gouge");
        protected AIOSpell Evasion = new AIOSpell("Evasion");
        protected AIOSpell Kick = new AIOSpell("Kick");
        protected AIOSpell Garrote = new AIOSpell("Garrote");
        protected AIOSpell SliceAndDice = new AIOSpell("Slice and Dice");
        protected AIOSpell Vanish = new AIOSpell("Vanish");
        protected AIOSpell CheapShot = new AIOSpell("Cheap Shot");
        protected AIOSpell Riposte = new AIOSpell("Riposte");
        protected AIOSpell BladeFlurry = new AIOSpell("Blade Flurry");
        protected AIOSpell AdrenalineRush = new AIOSpell("Adrenaline Rush");
        protected AIOSpell Sprint = new AIOSpell("Sprint");
        protected AIOSpell CloakOfShadows = new AIOSpell("Cloak of Shadows");
        protected AIOSpell Blind = new AIOSpell("Blind");
        protected AIOSpell KidneyShot = new AIOSpell("Kidney Shot");
        protected AIOSpell Hemorrhage = new AIOSpell("Hemorrhage");
        protected AIOSpell GhostlyStrike = new AIOSpell("Ghostly Strike");
        protected AIOSpell Rupture = new AIOSpell("Rupture");
        protected AIOSpell Shiv = new AIOSpell("Shiv");

        protected bool BehindTargetCheck => _behindTargetTimer.IsReady && ToolBox.MeBehindTarget();

        protected void CastOpener()
        {
            if (ToolBox.MeBehindTarget() && BehindTargetCheck)
            {
                if (settings.UseGarrote
                    && cast.OnTarget(Garrote))
                    return;
                if (cast.OnTarget(Backstab))
                    return;
                if (cast.OnTarget(CheapShot))
                    return;
                if (cast.OnTarget(Hemorrhage) || cast.OnTarget(SinisterStrike))
                    return;
            }
            else
            {
                if (cast.OnTarget(CheapShot))
                    return;
                if (HaveDaggerInMH() 
                    && cast.OnTarget(Gouge))
                    return;
                if (cast.OnTarget(Hemorrhage) || cast.OnTarget(SinisterStrike))
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

            if (!activate 
                && _autoAttacking
                && cast.OnTarget(Attack))
            {
                Logger.Log("Turning auto attack OFF");
                return;
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

        protected void StealthApproach()
        {
            RangeManager.SetRangeToMelee();
            Timer stealthApproachTimer = new Timer(15000);
            _isStealthApproching = true;

            if (ObjectManager.Me.IsAlive && ObjectManager.Target.IsAlive)
            {
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                && ObjectManager.Target.GetDistance > 2.5f
                && (specialization.RotationType == Enums.RotationType.Party || ToolBox.GetClosestHostileFrom(ObjectManager.Target, 20f) == null)
                && Fight.InFight
                && !stealthApproachTimer.IsReady
                && Me.HaveBuff("Stealth"))
                {
                    ToggleAutoAttack(false);

                    Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f);
                    MovementManager.MoveTo(position);
                    Thread.Sleep(50);
                    CastOpener();
                }

                if (stealthApproachTimer.IsReady
                    && ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { Shoot, Throw }))
                {
                    _combatMeleeTimer = new Timer(2000);
                    return;
                }

                //ToolBox.CheckAutoAttack(Attack);

                _isStealthApproching = false;
            }
        }

        // EVENT HANDLERS
        private void BlackListHandler(ulong guid, int timeInMilisec, bool isSessionBlacklist, CancelEventArgs cancelable)
        {
            if (Me.HaveBuff("Stealth"))
            {
                Logger.LogDebug("BL : " + guid + " ms : " + timeInMilisec + " is session: " + isSessionBlacklist);
                Logger.Log("Cancelling Blacklist event");
                cancelable.Cancel = true;
            }
        }

        private void FightEndHandler(ulong guid)
        {
            _fightingACaster = false;
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
            if (specialization.RotationType == Enums.RotationType.Party
                && _moveBehindTimer.IsReady)
            {
                if (ToolBox.StandBehindTargetCombat())
                    _moveBehindTimer = new Timer(4000);
            }
            else
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
        }

        private void MoveToPulseHandler(Vector3 point, CancelEventArgs cancelable)
        {
            if (_isStealthApproching &&
            !point.ToString().Equals(ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f).ToString()))
                cancelable.Cancel = true;
        }

        private void EventsWithArgsHandler(string id, List<string> args)
        {
            if (_behindTargetTimer.IsReady && args[11].Contains("You must be behind"))
                _behindTargetTimer = new Timer(10000);
        }
    }
}