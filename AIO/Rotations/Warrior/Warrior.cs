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

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class Warrior : IClassRotation
    {
        public static WarriorSettings settings;

        protected Cast cast;

        protected Stopwatch _pullMeleeTimer = new Stopwatch();
        protected Stopwatch _meleeTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected float _pullRange = 25f;
        protected bool _fightingACaster = false;
        protected List<string> _casterEnemies = new List<string>();
        protected bool _pullFromAfar = false;

        protected Warrior specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = WarriorSettings.Current;
            cast = new Cast(BattleShout, settings.ActivateCombatDebug, null);

            this.specialization = specialization as Warrior;
            TalentsManager.InitTalents(settings);

            FightEvents.OnFightEnd += FightEndHandler;

            Rotation();
        }

        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
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
        protected Spell Intercept = new Spell("Intercept");
        protected Spell Pummel = new Spell("Pummel");
        protected Spell BerserkerRage = new Spell("Berserker Rage");
        protected Spell Rampage = new Spell("Rampage");
        protected Spell VictoryRush = new Spell("Victory Rush");
        protected Spell Whirlwind = new Spell("Whirlwind");

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