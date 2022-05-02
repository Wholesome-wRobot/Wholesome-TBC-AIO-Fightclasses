using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache
{
    public class UnitCache : IUnitCache
    {
        private readonly float _enemiesNearMeRange;

        public IWoWLocalPlayer Me { get; private set; } = new CachedWoWLocalPlayer(new WoWLocalPlayer(0));
        public IWoWUnit Target { get; private set; } = new CachedWoWUnit(new WoWUnit(0));
        public IWoWUnit Pet { get; private set; } = new CachedWoWUnit(new WoWUnit(0));
        public IWoWUnit[] EnemyUnitsNearPlayer { get; private set; } = new IWoWUnit[0];
        public IWoWUnit[] EnemyUnitsTargetingPlayer { get; private set; } = new IWoWUnit[0];
        public IWoWPlayer[] Group { get; private set; } = new IWoWPlayer[0];
        public Dictionary<int, List<IWoWPlayer>> Raid { get; private set; } = new Dictionary<int, List<IWoWPlayer>>();

        public UnitCache()
        {
            _enemiesNearMeRange = 60;
        }

        public void Initialize()
        {
            UpdateGroupAndRaid();
            ObjectManagerEvents.OnObjectManagerPulsed += OnObjectManagerPulse;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsWithArgsHandler;
        }

        public void Dispose()
        {
            ObjectManagerEvents.OnObjectManagerPulsed -= OnObjectManagerPulse;
            EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsWithArgsHandler;
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
            }
        }

        private void UpdateGroupAndRaid()
        {
            // Raid loop
            if (Party.GetRaidMemberCount() > 0)
            {
                Dictionary<int, List<IWoWPlayer>> raid = new Dictionary<int, List<IWoWPlayer>>();
                Group = new IWoWPlayer[0];
                List<WoWPlayer> allPlayers = ObjectManager.GetObjectWoWPlayer();
                string raidString = Lua.LuaDoString<string>(@$"
                            local raidCount = GetNumRaidMembers();
                            local result = '';
                            for i = 1 , raidCount do
                                local name, _, subgroup = GetRaidRosterInfo(i);
                                result = result .. '|' .. name .. ':' .. subgroup;
                            end
                            return result;
                        ");

                string[] playersStrings = raidString.Split('|');

                foreach (var playerString in playersStrings)
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
                        WoWPlayer raidPlayer = allPlayers.Find(m => m.Name == name);

                        if (raidPlayer == null && name == Me.Name)
                        {
                            raidPlayer = ObjectManager.Me;
                        }

                        if (raidPlayer != null)
                        {
                            if (raid.TryGetValue(subGroupNumber, out var subgroup))
                            {
                                subgroup.Add(new CachedWoWPlayer(raidPlayer));
                            }
                            else
                            {
                                raid[subGroupNumber] = new List<IWoWPlayer>() { new CachedWoWPlayer(raidPlayer) };
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError($"{name} in group {subGroupNumber} is not a valid group number");
                    }
                }
                Raid = raid;
            }
            // Group loop
            else if (Party.GetPartyNumberPlayers() > 0)
            {
                List<IWoWPlayer> group = new List<IWoWPlayer>();
                Raid = new Dictionary<int, List<IWoWPlayer>>();
                group.Add(Me);
                foreach (WoWPlayer p in Party.GetParty())
                {
                    group.Add(new CachedWoWPlayer(p));
                }
                Group = group.ToArray();
            }
            else
            {
                Raid = new Dictionary<int, List<IWoWPlayer>>();
                Group = new IWoWPlayer[0];
            }
        }

        private void OnObjectManagerPulse()
        {
            Stopwatch watch = Stopwatch.StartNew();
            lock (ObjectManager.Locker)
            {
                WoWLocalPlayer player;
                IWoWLocalPlayer cachedPlayer;
                IWoWUnit cachedTarget, cachedPet;
                List<WoWUnit> units;

                lock (ObjectManager.Locker)
                {
                    player = ObjectManager.Me;
                    cachedPlayer = new CachedWoWLocalPlayer(player);

                    cachedTarget = new CachedWoWUnit(new WoWUnit(0));
                    var targetObjectBaseAddress = ObjectManager.GetObjectByGuid(player.Target).GetBaseAddress;
                    if (targetObjectBaseAddress != 0)
                    {
                        var target = new WoWUnit(targetObjectBaseAddress);
                        cachedTarget = new CachedWoWUnit(target);
                    }

                    cachedPet = new CachedWoWUnit(ObjectManager.Pet);

                    units = ObjectManager.GetObjectWoWUnit();
                }

                var enemyUnitsNearPlayer = new List<IWoWUnit>(units.Count);
                var interruptibleEnemyUnits = new List<IWoWUnit>(units.Count);
                var enemyUnitsTargetingPlayer = new List<IWoWUnit>(units.Count);

                var targetPosition = cachedTarget.PositionWithoutType;
                var targetGuid = cachedTarget.Guid;
                var playerPosition = cachedPlayer.PositionWithoutType;
                var playerGuid = cachedPlayer.Guid;

                // Enemy loop
                foreach (var unit in units)
                {
                    if (!unit.IsAlive)
                    {
                        continue;
                    }

                    if (!unit.IsAttackable || unit.NotSelectable)
                    {
                        continue;
                    }

                    var unitGuid = unit.Guid;

                    IWoWUnit cachedUnit = unitGuid == targetGuid ? cachedTarget : new CachedWoWUnit(unit);

                    if (unit.Target == player.Guid)
                    {
                        enemyUnitsTargetingPlayer.Add(cachedUnit);
                    }

                    var unitPosition = unit.PositionWithoutType;

                    if (playerPosition.DistanceTo(unitPosition) <= _enemiesNearMeRange)
                    {
                        enemyUnitsNearPlayer.Add(cachedUnit);
                    }
                }

                Me = cachedPlayer;
                Target = cachedTarget;
                Pet = cachedPet;

                EnemyUnitsNearPlayer = enemyUnitsNearPlayer.ToArray();
                EnemyUnitsTargetingPlayer = enemyUnitsTargetingPlayer.ToArray();
                /*
                foreach (KeyValuePair<int, List<IWoWPlayer>> pl in Raid)
                {
                    foreach (var play in pl.Value)
                    {
                        Logger.Log($"{pl.Key} => {play.Name}");
                    }
                }

                foreach (var p in Group)
                {
                    Logger.Log($"{p.Name}");
                }
                */
                Logger.LogError($"{watch.ElapsedMilliseconds}");
            }
        }
    }
}
