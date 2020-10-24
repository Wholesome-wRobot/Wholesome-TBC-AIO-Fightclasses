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
        [DisplayName("Blessing od Wisdom")]
        [Description("Use Blessing od Wisdom instead of Blessing od Might")]
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

        // TALENT
        [DropdownList(new string[] { "Retribution" })]
        public override string Specialization { get; set; }
    }
}