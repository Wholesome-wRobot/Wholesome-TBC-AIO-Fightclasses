using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class HolyPriestParty : Priest
    {
        public HolyPriestParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Healer;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasAura("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // PARTY Greater heal
                List<IWoWPlayer> needGreaterHeal = unitCache.GroupAndRaid
                    .FindAll(m => m.IsAlive && m.HealthPercent < 50)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needGreaterHeal.Count > 0 && cast.OnFocusUnit(GreaterHeal, needGreaterHeal[0]))
                    return;

                // PARTY Heal
                List<IWoWPlayer> needHeal = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 80)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needHeal.Count > 0 && cast.OnFocusUnit(FlashHeal, needHeal[0]))
                    return;

                if (!FlashHeal.KnownSpell)
                {
                    // PARTY Lesser Heal
                    List<IWoWPlayer> needLesserHeal = unitCache.GroupAndRaid
                        .FindAll(m => m.HealthPercent < 80)
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (needLesserHeal.Count > 0 && cast.OnFocusUnit(LesserHeal, needLesserHeal[0]))
                        return;
                }

                // PARTY Renew
                List<IWoWPlayer> needRenew = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 90 && !m.HasAura(Renew))
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needRenew.Count > 0 && cast.OnFocusUnit(Renew, needRenew[0]))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void HealerCombat()
        {
            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // Party Cure Disease
                IWoWPlayer needCureDisease = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Fade
            if (unitCache.EnemyUnitsTargetingPlayer.Count > 0
                && cast.OnSelf(Fade))
                return;

            // ShadowFiend
            if (Me.ManaPercentage < 10
                && cast.OnTarget(Shadowfiend))
                return;

            // Party Dispel Magic
            if (settings.PartyDispelMagic)
            {
                IWoWPlayer needDispelMagic = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needDispelMagic != null && cast.OnFocusUnit(DispelMagic, needDispelMagic))
                    return;
            }

            // PARTY Heal
            if (!FlashHeal.KnownSpell && !GreaterHealRank7.KnownSpell)
            {
                List<IWoWPlayer> needHeal = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 60)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needHeal.Count > 0 && cast.OnFocusUnit(Heal, needHeal[0]))
                    return;
            }

            // PARTY Lesser Heal
            if (!FlashHeal.KnownSpell)
            {
                List<IWoWPlayer> needLesserHeal = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 80)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needLesserHeal.Count > 0 && cast.OnFocusUnit(LesserHeal, needLesserHeal[0]))
                    return;
            }

            // PARTY Flash heal
            List<IWoWPlayer> needFlashHeal = unitCache.GroupAndRaid
                .FindAll(m => m.HealthPercent < 40)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needFlashHeal.Count > 0 && cast.OnFocusUnit(FlashHeal, needFlashHeal[0]))
                return;

            // PARTY Greater heal
            List<IWoWPlayer> needGreaterHeal = unitCache.GroupAndRaid
                .FindAll(m => m.HealthPercent < 60)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needGreaterHeal.Count > 0 && cast.OnFocusUnit(GreaterHeal, needGreaterHeal[0]))
                return;

            // PARTY Shield
            List<IWoWPlayer> neeedShield = unitCache.GroupAndRaid
                .FindAll(m => m.HealthPercent < 60 && !m.HasAura(PowerWordShield) && !WTEffects.HasDebuff("Weakened Soul", m.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (neeedShield.Count > 0 && cast.OnFocusUnit(PowerWordShield, neeedShield[0]))
                return;

            // PARTY Prayer Healing
            List<IWoWPlayer> needPrayerOfHealing = unitCache.GroupAndRaid
                .FindAll(m => m.IsAlive && m.HealthPercent < 70)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needPrayerOfHealing.Count > 2 && cast.OnTarget(PrayerOfHealing))
                return;

            // PARTY Prayer of Mending
            List<IWoWPlayer> needPrayerOfMending = unitCache.GroupAndRaid
                .FindAll(m => m.IsAlive && m.HealthPercent < 70 && !m.HasAura(PrayerOfMending))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needPrayerOfMending.Count > 1 && cast.OnFocusUnit(PrayerOfMending, needPrayerOfMending[0]))
                return;

            // PARTY Greater Heal rank 2
            if (GreaterHealRank7.KnownSpell)
            {
                List<IWoWPlayer> needGHeal2 = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 75)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needGHeal2.Count > 0 && cast.OnFocusUnit(GreaterHealRank2, needGHeal2[0]))
                    return;
            }
            // PARTY Heal
            else
            {
                List<IWoWPlayer> needHeal70 = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 80)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needHeal70.Count > 0 && cast.OnFocusUnit(Heal, needHeal70[0]))
                    return;
            }


            // PARTY Renew
            List<IWoWPlayer> needRenew = unitCache.GroupAndRaid
                .FindAll(m => m.HealthPercent < 90 && !m.HasAura(Renew))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needRenew.Count > 0 && cast.OnFocusUnit(Renew, needRenew[0]))
                return;
        }
    }
}
