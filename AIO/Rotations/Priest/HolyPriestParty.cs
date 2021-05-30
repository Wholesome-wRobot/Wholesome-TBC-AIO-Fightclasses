using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class HolyPriestParty : Priest
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Greater heal
            List<AIOPartyMember> needGreaterHeal = AIOParty.Group
                .FindAll(m => m.IsAlive && m.HealthPercent < 50)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needGreaterHeal.Count > 0 && cast.OnFocusUnit(GreaterHeal, needGreaterHeal[0]))
                return;

            // PARTY Heal
            List<AIOPartyMember> needHeal = AIOParty.Group
                .FindAll(m => m.HealthPercent < 80)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needHeal.Count > 0 && cast.OnFocusUnit(FlashHeal, needHeal[0]))
                return;

            if (!FlashHeal.KnownSpell)
            {
                // PARTY Lesser Heal
                List<AIOPartyMember> needLesserHeal = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 80)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needLesserHeal.Count > 0 && cast.OnFocusUnit(LesserHeal, needLesserHeal[0]))
                    return;
            }

            // PARTY Renew
            List<AIOPartyMember> needRenew = AIOParty.Group
                .FindAll(m => m.HealthPercent < 90 && !m.HaveBuff(Renew.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needRenew.Count > 0 && cast.OnFocusUnit(Renew, needRenew[0]))
                return;

            // PARTY Power Word Fortitude
            WoWPlayer noPWF = AIOParty.Group
                .Find(m => !m.HaveBuff(PowerWordFortitude.Name));
            if (noPWF != null && cast.OnFocusUnit(PowerWordFortitude, noPWF))
                return;

            // PARTY Divine Spirit
            WoWPlayer noDS = AIOParty.Group
                .Find(m => m.Mana > 0 && !m.HaveBuff(DivineSpirit.Name));
            if (noDS != null && cast.OnFocusUnit(DivineSpirit, noDS))
                return;

            // OOC Inner Fire
            if (!Me.HaveBuff("Inner Fire")
                && settings.UseInnerFire
                && cast.OnSelf(InnerFire))
                return;

            // PARTY Shadow Protection
            if (settings.PartyShadowProtection)
            {
                WoWPlayer noShadowProtection = AIOParty.Group
                    .Find(m => !m.HaveBuff(ShadowProtection.Name));
                if (noShadowProtection != null && cast.OnFocusUnit(ShadowProtection, noShadowProtection))
                    return;
            }

            // OOC Shadow Protection
            if (!Me.HaveBuff("Shadow Protection")
                && settings.UseShadowProtection
                && cast.OnSelf(ShadowProtection))
                return;

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;
        }

        protected override void HealerCombat()
        {
            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // Party Cure Disease
                WoWPlayer needCureDisease = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Fade
            if (AIOParty.EnemiesClose.Exists(m => m.IsTargetingMe)
                && cast.OnSelf(Fade))
                return;

            // ShadowFiend
            if (Me.ManaPercentage < 10
                && cast.OnTarget(Shadowfiend))
                return;

            // Party Dispel Magic
            if (settings.PartyDispelMagic)
            {
                WoWPlayer needDispelMagic = AIOParty.Group
                    .Find(m => ToolBox.HasMagicDebuff(m.Name));
                if (needDispelMagic != null && cast.OnFocusUnit(DispelMagic, needDispelMagic))
                    return;
            }

            // PARTY Heal
            if (!FlashHeal.KnownSpell && !GreaterHealRank7.KnownSpell)
            {
                List<AIOPartyMember> needHeal = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 60)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needHeal.Count > 0 && cast.OnFocusUnit(Heal, needHeal[0]))
                    return;
            }

            // PARTY Lesser Heal
            if (!FlashHeal.KnownSpell)
            {
                List<AIOPartyMember> needLesserHeal = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 80)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needLesserHeal.Count > 0 && cast.OnFocusUnit(LesserHeal, needLesserHeal[0]))
                    return;
            }

            // PARTY Flash heal
            List<AIOPartyMember> needFlashHeal = AIOParty.Group
                .FindAll(m => m.HealthPercent < 40)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needFlashHeal.Count > 0 && cast.OnFocusUnit(FlashHeal, needFlashHeal[0]))
                return;

            // PARTY Greater heal
            List<AIOPartyMember> needGreaterHeal = AIOParty.Group
                .FindAll(m => m.HealthPercent < 60)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needGreaterHeal.Count > 0 && cast.OnFocusUnit(GreaterHeal, needGreaterHeal[0]))
                return;

            // PARTY Shield
            List<AIOPartyMember> neeedShield = AIOParty.Group
                .FindAll(m => m.HealthPercent < 60 && !m.HaveBuff("Power Word: Shield") && !ToolBox.HasDebuff("Weakened Soul", m.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (neeedShield.Count > 0 && cast.OnFocusUnit(PowerWordShield, neeedShield[0]))
                return;

            // PARTY Prayer Healing
            List<AIOPartyMember> needPrayerOfHealing = AIOParty.Group
                .FindAll(m => m.IsAlive && m.HealthPercent < 70)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needPrayerOfHealing.Count > 2 && cast.OnTarget(PrayerOfHealing))
                return;

            // PARTY Prayer of Mending
            List<AIOPartyMember> needPrayerOfMending = AIOParty.Group
                .FindAll(m => m.IsAlive && m.HealthPercent < 70 && !m.HaveBuff(PrayerOfMending.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needPrayerOfMending.Count > 1 && cast.OnFocusUnit(PrayerOfMending, needPrayerOfMending[0]))
                return;

            // PARTY Greater Heal rank 2
            if (GreaterHealRank7.KnownSpell)
            {
                List<AIOPartyMember> needGHeal2 = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 75)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needGHeal2.Count > 0 && cast.OnFocusUnit(GreaterHealRank2, needGHeal2[0]))
                    return;
            }
            // PARTY Heal
            else
            {
                List<AIOPartyMember> needHeal70 = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 80)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needHeal70.Count > 0 && cast.OnFocusUnit(Heal, needHeal70[0]))
                    return;
            }

            
            // PARTY Renew
            List<AIOPartyMember> needRenew = AIOParty.Group
                .FindAll(m => m.HealthPercent < 90 && !m.HaveBuff(Renew.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needRenew.Count > 0 && cast.OnFocusUnit(Renew, needRenew[0]))
                return;
        }
    }
}
