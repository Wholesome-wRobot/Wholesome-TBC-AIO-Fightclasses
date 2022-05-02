using System.Collections.Generic;
using WholesomeTBCAIO.Managers.UnitCache.Entities;

namespace WholesomeTBCAIO.Managers.UnitCache
{
    public interface IUnitCache : ICycleable
    {
        IWoWLocalPlayer Me { get; }
        IWoWUnit Target { get; }
        IWoWUnit Pet { get; }

        IWoWUnit[] EnemyUnitsNearPlayer { get; }
        IWoWUnit[] EnemyUnitsTargetingPlayer { get; }

        IWoWPlayer[] Group { get; }
        Dictionary<int, List<IWoWPlayer>> Raid { get; }
    }
}
