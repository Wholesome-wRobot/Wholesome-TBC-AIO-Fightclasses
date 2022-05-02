using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public class CachedWoWPlayer : CachedWoWUnit, IWoWPlayer
    {
        public CachedWoWPlayer(WoWPlayer player) : base(player)
        {
        }
    }
}
