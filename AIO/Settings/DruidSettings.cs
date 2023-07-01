using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class DruidSettings : BasePersistentSettings<DruidSettings>
    {
        private const string _settingsTriggerName = "DruidRotationTrigger";
        private const string _soloFeralName = "Feral";
        private const string _partyFeralName = "Party Feral DPS";
        private const string _partyTankName = "Party Feral Tank";
        private const string _partyRestoName = "Party Restoration";
        private const string _rotationTabName = "Rotation";

        public DruidSettings()
        {
            // Solo Feral
            SFER_AlwaysPull = false;
            SFER_UseBarkskin = true;
            SFER_UseInnervate = true;
            SFER_UseEnrage = true;
            SFER_UseTigersFury = true;
            SFER_StealthEngage = true;
            SFER_NumberOfAttackersBearForm = 2;

            // Party Feral
            PFER_UseInnervate = true;
            PFER_UseEnrage = true;
            PFER_StealthEngage = true;
            PFER_PartyUseRebirth = true;
            PFER_PartyAbolishPoison = true;
            PFER_PartyRemoveCurse = true;
            PFER_PartyTranquility = true;
            PFER_PartyStandBehind = true;

            // Party Tank
            PTANK_AlwaysPull = false;
            PTANK_UseInnervate = true;
            PTANK_UseEnrage = true;
            PTANK_PartyTankSwitchTarget = true;
            PTANK_PartyUseRebirth = true;
            PTANK_PartyAbolishPoison = true;
            PTANK_PartyRemoveCurse = true;
            PTANK_PartyTranquility = true;

            // Party Restoration
            PREST_UseInnervate = true;
            PREST_PartyUseRebirth = true;
            PREST_PartyAbolishPoison = true;
            PREST_PartyRemoveCurse = true;
            PREST_PartyTranquility = true;

            Specialization = _soloFeralName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloFeralName, _partyFeralName, _partyTankName, _partyRestoName })]
        public override string Specialization { get; set; }

        // SOLO FERAL
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always range pull")]
        [Description("Always pull with a range spell")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFeralName)]
        public bool SFER_AlwaysPull { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Barkskin")]
        [Description("Use Barkskin before healing in dangerous situations")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFeralName)]
        public bool SFER_UseBarkskin { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Innervate")]
        [Description("Use Innervate")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFeralName)]
        public bool SFER_UseInnervate { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Always use Bear Form Enrage")]
        [Description("Always use Enrage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFeralName)]
        public bool SFER_UseEnrage { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Tiger's Fury")]
        [Description("Use Tiger's Fury")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFeralName)]
        public bool SFER_UseTigersFury { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cat Stealth engage")]
        [Description("Try to engage fights using Prowl and going behind the target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFeralName)]
        public bool SFER_StealthEngage { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(2)]
        [DisplayName("Bear number of attackers")]
        [Description("Bear Form when the number of enemies attacking you is superior or equal to this value.")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFeralName)]
        public int SFER_NumberOfAttackersBearForm { get; set; }

        // PARTY FERAL
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Innervate")]
        [Description("Use Innervate on low mana team members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_UseInnervate { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Always use Bear Form Enrage")]
        [Description("Always use Enrage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_UseEnrage { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cat Stealth engage")]
        [Description("Try to engage fights using Prowl and going behind the target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_StealthEngage { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Rebirth")]
        [Description("Use Rebirth on dead team members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_PartyUseRebirth { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Abolish Poison")]
        [Description("Use Abolish Poison in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_PartyAbolishPoison { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Remove Curse")]
        [Description("Use Remove Curse in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_PartyRemoveCurse { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Tranquility")]
        [Description("Use Tranquility in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_PartyTranquility { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Stand behind")]
        [Description("Try to stand behind the target in Feral DPS")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFeralName)]
        public bool PFER_PartyStandBehind { get; set; }

        // PARTY TANK
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always range pull")]
        [Description("Always pull with a range spell")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_AlwaysPull { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Innervate")]
        [Description("Use Innervate on low mana team members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_UseInnervate { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Always use Bear Form Enrage")]
        [Description("Always use Enrage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_UseEnrage { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Switch target")]
        [Description("Switch targets to regain aggro when tanking")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_PartyTankSwitchTarget { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Rebirth")]
        [Description("Use Rebirth on dead team members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_PartyUseRebirth { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Abolish Poison")]
        [Description("Use Abolish Poison in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_PartyAbolishPoison { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Remove Curse")]
        [Description("Use Remove Curse in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_PartyRemoveCurse { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Tranquility")]
        [Description("Use Tranquility in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyTankName)]
        public bool PTANK_PartyTranquility { get; set; }

        // PARTY RESTO
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Innervate")]
        [Description("Use Innervate on low mana team members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRestoName)]
        public bool PREST_UseInnervate { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Rebirth")]
        [Description("Use Rebirth on dead team members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRestoName)]
        public bool PREST_PartyUseRebirth { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Abolish Poison")]
        [Description("Use Abolish Poison in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRestoName)]
        public bool PREST_PartyAbolishPoison { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Remove Curse")]
        [Description("Use Remove Curse in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRestoName)]
        public bool PREST_PartyRemoveCurse { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Party Tranquility")]
        [Description("Use Tranquility in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRestoName)]
        public bool PREST_PartyTranquility { get; set; }
    }
}