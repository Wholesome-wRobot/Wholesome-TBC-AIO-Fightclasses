using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public class CachedWoWPlayer : CachedWoWUnit, IWoWPlayer
    {
        public int ComboPoint { get; }

        public CachedWoWPlayer(WoWPlayer player) : base(player)
        {
            ComboPoint = player.ComboPoint;
        }
    }
}
