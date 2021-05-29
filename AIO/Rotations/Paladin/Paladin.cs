using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class Paladin : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        private static List<BlessingBuff> RecordedBlessingBuffs { get; set; } = new List<BlessingBuff>();

        public static PaladinSettings settings;

        protected Cast cast;

        protected Stopwatch _purifyTimer = new Stopwatch();
        protected Stopwatch _cleanseTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected static int _manaSavePercent;
        private Timer _moveBehindTimer = new Timer(500);
        protected Timer _combatMeleeTimer = new Timer();

        protected Paladin specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = PaladinSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);
            cast = new Cast(HolyLight, null, settings);
            
            this.specialization = specialization as Paladin;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            if (specialization.RotationType == Enums.RotationType.Party && settings.PartyDetectSpecs)
                AIOParty.ActivateSpecRecord = true;
            
            _manaSavePercent = System.Math.Max(20, settings.ManaSaveLimitPercent);

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
                    if (StatusChecker.OOCMounted())
                        // Crusader Aura
                        if (CrusaderAura.KnownSpell
                            && !Me.HaveBuff("Crusader Aura"))
                            cast.OnTarget(CrusaderAura);

                    if (StatusChecker.OutOfCombat(RotationRole))
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.PullRotation();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();

                    if (StatusChecker.InCombatNoTarget())
                        specialization.CombatNoTarget();

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
            // PARTY buff rotations
            if (specialization.RotationType == Enums.RotationType.Party)
            {
                // Aura
                if (!Me.HaveBuff(settings.PartyAura)
                    && cast.OnSelf(AIOSpell.GetSpellByName(settings.PartyAura)))
                    return;

                // PARTY Resurrection
                List<AIOPartyMember> needRes = AIOParty.Group
                    .FindAll(m => m.IsDead)
                    .OrderBy(m => m.GetDistance)
                    .ToList();
                if (needRes.Count > 0 && cast.OnFocusUnit(Redemption, needRes[0]))
                    return;

                if (settings.PartyHealOOC || specialization is PaladinHolyParty)
                {
                    // PARTY Heal
                    List<AIOPartyMember> needHeal = AIOParty.Group
                        .FindAll(m => m.HealthPercent < 70)
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (needHeal.Count > 0 && cast.OnFocusUnit(HolyLight, needHeal[0]))
                        return;

                    // PARTY Flash of Light
                    List<AIOPartyMember> needFoL = AIOParty.Group
                        .FindAll(m => m.HealthPercent < 85)
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (needFoL.Count > 0 && cast.OnFocusUnit(FlashOfLight, needFoL[0]))
                        return;
                }

                // PARTY Purifiy
                WoWPlayer needsPurify = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name) || ToolBox.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;

                // Party Cleanse
                WoWPlayer needsCleanse = AIOParty.Group
                    .Find(m => ToolBox.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;

                // Blessings
                if (PartyBlessingBuffs())
                    return;

                // PARTY Drink
                if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected virtual void PullRotation()
        {
        }

        protected virtual void CombatRotation()
        {
        }

        protected virtual void CombatNoTarget()
        {
        }

        protected virtual void HealerCombat()
        {
        }

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

        public static void RecordBlessingCast(string casterName, string spellName, string targetName)
        {
            // Remove buff from same source
            while (RecordedBlessingBuffs.Exists(b => b.CasterName == casterName && b.TargetName == targetName))
            {
                BlessingBuff existingbuff = RecordedBlessingBuffs.Find(b => b.CasterName == casterName && b.TargetName == targetName);
                RecordedBlessingBuffs.Remove(existingbuff);
            }
            // Remove buff for same spell
            while (RecordedBlessingBuffs.Exists(b => b.SpellName == spellName && b.TargetName == targetName))
            {
                BlessingBuff existingbuff = RecordedBlessingBuffs.Find(b => b.SpellName == spellName && b.TargetName == targetName);
                RecordedBlessingBuffs.Remove(existingbuff);
            }

            RecordedBlessingBuffs.Add(new BlessingBuff(casterName, spellName, targetName));
            /*
            Logger.Log("************");
            RecordedBlessingBuffs.OrderBy(b => b.CasterName).ToList().ForEach(b => Logger.Log($"{b.CasterName} -> {b.TargetName} -> {b.SpellName}"));
            Logger.Log("************");
            */
        }


        private struct BlessingBuff
        {
            public BlessingBuff(string caster, string spell, string target)
            {
                CasterName = caster;
                SpellName = spell;
                TargetName = target;
            }

            public string CasterName { get; }
            public string SpellName { get; }
            public string TargetName { get; }
        }

        protected bool PartyBlessingBuffs()
        {
            AIOSpell myBuffSpell = null;
            if (specialization is RetributionParty) myBuffSpell = BlessingOfMight;
            if (specialization is PaladinHolyParty || !BlessingOfKings.KnownSpell) myBuffSpell = BlessingOfWisdom;
            if (specialization is PaladinProtectionParty) myBuffSpell = BlessingOfKings;

            if (myBuffSpell == null)
                return false;

            if (!RecordedBlessingBuffs.Exists(b => b.CasterName == Me.Name && b.TargetName == Me.Name))
            {
                if (Me.HaveBuff(myBuffSpell.Name))
                    ToolBox.CancelPlayerBuff(myBuffSpell.Name);
                if (cast.OnSelf(myBuffSpell))
                    return true;
            }
            else
            {
                if (!Me.HaveBuff(myBuffSpell.Name))
                {
                    RecordedBlessingBuffs.Remove(RecordedBlessingBuffs.Find(b => b.CasterName == Me.Name && b.TargetName == Me.Name));
                    return true;
                }
            }
            
            foreach (AIOPartyMember member in AIOParty.Group)
            {
                // Avoid paladin loop buff
                if (member.WowClass == WoWClass.Paladin
                    && AIOParty.Group.Exists(m => m.WowClass == WoWClass.Paladin && (m.HaveBuff("Drink") || m.GetDistance > 25)))
                    continue;

                List<AIOSpell> buffsForThisMember = settings.PartyDetectSpecs ? GetBlessingPerSpec(member.Specialization, member.WowClass) : GetBlessingPerClass(member.WowClass);
                if (member.IsDead 
                    || !member.IsValid 
                    || member.Guid == Me.Guid
                    || buffsForThisMember == null
                    || (settings.PartyDetectSpecs && member.Specialization == null))
                    continue;

                // check if ideal member buffs -> eg Hunter has wisdom + kings (not ideal)
                bool memberbuffsAreIdeal = true;
                int lastFoundBuffIndex = -1;
                int lastMissingBuffIndex = -1;
                for (int i = 0; i < buffsForThisMember.Count; i++)
                {
                    if (member.HaveBuff(buffsForThisMember[i].Name))
                        lastFoundBuffIndex = i;
                    else
                        lastMissingBuffIndex = i;

                    if (lastMissingBuffIndex < lastFoundBuffIndex 
                        && lastMissingBuffIndex > -1 
                        && !RecordedBlessingBuffs.Exists(b => b.CasterName == member.Name && b.TargetName == member.Name && b.SpellName == buffsForThisMember[i].Name))
                    {
                        memberbuffsAreIdeal = false;
                        break;
                    }
                }

                BlessingBuff? myBuffOnThisTarget = RecordedBlessingBuffs.Find(b => b.CasterName == Me.Name && b.TargetName == member.Name);
                if (member.HaveBuff(myBuffOnThisTarget?.SpellName) && (memberbuffsAreIdeal /*|| member.WowClass == WoWClass.Paladin*/))
                    continue;

                for (int i = 0; i < buffsForThisMember.Count; i++)
                {
                    if (!member.HaveBuff(buffsForThisMember[i].Name)
                        && cast.OnFocusUnit(buffsForThisMember[i], member))
                        return true;
                }
            }

            return false;
        }

        private List<AIOSpell> GetBlessingPerSpec(string spec, WoWClass playerClass)
        {
            if (spec == null)
                return null;

            if (spec == "Balance" 
                || spec == "Restoration" 
                || spec == "Arcane" 
                || spec == "Fire" 
                || spec == "Frost" 
                || spec == "Shadow"
                || spec == "Holy"
                || spec == "Discipline"
                || spec == "Elemental" 
                || spec == "Affliction" 
                || spec == "Demonology" 
                || spec == "Destruction")
                return new List<AIOSpell>() { BlessingOfWisdom, BlessingOfKings };

            if (spec == "Assassination" 
                || spec == "Combat" 
                || spec == "Subtlety" 
                || spec == "Arms"
                || spec == "Fury")
                return new List<AIOSpell>() { BlessingOfMight, BlessingOfKings };

            if (spec == "Feral" 
                || spec == "Beast Mastery" 
                || spec == "Marksmanship" 
                || spec == "Survival" 
                || spec == "Retribution"
                || spec == "Enhancement")
                return new List<AIOSpell>() { BlessingOfMight, BlessingOfKings, BlessingOfWisdom };

            if (spec == "Protection" && playerClass == WoWClass.Warrior)
                return new List<AIOSpell>() { BlessingOfKings, BlessingOfMight };

            if (spec == "Protection" && playerClass == WoWClass.Paladin)
                return new List<AIOSpell>() { BlessingOfKings, BlessingOfMight, BlessingOfWisdom };

            return null;
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
    }
}