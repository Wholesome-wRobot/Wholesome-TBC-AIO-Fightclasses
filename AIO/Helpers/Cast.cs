using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Helpers
{
    public class Cast
    {
        private AIOSpell DefaultBaseSpell { get; }
        private bool CombatDebugON { get; }
        private AIOSpell WandSpell { get; }
        private bool AutoDetectImmunities { get; }
        public bool IsBackingUp { get; set; }
        private WoWUnit CurrentSpellTarget { get; set; }
        private AIOSpell CurrentSpell { get; set; }
        public bool IsApproachingTarget { get; set; }
        private bool CombatLogON { get; set; }

        private static Random rng = new Random();

        public Cast(AIOSpell defaultBaseSpell, AIOSpell wandSpell, BaseSettings settings)
        {
            AutoDetectImmunities = settings.AutoDetectImmunities;
            DefaultBaseSpell = defaultBaseSpell;
            CombatDebugON = settings.ActivateCombatDebug;
            WandSpell = wandSpell;
            IsApproachingTarget = false;
            CombatLogON = settings.ActivateCombatLog;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsLuaStringWithArgsHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
        }

        public bool PetSpell(string spellName, bool onFocus = false, bool noTargetNeeded = false)
        {
            int spellIndex = ToolBox.GetPetSpellIndex(spellName);
            if (ToolBox.PetKnowsSpell(spellName)
                && ToolBox.GetPetSpellReady(spellName)
                && !UnitImmunities.Contains(ObjectManager.Target, spellName)
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
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsLuaStringWithArgsHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
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

        public bool Buff(List<AIOPartyMember> units, AIOSpell spell, uint reagent = 0)
        {
            if (!spell.KnownSpell)
                return false;
            if (reagent != 0 && !ItemsManager.HasItemById(reagent))
                return false;
            List<AIOPartyMember> unitsNeedsBuff = units
                .FindAll(m => !m.HaveBuff(spell.Name))
                .OrderBy(m => rng.Next())
                .ToList();
            if (unitsNeedsBuff.Count <= 0)
                return false;
            return OnFocusUnit(spell, unitsNeedsBuff[0]);
        }

        public bool OnTarget(AIOSpell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast);
        }

        public bool OnSelf(AIOSpell s, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, true);
        }
        /*
        public bool OnFocusPlayer(AIOSpell s, WoWPlayer onPlayerFocus, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, onPlayerFocus: onPlayerFocus);
        }
        */
        public bool OnFocusUnit(AIOSpell s, WoWUnit onUnitFocus, bool stopWandAndCast = true)
        {
            return AdvancedCast(s, stopWandAndCast, onUnitFocus: onUnitFocus);
        }

        public bool AdvancedCast(AIOSpell spell, bool stopWandAndCast = true, bool onSelf = false, WoWUnit onUnitFocus = null)
        {
            WoWUnit Me = ObjectManager.Me;
            float buffer = 700;

            if (IsApproachingTarget)
                return true;

            CurrentSpell = spell;

            CombatDebug("*----------- INTO PRE CAST FOR " + CurrentSpell.Name);

            if (!CurrentSpell.KnownSpell
                || IsBackingUp
                || Me.CastingTimeLeft > buffer
                || !CurrentSpell.IsForcedCooldownReady
                || Me.IsStunned)
                return false;

            // Define target
            else if (onUnitFocus != null)
                CurrentSpellTarget = onUnitFocus;
            else if (onSelf)
                CurrentSpellTarget = ObjectManager.Me;
            else
            {
                if (CurrentSpell.MaxRange <= 0 && ObjectManager.Target.GetDistance > RangeManager.GetMeleeRangeWithTarget())
                    return false;
                CurrentSpellTarget = ObjectManager.Target;
            }

            // Now that we know the target
            if (CurrentSpellTarget == null
                || CurrentSpellTarget.GetDistance > 100
                || (CurrentSpellTarget.IsDead && !CurrentSpell.OnDeadTarget)
                || (CurrentSpell.MinRange > 0 && CurrentSpellTarget.GetDistance <= CurrentSpell.MinRange)
                || UnitImmunities.Contains(CurrentSpellTarget, CurrentSpell.Name)
                || (!CurrentSpellTarget.IsValid && !CurrentSpell.OnDeadTarget)) // double check this
                return false;

            CombatDebug("*----------- INTO CAST FOR " + CurrentSpell.Name);
            
            // CHECK COST
            if (CurrentSpell.PowerType == -2 && Me.Health < CurrentSpell.Cost)
            {
                CombatDebug($"{CurrentSpell.Name}: Not enough health {CurrentSpell.Cost}/{Me.Health}, SKIPPING");
                return false;
            }
            else if (CurrentSpell.PowerType == 0 && Me.Mana < CurrentSpell.Cost)
            {
                CombatDebug($"{CurrentSpell.Name}: Not enough mana {CurrentSpell.Cost}/{Me.Mana}, SKIPPING");
                return false;
            }
            else if (CurrentSpell.PowerType == 1 && Me.Rage < CurrentSpell.Cost)
            {
                CombatDebug($"{CurrentSpell.Name}: Not enough rage {CurrentSpell.Cost}/{Me.Rage}, SKIPPING");
                return false;
            }
            else if (CurrentSpell.PowerType == 2 && ObjectManager.Pet.Focus < CurrentSpell.Cost)
            {
                CombatDebug($"{CurrentSpell.Name}: Not enough pet focus {CurrentSpell.Cost}/{ObjectManager.Pet.Focus}, SKIPPING");
                return false;
            }
            else if (CurrentSpell.PowerType == 3 && Me.Energy < CurrentSpell.Cost)
            {
                CombatDebug($"{CurrentSpell.Name}: Not enough energy {CurrentSpell.Cost}/{Me.Energy}, SKIPPING");
                return false;
            }
            
            // DON'T CAST BECAUSE WANDING
            if (WandSpell != null 
                && ToolBox.UsingWand() 
                && !stopWandAndCast)
            {
                CombatDebug("Didn't cast because we were wanding");
                return false;
            }
            
            // COOLDOWN CHECKS
            float _spellCD = CurrentSpell.GetCurrentCooldown;
            CombatDebug($"Cooldown is {_spellCD}");
            
            if (_spellCD >= 500)
            {
                CombatDebug("Didn't cast because cd is too long");
                return false;
            }
            
            // STOP WAND FOR CAST
            if (WandSpell != null
                && ToolBox.UsingWand()
                && stopWandAndCast)
                StopWandWaitGCD(WandSpell);
            
            
            // Wait for remaining Cooldown
            if (_spellCD > 0f && _spellCD < buffer)
            {
                CombatDebug($"{CurrentSpell.Name} is almost ready, waiting");
                while (CurrentSpell.GetCurrentCooldown > 0 && CurrentSpell.GetCurrentCooldown < 500)
                    Thread.Sleep(50);
            }
            
            if (!CurrentSpell.IsSpellUsable)
            {
                CombatDebug("Didn't cast because spell somehow not usable");
                return false;
            }

            bool stopMove = CurrentSpell.CastTime > 0 || CurrentSpell.IsChannel;

            if (CurrentSpellTarget.Guid != Me.Guid)
            {
                if (CurrentSpell.MaxRange > 0 && CurrentSpellTarget.GetDistance > CurrentSpell.MaxRange || TraceLine.TraceLineGo(CurrentSpellTarget.Position))
                {
                    if (Me.HaveBuff("Spirit of Redemption"))
                        return false;

                    Logger.LogFight($"Target not in range/sight, recycling {CurrentSpell.Name}");

                    if (Fight.InFight)
                        IsApproachingTarget = true;
                    else
                        ApproachSpellTarget();

                    return true;
                }
            }

            if (onUnitFocus != null)
                ObjectManager.Me.FocusGuid = CurrentSpellTarget.Guid;

            string unit = onUnitFocus != null ? "focus" : "target";
            unit = onSelf ? "player" : unit;

            // Wait for remaining cast in case of buffer
            while (Me.CastingTimeLeft > 0)
                Thread.Sleep(25);

            if (stopMove)
                MovementManager.StopMoveNewThread();

            if (CombatLogON)
            {
                string rankString = CurrentSpell.Rank > 0 ? $"(Rank {CurrentSpell.Rank})" : "";
                Logger.Log($"[Spell] Casting {CurrentSpell.Name.Replace("()", "")} {rankString} on {CurrentSpellTarget.Name}");
            }

            CurrentSpell.Launch(stopMove, false, true, unit);
            Thread.Sleep(100);

            ToolBox.ClearCursor();

            // Wait for channel to end
            if (CurrentSpell.IsChannel)
            {
                CombatDebug($"{CurrentSpell.Name} is channel, wait cast");
                while (ToolBox.GetChannelTimeLeft("player") < 0)
                    Thread.Sleep(50);

                CurrentSpell.StartForcedCooldown();
                return true;
            }

            // Wait for instant cast GCD
            if (CurrentSpell.CastTime <= 0)
            {
                CurrentSpell.StartForcedCooldown();
                Timer gcdLimit = new Timer(1500);
                CombatDebug($"{CurrentSpell.Name} is instant, wait GCD");
                while (DefaultBaseSpell.GetCurrentCooldown > buffer && !gcdLimit.IsReady)
                    Thread.Sleep(50);

                if (gcdLimit.IsReady)
                    Logger.LogError("We had to resort to timer wait (GCD)");

                return true;
            }

            if (CurrentSpell.CastTime > 0)
            {
                // Wait for cast to end
                buffer = CurrentSpell.PreventDoubleCast ? 0 : buffer;
                CombatDebug($"{CurrentSpell.Name} is normal, wait until {buffer} left");
                while (Me.CastingTimeLeft > buffer)
                {
                    if (CurrentSpell.IsResurrectionSpell && CurrentSpellTarget.IsAlive)
                        Lua.RunMacroText("/stopcasting");

                    Thread.Sleep(50);
                }
                CurrentSpell.StartForcedCooldown();
            }

            return true;
        }

        // Manage spell target approach
        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            if (IsApproachingTarget)
            {
                ApproachSpellTarget();
                IsApproachingTarget = false;
            }
        }

        // Approach spell target
        private void ApproachSpellTarget()
        {
            Logger.Log($"Approaching {CurrentSpellTarget.Name} to cast {CurrentSpell.Name} ({CurrentSpellTarget.GetDistance}/{CurrentSpell.MaxRange} - {!TraceLine.TraceLineGo(CurrentSpellTarget.Position)})");
            MovementManager.Go(PathFinder.FindPath(CurrentSpellTarget.Position), false);
            Timer limit = new Timer(10000);
            Thread.Sleep(1000);

            while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                && !limit.IsReady
                && (CurrentSpellTarget.IsAlive || CurrentSpell.OnDeadTarget)
                && (CurrentSpellTarget.GetDistance > CurrentSpell.MaxRange - 2 || TraceLine.TraceLineGo(CurrentSpellTarget.Position)))
                Thread.Sleep(100);

            MovementManager.StopMoveNewThread();
        }

        // Stops using wand and waits for its CD to be over
        private void StopWandWaitGCD(AIOSpell wandSpell)
        {
            CombatDebug("Stopping Wand and waiting for GCD");
            wandSpell.Launch();
            Timer limit = new Timer(1500);

            while (!CurrentSpell.IsSpellUsable && !limit.IsReady)
                Thread.Sleep(50);

            CombatDebug($"Waited for GCD after wand stop : {1500 - limit.TimeLeft()}");
        }

        private void CombatDebug(string s)
        {
            if (CombatDebugON)
                Logger.CombatDebug(s);
        }

        private void EventsLuaStringWithArgsHandler(string id, List<string> args)
        {
            if (args.Count >= 12 && AutoDetectImmunities && args[11] == "IMMUNE")
                UnitImmunities.Add(CurrentSpellTarget, args[9]);

            if (args.Count >= 12 && args[11] == "Target not in line of sight" && Fight.InFight)
            {
                Logger.Log("Forcing Approach");
                IsApproachingTarget = true;
            }
        }
    }
}
