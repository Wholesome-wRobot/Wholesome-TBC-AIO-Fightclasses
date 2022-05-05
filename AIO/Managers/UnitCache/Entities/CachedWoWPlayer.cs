using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public class CachedWoWPlayer : CachedWoWUnit, IWoWPlayer
    {
        private WoWPlayer _player;
        public int ComboPoint => _player.ComboPoint;

        public CachedWoWPlayer(WoWPlayer player) : base(player)
        {
            _player = player;
        }
    }
}
