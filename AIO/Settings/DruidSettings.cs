using System;
using System.ComponentModel;
using MarsSettingsGUI;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class DruidSettings : BasePersistentSettings<DruidSettings>
    {
        public DruidSettings()
        {
            AlwaysPull = false;
            UseBarkskin = true;
            UseInnervate = true;
            UseAquaticForm = true;

            UseEnrage = true;
            UseSwipe = true;
            UseTigersFury = true;
            StealthEngage = true;
            NumberOfAttackersBearForm = 2;

            PartyTankSwitchTarget = true;
            PartyUseInnervate = true;
            PartyUseRebirth = true;
            PartyAbolishPoison = true;
            PartyRemoveCurse = true;
            PartyTranquility = true;
            PartyStandBehind = true;

            Specialization = "Feral";
        }

        // COMMON
        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Use Aquatic Form")]
        [Description("Use Aquatic Form")]
        public bool UseAquaticForm { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Use Innervate")]
        [Description("Use Innervate")]
        public bool UseInnervate { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Use Barkskin")]
        [Description("Use Barkskin before healing in dangerous situations")]
        public bool UseBarkskin { get; set; }

        // FERAL
        [Category("Feral")]
        [DefaultValue(false)]
        [DisplayName("Always range pull")]
        [Description("Always pull with a range spell")]
        public bool AlwaysPull { get; set; }

        [Category("Feral")]
        [DefaultValue(2)]
        [DisplayName("Bear number of attackers")]
        [Description("Bear Form when the number of enemies attacking you is superior or equal to this value.")]
        public int NumberOfAttackersBearForm { get; set; }

        [Category("Feral")]
        [DefaultValue(true)]
        [DisplayName("Always use Bear Form Enrage")]
        [Description("Always use Enrage")]
        public bool UseEnrage { get; set; }

        [Category("Feral")]
        [DefaultValue(true)]
        [DisplayName("Swipe")]
        [Description("Use Swipe on multi aggro")]
        public bool UseSwipe { get; set; }

        [Category("Feral")]
        [DefaultValue(true)]
        [DisplayName("Use Tiger's Fury")]
        [Description("Use Tiger's Fury")]
        public bool UseTigersFury { get; set; }

        [Category("Feral")]
        [DefaultValue(true)]
        [DisplayName("Cat Stealth engage")]
        [Description("Try to engage fights using Prowl and going behind the target")]
        public bool StealthEngage { get; set; }

        // PARTY
        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[Tank] Switch target")]
        [Description("Switch targets to regain aggro when tanking")]
        public bool PartyTankSwitchTarget { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[DPS] Stand behind")]
        [Description("Try to stand behind the target in Feral DPS")]
        public bool PartyStandBehind { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("Rebirth")]
        [Description("Use Rebirth on dead team members")]
        public bool PartyUseRebirth { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("Party Innervate")]
        [Description("Use Innervate on low mana team members")]
        public bool PartyUseInnervate { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("Party Remove Curse")]
        [Description("Use Remove Curse in combat")]
        public bool PartyRemoveCurse { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("Party Abolish Poison")]
        [Description("Use Abolish Poison in combat")]
        public bool PartyAbolishPoison { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("Party Tranquility")]
        [Description("Use Tranquility in combat")]
        public bool PartyTranquility { get; set; }


        // TALENT
        [DropdownList(new string[] { "Feral", "Party Feral DPS", "Party Feral Tank", "Party Restoration" })]
        public override string Specialization { get; set; }
    }
}