using System;
using System.Diagnostics;
using System.Threading;
using robotManager.Helpful;
using wManager.Events;
using wManager.Wow.Class;
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

        protected Spell Attack = new Spell("Attack");
        protected Spell HealingTouch = new Spell("Healing Touch");
        protected Spell Wrath = new Spell("Wrath");
        protected Spell MarkOfTheWild = new Spell("Mark of the Wild");
        protected Spell Moonfire = new Spell("Moonfire");
        protected Spell Rejuvenation = new Spell("Rejuvenation");
        protected Spell Thorns = new Spell("Thorns");
        protected Spell BearForm = new Spell("Bear Form");
        protected Spell DireBearForm = new Spell("Dire Bear Form");
        protected Spell CatForm = new Spell("Cat Form");
        protected Spell TravelForm = new Spell("Travel Form");
        protected Spell Maul = new Spell("Maul");
        protected Spell DemoralizingRoar = new Spell("Demoralizing Roar");
        protected Spell Enrage = new Spell("Enrage");
        protected Spell Regrowth = new Spell("Regrowth");
        protected Spell Bash = new Spell("Bash");
        protected Spell Swipe = new Spell("Swipe");
        protected Spell FaerieFire = new Spell("Faerie Fire");
        protected Spell FaerieFireFeral = new Spell("Faerie Fire (Feral)");
        protected Spell Claw = new Spell("Claw");
        protected Spell Prowl = new Spell("Prowl");
        protected Spell Rip = new Spell("Rip");
        protected Spell Shred = new Spell("Shred");
        protected Spell RemoveCurse = new Spell("Remove Curse");
        protected Spell Rake = new Spell("Rake");
        protected Spell TigersFury = new Spell("Tiger's Fury");
        protected Spell AbolishPoison = new Spell("Abolish Poison");
        protected Spell Ravage = new Spell("Ravage");
        protected Spell FerociousBite = new Spell("Ferocious Bite");
        protected Spell Pounce = new Spell("Pounce");
        protected Spell FrenziedRegeneration = new Spell("Frenzied Regeneration");
        protected Spell Innervate = new Spell("Innervate");
        protected Spell Barkskin = new Spell("Barkskin");
        protected Spell MangleCat = new Spell("Mangle (Cat)");
        protected Spell MangleBear = new Spell("Mangle (Bear)");
        protected Spell Maim = new Spell("Maim");
        protected Spell OmenOfClarity = new Spell("Omen of Clarity");
        protected Spell AquaticForm = new Spell("Aquatic Form");
        protected Spell Lacerate = new Spell("Lacerate");
        protected Spell FeralCharge = new Spell("Feral Charge");
        protected Spell Growl = new Spell("Growl");
        protected Spell ChallengingRoar = new Spell("Challenging Roar");
        protected Spell Rebirth = new Spell("Rebirth");
        protected Spell TreeOfLife = new Spell("Tree of Life");
        protected Spell NaturesSwiftness = new Spell("Nature\'s Swiftness");
        protected Spell Lifebloom = new Spell("Lifebloom");
        protected Spell Tranquility = new Spell("Tranquility");
        protected Spell Swiftmend = new Spell("Swiftmend");
        protected Spell InsectSwarm = new Spell("Insect Swarm");

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
                string bearFormSpell = DireBearForm.KnownSpell ? "Dire Bear Form" : "Bear Form";
                bigHealComboCost = ToolBox.GetSpellCost("Regrowth") + ToolBox.GetSpellCost("Rejuvenation") + ToolBox.GetSpellCost(bearFormSpell);
                smallHealComboCost = ToolBox.GetSpellCost("Regrowth") + ToolBox.GetSpellCost(bearFormSpell);
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