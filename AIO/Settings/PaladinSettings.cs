using MarsSettingsGUI;
using System;
using System.ComponentModel;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class PaladinSettings : BasePersistentSettings<PaladinSettings>
    {
        private const string _settingsTriggerName = "PaladinRotationTrigger";
        private const string _soloRetributionName = "Retribution";
        private const string _partyRetributionName = "Party Retribution";
        private const string _partyProtectionName = "Party Protection";
        private const string _partyHolyName = "Party Holy";
        private const string _raidHolyName = "Raid Holy";
        private const string _rotationTabName = "Rotation";
        private const string _partyTabName = "Party";

        public PaladinSettings()
        {
            // Solo Retribution
            SRET_ManaSaveLimitPercent = 50;
            SRET_UseExorcism = false;
            SRET_OOCFlashHealThreshold = 80;
            SRET_OOCHolyLightThreshold = 50;
            SRET_HealDuringCombat = true;
            SRET_RetributionAura = false;
            SRET_UseBlessingOfWisdom = false;
            SRET_UseSealOfCommand = false;
            SRET_UseHammerOfWrath = false;
            SRET_DevoAuraOnMulti = true;
            SRET_UseSealOfTheCrusader = true;

            // Party Retribution
            PRET_ManaSaveLimitPercent = 50;
            PRET_UseSealOfCommand = false;
            PRET_UseHammerOfWrath = false;
            PRET_UseSealOfTheCrusader = true;
            PRET_PartyPurify = false;
            PRET_PartyCleanse = false;
            PRET_PartyStandBehind = true;
            PRET_PartyRetConsecrationThreshold = 70;
            PRET_PartyRetExorcismThreshold = 70;

            // Party Protection
            PPROT_UseExorcism = false;
            PPROT_UseHammerOfWrath = false;
            PPROT_PartyPurify = false;
            PPROT_PartyCleanse = false;
            PPROT_PartyTankSwitchTarget = true;
            PPROT_PartyAvengersShieldnRank1 = false;
            PPROT_PartyConsecrationRank1 = false;
            PPROT_PartyHolyShieldRank1 = false;
            PPROT_PartyProtSealOfWisdom = 40;

            // Party Holy
            PHO_PartyPurify = false;
            PHO_PartyCleanse = false;
            PHO_PartyFlashOfLightThreshold = 80;
            PHO_PartyHolyLightPercentThreshold = 65;
            PHO_PartyHolySealOfLight = false;

            // Raid Holy
            RHO_PartyPurify = false;
            RHO_PartyCleanse = false;
            RHO_PartyCleansePriority = "Random";
            RHO_PartyFlashOfLightThreshold = 80;
            RHO_PartyHolyLightPercentThreshold = 65;
            RHO_PartyHolyLightValueThreshold = 4000;
            RHO_PartyHolySealOfLight = false;
            RHO_PartyTankHealingPriority = 0;

            // Common Party
            PARTY_PartyAura = "Devotion Aura";
            PARTY_PartyHealOOC = false;
            PARTY_PartyBlessings = true;

            Specialization = _soloRetributionName;
        }

        // TALENT
        [TriggerDropdown(_settingsTriggerName, new string[] { _soloRetributionName, _partyRetributionName, _partyProtectionName, _partyHolyName, _raidHolyName })]
        public override string Specialization { get; set; }

        // SOLO RETRIBUTION
        [Category(_rotationTabName)]
        [DefaultValue(50)]
        [DisplayName("Mana Save")]
        [Description("Try to save this percentage of mana")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public int SRET_ManaSaveLimitPercent { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Retribution Aura")]
        [Description("Use Retribution AUra instead of Sanctity Aura")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_RetributionAura { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Use Exorcism")]
        [Description("Use Exorcism against Undead and Demon target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_UseExorcism { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Flash heal OOC")]
        [Description("Health precentage under which you want out of combat Flash of Light Heal")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public int SRET_OOCFlashHealThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(50)]
        [DisplayName("Holy Light OOC")]
        [Description("Health precentage under which you want out of combat Holy Light heal")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public int SRET_OOCHolyLightThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Heal during combat")]
        [Description("Use healing spells during combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_HealDuringCombat { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Blessing of Wisdom")]
        [Description("Use Blessing od Wisdom instead of Blessing of Might")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_UseBlessingOfWisdom { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Seal of Command")]
        [Description("Use Seal of Command instead of Seal of Righteousness")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_UseSealOfCommand { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Hammer of Wrath")]
        [Description("Use Hammer of Wrath")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_UseHammerOfWrath { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Devotion on multi")]
        [Description("Use Devotion Aura on multi aggro")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_DevoAuraOnMulti { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Seal of the Crusader")]
        [Description("Use Seal of the Crusader when opening a fight")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _soloRetributionName)]
        public bool SRET_UseSealOfTheCrusader { get; set; }

        // PARTY RETRIBUTION
        [Category(_rotationTabName)]
        [DefaultValue(50)]
        [DisplayName("Mana Save")]
        [Description("Try to save this percentage of mana")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public int PRET_ManaSaveLimitPercent { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Seal of Command")]
        [Description("Use Seal of Command instead of Seal of Righteousness")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public bool PRET_UseSealOfCommand { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Hammer of Wrath")]
        [Description("Use Hammer of Wrath")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public bool PRET_UseHammerOfWrath { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Seal of the Crusader")]
        [Description("Use Seal of the Crusader when opening a fight")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public bool PRET_UseSealOfTheCrusader { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Purify")]
        [Description("Use Purify in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public bool PRET_PartyPurify { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cleanse")]
        [Description("Use Cleanse in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public bool PRET_PartyCleanse { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Stand behind")]
        [Description("Try to stand behind the target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public bool PRET_PartyStandBehind { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(70)]
        [DisplayName("Consecration")]
        [Description("Use Consecration for extra damage when mana is over this threshold")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public int PRET_PartyRetConsecrationThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(70)]
        [DisplayName("Exorcism")]
        [Description("Use Exorcism for extra damage when mana is over this threshold")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyRetributionName)]
        public int PRET_PartyRetExorcismThreshold { get; set; }

        // PARTY PROTECTION
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Use Exorcism")]
        [Description("Use Exorcism against Undead and Demon target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_UseExorcism { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Hammer of Wrath")]
        [Description("Use Hammer of Wrath")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_UseHammerOfWrath { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Purify")]
        [Description("Use Purify in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_PartyPurify { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cleanse")]
        [Description("Use Cleanse in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_PartyCleanse { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(true)]
        [DisplayName("Switch target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_PartyTankSwitchTarget { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Av. Shield 1")]
        [Description("Use Avenger's Shield Rank 1")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_PartyAvengersShieldnRank1 { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Consecration 1")]
        [Description("Use Consecration Rank 1")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_PartyConsecrationRank1 { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Holy Shield 1")]
        [Description("Use Holy Shield Rank 1")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        public bool PPROT_PartyHolyShieldRank1 { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(40)]
        [DisplayName("S. of Wisdom")]
        [Description("Use Seal of Wisdom when under this mana threshold")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyProtectionName)]
        [Percentage(true)]
        public int PPROT_PartyProtSealOfWisdom { get; set; }

        // PARTY HOLY
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Purify")]
        [Description("Use Purify in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_PartyPurify { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cleanse")]
        [Description("Use Cleanse in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_PartyCleanse { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Flash of Light")]
        [Description("Use Flash of Light on party members under this health threshold")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public int PHO_PartyFlashOfLightThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(65)]
        [DisplayName("Holy Light Percentage")]
        [Description("Use Holy Light on party members under this health percentage")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public int PHO_PartyHolyLightPercentThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Seal of Light")]
        [Description("Use Seal of Light/Judgement on target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _partyHolyName)]
        public bool PHO_PartyHolySealOfLight { get; set; }

        // RAID HOLY
        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Purify")]
        [Description("Use Purify in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_PartyPurify { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Cleanse")]
        [Description("Use Cleanse in Party combat")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_PartyCleanse { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue("Random")]
        [DisplayName("Cleanse priority")]
        [Description("Use Cleanse with the selected priority")]
        [DropdownList(new string[] { "High", "Random", "Low" })]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public string RHO_PartyCleansePriority { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(80)]
        [DisplayName("Flash of Light")]
        [Description("Use Flash of Light on party members under this health threshold")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public int RHO_PartyFlashOfLightThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(65)]
        [DisplayName("Holy Light Percentage")]
        [Description("Use Holy Light on party members under this health percentage")]
        [Percentage(true)]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public int RHO_PartyHolyLightPercentThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(4000)]
        [DisplayName("Holy Light Value")]
        [Description("Use Holy Light on party members under this amount of missing health")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public int RHO_PartyHolyLightValueThreshold { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(false)]
        [DisplayName("Seal of Light")]
        [Description("Use Seal of Light/Judgement on target")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        public bool RHO_PartyHolySealOfLight { get; set; }

        [Category(_rotationTabName)]
        [DefaultValue(0)]
        [DisplayName("Tank healing priority")]
        [Description("Prefer healing tanks over other group members")]
        [VisibleWhenDropdownValue(_settingsTriggerName, _raidHolyName)]
        [Percentage(true)]
        public int RHO_PartyTankHealingPriority { get; set; }

        // PARTY
        [Category(_partyTabName)]
        [DefaultValue(false)]
        [DisplayName("Heal OOC")]
        [Description("Healing allies when out of combat")]
        public bool PARTY_PartyHealOOC { get; set; }

        [Category(_partyTabName)]
        [DefaultValue("Devotion Aura")]
        [DisplayName("Aura")]
        [Description("Select the aura to use")]
        [DropdownList(new string[] { "Devotion Aura", "Retribution Aura", "Concentration Aura", "Sanctity Aura", "Shadow Resistance Aura", "Frost Resistance Aura", "Fire Resistance Aura" })]
        public string PARTY_PartyAura { get; set; }

        [Category(_partyTabName)]
        [DefaultValue(true)]
        [DisplayName("Buff Group")]
        [Description("Buff group members with automatically inferred blessing")]
        public bool PARTY_PartyBlessings { get; set; }
    }
}