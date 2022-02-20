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
                .FindAll(a => a.IsAlive && a.GetDistance < 60);
            double groupHealthAverage = aliveMembers
                .Aggregate(0.0, (s, a) => s + a.HealthPercent) / (double)aliveMembers.Count;

            List<AIOPartyMember> allyNeedQuickHeal = aliveMembers
                .FindAll(a => a.HealthPercent < 20)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            List<AIOPartyMember> allyNeedBigHeal = aliveMembers
                .FindAll(a => a.HealthPercent < 40)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            List<AIOPartyMember> allyNeedMediumHeal = aliveMembers
                .FindAll(a => a.HealthPercent < settings.PartyHolyLightThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            List<AIOPartyMember> allyNeedSmallHeal = aliveMembers
                .FindAll(a => a.HealthPercent < settings.PartyFlashOfLightThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();

            // Divine Illumination
            if (groupHealthAverage < 60
                && cast.OnSelf(DivineIllumination))
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

            // Divine Shield
            if (Me.HealthPercent < 30
                && cast.OnSelf(DivineShield))
                return;

            if (allyNeedQuickHeal.Count > 0)
            {
                var ally = allyNeedQuickHeal[0];
                if (cast.OnFocusUnit(HolyShock, ally))
                    return;
                if (cast.OnFocusUnit(FlashOfLight, ally))
                    return;
            }

            bool isCleanseHighPriority = settings.PartyCleansePriority != "Low"
                && (settings.PartyCleansePriority == "High" || rng.NextDouble() >= 0.5);
            WoWPlayer needsCleanse = AIOParty.Group
                    .Find(m => UnitHasCleansableDebuff(m.Name));

            // PARTY Cleanse
            if (settings.PartyCleanse && isCleanseHighPriority)
            {
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // PARTY Holy Light with Divine Favor
            if (Me.HaveBuff("Divine Favor")
                && allyNeedBigHeal.Count > 0
                && cast.OnFocusUnit(HolyLight, allyNeedBigHeal[0]))
                return;

            // PARTY Divine Favor
            if (allyNeedBigHeal.Count > 0
                && !Me.HaveBuff("Divine Favor")
                && cast.OnSelf(DivineFavor))
                return;

            // PARTY Holy Light
            if (allyNeedMediumHeal.Count > 0 
                && cast.OnFocusUnit(HolyLight, allyNeedMediumHeal[0]))
                return;

            // PARTY Holy Light rank 5 (for the buff)
            if (allyNeedSmallHeal.Count == 1
                && HolyLight.Cost == 840
                && ToolBox.BuffTimeLeft("Light\'s Grace") < 5
                && cast.OnFocusUnit(HolyLightRank5, allyNeedSmallHeal[0]))
                return;

            // PARTY Flash of Light
            if (allyNeedSmallHeal.Count > 0
                && cast.OnFocusUnit(FlashOfLight, allyNeedSmallHeal[0]))
                return;

            // PARTY Cleanse
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
    }
}
