using System.Collections.Generic;
using WholesomeTBCAIO.Managers.UnitCache.Entities;

namespace WholesomeTBCAIO.Managers.UnitCache
{
    public interface IUnitCache : ICycleable
    {
        IWoWLocalPlayer Me { get; }
        IWoWUnit Target { get; }
        IWoWUnit Pet { get; }
        List<IWoWUnit> EnemyUnitsNearPlayer { get; }
        List<IWoWUnit> EnemyUnitsTargetingPlayer { get; }
        List<IWoWPlayer> GroupAndRaid { get; }
        Dictionary<int, List<IWoWPlayer>> Raid { get; }
        List<IWoWUnit> EnemiesFighting { get; }
        List<IWoWPlayer> TargetedByEnemies { get; }
        List<IWoWUnit> EnemiesAttackingMe { get; }
        List<IWoWPlayer> ClosePartyMembers { get; }

        IWoWUnit GetClosestHostileFrom(IWoWUnit target, float distance);
    }
}
