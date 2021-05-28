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
            UseShadowProtection = true;
            UsePowerWordShield = true;

            UseShadowWordDeath = true;

            PartyCureDisease = false;
            PartyShadowProtection = true;
            PartyDispelMagic = false;
            PartyVampiricEmbrace = false;
            PartySWDeathThreshold = 90;
            PartyMindBlastThreshold = 70;

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

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Shadow Protection")]
        [Description("Use Shadow Protection")]
        public bool UseShadowProtection { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Inner Fire")]
        [Description("Use Inner Fire")]
        public bool UseInnerFire { get; set; }

        // SHADOW
        [Category("Shadow")]
        [DefaultValue(true)]
        [DisplayName("SW: Death")]
        [Description("Use Shadow Word: Death")]
        public bool UseShadowWordDeath { get; set; }

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
        [DefaultValue(true)]
        [DisplayName("Shadow Protection")]
        [Description("Buff party with Shadow Protection")]
        public bool PartyShadowProtection { get; set; }

        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("[SH] Vamp. Embrace")]
        [Description("Use Vampiric Embrace in combat")]
        public bool PartyVampiricEmbrace { get; set; }

        [Category("Party")]
        [DefaultValue(90)]
        [DisplayName("[SH] SW: Death")]
        [Description("Use Shadow Word: Death when above this HEALTH percentage threshold (100 to disable)")]
        [Percentage(true)]
        public int PartySWDeathThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(70)]
        [DisplayName("[SH] Mind Blast")]
        [Description("Use Mind Blast when above this MANA percentage threshold (100 to disable)")]
        [Percentage(true)]
        public int PartyMindBlastThreshold { get; set; }

        // TALENT
        [DropdownList(new string[] { "Shadow", "Party Shadow", "Party Holy" })]
        public override string Specialization { get; set; }
    }
}