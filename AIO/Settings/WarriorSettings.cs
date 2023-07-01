using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class WarriorSettings : BasePersistentSettings<WarriorSettings>
    {
        private const string _settingsTriggerName = "WarriorRotationTrigger";
        private const string _soloFuryName = "Fury";
        private const string _partyFuryName = "Party Fury";
        private const string _partyProtName = "Party Protection";
        private const string _rotationTabName = "Rotation";

        public WarriorSettings()
        {
            // Solo Fury
            SFU_AlwaysPull = false;
            SFU_UseHamstring = true;
            SFU_UseBloodRage = true;
            SFU_UseDemoralizingShout = true;
            SFU_UseCommandingShout = false;
            SFU_UseRend = false;
            SFU_UseCleave = true;
            SFU_PrioritizeBerserkStance = false;

            // Party Fury
            PFU_UseHamstring = true;
            PFU_UseBloodRage = true;
            PFU_UseCommandingShout = false;
            PFU_UseCleave = true;
            PFU_PartyStandBehind = true;

            // Party Protection
            PPR_AlwaysPull = false;
            PPR_UseDemoralizingShout = true;
            PPR_UseCommandingShout = false;
            PPR_PartyTankSwitchTarget = true;
            PPR_PartyUseIntervene = true;

            Specialization = _soloFuryName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloFuryName, _partyFuryName, _partyProtName })]
        public override string Specialization { get; set; }

        // FURY
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always range pull")]
        [Description("Always pull with a range weapon")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_AlwaysPull { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Hamstring humanoids")]
        [Description("Use Hamstring against humanoids to prevent them from fleeing too far")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_UseHamstring { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Bloodrage")]
        [Description("Use Bloodrage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_UseBloodRage { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Demoralizing Shout")]
        [Description("Use Demoralizing Shout")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_UseDemoralizingShout { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Commanding Shout")]
        [Description("Use Commanding Shout instead of Battle Shout")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_UseCommandingShout { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Rend")]
        [Description("Use Rend")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_UseRend { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Cleave")]
        [Description("Use Cleave on multi aggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_UseCleave { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Prioritize Berserker")]
        [Description("Prioritize Berserker Stance over Battle Stance")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloFuryName)]
        public bool SFU_PrioritizeBerserkStance { get; set; }

        // PARTY FURY
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Hamstring humanoids")]
        [Description("Use Hamstring against humanoids to prevent them from fleeing too far")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFuryName)]
        public bool PFU_UseHamstring { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Bloodrage")]
        [Description("Use Bloodrage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFuryName)]
        public bool PFU_UseBloodRage { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Commanding Shout")]
        [Description("Use Commanding Shout instead of Battle Shout")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFuryName)]
        public bool PFU_UseCommandingShout { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cleave")]
        [Description("Use Cleave on multi aggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFuryName)]
        public bool PFU_UseCleave { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Stand behind")]
        [Description("Try to stand behind the target in Fury DPS")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyFuryName)]
        public bool PFU_PartyStandBehind { get; set; }

        // PARTY PROT
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always range pull")]
        [Description("Always pull with a range weapon")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtName)]
        public bool PPR_AlwaysPull { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Demoralizing Shout")]
        [Description("Use Demoralizing Shout")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtName)]
        public bool PPR_UseDemoralizingShout { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Commanding Shout")]
        [Description("Use Commanding Shout instead of Battle Shout")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtName)]
        public bool PPR_UseCommandingShout { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Switch target")]
        [Description("Switch targets to regain aggro when tanking")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtName)]
        public bool PPR_PartyTankSwitchTarget { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Intervene")]
        [Description("Use Intervene")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtName)]
        public bool PPR_PartyUseIntervene { get; set; }
    }
}