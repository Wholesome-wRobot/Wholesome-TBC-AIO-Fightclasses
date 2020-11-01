using System;
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
        public static PaladinSettings settings;

        protected Stopwatch _purifyTimer = new Stopwatch();
        protected Stopwatch _cleanseTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;

        private static int _manaSavePercent;

        protected Paladin specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = PaladinSettings.Current;

            this.specialization = specialization as Paladin;
            Talents.InitTalents(settings);

            _manaSavePercent = System.Math.Max(20, settings.ManaSaveLimitPercent);

            FightEvents.OnFightEnd += FightEndHandler;

            Rotation();
        }


        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
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
                            Cast(CrusaderAura);

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.PullRotation();

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
            // Holy Light
            if (Me.HealthPercent < settings.OOCHolyLightThreshold
                && HolyLight.IsSpellUsable)
                if (Cast(HolyLight, true))
                    return;

            // Flash of Light
            if (FlashOfLight.IsSpellUsable
                && Me.HealthPercent < settings.OOCFlashHealThreshold)
                if (Cast(FlashOfLight, true))
                    return;

            // Sanctity Aura
            if (!Me.HaveBuff("Sanctity Aura")
                && !settings.RetributionAura)
                if (Cast(SanctityAura))
                    return;

            // Retribution Aura
            if (!Me.HaveBuff("Retribution Aura") 
                && (!SanctityAura.KnownSpell || settings.RetributionAura))
                if (Cast(RetributionAura))
                    return;

            // Blessing of Wisdom
            if (settings.UseBlessingOfWisdom 
                && !Me.HaveBuff("Blessing of Wisdom")
                && BlessingOfWisdom.IsSpellUsable)
                if (Cast(BlessingOfWisdom, true))
                    return;

            // Blessing of Might
            if (!settings.UseBlessingOfWisdom 
                && !Me.HaveBuff("Blessing of Might")
                && !Me.IsMounted 
                && BlessingOfMight.IsSpellUsable)
                if (Cast(BlessingOfMight, true))
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
                if (Cast(Judgement))
                    return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader")
                && !Me.HaveBuff("Seal of the Crusader")
                && Me.ManaPercentage > _manaSavePercent - 20
                && settings.UseSealOfTheCrusader)
                if (Cast(SealOfTheCrusader))
                    return;

            // Seal of Righteousness
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !settings.UseSealOfTheCrusader
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell))
                if (Cast(SealOfRighteousness))
                    return;

            // Seal of Command
            if (!Me.HaveBuff("Seal of Command")
                && !Me.HaveBuff("Seal of the Crusader")
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && settings.UseSealOfCommand
                && SealOfCommand.KnownSpell)
                if (Cast(SealOfCommand))
                    return;

            // Seal of Command Rank 1
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !Me.HaveBuff("Seal of Command")
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && SealOfCommand.KnownSpell
                && Me.Mana < _manaSavePercent)
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
                if (Cast(DevotionAura))
                    return;

            // Devotion Aura
            if (!Me.HaveBuff("Devotion Aura") 
                && !SanctityAura.KnownSpell 
                && !RetributionAura.KnownSpell)
                if (Cast(DevotionAura))
                    return;

            // Sanctity Aura
            if (!Me.HaveBuff("Sanctity Aura") 
                && !settings.RetributionAura
                && ObjectManager.GetNumberAttackPlayer() <= 1)
                if (Cast(SanctityAura))
                    return;

            // Retribution Aura
            if (!Me.HaveBuff("Retribution Aura") 
                && (!SanctityAura.KnownSpell || settings.RetributionAura)
                && ObjectManager.GetNumberAttackPlayer() <= 1)
                if (Cast(RetributionAura))
                    return;

            // Lay on Hands
            if (Me.HealthPercent < 10)
                if (Cast(LayOnHands))
                    return;

            // Hammer of Justice
            if (Me.HealthPercent < 50
                && Me.ManaPercentage > _manaSavePercent)
                if (Cast(HammerOfJustice))
                    return;

            // Holy Light / Flash of Light
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && settings.HealDuringCombat)
            {
                if (!HolyLight.IsSpellUsable)
                {
                    if (Me.HealthPercent < 20)
                        if (Cast(DivineShield))
                            return;
                    if (Cast(FlashOfLight))
                        return;
                }
                if (Cast(HolyLight))
                    return;
            }

            // Avenging Wrath
            if (Me.ManaPercentage > _manaSavePercent 
                && ObjectManager.GetNumberAttackPlayer() > 1)
                if (Cast(AvengingWrath))
                    return;

            // Exorcism
            if ((Target.CreatureTypeTarget == "Undead" || Target.CreatureTypeTarget == "Demon")
                && settings.UseExorcism)
                if (Cast(Exorcism))
                    return;

            // Judgement (Crusader)
            if (Me.HaveBuff("Seal of the Crusader") 
                && Target.GetDistance < 10)
            {
                if (Cast(Judgement))
                {
                    Thread.Sleep(200);
                    return;
                }
            }

            // Judgement
            if ((Me.HaveBuff("Seal of Righteousness") || Me.HaveBuff("Seal of Command"))
                && Target.GetDistance < 10
                && (Me.ManaPercentage >= _manaSavePercent || Me.HaveBuff("Seal of the Crusader")))
                if (Cast(Judgement))
                    return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader") 
                && !Me.HaveBuff("Seal of the Crusader")
                && Me.ManaPercentage > _manaSavePercent - 20 
                && Target.IsAlive 
                && settings.UseSealOfTheCrusader)
                if (Cast(SealOfTheCrusader))
                    return;

            // Seal of Righteousness
            if (!Me.HaveBuff("Seal of Righteousness") 
                && !Me.HaveBuff("Seal of the Crusader") 
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell))
                if (Cast(SealOfRighteousness))
                    return;

            // Seal of Command
            if (!Me.HaveBuff("Seal of Command") 
                && !Me.HaveBuff("Seal of the Crusader") 
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && settings.UseSealOfCommand 
                && SealOfCommand.KnownSpell)
                if (Cast(SealOfCommand))
                    return;

            // Seal of Command Rank 1
            if (!Me.HaveBuff("Seal of Righteousness") 
                && !Me.HaveBuff("Seal of the Crusader") 
                && !Me.HaveBuff("Seal of Command") 
                && !SealOfCommand.IsSpellUsable 
                && !SealOfRighteousness.IsSpellUsable
                && SealOfCommand.KnownSpell 
                && Me.Mana < _manaSavePercent)
            {
                Lua.RunMacroText("/cast Seal of Command(Rank 1)");
                return;
            }

            // Crusader Strike
            if (Me.ManaPercentage > 10)
                if (Cast(CrusaderStrike))
                    return;

            // Hammer of Wrath
            if (settings.UseHammerOfWrath)
                if (Cast(HammerOfWrath))
                    return;

            // Purify
            if ((ToolBox.HasPoisonDebuff() || ToolBox.HasDiseaseDebuff()) && Purify.IsSpellUsable &&
                (_purifyTimer.ElapsedMilliseconds > 10000 || _purifyTimer.ElapsedMilliseconds <= 0))
            {
                _purifyTimer.Restart();
                Thread.Sleep(Main.humanReflexTime);
                Cast(Purify, true);
                return;
            }

            // Cleanse
            if (ToolBox.HasMagicDebuff() && (_cleanseTimer.ElapsedMilliseconds > 10000 || _cleanseTimer.ElapsedMilliseconds <= 0)
                && Cleanse.IsSpellUsable)
            {
                _cleanseTimer.Restart();
                Thread.Sleep(Main.humanReflexTime);
                Cast(Cleanse, true);
                return;
            }
        }

        protected Spell SealOfRighteousness = new Spell("Seal of Righteousness");
        protected Spell SealOfTheCrusader = new Spell("Seal of the Crusader");
        protected Spell SealOfCommand = new Spell("Seal of Command");
        protected Spell HolyLight = new Spell("Holy Light");
        protected Spell DevotionAura = new Spell("Devotion Aura");
        protected Spell BlessingOfMight = new Spell("Blessing of Might");
        protected Spell Judgement = new Spell("Judgement");
        protected Spell LayOnHands = new Spell("Lay on Hands");
        protected Spell HammerOfJustice = new Spell("Hammer of Justice");
        protected Spell RetributionAura = new Spell("Retribution Aura");
        protected Spell Exorcism = new Spell("Exorcism");
        protected Spell ConcentrationAura = new Spell("Concentration Aura");
        protected Spell SanctityAura = new Spell("Sanctity Aura");
        protected Spell FlashOfLight = new Spell("Flash of Light");
        protected Spell BlessingOfWisdom = new Spell("Blessing of Wisdom");
        protected Spell DivineShield = new Spell("Divine Shield");
        protected Spell Cleanse = new Spell("Cleanse");
        protected Spell Purify = new Spell("Purify");
        protected Spell CrusaderStrike = new Spell("Crusader Strike");
        protected Spell HammerOfWrath = new Spell("Hammer of Wrath");
        protected Spell Attack = new Spell("Attack");
        protected Spell CrusaderAura = new Spell("Crusader Aura");
        protected Spell AvengingWrath = new Spell("Avenging Wrath");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _purifyTimer.Reset();
            _cleanseTimer.Reset();
        }

        protected bool Cast(Spell s, bool onSelf = false)
        {
            CombatDebug("In cast for " + s.Name);
            if (!s.IsSpellUsable || !s.KnownSpell)
                return false;
                
            s.Launch(false, false, true, onSelf);
            return true;
        }

        protected void CombatDebug(string s)
        {
            if (settings.ActivateCombatDebug)
                Logger.CombatDebug(s);
        }
    }
}