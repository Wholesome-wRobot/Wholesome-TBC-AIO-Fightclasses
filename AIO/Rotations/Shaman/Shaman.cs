using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class Shaman : BaseRotation
    {

        protected ShamanSettings settings;
        protected Shaman specialization;
        protected TotemManager totemManager;
        protected bool fightingACaster = false;
        protected readonly float pullRange = 28f;
        protected readonly int lowManaThreshold = 20;
        protected readonly int mediumManaThreshold = 50;
        protected List<string> casterEnemies = new List<string>();
        protected Timer combatMeleeTimer = new Timer();
        private Timer _moveBehindTimer = new Timer();

        public Shaman(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Shaman;
            settings = ShamanSettings.Current;
            BaseInit(pullRange, LightningBolt, null, settings);
            totemManager = new TotemManager(cast, settings, partyManager, unitCache);

            WTSettings.AddToDoNotSellList(new List<string>
            {
                "Air Totem",
                "Earth Totem",
                "Water Totem",
                "Fire Totem"
            });

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            Rotation();
        }

        public override void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;

            BaseDispose();
        }

        public override bool AnswerReadyCheck()
        {
            return true;
        }

        private void Rotation()
        {
            while (Main.IsLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions() && !Me.HasDrinkAura && !Me.HasFoodAura)
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

                    if (unitCache.GroupAndRaid.Any(p => p.InCombatFlagOnly && p.GetDistance < 50))
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

        protected override void BuffRotation()
        {
            if (specialization.RotationType == Enums.RotationType.Party)
            {
                // PARTY Resurrection
                List<IWoWPlayer> needRes = unitCache.GroupAndRaid
                    .FindAll(m => m.IsDead)
                    .OrderBy(m => m.GetDistance)
                    .ToList();
                if (needRes.Count > 0 && cast.OnFocusUnit(AncestralSpirit, needRes[0]))
                    return;
            }
        }

        protected override void Pull() { }
        protected override void CombatRotation() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

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

        protected bool HaveMainHandEnchant => WTGear.HaveMainHandEnchant();
        protected bool HaveOffHandEnchant => WTGear.HaveOffHandEnchant();
        protected bool HaveOffHandWheapon => WTGear.HaveOffHandWeaponEquipped;

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            fightingACaster = false;
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            RangeManager.SetRange(pullRange);
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (specialization is EnhancementParty
                && settings.PartyStandBehind
                && _moveBehindTimer.IsReady)
            {
                if (ToolBox.StandBehindTargetCombat(unitCache))
                    _moveBehindTimer = new Timer(4000);
            }
        }
    }
}