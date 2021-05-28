using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class Retribution : Paladin
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Holy Light
            if (Me.HealthPercent < settings.OOCHolyLightThreshold
                && HolyLight.IsSpellUsable
                && cast.OnSelf(HolyLight))
                return;

            // Flash of Light
            if (FlashOfLight.IsSpellUsable
                && Me.HealthPercent < settings.OOCFlashHealThreshold
                && cast.OnSelf(FlashOfLight))
                return;

            // Sanctity Aura
            if (!Me.HaveBuff("Sanctity Aura")
                && !settings.RetributionAura
                && cast.OnSelf(SanctityAura))
                return;

            // Retribution Aura
            if (!Me.HaveBuff("Retribution Aura")
                && (!SanctityAura.KnownSpell || settings.RetributionAura)
                && cast.OnSelf(RetributionAura))
                return;

            // Blessing of Wisdom
            if (settings.UseBlessingOfWisdom
                && !Me.HaveBuff("Blessing of Wisdom")
                && cast.OnSelf(BlessingOfWisdom))
                return;

            // Blessing of Might
            if (!settings.UseBlessingOfWisdom
                && !Me.HaveBuff("Blessing of Might")
                && !Me.IsMounted
                && cast.OnSelf(BlessingOfMight))
                return;
        }

        protected override void PullRotation()
        {
            base.PullRotation();

            WoWUnit Target = ObjectManager.Target;

            ToolBox.CheckAutoAttack(Attack);

            // Judgement
            if ((Me.HaveBuff("Seal of Righteousness") || Me.HaveBuff("Seal of Command"))
                && Target.GetDistance < Judgement.MaxRange
                && (Me.ManaPercentage >= _manaSavePercent || Me.HaveBuff("Seal of the Crusader"))
                && cast.OnTarget(Judgement))
                return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader")
                && !Me.HaveBuff("Seal of the Crusader")
                && Me.ManaPercentage > _manaSavePercent - 20
                && settings.UseSealOfTheCrusader
                && cast.OnTarget(SealOfTheCrusader))
                return;

            // Seal of Righteousness
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !settings.UseSealOfTheCrusader
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell)
                && cast.OnTarget(SealOfRighteousness))
                return;

            // Seal of Command
            if (!Me.HaveBuff("Seal of Command")
                && !Me.HaveBuff("Seal of the Crusader")
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && settings.UseSealOfCommand
                && SealOfCommand.KnownSpell
                && cast.OnTarget(SealOfCommand))
                return;

            // Seal of Command Rank 1
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !Me.HaveBuff("Seal of Command")
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && Me.Mana < _manaSavePercent
                && cast.OnTarget(SealOfCommandRank1))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;

            ToolBox.CheckAutoAttack(Attack);

            // Devotion Aura multi
            if (ObjectManager.GetNumberAttackPlayer() > 1
                && settings.DevoAuraOnMulti
                && !Me.HaveBuff("Devotion Aura")
                && cast.OnSelf(DevotionAura))
                return;

            // Devotion Aura
            if (!Me.HaveBuff("Devotion Aura")
                && !SanctityAura.KnownSpell
                && !RetributionAura.KnownSpell
                && cast.OnSelf(DevotionAura))
                return;

            // Sanctity Aura
            if (!Me.HaveBuff("Sanctity Aura")
                && !settings.RetributionAura
                && ObjectManager.GetNumberAttackPlayer() <= 1
                && cast.OnSelf(SanctityAura))
                return;

            // Retribution Aura
            if (!Me.HaveBuff("Retribution Aura")
                && (!SanctityAura.KnownSpell || settings.RetributionAura)
                && ObjectManager.GetNumberAttackPlayer() <= 1
                && cast.OnSelf(RetributionAura))
                return;

            // Lay on Hands
            if (Me.HealthPercent < 10
                && cast.OnSelf(LayOnHands))
                return;

            // Hammer of Justice
            if (Me.HealthPercent < 50
                && Me.ManaPercentage > _manaSavePercent
                && cast.OnTarget(HammerOfJustice))
                return;

            // Holy Light / Flash of Light
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && settings.HealDuringCombat)
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
            if (Me.ManaPercentage > _manaSavePercent
                && ObjectManager.GetNumberAttackPlayer() > 1
                && cast.OnSelf(AvengingWrath))
                return;

            // Exorcism
            if ((Target.CreatureTypeTarget == "Undead" || Target.CreatureTypeTarget == "Demon")
                && settings.UseExorcism
                && cast.OnTarget(Exorcism))
                return;

            // Judgement (Crusader)
            if (Me.HaveBuff("Seal of the Crusader")
                && Target.GetDistance < Judgement.MaxRange
                && cast.OnTarget(Judgement))
                return;

            // Judgement
            if ((Me.HaveBuff("Seal of Righteousness") || Me.HaveBuff("Seal of Command"))
                && Target.GetDistance < Judgement.MaxRange
                && (Me.ManaPercentage >= _manaSavePercent || Me.HaveBuff("Seal of the Crusader"))
                && cast.OnTarget(Judgement))
                return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader")
                && !Me.HaveBuff("Seal of the Crusader")
                && Me.ManaPercentage > _manaSavePercent - 20
                && Target.IsAlive
                && settings.UseSealOfTheCrusader
                && cast.OnSelf(SealOfTheCrusader))
                return;

            // Seal of Righteousness
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell)
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Command
            if (!Me.HaveBuff("Seal of Command")
                && !Me.HaveBuff("Seal of the Crusader")
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && settings.UseSealOfCommand
                && cast.OnSelf(SealOfCommand))
                return;

            // Seal of Command Rank 1
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !Me.HaveBuff("Seal of Command")
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && Me.Mana < _manaSavePercent
                && cast.OnSelf(SealOfCommandRank1))
                return;

            // Crusader Strike
            if (Me.ManaPercentage > 10
                && cast.OnTarget(CrusaderStrike))
                return;

            // Hammer of Wrath
            if (settings.UseHammerOfWrath
                && cast.OnTarget(HammerOfWrath))
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
    }
}
