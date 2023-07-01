using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class ShamanSettings : BasePersistentSettings<ShamanSettings>
    {
        private const string _settingsTriggerName = "ShamanRotationTrigger";
        private const string _ENH_name = "Enhancement";
        private const string _ELE_name = "Elemental";
        private const string _PEN_name = "Party Enhancement";
        private const string _PRE_name = "Party Restoration";
        private const string _rotationTabName = "Rotation";
        private const string _totemsTabName = "Totems";

        public ShamanSettings()
        {
            // Elemental
            ELE_OOCHealThreshold = 60;
            ELE_HealThreshold = 50;
            ELE_GhostWolfMount = false;
            ELE_CurePoison = true;
            ELE_UseLightningShield = false;
            ELE_CureDisease = true;
            ELE_UseWaterShield = true;
            ELE_UseFlameShock = true;
            ELE_FrostShockHumanoids = true;
            ELE_ShockDPSMana = 50;
            ELE_LBHealthThreshold = 20;
            ELE_ChainLightningOnMulti = true;

            // Enhancement
            ENH_OOCHealThreshold = 60;
            ENH_HealThreshold = 50;
            ENH_GhostWolfMount = false;
            ENH_CurePoison = true;
            ENH_UseLightningShield = false;
            ENH_CureDisease = true;
            ENH_UseWaterShield = true;
            ENH_UseFlameShock = true;
            ENH_PullRankOneLightningBolt = true;
            ENH_InterruptWithRankOne = false;
            ENH_FrostShockHumanoids = true;
            ENH_AlwaysPullWithLightningBolt = true;
            ENH_ENShamanisticRageOnMultiOnly = true;

            // Party Enhancement
            PEN_OOCHealThreshold = 60;
            PEN_GhostWolfMount = false;
            PEN_UseLightningShield = false;
            PEN_CureDisease = true;
            PEN_UseWaterShield = true;
            PEN_StandBehind = true;
            PEN_CurePoison = false;

            // Party Resto
            PRE_GhostWolfMount = false;
            PRE_UseLightningShield = false;
            PRE_UseWaterShield = true;
            PRE_LesserHealingWaveThreshold = 80;
            PRE_HealingWaveThreshold = 60;
            PRE_ChainHealAmount = 3;
            PRE_ChainHealThreshold = 80;
            PRE_CurePoison = false;
            PRE_CureDisease = false;

            // Totems
            UseTotemicCall = true;
            UseAirTotems = true;
            UseEarthTotems = true;
            UseFireTotems = true;
            UseWaterTotems = true;
            UseMagmaTotem = false;
            UseStoneSkinTotem = false;
            UseTotemOfWrath = true;

            Specialization = _ENH_name;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _ENH_name, _ELE_name, _PEN_name, _PRE_name })]
        public override string Specialization { get; set; }

        // Enhancement
        [Category(_rotationTabName)]
        [DefaultValue(60)]
        [DisplayName("OOC Heal Threshold")]
        [Description("Heal when out of combat and under this HP percentage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        [Percentage(true)]
        public int ENH_OOCHealThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(50)]
        [DisplayName("Heal Threshold")]
        [Description("Heal when in combat and under this HP percentage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        [Percentage(true)]
        public int ENH_HealThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Ghost Wolf Mount")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        [Description("Use Ghost Wolf as mount if you don't have a mount yet")]
        public bool ENH_GhostWolfMount { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cure poison")]
        [Description("Use Cure poison")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_CurePoison { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Lightning Shield")]
        [Description("Use Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_UseLightningShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cure disease")]
        [Description("Use Cure disease")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_CureDisease { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Water Shield")]
        [Description("Prioritize Water Shield over Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_UseWaterShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Flame Shock")]
        [Description("Use Flame Shock instead of Earth Shock for DPS")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_UseFlameShock { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Pull with LB1")]
        [Description("Use rank 1 Lightning Bolt to pull enemies (saves mana)")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_PullRankOneLightningBolt { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Interrupt ES1")]
        [Description("Use rank 1 Earth Shock to interrupt enemy casting")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_InterruptWithRankOne { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Frost Shock")]
        [Description("Use Frost Shock on low HP humanoids to keep them from fleeing")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_FrostShockHumanoids { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Always pull")]
        [Description("Always use Lightning Bolt to pull enemies")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_AlwaysPullWithLightningBolt { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("SR on multi")]
        [Description("Save Shamanistic Rage for multi aggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ENH_name)]
        public bool ENH_ENShamanisticRageOnMultiOnly { get; set; }

        // Elemental
        [Category(_rotationTabName)]
        [DefaultValue(60)]
        [DisplayName("OOC Heal Threshold")]
        [Description("Heal when out of combat and under this HP percentage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        [Percentage(true)]
        public int ELE_OOCHealThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(50)]
        [DisplayName("Heal Threshold")]
        [Description("Heal when in combat and under this HP percentage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        [Percentage(true)]
        public int ELE_HealThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Ghost Wolf Mount")]
        [Description("Use Ghost Wolf as mount if you don't have a mount yet")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_GhostWolfMount { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cure poison")]
        [Description("Use Cure poison")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_CurePoison { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Lightning Shield")]
        [Description("Use Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_UseLightningShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cure disease")]
        [Description("Use Cure disease")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_CureDisease { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Water Shield")]
        [Description("Prioritize Water Shield over Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_UseWaterShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Flame Shock")]
        [Description("Use Flame Shock instead of Earth Shock for DPS")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_UseFlameShock { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Frost Shock")]
        [Description("Use Frost Shock on low HP humanoids to keep them from fleeing")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_FrostShockHumanoids { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(50)]
        [DisplayName("Shock DPS Mana")]
        [Description("Minimum mana percentage to use Shock DPS")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        [Percentage(true)]
        public int ELE_ShockDPSMana { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(20)]
        [DisplayName("LB Health Threshold")]
        [Description("Only cast Lightning Bolt if enemy is over this percentage of health")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        [Percentage(true)]
        public int ELE_LBHealthThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("CL on multi")]
        [Description("Use Chain Lightning on multi aggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _ELE_name)]
        public bool ELE_ChainLightningOnMulti { get; set; }

        // Party Enhancement
        [Category(_rotationTabName)]
        [DefaultValue(60)]
        [DisplayName("OOC Heal Threshold")]
        [Description("Heal when out of combat and under this HP percentage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PEN_name)]
        [Percentage(true)]
        public int PEN_OOCHealThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Ghost Wolf Mount")]
        [Description("Use Ghost Wolf as mount if you don't have a mount yet")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PEN_name)]
        public bool PEN_GhostWolfMount { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Lightning Shield")]
        [Description("Use Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PEN_name)]
        public bool PEN_UseLightningShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cure disease")]
        [Description("Use Cure disease")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PEN_name)]
        public bool PEN_CureDisease { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Water Shield")]
        [Description("Prioritize Water Shield over Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PEN_name)]
        public bool PEN_UseWaterShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Stand behind")]
        [Description("Try to stand behind the target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PEN_name)]
        public bool PEN_StandBehind { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cure Poison")]
        [Description("Use Cure Poison in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PEN_name)]
        public bool PEN_CurePoison { get; set; }

        // Party Resto
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Ghost Wolf Mount")]
        [Description("Use Ghost Wolf as mount if you don't have a mount yet")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        public bool PRE_GhostWolfMount { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Lightning Shield")]
        [Description("Use Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        public bool PRE_UseLightningShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Water Shield")]
        [Description("Prioritize Water Shield over Lightning Shield")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        public bool PRE_UseWaterShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Lesser H. Wave")]
        [Description("Use Lesser Healing Wave when ally has less HP than this threshold")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        [Percentage(true)]
        public int PRE_LesserHealingWaveThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(60)]
        [DisplayName("Healing Wave")]
        [Description("Use Healing Wave when ally has less HP than this threshold")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        [Percentage(true)]
        public int PRE_HealingWaveThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(3)]
        [DisplayName("Chain Heal count")]
        [Description("Use Chain Heal when at least this amount of allies has less HP than Chain Heal threshold")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        public int PRE_ChainHealAmount { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Chain Heal")]
        [Description("Use Chain Heal when allies has less HP than this threshold")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        [Percentage(true)]
        public int PRE_ChainHealThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cure Poison")]
        [Description("Use Cure Poison in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        public bool PRE_CurePoison { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cure Disease")]
        [Description("Use Cure Disease in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _PRE_name)]
        public bool PRE_CureDisease { get; set; }

        // TOTEMS
        [Category(_totemsTabName)]
        [DefaultValue(true)]
        [DisplayName("Totemic Call")]
        [Description("Use Totemic Call")]
        public bool UseTotemicCall { get; set; }
        [Category(_totemsTabName)]
        [DefaultValue(false)]
        [DisplayName("Stoneskin Totem")]
        [Description("Use Stoneskin Totem instead of Strength of Earth Totem")]
        public bool UseStoneSkinTotem { get; set; }
        [Category(_totemsTabName)]
        [DefaultValue(true)]
        [DisplayName("Fire totems")]
        [Description("Use Fire totems")]
        public bool UseFireTotems { get; set; }
        [Category(_totemsTabName)]
        [DefaultValue(true)]
        [DisplayName("Air totems")]
        [Description("Use Air totems")]
        public bool UseAirTotems { get; set; }
        [Category(_totemsTabName)]
        [DefaultValue(true)]
        [DisplayName("Water totems")]
        [Description("Use Water totems")]
        public bool UseWaterTotems { get; set; }
        [Category(_totemsTabName)]
        [DefaultValue(true)]
        [DisplayName("Earth totems")]
        [Description("Use Earth totems")]
        public bool UseEarthTotems { get; set; }
        [Category(_totemsTabName)]
        [DefaultValue(false)]
        [DisplayName("Magma Totem")]
        [Description("Use Magma Totem on multi aggro")]
        public bool UseMagmaTotem { get; set; }
        [Category(_totemsTabName)]
        [DefaultValue(true)]
        [DisplayName("Totem of Wrath")]
        [Description("Use Totem of Wrath")]
        public bool UseTotemOfWrath { get; set; }
    }
}