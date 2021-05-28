using System.Collections.Generic;

namespace WholesomeTBCAIO.Helpers
{
    public class Enums
    {
        public static Dictionary<string, Specs> GetSpecDictionary()
        {
            switch (Main.wowClass)
            {
                case "Shaman": return ShamanSpecs;
                case "Druid": return DruidSpecs;
                case "Hunter": return HunterSpecs;
                case "Mage": return MageSpecs;
                case "Paladin": return PaladinSpecs;
                case "Priest": return PriestSpecs;
                case "Rogue": return RogueSpecs;
                case "Warlock": return WarlockSpecs;
                case "Warrior": return WarriorSpecs;
                default: return null;
            }
        }

        private readonly static Dictionary<string, Specs> DruidSpecs = new Dictionary<string, Specs>()
        {
            { "Feral", Specs.DruidFeral },
            { "Party Feral DPS", Specs.DruidFeralDPSParty },
            { "Party Feral Tank", Specs.DruidFeralTankParty },
            { "Party Restoration", Specs.DruidRestorationParty },
        };
        private readonly static Dictionary<string, Specs> HunterSpecs = new Dictionary<string, Specs>()
        {
            { "BeastMaster", Specs.HunterBeastMaster },
            { "Party BeastMaster", Specs.HunterBeastMasterParty },
        };
        private readonly static Dictionary<string, Specs> MageSpecs = new Dictionary<string, Specs>()
        {
            { "Frost", Specs.MageFrost },
            { "Party Frost", Specs.MageFrostParty },
            { "Arcane", Specs.MageArcane },
            { "Party Arcane", Specs.MageArcaneParty },
            { "Fire", Specs.MageFire },
            { "Party Fire", Specs.MageFireParty },
        };
        private readonly static Dictionary<string, Specs> PaladinSpecs = new Dictionary<string, Specs>()
        {
            { "Retribution", Specs.PaladinRetribution },
            { "Party Holy", Specs.PaladinHolyParty },
            { "Party Protection", Specs.PaladinProtectionParty },
            { "Party Retribution", Specs.PaladinRetributionParty },
        };
        private readonly static Dictionary<string, Specs> PriestSpecs = new Dictionary<string, Specs>()
        {
            { "Shadow", Specs.PriestShadow },
            { "Party Shadow", Specs.PriestShadowParty },
            { "Party Holy", Specs.PriestHolyParty },
        };
        private readonly static Dictionary<string, Specs> RogueSpecs = new Dictionary<string, Specs>()
        {
            { "Combat", Specs.RogueCombat },
            { "Party Combat", Specs.RogueCombatParty },
        };
        private readonly static Dictionary<string, Specs> ShamanSpecs = new Dictionary<string, Specs>()
        {
            { "Enhancement", Specs.ShamanEnhancement },
            { "Elemental", Specs.ShamanElemental },
            { "Party Enhancement", Specs.ShamanEnhancementParty },
            { "Party Restoration", Specs.ShamanRestoParty },
        };
        private readonly static Dictionary<string, Specs> WarlockSpecs = new Dictionary<string, Specs>()
        {
            { "Affliction", Specs.WarlockAffliction },
            { "Demonology", Specs.WarlockDemonology },
            { "Party Affliction", Specs.WarlockAfflictionParty },
        };
        private readonly static Dictionary<string, Specs> WarriorSpecs = new Dictionary<string, Specs>()
        {
            { "Fury", Specs.WarriorFury },
            { "Party Fury", Specs.WarriorFuryParty },
            { "Party Protection", Specs.WarriorProtectionParty },
        };

        public enum Specs
        {
            // Shaman
            ShamanEnhancement,
            ShamanEnhancementParty,
            ShamanElemental,
            ShamanRestoParty,
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
            PaladinRetributionParty,
            PaladinHolyParty,
            PaladinProtectionParty,
            // Priest
            PriestShadow,
            PriestShadowParty,
            PriestHolyParty,
            // Rogue
            RogueCombat,
            RogueCombatParty,
            // Warlock
            WarlockAffliction,
            WarlockDemonology,
            WarlockAfflictionParty,
            // Warrior
            WarriorFury,
            WarriorFuryParty,
            WarriorProtectionParty
        }

        public enum RotationType
        {
            None,
            Solo,
            Party
        }

        public enum RotationRole
        {
            None,
            DPS,
            Tank,
            Healer
        }
    }
}
