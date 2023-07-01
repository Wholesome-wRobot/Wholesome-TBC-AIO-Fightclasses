using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class Priest : BaseRotation
    {
        protected PriestSettings settings;
        protected Priest specialization;
        protected Stopwatch dispelTimer = new Stopwatch();
        protected bool iCanUseWand = WTGear.HaveRangedWeaponEquipped;
        protected int innerManaSaveThreshold = 20;
        private readonly float _defaultRange = 28;

        public Priest(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Priest;
            settings = PriestSettings.Current;
            BaseInit(_defaultRange, Smite, UseWand, settings);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;

            Rotation();
        }

        public override void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;

            BaseDispose();
        }

        private void Rotation()
        {
            while (Main.IsLaunched)
            {
                try
                {
                    if (Me.HasAura("Spirit of Redemption"))
                    {
                        // PARTY Greater heal
                        List<IWoWPlayer> needGreaterHealSR = unitCache.GroupAndRaid
                            .FindAll(m => m.HealthPercent < 100)
                            .OrderBy(m => m.HealthPercent)
                            .ToList();
                        if (needGreaterHealSR.Count > 0 && cast.OnFocusUnit(GreaterHeal, needGreaterHealSR[0]))
                            continue;

                        // PARTY Heal
                        List<IWoWPlayer> needHealSR = unitCache.GroupAndRaid
                            .FindAll(m => m.HealthPercent < 100)
                            .OrderBy(m => m.HealthPercent)
                            .ToList();
                        if (!GreaterHeal.KnownSpell && needHealSR.Count > 0 && cast.OnFocusUnit(FlashHeal, needHealSR[0]))
                            continue;
                    }

                    if (StatusChecker.OutOfCombat(RotationRole))
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();

                    if (unitCache.GroupAndRaid.Any(p => p.InCombatFlagOnly && p.GetDistance < 50) || Me.HasAura("Spirit of Redemption"))
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
                if (needRes.Count > 0 && cast.OnFocusUnit(Resurrection, needRes[0]))
                    return;

                // Party Cure Disease
                IWoWPlayer needCureDisease = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;

                // Party Dispel Magic
                IWoWPlayer needDispelMagic = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needDispelMagic != null && cast.OnFocusUnit(DispelMagic, needDispelMagic))
                    return;
            }
        }

        protected override void Pull() { }
        protected override void CombatRotation() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

        protected AIOSpell Smite = new AIOSpell("Smite");
        protected AIOSpell LesserHeal = new AIOSpell("Lesser Heal");
        protected AIOSpell PowerWordFortitude = new AIOSpell("Power Word: Fortitude");
        protected AIOSpell PrayerOfFortitude = new AIOSpell("Prayer of Fortitude");
        protected AIOSpell PowerWordShield = new AIOSpell("Power Word: Shield");
        protected AIOSpell ShadowWordPain = new AIOSpell("Shadow Word: Pain");
        protected AIOSpell ShadowWordDeath = new AIOSpell("Shadow Word: Death");
        protected AIOSpell UseWand = new AIOSpell("Shoot");
        protected AIOSpell Renew = new AIOSpell("Renew");
        protected AIOSpell RenewRank8 = new AIOSpell("Renew", 8);
        protected AIOSpell RenewRank10 = new AIOSpell("Renew", 10);
        protected AIOSpell MindBlast = new AIOSpell("Mind Blast");
        protected AIOSpell InnerFire = new AIOSpell("Inner Fire");
        protected AIOSpell CureDisease = new AIOSpell("Cure Disease");
        protected AIOSpell PsychicScream = new AIOSpell("Psychic Scream");
        protected AIOSpell Heal = new AIOSpell("Heal");
        protected AIOSpell GreaterHeal = new AIOSpell("Greater Heal");
        protected AIOSpell GreaterHealRank2 = new AIOSpell("Greater Heal", 2);
        protected AIOSpell GreaterHealRank7 = new AIOSpell("Greater Heal", 7);
        protected AIOSpell MindFlay = new AIOSpell("Mind Flay");
        protected AIOSpell HolyFire = new AIOSpell("Holy Fire");
        protected AIOSpell DispelMagic = new AIOSpell("Dispel Magic");
        protected AIOSpell FlashHeal = new AIOSpell("Flash Heal");
        protected AIOSpell VampiricEmbrace = new AIOSpell("Vampiric Embrace");
        protected AIOSpell Shadowguard = new AIOSpell("Shadowguard");
        protected AIOSpell ShadowProtection = new AIOSpell("Shadow Protection");
        protected AIOSpell PrayerOfShadowProtection = new AIOSpell("Prayer of Shadow Protection");
        protected AIOSpell Shadowform = new AIOSpell("Shadowform");
        protected AIOSpell VampiricTouch = new AIOSpell("Vampiric Touch");
        protected AIOSpell InnerFocus = new AIOSpell("Inner Focus");
        protected AIOSpell Shadowfiend = new AIOSpell("Shadowfiend");
        protected AIOSpell Silence = new AIOSpell("Silence");
        protected AIOSpell DevouringPlague = new AIOSpell("Devouring Plague");
        protected AIOSpell Resurrection = new AIOSpell("Resurrection");
        protected AIOSpell PrayerOfHealing = new AIOSpell("Prayer of Healing");
        protected AIOSpell PrayerOfMending = new AIOSpell("Prayer of Mending");
        protected AIOSpell Fade = new AIOSpell("Fade");
        protected AIOSpell CircleOfHealing = new AIOSpell("Circle of Healing");
        protected AIOSpell MassDispel = new AIOSpell("Mass Dispel");
        protected AIOSpell DivineSpirit = new AIOSpell("Divine Spirit");
        protected AIOSpell PrayerOfSpirit = new AIOSpell("Prayer of Spirit");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            dispelTimer.Reset();
            iCanUseWand = false;
            RangeManager.SetRange(_defaultRange);
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            iCanUseWand = WTGear.HaveRangedWeaponEquipped;
        }
    }
}