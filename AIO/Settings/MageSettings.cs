using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class MageSettings : BasePersistentSettings<MageSettings>
    {
        private const string _settingsTriggerName = "MageRotationTrigger";
        private const string _soloFrostName = "Frost";
        private const string _partyFrostName = "Party Frost";
        private const string _soloFireName = "Fire";
        private const string _partyFireName = "Party Fire";
        private const string _soloArcaneName = "Arcane";
        private const string _partyArcaneName = "Party Arcane";

        private const string _rotationTabName = "Rotation";
        private const string _commonTabName = "Common";

        public MageSettings()
        {
            // Frost
            SFRO_UsePolymorph = true;
            SFRO_WandThreshold = 30;
            SFRO_UseConeOfCold = true;
            SFRO_IcyVeinMultiPull = true;
            SFRO_FireblastThreshold = 30;

            // Fire
            SFIR_UsePolymorph = true;
            SFIR_WandThreshold = 30;
            SFIR_UseConeOfCold = true;
            SFIR_FireblastThreshold = 30;
            SFIR_UseDragonsBreath = true;
            SFIR_BlastWaveOnMulti = true;

            // Arcane
            SARC_UsePolymorph = true;
            SARC_WandThreshold = 30;
            SARC_UseConeOfCold = true;
            SARC_FireblastThreshold = 30;
            SARC_ACMageArmor = true;
            SARC_ACSlow = true;
            SARC_ArcanePowerOnMulti = false;
            SARC_PoMOnMulti = false;

            // Party Frost
            PFRO_PartyRemoveCurse = false;

            // Party Fire
            PFIR_PartyRemoveCurse = false;

            // Party Arcane
            PARC_ACMageArmor = true;
            PARC_ACSlow = true;
            PARC_PartyRemoveCurse = false;

            // Common
            MCOM_UseDampenMagic = true;
            MCOM_BackupUsingCTM = true;
            MCOM_BlinkWhenBackup = true;
            MCOM_UseCounterspell = true;

            Specialization = _soloFrostName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloFrostName, _soloFireName, _soloArcaneName, _partyFrostName, _partyFireName, _partyArcaneName })]
        public override string Specialization { get; set; }

        // FROST
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Polymorph")]
        [Description("Use Polymorph on multiaggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFrostName)]
        public bool SFRO_UsePolymorph { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(30)]
        [DisplayName("Wand Threshold")]
        [Description("Enemy HP % under which the wand should be used")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFrostName)]
        public int SFRO_WandThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Cone of Cold")]
        [Description("Use Cone of Cold during the combat rotation")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFrostName)]
        public bool SFRO_UseConeOfCold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Icy Veins on multi")]
        [Description("Only use Icy Veins when 2 or more enemy are pulled")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFrostName)]
        public bool SFRO_IcyVeinMultiPull { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(30)]
        [DisplayName("Fire Blast Threshold")]
        [Description("Enemy HP % under which Fire Blast should be used")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFrostName)]
        public int SFRO_FireblastThreshold { get; set; }

        // FIRE
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Polymorph")]
        [Description("Use Polymorph on multiaggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFireName)]
        public bool SFIR_UsePolymorph { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(30)]
        [DisplayName("Wand Threshold")]
        [Description("Enemy HP % under which the wand should be used")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFireName)]
        public int SFIR_WandThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Cone of Cold")]
        [Description("Use Cone of Cold during the combat rotation")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFireName)]
        public bool SFIR_UseConeOfCold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(30)]
        [DisplayName("Fire Blast Threshold")]
        [Description("Enemy HP % under which Fire Blast should be used")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFireName)]
        public int SFIR_FireblastThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Dragon's Breath")]
        [Description("Use Dragon's Breath")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFireName)]
        public bool SFIR_UseDragonsBreath { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Blast Wave on multi")]
        [Description("Use Blast Wave on multipull")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFireName)]
        public bool SFIR_BlastWaveOnMulti { get; set; }

        // ARCANE
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Polymorph")]
        [Description("Use Polymorph on multiaggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public bool SARC_UsePolymorph { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(30)]
        [DisplayName("Wand Threshold")]
        [Description("Enemy HP % under which the wand should be used")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public int SARC_WandThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Cone of Cold")]
        [Description("Use Cone of Cold during the combat rotation")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public bool SARC_UseConeOfCold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(30)]
        [DisplayName("Fire Blast Threshold")]
        [Description("Enemy HP % under which Fire Blast should be used")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public int SARC_FireblastThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Mage Armor")]
        [Description("Use Mage Armor instead of Frost/Ice Armor")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public bool SARC_ACMageArmor { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Slow")]
        [Description("Use Slow")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public bool SARC_ACSlow { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("AP on multi")]
        [Description("Use Arcane Power on multipull only")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public bool SARC_ArcanePowerOnMulti { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("PoM on multi")]
        [Description("Use Presence of Mind on multipull only")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloArcaneName)]
        public bool SARC_PoMOnMulti { get; set; }

        // PARTY FROST
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Remove Curse")]
        [Description("Use Remove Curse in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFrostName)]
        public bool PFRO_PartyRemoveCurse { get; set; }

        // PARTY FIRE
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Remove Curse")]
        [Description("Use Remove Curse in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFireName)]
        public bool PFIR_PartyRemoveCurse { get; set; }

        // PARTY ARCANE
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Mage Armor")]
        [Description("Use Mage Armor instead of Frost/Ice Armor")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyArcaneName)]
        public bool PARC_ACMageArmor { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Slow")]
        [Description("Use Slow")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyArcaneName)]
        public bool PARC_ACSlow { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Remove Curse")]
        [Description("Use Remove Curse in combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyArcaneName)]
        public bool PARC_PartyRemoveCurse { get; set; }

        // COMMON

        [Category(_commonTabName)]
        [DefaultValue(true)]
        [DisplayName("Counterspell")]
        [Description("Use Counterspell")]
        public bool MCOM_UseCounterspell { get; set; }

        [Category(_commonTabName)]
        [DefaultValue(true)]
        [DisplayName("Dampen Magic")]
        [Description("Use Dampen Magic")]
        public bool MCOM_UseDampenMagic { get; set; }

        [Category(_commonTabName)]
        [DefaultValue(true)]
        [DisplayName("Blink when backup")]
        [Description("Use Blink when backing up from the target")]
        public bool MCOM_BlinkWhenBackup { get; set; }

        [Category(_commonTabName)]
        [DefaultValue(true)]
        [DisplayName("Backup using CTM")]
        [Description("If set to True, will backup using Click To Move. If false, will use the keyboard")]
        public bool MCOM_BackupUsingCTM { get; set; }
    }
}