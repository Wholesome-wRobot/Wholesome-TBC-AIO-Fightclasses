using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class Druid : BaseRotation
    {
        protected DruidSettings settings;
        protected Druid specialization;
        protected bool fightingACaster = false;
        protected List<string> casterEnemies = new List<string>();
        protected int bigHealComboCost;
        protected int smallHealComboCost;
        protected bool isStealthApproching;
        protected Timer combatMeleeTimer = new Timer();
        private Timer _moveBehindTimer = new Timer();

        public Druid(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Druid;
            settings = DruidSettings.Current;
            BaseInit(28, Wrath, null, settings);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
            MovementEvents.OnMoveToPulse += MoveToPulseHandler;
            OthersEvents.OnAddBlackListGuid += BlackListHandler;

            Rotation();
        }

        public override void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            MovementEvents.OnMoveToPulse -= MoveToPulseHandler;
            OthersEvents.OnAddBlackListGuid -= BlackListHandler;

            BaseDispose();
        }

        private void Rotation()
        {
            while (Main.IsLaunched)
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

                    if (unitCache.GroupAndRaid.Any(p => p.InCombatFlagOnly))
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

        protected override void BuffRotation() { }
        protected override void Pull() { }
        protected override void CombatRotation() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

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

        protected void StealthApproach()
        {
            Timer stealthApproachTimer = new Timer(7000);
            isStealthApproching = true;

            if (Me.IsAlive && Target.IsAlive)
            {
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    //&& (Target.GetDistance > 3f || !Claw.IsSpellUsable)
                    && (specialization.RotationType == Enums.RotationType.Party || unitCache.GetClosestHostileFrom(Target, 20f) == null)
                    && Fight.InFight
                    && !stealthApproachTimer.IsReady
                    && !TraceLine.TraceLineGo(Me.PositionWithoutType, Target.PositionWithoutType, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS)
                    && Me.HasAura(Prowl))
                {
                    Vector3 position;
                    position = WTSpace.BackOfUnit(Target.WowUnit, 2.5f);
                    MovementManager.MoveTo(position);

                    Thread.Sleep(100);
                    CastOpener();
                }

                if (TraceLine.TraceLineGo(Me.PositionWithoutType, Target.PositionWithoutType, CGWorldFrameHitFlags.HitTestSpellLoS | CGWorldFrameHitFlags.HitTestLOS))
                {
                    cast.OnSelf(Prowl);
                    isStealthApproching = false;
                    return;
                }

                CastOpener();

                if (stealthApproachTimer.IsReady
                    && ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { FaerieFireFeral, MoonfireRank1, Wrath }, unitCache))
                {
                    combatMeleeTimer = new Timer(2000);
                    isStealthApproching = false;
                    return;
                }

                ToolBox.CheckAutoAttack(Attack);
            }

            isStealthApproching = false;
        }

        protected void CastOpener()
        {
            cast.OnTarget(Pounce);
            cast.OnTarget(Ravage);
            cast.OnTarget(Shred);
            cast.OnTarget(Rake);
            cast.OnTarget(Claw);
            /*
            if (Me.Energy > 80
                && cast.OnTarget(Pounce))
                return;

            // Opener
            if (ToolBox.MeBehindTarget())
            {
                cast.OnTarget(Ravage);
                cast.OnTarget(Shred);
                cast.OnTarget(Rake);
                cast.OnTarget(Claw);
            }
            else
            {
                cast.OnTarget(Rake);
                cast.OnTarget(Claw);
            }*/
        }

        // EVENT HANDLERS
        private void BlackListHandler(ulong guid, int timeInMilisec, bool isSessionBlacklist, CancelEventArgs cancelable)
        {
            Logger.LogDebug("BL : " + guid + " ms : " + timeInMilisec + " is session: " + isSessionBlacklist);
            if (Me.HasAura(Prowl))
                cancelable.Cancel = true;
        }

        private void FightEndHandler(ulong guid)
        {
            fightingACaster = false;
            isStealthApproching = false;
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
                if (ToolBox.StandBehindTargetCombat(unitCache))
                    _moveBehindTimer = new Timer(4000);
            }

            if (specialization is Feral
                && (Target.HasAura(Pounce) || Target.HasAura(Maim))
                && !MovementManager.InMovement
                && Me.IsAlive
                && !Me.IsCast
                && Target.IsAlive)
            {
                Vector3 position = WTSpace.BackOfUnit(Target.WowUnit, 2.5f);
                MovementManager.Go(PathFinder.FindPath(position), false);

                while (MovementManager.InMovement
                    && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && (Target.HasAura(Pounce) || Target.HasAura(Maim)))
                {
                    // Wait follow path
                    Thread.Sleep(500);
                }
            }
        }

        private void MoveToPulseHandler(Vector3 point, CancelEventArgs cancelable)
        {
            if (isStealthApproching &&
            !point.ToString().Equals(WTSpace.BackOfUnit(Target.WowUnit, 2.5f).ToString()))
                cancelable.Cancel = true;
        }
    }
}