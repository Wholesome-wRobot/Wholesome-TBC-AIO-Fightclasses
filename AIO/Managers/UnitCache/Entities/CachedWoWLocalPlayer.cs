using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public class CachedWoWLocalPlayer : CachedWoWPlayer, IWoWLocalPlayer
    {
        private readonly WoWLocalPlayer _wowLocalPlayer;

        public bool IsMounted { get; }
        public bool IsOnTaxi { get; }

        public CachedWoWLocalPlayer(WoWLocalPlayer player) : base(player)
        {
            IsMounted = player.IsMounted;
            IsOnTaxi = player.IsOnTaxi;
            _wowLocalPlayer = player;
        }

        public void SetFocus(ulong focusGuid) => _wowLocalPlayer.FocusGuid = focusGuid;
        public void SetTarget(ulong targetGuid) => _wowLocalPlayer.Target = targetGuid;
    }
}
