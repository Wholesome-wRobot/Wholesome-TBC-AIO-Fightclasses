﻿using robotManager.Helpful;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ulong Target { get; }
        public bool IsTargetingMe { get; }
        public uint Energy { get; }
        public bool IsCast { get; }
        public uint Rage { get; }
        public uint Mana { get; }
        public uint Level { get; }
        public bool IsSwimming { get; }
        public bool IsStunned { get; }
        public uint GetBaseAddress { get; }
        public bool IsBoss { get; }
        public long MaxHealth { get; }
        public long Health { get; }
        public bool IsTapDenied { get; }
        public bool IsTaggedByOther { get; }
        public bool PlayerControlled { get; }
        public Reaction Reaction { get; }
        public bool IsElite { get; }
        public WoWUnit WowUnit { get; }

        public CachedWoWUnit(WoWUnit unit)
        {
            Stopwatch watch = Stopwatch.StartNew();
            Name = unit.Name;
            WowUnit = unit;
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
            Target = unit.Target;
            IsTargetingMe = unit.IsTargetingMe;
            Energy = unit.Energy;
            IsCast = unit.IsCast;
            Rage = unit.Rage;
            Mana = unit.Mana;
            Level = unit.Level;
            IsSwimming = unit.IsSwimming;
            IsStunned = unit.IsStunned;
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
            if (watch.ElapsedMilliseconds > 15)
                Logger.LogError($"{Name} update took {watch.ElapsedMilliseconds}");
        }

        public bool HasAura(AIOSpell spell)
        {
            return Auras.ContainsKey(spell.SpellId);
        }

        public bool HasAura(string spell) => WowUnit.HaveBuff(spell);

        public bool HasMyAura(AIOSpell spell) // only works for other players, player always returns true
        {
            if (Auras.TryGetValue(spell.SpellId, out IAura aura))
            {
                return aura.TimeLeft > 0;
            }
            return false;
        }

        public int AuraTimeLeft(AIOSpell spell)
        {
            if (Auras.TryGetValue(spell.SpellId, out IAura aura))
            {
                return aura.TimeLeft;
            }
            return 0;
        }

        public int BuffStacks(AIOSpell spell)
        {
            if (Auras.TryGetValue(spell.SpellId, out IAura aura))
            {
                return aura.Stacks;
            }
            return 0;
        }

        public bool IsFacing(Vector3 position, float arcRadians) => WowUnit.IsFacing(position, arcRadians);
        public string CreatureTypeTarget => WowUnit.CreatureTypeTarget; // slow call
        public bool IsAttackable => WowUnit.IsAttackable; // slow call
    }
}
