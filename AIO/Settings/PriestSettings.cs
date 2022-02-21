using System;
using System.ComponentModel;
using MarsSettingsGUI;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class PriestSettings : BasePersistentSettings<PriestSettings>
    {
        public PriestSettings()
        {
            WandThreshold = 40;
            UseInnerFire = true;
            UseShieldOnPull = true;
            UseShadowGuard = true;
            UsePowerWordShield = true;
            UseShadowWordDeath = true;

            PartyCureDisease = false;
            PartyDispelMagic = false;
            PartyMassDispel = false;
            UsePowerWordFortitude = false;
            PartyPrayerOfFortitude = false;
            UseShadowProtection = false;
            PartyPrayerOfShadowProtection = false;
            UseDivineSpirit = false;
            PartyPrayerOfSpirit = false;
            PartyVampiricEmbrace = false;
            PartyMassDispelCount = 5;
            PartySWDeathThreshold = 90;
            PartyMindBlastThreshold = 70;
            PartyCircleOfHealingThreshold = 90;

            Specialization = "Shadow";
        }

        // COMMON
        [Category("Common")]
        [DefaultValue(40)]
        [DisplayName("Wand Threshold")]
        [Description("Enemy HP under which the wand should be used")]
        [Percentage(true)]
        public int WandThreshold { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Power Word: Shield")]
        [Description("Use Power Word: Shield")]
        public bool UsePowerWordShield { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Shield on pull")]
        [Description("Use Power Word: Shield on pull")]
        public bool UseShieldOnPull { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Shadowguard")]
        [Description("Use Shadowguard")]
        public bool UseShadowGuard { get; set; }

        // COMMON - Buffs
        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Inner Fire")]
        [Description("Use Inner Fire")]
        public bool UseInnerFire { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Power Word: Fortitude")]
        [Description("Use Power Word: Fortitude")]
        public bool UsePowerWordFortitude { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Shadow Protection")]
        [Description("Use Shadow Protection")]
        public bool UseShadowProtection { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Divine Spirit")]
        [Description("Use Divine Spirit")]
        public bool UseDivineSpirit { get; set; }

        // SHADOW
        [Category("Shadow")]
        [DefaultValue(true)]
        [DisplayName("SW: Death")]
        [Description("Use Shadow Word: Death")]
        public bool UseShadowWordDeath { get; set; }

        [Category("Shadow")]
        [DefaultValue(false)]
        [DisplayName("[Party] Vamp. Embrace")]
        [Description("Use Vampiric Embrace in combat")]
        public bool PartyVampiricEmbrace { get; set; }

        [Category("Shadow")]
        [DefaultValue(90)]
        [DisplayName("[Party] SW: Death")]
        [Description("Use Shadow Word: Death when above this HEALTH percentage threshold (100 to disable)")]
        [Percentage(true)]
        public int PartySWDeathThreshold { get; set; }

        [Category("Shadow")]
        [DefaultValue(70)]
        [DisplayName("[Party] Mind Blast")]
        [Description("Use Mind Blast when above this MANA percentage threshold (100 to disable)")]
        [Percentage(true)]
        public int PartyMindBlastThreshold { get; set; }

        // HOLY
        [Category("Holy")]
        [DefaultValue(false)]
        [DisplayName("[Party] Mass Dispel")]
        [Description("Use Mass Dispel in combat")]
        public bool PartyMassDispel { get; set; }

        [Category("Holy")]
        [DefaultValue(5)]
        [DisplayName("[Party] Mass Dispel Count")]
        [Description("Minimum number of group members with dispellable debuff to use Mass Dispel")]
        public int PartyMassDispelCount { get; set; }

        [Category("Holy")]
        [DefaultValue(90)]
        [DisplayName("[Party] Circle of Healing")]
        [Description("Use Circle of Healing on party members under this health threshold")]
        [Percentage(true)]
        public int PartyCircleOfHealingThreshold { get; set; }

        // PARTY
        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Cure Disease")]
        [Description("Use Cure Disease in combat")]
        public bool PartyCureDisease { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Dispel Magic")]
        [Description("Use Dispel Magic in combat")]
        public bool PartyDispelMagic { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Prayer of Fortitude")]
        [Description("Use Prayer of Fortitude")]
        public bool PartyPrayerOfFortitude { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Prayer of Shadow Protection")]
        [Description("Use Prayer of Shadow Protection")]
        public bool PartyPrayerOfShadowProtection { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Prayer of Spirit")]
        [Description("Use Prayer of Spirit")]
        public bool PartyPrayerOfSpirit { get; set; }

        // TALENT
        [DropdownList(new string[] { "Shadow", "Party Shadow", "Party Holy", "Raid Holy" })]
        public override string Specialization { get; set; }
    }
}