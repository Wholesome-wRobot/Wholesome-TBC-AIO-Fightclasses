﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class PaladinHolyRaid : Paladin
    {
        public PaladinHolyRaid(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Healer;
        }

        private static Random rng = new Random();

        protected override void BuffRotation()
        {
            RangeManager.SetRange(30);

            if (!Me.HasDrinkAura || Me.ManaPercentage > 95)
            {
                base.BuffRotation();
            }
        }

        protected override void HealerCombat()
        {
            base.CombatRotation();

            List<IWoWPlayer> aliveMembers = unitCache.GroupAndRaid
                .FindAll(a => a.IsAlive && a.GetDistance < 60)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            double groupHealthAverage = aliveMembers
                .Aggregate(0.0, (s, a) => s + a.HealthPercent) / (double)aliveMembers.Count;

            // Divine Illumination
            if (groupHealthAverage < 70
                && cast.OnSelf(DivineIllumination))
                return;

            // Using consumables such as Healthstone
            if (Me.HealthPercent < 50)
            {
                ToolBox.UseConsumableToSelfHeal();
            }

            // Divine Shield
            if (Me.HealthPercent < 30
                && cast.OnSelf(DivineShield))
                return;

            // PARTY Lay On Hands
            if (Me.ManaPercentage < 5)
            {
                List<IWoWPlayer> needsLoH = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 10)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                    return;
            }

            bool isCleanseHighPriority = settings.RHO_PartyCleansePriority != "Low"
                && (settings.RHO_PartyCleansePriority == "High" || rng.NextDouble() >= 0.5);

            // High priority Cleanse
            if (settings.RHO_PartyCleanse && isCleanseHighPriority)
            {
                IWoWPlayer needsCleanse = unitCache.GroupAndRaid
                    .Find(m => UnitHasCleansableDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // High priority heal
            if (settings.RHO_PartyTankHealingPriority > 0)
            {
                var tanks = unitCache.TargetedByEnemies
                    .FindAll(a => a.IsAlive && a.GetDistance < 60)
                    .ToList();
                var priorityTanks = partyManager.TanksNeedPriorityHeal(tanks, aliveMembers, settings.RHO_PartyTankHealingPriority);
                foreach (var tank in priorityTanks)
                {
                    if (SingleTargetHeal(tank))
                        return;
                }
            }

            // Single target heal
            if (aliveMembers.Count > 0 && SingleTargetHeal(aliveMembers[0]))
                return;

            // Low priority Cleanse
            if (settings.RHO_PartyCleanse && !isCleanseHighPriority)
            {
                IWoWPlayer needsCleanse = unitCache.GroupAndRaid
                    .Find(m => UnitHasCleansableDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // Seal of light
            if (settings.RHO_PartyHolySealOfLight
                && !Target.HasAura("Judgement of Light"))
            {
                if (cast.OnTarget(Judgement))
                    return;

                if (!Me.HasAura(SealOfLight)
                    && cast.OnSelf(SealOfLight))
                    return;
            }

            // PARTY Purifiy
            if (settings.RHO_PartyPurify)
            {
                IWoWPlayer needsPurify = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name) || WTEffects.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }
        }

        private bool SingleTargetHeal(IWoWPlayer unit)
        {
            if (unit.HealthPercent == 100)
                return false;

            // Quick heal
            if (unit.HealthPercent < 20)
            {
                if (unit.GetDistance < HolyShock.MaxRange && cast.OnFocusUnit(HolyShock, unit))
                    return true;
                if (cast.OnFocusUnit(FlashOfLight, unit))
                    return true;
            }
            // Big heal
            if (unit.HealthPercent < 40)
            {
                // Divine Favor
                if (!Me.HasAura(DivineFavor) && cast.OnSelf(DivineFavor))
                    Thread.Sleep(50); // Divine Favor causes no GCD to occour, no need to return here
                if (cast.OnFocusUnit(HolyLight, unit))
                    return true;
            }
            // Medium heal
            if (unit.HealthPercent < settings.RHO_PartyHolyLightPercentThreshold
                || (unit.MaxHealth - unit.Health) > settings.RHO_PartyHolyLightValueThreshold)
            {
                if (cast.OnFocusUnit(HolyLight, unit))
                    return true;
            }
            // Small heal
            if (unit.HealthPercent < settings.RHO_PartyFlashOfLightThreshold)
            {
                if (HolyLight.Cost == 840
                    && WTEffects.BuffTimeLeft("Light\'s Grace") < 5
                    && cast.OnFocusUnit(HolyLightRank5, unit))
                    return true;
                if (cast.OnFocusUnit(FlashOfLight, unit))
                    return true;
            }
            return false;
        }
    }
}
