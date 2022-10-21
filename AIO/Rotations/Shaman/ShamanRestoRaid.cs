using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class ShamanRestoRaid : Shaman
    {
        private static Random rng = new Random();
        private IWoWUnit lastEarthShieldTarget;

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

                List<IWoWPlayer> aliveMembers = unitCache.GroupAndRaid
                    .FindAll(a => a.IsAlive && a.GetDistance < 60)
                    .OrderBy(a => a.HealthPercent)
                    .ToList();
                if (Heal(aliveMembers, false))
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
            lastEarthShieldTarget = null;

            // Water Shield
            if (!Me.HasAura(WaterShield)
                && !Me.HasAura(LightningShield)
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;

            // Totems
            if (totemManager.CastTotems(specialization))
                return;
        }

        protected override void HealerCombat()
        {
            base.HealerCombat();

            List<IWoWPlayer> aliveMembers = unitCache.GroupAndRaid
                .FindAll(a => a.IsAlive && a.GetDistance < 60)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            bool isCureHighPriority = settings.PartyCurePriority != "Low"
                && (settings.PartyCurePriority == "High" || rng.NextDouble() >= 0.5);
            var tanks = unitCache.TargetedByEnemies
                    .FindAll(a => a.IsAlive && a.GetDistance < 60)
                    .ToList();
            RangeManager.SetRange(38);

            // High priority Cure
            if (settings.PartyCureDisease || settings.PartyCurePoison && isCureHighPriority)
            {
                if (Cure(aliveMembers))
                    return;
            }

            // High priority heal
            if (settings.PartyTankHealingPriority > 0)
            {
                var priorityTanks = partyManager.TanksNeedPriorityHeal(tanks, aliveMembers, settings.PartyTankHealingPriority);
                if (Heal(priorityTanks))
                    return;
            }

            if(Heal(aliveMembers))
                return;
            
            // Low priority Cure
            if (settings.PartyCureDisease || settings.PartyCurePoison && !isCureHighPriority)
            {
                if (Cure(aliveMembers))
                    return;
            }

            // PARTY Earth Shield
            if (EarthShield.KnownSpell)
            {
                bool stillUp = false;
                foreach (IWoWPlayer tank in tanks)
                {
                    if(tank.HasAura(EarthShield))
                    {
                        stillUp = true;
                    }
                }
                if (!stillUp)
                {
                    if (TryCastSpell(EarthShield, tanks))
                        return;
                }
            }

            // Bloodlust
            //if (!Me.HasAura(Bloodlust)
            //    && Target.HealthPercent > 80
            //    && cast.OnSelf(Bloodlust))
            //    return;

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

        private bool Heal(List<IWoWPlayer> aliveMembers, bool combat = true)
        {
            foreach (IWoWPlayer member in aliveMembers)
            {
                if (combat)
                {
                    if (member.HealthPercent <= settings.PartyInstantHealThreshold)
                    {
                        if (!Me.HasAura(NaturesSwiftness) && cast.OnSelf(NaturesSwiftness))
                        {
                            // Natures Swiftness causes no GCD to occour, no need to return here
                            // We just need to wait a little not to overwhelm the input
                            Thread.Sleep(50);
                            if (cast.OnFocusUnit(HealingWave, member))
                                return true;
                        }
                    }
                }

                if (member.HealthPercent < settings.PartyChainHealThreshold
                    && cast.OnFocusUnit(SelectChainHeal(), member))
                    return true;

                if (member.HealthPercent < settings.PartyLesserHealingWaveThreshold
                    && cast.OnFocusUnit(LesserHealingWave, member))
                    return true;

                if (member.HealthPercent < settings.PartyHealingWaveThreshold
                    && cast.OnFocusUnit(HealingWave, member))
                    return true;
            }
            return false;
        }

        private AIOSpell SelectChainHeal()
        {
            var rank = settings.PartyChainHealMaxRank;
            if (rank > 0)
            {
                switch (rank)
                {
                    case 1:
                        return ChainHealRank1;
                    case 2:
                        return ChainHealRank2;
                    case 3:
                        return ChainHealRank3;
                    case 4:
                        return ChainHealRank4;
                    default:
                        break;
                }
            }
            return ChainHeal;
        }

        private bool Cure(List<IWoWPlayer> aliveMembers)
        {
            foreach (IWoWPlayer member in aliveMembers)
            {
                if (WTEffects.HasPoisonDebuff(member.Name) && cast.OnFocusUnit(CurePoison, member))
                {
                    return true;
                }
                if (WTEffects.HasDiseaseDebuff(member.Name) && cast.OnFocusUnit(CureDisease, member))
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryCastSpell(AIOSpell spell, List<IWoWPlayer> aliveMembers)
        {
            foreach (IWoWPlayer member in aliveMembers)
            {
                if (cast.OnFocusUnit(spell, member))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
