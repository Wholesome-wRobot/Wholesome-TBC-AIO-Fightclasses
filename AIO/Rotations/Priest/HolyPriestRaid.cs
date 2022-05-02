using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;

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
            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // PARTY Circle of Healing
                if (AoEHeal(false))
                    return;

                List<AIOPartyMember> membersByMissingHealth = partyManager.ClosePartyMembers
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
            if (ObjectManager.Pet.IsValid && !ObjectManager.Me.HasTarget)
            {
                WoWUnit target = partyManager.EnemiesFighting.Find(u => u.IsValid);
                if (target != null)
                    ObjectManager.Me.Target = target.Guid;
            }

            List<AIOPartyMember> membersByMissingHealth = partyManager.ClosePartyMembers
                .OrderBy(m => m.HealthPercent)
                .ToList();

            // Fade
            if (unitCache.CloseUnitsTargetingMe.Count > 0
                && cast.OnSelf(Fade))
                return;

            List<AIOPartyMember> needDispel = partyManager.ClosePartyMembers
                    .FindAll(m => m.IsAlive && WTEffects.HasMagicDebuff(m.Name));

            // PARTY Mass Dispel
            if (settings.PartyMassDispel && MassDispel.KnownSpell)
            {
                Vector3 centerPosition = WTSpace.FindAggregatedCenter(needDispel.Select(u => u.Position).ToList(), 15, settings.PartyMassDispelCount);
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
            if (Me.ManaPercentage < 50 && !ObjectManager.Pet.IsValid)
            {
                WoWUnit target = partyManager.EnemiesFighting.Find(u => u.IsValid);
                if (target != null)
                {
                    ObjectManager.Me.Target = target.Guid;
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
                WoWPlayer needCureDisease = membersByMissingHealth
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
                var tanks = partyManager.TargetedByEnemies
                    .FindAll(a => a.IsAlive && a.GetDistance < 60)
                    .ToList();
                var priorityTanks = partyManager.TanksNeedPriorityHeal(tanks, membersByMissingHealth, settings.PartyTankHealingPriority);
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
                foreach (var tank in tanks)
                {
                    if (!tank.HaveBuff(Renew.Name) && cast.OnFocusUnit(Renew, tank))
                        return;
                }
            }
        }

        private bool SingleTargetHeal(WoWUnit unit, bool combat = true)
        {
            if (unit.HealthPercent < 30 && cast.OnFocusUnit(FlashHeal, unit))
                return true;
            if (settings.UsePowerWordShield
                && unit.HealthPercent < 50
                && unit.RagePercentage <= 0
                && !unit.HaveBuff("Power Word: Shield")
                && !WTEffects.HasDebuff("Weakened Soul", unit.Name)
                && cast.OnFocusUnit(PowerWordShield, unit))
                return true;
            if (unit.HealthPercent < 60 && cast.OnFocusUnit(GreaterHeal, unit))
                return true;
            if (unit.HealthPercent < 80 && !unit.HaveBuff(Renew.Name) && cast.OnFocusUnit(Renew, unit))
                return true;
            if (unit.HealthPercent < 95 && !unit.HaveBuff(Renew.Name) && cast.OnFocusUnit(RenewRank8, unit))
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
                List<AIOPartyMember> needPrayerOfHealing = partyManager.GroupAndRaid
                    .FindAll(m => m.IsAlive && m.GetDistance < 33 && m.HealthPercent < 75)
                    .ToList();
                if (needPrayerOfHealing.Count > 2 && cast.OnSelf(PrayerOfHealing))
                    return true;
            }

            return false;
        }

        private bool CastCircleOfHealing(bool combat = true)
        {
            List<List<AIOPartyMember>> groups = partyManager.RaidGroups.Count == 0
                ? new List<List<AIOPartyMember>> { partyManager.GroupAndRaid }
                : partyManager.RaidGroups.Values.ToList();
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
                List<(AIOPartyMember member, int count)> alliesNeedCoH = group
                    // Checking all group members how many healable allies are in range (count)
                    .Select(member => (member, count: group.FindAll(otherMember =>
                        otherMember.IsAlive
                        && otherMember.HealthPercent < healthThreshold
                        && otherMember.Position.DistanceTo(member.Position) < settings.PartyCircleofHealingRadius)
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
