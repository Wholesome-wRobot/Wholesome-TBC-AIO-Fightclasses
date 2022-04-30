using System;
using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class PaladinHolyRaid : Paladin
    {
        private static Random rng = new Random();

        protected override void BuffRotation()
        {
            RangeManager.SetRange(30);

            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();
            }
        }

        protected override void HealerCombat()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;

            List<AIOPartyMember> aliveMembers = AIOParty.GroupAndRaid
                .FindAll(a => a.IsAlive && a.GetDistance < 60)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            double groupHealthAverage = aliveMembers
                .Aggregate(0.0, (s, a) => s + a.HealthPercent) / (double)aliveMembers.Count;
            var tanks = AIOParty.TargetedByEnemies
                .FindAll(a => a.IsAlive && a.GetDistance < 60)
                .ToList();
 
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
                List<AIOPartyMember> needsLoH = AIOParty.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 10)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                    return;
            }

            bool isCleanseHighPriority = settings.PartyCleansePriority != "Low"
                && (settings.PartyCleansePriority == "High" || rng.NextDouble() >= 0.5);

            // High priority Cleanse
            if (settings.PartyCleanse && isCleanseHighPriority)
            {
                WoWPlayer needsCleanse = AIOParty.GroupAndRaid
                    .Find(m => UnitHasCleansableDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            if (tanks.Count > 0 && aliveMembers.Count > 0)
            {
                var lowestTankHealth = tanks[0].HealthPercent;
                // Virtually increasing missing HP based on user settings
                var virtualHP = 100 - (100.0 - lowestTankHealth) * (1.0 + ((float)settings.PartyTankHealingPriority) / 100);
                if (virtualHP < aliveMembers[0].HealthPercent)
                {
                    if (SingleTargetHeal(tanks[0]))
                        return;
                }
            }

            // Single target heal
            if (aliveMembers.Count > 0 && SingleTargetHeal(aliveMembers[0]))
                return;

            // Low priority Cleanse
            if (settings.PartyCleanse && !isCleanseHighPriority)
            {
                WoWPlayer needsCleanse = AIOParty.GroupAndRaid
                    .Find(m => UnitHasCleansableDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // Seal of light
            if (settings.PartyHolySealOfLight
                && !Target.HaveBuff("Judgement of Light"))
            {
                if (cast.OnTarget(Judgement))
                    return;

                if (!Me.HaveBuff("Seal of Light")
                    && cast.OnSelf(SealOfLight))
                    return;
            }

            // PARTY Purifiy
            if (settings.PartyPurify)
            {
                WoWPlayer needsPurify = AIOParty.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name) || WTEffects.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }
        }

        private bool SingleTargetHeal(WoWUnit unit)
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
                if (!Me.HaveBuff("Divine Favor") && cast.OnSelf(DivineFavor))
                    return true;
                if (cast.OnFocusUnit(HolyLight, unit))
                    return true;
            }
            // Medium heal
            if (unit.HealthPercent < settings.PartyHolyLightPercentThreshold 
                || (unit.MaxHealth - unit.Health) > settings.PartyHolyLightValueThreshold)
            {
                if (cast.OnFocusUnit(HolyLight, unit))
                    return true;
            }
            // Small heal
            if (unit.HealthPercent < settings.PartyFlashOfLightThreshold)
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
