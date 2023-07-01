using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class PriestSettings : BasePersistentSettings<PriestSettings>
    {
        private const string _settingsTriggerName = "PriestRotationTrigger";
        private const string _soloShadowName = "Shadow";
        private const string _partyShadowName = "Party Shadow";
        private const string _partyHolyName = "Party Holy";
        private const string _raidHolyName = "Raid Holy";
        private const string _rotationTabName = "Rotation";

        public PriestSettings()
        {
            // Solo Shadow
            SSH_WandThreshold = 40;
            SSH_UsePowerWordShield = true;
            SSH_UseShieldOnPull = true;
            SSH_UseShadowGuard = true;
            SSH_UseInnerFire = true;
            SSH_UsePowerWordFortitude = true;
            SSH_UseShadowProtection = false;
            SSH_UseDivineSpirit = false;
            SSH_DevouringPlagueThreshold = 80;
            SSH_UseShadowWordDeath = true;
            SSH_DispelMagic = false;
            SSH_CureDisease = false;

            // Party Shadow
            PSH_UsePowerWordShield = true;
            PSH_UseShadowGuard = true;
            PSH_UseInnerFire = true;
            PSH_UsePowerWordFortitude = true;
            PSH_UseShadowProtection = false;
            PSH_UseDivineSpirit = false;
            PSH_DispelMagic = false;
            PSH_CureDisease = false;
            PSH_VampiricEmbrace = false;
            PSH_SWDeathThreshold = 90;
            PSH_MindBlastThreshold = 70;

            // Party Holy
            PHO_UseInnerFire = true;
            PHO_UsePowerWordFortitude = true;
            PHO_UseShadowProtection = false;
            PHO_UseDivineSpirit = false;
            PHO_DispelMagic = false;
            PHO_CureDisease = false;

            // Raid Holy
            RHO_UsePowerWordShield = true;
            RHO_UseInnerFire = true;
            RHO_UsePowerWordFortitude = true;
            RHO_UseShadowProtection = false;
            RHO_UseDivineSpirit = false;
            RHO_DispelMagic = false;
            RHO_CureDisease = false;
            RHO_MassDispel = false;
            RHO_MassDispelCount = 5;
            RHO_CircleOfHealingThreshold = 90;
            RHO_CircleofHealingRadius = 18;
            RHO_TankHealingPriority = 0;
            RHO_KeepRenewOnTank = false;
            RHO_PrayerOfFortitude = false;
            RHO_PrayerOfShadowProtection = false;
            RHO_PrayerOfSpirit = false;

            Specialization = _soloShadowName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloShadowName, _partyShadowName, _partyHolyName, _raidHolyName })]
        public override string Specialization { get; set; }

        // SOLO SHADOW
        [Category(_rotationTabName)]
        [DefaultValue(40)]
        [DisplayName("Wand Threshold")]
        [Description("Use wand when the enemy HP goes under this percentage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        [Percentage(true)]
        public int SSH_WandThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Power Word: Shield")]
        [Description("Use Power Word: Shield on yourself")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UsePowerWordShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Shield on pull")]
        [Description("Use Power Word: Shield on pull")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UseShieldOnPull { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Shadowguard")]
        [Description("Use Shadowguard")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UseShadowGuard { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Inner Fire")]
        [Description("Use Inner Fire")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UseInnerFire { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Power Word: Fortitude")]
        [Description("Use Power Word: Fortitude")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UsePowerWordFortitude { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Shadow Protection")]
        [Description("Use Shadow Protection")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UseShadowProtection { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Divine Spirit")]
        [Description("Use Divine Spirit")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UseDivineSpirit { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Devouring Plague")]
        [Description("Enemy HP over which Devouring Plague should be used")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        [Percentage(true)]
        public int SSH_DevouringPlagueThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("SW: Death")]
        [Description("Use Shadow Word: Death")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_UseShadowWordDeath { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Dispel Magic")]
        [Description("Use Dispel Magic")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_DispelMagic { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cure Disease")]
        [Description("Use Cure Disease")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloShadowName)]
        public bool SSH_CureDisease { get; set; }

        // PARTY SHADOW
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Power Word: Shield")]
        [Description("Use Power Word: Shield on yourself")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_UsePowerWordShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Shadowguard")]
        [Description("Use Shadowguard")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_UseShadowGuard { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Inner Fire")]
        [Description("Use Inner Fire")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_UseInnerFire { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Power Word: Fortitude")]
        [Description("Use Power Word: Fortitude")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_UsePowerWordFortitude { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Shadow Protection")]
        [Description("Use Shadow Protection")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_UseShadowProtection { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Divine Spirit")]
        [Description("Use Divine Spirit")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_UseDivineSpirit { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Dispel Magic")]
        [Description("Use Dispel Magic")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_DispelMagic { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cure Disease")]
        [Description("Use Cure Disease")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_CureDisease { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Vampiric Embrace")]
        [Description("Use Vampiric Embrace in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        public bool PSH_VampiricEmbrace { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(90)]
        [DisplayName("SW: Death")]
        [Description("Use Shadow Word: Death when you are above this HEALTH percentage threshold (100 to disable)")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        [Percentage(true)]
        public int PSH_SWDeathThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(70)]
        [DisplayName("Mind Blast")]
        [Description("Use Mind Blast when above this MANA percentage threshold (100 to disable)")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyShadowName)]
        [Percentage(true)]
        public int PSH_MindBlastThreshold { get; set; }

        // PARTY HOLY
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Inner Fire")]
        [Description("Use Inner Fire")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_UseInnerFire { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Power Word: Fortitude")]
        [Description("Use Power Word: Fortitude")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_UsePowerWordFortitude { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Shadow Protection")]
        [Description("Use Shadow Protection")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_UseShadowProtection { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Divine Spirit")]
        [Description("Use Divine Spirit")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_UseDivineSpirit { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Dispel Magic")]
        [Description("Use Dispel Magic")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_DispelMagic { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cure Disease")]
        [Description("Use Cure Disease")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_CureDisease { get; set; }

        // RAID HOLY
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Power Word: Shield")]
        [Description("Use Power Word: Shield on the group")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_UsePowerWordShield { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Inner Fire")]
        [Description("Use Inner Fire")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_UseInnerFire { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Power Word: Fortitude")]
        [Description("Use Power Word: Fortitude")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_UsePowerWordFortitude { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Shadow Protection")]
        [Description("Use Shadow Protection")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_UseShadowProtection { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Divine Spirit")]
        [Description("Use Divine Spirit")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_UseDivineSpirit { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Dispel Magic")]
        [Description("Use Dispel Magic")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_DispelMagic { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cure Disease")]
        [Description("Use Cure Disease")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_CureDisease { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Mass Dispel")]
        [Description("Use Mass Dispel in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_MassDispel { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(5)]
        [DisplayName("Mass Dispel Count")]
        [Description("Minimum number of group members with dispellable debuff to use Mass Dispel")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public int RHO_MassDispelCount { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(90)]
        [DisplayName("CoH Threshold")]
        [Description("Use Circle of Healing on party members under this health threshold")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        [Percentage(true)]
        public int RHO_CircleOfHealingThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(18)]
        [DisplayName("Circle of Healing Radius")]
        [Description("Healing radius of Circle of Healing")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public int RHO_CircleofHealingRadius { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(0)]
        [DisplayName("Tank healing priority")]
        [Description("Prefer healing tanks over other group members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        [Percentage(true)]
        public int RHO_TankHealingPriority { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Keep renew on tank")]
        [Description("Keep renew on tank")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_KeepRenewOnTank { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Prayer of Fortitude")]
        [Description("Use Prayer of Fortitude")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_PrayerOfFortitude { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Prayer of Shadow Protection")]
        [Description("Use Prayer of Shadow Protection")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_PrayerOfShadowProtection { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Prayer of Spirit")]
        [Description("Use Prayer of Spirit")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_PrayerOfSpirit { get; set; }
    }
}