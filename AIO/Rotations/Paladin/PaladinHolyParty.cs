using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class PaladinHolyParty : Paladin
    {
        protected override void BuffRotation()
        {
            RangeManager.SetRange(30);

            base.BuffRotation();
        }

        protected override void HealerCombat()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;

            List<AIOPartyMember> allyNeedBigHeal = AIOParty.Group
                .FindAll(a => a.IsAlive && a.HealthPercent < 40)
                .OrderBy(a => a.HealthPercent)
                .ToList();

            List<AIOPartyMember> allyNeedSmallHeal = AIOParty.Group
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyFlashOfLightThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();

            // Divine Illumination
            if (allyNeedSmallHeal.Count > 2
                && cast.OnSelf(DivineIllumination))
                return;

            // PARTY Lay On Hands
            if (Me.ManaPercentage < 10)
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
            List<AIOPartyMember> allyNeedMediumHeal = AIOParty.Group
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyHolyLightThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            if (allyNeedMediumHeal.Count > 0 
                && cast.OnFocusUnit(HolyLight, allyNeedMediumHeal[0]))
                return;

            // PARTY Holy Light rank 5 (for the buff)
            if (allyNeedSmallHeal.Count == 1
                && HolyLight.Cost == 840
                && ToolBox.BuffTimeLeft("Light\'s Grace") < 5
                && cast.OnFocusUnit(HolyLightRank5, allyNeedSmallHeal[0]))
                return;

            // PARTY Flash Heal
            if (allyNeedSmallHeal.Count > 0
                && cast.OnFocusUnit(FlashOfLight, allyNeedSmallHeal[0]))
                return;

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

            // PARTY Cleanse
            if (settings.PartyCleanse)
            {
                WoWPlayer needsCleanse = AIOParty.Group
                    .Find(m => ToolBox.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }
        }
    }
}
