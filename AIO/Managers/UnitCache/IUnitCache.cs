using System.Collections.Generic;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache
{
    public interface IUnitCache : ICycleable
    {
        List<WoWUnit> UnitsTargetingMe { get; }
        List<WoWUnit> AllUnits { get; }
        List<WoWUnit> CloseUnitsTargetingMe { get; }
    }
}
