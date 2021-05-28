using System.Collections.Generic;
using wManager.Wow.Class;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    class UnitImmunities
    {
        public static List<UnitImmunity> ListUnitImmunities { get; } = new List<UnitImmunity>();

        public static bool Contains(WoWUnit unit, string spellName)
        {
            return ListUnitImmunities.Exists(ei => ei.UnitGuid == unit.Guid && ei.SpellName == spellName);
        }

        public static void Add(WoWUnit unit, string spellName)
        {
            Spell spell = new Spell(spellName);
            if (!Contains(unit, spellName) && spell.KnownSpell)
            {
                Logger.Log($"{unit.Name} is immune to {spellName}. Banning this spell against this unit.");
                ListUnitImmunities.Add(new UnitImmunity(unit.Guid, spellName));
            }
        }

        public static void Clear()
        {
            ListUnitImmunities.Clear();
        }
    }

    public struct UnitImmunity
    {
        public UnitImmunity(ulong unitGuid, string spellName)
        {
            UnitGuid = unitGuid;
            SpellName = spellName;
        }
        public ulong UnitGuid { get; }
        public string SpellName { get; }
    }
}
