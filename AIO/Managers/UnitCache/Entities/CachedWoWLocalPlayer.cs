using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public class CachedWoWLocalPlayer : CachedWoWUnit, IWoWLocalPlayer
    {
        public bool IsMounted { get; }

        public CachedWoWLocalPlayer(WoWLocalPlayer player) : base(player)
        {
            IsMounted = player.IsMounted;
        }
    }
}
