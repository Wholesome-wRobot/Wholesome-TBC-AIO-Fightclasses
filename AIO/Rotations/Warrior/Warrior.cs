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
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class Warrior : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static WarriorSettings settings;

        protected Cast cast;

        protected Stopwatch _pullMeleeTimer = new Stopwatch();
        protected Stopwatch _meleeTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected float _pullRange = 25f;
        protected bool _fightingACaster = false;
        protected List<string> _casterEnemies = new List<string>();
        protected bool _pullFromAfar = false;
        protected List<WoWUnit> _partyEnemiesAround = new List<WoWUnit>();
        private Timer _moveBehindTimer = new Timer(500);

        protected Warrior specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = WarriorSettings.Current;
            cast = new Cast(BattleShout, settings.ActivateCombatDebug, null, settings.AutoDetectImmunities);

            this.specialization = specialization as Warrior;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            cast.Normal(BattleStance);

            Rotation();
        }

        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
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
                // Battle Shout
                if (!Me.HaveBuff("Battle Shout")
                    && BattleShout.IsSpellUsable &&
                    (!settings.UseCommandingShout || !CommandingShout.KnownSpell))
                    if (cast.Normal(BattleShout))
                        return;

                // Commanding Shout
                if (!Me.HaveBuff("Commanding Shout")
                    && settings.UseCommandingShout
                    && CommandingShout.KnownSpell)
                    if (cast.Normal(CommandingShout))
                        return;
            }
        }

        protected virtual void Pull()
        {
            RangeManager.SetRangeToMelee();
        }

        protected virtual void CombatRotation()
        {
        }

        protected virtual void CombatNoTarget()
        {
            RegainAggro();
        }

        protected void RegainAggro()
        {
            // Regain aggro
            if (settings.PartyTankSwitchTarget
                && specialization is ProtectionWarrior
                && (ObjectManager.Target.Target == ObjectManager.Me.Guid || !ObjectManager.Target.IsAlive || ObjectManager.Target.Target <= 0)
                && !ToolBox.HasDebuff("Taunt", "target"))
            {
                foreach (WoWUnit enemy in _partyEnemiesAround)
                {
                    WoWPlayer partyMemberToSave = AIOParty.Group.Find(m => enemy.Target == m.Guid && m.Guid != ObjectManager.Me.Guid);
                    if (partyMemberToSave != null)
                    {
                        Logger.Log($"Regaining aggro [{enemy.Name} attacking {partyMemberToSave.Name}]");
                        ObjectManager.Me.Target = enemy.Guid;
                        if (settings.PartyUseIntervene && enemy.Position.DistanceTo(partyMemberToSave.Position) < 10)
                            cast.Normal(Intervene);
                        break;
                    }
                }
            }
        }
        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (specialization is FuryParty
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
        }

        protected Spell Attack = new Spell("Attack");
        protected Spell HeroicStrike = new Spell("Heroic Strike");
        protected Spell BattleShout = new Spell("Battle Shout");
        protected Spell CommandingShout = new Spell("Commanding Shout");
        protected Spell Charge = new Spell("Charge");
        protected Spell Rend = new Spell("Rend");
        protected Spell Hamstring = new Spell("Hamstring");
        protected Spell BloodRage = new Spell("Bloodrage");
        protected Spell Overpower = new Spell("Overpower");
        protected Spell DemoralizingShout = new Spell("Demoralizing Shout");
        protected Spell Throw = new Spell("Throw");
        protected Spell Shoot = new Spell("Shoot");
        protected Spell Retaliation = new Spell("Retaliation");
        protected Spell Cleave = new Spell("Cleave");
        protected Spell Execute = new Spell("Execute");
        protected Spell SweepingStrikes = new Spell("Sweeping Strikes");
        protected Spell Bloodthirst = new Spell("Bloodthirst");
        protected Spell BerserkerStance = new Spell("Berserker Stance");
        protected Spell BattleStance = new Spell("Battle Stance");
        protected Spell DefensiveStance = new Spell("Defensive Stance");
        protected Spell Intercept = new Spell("Intercept");
        protected Spell Pummel = new Spell("Pummel");
        protected Spell BerserkerRage = new Spell("Berserker Rage");
        protected Spell Rampage = new Spell("Rampage");
        protected Spell VictoryRush = new Spell("Victory Rush");
        protected Spell Whirlwind = new Spell("Whirlwind");
        protected Spell ThunderClap = new Spell("Thunder Clap");
        protected Spell ShieldSlam = new Spell("Shield Slam");
        protected Spell Revenge = new Spell("Revenge");
        protected Spell Devastate = new Spell("Devastate");
        protected Spell ShieldBlock = new Spell("Shield Block");
        protected Spell Taunt = new Spell("Taunt");
        protected Spell SpellReflection = new Spell("Spell Reflection");
        protected Spell ShieldBash = new Spell("Shield Bash");
        protected Spell LastStand = new Spell("Last Stand");
        protected Spell Intervene = new Spell("Intervene");
        protected Spell SunderArmor = new Spell("Sunder Armor");

        protected bool HeroicStrikeOn()
        {
            return Lua.LuaDoString<bool>("hson = false; if IsCurrentSpell('Heroic Strike') then hson = true end", "hson");
        }

        protected bool InBattleStance()
        {
            return Lua.LuaDoString<bool>("bs = false; if GetShapeshiftForm() == 1 then bs = true end", "bs");
        }

        protected bool InBerserkStance()
        {
            return Lua.LuaDoString<bool>("bs = false; if GetShapeshiftForm() == 3 or GetShapeshiftForm() == 2 then bs = true end", "bs");
        }

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _fightingACaster = false;
            _meleeTimer.Reset();
            _pullMeleeTimer.Reset();
            _pullFromAfar = false;
            RangeManager.SetRange(RangeManager.DefaultMeleeRange);
        }
    }
}