using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class Paladin : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static PaladinSettings settings;

        protected Cast cast;

        protected Stopwatch _purifyTimer = new Stopwatch();
        protected Stopwatch _cleanseTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected List<WoWUnit> _partyEnemiesAround = new List<WoWUnit>();

        private static int _manaSavePercent;

        protected Paladin specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = PaladinSettings.Current;
            cast = new Cast(HolyLight, settings.ActivateCombatDebug, null, settings.AutoDetectImmunities);

            this.specialization = specialization as Paladin;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            _manaSavePercent = System.Math.Max(20, settings.ManaSaveLimitPercent);

            FightEvents.OnFightEnd += FightEndHandler;

            Rotation();
        }


        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
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
                            cast.Normal(CrusaderAura);

                    if (RotationType == Enums.RotationType.Party)
                        _partyEnemiesAround = ToolBox.GetSuroundingEnemies();

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.PullRotation();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();

                    if (StatusChecker.InCombatNoTarget())
                        specialization.CombatNoTarget();
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
            // Holy Light
            if (Me.HealthPercent < settings.OOCHolyLightThreshold
                && HolyLight.IsSpellUsable)
                if (cast.OnSelf(HolyLight))
                    return;

            // Flash of Light
            if (FlashOfLight.IsSpellUsable
                && Me.HealthPercent < settings.OOCFlashHealThreshold)
                if (cast.OnSelf(FlashOfLight))
                    return;

            // Sanctity Aura
            if (!Me.HaveBuff("Sanctity Aura")
                && !settings.RetributionAura)
                if (cast.Normal(SanctityAura))
                    return;

            // Retribution Aura
            if (!Me.HaveBuff("Retribution Aura") 
                && (!SanctityAura.KnownSpell || settings.RetributionAura))
                if (cast.Normal(RetributionAura))
                    return;

            // Blessing of Wisdom
            if (settings.UseBlessingOfWisdom 
                && !Me.HaveBuff("Blessing of Wisdom")
                && BlessingOfWisdom.IsSpellUsable)
                if (cast.OnSelf(BlessingOfWisdom))
                    return;

            // Blessing of Might
            if (!settings.UseBlessingOfWisdom 
                && !Me.HaveBuff("Blessing of Might")
                && !Me.IsMounted 
                && BlessingOfMight.IsSpellUsable)
                if (cast.OnSelf(BlessingOfMight))
                    return;
        }

        protected virtual void PullRotation()
        {
            WoWUnit Target = ObjectManager.Target;

            ToolBox.CheckAutoAttack(Attack);

            // Judgement
            if ((Me.HaveBuff("Seal of Righteousness") || Me.HaveBuff("Seal of Command"))
                && Judgement.IsDistanceGood
                && (Me.ManaPercentage >= _manaSavePercent || Me.HaveBuff("Seal of the Crusader")))
                if (cast.Normal(Judgement))
                    return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader")
                && !Me.HaveBuff("Seal of the Crusader")
                && Me.ManaPercentage > _manaSavePercent - 20
                && settings.UseSealOfTheCrusader)
                if (cast.Normal(SealOfTheCrusader))
                    return;

            // Seal of Righteousness
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !settings.UseSealOfTheCrusader
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell))
                if (cast.Normal(SealOfRighteousness))
                    return;

            // Seal of Command
            if (!Me.HaveBuff("Seal of Command")
                && !Me.HaveBuff("Seal of the Crusader")
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && settings.UseSealOfCommand
                && SealOfCommand.KnownSpell)
                if (cast.Normal(SealOfCommand))
                    return;

            // Seal of Command Rank 1
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !Me.HaveBuff("Seal of Command")
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && SealOfCommand.KnownSpell
                && Me.Mana < _manaSavePercent
                && !cast.BannedSpells.Contains("Seal of Command(Rank 1)"))
                    Lua.RunMacroText("/cast Seal of Command(Rank 1)");
        }


        protected virtual void CombatRotation()
        {
            WoWUnit Target = ObjectManager.Target;

            ToolBox.CheckAutoAttack(Attack);

            // Devotion Aura multi
            if (ObjectManager.GetNumberAttackPlayer() > 1 
                && settings.DevoAuraOnMulti 
                && !Me.HaveBuff("Devotion Aura"))
                if (cast.Normal(DevotionAura))
                    return;

            // Devotion Aura
            if (!Me.HaveBuff("Devotion Aura") 
                && !SanctityAura.KnownSpell 
                && !RetributionAura.KnownSpell)
                if (cast.Normal(DevotionAura))
                    return;

            // Sanctity Aura
            if (!Me.HaveBuff("Sanctity Aura") 
                && !settings.RetributionAura
                && ObjectManager.GetNumberAttackPlayer() <= 1)
                if (cast.Normal(SanctityAura))
                    return;

            // Retribution Aura
            if (!Me.HaveBuff("Retribution Aura") 
                && (!SanctityAura.KnownSpell || settings.RetributionAura)
                && ObjectManager.GetNumberAttackPlayer() <= 1)
                if (cast.Normal(RetributionAura))
                    return;

            // Lay on Hands
            if (Me.HealthPercent < 10)
                if (cast.OnSelf(LayOnHands))
                    return;

            // Hammer of Justice
            if (Me.HealthPercent < 50
                && Me.ManaPercentage > _manaSavePercent)
                if (cast.Normal(HammerOfJustice))
                    return;

            // Holy Light / Flash of Light
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && settings.HealDuringCombat)
            {
                if (!HolyLight.IsSpellUsable)
                {
                    if (Me.HealthPercent < 20)
                        if (cast.Normal(DivineShield))
                            return;
                    if (cast.OnSelf(FlashOfLight))
                        return;
                }
                if (cast.OnSelf(HolyLight))
                    return;
            }

            // Avenging Wrath
            if (Me.ManaPercentage > _manaSavePercent 
                && ObjectManager.GetNumberAttackPlayer() > 1)
                if (cast.Normal(AvengingWrath))
                    return;

            // Exorcism
            if ((Target.CreatureTypeTarget == "Undead" || Target.CreatureTypeTarget == "Demon")
                && settings.UseExorcism)
                if (cast.Normal(Exorcism))
                    return;

            // Judgement (Crusader)
            if (Me.HaveBuff("Seal of the Crusader") 
                && Target.GetDistance < 10)
            {
                if (cast.Normal(Judgement))
                {
                    Thread.Sleep(200);
                    return;
                }
            }

            // Judgement
            if ((Me.HaveBuff("Seal of Righteousness") || Me.HaveBuff("Seal of Command"))
                && Target.GetDistance < 10
                && (Me.ManaPercentage >= _manaSavePercent || Me.HaveBuff("Seal of the Crusader")))
                if (cast.Normal(Judgement))
                    return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader") 
                && !Me.HaveBuff("Seal of the Crusader")
                && Me.ManaPercentage > _manaSavePercent - 20 
                && Target.IsAlive 
                && settings.UseSealOfTheCrusader)
                if (cast.Normal(SealOfTheCrusader))
                    return;

            // Seal of Righteousness
            if (!Me.HaveBuff("Seal of Righteousness") 
                && !Me.HaveBuff("Seal of the Crusader") 
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell))
                if (cast.Normal(SealOfRighteousness))
                    return;

            // Seal of Command
            if (!Me.HaveBuff("Seal of Command") 
                && !Me.HaveBuff("Seal of the Crusader") 
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && settings.UseSealOfCommand 
                && SealOfCommand.KnownSpell)
                if (cast.Normal(SealOfCommand))
                    return;

            // Seal of Command Rank 1
            if (!Me.HaveBuff("Seal of Righteousness") 
                && !Me.HaveBuff("Seal of the Crusader") 
                && !Me.HaveBuff("Seal of Command") 
                && !SealOfCommand.IsSpellUsable 
                && !SealOfRighteousness.IsSpellUsable
                && SealOfCommand.KnownSpell 
                && Me.Mana < _manaSavePercent
                && !cast.BannedSpells.Contains("Seal of Command(Rank 1)"))
            {
                Lua.RunMacroText("/cast Seal of Command(Rank 1)");
                return;
            }

            // Crusader Strike
            if (Me.ManaPercentage > 10)
                if (cast.Normal(CrusaderStrike))
                    return;

            // Hammer of Wrath
            if (settings.UseHammerOfWrath)
                if (cast.Normal(HammerOfWrath))
                    return;

            // Purify
            if ((ToolBox.HasPoisonDebuff() || ToolBox.HasDiseaseDebuff()) && Purify.IsSpellUsable &&
                (_purifyTimer.ElapsedMilliseconds > 10000 || _purifyTimer.ElapsedMilliseconds <= 0))
            {
                _purifyTimer.Restart();
                Thread.Sleep(Main.humanReflexTime);
                cast.OnSelf(Purify);
                return;
            }

            // Cleanse
            if (ToolBox.HasMagicDebuff() && (_cleanseTimer.ElapsedMilliseconds > 10000 || _cleanseTimer.ElapsedMilliseconds <= 0)
                && Cleanse.IsSpellUsable)
            {
                _cleanseTimer.Restart();
                Thread.Sleep(Main.humanReflexTime);
                cast.OnSelf(Cleanse);
                return;
            }
        }

        protected virtual void CombatNoTarget()
        {
        }

        protected AIOSpell SealOfRighteousness = new AIOSpell("Seal of Righteousness");
        protected AIOSpell SealOfTheCrusader = new AIOSpell("Seal of the Crusader");
        protected AIOSpell SealOfCommand = new AIOSpell("Seal of Command");
        protected AIOSpell HolyLight = new AIOSpell("Holy Light");
        protected AIOSpell DevotionAura = new AIOSpell("Devotion Aura");
        protected AIOSpell BlessingOfMight = new AIOSpell("Blessing of Might");
        protected AIOSpell Judgement = new AIOSpell("Judgement");
        protected AIOSpell LayOnHands = new AIOSpell("Lay on Hands");
        protected AIOSpell HammerOfJustice = new AIOSpell("Hammer of Justice");
        protected AIOSpell RetributionAura = new AIOSpell("Retribution Aura");
        protected AIOSpell Exorcism = new AIOSpell("Exorcism");
        protected AIOSpell ConcentrationAura = new AIOSpell("Concentration Aura");
        protected AIOSpell SanctityAura = new AIOSpell("Sanctity Aura");
        protected AIOSpell FlashOfLight = new AIOSpell("Flash of Light");
        protected AIOSpell BlessingOfWisdom = new AIOSpell("Blessing of Wisdom");
        protected AIOSpell DivineShield = new AIOSpell("Divine Shield");
        protected AIOSpell Cleanse = new AIOSpell("Cleanse");
        protected AIOSpell Purify = new AIOSpell("Purify");
        protected AIOSpell CrusaderStrike = new AIOSpell("Crusader Strike");
        protected AIOSpell HammerOfWrath = new AIOSpell("Hammer of Wrath");
        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell CrusaderAura = new AIOSpell("Crusader Aura");
        protected AIOSpell AvengingWrath = new AIOSpell("Avenging Wrath");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _purifyTimer.Reset();
            _cleanseTimer.Reset();
        }
    }
}