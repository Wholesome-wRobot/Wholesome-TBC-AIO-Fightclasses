using System;
using System.ComponentModel;
using MarsSettingsGUI;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class MageSettings : BasePersistentSettings<MageSettings>
    {
        public MageSettings()
        {
            BackupUsingCTM = true;
            UsePolymorph = true;
            WandThreshold = 30;
            BlinkWhenBackup = true;
            UseCounterspell = true;
            UseDampenMagic = true;

            UseConeOfCold = true;
            IcyVeinMultiPull = true;
            FireblastThreshold = 30;

            ACMageArmor = true;
            ACSlow = true;
            ArcanePowerOnMulti = false;
            PoMOnMulti = false;

            UseDragonsBreath = true;
            BlastWaveOnMulti = true;

            PartyRemoveCurse = false;

            Specialization = "Frost";
        }

        // COMMON
        [Category("Common")]
        [DefaultValue(30)]
        [DisplayName("Wand Threshold")]
        [Description("Enemy HP % under which the wand should be used")]
        [Percentage(true)]
        public int WandThreshold { get; set; }

        [Category("Common")]
        [DefaultValue(30)]
        [DisplayName("Fire Blast Threshold")]
        [Description("Enemy HP % under which Fire Blast should be used")]
        [Percentage(true)]
        public int FireblastThreshold { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Use Cone of Cold")]
        [Description("Use Cone of Cold during the combat rotation")]
        public bool UseConeOfCold { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Use Polymorph")]
        [Description("Use Polymorph on multiaggro")]
        public bool UsePolymorph { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Counterspell")]
        [Description("Use Counterspell")]
        public bool UseCounterspell { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Dampen Magic")]
        [Description("Use Dampen Magic")]
        public bool UseDampenMagic { get; set; }

        // ARCANE
        [Category("Arcane")]
        [DefaultValue(true)]
        [DisplayName("Mage Armor")]
        [Description("Use Mage Armor instead of Frost/Ice Armor")]
        public bool ACMageArmor { get; set; }

        [Category("Arcane")]
        [DefaultValue(true)]
        [DisplayName("Slow")]
        [Description("Use Slow")]
        public bool ACSlow { get; set; }

        [Category("Arcane")]
        [DefaultValue(false)]
        [DisplayName("PoM on multi")]
        [Description("Use Presence of Mind on multipull only")]
        public bool PoMOnMulti { get; set; }

        [Category("Arcane")]
        [DefaultValue(false)]
        [DisplayName("AP on multi")]
        [Description("Use Arcane Power on multipull only")]
        public bool ArcanePowerOnMulti { get; set; }

        // FROST
        [Category("Frost")]
        [DefaultValue(true)]
        [DisplayName("Icy Veins on multi")]
        [Description("Only use Icy Veins when 2 or more enemy are pulled")]
        public bool IcyVeinMultiPull { get; set; }

        [Category("Frost")]
        [DefaultValue(true)]
        [DisplayName("Blink when backup")]
        [Description("Use Blink when backing up from the target")]
        public bool BlinkWhenBackup { get; set; }

        [Category("Frost")]
        [DefaultValue(true)]
        [DisplayName("Backup using CTM")]
        [Description("If set to True, will backup using Click To Move. If false, will use the keyboard")]
        public bool BackupUsingCTM { get; set; }

        // FIRE
        [Category("Fire")]
        [DefaultValue(true)]
        [DisplayName("Dragon's Breath")]
        [Description("Use Dragon's Breath")]
        public bool UseDragonsBreath { get; set; }

        [Category("Fire")]
        [DefaultValue(true)]
        [DisplayName("Blast Wave on multi")]
        [Description("Use Blast Wave on multipull")]
        public bool BlastWaveOnMulti { get; set; }

        //PARTY
        [Category("Party")]
        [DefaultValue(false)]
        [DisplayName("Remove Curse")]
        [Description("Use Remove Curse in combat")]
        public bool PartyRemoveCurse { get; set; }

        // TALENT
        [DropdownList(new string[] { "Frost", "Arcane", "Fire", "Party Frost", "Party Arcane", "Party Fire" })]
        public override string Specialization { get; set; }
    }
}