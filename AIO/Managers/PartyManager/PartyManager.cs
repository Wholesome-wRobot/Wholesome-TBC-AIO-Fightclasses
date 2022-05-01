using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Managers.PartyManager
{
    public class PartyManager : IPartyManager
    {
        private readonly object _partyLock = new object();
        private List<AIOPartyMember> _groupAndRaid = new List<AIOPartyMember>();
        private Dictionary<int, List<AIOPartyMember>> _raidGroups = new Dictionary<int, List<AIOPartyMember>>();
        private Dictionary<string, string> _partySpecsCache = new Dictionary<string, string>();
        private bool _activateSpecRecords;
        private bool _inspectTalentReady;
        private IUnitCache _unitCache;

        public Dictionary<int, List<AIOPartyMember>> RaidGroups
        {
            get
            {
                lock (_partyLock)
                {
                    return _raidGroups;
                }
            }
        }

        public List<AIOPartyMember> GroupAndRaid
        {
            get
            {
                lock (_partyLock)
                {
                    return _groupAndRaid;
                }
            }
        }

        public List<AIOPartyMember> ClosePartyMembers
        {
            get
            {
                lock (_partyLock)
                {
                    return _groupAndRaid.FindAll(m => m.GetDistance < 60);
                }
            }
        }

        public List<WoWUnit> EnemiesFighting
        {
            get
            {
                lock (_partyLock)
                {
                    return _unitCache.AllUnits.FindAll(unit => unit.IsAttackable && GroupAndRaid.Exists(member => unit.Target == member.Guid));
                }
            }
        }

        public List<WoWUnit> TargetedByEnemies
        {
            get
            {
                return EnemiesFighting
                    .Select(u => u.TargetObject)
                    .Distinct()
                    .ToList()
                    .FindAll(u => GroupAndRaid.Exists(m => m.Guid == u.Guid))
                    .OrderBy(a => a.HealthPercent)
                    .ToList();
            }
        }


        public PartyManager(IUnitCache unitCache)
        {
            _unitCache = unitCache;
        }

        public void Initialize()
        {
            UpdateGroupAndRaid();
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsWithArgsHandler;
        }

        public void Dispose()
        {
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsWithArgsHandler;
        }

        public void ActivateSpecRecords()
        {
            if (!_activateSpecRecords)
            {
                _activateSpecRecords = true;
                RecordPartySpecs();
            }
        }

        private void UpdateGroupAndRaid()
        {
            if (StatusChecker.BasicConditions())
            {
                lock (_partyLock)
                {
                    _groupAndRaid.Clear();

                    List<WoWPlayer> allMembersList = new List<WoWPlayer>();
                    allMembersList.AddRange(Party.GetRaidMembers());
                    allMembersList.AddRange(Party.GetParty());

                    // Add me
                    _groupAndRaid.Add(new AIOPartyMember(ObjectManager.Me.GetBaseAddress));

                    // Add party/raid players
                    foreach (WoWPlayer player in allMembersList)
                    {
                        if (!_groupAndRaid.Exists(m => m.GetBaseAddress == player.GetBaseAddress))
                        {
                            _groupAndRaid.Add(new AIOPartyMember(player.GetBaseAddress));
                        }
                    }

                    // Raid update
                    string raidString = Lua.LuaDoString<string>(@$"
                        local raidCount = GetNumRaidMembers();
                        local result = raidCount;
                        for i = 1 , raidCount do
                            local name, _, subgroup = GetRaidRosterInfo(i);
                            result = result .. '|' .. name .. ':' .. subgroup;
                        end
                        return result;
                        ");

                    _raidGroups.Clear();
                    if (raidString != "0")
                    {
                        string[] players = raidString.Split('|');
                        foreach (var playerString in players)
                        {
                            if (!playerString.Contains(":"))
                            {
                                continue;
                            }

                            string[] parts = playerString.Split(':');
                            if (parts.Length != 2)
                            {
                                continue;
                            }

                            string name = parts[0];
                            string stringSubgroupNumber = parts[1];

                            if (int.TryParse(parts[1], out int subGroupNumber))
                            {
                                AIOPartyMember player = _groupAndRaid.Find(m => m.Name == name && m.IsValid);
                                if (player != null)
                                {
                                    if (_raidGroups.TryGetValue(subGroupNumber, out var subgroup))
                                    {
                                        subgroup.Add(player);
                                    }
                                    else
                                    {
                                        _raidGroups[subGroupNumber] = new List<AIOPartyMember>() { player };
                                    }
                                }
                            }
                            else
                            {
                                Logger.LogError($"{name} in group {subGroupNumber} is not a valid group number");
                            }
                        }
                    }
                }

                if (_activateSpecRecords)
                {
                    RecordPartySpecs();
                }
            }
        }

        public void SwitchTarget(Cast cast, AIOSpell spell)
        {
            if ((ObjectManager.Target.Target == ObjectManager.Me.Guid
                || !ObjectManager.Target.IsAlive
                || !ObjectManager.Target.HasTarget
                || !ObjectManager.Me.HasTarget)
                && !WTEffects.HasDebuff("Taunt", "target")
                && !WTEffects.HasDebuff("Growl", "target"))
            {
                lock (_partyLock)
                {
                    foreach (WoWUnit enemy in EnemiesFighting)
                    {
                        WoWPlayer partyMemberToSave = _groupAndRaid
                            .Find(m => enemy.Target == m.Guid && (m.Guid != ObjectManager.Me.Guid || !ObjectManager.Me.HasTarget));

                        if (partyMemberToSave != null)
                        {
                            Logger.Log($"Regaining aggro [{enemy.Name} attacking {partyMemberToSave.Name}]");
                            ObjectManager.Me.Target = enemy.Guid;

                            if (spell != null)
                            {
                                if (spell.Name == "Righteous Defense")
                                    cast.OnFocusUnit(spell, partyMemberToSave);
                                if (spell.Name == "Intervene" && enemy.Position.DistanceTo(partyMemberToSave.Position) < 10)
                                    cast.OnTarget(spell);
                            }
                            return;
                        }
                    }
                }
            }
        }

        private void EventsWithArgsHandler(string id, List<string> args)
        {
            if (id == "PARTY_MEMBERS_CHANGED"
                || id == "PARTY_MEMBER_DISABLE"
                || id == "PARTY_MEMBER_ENABLE"
                || id == "RAID_ROSTER_UPDATE"
                || id == "GROUP_ROSTER_CHANGED"
                || id == "PARTY_CONVERTED_TO_RAID"
                || id == "RAID_TARGET_UPDATE")
            {
                Thread.Sleep(1000);
                UpdateGroupAndRaid();
                /*
                Logger.Log(id);
                foreach (WoWPlayer player in _groupAndRaid)
                {
                    Logger.Log(player.Name);
                }
                */
            }

            if (id == "INSPECT_TALENT_READY")
            {
                _inspectTalentReady = true;
            }
        }

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

        // Party Drink
        public bool PartyDrink(string drinkName, int threshold)
        {
            if (ObjectManager.Me.ManaPercentage >= threshold)
                return false;

            Timer wait = new Timer(1000);
            while (!wait.IsReady && !ObjectManager.Me.InCombatFlagOnly && !Fight.InFight)
                Thread.Sleep(300);

            if (ObjectManager.Me.ManaPercentage < threshold
                && !ObjectManager.Me.HaveBuff("Drink")
                && !MovementManager.InMovement
                && !MovementManager.InMoveTo
                && drinkName.Trim().Length > 0)
            {
                WTSettings.AddToDoNotSellList(drinkName.Trim());
                if (WTItem.CountItemStacks(drinkName) > 0)
                {
                    ItemsManager.UseItemByNameOrId(drinkName);
                    Logger.Log($"[Party drink] Using {drinkName}");
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

        // Virtually increases missing HP of tanks based on given `priorityPercent`.
        // Then returns a list of tanks those should be healed before other group members.
        public List<WoWUnit> TanksNeedPriorityHeal(List<WoWUnit> tanks, List<AIOPartyMember> groupMembers, int priorityPercent)
        {
            var prioirtyList = new List<WoWUnit>();
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
