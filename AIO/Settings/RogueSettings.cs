using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class RogueSettings : BasePersistentSettings<RogueSettings>
    {
        private const string _settingsTriggerName = "RogueRotationTrigger";
        private const string _soloCombatName = "Combat";
        private const string _partyCombatName = "Party Combat";
        private const string _rotationTabName = "Rotation";

        public RogueSettings()
        {
            // Combat
            SC_AlwaysPull = false;
            SC_StealthApproach = true;
            SC_UseGarrote = true;
            SC_StealthWhenPoisoned = false;
            SC_SprintWhenAvail = false;
            SC_UseBlindBandage = true;
            SC_RiposteAll = false;

            // Party combat
            PC_StealthApproach = true;
            PC_UseGarrote = true;
            PC_StealthWhenPoisoned = false;
            PC_SprintWhenAvail = false;
            PC_UseBlindBandage = true;
            PC_RiposteAll = false;

            Specialization = _soloCombatName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloCombatName, _partyCombatName })]
        public override string Specialization { get; set; }

        // COMBAT
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always range pull")]
        [Description("Always pull with a range weapon")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloCombatName)]
        public bool SC_AlwaysPull { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Stealth approach")]
        [Description("Always try to approach enemies in Stealth")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloCombatName)]
        public bool SC_StealthApproach { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Garrote")]
        [Description("Use Garrote when opening behind the target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloCombatName)]
        public bool SC_UseGarrote { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Stealth poisoned")]
        [Description("Try going in stealth even if affected by poison")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloCombatName)]
        public bool SC_StealthWhenPoisoned { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Sprint")]
        [Description("Use Sprint when available")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloCombatName)]
        public bool SC_SprintWhenAvail { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Blind + Bandage")]
        [Description("Use Blind + the best bandage in your bags during combat " +
            "(If true, you should avoid using poisons and bleed effects, as they will break Blind)")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloCombatName)]
        public bool SC_UseBlindBandage { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Riposte all enemies")]
        [Description("On some servers, only humanoids can be riposted. Set this value False if it is the case.")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloCombatName)]
        public bool SC_RiposteAll { get; set; }

        // PARTY COMBAT
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Stealth approach")]
        [Description("Always try to approach enemies in Stealth")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyCombatName)]
        public bool PC_StealthApproach { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Garrote")]
        [Description("Use Garrote when opening behind the target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyCombatName)]
        public bool PC_UseGarrote { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Stealth poisoned")]
        [Description("Try going in stealth even if affected by poison")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyCombatName)]
        public bool PC_StealthWhenPoisoned { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Sprint")]
        [Description("Use Sprint when available")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyCombatName)]
        public bool PC_SprintWhenAvail { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Use Blind + Bandage")]
        [Description("Use Blind + the best bandage in your bags during combat " +
            "(If true, you should avoid using poisons and bleed effects, as they will break Blind)")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyCombatName)]
        public bool PC_UseBlindBandage { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Riposte all enemies")]
        [Description("On some servers, only humanoids can be riposted. Set this value False if it is the case.")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyCombatName)]
        public bool PC_RiposteAll { get; set; }
    }
}