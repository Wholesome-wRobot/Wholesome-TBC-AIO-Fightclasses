using wManager.Wow.Enums;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public interface IWoWPlayer : IWoWUnit
    {
        int ComboPoint { get; }
    }
}
