using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class ShamanSettings : BasePersistentSettings<ShamanSettings>
    {
        public ShamanSettings()
        {
            UseLightningShield = false;
            UseTotemicCall = true;
            UseFlameShock = true;
            UseWaterShield = true;
            OOCHealThreshold = 60;
            HealThreshold = 50;
            CurePoison = true;
            CureDisease = true;
            GhostWolfMount = false;

            ENPullRankOneLightningBolt = true;
            ENInterruptWithRankOne = false;
            ENShamanisticRageOnMultiOnly = true;
            ENFrostShockHumanoids = true;
            ENAlwaysPullWithLightningBolt = true;

            ELShockDPSMana = 50;
            ELLBHealthThreshold = 20;
            ELChainLightningOnMulti = true;

            UseAirTotems = true;
            UseEarthTotems = true;
            UseFireTotems = true;
            UseWaterTotems = true;
            UseMagmaTotem = false;
            UseStoneSkinTotem = false;
            UseTotemOfWrath = true;

            PartyStandBehind = true;
            PartyLesserHealingWaveThreshold = 80;
            PartyHealingWaveThreshold = 60;
            PartyChainHealAmount = 3;
            PartyChainHealThreshold = 80;

            Specialization = "Enhancement";
        }

        // COMMON

        [Category("Common")]
        [DefaultValue(60)]
        [DisplayName("OOC Heal Threshold")]
        [Description("Heal when out of combat and under this HP percentage")]
        [Percentage(true)]
        public int OOCHealThreshold { get; set; }

        [Category("Common")]
        [DefaultValue(50)]
        [DisplayName("Heal Threshold")]
        [Description("Heal when in combat and under this HP percentage")]
        [Percentage(true)]
        public int HealThreshold { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Ghost Wolf Mount")]
        [Description("Use Ghost Wolf as mount if you don't have a mount yet")]
        public bool GhostWolfMount { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Cure poison")]
        [Description("Use Cure poison")]
        public bool CurePoison { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Cure disease")]
        [Description("Use Cure disease")]
        public bool CureDisease { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Totemic Call")]
        [Description("Use Totemic Call")]
        public bool UseTotemicCall { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Lightning Shield")]
        [Description("Use Lightning Shield")]
        public bool UseLightningShield { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Water Shield")]
        [Description("Prioritize Water Shield over Lightning Shield")]
        public bool UseWaterShield { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Flame Shock")]
        [Description("Use Flame Shock instead of Earth Shock for DPS")]
        public bool UseFlameShock { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Frost Shock")]
        [Description("Use Frost Shock on low HP humanoids to keep them from fleeing")]
        public bool ENFrostShockHumanoids { get; set; }

        // ENHANCEMENT
        [Category("Enhancement")]
        [DefaultValue(true)]
        [DisplayName("Always pull")]
        [Description("Always use Lightning Bolt to pull enemies")]
        public bool ENAlwaysPullWithLightningBolt { get; set; }

        [Category("Enhancement")]
        [DefaultValue(true)]
        [DisplayName("Pull with LB1")]
        [Description("Use rank 1 Lightning Bolt to pull enemies (saves mana)")]
        public bool ENPullRankOneLightningBolt { get; set; }

        [Category("Enhancement")]
        [DefaultValue(false)]
        [DisplayName("Interrupt ES1")]
        [Description("Use rank 1 Earth Shock to interrupt enemy casting")]
        public bool ENInterruptWithRankOne { get; set; }

        [Category("Enhancement")]
        [DefaultValue(true)]
        [DisplayName("SR on multi")]
        [Description("Save Shamanistic Rage for multi aggro")]
        public bool ENShamanisticRageOnMultiOnly { get; set; }

        // TOTEMS
        [Category("Totems")]
        [DefaultValue(false)]
        [DisplayName("Stoneskin Totem")]
        [Description("Use Stoneskin Totem instead of Strength of Earth Totem")]
        public bool UseStoneSkinTotem { get; set; }

        [Category("Totems")]
        [DefaultValue(true)]
        [DisplayName("Fire totems")]
        [Description("Use Fire totems")]
        public bool UseFireTotems { get; set; }

        [Category("Totems")]
        [DefaultValue(true)]
        [DisplayName("Air totems")]
        [Description("Use Air totems")]
        public bool UseAirTotems { get; set; }

        [Category("Totems")]
        [DefaultValue(true)]
        [DisplayName("Water totems")]
        [Description("Use Water totems")]
        public bool UseWaterTotems { get; set; }

        [Category("Totems")]
        [DefaultValue(true)]
        [DisplayName("Earth totems")]
        [Description("Use Earth totems")]
        public bool UseEarthTotems { get; set; }

        [Category("Totems")]
        [DefaultValue(false)]
        [DisplayName("Magma Totem")]
        [Description("Use Magma Totem on multi aggro")]
        public bool UseMagmaTotem { get; set; }

        [Category("Totems")]
        [DefaultValue(true)]
        [DisplayName("Totem of Wrath")]
        [Description("Use Totem of Wrath")]
        public bool UseTotemOfWrath { get; set; }

        // ELEMENTAL
        [Category("Elemental")]
        [DefaultValue(50)]
        [DisplayName("Shock DPS Mana")]
        [Description("Minimum mana percentage to use Shock DPS")]
        [Percentage(true)]
        public int ELShockDPSMana { get; set; }

        [Category("Elemental")]
        [DefaultValue(20)]
        [DisplayName("LB Health Threshold")]
        [Description("Only cast Lightning Bolt if enemy is over this percentage of health")]
        [Percentage(true)]
        public int ELLBHealthThreshold { get; set; }

        [Category("Elemental")]
        [DefaultValue(true)]
        [DisplayName("CL on multi")]
        [Description("Use Chain Lightning on multi aggro")]
        public bool ELChainLightningOnMulti { get; set; }

        // PARTY
        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[ENH] Stand behind")]
        [Description("Try to stand behind the target in Enhancement")]
        public bool PartyStandBehind { get; set; }

        [Category("Party")]
        [DefaultValue(80)]
        [DisplayName("[RES] Lesser H. Wave")]
        [Description("Use Lesser Healing Wave when ally has less HP than this threshold")]
        [Percentage(true)]
        public int PartyLesserHealingWaveThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(60)]
        [DisplayName("[RES] Healing Wave")]
        [Description("Use Healing Wave when ally has less HP than this threshold")]
        [Percentage(true)]
        public int PartyHealingWaveThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(3)]
        [DisplayName("[RES] Chain Heal count")]
        [Description("Use Chain Heal when at least this amount of allies has less HP than Chain Heal threshold")]
        public int PartyChainHealAmount { get; set; }

        [Category("Party")]
        [DefaultValue(80)]
        [DisplayName("[RES] Chain Heal")]
        [Description("Use Chain Heal when allies has less HP than this threshold")]
        [Percentage(true)]
        public int PartyChainHealThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Cure Poison")]
        [Description("Use Cure Poison in combat")]
        public bool PartyCurePoison { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Cure Disease")]
        [Description("Use Cure Disease in combat")]
        public bool PartyCureDisease { get; set; }

        // TALENT
        [DropdownList(new string[] { "Enhancement", "Elemental", "Party Enhancement", "Party Restoration" })]
        public override string Specialization { get; set; }
    }
}