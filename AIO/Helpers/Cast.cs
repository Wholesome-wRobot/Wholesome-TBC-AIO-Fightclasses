using robotManager.Helpful;
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
        private bool AutoDetectImmunities { get; }
        private ulong CurrentEnemyGuid { get; set; }

        public bool IsBackingUp { get; set; }
        public bool PlayingManaClass { get; set; }
        public List<string> BannedSpells { get; set; }

        public Cast(Spell defaultBaseSpell, bool combatDebugON, Spell wandSpell, bool autoDetectImmunities)
        {
            AutoDetectImmunities = autoDetectImmunities;
            DefaultBaseSpell = defaultBaseSpell;
            CombatDebugON = combatDebugON;
            WandSpell = wandSpell;
            PlayingManaClass = ObjectManager.Me.WowClass != WoWClass.Rogue && ObjectManager.Me.WowClass != WoWClass.Warrior;
            BannedSpells = new List<string>();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += LuaEventsHandler;
        }

        public bool PetSpell(string spellName, bool onFocus = false, bool noTargetNeeded = false)
        {
            int spellIndex = ToolBox.GetPetSpellIndex(spellName);
            if (ToolBox.PetKnowsSpell(spellName)
                && ToolBox.GetPetSpellReady(spellName)
                && !BannedSpells.Contains(spellName)
                && (ObjectManager.Pet.HasTarget || noTargetNeeded))
            {
                Thread.Sleep(ToolBox.GetLatency() + 100);
                Logger.Combat($"Cast (Pet) {spellName}");
                if (!onFocus)
                    Lua.LuaDoString($"CastPetAction({spellIndex});");
                else
                    Lua.LuaDoString($"CastPetAction({spellIndex}, \'focus\');");

                return true;
            }
            return false;
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= LuaEventsHandler;
        }

        public bool PetSpellIfEnoughForGrowl(string spellName, uint spellCost)
        {
            if (ObjectManager.Pet.Focus >= spellCost + 15
                && ObjectManager.Me.InCombatFlagOnly
                && ToolBox.PetKnowsSpell(spellName))
                if (PetSpell(spellName))
                    return true;
            return false;
        }

        public bool Normal(Spell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast);
        }

        public bool OnSelf(Spell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, true);
        }

        public bool OnFocusPlayer(Spell s, WoWPlayer onPlayerFocus, bool stopWandAndCast = true, bool onDeadTarget = false)
        {
            return AdvancedCast(s, stopWandAndCast, onPlayerFocus: onPlayerFocus, onDeadTarget: onDeadTarget);
        }

        public bool OnFocusUnit(Spell s, WoWUnit onUnitFocus, bool stopWandAndCast = true, bool onDeadTarget = false)
        {
            return AdvancedCast(s, stopWandAndCast, onUnitFocus: onUnitFocus, onDeadTarget: onDeadTarget);
        }

        public bool AdvancedCast(Spell s, bool stopWandAndCast = true, bool onSelf = false, WoWPlayer onPlayerFocus = null, WoWUnit onUnitFocus = null, bool onDeadTarget = false)
        {
            // Change and clear guid + banned list
            if (ObjectManager.Target.Guid != CurrentEnemyGuid)
            {
                BannedSpells.Clear();
                CurrentEnemyGuid = ObjectManager.Target.Guid;
            }

            if (onUnitFocus != null && onUnitFocus.IsDead && !onDeadTarget)
                return false;

            if (onPlayerFocus != null && onPlayerFocus.IsDead && !onDeadTarget)
                return false;

            if (onPlayerFocus == null 
                && onUnitFocus == null 
                && ObjectManager.Target.Guid > 0 
                && ObjectManager.Target.IsDead 
                && !onDeadTarget)
                return false;

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
                StopWandWaitGCD(WandSpell, s);

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

            if (onPlayerFocus != null || onUnitFocus != null)
            {
                if (onPlayerFocus != null && (!onPlayerFocus.IsValid || onPlayerFocus.GetDistance > 50))
                    return false;
                if (onUnitFocus != null && (!onUnitFocus.IsValid || onUnitFocus.GetDistance > 50))
                    return false;

                string focusName = onPlayerFocus != null ? onPlayerFocus.Name : onUnitFocus.Name;
                float focusDistance = onPlayerFocus != null ? onPlayerFocus.GetDistance : onUnitFocus.GetDistance;
                Vector3 focusPosition = onPlayerFocus != null ? onPlayerFocus.Position : onUnitFocus.Position;
                ulong focusGuid = onPlayerFocus != null ? onPlayerFocus.Guid : onUnitFocus.Guid;

                if (focusDistance > s.MaxRange || TraceLine.TraceLineGo(focusPosition))
                {
                    if (ObjectManager.Me.HaveBuff("Spirit of Redemption"))
                        return false;

                    Logger.Log($"Approaching {focusName}");
                    List<Vector3> path = PathFinder.FindPath(focusPosition);
                    if (path.Count <= 0)
                    {
                        Logger.Log($"Couldn't make a path toward {focusName}, skipping");
                        return false;
                    }
                    MovementManager.Go(path, false);

                    int limit = 3000;
                    while (MovementManager.InMoveTo
                    && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && ObjectManager.Me.IsAlive
                    && focusDistance > 20 || TraceLine.TraceLineGo(focusPosition)
                    && limit >= 0)
                    {
                        focusDistance = onPlayerFocus != null ? onPlayerFocus.GetDistance : onUnitFocus.GetDistance;
                        focusPosition = onPlayerFocus != null ? onPlayerFocus.Position : onUnitFocus.Position;
                        Thread.Sleep(1000);
                        limit -= 1000;
                    }
                }

                Logger.LogFight($"Casting {s.Name} on {focusName}");
                MovementManager.StopMove();
                //Lua.LuaDoString($"FocusUnit(\"{focusName}\")");
                ObjectManager.Me.FocusGuid = focusGuid;
                Lua.RunMacroText($"/cast [target=focus] {s.Name}");
                Usefuls.WaitIsCasting();
                return true;
            }

            s.Launch(stopMove, true, true);

            return true;
        }

        // Stops using wand and waits for its CD to be over
        private void StopWandWaitGCD(Spell wandSpell, Spell basicSpell)
        {
            CombatDebug("Stopping Wand and waiting for GCD");
            wandSpell.Launch();
            int c = 0;
            while (!basicSpell.IsSpellUsable && c <= 1500)
            {
                c += 50;
                Thread.Sleep(50);
            }
            CombatDebug("Waited for GCD : " + c);
            if (c > 1500)
                wandSpell.Launch();
        }

        private void CombatDebug(string s)
        {
            if (CombatDebugON)
                Logger.CombatDebug(s);
        }

        private void LuaEventsHandler(string id, List<string> args)
        {
            if (AutoDetectImmunities && args[11] == "IMMUNE" && !BannedSpells.Contains(args[9]))
            {
                Logger.Log($"{ObjectManager.Target.Name} is immune to {args[9]}, banning spell for this fight");
                BannedSpells.Add(args[9]);
            }
        }
    }
}
