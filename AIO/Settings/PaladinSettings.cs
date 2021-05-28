using System;
using System.ComponentModel;
using MarsSettingsGUI;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class PaladinSettings : BasePersistentSettings<PaladinSettings>
    {
        public PaladinSettings()
        {
            ManaSaveLimitPercent = 50;
            UseExorcism = false;
            HealDuringCombat = true;
            OOCFlashHealThreshold = 80;
            OOCHolyLightThreshold = 50;
            RetributionAura = false;

            UseBlessingOfWisdom = false;
            UseSealOfCommand = false;
            UseHammerOfWrath = false;
            DevoAuraOnMulti = true;
            UseSealOfTheCrusader = true;
            ActivateCombatDebug = false;

            PartyAura = "Devotion Aura";
            PartyPurify = false;
            PartyCleanse = false;
            PartyStandBehind = true;
            PartyTankSwitchTarget = true;
            PartyRetConsecrationThreshold = 70;
            PartyRetExorcismThreshold = 70;
            PartyFlashOfLightThreshold = 80;
            PartyHolyLightThreshold = 65;
            PartyHolySealOfLight = false;
            PartyDetectSpecs = false;
            PartyAvengersShieldnRank1 = false;
            PartyConsecrationRank1 = false;
            PartyHolyShieldRank1 = false;
            PartyHealOOC = false;
            PartyProtSealOfWisdom = 40;

            Specialization = "Retribution";
        }

        // COMMON
        [Category("Common")]
        [DefaultValue(50)]
        [DisplayName("Mana Save")]
        [Description("Try to save this percentage of mana")]
        [Percentage(true)]
        public int ManaSaveLimitPercent { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Use Exorcism")]
        [Description("Use Exorcism against Undead and Demon target")]
        public bool UseExorcism { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Heal during combat")]
        [Description("Use healing spells during combat")]
        public bool HealDuringCombat { get; set; }

        [Category("Common")]
        [DefaultValue(80)]
        [DisplayName("Flash heal OOC")]
        [Description("Health precentage under which you want out of combat Flash of Light Heal")]
        [Percentage(true)]
        public int OOCFlashHealThreshold { get; set; }

        [Category("Common")]
        [DefaultValue(50)]
        [DisplayName("Holy Light OOC")]
        [Description("Health precentage under which you want out of combat Holy Light heal")]
        [Percentage(true)]
        public int OOCHolyLightThreshold { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Retribution Aura")]
        [Description("Use Retribution AUra instead of Sanctity Aura")]
        public bool RetributionAura { get; set; }

        // RETRIBUTION
        [Category("Retribution")]
        [DefaultValue(false)]
        [DisplayName("Hammer of Wrath")]
        [Description("Use Hammer of Wrath")]
        public bool UseHammerOfWrath { get; set; }

        [Category("Retribution")]
        [DefaultValue(false)]
        [DisplayName("Blessing of Wisdom")]
        [Description("Use Blessing od Wisdom instead of Blessing of Might")]
        public bool UseBlessingOfWisdom { get; set; }

        [Category("Retribution")]
        [DefaultValue(false)]
        [DisplayName("Seal of Command")]
        [Description("Use Seal of Command instead of Seal of Righteousness")]
        public bool UseSealOfCommand { get; set; }

        [Category("Retribution")]
        [DefaultValue(true)]
        [DisplayName("Seal of the Crusader")]
        [Description("Use Seal of the Crusader when opening a fight")]
        public bool UseSealOfTheCrusader { get; set; }

        [Category("Retribution")]
        [DefaultValue(true)]
        [DisplayName("Devotion on multi")]
        [Description("Use Devotion Aura on multi aggro")]
        public bool DevoAuraOnMulti { get; set; }

        // PARTY
        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[RET] Stand behind")]
        [Description("Try to stand behind the target in Retribution spec")]
        public bool PartyStandBehind { get; set; }

        [Category("Party")]
        [DefaultValue(70)]
        [DisplayName("[RET] Consecration")]
        [Description("Use Consecration for extra damage when mana is over this threshold")]
        [Percentage(true)]
        public int PartyRetConsecrationThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(70)]
        [DisplayName("[RET] Exorcism")]
        [Description("Use Exorcism for extra damage when mana is over this threshold")]
        [Percentage(true)]
        public int PartyRetExorcismThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(80)]
        [DisplayName("[HOL] Flash of Light")]
        [Description("Use Flash of Light on party members under this health threshold")]
        [Percentage(true)]
        public int PartyFlashOfLightThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(65)]
        [DisplayName("[HOL] Holy Light")]
        [Description("Use Holy Light on party members under this health threshold")]
        [Percentage(true)]
        public int PartyHolyLightThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("[HOL] Seal of Light")]
        [Description("Use Seal of Light/Judgement on target")]
        public bool PartyHolySealOfLight { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[PRT] Switch target")]
        [Description("Switch targets to regain aggro when tanking")]
        public bool PartyTankSwitchTarget { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("[PRT] Holy Shield 1")]
        [Description("Use Holy Shield Rank 1")]
        public bool PartyHolyShieldRank1 { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("[PRT] Consecration 1")]
        [Description("Use Consecration Rank 1")]
        public bool PartyConsecrationRank1 { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("[PRT] Av. Shield 1")]
        [Description("Use Avenger's Shield Rank 1")]
        public bool PartyAvengersShieldnRank1 { get; set; }

        [Category("Party")]
        [DefaultValue(40)]
        [DisplayName("[PRT] S. of Wisdom")]
        [Description("Use Seal of Wisdom when under this mana threshold")]
        [Percentage(true)]
        public int PartyProtSealOfWisdom { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("[PRT-RET] Heal OOC")]
        [Description("Healing allies when out of combat")]
        public bool PartyHealOOC { get; set; }

        [Category("Party")]
        [DefaultValue("Devotion Aura")]
        [DisplayName("Aura")]
        [Description("Select the aura to use")]
        [DropdownList(new string[] { "Devotion Aura", "Retribution Aura", "Concentration Aura", "Sanctity Aura", "Shadow Resistance Aura", "Frost Resistance Aura", "Fire Resistance Aura" })]
        public string PartyAura { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Purify")]
        [Description("Use Purify in Party combat")]
        public bool PartyPurify { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Cleanse")]
        [Description("Use Cleanse in Party combat")]
        public bool PartyCleanse { get; set; }
        
        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Detect party specs")]
        [Description("Allow party specs detection. By enabling this setting, your character will inspect every group member in range in order to detect its spcialization and select the best buffs. Be aware that it can take some time due to the TBC server API forcing a cooldown on inspection talent detection.")]
        public bool PartyDetectSpecs { get; set; }

        // TALENT
        [DropdownList(new string[] { "Retribution", "Party Retribution", "Party Protection", "Party Holy" })]
        public override string Specialization { get; set; }
    }
}