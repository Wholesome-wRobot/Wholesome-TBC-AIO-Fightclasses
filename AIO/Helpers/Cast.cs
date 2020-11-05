using System.Collections.Generic;
using System.Threading;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public class Cast
    {
        private Spell DefaultBaseSpell { get; }
        private bool CombatDebugON { get; }
        private Spell WandSpell { get; }
        private ulong EnemyGuid { get; set; }

        public bool IsWanding { get; set; }
        public bool IsBackingUp { get; set; }
        public bool PlayingManaClass { get; set; }
        public List<string> BannedSpells { get; set; }

        public Cast(Spell defaultBaseSpell, bool combatDebugON, Spell wandSpell)
        {
            DefaultBaseSpell = defaultBaseSpell;
            CombatDebugON = combatDebugON;
            WandSpell = wandSpell;
            PlayingManaClass = ObjectManager.Me.WowClass != WoWClass.Rogue && ObjectManager.Me.WowClass != WoWClass.Warrior;
            BannedSpells = new List<string>();
            EventsLuaWithArgs.OnEventsLuaWithArgs += SpellsHandler;
        }

        public bool Normal(Spell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast);
        }

        public bool OnSelf(Spell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, true);
        }

        public bool AdvancedCast(Spell s, bool stopWandAndCast = true, bool onSelf = false)
        {
            if (ObjectManager.Target.Guid != EnemyGuid)
            {
                BannedSpells.Clear();
                EnemyGuid = ObjectManager.Target.Guid;
            }

            if (!s.KnownSpell 
                || IsBackingUp 
                || ObjectManager.Me.IsCast 
                || ObjectManager.Me.CastingTimeLeft > Usefuls.Latency
                || ObjectManager.Me.IsStunned)
                return false;

            if (BannedSpells.Count > 0 && BannedSpells.Contains(s.Name))
                return false;

            CombatDebug("*----------- INTO CAST FOR " + s.Name);

            if (PlayingManaClass 
                && ToolBox.GetSpellCost(s.Name) > ObjectManager.Me.Mana)
            {
                CombatDebug(s.Name + ": Not enough mana, SKIPPING");
                return false;
            }

            if (WandSpell != null 
                && ToolBox.UsingWand() 
                && !stopWandAndCast)
            {
                CombatDebug("Didn't cast because we were wanding");
                return false;
            }

            float _spellCD = ToolBox.GetSpellCooldown(s.Name);
            CombatDebug("Cooldown is " + _spellCD);

            if (_spellCD >= 2f)
            {
                CombatDebug("Didn't cast because cd is too long");
                return false;
            }

            if (WandSpell != null 
                && ToolBox.UsingWand() 
                && stopWandAndCast)
                ToolBox.StopWandWaitGCD(WandSpell, DefaultBaseSpell);

            if (_spellCD < 2f && _spellCD > 0f)
            {
                if (ToolBox.GetSpellCastTime(s.Name) < 1f)
                {
                    CombatDebug(s.Name + " is instant and low CD, recycle");
                    return true;
                }

                int t = 0;
                while (ToolBox.GetSpellCooldown(s.Name) > 0)
                {
                    Thread.Sleep(50);
                    t += 50;
                    if (t > 2000)
                    {
                        CombatDebug(s.Name + ": waited for tool long, give up");
                        return false;
                    }
                }
                Thread.Sleep(100 + Usefuls.Latency);
                CombatDebug(s.Name + ": waited " + (t + 100) + " for it to be ready");
            }

            if (!s.IsSpellUsable)
            {
                CombatDebug("Didn't cast because spell somehow not usable");
                return false;
            }

            if (onSelf && !ObjectManager.Target.IsAttackable)
                Lua.RunMacroText("/cleartarget");

            bool stopMove = s.CastTime > 0;

            s.Launch(stopMove, true, true);

            return true;
        }

        private void CombatDebug(string s)
        {
            if (CombatDebugON)
                Logger.CombatDebug(s);
        }

        private void SpellsHandler(LuaEventsId id, List<string> args)
        {
            if (args[11] == "IMMUNE" && !BannedSpells.Contains(args[9]))
            {
                Logger.Log($"{ObjectManager.Target.Name} is immune to {args[9]}, banning spell for this fight");
                BannedSpells.Add(args[9]);
            }
        }
    }
}
