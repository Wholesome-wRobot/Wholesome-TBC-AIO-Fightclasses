using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class WarlockSettings : BasePersistentSettings<WarlockSettings>
    {
        private const string _settingsTriggerName = "WarlockRotationTrigger";
        private const string _soloAffliName = "Affliction";
        private const string _soloDemoName = "Demonology";
        private const string _partyAffliName = "Party Affliction";
        private const string _rotationTabName = "Rotation";
        private const string _petTabName = "Pet";

        public WarlockSettings()
        {
            // Solo Affli
            AFF_LifeTap = true;
            AFF_WandingOverSB = true;
            AFF_SiphonLife = false;
            AFF_ImmolateHighLevel = true;
            AFF_UnendingBreath = true;
            AFF_DarkPact = true;
            AFF_FelArmor = true;
            AFF_Incinerate = true;
            AFF_DrainSoulLevel1 = false;
            AFF_DrainSoulHP = 40;
            AFF_AlwaysDrainSoul = false;
            AFF_HealthFunnelOOC = true;

            // Solo Demonology
            DEM_LifeTap = true;
            DEM_WandingOverSB = true;
            DEM_SiphonLife = false;
            DEM_ImmolateHighLevel = true;
            DEM_UnendingBreath = true;
            DEM_DarkPact = true;
            DEM_FelArmor = true;
            DEM_Incinerate = true;
            DEM_DrainSoulLevel1 = false;
            DEM_DrainSoulHP = 40;
            DEM_AlwaysDrainSoul = false;
            DEM_HealthFunnelOOC = true;

            // Party Affli
            PAF_LifeTap = true;
            PAF_SiphonLife = false;
            PAF_UnendingBreath = true;
            PAF_SoulShatter = true;
            PAF_DrainSoulLevel1 = false;
            PAF_DrainSoulHP = 40;
            PAF_AlwaysDrainSoul = false;
            PAF_HealthFunnelOOC = true;
            PAF_CurseOfTheElements = true;
            PAF_LifeTapManaThreshold = 20;
            PAF_LifeTapHealthThreshold = 50;
            PAF_SeedOfCorruptionAmount = 3;

            // Common
            CommonNumberOfSoulShards = 4;

            // Pet
            AutoAnguish = true;
            FelguardCleave = true;
            AutoTorment = false;
            PetInPassiveWhenOOC = true;
            HealthThresholdResummon = 30;
            ManaThresholdResummon = 20;

            Specialization = _soloAffliName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloAffliName, _soloDemoName, _partyAffliName })]
        public override string Specialization { get; set; }

        // SOLO AFFLI
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Life Tap")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Life Tap")]
        public bool AFF_LifeTap { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Wand over SB")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Prioritize wanding over Shadow Bolt during combat to save mana")]
        public bool AFF_WandingOverSB { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Siphon Life")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Siphon Life (Recommended only after TBC green gear)")]
        public bool AFF_SiphonLife { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Immolate high lvl")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Keep using Immmolate once Unstable Affliction is learnt")]
        public bool AFF_ImmolateHighLevel { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Unending Breath")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Makes sure you have Unending Breath up at all time")]
        public bool AFF_UnendingBreath { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Dark Pact")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Dark Pact")]
        public bool AFF_DarkPact { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Fel Armor")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Fel Armor instead of Demon Armor")]
        public bool AFF_FelArmor { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Incinerate")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Incinerate (Use Immolate at high level must be True)")]
        public bool AFF_Incinerate { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Drain Soul lvl1")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Drain Soul level 1 instead of max rank")]
        public bool AFF_DrainSoulLevel1 { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(40)]
        [DisplayName("Drain Soul HP")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Drain Soul when the enemy HP is under this threshold")]
        [Percentage(true)]
        public int AFF_DrainSoulHP { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always Drain Soul")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Always try to finish the enemy with Drain Soul (synergizes with Improved Drain Soul talent)")]
        public bool AFF_AlwaysDrainSoul { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Health Funnel OOC")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloAffliName)]
        [Description("Use Health Funnel when out of combat. If OFF, will resummon your pet when it's low HP instead.")]
        public bool AFF_HealthFunnelOOC { get; set; }

        // SOLO DEMO
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Life Tap")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Life Tap")]
        public bool DEM_LifeTap { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Wand over SB")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Prioritize wanding over Shadow Bolt during combat to save mana")]
        public bool DEM_WandingOverSB { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Siphon Life")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Siphon Life (Recommended only after TBC green gear)")]
        public bool DEM_SiphonLife { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Immolate high lvl")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Keep using Immmolate once Unstable Affliction is learnt")]
        public bool DEM_ImmolateHighLevel { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Unending Breath")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Makes sure you have Unending Breath up at all time")]
        public bool DEM_UnendingBreath { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Dark Pact")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Dark Pact")]
        public bool DEM_DarkPact { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Fel Armor")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Fel Armor instead of Demon Armor")]
        public bool DEM_FelArmor { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Incinerate")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Incinerate (Use Immolate at high level must be True)")]
        public bool DEM_Incinerate { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Drain Soul lvl1")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Drain Soul level 1 instead of max rank")]
        public bool DEM_DrainSoulLevel1 { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(40)]
        [DisplayName("Drain Soul HP")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Drain Soul when the enemy HP is under this threshold")]
        [Percentage(true)]
        public int DEM_DrainSoulHP { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always Drain Soul")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Always try to finish the enemy with Drain Soul (synergizes with Improved Drain Soul talent)")]
        public bool DEM_AlwaysDrainSoul { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Health Funnel OOC")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloDemoName)]
        [Description("Use Health Funnel when out of combat. If OFF, will resummon your pet when it's low HP instead.")]
        public bool DEM_HealthFunnelOOC { get; set; }

        // PARTY AFFLI
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Life Tap")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Life Tap")]
        public bool PAF_LifeTap { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Siphon Life")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Siphon Life (Recommended only after TBC green gear)")]
        public bool PAF_SiphonLife { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Unending Breath")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Makes sure you have Unending Breath up at all time")]
        public bool PAF_UnendingBreath { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Soul Shatter")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Soul Shatter on multi aggro")]
        public bool PAF_SoulShatter { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Drain Soul lvl1")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Drain Soul level 1 instead of max rank")]
        public bool PAF_DrainSoulLevel1 { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(40)]
        [DisplayName("Drain Soul HP")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Drain Soul when the enemy HP is under this threshold")]
        [Percentage(true)]
        public int PAF_DrainSoulHP { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Always Drain Soul")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Always try to finish the enemy with Drain Soul (synergizes with Improved Drain Soul talent)")]
        public bool PAF_AlwaysDrainSoul { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Health Funnel OOC")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Health Funnel when out of combat. If OFF, will resummon your pet when it's low HP instead.")]
        public bool PAF_HealthFunnelOOC { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Curse of the Elements")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Curse of the Elements")]
        public bool PAF_CurseOfTheElements { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(20)]
        [DisplayName("Life Tap Mana")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Life Tap when your mana goes under this threshold")]
        [Percentage(true)]
        public int PAF_LifeTapManaThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(50)]
        [DisplayName("Life Tap Health")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Allow Life Tap only if your Health is over this threshold")]
        [Percentage(true)]
        public int PAF_LifeTapHealthThreshold { get; set; }
        [Category(_rotationTabName)]
        [DefaultValue(3)]
        [DisplayName("Seed of Corruption")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyAffliName)]
        [Description("Use Seed of Corruption when there is at least this amount of enemies")]
        public int PAF_SeedOfCorruptionAmount { get; set; }

        // COMMON
        [Category(_rotationTabName)]
        [DefaultValue(4)]
        [DisplayName("Soul Shards")]
        [Description("Sets the minimum number of Soul Shards to have in your bags")]
        public int CommonNumberOfSoulShards { get; set; }

        // PET
        [Category(_petTabName)]
        [DefaultValue(true)]
        [DisplayName("Passive Pet OOC")]
        [Description("Puts pet in passive when out of combat (can be useful if you want to ignore fights when traveling)")]
        public bool PetInPassiveWhenOOC { get; set; }
        [Category(_petTabName)]
        [DefaultValue(false)]
        [DisplayName("Auto torment")]
        [Description("If true, will let Torment on autocast. If false, will let the AIO manage Torment in order to save Voidwalker mana.")]
        public bool AutoTorment { get; set; }
        [Category(_petTabName)]
        [DefaultValue(true)]
        [DisplayName("Auto Anguish")]
        [Description("If true, will let Anguish on autocast. If false, will let the AIO manage Anguish in order to save Felguard mana.")]
        public bool AutoAnguish { get; set; }
        [Category(_petTabName)]
        [DefaultValue(true)]
        [DisplayName("Felguard Cleave")]
        [Description("Use Felguard's Cleave")]
        public bool FelguardCleave { get; set; }
        [Category(_petTabName)]
        [Percentage(true)]
        [DefaultValue(30)]
        [DisplayName("Health Resummon")]
        [Description("Resummon your pet if its health falls below this threshold")]
        public int HealthThresholdResummon { get; set; }
        [Category(_petTabName)]
        [Percentage(true)]
        [DefaultValue(20)]
        [DisplayName("Mana Resummon")]
        [Description("Resummon your pet if its mana falls below this threshold")]
        public int ManaThresholdResummon { get; set; }
    }
}