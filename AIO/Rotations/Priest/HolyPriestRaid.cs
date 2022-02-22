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

                if (membersByMissingHealth.Count > 0 && SingleTargetHeal(membersByMissingHealth[0], false))
                    return;

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
            if (Me.ManaPercentage < 50 && !ObjectManager.Pet.IsValid && cast.OnSelf(Shadowfiend))
                return;

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

            if (membersByMissingHealth.Count > 0 && SingleTargetHeal(membersByMissingHealth[0]))
                return;
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
                List<AIOPartyMember> needCircleOfHealing = new List<AIOPartyMember>();
                int treshold = (combat && Me.ManaPercentage < 80) ? settings.PartyCircleOfHealingThreshold : 95;
                if (AIOParty.RaidGroups.Count == 0)
                {
                    // PARTY Circle of Healing
                    needCircleOfHealing = AIOParty.ClosePartyMembers
                        .FindAll(m => m.IsAlive && m.HealthPercent < treshold);
                    if (needCircleOfHealing.Count > 2)
                    {
                        AIOPartyMember target = needCircleOfHealing
                            .Find(m => needCircleOfHealing.FindAll(pm => pm.Guid != m.Guid && pm.Position.DistanceTo(m.Position) < 18).Count >= 2);
                        if (target != null && cast.OnFocusUnit(CircleOfHealing, target))
                            return true;
                    }
                }
                else
                {
                    // RAID Circle of Healing
                    foreach (var item in AIOParty.RaidGroups)
                    {
                        List<AIOPartyMember> subGroupNeedCircleOfHealing = item.Value
                            .FindAll(m => m.IsAlive && m.GetDistance < 70 && m.HealthPercent < treshold);
                        if (subGroupNeedCircleOfHealing.Count > 2)
                        {
                            AIOPartyMember target = subGroupNeedCircleOfHealing
                                .Find(m => subGroupNeedCircleOfHealing.FindAll(pm => pm.Guid != m.Guid && pm.Position.DistanceTo(m.Position) < 18).Count >= 2);
                            if (target != null && cast.OnFocusUnit(CircleOfHealing, target))
                                return true;
                        }
                    }
                }
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
    }
}
