using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class HolyPriestRaid : Priest
    {
        public HolyPriestRaid(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Healer;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasDrinkAura || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // PARTY Circle of Healing
                if (AoEHeal(false))
                    return;

                List<IWoWPlayer> membersByMissingHealth = unitCache.ClosePartyMembers
                    .OrderBy(m => m.HealthPercent)
                    .ToList();

                foreach (var member in membersByMissingHealth)
                {
                    if (SingleTargetHeal(member, false))
                        return;
                }

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void HealerCombat()
        {
            // Target an enemy if we have a shadowfiend
            if (Pet.IsValid && !Me.HasTarget)
            {
                IWoWUnit target = unitCache.EnemiesFighting.Find(u => u.IsValid);
                if (target != null)
                    Me.SetTarget(target.Guid);
            }

            List<IWoWPlayer> membersByMissingHealth = unitCache.ClosePartyMembers
                .OrderBy(m => m.HealthPercent)
                .ToList();
            // Collecting tanks only if needed
            var tanks = new Lazy<List<IWoWPlayer>>(() => unitCache.TargetedByEnemies
                  .FindAll(a => a.IsAlive && a.GetDistance < 60)
                  .ToList());

            // Fade
            if (unitCache.EnemyUnitsTargetingPlayer.Count > 0
                && cast.OnSelf(Fade))
                return;

            List<IWoWPlayer> needDispel = unitCache.ClosePartyMembers
                    .FindAll(m => m.IsAlive && WTEffects.HasMagicDebuff(m.Name));

            // PARTY Mass Dispel
            if (settings.PartyMassDispel && MassDispel.KnownSpell)
            {
                Vector3 centerPosition = WTSpace.FindAggregatedCenter(needDispel.Select(u => u.PositionWithoutType).ToList(), 15, settings.PartyMassDispelCount);
                if (centerPosition != null && cast.OnLocation(MassDispel, centerPosition))
                    return;
            }

            // Prioritize self healing over other things in case of danger
            if (Me.HealthPercent < 40)
            {
                ToolBox.UseConsumableToSelfHeal();
                if (SingleTargetHeal(Me))
                    return;
            }

            // ShadowFiend
            if (Me.ManaPercentage < 50 && !Pet.IsValid)
            {
                IWoWUnit target = unitCache.EnemiesFighting.Find(u => u.IsValid);
                if (target != null)
                {
                    Me.SetTarget(target.Guid);
                    if (cast.OnTarget(Shadowfiend))
                        return;
                }
            }

            if (AoEHeal())
                return;

            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // Party Cure Disease
                IWoWPlayer needCureDisease = membersByMissingHealth
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Party Dispel Magic
            if (settings.PartyDispelMagic)
            {
                if (needDispel.Count > 0 && cast.OnFocusUnit(DispelMagic, needDispel[0]))
                    return;
            }

            // High priority single target heal
            if (settings.PartyTankHealingPriority > 0)
            {
                var priorityTanks = partyManager.TanksNeedPriorityHeal(tanks.Value, membersByMissingHealth, settings.PartyTankHealingPriority);
                foreach (var tank in priorityTanks)
                {
                    if (SingleTargetHeal(tank))
                        return;
                }
            }

            // Normal single target heal on lowest health group member
            foreach (var member in membersByMissingHealth)
            {
                if (SingleTargetHeal(member))
                    return;
            }

            // Keep Renew on tank
            if (settings.PartyKeepRenewOnTank)
            {
                foreach (var tank in tanks.Value)
                {
                    if (!tank.HasAura("Renew") && cast.OnFocusUnit(Renew, tank))
                        return;
                }
            }
        }

        private bool SingleTargetHeal(IWoWUnit unit, bool combat = true)
        {
            if (unit.HealthPercent < 30 && cast.OnFocusUnit(FlashHeal, unit))
                return true;
            if (settings.UsePowerWordShield
                && unit.HealthPercent < 50
                && unit.RagePercent <= 0
                && !unit.HasAura(PowerWordShield)
                && !WTEffects.HasDebuff("Weakened Soul", unit.Name)
                && cast.OnFocusUnit(PowerWordShield, unit))
                return true;
            if (unit.HealthPercent < 60 && cast.OnFocusUnit(GreaterHeal, unit))
                return true;
            if (unit.HealthPercent < 80 && !unit.HasAura("Renew") && cast.OnFocusUnit(Renew, unit))
                return true;
            if (unit.HealthPercent < 95 && !unit.HasAura("Renew") && cast.OnFocusUnit(RenewRank8, unit))
                return true;
            if (combat && unit.HealthPercent < 100)
            {
                if (cast.OnFocusUnit(PrayerOfMending, unit))
                    return true;
            }
            return false;
        }

        private bool AoEHeal(bool combat = true)
        {
            if (CircleOfHealing.KnownSpell)
            {
                if (CastCircleOfHealing(combat))
                    return true;
            }
            else if (PrayerOfHealing.KnownSpell)
            {
                // PARTY Prayer of Healing
                List<IWoWPlayer> needPrayerOfHealing = unitCache.GroupAndRaid
                    .FindAll(m => m.IsAlive && m.GetDistance < 33 && m.HealthPercent < 75)
                    .ToList();
                if (needPrayerOfHealing.Count > 2 && cast.OnSelf(PrayerOfHealing))
                    return true;
            }

            return false;
        }

        private bool CastCircleOfHealing(bool combat = true)
        {
            List<List<IWoWPlayer>> groups = unitCache.Raid.Count == 0
                ? new List<List<IWoWPlayer>> { unitCache.GroupAndRaid }
                : unitCache.Raid.Values.ToList();
            var minimumCount = 3;
            int healthThreshold = (combat && Me.ManaPercentage < 80) ? settings.PartyCircleOfHealingThreshold : 95;

            var groupsNeedCoH = groups
                // Find all groups those need CoH
                .FindAll(g => g.FindAll(m => m.IsAlive && m.GetDistance < 70 && m.HealthPercent < healthThreshold).Count >= minimumCount)
                // Order groups by average health
                .OrderBy(g =>
                {
                    var healableMembers = g.FindAll(m => m.IsAlive && m.GetDistance < 70);
                    return healableMembers.Aggregate(0.0, (sum, m) => sum + m.HealthPercent) / (float)healableMembers.Count;
                })
                .ToList();

            foreach (var group in groupsNeedCoH)
            {
                List<(IWoWPlayer member, int count)> alliesNeedCoH = group
                    // Checking all group members how many healable allies are in range (count)
                    .Select(member => (member, count: group.FindAll(otherMember =>
                        otherMember.IsAlive
                        && otherMember.HealthPercent < healthThreshold
                        && otherMember.PositionWithoutType.DistanceTo(member.PositionWithoutType) < settings.PartyCircleofHealingRadius)
                        .Count))
                    .ToList()
                    // Removing those who would heal less members than `minimumCount`
                    .FindAll(t => t.count >= minimumCount)
                    // Ordering them by count
                    .OrderByDescending(t => t.count)
                    .ToList();

                if (alliesNeedCoH.Count > 0)
                {
                    if (cast.OnFocusUnit(CircleOfHealing, alliesNeedCoH[0].member))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
