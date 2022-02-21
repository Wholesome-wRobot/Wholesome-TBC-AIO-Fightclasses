using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class HolyPriestRaid : Priest
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Circle of Healing
            if (AoEHeal(false))
                return;

            List<AIOPartyMember> aliveMembers = AIOParty.Group
                .FindAll(m => m.IsAlive && m.GetDistance < 60)
                .OrderBy(m => m.HealthPercent)
                .ToList();

            if (aliveMembers.Count > 0 && SingleTargetHeal(aliveMembers[0], false))
                return;

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;
        }

        protected override void HealerCombat()
        {
            List<AIOPartyMember> aliveMembers = AIOParty.Group
                .FindAll(m => m.IsAlive && m.GetDistance < 70)
                .OrderBy(m => m.HealthPercent)
                .ToList();

            List<AIOPartyMember> needDispelMagic = aliveMembers
                    .FindAll(m => ToolBox.HasMagicDebuff(m.Name))
                    .OrderBy(m => m.HealthPercent)
                    .ToList();

            // Fade
            if (AIOParty.EnemiesClose.Exists(m => m.IsTargetingMe)
                && cast.OnSelf(Fade))
                return;

            // PARTY Mass Dispel
            if (settings.PartyMassDispel 
                && needDispelMagic.Count >= settings.PartyMassDispelCount)
            {
                // Get unit in the middle of the pack
                var watch = System.Diagnostics.Stopwatch.StartNew();
                WoWUnit unit = ToolBox.GetBestAoETarget(40, needDispelMagic);
                watch.Stop();
                Logger.LogDebug("ToolBox.GetBestAoETarget ran in " + watch.ElapsedMilliseconds + " ms");
                if (unit != null)
                {
                    Logger.LogDebug("Sending Mass Dispel to " + unit.Name);
                    var watch2 = System.Diagnostics.Stopwatch.StartNew();
                    ClickOnTerrain.Spell(MassDispel.Id, unit.Position);
                    watch2.Stop();
                    Logger.LogDebug("Mass Dispel arrived after " + watch2.ElapsedMilliseconds + " ms");
                    return;
                }
            }

            // Prioritize self healing over other things in case of danger
            if (Me.HealthPercent < 40)
            {
                ToolBox.UseConsumableToSelfHeal();
                if (SingleTargetHeal(Me))
                    return;
            }

            // ShadowFiend
            if (Me.ManaPercentage < 50)
            {
                var enemies = AIOParty.EnemiesFighting
                    .OrderBy(m => m.Health)
                    .ToList();
                if (enemies.Count > 0)
                {
                    WoWUnit unit = enemies.Last();
                    if (unit != null && cast.OnFocusUnit(Shadowfiend, unit))
                        return;
                }                
            }

            // PARTY Circle of Healing
            if (AoEHeal())
                return;

            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // Party Cure Disease
                WoWPlayer needCureDisease = aliveMembers
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Party Dispel Magic
            if (settings.PartyDispelMagic)
            {
                if (needDispelMagic.Count > 0 && cast.OnFocusUnit(DispelMagic, needDispelMagic[0]))
                    return;
            }

            if (aliveMembers.Count > 0 && SingleTargetHeal(aliveMembers[0]))
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
                // Healing very proactively while there is a lot of mana
                int treshold = (combat && Me.ManaPercentage < 80) ? settings.PartyCircleOfHealingThreshold : 95;
                if (AIOParty.RaidGroups.Count == 0)
                {
                    // Party healing
                    needCircleOfHealing = AIOParty.Group
                        .FindAll(m => m.IsAlive && m.GetDistance < 70 && m.HealthPercent < treshold)
                        .OrderBy(m => m.HealthPercent)
                        .ToList();
                    if (needCircleOfHealing.Count > 2)
                    {
                        if (cast.OnFocusUnit(CircleOfHealing, needCircleOfHealing[0]))
                            return true;
                    }
                } else {
                    // Raid healing
                    foreach (var item in AIOParty.RaidGroups)
                    {
                        List<AIOPartyMember> subGroupNeedCircleOfHealing = item.Value
                            .FindAll(m => m.IsAlive && m.GetDistance < 70 && m.HealthPercent < treshold)
                            .OrderBy(m => m.HealthPercent)
                            .ToList();
                        if (subGroupNeedCircleOfHealing.Count > 2)
                        {
                            needCircleOfHealing.Add(subGroupNeedCircleOfHealing[0]);
                        }
                    }
                    if (needCircleOfHealing.Count > 0)
                    {
                        List<AIOPartyMember> needCircleOfHealingOrdered = needCircleOfHealing
                            .OrderBy(m => m.HealthPercent)
                            .ToList();
                        if (cast.OnFocusUnit(CircleOfHealing, needCircleOfHealingOrdered[0]))
                            return true;
                    }
                }
            } 
            else if (PrayerOfHealing.KnownSpell)
            {
                List<AIOPartyMember> needPrayerOfHealing = AIOParty.Group
                    .FindAll(m => m.IsAlive && m.GetDistance < 33 && m.HealthPercent < 75)
                    .ToList();
                if (needPrayerOfHealing.Count > 2 && cast.OnSelf(PrayerOfHealing))
                    return true;
            }

            return false;
        }
    }
}
