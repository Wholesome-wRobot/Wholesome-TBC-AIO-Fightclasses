using System;
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

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected DruidSettings settings;

        protected Cast cast;

        protected bool _fightingACaster = false;
        protected List<string> _casterEnemies = new List<string>();
        protected int bigHealComboCost;
        protected int smallHealComboCost;
        protected bool _isStealthApproching;

        private Timer _moveBehindTimer = new Timer();
        protected Timer _combatMeleeTimer = new Timer();

        protected Druid specialization;

        public void Initialize(IClassRotation specialization)
        {
            RangeManager.SetRange(28);
            settings = DruidSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);
            cast = new Cast(Wrath, null, settings);

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
                    if (StatusChecker.OutOfCombat(RotationRole))
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
        }

        protected virtual void HealerCombat()
        {
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
        protected AIOSpell MoonfireRank1 = new AIOSpell("Moonfire", 1);

        protected bool MaulOn()
        {
            return Lua.LuaDoString<bool>("maulon = false; if IsCurrentSpell('Maul') then maulon = true end", "maulon");
        }

        protected void StealthApproach()
        {
            Timer stealthApproachTimer = new Timer(7000);
            _isStealthApproching = true;

            if (ObjectManager.Me.IsAlive && ObjectManager.Target.IsAlive)
            {
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && (ObjectManager.Target.GetDistance > 2.5f || !Claw.IsSpellUsable)
                    && (specialization.RotationType == Enums.RotationType.Party || ToolBox.GetClosestHostileFrom(ObjectManager.Target, 20f) == null)
                    && Fight.InFight
                    && !stealthApproachTimer.IsReady
                    && Me.HaveBuff("Prowl"))
                {
                    Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f);
                    MovementManager.MoveTo(position);
                    Thread.Sleep(50);
                    CastOpener();
                }

                if (stealthApproachTimer.IsReady
                    && ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { FaerieFireFeral, MoonfireRank1, Wrath }))
                {
                    _combatMeleeTimer = new Timer(2000);
                    return;
                }

                //ToolBox.CheckAutoAttack(Attack);

                _isStealthApproching = false;
            }
        }

        protected void CastOpener()
        {
            if (Me.Energy > 80
                && cast.OnTarget(Pounce))
                return;

            // Opener
            if (ToolBox.MeBehindTarget())
            {
                if (cast.OnTarget(Ravage))
                    return;
                if (cast.OnTarget(Shred))
                    return;
            }

            if (cast.OnTarget(Rake))
                return;
            if (cast.OnTarget(Claw))
                return;
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
                && _moveBehindTimer.IsReady)
            {
                if (ToolBox.StandBehindTargetCombat())
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