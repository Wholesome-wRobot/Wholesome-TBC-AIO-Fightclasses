using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public class CachedWoWUnit : IWoWUnit
    {
        public string Name { get; }
        public ulong Guid { get; }
        public ulong TargetGuid { get; }
        public bool IsValid { get; }
        public bool IsDead { get; }
        public Vector3 PositionWithoutType { get; }
        public double HealthPercent { get; }
        public double ManaPercent { get; }
        public double RagePercent { get; }
        public double FocusPercent { get; }
        public bool InCombatFlagOnly { get; }
        public UnitFlags UnitFlags { get; }
        public IReadOnlyDictionary<uint, IAura> Auras { get; }
        public WoWClass WowClass { get; }

        public CachedWoWUnit(WoWUnit unit)
        {
            Name = unit.Name;
            Guid = unit.Guid;
            TargetGuid = unit.Target;
            IsValid = unit.IsValid;
            IsDead = unit.IsDead;
            PositionWithoutType = unit.PositionWithoutType;
            HealthPercent = unit.HealthPercent;
            ManaPercent = unit.ManaPercentage;
            RagePercent = unit.RagePercentage;
            FocusPercent = unit.FocusPercentage;
            InCombatFlagOnly = unit.InCombatFlagOnly;
            UnitFlags = unit.UnitFlags;
            WowClass = unit.WowClass;

            var auras = new Dictionary<uint, IAura>();
            foreach (var aura in BuffManager.GetAuras(unit.GetBaseAddress))
            {
                auras[aura.SpellId] = new CachedAura(aura);
            }
            Auras = auras;
        }

        public bool HasAura(AIOSpell spell) => Auras.ContainsKey(spell.MaxRankId);
        public bool HasMyAura(AIOSpell spell)
        {
            if (Auras.TryGetValue(spell.MaxRankId, out IAura aura))
            {
                return aura.TimeLeft > 0;
            }
            return false;
        }
    }
}
