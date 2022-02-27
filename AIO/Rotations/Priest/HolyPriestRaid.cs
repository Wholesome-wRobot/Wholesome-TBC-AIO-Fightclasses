using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class HolyPriestRaid : Priest
    {
        protected override void BuffRotation()
        {
            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // PARTY Circle of Healing
                if (AoEHeal(false))
                    return;

                List<AIOPartyMember> membersByMissingHealth = AIOParty.ClosePartyMembers
                    .OrderBy(m => m.HealthPercent)
                    .ToList();

                foreach (var member in membersByMissingHealth)
                {
                    if (SingleTargetHeal(member, false))
                        return;
                }

                // PARTY Drink
                if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void HealerCombat()
        {
            // Target an enemy if we have a shadowfiend
            if (ObjectManager.Pet.IsValid && !ObjectManager.Me.HasTarget)
            {
                WoWUnit target = AIOParty.EnemiesFighting.Find(u => u.IsValid);
                if (target != null)
                    ObjectManager.Me.Target = target.Guid;
            }

            List<AIOPartyMember> membersByMissingHealth = AIOParty.ClosePartyMembers
                .OrderBy(m => m.HealthPercent)
                .ToList();

            // Fade
            if (AIORadar.CloseUnitsTargetingMe.Count > 0
                && cast.OnSelf(Fade))
                return;

            List<AIOPartyMember> needDispel = AIOParty.ClosePartyMembers
                    .FindAll(m => m.IsAlive && ToolBox.HasMagicDebuff(m.Name));

            // PARTY Mass Dispel
            if (settings.PartyMassDispel && MassDispel.KnownSpell)
            {
                Vector3 centerPosition = ToolBox.FindAggregatedCenter(needDispel.Select(u => u.Position).ToList(), 15, settings.PartyMassDispelCount);
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
                WoWUnit target = AIOParty.EnemiesFighting.Find(u => u.IsValid);
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
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Party Dispel Magic
            if (settings.PartyDispelMagic)
            {
                if (needDispel.Count > 0 && cast.OnFocusUnit(DispelMagic, needDispel[0]))
                    return;
            }

            foreach (var member in membersByMissingHealth)
            {
                if (SingleTargetHeal(member))
                    return;
            }
        }

        private bool SingleTargetHeal(WoWPlayer player, bool combat = true)
        {
            if (player.HealthPercent < 30 && cast.OnFocusUnit(FlashHeal, player))
                return true;
            if (settings.UsePowerWordShield
                && player.HealthPercent < 50
                && player.RagePercentage <= 0
                && player.HaveBuff("Power Word: Shield")
                && !ToolBox.HasDebuff("Weakened Soul", player.Name)
                && cast.OnFocusUnit(PowerWordShield, player))
                return true;
            if (player.HealthPercent < 60 && cast.OnFocusUnit(GreaterHeal, player))
                return true;
            if (player.HealthPercent < 80 && !player.HaveBuff(Renew.Name) && cast.OnFocusUnit(Renew, player))
                return true;
            if (player.HealthPercent < 95 && !player.HaveBuff(Renew.Name) && cast.OnFocusUnit(RenewRank8, player))
                return true;
            if (combat && player.HealthPercent < 100)
            {
                if (cast.OnFocusUnit(PrayerOfMending, player))
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
                List<AIOPartyMember> needPrayerOfHealing = AIOParty.GroupAndRaid
                    .FindAll(m => m.IsAlive && m.GetDistance < 33 && m.HealthPercent < 75)
                    .ToList();
                if (needPrayerOfHealing.Count > 2 && cast.OnSelf(PrayerOfHealing))
                    return true;
            }

            return false;
        }

        private bool CastCircleOfHealing(bool combat = true)
        {
            List<List<AIOPartyMember>> groups = AIOParty.RaidGroups.Count == 0 
                ? new List<List<AIOPartyMember>> { AIOParty.GroupAndRaid }
                : AIOParty.RaidGroups.Values.ToList();
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
