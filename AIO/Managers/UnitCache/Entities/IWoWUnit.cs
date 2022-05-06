using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public interface IWoWUnit
    {
        WoWUnit WowUnit { get; }
        string Name { get; }
        ulong Guid { get; }
        ulong TargetGuid { get; }
        bool IsValid { get; }
        bool IsDead { get; }
        bool IsAlive { get; }
        Vector3 PositionWithoutType { get; }
        double HealthPercent { get; }
        double ManaPercentage { get; }
        double RagePercent { get; }
        double FocusPercent { get; }
        bool InCombatFlagOnly { get; }
        UnitFlags UnitFlags { get; }
        Dictionary<uint, IAura> Auras { get; }
        WoWClass WowClass { get; }
        float GetDistance { get; }
        bool IsAttackable { get; }
        ulong Target { get; }
        bool HasTarget { get; }
        bool IsTargetingMe { get; }
        uint Energy { get; }
        bool IsCast { get; }
        uint Rage { get; }
        uint Mana { get; }
        uint Level { get; }
        bool IsSwimming { get; }
        bool IsStunned { get; }
        string CreatureTypeTarget { get; }
        uint GetBaseAddress { get; }
        bool IsBoss { get; }
        long MaxHealth { get; }
        long Health { get; }
        bool IsTapDenied { get; }
        bool IsTaggedByOther { get; }
        bool PlayerControlled { get; }
        Reaction Reaction { get; }
        bool IsElite { get; }

        bool HasAura(AIOSpell spell);
        bool HasMyAura(AIOSpell spell);
        bool HasAura(string spell);
        int BuffStacks(AIOSpell spell);
        bool IsFacing(Vector3 position, float arcRadians);
        int AuraTimeLeft(AIOSpell spell);
    }
}
