using System;
using System.Diagnostics;
using System.Threading;
using robotManager.Helpful;
using wManager.Events;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.Collections.Generic;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;
using System.ComponentModel;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class Shaman : IClassRotation
    {
        public static ShamanSettings settings;
        protected Cast cast;

        protected TotemManager totemManager = new TotemManager();

        protected Stopwatch _pullMeleeTimer = new Stopwatch();
        protected Stopwatch _meleeTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected bool _fightingACaster = false;
        protected float _pullRange = 28f;
        protected int _lowManaThreshold = 20;
        protected int _mediumManaThreshold = 50;
        protected List<string> _casterEnemies = new List<string>();
        protected int _pullAttempt;

        protected Shaman specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = ShamanSettings.Current;
            cast = new Cast(LightningBolt, settings.ActivateCombatDebug, null);

            this.specialization = specialization as Shaman;
            TalentsManager.InitTalents(settings);

            RangeManager.SetRange(_pullRange);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;

            Rotation();
        }


        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        ApplyEnchantWeapon();
                        totemManager.CheckForTotemicCall();
                    }

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();
                }
                catch (Exception arg)
                {
                    Logging.WriteError("ERROR: " + arg, true);
                }
                Thread.Sleep(ToolBox.GetLatency() + settings.ThreadSleepCycle);
            }
            Logger.Log("Stopped.");
        }

        protected virtual void BuffRotation()
        {
            if (!Me.HaveBuff("Ghost Wolf") && !Me.IsCast)
            {
                // Ghost Wolf
                if (settings.GhostWolfMount
                    && wManager.wManagerSetting.CurrentSetting.GroundMountName == ""
                    && GhostWolf.KnownSpell)
                    ToolBox.SetGroundMount(GhostWolf.Name);

                // Lesser Healing Wave OOC
                if (Me.HealthPercent < settings.OOCHealThreshold)
                    if (cast.OnSelf(LesserHealingWave))
                        return;

                // Healing Wave OOC
                if (Me.HealthPercent < settings.OOCHealThreshold)
                    if (cast.OnSelf(HealingWave))
                        return;

                // Water Shield
                if (!Me.HaveBuff("Water Shield")
                    && !Me.HaveBuff("Lightning Shield")
                    && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < 20))
                    if (cast.Normal(WaterShield))
                        return;
            }
        }

        protected virtual void Pull()
        {
            // Check if caster
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Water Shield
            if (!Me.HaveBuff("Water Shield")
                && !Me.HaveBuff("Lightning Shield")
                && (settings.UseWaterShield || !settings.UseLightningShield) || Me.ManaPercentage < _lowManaThreshold)
                if (cast.Normal(WaterShield))
                    return;

            // Ligntning Shield
            if (Me.ManaPercentage > _lowManaThreshold
                && !Me.HaveBuff("Lightning Shield")
                && !Me.HaveBuff("Water Shield")
                && settings.UseLightningShield
                && (!WaterShield.KnownSpell || !settings.UseWaterShield))
                if (cast.Normal(LightningShield))
                    return;
        }

        protected virtual void CombatRotation()
        {
            WoWUnit Target = ObjectManager.Target;
            bool _isPoisoned = ToolBox.HasPoisonDebuff();
            bool _hasDisease = ToolBox.HasDiseaseDebuff();

            // Healing Wave + Lesser Healing Wave
            if (Me.HealthPercent < settings.HealThreshold
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.OnSelf(LesserHealingWave) || cast.OnSelf(HealingWave))
                    return;

            // Cure Poison
            if (settings.CurePoison
                && _isPoisoned 
                && Me.ManaPercentage > _lowManaThreshold)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CurePoison))
                    return;
            }

            // Cure Disease
            if (settings.CureDisease
                && _hasDisease 
                && Me.ManaPercentage > _lowManaThreshold)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CureDisease))
                    return;
            }

            // Bloodlust
            if (!Me.HaveBuff("Bloodlust")
                && Target.HealthPercent > 80)
                if (cast.Normal(Bloodlust))
                    return;

            // Water Shield
            if (!Me.HaveBuff("Water Shield")
                && !Me.HaveBuff("Lightning Shield")
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage <= _lowManaThreshold))
                if (cast.Normal(WaterShield))
                    return;

            // Lightning Shield
            if (Me.ManaPercentage > _lowManaThreshold
                && !Me.HaveBuff("Lightning Shield")
                && !Me.HaveBuff("Water Shield")
                && settings.UseLightningShield
                && (!WaterShield.KnownSpell || !settings.UseWaterShield))
                if (cast.Normal(LightningShield))
                    return;
        }

        protected Spell LightningBolt = new Spell("Lightning Bolt");
        protected Spell HealingWave = new Spell("Healing Wave");
        protected Spell LesserHealingWave = new Spell("Lesser Healing Wave");
        protected Spell RockbiterWeapon = new Spell("Rockbiter Weapon");
        protected Spell EarthShock = new Spell("Earth Shock");
        protected Spell FlameShock = new Spell("Flame Shock");
        protected Spell FrostShock = new Spell("Frost Shock");
        protected Spell LightningShield = new Spell("Lightning Shield");
        protected Spell WaterShield = new Spell("Water Shield");
        protected Spell GhostWolf = new Spell("Ghost Wolf");
        protected Spell CurePoison = new Spell("Cure Poison");
        protected Spell CureDisease = new Spell("Cure Disease");
        protected Spell WindfuryWeapon = new Spell("Windfury Weapon");
        protected Spell Stormstrike = new Spell("Stormstrike");
        protected Spell ShamanisticRage = new Spell("Shamanistic Rage");
        protected Spell Attack = new Spell("Attack");
        protected Spell ElementalMastery = new Spell("Elemental Mastery");
        protected Spell ChainLightning = new Spell("Chain Lightning");
        protected Spell Bloodlust = new Spell("Bloodlust");

        protected virtual void ApplyEnchantWeapon()
        {
            if (!HaveMainHandEnchant || HaveOffHandWheapon && !HaveOffHandEnchant)
            {
                if (!WindfuryWeapon.KnownSpell && RockbiterWeapon.KnownSpell)
                    cast.Normal(RockbiterWeapon);

                if (WindfuryWeapon.KnownSpell)
                    cast.Normal(WindfuryWeapon);
            }
        }

        protected bool HaveMainHandEnchant => Lua.LuaDoString<bool>
                (@"local hasMainHandEnchant, _, _, _, _, _, _, _, _ = GetWeaponEnchantInfo()
                    if (hasMainHandEnchant) then 
                       return '1'
                    else
                       return '0'
                    end");

        protected bool HaveOffHandEnchant => Lua.LuaDoString<bool>
                (@"local _, _, _, _, hasOffHandEnchant, _, _, _, _ = GetWeaponEnchantInfo()
                    if (hasOffHandEnchant) then 
                       return '1'
                    else
                       return '0'
                    end");

        protected bool HaveOffHandWheapon => Lua.LuaDoString<bool>(@"local hasWeapon = OffhandHasWeapon()
                return hasWeapon");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _fightingACaster = false;
            _meleeTimer.Reset();
            _pullMeleeTimer.Reset();
            _pullAttempt = 0;
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            RangeManager.SetRange(_pullRange);
        }
    }
}