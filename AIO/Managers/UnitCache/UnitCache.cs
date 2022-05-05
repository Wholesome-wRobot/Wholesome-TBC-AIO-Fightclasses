using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache
{
    public class UnitCache : IUnitCache
    {
        private readonly float _enemiesNearMeRange;
        private List<ulong> _groupGuids = new List<ulong>();
        private Dictionary<int, List<ulong>> _raidGuids = new Dictionary<int, List<ulong>>();

        public IWoWLocalPlayer Me { get; private set; } = new CachedWoWLocalPlayer(new WoWLocalPlayer(0));
        public IWoWUnit Target { get; private set; } = new CachedWoWUnit(new WoWUnit(0));
        public IWoWUnit Pet { get; private set; } = new CachedWoWUnit(new WoWUnit(0));
        public List<IWoWUnit> EnemyUnitsNearPlayer { get; private set; } = new List<IWoWUnit>();
        public List<IWoWUnit> EnemyUnitsTargetingPlayer { get; private set; } = new List<IWoWUnit>();
        public List<IWoWPlayer> GroupAndRaid { get; private set; } = new List<IWoWPlayer>();
        public List<IWoWPlayer> NearbyPlayers { get; private set; } = new List<IWoWPlayer>();
        public Dictionary<int, List<IWoWPlayer>> Raid { get; private set; } = new Dictionary<int, List<IWoWPlayer>>();

        public List<IWoWPlayer> ClosePartyMembers => GroupAndRaid.FindAll(member => member.GetDistance < 60).ToList();
        public List<IWoWUnit> EnemiesAttackingMe => EnemiesFighting.FindAll(enemy => enemy.TargetGuid == Me.Guid);

        public UnitCache()
        {
            _enemiesNearMeRange = 60;
            OnObjectManagerPulse();
        }

        public List<IWoWUnit> EnemiesFighting
        {
            get
            {
                return EnemyUnitsNearPlayer.FindAll(unit =>
                    unit.InCombatFlagOnly
                    && (unit.TargetGuid == Me.Guid || unit.TargetGuid == Pet.Guid || GroupAndRaid.Exists(member => unit.TargetGuid == member.Guid)));
            }
        }

        public List<IWoWPlayer> TargetedByEnemies
        {
            get
            {
                List<ulong> targetedGuids = EnemiesFighting
                    .Select(enemy => enemy.TargetGuid)
                    .ToList();
                return GroupAndRaid
                    .Where(player => targetedGuids.Contains(player.Guid))
                    .OrderBy(player => player.HealthPercent)
                    .ToList();
                /*
                return EnemiesFighting
                    .Select(u => u.GetTargetObject)
                    .Distinct()
                    .ToList()
                    .FindAll(u => GroupAndRaid.Any(m => m.Guid == u.Guid))
                    .OrderBy(a => a.HealthPercent)
                    .ToList();
                */
            }
        }


        // Returns whether hostile units are close to the target. Target and distance must be passed as argument
        public IWoWUnit GetClosestHostileFrom(IWoWUnit target, float distance)
        {
            foreach (IWoWUnit unit in EnemyUnitsNearPlayer.Where(e => e.PositionWithoutType.DistanceTo(target.PositionWithoutType) < distance))
            {
                if (unit.IsAlive
                    && !unit.IsTapDenied
                    && unit.IsValid
                    && !unit.IsTaggedByOther
                    && !unit.PlayerControlled
                    && unit.IsAttackable
                    && unit.Reaction == Reaction.Hostile
                    && unit.Guid != target.Guid)
                    return unit;
            }

            return null;
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
                List<ulong> grGuids = new List<ulong>();
                Dictionary<int, List<ulong>> raidGuids = new Dictionary<int, List<ulong>>();
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
                        ulong playerGuid = 0;
                        WoWPlayer raidPlayer = allPlayers.Find(m => m.Name == name);

                        if (raidPlayer == null && name == Me.Name)
                        {
                            playerGuid = Me.Guid;
                        }

                        if (raidPlayer != null || playerGuid > 0)
                        {
                            playerGuid = playerGuid > 0 ? playerGuid : raidPlayer.Guid;
                            grGuids.Add(playerGuid);
                            if (raidGuids.TryGetValue(subGroupNumber, out var subgroup))
                            {
                                subgroup.Add(playerGuid);
                            }
                            else
                            {
                                raidGuids[subGroupNumber] = new List<ulong>() { playerGuid };
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError($"{name} in group {subGroupNumber} is not a valid group number");
                    }
                }
                _groupGuids = grGuids;
                _raidGuids = raidGuids;
            }
            // Group loop
            else if (Party.GetPartyNumberPlayers() > 0)
            {
                _raidGuids.Clear();
                List<ulong> grGuids = new List<ulong>();
                grGuids.Add(Me.Guid);
                foreach (WoWPlayer p in Party.GetParty())
                {
                    grGuids.Add(p.Guid);
                }
                _groupGuids = grGuids;
            }
            else
            {
                _groupGuids.Clear();
                _raidGuids.Clear();
            }
        }

        private void OnObjectManagerPulse()
        {
            Stopwatch watch = Stopwatch.StartNew();
            WoWLocalPlayer playerObject;
            IWoWLocalPlayer cachedPlayer;
            IWoWUnit cachedTarget, cachedPet;
            List<WoWUnit> units;
            List<WoWPlayer> players;
            List<IWoWPlayer> groupAndRaid = new List<IWoWPlayer>();
            Dictionary<int, List<IWoWPlayer>> raid = new Dictionary<int, List<IWoWPlayer>>();

            lock (ObjectManager.Locker)
            {
                playerObject = ObjectManager.Me;
                cachedPlayer = new CachedWoWLocalPlayer(playerObject);

                var targetObjectBaseAddress = ObjectManager.GetObjectByGuid(playerObject.Target).GetBaseAddress;
                if (targetObjectBaseAddress != 0)
                {
                    var target = new WoWUnit(targetObjectBaseAddress);
                    cachedTarget = new CachedWoWUnit(target);
                }
                else
                {
                    cachedTarget = new CachedWoWUnit(new WoWUnit(0));
                }

                cachedPet = new CachedWoWUnit(ObjectManager.Pet);
                units = ObjectManager.GetObjectWoWUnit();
                players = ObjectManager.GetObjectWoWPlayer();
            }

            long watch1 = watch.ElapsedMilliseconds;

            var enemyUnitsNearPlayer = new List<IWoWUnit>(units.Count);
            var enemyUnitsTargetingPlayer = new List<IWoWUnit>(units.Count);

            var targetPosition = cachedTarget.PositionWithoutType;
            var targetGuid = cachedTarget.Guid;
            var playerPosition = cachedPlayer.PositionWithoutType;
            var playerGuid = cachedPlayer.Guid;

            // Raid
            if (_raidGuids.Count > 0)
            {
                foreach (var raidGroup in _raidGuids)
                {
                    raid[raidGroup.Key] = new List<IWoWPlayer>();
                    foreach (ulong memberGuid in raidGroup.Value)
                    {
                        WoWPlayer memberTOAdd = players.Find(pl => pl.Guid == memberGuid);
                        if (memberTOAdd != null)
                        {
                            //Logger.Log($"Adding {memberTOAdd.Name} to group {raidGroup.Key}");
                            CachedWoWPlayer playerToAdd = new CachedWoWPlayer(memberTOAdd);
                            groupAndRaid.Add(playerToAdd);
                            raid[raidGroup.Key].Add(playerToAdd);
                        }
                        else if (memberGuid == Me.Guid)
                        {
                            //Logger.Log($"Adding {ObjectManager.Me.Name} to group {raidGroup.Key}");
                            CachedWoWPlayer playerToAdd = new CachedWoWPlayer(playerObject);
                            groupAndRaid.Add(playerToAdd);
                            raid[raidGroup.Key].Add(playerToAdd);
                        }
                    }
                }
            }
            else
            {
                // Group
                foreach (ulong memberGuid in _groupGuids)
                {
                    WoWPlayer memberTOAdd = players.Find(pl => pl.Guid == memberGuid);
                    if (memberTOAdd != null)
                    {
                        groupAndRaid.Add(new CachedWoWPlayer(memberTOAdd));
                    }
                    else if (memberGuid == Me.Guid)
                    {
                        groupAndRaid.Add(new CachedWoWPlayer(playerObject));
                    }
                }
            }


            long watch2 = watch.ElapsedMilliseconds;

            int enemiesFound = 0;
            //Logger.LogError($"Checking {units.Count} units");
            // Enemy loop
            foreach (var unit in units)
            {
                
                if (!unit.IsAlive || !unit.IsValid || unit.NotSelectable || (int)unit.Reaction > 3)
                {
                    continue;
                }
                
                ulong unitGuid = unit.Guid;
                /*
                if (targetGuid != 0 && unitGuid != targetGuid && unit.Target != playerGuid && !_groupGuids.Contains(unit.Target) && unit.PositionWithoutType.DistanceTo(cachedTarget.PositionWithoutType) > 30)
                {
                    continue;
                }                

                if (targetGuid == 0 && !unit.InCombatFlagOnly)
                {
                    continue;
                }
                */

                IWoWUnit cachedUnit = unitGuid == targetGuid ? cachedTarget : new CachedWoWUnit(unit);

                if (unit.Target == playerObject.Guid)
                {
                    enemiesFound++;
                    enemyUnitsTargetingPlayer.Add(cachedUnit);
                }

                if (playerPosition.DistanceTo(unit.PositionWithoutType) <= _enemiesNearMeRange)
                {
                    enemiesFound++;
                    enemyUnitsNearPlayer.Add(cachedUnit);
                }
            }

            //Logger.LogError($"{enemiesFound} enemies found");

            long watch3 = watch.ElapsedMilliseconds;

            Me = cachedPlayer;
            Target = cachedTarget;
            Pet = cachedPet;

            EnemyUnitsNearPlayer = enemyUnitsNearPlayer;
            EnemyUnitsTargetingPlayer = enemyUnitsTargetingPlayer;
            GroupAndRaid = groupAndRaid;
            Raid = raid;
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
            long final = watch.ElapsedMilliseconds;
            if (final > 0)
                Logger.LogError($"{units.Count} units | LOCK=>{watch1}, GROUP=>{watch2}, LOOP=>{watch3}, ASSIGN=>{final}");
        }
    }
}
