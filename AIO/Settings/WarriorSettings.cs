using System;
using System.ComponentModel;
using MarsSettingsGUI;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class WarriorSettings : BasePersistentSettings<WarriorSettings>
    {
        public WarriorSettings()
        {
            UseHamstring = true;
            UseBloodRage = true;
            UseDemoralizingShout = true;
            UseRend = false;
            UseCleave = true;
            AlwaysPull = false;
            UseCommandingShout = false;

            PrioritizeBerserkStance = false;

            PartyTankSwitchTarget = true;
            PartyUseIntervene = true;
            PartyStandBehind = true;

            Specialization = "Fury";
        }

        // COMMON
        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Always range pull")]
        [Description("Always pull with a range weapon")]
        public bool AlwaysPull { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Hamstring humanoids")]
        [Description("Use Hamstring against humanoids to prevent them from fleeing too far")]
        public bool UseHamstring { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Bloodrage")]
        [Description("Use Bloodrage")]
        public bool UseBloodRage { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Demoralizing Shout")]
        [Description("Use Demoralizing Shout")]
        public bool UseDemoralizingShout { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Commanding Shout")]
        [Description("Use Commanding Shout instead of Battle Shout")]
        public bool UseCommandingShout { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Rend")]
        [Description("Use Rend")]
        public bool UseRend { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Cleave")]
        [Description("Use Cleave on multi aggro")]
        public bool UseCleave { get; set; }

        // FURY
        [Category("Fury")]
        [DefaultValue(false)]
        [DisplayName("Prioritize Berserker")]
        [Description("Prioritize Berserker Stance over Battle Stance")]
        public bool PrioritizeBerserkStance { get; set; }

        // PARTY
        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[Tank] Switch target")]
        [Description("Switch targets to regain aggro when tanking")]
        public bool PartyTankSwitchTarget { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[Tank] Intervene")]
        [Description("Use Intervene")]
        public bool PartyUseIntervene { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("[DPS] Stand behind")]
        [Description("Try to stand behind the target in Fury DPS")]
        public bool PartyStandBehind { get; set; }

        // TALENT
        [DropdownList(new string[] { "Fury", "Party Fury", "Party Protection" })]
        public override string Specialization { get; set; }
    }
}