using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Managers.PartyManager
{
    public class PartyManager : IPartyManager
    {
        private IUnitCache _unitCache;

        public PartyManager(IUnitCache unitCache)
        {
            _unitCache = unitCache;
        }

        public void SwitchTarget(Cast cast, AIOSpell spell)
        {
            if ((_unitCache.Target.Target == _unitCache.Me.Guid
                || !_unitCache.Target.IsAlive
                || !_unitCache.Target.HasTarget
                || !_unitCache.Me.HasTarget)
                && !WTEffects.HasDebuff("Taunt", "target")
                && !WTEffects.HasDebuff("Growl", "target"))
            {
                foreach (IWoWUnit enemy in _unitCache.EnemiesFighting)
                {
                    IWoWPlayer partyMemberToSave = _unitCache.GroupAndRaid
                        .Find(m => enemy.Target == m.Guid && (m.Guid != _unitCache.Me.Guid || !_unitCache.Me.HasTarget));

                    if (partyMemberToSave != null)
                    {
                        Logger.Log($"Regaining aggro [{enemy.Name} attacking {partyMemberToSave.Name}]");
                        _unitCache.Me.SetTarget(enemy.Guid);

                        if (spell != null)
                        {
                            if (spell.Name == "Righteous Defense")
                                cast.OnFocusUnit(spell, partyMemberToSave);
                            if (spell.Name == "Intervene" && enemy.PositionWithoutType.DistanceTo(partyMemberToSave.PositionWithoutType) < 10)
                                cast.OnTarget(spell);
                        }
                        return;
                    }
                }
            }
        }

        /*
        private void RecordPartySpecs()
        {
            lock (_partyLock)
            {
                foreach (AIOPartyMember p in _groupAndRaid)
                {
                    Logger.Log($"{p.Name} spec is {p.Specialization}");
                    if (p.Guid != ObjectManager.Me.Guid
                        && p.GetDistance < 25
                        && p.IsAlive
                        && p.IsValid
                        && p.Specialization == null)
                    {
                        if (_partySpecsCache.ContainsKey(p.Name))
                        {
                            p.Specialization = _partySpecsCache[p.Name];
                            continue;
                        }

                        string spec = GetSpec(p.Name);

                        if (spec == null)
                        {
                            Logger.Log($"Couldn't record {p.Name}'s specialization");
                            p.Specialization = null;
                            break;
                        }

                        if (spec == "retry")
                            continue;

                        p.Specialization = spec;
                        _partySpecsCache.Add(p.Name, spec);
                        Logger.Log($"{p.Name}'s specialization is {p.Specialization}");
                    }
                }
            }
        }
        */
        // Party Drink
        public bool PartyDrink(string drinkName, int threshold)
        {
            if (_unitCache.Me.ManaPercentage >= threshold)
                return false;

            Timer wait = new Timer(1000);
            while (!wait.IsReady && !_unitCache.Me.InCombatFlagOnly && !Fight.InFight)
                Thread.Sleep(300);

            if (_unitCache.Me.ManaPercentage < threshold
                && !_unitCache.Me.HasDrinkBuff
                && !MovementManager.InMovement
                && !MovementManager.InMoveTo
                && drinkName.Trim().Length > 0)
            {
                WTSettings.AddToDoNotSellList(drinkName.Trim());
                if (WTItem.CountItemStacks(drinkName) > 0)
                {
                    ItemsManager.UseItemByNameOrId(drinkName);
                    Logger.Log($"[Party drink] Using {drinkName}");
                    Thread.Sleep(2000);
                    return true;
                }
                else
                {
                    Logger.Log($"Couldn't find any {drinkName} in bags");
                    return false;
                }
            }
            return false;
        }

        /*
        // Gets Character's specialization (talents)
        private string GetSpec(string inspectUnitName = null)
        {
            string inspectString = inspectUnitName == null ? "false" : "true";

            int highestTalents = 0;
            Dictionary<string, int> Talents = new Dictionary<string, int>();

            if (inspectUnitName != null)
            {
                Lua.LuaDoString($"InspectUnit('{inspectUnitName}');");
                Thread.Sleep(500 + ToolBox.GetLatency());
                if (!_inspectTalentReady)
                {
                    Lua.RunMacroText("/Click InspectFrameCloseButton");
                    return "retry";
                }
            }

            for (int i = 1; i <= 3; i++)
            {
                Talents.Add(
                    Lua.LuaDoString<string>($"local name, _, _ = GetTalentTabInfo('{i}', {inspectString}); return name"),
                    Lua.LuaDoString<int>($"local _, _, pointsSpent = GetTalentTabInfo('{i}', {inspectString}); return pointsSpent")
                );
            }
            highestTalents = Talents.Max(x => x.Value);

            if (inspectUnitName != null)
            {
                Lua.RunMacroText("/Click InspectFrameCloseButton");
                _inspectTalentReady = false;
            }

            if (highestTalents == 0)
                return null;

            return Talents.Where(t => t.Value == highestTalents).FirstOrDefault().Key;
        }
        */
        // Virtually increases missing HP of tanks based on given `priorityPercent`.
        // Then returns a list of tanks those should be healed before other group members.
        public List<IWoWPlayer> TanksNeedPriorityHeal(List<IWoWPlayer> tanks, List<IWoWPlayer> groupMembers, int priorityPercent)
        {
            var prioirtyList = new List<IWoWPlayer>();
            if (tanks.Count == 0 || groupMembers.Count == 0)
            {
                return prioirtyList;
            }

            foreach (var tank in tanks)
            {
                var missingHPPercent = 100.0 - tank.HealthPercent;
                var virtualHP = 100 - missingHPPercent * (1.0 + ((float)priorityPercent) / 100);
                if (virtualHP < groupMembers[0].HealthPercent)
                {
                    prioirtyList.Add(tank);
                }
            }

            return prioirtyList;
        }
    }
}
