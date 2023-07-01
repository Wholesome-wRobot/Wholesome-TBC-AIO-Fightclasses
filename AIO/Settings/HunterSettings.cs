using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class HunterSettings : BasePersistentSettings<HunterSettings>
    {
        private const string _settingsTriggerName = "HunterRotationTrigger";
        private const string _soloBeastMasterName = "BeastMaster";
        private const string _partyBeastMasterName = "Party BeastMaster";
        private const string _rotationTabName = "Rotation";
        private const string _commonTabName = "Common";
        private const string _petTabName = "Pet";

        public HunterSettings()
        {
            // Beastmaster
            BM_AspectOfTheCheetahThreashold = 80;
            BM_UseRaptorStrike = true;
            BM_RapidFireOnMulti = false;
            BM_UseConcussiveShot = true;
            BM_UseDisengage = false;
            BM_BestialWrathOnMulti = false;
            BM_ArcaneShotThreshold = 60;
            BM_MultishotThreshold = 60;
            BM_MultishotOnSolo = true;

            // Party Beastmaster
            PBM_AspectOfTheCheetahThreashold = 80;
            PBM_UseRaptorStrike = true;
            PBM_UseConcussiveShot = true;
            PBM_UseDisengage = false;
            PBM_MultishotCount = 2;

            // Pet
            FeedPet = true;
            AutoGrowl = false;
            RevivePetInCombat = true;
            IntimidationInterrupt = true;

            // Common
            BackupFromMelee = true;
            MaxBackupAttempts = 3;
            BackupUsingCTM = false;
            BackupDistance = 6;

            Specialization = _soloBeastMasterName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloBeastMasterName, _partyBeastMasterName })]
        public override string Specialization { get; set; }

        // BEASTMASTER
        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Cheetah Threshold")]
        [Description("Use Aspect of the Cheetah when above this mana percent (100 to disable)")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        [Percentage(true)]
        public int BM_AspectOfTheCheetahThreashold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Raptor Strike")]
        [Description("Use Raptor Strike")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        public bool BM_UseRaptorStrike { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Rapid Fire on multi")]
        [Description("Only use Rapid Fire on multi aggro. If set to False, Rapid Fire will be used as soon at available")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        public bool BM_RapidFireOnMulti { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Concussive Shot")]
        [Description("Use Concussive Shot on low HP humanoids to keep them from fleeing")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        public bool BM_UseConcussiveShot { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Disengage")]
        [Description("Use Disengage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        public bool BM_UseDisengage { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("BW on multi")]
        [Description("Only use Bestial Wrath on multi aggro. If set to False, Bestial Wrath will be used as soon at available")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        public bool BM_BestialWrathOnMulti { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(60)]
        [DisplayName("Arcane Shot Threshold")]
        [Description("Use Arcane shot when mana percent is greater than this value")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        [Percentage(true)]
        public int BM_ArcaneShotThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("MS on single")]
        [Description("Use multishot as part of the normal rotation against single enemies")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        public bool BM_MultishotOnSolo { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(60)]
        [DisplayName("Multi Shot Threshold")]
        [Description("Use Multi shot when mana percent is greater than this value")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloBeastMasterName)]
        [Percentage(true)]
        public int BM_MultishotThreshold { get; set; }

        // PARTY BEASTMASTER
        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Cheetah Threshold")]
        [Description("Use Aspect of the Cheetah when above this mana percent (100 to disable)")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyBeastMasterName)]
        [Percentage(true)]
        public int PBM_AspectOfTheCheetahThreashold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Raptor Strike")]
        [Description("Use Raptor Strike")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyBeastMasterName)]
        public bool PBM_UseRaptorStrike { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Concussive Shot")]
        [Description("Use Concussive Shot on low HP humanoids to keep them from fleeing")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyBeastMasterName)]
        public bool PBM_UseConcussiveShot { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Disengage")]
        [Description("Use Disengage")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyBeastMasterName)]
        public bool PBM_UseDisengage { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(2)]
        [DisplayName("MultiShot Count")]
        [Description("Minimum amount of enemies around the target to use Multi-Shot")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyBeastMasterName)]
        public int PBM_MultishotCount { get; set; }

        // PET
        [Category(_petTabName)]
        [DefaultValue(true)]
        [DisplayName("Feed Pet")]
        [Description("Let the AIO manage pet feeding")]
        public bool FeedPet { get; set; }
        [Category(_petTabName)]
        [DefaultValue(false)]
        [DisplayName("Auto Growl")]
        [Description("If true, will let Growl on autocast. If false, will let the AIO manage Growl in order to save your pet's energy.")]
        public bool AutoGrowl { get; set; }
        [Category(_petTabName)]
        [DefaultValue(true)]
        [DisplayName("Revive pet in combat")]
        [Description("Revive your pet during combat")]
        public bool RevivePetInCombat { get; set; }
        [Category(_petTabName)]
        [DefaultValue(true)]
        [DisplayName("Intimidation interrupt")]
        [Description("Use Intimidation to interrupt enemy casting")]
        public bool IntimidationInterrupt { get; set; }

        // COMMON
        [Category(_commonTabName)]
        [DefaultValue(true)]
        [DisplayName("Backup from melee")]
        [Description("Set to True is you want to backup from melee range when you don't have direct aggro")]
        public bool BackupFromMelee { get; set; }
        [Category(_commonTabName)]
        [DefaultValue(false)]
        [DisplayName("Backup using CTM")]
        [Description("If set to True, will backup using Click To Move. If false, will use the keyboard")]
        public bool BackupUsingCTM { get; set; }
        [Category(_commonTabName)]
        [DefaultValue(3)]
        [DisplayName("Max backup attempts")]
        [Description("Maximum number of attempts after failing to backup to a valid distance (eg when back to a wall)")]
        public int MaxBackupAttempts { get; set; }
        [Category(_commonTabName)]
        [DefaultValue(6)]
        [DisplayName("Backup distance")]
        [Description("Adjusted backup distance (default 6). This distance will be added to a rough target size calculation.")]
        public int BackupDistance { get; set; }
    }
}