using System;
using System.Diagnostics;
using System.Threading;
using robotManager.Helpful;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.Collections.Generic;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;
using System.ComponentModel;
using System.Linq;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class Druid : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        protected Stopwatch _pullMeleeTimer = new Stopwatch();
        protected Stopwatch _meleeTimer = new Stopwatch();
        protected Stopwatch _stealthApproachTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected DruidSettings settings;

        protected Cast cast;

        protected bool _fightingACaster = false;
        protected List<string> _casterEnemies = new List<string>();
        protected bool _pullFromAfar = false;
        protected int bigHealComboCost;
        protected int smallHealComboCost;
        protected float _pullRange = 27f;
        protected bool _isStealthApproching;
        protected List<WoWUnit> _partyEnemiesAround = new List<WoWUnit>();
        private Timer _moveBehindTimer = new Timer(500);

        protected Druid specialization;

        public void Initialize(IClassRotation specialization)
        {
            RangeManager.SetRange(_pullRange);
            settings = DruidSettings.Current;
            cast = new Cast(Wrath, settings.ActivateCombatDebug, null, settings.AutoDetectImmunities);

            this.specialization = specialization as Druid;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
            MovementEvents.OnMoveToPulse += MoveToPulseHandler;
            OthersEvents.OnAddBlackListGuid += BlackListHandler;

            Rotation();
        }

        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            MovementEvents.OnMoveToPulse -= MoveToPulseHandler;
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
                    if (RotationType == Enums.RotationType.Party)
                        _partyEnemiesAround = ToolBox.GetSuroundingEnemies();

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();

                    if (StatusChecker.InCombatNoTarget())
                        specialization.CombatNoTarget();

                    if (AIOParty.Group.Any(p => p.InCombatFlagOnly))
                        specialization.HealerCombat();
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
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
        }

        protected virtual void CombatNoTarget()
        {
            RegainAggro();
        }

        protected virtual void HealerCombat()
        {
        }

        protected void RegainAggro()
        {
            // Regain aggro
            if (settings.PartyTankSwitchTarget
                && specialization is FeralTankParty
                && (ObjectManager.Target.Target == ObjectManager.Me.Guid || !ObjectManager.Target.IsAlive || !ObjectManager.Target.HasTarget)
                && !ToolBox.HasDebuff("Growl", "target"))
            {
                foreach (WoWUnit enemy in _partyEnemiesAround)
                {
                    WoWPlayer partyMemberToSave = AIOParty.Group.Find(m => enemy.Target == m.Guid && m.Guid != ObjectManager.Me.Guid);
                    if (partyMemberToSave != null)
                    {
                        Logger.Log($"Regaining aggro [{enemy.Name} attacking {partyMemberToSave.Name}]");
                        ObjectManager.Me.Target = enemy.Guid;
                        break;
                    }
                }
            }
        }

        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell HealingTouch = new AIOSpell("Healing Touch");
        protected AIOSpell Wrath = new AIOSpell("Wrath");
        protected AIOSpell MarkOfTheWild = new AIOSpell("Mark of the Wild");
        protected AIOSpell Moonfire = new AIOSpell("Moonfire");
        protected AIOSpell Rejuvenation = new AIOSpell("Rejuvenation");
        protected AIOSpell Thorns = new AIOSpell("Thorns");
        protected AIOSpell BearForm = new AIOSpell("Bear Form");
        protected AIOSpell DireBearForm = new AIOSpell("Dire Bear Form");
        protected AIOSpell CatForm = new AIOSpell("Cat Form");
        protected AIOSpell TravelForm = new AIOSpell("Travel Form");
        protected AIOSpell Maul = new AIOSpell("Maul");
        protected AIOSpell DemoralizingRoar = new AIOSpell("Demoralizing Roar");
        protected AIOSpell Enrage = new AIOSpell("Enrage");
        protected AIOSpell Regrowth = new AIOSpell("Regrowth");
        protected AIOSpell Bash = new AIOSpell("Bash");
        protected AIOSpell Swipe = new AIOSpell("Swipe");
        protected AIOSpell FaerieFire = new AIOSpell("Faerie Fire");
        protected AIOSpell FaerieFireFeral = new AIOSpell("Faerie Fire (Feral)");
        protected AIOSpell Claw = new AIOSpell("Claw");
        protected AIOSpell Prowl = new AIOSpell("Prowl");
        protected AIOSpell Rip = new AIOSpell("Rip");
        protected AIOSpell Shred = new AIOSpell("Shred");
        protected AIOSpell RemoveCurse = new AIOSpell("Remove Curse");
        protected AIOSpell Rake = new AIOSpell("Rake");
        protected AIOSpell TigersFury = new AIOSpell("Tiger's Fury");
        protected AIOSpell AbolishPoison = new AIOSpell("Abolish Poison");
        protected AIOSpell Ravage = new AIOSpell("Ravage");
        protected AIOSpell FerociousBite = new AIOSpell("Ferocious Bite");
        protected AIOSpell Pounce = new AIOSpell("Pounce");
        protected AIOSpell FrenziedRegeneration = new AIOSpell("Frenzied Regeneration");
        protected AIOSpell Innervate = new AIOSpell("Innervate");
        protected AIOSpell Barkskin = new AIOSpell("Barkskin");
        protected AIOSpell MangleCat = new AIOSpell("Mangle (Cat)");
        protected AIOSpell MangleBear = new AIOSpell("Mangle (Bear)");
        protected AIOSpell Maim = new AIOSpell("Maim");
        protected AIOSpell OmenOfClarity = new AIOSpell("Omen of Clarity");
        protected AIOSpell AquaticForm = new AIOSpell("Aquatic Form");
        protected AIOSpell Lacerate = new AIOSpell("Lacerate");
        protected AIOSpell FeralCharge = new AIOSpell("Feral Charge");
        protected AIOSpell Growl = new AIOSpell("Growl");
        protected AIOSpell ChallengingRoar = new AIOSpell("Challenging Roar");
        protected AIOSpell Rebirth = new AIOSpell("Rebirth");
        protected AIOSpell TreeOfLife = new AIOSpell("Tree of Life");
        protected AIOSpell NaturesSwiftness = new AIOSpell("Nature\'s Swiftness");
        protected AIOSpell Lifebloom = new AIOSpell("Lifebloom");
        protected AIOSpell Tranquility = new AIOSpell("Tranquility");
        protected AIOSpell Swiftmend = new AIOSpell("Swiftmend");
        protected AIOSpell InsectSwarm = new AIOSpell("Insect Swarm");

        protected bool MaulOn()
        {
            return Lua.LuaDoString<bool>("maulon = false; if IsCurrentSpell('Maul') then maulon = true end", "maulon");
        }

        protected bool PullSpell()
        {
            RangeManager.SetRange(_pullRange);
            if ((Me.HaveBuff("Cat Form")
                || Me.HaveBuff("Bear Form")
                || Me.HaveBuff("Dire Bear Form"))
                && FaerieFireFeral.KnownSpell)
            {
                Logger.Log("Pulling with Faerie Fire (Feral)");
                Lua.RunMacroText("/cast Faerie Fire (Feral)()");
                Thread.Sleep(2000);
                return true;
            }
            else if (CatForm.KnownSpell
                && !Me.HaveBuff("Cat Form")
                && FaerieFireFeral.KnownSpell)
            {
                Logger.Log("Switching to cat form");
                cast.Normal(CatForm);
                return true;
            }
            else if (Moonfire.KnownSpell
                && !ObjectManager.Target.HaveBuff("Moonfire")
                && ObjectManager.Me.Level >= 10)
            {
                Logger.Log("Pulling with Moonfire (Rank 1)");
                Lua.RunMacroText("/cast Moonfire(Rank 1)");
                Usefuls.WaitIsCasting();
                return true;
            }
            else if (cast.Normal(Wrath))
                return true;

            return false;
        }

        protected void CastOpener()
        {
            if (Claw.IsDistanceGood)
            {
                if (Me.Energy > 80)
                    if (cast.Normal(Pounce))
                        return;

                // Opener
                if (ToolBox.MeBehindTarget())
                {
                    if (cast.Normal(Ravage))
                        return;
                    if (cast.Normal(Shred))
                        return;
                }

                if (cast.Normal(Rake))
                    return;
                if (cast.Normal(Claw))
                    return;
            }
        }

        // EVENT HANDLERS
        private void BlackListHandler(ulong guid, int timeInMilisec, bool isSessionBlacklist, CancelEventArgs cancelable)
        {
            Logger.LogDebug("BL : " + guid + " ms : " + timeInMilisec + " is session: " + isSessionBlacklist);
            if (Me.HaveBuff("Prowl"))
                cancelable.Cancel = true;
        }

        private void FightEndHandler(ulong guid)
        {
            _fightingACaster = false;
            _meleeTimer.Reset();
            _pullMeleeTimer.Reset();
            _stealthApproachTimer.Reset();
            _pullFromAfar = false;
            RangeManager.SetRange(_pullRange);
            _isStealthApproching = false;
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (Regrowth.KnownSpell)
            {
                AIOSpell bearFormSpell = DireBearForm.KnownSpell ? DireBearForm : BearForm;
                bigHealComboCost = Regrowth.Cost + Rejuvenation.Cost + bearFormSpell.Cost;
                smallHealComboCost = Regrowth.Cost + bearFormSpell.Cost;
            }
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (specialization is FeralDPSParty
                && settings.PartyStandBehind
                && Me.IsAlive
                && _moveBehindTimer.IsReady
                && !Me.IsCast
                && ObjectManager.Target.IsAlive
                && ObjectManager.Target.HasTarget
                && !ObjectManager.Target.IsTargetingMe
                && !MovementManager.InMovement)
            {
                int limit = 5;
                Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2f);
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && Me.Position.DistanceTo(position) > 1
                    && limit >= 0)
                {
                    position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    // Wait follow path
                    Thread.Sleep(500);
                    limit--;
                }
                _moveBehindTimer = new Timer(4000);
            }

            if (specialization is Feral
                && (ObjectManager.Target.HaveBuff("Pounce") || ObjectManager.Target.HaveBuff("Maim"))
                && !MovementManager.InMovement 
                && Me.IsAlive 
                && !Me.IsCast 
                && ObjectManager.Target.IsAlive)
            {
                Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f);
                MovementManager.Go(PathFinder.FindPath(position), false);

                while (MovementManager.InMovement 
                    && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && (ObjectManager.Target.HaveBuff("Pounce") || ObjectManager.Target.HaveBuff("Maim")))
                {
                    // Wait follow path
                    Thread.Sleep(500);
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