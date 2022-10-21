using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class ShamanRestoRaid : Shaman
    {
        public ShamanRestoRaid(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Healer;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();

            if (!Me.HasAura(GhostWolf) && (!Me.HasDrinkAura || Me.ManaPercentage > 95))
            {
                // Ghost Wolf
                if (settings.GhostWolfMount
                    && wManager.wManagerSetting.CurrentSetting.GroundMountName == ""
                    && GhostWolf.KnownSpell)
                    WTSettings.SetGroundMount(GhostWolf.Name);

                // PARTY Healing Wave
                List<IWoWPlayer> alliesNeedingHealWave = unitCache.GroupAndRaid
                    .FindAll(a => a.IsAlive && a.HealthPercent < 70)
                    .OrderBy(a => a.HealthPercent)
                    .ToList();
                if (alliesNeedingHealWave.Count > 0
                    && cast.OnFocusUnit(HealingWave, alliesNeedingHealWave[0]))
                    return;

                // Water Shield
                if (!Me.HasAura(WaterShield)
                    && !Me.HasAura(LightningShield)
                    && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < 20)
                    && cast.OnSelf(WaterShield))
                    return;

                // PARTY Cure poison
                IWoWPlayer needCurePoison = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusUnit(CurePoison, needCurePoison))
                    return;

                // PARTY Cure Disease
                IWoWPlayer needCureDisease = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Remove Ghost Wolf
            if (Me.HasAura(GhostWolf)
                && cast.OnSelf(GhostWolf))
                return;

            // Water Shield
            if (!Me.HasAura(WaterShield)
                && !Me.HasAura(LightningShield)
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;
        }

        protected override void HealerCombat()
        {
            base.HealerCombat();

            IWoWPlayer allyNeedBigHeal = unitCache.GroupAndRaid
                .Find(a => a.IsAlive && a.HealthPercent < 40);

            RangeManager.SetRange(25);

            // Remove Ghost Wolf
            if (Me.HasAura(GhostWolf)
                && cast.OnSelf(GhostWolf))
                return;

            // PARTY Healing Wave with NATURE SWIFTNESS
            if (Me.HasAura(NaturesSwiftness))
            {
                if (allyNeedBigHeal != null && cast.OnFocusUnit(HealingWave, allyNeedBigHeal))
                    return;
            }

            // Party Nature's Swiftness
            if (allyNeedBigHeal != null
                && !Me.HasAura(NaturesSwiftness)
                && cast.OnSelf(NaturesSwiftness))
                return;

            // PARTY Lesser Healing Wave
            List<IWoWPlayer> alliesNeedingLesserHealWave = unitCache.GroupAndRaid
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyLesserHealingWaveThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            if (alliesNeedingLesserHealWave.Count > 0
                && cast.OnFocusUnit(LesserHealingWave, alliesNeedingLesserHealWave[0]))
                return;

            // PARTY Healing Wave
            List<IWoWPlayer> alliesNeedingHealWave = unitCache.GroupAndRaid
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyHealingWaveThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            if (alliesNeedingHealWave.Count > 0
                && cast.OnFocusUnit(HealingWave, alliesNeedingHealWave[0]))
                return;

            // PARTY Chain Heal
            List<IWoWPlayer> alliesNeedChainHeal = unitCache.GroupAndRaid
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyChainHealThreshold)
                .OrderBy(a => a.GetDistance)
                .ToList();
            if (alliesNeedChainHeal.Count >= settings.PartyChainHealAmount)
            {
                if (alliesNeedChainHeal.Exists(p => p.Guid == Me.Guid)
                    && cast.OnSelf(ChainHeal))
                    return;
                if (cast.OnFocusUnit(ChainHeal, alliesNeedChainHeal[0]))
                    return;
            }

            // PARTY Earth Shield
            if (EarthShield.KnownSpell && !unitCache.GroupAndRaid.Exists(a => a.HasAura(EarthShield)))
            {
                foreach (IWoWPlayer player in unitCache.GroupAndRaid.FindAll(p => p.IsAlive && p.WowClass != wManager.Wow.Enums.WoWClass.Shaman))
                {
                    List<IWoWUnit> enemiesTargetingHim = unitCache.EnemiesFighting
                        .FindAll(e => e.Target == player.Guid);
                    if (enemiesTargetingHim.Count > 1 && cast.OnFocusUnit(EarthShield, player))
                        return;
                }
            }

            // PARTY Cure Poison
            if (settings.PartyCurePoison)
            {
                IWoWPlayer needCurePoison = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusUnit(CurePoison, needCurePoison))
                    return;
            }

            // PARTY Cure Disease
            if (settings.PartyCureDisease)
            {
                IWoWPlayer needCureDisease = unitCache.GroupAndRaid
                    .Find(m => m.IsAlive && WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Bloodlust
            if (!Me.HasAura(Bloodlust)
                && Target.HealthPercent > 80
                && cast.OnSelf(Bloodlust))
                return;

            // Water Shield
            if (!Me.HasAura(WaterShield)
                && !Me.HasAura(LightningShield)
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage <= lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;

            // Totems
            if (totemManager.CastTotems(specialization))
                return;
        }
    }
}
