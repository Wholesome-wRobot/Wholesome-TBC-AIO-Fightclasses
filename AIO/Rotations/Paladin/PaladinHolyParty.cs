using System;
using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class PaladinHolyParty : Paladin
    {
        private static Random rng = new Random();

        protected override void BuffRotation()
        {
            RangeManager.SetRange(30);

            base.BuffRotation();
        }

        protected override void HealerCombat()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;

            List<AIOPartyMember> aliveMembers = AIOParty.Group
                .FindAll(a => a.IsAlive && a.GetDistance < 60)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            double groupHealthAverage = aliveMembers
                .Aggregate(0.0, (s, a) => s + a.HealthPercent) / (double)aliveMembers.Count;
            var tanks = AIOParty.Tanks
                .FindAll(a => a.IsAlive && a.GetDistance < 60)
                .OrderBy(a => a.HealthPercent)
                .ToList();

            string logMessage = "TANKS detected [";
            tanks.ForEach(m => logMessage += m.Name + "-");
            logMessage = logMessage.Remove(logMessage.Length - 1);
            logMessage += "]";
            Logger.Log(logMessage);

            // Divine Illumination
            if (groupHealthAverage < 70
                && cast.OnSelf(DivineIllumination))
                return;

            // Divine Shield
            if (Me.HealthPercent < 30
                && cast.OnSelf(DivineShield))
                return;

            // PARTY Lay On Hands
            if (Me.ManaPercentage < 5)
            {
                List<AIOPartyMember> needsLoH = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 10)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                    return;
            }

            bool isCleanseHighPriority = settings.PartyCleansePriority != "Low"
                && (settings.PartyCleansePriority == "High" || rng.NextDouble() >= 0.5);
            WoWPlayer needsCleanse = AIOParty.Group
                    .Find(m => UnitHasCleansableDebuff(m.Name));

            // High priority Cleanse
            if (settings.PartyCleanse && isCleanseHighPriority)
            {
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
                    Logger.Log("Healing " + tanks[0].Name + " virtual HP:" + virtualHP + " real HP:" + tanks[0].HealthPercent);
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
                WoWPlayer needsPurify = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name) || ToolBox.HasPoisonDebuff(m.Name));
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
            if (unit.HealthPercent < settings.PartyHolyLightThreshold)
            {
                if (cast.OnFocusUnit(HolyLight, unit))
                    return true;
            }
            // Small heal
            if (unit.HealthPercent < settings.PartyFlashOfLightThreshold)
            {
                if (HolyLight.Cost == 840
                    && ToolBox.BuffTimeLeft("Light\'s Grace") < 5
                    && cast.OnFocusUnit(HolyLightRank5, unit))
                    return true;
                if (cast.OnFocusUnit(FlashOfLight, unit))
                    return true;
            }            
            return false;
        }
    }
}
