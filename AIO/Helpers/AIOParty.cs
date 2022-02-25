using System.Collections.Generic;
using System.Threading;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Helpers
{
    public class AIOParty
    {
        private static object _groupLock = new object();
        private static List<AIOPartyMember> _groupAndRaid = new List<AIOPartyMember>();
        private static Dictionary<int, List<AIOPartyMember>> _raidGroups = new Dictionary<int, List<AIOPartyMember>>();
        private static Dictionary<string, string> _partySpecsCache = new Dictionary<string, string>();

        public static bool ActivateSpecRecord { get; set; }
        public static bool InspectTalentReady { get; set; } = true;

        public static Dictionary<int, List<AIOPartyMember>> RaidGroups
        {
            get
            {
                lock (_groupLock)
                {
                    return _raidGroups;
                }
            }
        }

        public static List<AIOPartyMember> GroupAndRaid
        {
            get
            {
                lock (_groupLock)
                {
                    return _groupAndRaid;
                }
            }
        }

        public static List<AIOPartyMember> ClosePartyMembers
        {
            get
            {
                lock (_groupLock)
                {
                    return _groupAndRaid.FindAll(m => m.GetDistance < 60);
                }
            }
        }

        public static void UpdateParty()
        {
            if (StatusChecker.BasicConditions())
            {
                lock (_groupLock)
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

                    if (ActivateSpecRecord)
                        RecordPartySpecs();

                    DoRaidUpdate();
                }
            }
        }

        public static void DoRaidUpdate()
        {
            string raidString = Lua.LuaDoString<string>
                (@$"raidCount = GetNumRaidMembers()
                    result = raidCount
                    for i = 1 , raidCount do
                        name, _, subgroup = GetRaidRosterInfo(i);
                        result = result .. '|' .. name .. ':' .. subgroup
                    end
                    return result");

            _raidGroups.Clear();
            if (raidString == "0")
            {
                return;
            }

            string[] players = raidString.Split('|');
            foreach (var playerString in players)
            {
                if (playerString.Contains(":"))
                {
                    string[] parts = playerString.Split(':');
                    if (parts.Length == 2)
                    {
                        string name = parts[0];
                        string stringSubgroupNumber = parts[1];

                        if (int.TryParse(parts[1], out int subGroupNumber))
                        {
                            AIOPartyMember player = _groupAndRaid.Find(m => (m.Name == name) && m.IsValid);
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
                            Logger.LogError($"{name} - {subGroupNumber}, not a valid group number");
                        }
                    }
                }
            }
        }

        public static List<WoWUnit> EnemiesFighting
        {
            get
            {
                lock (_groupLock)
                {
                    return AIORadar.AllUnits.FindAll(e => GroupAndRaid.Exists(u => e.Target == u.Guid));
                }
            }
        }


        public static void SwitchTarget(Cast cast, AIOSpell spell)
        {
            if ((ObjectManager.Target.Target == ObjectManager.Me.Guid
                || !ObjectManager.Target.IsAlive
                || !ObjectManager.Target.HasTarget
                || !ObjectManager.Me.HasTarget)
                && !ToolBox.HasDebuff("Taunt", "target")
                && !ToolBox.HasDebuff("Growl", "target"))
            {
                lock (_groupLock)
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

        public static void RecordPartySpecs()
        {
            lock (_groupLock)
            {
                foreach (AIOPartyMember p in _groupAndRaid)
                {
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

                        string spec = ToolBox.GetSpec(p.Name);

                        if (spec == null)
                        {
                            Logger.Log($"Couldn't record {p.Name}'s specialization");
                            p.Specialization = null;
                            break;
                        }

                        if (spec == "retry")
                            break;

                        p.Specialization = spec;
                        _partySpecsCache.Add(p.Name, spec);
                        Logger.Log($"{p.Name}'s specialization is {p.Specialization}");
                        break;
                    }
                }
            }
        }

        // Party Drink
        public static bool PartyDrink(string drinkName, int threshold)
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
                ToolBox.AddToDoNotSellList(drinkName.Trim());
                if (ToolBox.CountItemStacks(drinkName) > 0)
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

        public static void InspectTalentReadyHandler()
        {
            InspectTalentReady = true;
        }

        public static void GroupRosterChangedHandler()
        {
            UpdateParty();
        }
    }
}
