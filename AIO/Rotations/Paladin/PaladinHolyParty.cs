using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class PaladinHolyParty : Paladin
    {
        public PaladinHolyParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Healer;
        }

        protected override void BuffRotation()
        {
            RangeManager.SetRange(30);

            if (!Me.HasAura("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();
            }
        }

        protected override void HealerCombat()
        {
            base.CombatRotation();

            List<IWoWPlayer> allyNeedBigHeal = unitCache.GroupAndRaid
                .FindAll(a => a.IsAlive && a.HealthPercent < 40)
                .OrderBy(a => a.HealthPercent)
                .ToList();

            List<IWoWPlayer> allyNeedSmallHeal = unitCache.GroupAndRaid
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
                List<IWoWPlayer> needsLoH = unitCache.GroupAndRaid
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
            if (Me.HasAura(DivineFavor)
                && allyNeedBigHeal.Count > 0
                && cast.OnFocusUnit(HolyLight, allyNeedBigHeal[0]))
                return;

            // PARTY Divine Favor
            if (allyNeedBigHeal.Count > 0
                && !Me.HasAura(DivineFavor)
                && cast.OnSelf(DivineFavor))
                return;

            // PARTY Holy Light
            List<IWoWPlayer> allyNeedMediumHeal = unitCache.GroupAndRaid
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyHolyLightPercentThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            if (allyNeedMediumHeal.Count > 0
                && cast.OnFocusUnit(HolyLight, allyNeedMediumHeal[0]))
                return;

            // PARTY Holy Light rank 5 (for the buff)
            if (allyNeedSmallHeal.Count == 1
                && HolyLight.Cost == 840
                && WTEffects.BuffTimeLeft("Light's Grace") < 5
                && cast.OnFocusUnit(HolyLightRank5, allyNeedSmallHeal[0]))
                return;

            // PARTY Flash Heal
            if (allyNeedSmallHeal.Count > 0
                && cast.OnFocusUnit(FlashOfLight, allyNeedSmallHeal[0]))
                return;

            // Seal of light
            if (settings.PartyHolySealOfLight
                && !Target.HasAura("Judgement of Light"))
            {
                if (cast.OnTarget(Judgement))
                    return;

                if (!Me.HasAura(SealOfLight)
                    && cast.OnSelf(SealOfLight))
                    return;
            }

            // PARTY Purifiy
            if (settings.PartyPurify)
            {
                IWoWPlayer needsPurify = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name) || WTEffects.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }

            // PARTY Cleanse
            if (settings.PartyCleanse)
            {
                IWoWPlayer needsCleanse = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }
        }
    }
}