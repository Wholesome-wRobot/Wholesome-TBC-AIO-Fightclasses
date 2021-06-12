using System;
using System.Collections.Generic;
using MarsSettingsGUI;
using System.ComponentModel;
using System.Configuration;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Settings
{
    [Serializable]
    public abstract class BaseSettings : robotManager.Helpful.Settings
    {
        SettingsWindow settingWindow;

        [Setting]
        [DefaultValue(100)]
        [Category("General")]
        [DisplayName("Refresh rate")]
        [Description("Set this value higher if you have low CPU performance. In doubt, do not change this value")]
        public int ThreadSleepCycle { get; set; }

        [Category("General")]
        [DefaultValue(true)]
        [DisplayName("Combat log")]
        [Description("Activate combat log")]
        public bool ActivateCombatLog { get; set; }

        [Category("General")]
        [DefaultValue(false)]
        [DisplayName("Combat log debug")]
        [Description("Activate combat log debug")]
        public bool ActivateCombatDebug { get; set; }

        [Category("General")]
        [DefaultValue(false)]
        [DisplayName("Detect Immunities")]
        [Description("If activated, will ban spells that your target is immune to for the duration of the fight")]
        public bool AutoDetectImmunities { get; set; }

        [Category("General")]
        [DefaultValue(true)]
        [DisplayName("Racial Skills")]
        [Description("Use Racial Skills ")]
        public bool UseRacialSkills { get; set; }

        [Setting]
        [Category("Rotation")]
        [DisplayName("Talents Codes")]
        [Description("Use a talent calculator to generate your own codes. Do not modify if you are not sure.")]
        public List<string> TalentCodes { get; set; }

        [Setting]
        [Category("Rotation")]
        [DefaultValue(true)]
        [DisplayName("Use default talents")]
        [Description("If True, Make sure your talents match the default talents, or reset your talents.")]
        public bool UseDefaultTalents { get; set; }

        [Setting]
        [Category("Rotation")]
        [DefaultValue(true)]
        [DisplayName("Auto assign talents")]
        [Description("Will automatically assign your talent points.")]
        public bool AssignTalents { get; set; }
        
        [Setting]
        [Category("Rotation")]
        [DefaultValue("Auto")]
        [DisplayName("Rotation")]
        [Description("Choose your specialization")]
        public abstract string Specialization { get; set; }

        [Setting]
        [Category("Party")]
        [DefaultValue("")]
        [DisplayName("Party Drink")]
        [Description("In Party mode, the regen state is disabled. Set a drink name here if you want the AIO to drink. Beware, movement and fight states will interrupt the drinking. Leave empty to disable.")]
        public string PartyDrinkName { get; set; }

        [Category("Party")]
        [DefaultValue(40)]
        [DisplayName("Drink Threshold")]
        [Description("Mana threshold under which the AIO will try to drink")]
        [Percentage(true)]
        public int PartyDrinkThreshold { get; set; }

        protected BaseSettings()
        {
            ActivateCombatDebug = false;
            ThreadSleepCycle = 100;
            AssignTalents = true;
            TalentCodes = new List<string> { };
            UseDefaultTalents = true;
            Specialization = "Auto";
            AutoDetectImmunities = false;
            ActivateCombatLog = true;
            UseRacialSkills = true;

            PartyDrinkName = "";
            PartyDrinkThreshold = 40;
        }
                
        protected virtual void OnUpdate() {  }

        public void ShowConfiguration()
        {
            settingWindow = new SettingsWindow(this, ObjectManager.Me.WowClass.ToString());
            settingWindow.MaxWidth = 800;
            settingWindow.MaxHeight = 800;
            settingWindow.SaveWindowPosition = true;
            settingWindow.Title = $"{ObjectManager.Me.Name} - {ObjectManager.Me.WowClass} ({Specialization})";
            settingWindow.ShowDialog();
            OnUpdate();
        }
    }
}
