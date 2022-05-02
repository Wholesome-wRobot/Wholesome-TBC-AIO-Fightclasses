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
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class Paladin : BaseRotation
    {
        protected PaladinSettings settings;
        protected Paladin specialization;
        protected Stopwatch _purifyTimer = new Stopwatch();
        protected Stopwatch _cleanseTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected int _manaSavePercent;
        private Timer _moveBehindTimer = new Timer(500);
        protected Timer _combatMeleeTimer = new Timer();

        public Paladin(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Paladin;
            settings = PaladinSettings.Current;
            BaseInit(28, HolyLight, null, settings);

            _manaSavePercent = System.Math.Max(20, settings.ManaSaveLimitPercent);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            Rotation();
        }

        public override bool AnswerReadyCheck()
        {
            return Me.ManaPercentage > settings.PartyDrinkThreshold;
        }

        public override void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;

            BaseDispose();
        }

        private void Rotation()
        {
            while (Main.IsLaunched)
            {
                try
                {
                    if (StatusChecker.OOCMounted())
                        // Crusader Aura
                        if (CrusaderAura.KnownSpell
                            && !Me.HaveBuff("Crusader Aura"))
                            cast.OnTarget(CrusaderAura);

                    if (StatusChecker.OutOfCombat(RotationRole))
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();

                    if (StatusChecker.InCombatNoTarget())
                        specialization.CombatNoTarget();

                    if (partyManager.GroupAndRaid.Any(p => p.InCombatFlagOnly && p.GetDistance < 50))
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
            // PARTY buff rotations
            if (specialization.RotationType == Enums.RotationType.Party)
            {
                // Aura
                if (!Me.HaveBuff(settings.PartyAura)
                    && cast.OnSelf(AIOSpell.GetSpellByName(settings.PartyAura)))
                    return;

                // PARTY Resurrection
                List<AIOPartyMember> needRes = partyManager.GroupAndRaid
                    .FindAll(m => m.IsDead)
                    .OrderBy(m => m.GetDistance)
                    .ToList();
                if (needRes.Count > 0 && cast.OnFocusUnit(Redemption, needRes[0]))
                    return;

                if (settings.PartyHealOOC || specialization is PaladinHolyParty || specialization is PaladinHolyRaid)
                {
                    // PARTY Heal
                    List<AIOPartyMember> needHeal = partyManager.GroupAndRaid
                        .FindAll(m => m.HealthPercent < 70)
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (needHeal.Count > 0 && cast.OnFocusUnit(HolyLight, needHeal[0]))
                        return;

                    // PARTY Flash of Light
                    List<AIOPartyMember> needFoL = partyManager.GroupAndRaid
                        .FindAll(m => m.HealthPercent < 95)
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (needFoL.Count > 0 && cast.OnFocusUnit(FlashOfLight, needFoL[0]))
                        return;
                }

                // PARTY Purifiy
                WoWPlayer needsPurify = partyManager.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name) || WTEffects.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;

                // Party Cleanse
                WoWPlayer needsCleanse = partyManager.GroupAndRaid
                    .Find(m => UnitHasCleansableDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;

                // Blessings
                if (settings.PartyBlessings && PartyBlessingBuffs())
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull() { }
        protected override void CombatRotation() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

        protected AIOSpell SealOfRighteousness = new AIOSpell("Seal of Righteousness");
        protected AIOSpell SealOfTheCrusader = new AIOSpell("Seal of the Crusader");
        protected AIOSpell SealOfCommand = new AIOSpell("Seal of Command");
        protected AIOSpell DevotionAura = new AIOSpell("Devotion Aura");
        protected AIOSpell BlessingOfMight = new AIOSpell("Blessing of Might");
        protected AIOSpell Judgement = new AIOSpell("Judgement");
        protected AIOSpell LayOnHands = new AIOSpell("Lay on Hands");
        protected AIOSpell HammerOfJustice = new AIOSpell("Hammer of Justice");
        protected AIOSpell RetributionAura = new AIOSpell("Retribution Aura");
        protected AIOSpell Exorcism = new AIOSpell("Exorcism");
        protected AIOSpell ConcentrationAura = new AIOSpell("Concentration Aura");
        protected AIOSpell SanctityAura = new AIOSpell("Sanctity Aura");
        protected AIOSpell BlessingOfWisdom = new AIOSpell("Blessing of Wisdom");
        protected AIOSpell BlessingOfKings = new AIOSpell("Blessing of Kings");
        protected AIOSpell DivineShield = new AIOSpell("Divine Shield");
        protected AIOSpell Cleanse = new AIOSpell("Cleanse");
        protected AIOSpell Purify = new AIOSpell("Purify");
        protected AIOSpell CrusaderStrike = new AIOSpell("Crusader Strike");
        protected AIOSpell HammerOfWrath = new AIOSpell("Hammer of Wrath");
        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell CrusaderAura = new AIOSpell("Crusader Aura");
        protected AIOSpell AvengingWrath = new AIOSpell("Avenging Wrath");
        protected AIOSpell SealOfCommandRank1 = new AIOSpell("Seal of Command", 1);
        protected AIOSpell Consecration = new AIOSpell("Consecration");
        protected AIOSpell ConsecrationRank1 = new AIOSpell("Consecration", 1);
        protected AIOSpell RighteousFury = new AIOSpell("Righteous Fury");
        protected AIOSpell SealOfVengeance = new AIOSpell("Seal of Vengeance");
        protected AIOSpell SealOfWisdom = new AIOSpell("Seal of Wisdom");
        protected AIOSpell HolyShield = new AIOSpell("Holy Shield");
        protected AIOSpell HolyShieldRank1 = new AIOSpell("Holy Shield", 1);
        protected AIOSpell AvengersShield = new AIOSpell("Avenger's Shield");
        protected AIOSpell AvengersShieldRank1 = new AIOSpell("Avenger's Shield", 1);
        protected AIOSpell SealOfLight = new AIOSpell("Seal of Light");
        protected AIOSpell SealOfBlood = new AIOSpell("Seal of Blood");
        protected AIOSpell DivineIllumination = new AIOSpell("Divine Illumination");
        protected AIOSpell FlashOfLight = new AIOSpell("Flash of Light");
        protected AIOSpell FlashOfLightRank6 = new AIOSpell("Flash of Light", 6);
        protected AIOSpell HolyLight = new AIOSpell("Holy Light");
        protected AIOSpell HolyLightRank5 = new AIOSpell("Holy Light", 5);
        protected AIOSpell DivineFavor = new AIOSpell("Divine Favor");
        protected AIOSpell HolyShock = new AIOSpell("Holy Shock");
        protected AIOSpell Redemption = new AIOSpell("Redemption");
        protected AIOSpell RighteousDefense = new AIOSpell("Righteous Defense");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _purifyTimer.Reset();
            _cleanseTimer.Reset();
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancel)
        {
            if (specialization is RetributionParty
                && settings.PartyStandBehind
                && _moveBehindTimer.IsReady)
            {
                if (ToolBox.StandBehindTargetCombat())
                    _moveBehindTimer = new Timer(4000);
            }
        }

        protected bool PartyBlessingBuffs()
        {
            AIOSpell myBuffSpell = null;
            if (specialization is RetributionParty) myBuffSpell = BlessingOfMight;
            if (specialization is PaladinHolyParty || !BlessingOfKings.KnownSpell) myBuffSpell = BlessingOfWisdom;
            if (specialization is PaladinProtectionParty) myBuffSpell = BlessingOfKings;

            if (myBuffSpell == null)
                return false;

            if (unitCache.Group.Length > 0)
            {
                foreach (IWoWPlayer member in unitCache.Group)
                {
                    List<AIOSpell> buffsForThisMember = GetBlessingPerClass(member.WowClass);
                    if (member.IsDead
                        || !member.IsValid
                        || member.Guid == Me.Guid
                        || buffsForThisMember == null)
                        continue;

                    foreach (AIOSpell buff in buffsForThisMember)
                    {
                        if (member.HasAura(buff))
                        {
                            continue;
                        }
                        else
                        {
                            if (cast.OnFocusUnit(buff, member))
                                break;
                        }
                    }

                }
            }

            return false;
        }

        private List<AIOSpell> GetBlessingPerClass(WoWClass playerClass)
        {
            if (playerClass == WoWClass.Druid
                || playerClass == WoWClass.Paladin)
                return new List<AIOSpell>() { BlessingOfKings, BlessingOfWisdom, BlessingOfMight };

            if (playerClass == WoWClass.Hunter)
                return new List<AIOSpell>() { BlessingOfMight, BlessingOfKings, BlessingOfWisdom };

            if (playerClass == WoWClass.Warrior
                || playerClass == WoWClass.Rogue)
                return new List<AIOSpell>() { BlessingOfMight, BlessingOfKings };

            if (playerClass == WoWClass.Mage
                || playerClass == WoWClass.Priest
                || playerClass == WoWClass.Shaman
                || playerClass == WoWClass.Warlock)
                return new List<AIOSpell>() { BlessingOfWisdom, BlessingOfKings, BlessingOfMight };

            return null;
        }

        // Returns whether the player has a debuff that is one of the following: 'Poison', 'Magic', 'Disease'
        protected bool UnitHasCleansableDebuff(string unit = "player")
        {
            return Lua.LuaDoString<bool>
                (@$"for i=1,25 do 
	                local _, _, _, _, d  = UnitDebuff('{unit}',i);
	                if (d == 'Poison' or d == 'Magic' or d == 'Disease') then
                    return true
                    end
                end");
        }
    }
}