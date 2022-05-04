namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public interface IWoWLocalPlayer : IWoWPlayer
    {
        bool IsMounted { get; }
        bool IsOnTaxi { get; }

        void SetFocus(ulong focusGuid);
        void SetTarget(ulong targetGuid);
    }
}
