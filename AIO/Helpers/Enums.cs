using System.Collections.Generic;

namespace WholesomeTBCAIO.Helpers
{
    public class Enums
    {
        public readonly static Dictionary<string, Specs> SpecNames = new Dictionary<string, Specs>()
        {
            // Shaman
            { "Enhancement", Specs.ShamanEnhancement },
            { "Elemental", Specs.ShamanElemental },
            // Druid
            { "Feral", Specs.DruidFeral },
            { "Feral DPS Party", Specs.DruidFeralDPSParty },
            { "Feral Tank Party", Specs.DruidFeralTankParty },
            { "Restoration Party", Specs.DruidRestorationParty },
            // Hunter
            { "BeastMaster", Specs.HunterBeastMaster },
            { "BeastMaster Party", Specs.HunterBeastMasterParty },
            // Mage
            { "Frost", Specs.MageFrost },
            { "Frost Party", Specs.MageFrostParty },
            { "Arcane", Specs.MageArcane },
            { "Arcane Party", Specs.MageArcaneParty },
            { "Fire", Specs.MageFire },
            { "Fire Party", Specs.MageFireParty },
            // Paladin
            { "Retribution", Specs.PaladinRetribution },
            //Priest
            { "Shadow", Specs.PriestShadow },
            { "Shadow Party", Specs.PriestShadowParty },
            { "Holy Party", Specs.PriestHolyParty },
            // Rogue
            { "Combat", Specs.RogueCombat },
            // Warlock
            { "Affliction", Specs.WarlockAffliction },
            { "Demonology", Specs.WarlockDemonology },
            // Warrior
            { "Fury", Specs.WarriorFury },
            { "Fury Party", Specs.WarriorFuryParty },
            { "Protection Party", Specs.WarriorProtectionParty },
        };

        public enum Specs
        {
            // Shaman
            ShamanEnhancement,
            ShamanElemental,
            // Druid
            DruidFeral,
            DruidFeralDPSParty,
            DruidFeralTankParty,
            DruidRestorationParty,
            // Hunter
            HunterBeastMaster,
            HunterBeastMasterParty,
            // Mage
            MageFrost,
            MageFrostParty,
            MageArcane,
            MageArcaneParty,
            MageFire,
            MageFireParty,
            // Paladin
            PaladinRetribution,
            // Priest
            PriestShadow,
            PriestShadowParty,
            PriestHolyParty,
            // Rogue
            RogueCombat,
            // Warlock
            WarlockAffliction,
            WarlockDemonology,
            // Warrior
            WarriorFury,
            WarriorFuryParty,
            WarriorProtectionParty
        }

        public enum RotationType
        {
            Solo,
            Party
        }

        public enum RotationRole
        {
            DPS,
            Tank,
            Healer
        }
    }
}
