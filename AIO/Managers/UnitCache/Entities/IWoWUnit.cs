using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Enums;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public interface IWoWUnit
    {
        string Name { get; }
        ulong Guid { get; }
        ulong TargetGuid { get; }
        bool IsValid { get; }
        bool IsDead { get; }
        Vector3 PositionWithoutType { get; }
        double HealthPercent { get; }
        double ManaPercent { get; }
        double RagePercent { get; }
        double FocusPercent { get; }
        bool InCombatFlagOnly { get; }
        UnitFlags UnitFlags { get; }
        IReadOnlyDictionary<uint, IAura> Auras { get; }
        WoWClass WowClass { get; }

        bool HasAura(AIOSpell spell);
        bool HasMyAura(AIOSpell spell);
    }
}
