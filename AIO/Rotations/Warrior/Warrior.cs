using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class Warrior : BaseRotation
    {
        protected WarriorSettings settings;
        protected bool fightingACaster = false;
        protected List<string> casterEnemies = new List<string>();
        protected bool pullFromAfar = false;
        protected Timer combatMeleeTimer = new Timer();
        protected Warrior specialization;
        private Timer _moveBehindTimer = new Timer();
        private bool _useCommandingShout;

        public Warrior(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Warrior;
            settings = WarriorSettings.Current;
            BaseInit(RangeManager.DefaultMeleeRange, BattleShout, null, settings);

            _useCommandingShout = specialization is ProtectionWarrior && settings.PPR_UseCommandingShout
                || specialization is Fury && settings.SFU_UseCommandingShout
                || specialization is FuryParty && settings.PFU_UseCommandingShout;

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            cast.OnTarget(BattleStance);

            Rotation();
        }

        public override void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;

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
                }
                catch (Exception arg)
                {
                    Logging.WriteError("ERROR: " + arg, true);
                }
                Thread.Sleep(ToolBox.GetLatency() + settings.ThreadSleepCycle);
            }
            Logger.Log("Stopped.");
        }

        protected override void BuffRotation()
        {
            if (!Me.IsMounted && !Me.IsCast)
            {
                // Battle Shout
                if (!Me.HasAura(BattleShout)
                    && (!_useCommandingShout || !CommandingShout.KnownSpell)
                    && cast.OnSelf(BattleShout))
                    return;

                // Commanding Shout
                if (!Me.HasAura(CommandingShout)
                    && _useCommandingShout
                    && cast.OnSelf(CommandingShout))
                    return;
            }
        }

        protected override void Pull() { }
        protected override void CombatRotation() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (specialization is FuryParty
                && settings.PFU_PartyStandBehind
                && _moveBehindTimer.IsReady)
            {
                if (ToolBox.StandBehindTargetCombat(unitCache))
                    _moveBehindTimer = new Timer(4000);
            }
        }

        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell HeroicStrike = new AIOSpell("Heroic Strike");
        protected AIOSpell BattleShout = new AIOSpell("Battle Shout");
        protected AIOSpell CommandingShout = new AIOSpell("Commanding Shout");
        protected AIOSpell Charge = new AIOSpell("Charge");
        protected AIOSpell Rend = new AIOSpell("Rend");
        protected AIOSpell Hamstring = new AIOSpell("Hamstring");
        protected AIOSpell BloodRage = new AIOSpell("Bloodrage");
        protected AIOSpell Overpower = new AIOSpell("Overpower");
        protected AIOSpell DemoralizingShout = new AIOSpell("Demoralizing Shout");
        protected AIOSpell Throw = new AIOSpell("Throw");
        protected AIOSpell Shoot = new AIOSpell("Shoot");
        protected AIOSpell Retaliation = new AIOSpell("Retaliation");
        protected AIOSpell Cleave = new AIOSpell("Cleave");
        protected AIOSpell Execute = new AIOSpell("Execute");
        protected AIOSpell SweepingStrikes = new AIOSpell("Sweeping Strikes");
        protected AIOSpell Bloodthirst = new AIOSpell("Bloodthirst");
        protected AIOSpell BerserkerStance = new AIOSpell("Berserker Stance");
        protected AIOSpell BattleStance = new AIOSpell("Battle Stance");
        protected AIOSpell DefensiveStance = new AIOSpell("Defensive Stance");
        protected AIOSpell Intercept = new AIOSpell("Intercept");
        protected AIOSpell Pummel = new AIOSpell("Pummel");
        protected AIOSpell BerserkerRage = new AIOSpell("Berserker Rage");
        protected AIOSpell Rampage = new AIOSpell("Rampage");
        protected AIOSpell VictoryRush = new AIOSpell("Victory Rush");
        protected AIOSpell Whirlwind = new AIOSpell("Whirlwind");
        protected AIOSpell ThunderClap = new AIOSpell("Thunder Clap");
        protected AIOSpell ShieldSlam = new AIOSpell("Shield Slam");
        protected AIOSpell Revenge = new AIOSpell("Revenge");
        protected AIOSpell Devastate = new AIOSpell("Devastate");
        protected AIOSpell ShieldBlock = new AIOSpell("Shield Block");
        protected AIOSpell Taunt = new AIOSpell("Taunt");
        protected AIOSpell SpellReflection = new AIOSpell("Spell Reflection");
        protected AIOSpell ShieldBash = new AIOSpell("Shield Bash");
        protected AIOSpell LastStand = new AIOSpell("Last Stand");
        protected AIOSpell Intervene = new AIOSpell("Intervene");
        protected AIOSpell SunderArmor = new AIOSpell("Sunder Armor");

        protected bool InBattleStance()
        {
            return Lua.LuaDoString<bool>("return GetShapeshiftForm() == 1;");
        }

        protected bool InBerserkStance()
        {
            return Lua.LuaDoString<bool>("return (GetShapeshiftForm() == 3 or GetShapeshiftForm() == 2);");
        }

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            fightingACaster = false;
            pullFromAfar = false;
        }
    }
}