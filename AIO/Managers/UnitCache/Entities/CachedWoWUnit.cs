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
        public bool IsAlive { get; }
        public Vector3 PositionWithoutType { get; }
        public double HealthPercent { get; }
        public double ManaPercentage { get; }
        public double RagePercent { get; }
        public double FocusPercent { get; }
        public bool InCombatFlagOnly { get; }
        public UnitFlags UnitFlags { get; }
        public Dictionary<uint, IAura> Auras { get; }
        public WoWClass WowClass { get; }
        public float GetDistance { get; }
        public bool HasTarget { get; }
        public bool IsAttackable { get; }
        public ulong Target { get; }
        public WoWUnit TargetObject { get; }
        public bool IsTargetingMe { get; }
        public uint Energy { get; }
        public bool IsCast { get; }
        public WoWUnit WowUnit { get; }
        public uint Rage { get; }
        public uint Mana { get; }
        public uint Level { get; }
        public bool IsSwimming { get; }
        public bool IsStunned { get; }
        public string CreatureTypeTarget { get; }
        public uint GetBaseAddress { get; }
        public bool IsBoss { get; }
        public long MaxHealth { get; }
        public long Health { get; }
        public bool IsTapDenied { get; }
        public bool IsTaggedByOther { get; }
        public bool PlayerControlled { get; }
        public Reaction Reaction { get; }
        public bool IsElite { get; }

        public CachedWoWUnit(WoWUnit unit)
        {
            WowUnit = unit;
            Name = unit.Name;
            Guid = unit.Guid;
            TargetGuid = unit.Target;
            IsValid = unit.IsValid;
            IsDead = unit.IsDead;
            PositionWithoutType = unit.PositionWithoutType;
            HealthPercent = unit.HealthPercent;
            ManaPercentage = unit.ManaPercentage;
            RagePercent = unit.RagePercentage;
            FocusPercent = unit.FocusPercentage;
            InCombatFlagOnly = unit.InCombatFlagOnly;
            UnitFlags = unit.UnitFlags;
            WowClass = unit.WowClass;
            IsAlive = unit.IsAlive;
            GetDistance = unit.GetDistance;
            HasTarget = unit.HasTarget;
            IsAttackable = unit.IsAttackable;
            Target = unit.Target;
            TargetObject = unit.TargetObject;
            IsTargetingMe = unit.IsTargetingMe;
            Energy = unit.Energy;
            IsCast = unit.IsCast;
            Rage = unit.Rage;
            Mana = unit.Mana;
            Level = unit.Level;
            IsSwimming = unit.IsSwimming;
            IsStunned = unit.IsStunned;
            CreatureTypeTarget = unit.CreatureTypeTarget;
            GetBaseAddress = unit.GetBaseAddress;
            IsBoss = unit.IsBoss;
            MaxHealth = unit.MaxHealth;
            Health = unit.Health;
            IsTapDenied = unit.IsTapDenied;
            IsTaggedByOther = unit.IsTaggedByOther;
            PlayerControlled = unit.PlayerControlled;
            Reaction = unit.Reaction;
            IsElite = unit.IsElite;
            
            Dictionary<uint, IAura> auras = new Dictionary<uint, IAura>();
            
            foreach (var aura in BuffManager.GetAuras(unit.GetBaseAddress))
            {
                auras[aura.SpellId] = new CachedAura(aura);
            }
            Auras = auras;
        }

        public IWoWUnit GetTargetObject => new CachedWoWUnit(TargetObject);

        public bool HasAura(AIOSpell spell)
        {
            return Auras.ContainsKey(spell.MaxRankId);
        }

        public bool HasMyAura(AIOSpell spell)
        {
            if (Auras.TryGetValue(spell.MaxRankId, out IAura aura))
            {
                return aura.TimeLeft > 0;
            }
            return false;
        }

        public bool HasBuff(string spell)
        {
            foreach (KeyValuePair<uint, IAura> pair in Auras)
            {
                if (pair.Value.Name.StartsWith(spell))
                {
                    return true;
                }
            }
            return false;
        }

        public int BuffStacks(AIOSpell spell)
        {
            if (Auras.TryGetValue(spell.MaxRankId, out IAura aura))
            {
                return aura.Stacks;
            }
            return 0;
        }

        public bool IsFacing(Vector3 position, float arcRadians) => WowUnit.IsFacing(position, arcRadians);
    }
}
