using System;
using System.ComponentModel;
using MarsSettingsGUI;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public class WarlockSettings : BasePersistentSettings<WarlockSettings>
    {
        public WarlockSettings()
        {
            UseLifeTap = true;
            PrioritizeWandingOverSB = true;
            UseSiphonLife = false;
            UseImmolateHighLevel = true;
            UseUnendingBreath = true;
            UseDarkPact = true;
            UseFelArmor = true;
            UseIncinerate = true;
            UseSoulShatter = true;
            NumberOfSoulShards = 4;
            ActivateCombatDebug = false;
            DrainSoulLevel1 = false;
            DrainSoulHP = 40;
            AlwaysDrainSoul = false;
            HealthFunnelOOC = true;

            AutoAnguish = true;
            FelguardCleave = true;
            AutoTorment = false;
            PetInPassiveWhenOOC = true;
            HealthThresholdResummon = 30;
            ManaThresholdResummon = 20;

            PartyCurseOfTheElements = true;
            PartyLifeTapManaThreshold = 20;
            PartyLifeTapHealthThreshold = 50;
            PartySeedOfCorruptionAmount = 3;
            PartySoulShatter = true;

            Specialization = "Affliction";
        }

        // COMMON
        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Life Tap")]
        [Description("Use Life Tap")]
        public bool UseLifeTap { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Health Funnel OOC")]
        [Description("Use Health Funnel when out of combat. If OFF, will resummon your pet when it's low HP instead.")]
        public bool HealthFunnelOOC { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Soul Shatter")]
        [Description("Use Soul Shatter on multi aggro")]
        public bool UseSoulShatter { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Wand over SB")]
        [Description("Prioritize wanding over Shadow Bolt during combat to save mana")]
        public bool PrioritizeWandingOverSB { get; set; }

        [Category("Common")]
        [DefaultValue(4)]
        [DisplayName("Soul Shards")]
        [Description("Sets the minimum number of Soul Shards to have in your bags")]
        public int NumberOfSoulShards { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Unending Breath")]
        [Description("Makes sure you have Unending Breath up at all time")]
        public bool UseUnendingBreath { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Dark Pact")]
        [Description("Use Dark Pact")]
        public bool UseDarkPact { get; set; }

        [Category("Common")]
        [DefaultValue(true)]
        [DisplayName("Fel Armor")]
        [Description("Use Fel Armor instead of Demon Armor")]
        public bool UseFelArmor { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Drain Soul lvl1")]
        [Description("Use Drain Soul level 1 instead of max rank")]
        public bool DrainSoulLevel1 { get; set; }

        [Category("Common")]
        [DefaultValue(40)]
        [DisplayName("Drain Soul HP")]
        [Description("Use Drain Soul when the enemy HP is under this threshold")]
        [Percentage(true)]
        public int DrainSoulHP { get; set; }

        [Category("Common")]
        [DefaultValue(false)]
        [DisplayName("Always Drain Soul")]
        [Description("Always try to finish the enemy with Drain Soul (synergizes with Improved Drain Soul talent)")]
        public bool AlwaysDrainSoul { get; set; }

        // AFFLICTION
        [Category("Affliction")]
        [DefaultValue(true)]
        [DisplayName("Incinerate")]
        [Description("Use Incinerate (Use Immolate at high level must be True)")]
        public bool UseIncinerate { get; set; }

        [Category("Affliction")]
        [DefaultValue(true)]
        [DisplayName("Immolate high level")]
        [Description("Keep using Immmolate once Unstable Affliction is learnt")]
        public bool UseImmolateHighLevel { get; set; }

        [Category("Affliction")]
        [DefaultValue(false)]
        [DisplayName("Siphon Life")]
        [Description("Use Siphon Life (Recommended only after TBC green gear)")]
        public bool UseSiphonLife { get; set; }

        // DEMONOLOGY
        [Category("Pet")]
        [DefaultValue(true)]
        [DisplayName("Passive Pet OOC")]
        [Description("Puts pet in passive when out of combat (can be useful if you want to ignore fights when traveling)")]
        public bool PetInPassiveWhenOOC { get; set; }

        [Category("Pet")]
        [DefaultValue(false)]
        [DisplayName("Auto torment")]
        [Description("If true, will let Torment on autocast. If false, will let the AIO manage Torment in order to save Voidwalker mana.")]
        public bool AutoTorment { get; set; }

        [Category("Pet")]
        [DefaultValue(true)]
        [DisplayName("Auto Anguish")]
        [Description("If true, will let Anguish on autocast. If false, will let the AIO manage Anguish in order to save Felguard mana.")]
        public bool AutoAnguish { get; set; }

        [Category("Pet")]
        [DefaultValue(true)]
        [DisplayName("Felguard Cleave")]
        [Description("Use Felguard's Cleave")]
        public bool FelguardCleave { get; set; }

        [Category("Pet")]
        [Percentage(true)]
        [DefaultValue(30)]
        [DisplayName("Health Resummon")]
        [Description("Resummon your pet if its health falls below this threshold")]
        public int HealthThresholdResummon { get; set; }

        [Category("Pet")]
        [Percentage(true)]
        [DefaultValue(20)]
        [DisplayName("Mana Resummon")]
        [Description("Resummon your pet if its mana falls below this threshold")]
        public int ManaThresholdResummon { get; set; }

        // PARTY

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("Curse of the Elements")]
        [Description("Use Curse of the Elements")]
        public bool PartyCurseOfTheElements { get; set; }

        [Category("Party")]
        [DefaultValue(true)]
        [DisplayName("Soulshatter")]
        [Description("Use Soulshatter (costs a Soul Shard)")]
        public bool PartySoulShatter { get; set; }

        [Category("Party")]
        [DefaultValue(20)]
        [DisplayName("Life Tap Mana")]
        [Description("Use Life Tap when your mana goes under this threshold")]
        [Percentage(true)]
        public int PartyLifeTapManaThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(50)]
        [DisplayName("Life Tap Health")]
        [Description("Allow Life Tap only if your Health is over this threshold")]
        [Percentage(true)]
        public int PartyLifeTapHealthThreshold { get; set; }

        [Category("Party")]
        [DefaultValue(3)]
        [DisplayName("Seed of Corruption")]
        [Description("Use Seed of Corruption when there is at least this amount of enemies")]
        public int PartySeedOfCorruptionAmount { get; set; }

        // TALENT
        [DropdownList(new string[] { "Affliction", "Demonology", "Party Affliction" })]
        public override string Specialization { get; set; }
    }
}