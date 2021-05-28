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
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class Warrior : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static WarriorSettings settings;

        protected Cast cast;
        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected bool _fightingACaster = false;
        protected List<string> _casterEnemies = new List<string>();
        protected bool _pullFromAfar = false;

        private Timer _moveBehindTimer = new Timer();
        protected Timer _combatMeleeTimer = new Timer();

        protected Warrior specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = WarriorSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);
            cast = new Cast(BattleShout, null, settings);

            this.specialization = specialization as Warrior;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            cast.OnTarget(BattleStance);

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

        protected virtual void BuffRotation()
        {
            if (!Me.IsMounted && !Me.IsCast)
            {
                // Battle Shout
                if (!Me.HaveBuff("Battle Shout")
                    && (!settings.UseCommandingShout || !CommandingShout.KnownSpell)
                    && cast.OnSelf(BattleShout))
                    return;

                // Commanding Shout
                if (!Me.HaveBuff("Commanding Shout")
                    && settings.UseCommandingShout
                    && cast.OnSelf(CommandingShout))
                    return;
            }
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

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (specialization is FuryParty
                && settings.PartyStandBehind
                && _moveBehindTimer.IsReady)
            {
                if (ToolBox.StandBehindTargetCombat())
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
            _pullFromAfar = false;
        }
    }
}