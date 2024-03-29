﻿using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class Retribution : Paladin
    {
        public Retribution(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Holy Light
            if (Me.HealthPercent < settings.SRET_OOCHolyLightThreshold
                && HolyLight.IsSpellUsable
                && cast.OnSelf(HolyLight))
                return;

            // Flash of Light
            if (FlashOfLight.IsSpellUsable
                && Me.HealthPercent < settings.SRET_OOCFlashHealThreshold
                && cast.OnSelf(FlashOfLight))
                return;

            // Sanctity Aura
            if (!Me.HasAura(SanctityAura)
                && !settings.SRET_RetributionAura
                && cast.OnSelf(SanctityAura))
                return;

            // Retribution Aura
            if (!Me.HasAura(RetributionAura)
                && (!SanctityAura.KnownSpell || settings.SRET_RetributionAura)
                && cast.OnSelf(RetributionAura))
                return;

            // Blessing of Wisdom
            if (settings.SRET_UseBlessingOfWisdom
                && !Me.HasAura(BlessingOfWisdom)
                && cast.OnSelf(BlessingOfWisdom))
                return;

            // Blessing of Might
            if (!settings.SRET_UseBlessingOfWisdom
                && !Me.HasAura(BlessingOfMight)
                && !Me.IsMounted
                && cast.OnSelf(BlessingOfMight))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            ToolBox.CheckAutoAttack(Attack);
            RangeManager.SetRangeToMelee();

            // Judgement
            if ((Me.HasAura(SealOfRighteousness) || Me.HasAura(SealOfCommand) || Me.HasAura(SealOfCommandRank1) || Me.HasAura(SealOfTheCrusader))
                && Target.GetDistance < Judgement.MaxRange
                && cast.OnTarget(Judgement))
                return;

            // Seal of the Crusader
            if (!Target.HasAura("Judgement of the Crusader")
                && !Me.HasAura(SealOfTheCrusader)
                && Me.ManaPercentage > manaSavePercent - 20
                && settings.SRET_UseSealOfTheCrusader
                && cast.OnSelf(SealOfTheCrusader))
                return;

            // Seal of Righteousness
            if (!Me.HasAura(SealOfRighteousness)
                && !Me.HasAura(SealOfTheCrusader)
                && !settings.SRET_UseSealOfTheCrusader
                && (Target.HasAura("Judgement of the Crusader") || Me.ManaPercentage > manaSavePercent || !settings.SRET_UseSealOfTheCrusader)
                && (!settings.SRET_UseSealOfCommand || !SealOfCommand.KnownSpell)
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Command
            if (!Me.HasAura(SealOfCommand)
                && !Me.HasAura(SealOfTheCrusader)
                && (Target.HasAura("Judgement of the Crusader") || Me.ManaPercentage > manaSavePercent || !settings.SRET_UseSealOfTheCrusader)
                && settings.SRET_UseSealOfCommand
                && SealOfCommand.KnownSpell
                && cast.OnSelf(SealOfCommand))
                return;

            // Seal of Command Rank 1
            if (!Me.HasAura(SealOfRighteousness)
                && !Me.HasAura(SealOfTheCrusader)
                && !Me.HasAura(SealOfCommand)
                && !Me.HasAura(SealOfCommandRank1)
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && Me.Mana < manaSavePercent
                && cast.OnSelf(SealOfCommandRank1))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            ToolBox.CheckAutoAttack(Attack);

            // Devotion Aura multi
            if (unitCache.EnemiesAttackingMe.Count > 1
                && settings.SRET_DevoAuraOnMulti
                && !Me.HasAura(DevotionAura)
                && cast.OnSelf(DevotionAura))
                return;

            // Devotion Aura
            if (!Me.HasAura(DevotionAura)
                && !SanctityAura.KnownSpell
                && !RetributionAura.KnownSpell
                && cast.OnSelf(DevotionAura))
                return;

            // Sanctity Aura
            if (!Me.HasAura(SanctityAura)
                && !settings.SRET_RetributionAura
                && unitCache.EnemiesAttackingMe.Count <= 1
                && cast.OnSelf(SanctityAura))
                return;

            // Retribution Aura
            if (!Me.HasAura(RetributionAura)
                && (!SanctityAura.KnownSpell || settings.SRET_RetributionAura)
                && unitCache.EnemiesAttackingMe.Count <= 1
                && cast.OnSelf(RetributionAura))
                return;

            // Lay on Hands
            if (Me.HealthPercent < 10
                && cast.OnSelf(LayOnHands))
                return;

            // Hammer of Justice
            if (Me.HealthPercent < 50
                && Me.ManaPercentage > manaSavePercent
                && cast.OnTarget(HammerOfJustice))
                return;

            // Holy Light / Flash of Light
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && settings.SRET_HealDuringCombat)
            {
                if (!HolyLight.IsSpellUsable)
                {
                    if (Me.HealthPercent < 20)
                        if (cast.OnSelf(DivineShield))
                            return;
                    if (cast.OnSelf(FlashOfLight))
                        return;
                }
                if (cast.OnSelf(HolyLight))
                    return;
            }

            // Avenging Wrath
            if (Me.ManaPercentage > manaSavePercent
                && unitCache.EnemiesAttackingMe.Count > 1
                && cast.OnSelf(AvengingWrath))
                return;

            // Exorcism
            if ((Target.CreatureTypeTarget == "Undead" || Target.CreatureTypeTarget == "Demon")
                && settings.SRET_UseExorcism
                && cast.OnTarget(Exorcism))
                return;

            // Judgement (Crusader)
            if (Me.HasAura(SealOfTheCrusader)
                && Target.GetDistance < Judgement.MaxRange
                && cast.OnTarget(Judgement))
                return;

            // Judgement
            if ((Me.HasAura(SealOfRighteousness) || Me.HasAura(SealOfCommand) || Me.HasAura(SealOfCommandRank1))
                && Target.GetDistance < Judgement.MaxRange
                && (Me.ManaPercentage >= manaSavePercent || Me.HasAura(SealOfTheCrusader))
                && cast.OnTarget(Judgement))
                return;

            bool targetHasJoCrusader = Target.HasAura("Judgement of the Crusader");
            // Seal of the Crusader
            if (!targetHasJoCrusader
                && !Me.HasAura(SealOfTheCrusader)
                && Me.ManaPercentage > manaSavePercent - 20
                && Target.IsAlive
                && settings.SRET_UseSealOfTheCrusader
                && cast.OnSelf(SealOfTheCrusader))
                return;

            // Seal of Righteousness
            if (!Me.HasAura(SealOfRighteousness)
                && !Me.HasAura(SealOfTheCrusader)
                && (targetHasJoCrusader || Me.ManaPercentage > manaSavePercent || !settings.SRET_UseSealOfTheCrusader)
                && (!settings.SRET_UseSealOfCommand || !SealOfCommand.KnownSpell)
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Command
            if (settings.SRET_UseSealOfCommand
                && !Me.HasAura(SealOfCommand)
                && !Me.HasAura(SealOfCommandRank1)
                && !Me.HasAura(SealOfTheCrusader)
                && (targetHasJoCrusader || Me.ManaPercentage > manaSavePercent || !settings.SRET_UseSealOfTheCrusader)
                && cast.OnSelf(SealOfCommand))
                return;

            // Seal of Command Rank 1
            if (!Me.HasAura(SealOfRighteousness)
                && !Me.HasAura(SealOfTheCrusader)
                && !Me.HasAura(SealOfCommand)
                && !Me.HasAura(SealOfCommandRank1)
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && Me.Mana < manaSavePercent
                && cast.OnSelf(SealOfCommandRank1))
                return;

            // Crusader Strike
            if (Me.ManaPercentage > 10
                && cast.OnTarget(CrusaderStrike))
                return;

            // Hammer of Wrath
            if (settings.SRET_UseHammerOfWrath
                && cast.OnTarget(HammerOfWrath))
                return;

            // Purify
            if ((WTEffects.HasPoisonDebuff() || WTEffects.HasDiseaseDebuff()) && Purify.IsSpellUsable &&
                (purifyTimer.ElapsedMilliseconds > 10000 || purifyTimer.ElapsedMilliseconds <= 0))
            {
                purifyTimer.Restart();
                Thread.Sleep(Main.humanReflexTime);
                cast.OnSelf(Purify);
                return;
            }

            // Cleanse
            if (WTEffects.HasMagicDebuff() && (cleanseTimer.ElapsedMilliseconds > 10000 || cleanseTimer.ElapsedMilliseconds <= 0)
                && Cleanse.IsSpellUsable)
            {
                cleanseTimer.Restart();
                Thread.Sleep(Main.humanReflexTime);
                cast.OnSelf(Cleanse);
                return;
            }
        }
    }
}
