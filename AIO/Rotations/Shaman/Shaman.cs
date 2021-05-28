using System;
using System.Threading;
using robotManager.Helpful;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using System.Collections.Generic;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;
using System.ComponentModel;
using Timer = robotManager.Helpful.Timer;
using System.Linq;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class Shaman : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static ShamanSettings settings;
        protected Cast cast;

        protected TotemManager totemManager = new TotemManager();

        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected bool _fightingACaster = false;
        protected float _pullRange = 28f;
        protected int _lowManaThreshold = 20;
        protected int _mediumManaThreshold = 50;
        protected List<string> _casterEnemies = new List<string>();

        private Timer _moveBehindTimer = new Timer();
        protected Timer _combatMeleeTimer = new Timer();

        protected Shaman specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = ShamanSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);
            cast = new Cast(LightningBolt, null, settings);

            this.specialization = specialization as Shaman;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            ToolBox.AddToDoNotSellList("Air Totem");
            ToolBox.AddToDoNotSellList("Earth Totem");
            ToolBox.AddToDoNotSellList("Water Totem");
            ToolBox.AddToDoNotSellList("Fire Totem");

            RangeManager.SetRange(_pullRange);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            Rotation();
        }


        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            cast.Dispose();
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions() && !ObjectManager.Me.HaveBuff("Drink") && !ObjectManager.Me.HaveBuff("Food"))
                    {
                        ApplyEnchantWeapon();
                        totemManager.CheckForTotemicCall();
                    }

                    if (StatusChecker.OutOfCombat(RotationRole))
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();

                    if (AIOParty.Group.Any(p => p.InCombatFlagOnly && p.GetDistance < 50))
                        specialization.HealerCombat();
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
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
        }

        protected virtual void HealerCombat()
        {
        }

        protected AIOSpell LightningBolt = new AIOSpell("Lightning Bolt");
        protected AIOSpell LightningBoltRank1 = new AIOSpell("Lightning Bolt", 1);
        protected AIOSpell HealingWave = new AIOSpell("Healing Wave");
        protected AIOSpell LesserHealingWave = new AIOSpell("Lesser Healing Wave");
        protected AIOSpell RockbiterWeapon = new AIOSpell("Rockbiter Weapon");
        protected AIOSpell EarthShock = new AIOSpell("Earth Shock");
        protected AIOSpell EarthShockRank1 = new AIOSpell("Earth Shock", 1);
        protected AIOSpell FlameShock = new AIOSpell("Flame Shock");
        protected AIOSpell FrostShock = new AIOSpell("Frost Shock");
        protected AIOSpell LightningShield = new AIOSpell("Lightning Shield");
        protected AIOSpell WaterShield = new AIOSpell("Water Shield");
        protected AIOSpell GhostWolf = new AIOSpell("Ghost Wolf");
        protected AIOSpell CurePoison = new AIOSpell("Cure Poison");
        protected AIOSpell CureDisease = new AIOSpell("Cure Disease");
        protected AIOSpell WindfuryWeapon = new AIOSpell("Windfury Weapon");
        protected AIOSpell Stormstrike = new AIOSpell("Stormstrike");
        protected AIOSpell ShamanisticRage = new AIOSpell("Shamanistic Rage");
        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell ElementalMastery = new AIOSpell("Elemental Mastery");
        protected AIOSpell ChainLightning = new AIOSpell("Chain Lightning");
        protected AIOSpell Bloodlust = new AIOSpell("Bloodlust");
        protected AIOSpell EarthShield = new AIOSpell("Earth Shield");
        protected AIOSpell ChainHeal = new AIOSpell("Chain Heal");
        protected AIOSpell NaturesSwiftness = new AIOSpell("Nature\'s Swiftness");
        protected AIOSpell AncestralSpirit = new AIOSpell("Ancestral Spirit");

        protected virtual void ApplyEnchantWeapon()
        {
            if (!HaveMainHandEnchant || HaveOffHandWheapon && !HaveOffHandEnchant)
            {
                if (!WindfuryWeapon.KnownSpell && RockbiterWeapon.KnownSpell)
                    cast.OnSelf(RockbiterWeapon);

                if (WindfuryWeapon.KnownSpell)
                    cast.OnSelf(WindfuryWeapon);
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
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            RangeManager.SetRange(_pullRange);
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (specialization is EnhancementParty
                && settings.PartyStandBehind
                && _moveBehindTimer.IsReady)
            {
                if (ToolBox.StandBehindTargetCombat())
                    _moveBehindTimer = new Timer(4000);
            }
        }
    }
}