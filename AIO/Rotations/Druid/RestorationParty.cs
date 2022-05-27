using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class RestorationParty : Druid
    {
        public RestorationParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Tank;
        }

        protected override void BuffRotation()
        {
            if ((!Me.HasDrinkBuff || Me.ManaPercentage > 95) && Rejuvenation.IsSpellUsable)
            {
                base.BuffRotation();

                // PARTY Remove Curse
                IWoWPlayer needRemoveCurse = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                    return;

                // PARTY Abolish Poison
                IWoWPlayer needAbolishPoison = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                    return;

                // PARTY Mark of the Wild
                IWoWPlayer needMotW = unitCache.GroupAndRaid
                    .Find(m => !m.HasAura(MarkOfTheWild));
                if (needMotW != null && cast.OnFocusUnit(MarkOfTheWild, needMotW))
                    return;

                // PARTY Thorns
                IWoWPlayer needThorns = unitCache.GroupAndRaid
                    .Find(m => !m.HasAura(Thorns));
                if (needThorns != null && cast.OnFocusUnit(Thorns, needThorns))
                    return;

                // Omen of Clarity
                if (!Me.HasAura(OmenOfClarity)
                    && OmenOfClarity.IsSpellUsable
                    && cast.OnTarget(OmenOfClarity))
                    return;

                // Regrowth
                IWoWPlayer needRegrowth = unitCache.GroupAndRaid
                    .Find(m => m.HealthPercent < 80 && !m.HasAura(Regrowth));
                if (needRegrowth != null
                    && cast.OnFocusUnit(Regrowth, needRegrowth))
                    return;

                // Rejuvenation
                IWoWPlayer needRejuvenation = unitCache.GroupAndRaid
                    .Find(m => m.HealthPercent < 80 && !m.HasAura(Rejuvenation));
                if (needRejuvenation != null
                    && cast.OnFocusUnit(Rejuvenation, needRejuvenation))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;

                // Tree form
                if (!Me.HasAura(TreeOfLife)
                    && cast.OnSelf(TreeOfLife))
                    return;
            }
        }

        protected override void HealerCombat()
        {
            base.HealerCombat();

            List<IWoWPlayer> lisPartyOrdered = unitCache.GroupAndRaid
                .OrderBy(m => m.HealthPercent)
                .ToList();

            // Party Tranquility
            if (settings.PartyTranquility && !unitCache.EnemiesFighting.Any(e => e.IsTargetingMe))
            {
                bool needTranquility = unitCache.GroupAndRaid
                    .FindAll(m => m.HealthPercent < 50)
                    .Count > 2;
                if (needTranquility
                    && cast.OnTarget(Tranquility))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }
            }

            // PARTY Rebirth
            if (settings.PartyUseRebirth)
            {
                IWoWPlayer needRebirth = lisPartyOrdered
                    .Find(m => m.IsDead);
                if (needRebirth != null && cast.OnFocusUnit(Rebirth, needRebirth))
                    return;
            }

            // PARTY Innervate
            if (settings.PartyUseInnervate)
            {
                IWoWPlayer needInnervate = lisPartyOrdered
                    .Find(m => m.ManaPercentage < 10 && !m.HasAura(Innervate));
                if (needInnervate != null && cast.OnFocusUnit(Innervate, needInnervate))
                    return;
            }

            if (settings.PartyRemoveCurse)
            {
                // PARTY Remove Curse
                IWoWPlayer needRemoveCurse = lisPartyOrdered
                    .Find(m => WTEffects.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                    return;
            }

            if (settings.PartyAbolishPoison)
            {
                // PARTY Abolish Poison
                IWoWPlayer needAbolishPoison = lisPartyOrdered
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                    return;
            }

            // PARTY Burst Heal
            IWoWPlayer needBurstHeal = lisPartyOrdered
                .Find(m => m.HealthPercent < 30);
            if (needBurstHeal != null
                && Me.ManaPercentage > 10
                && cast.OnTarget(NaturesSwiftness))
                return;
            if (needBurstHeal != null
                && Me.HasAura("Nature's Swiftness"))
            {
                cast.OnFocusUnit(HealingTouch, needBurstHeal);
                return;
            }

            // Tree form
            if (!Me.HasAura(TreeOfLife)
                && Me.ManaPercentage > 20
                && cast.OnSelf(TreeOfLife))
                return;

            // Swiftmend
            IWoWPlayer needBigHeal = lisPartyOrdered
                .Find(m => m.HealthPercent < 60 && (m.HasAura(Regrowth) || m.HasAura(Rejuvenation)));
            if (needBigHeal != null
                && cast.OnFocusUnit(Swiftmend, needBigHeal))
                return;

            // Healing Touch
            if (!Me.HasAura(TreeOfLife))
            {
                if (needBigHeal != null
                    && cast.OnFocusUnit(HealingTouch, needBigHeal))
                    return;
            }

            // Regrowth
            IWoWPlayer needRegrowth = lisPartyOrdered
                .Find(m => m.HealthPercent < 70 && !m.HasAura(Regrowth));
            if (needRegrowth != null
                && cast.OnFocusUnit(Regrowth, needRegrowth))
                return;

            // Rejuvenation
            IWoWPlayer needRejuvenation = lisPartyOrdered
                .Find(m => m.HealthPercent < 80 && !m.HasAura(Rejuvenation));
            if (needRejuvenation != null
                && cast.OnFocusUnit(Rejuvenation, needRejuvenation))
                return;

            // Lifebloom 1
            IWoWPlayer needLifeBloom1 = lisPartyOrdered
                .Find(m => m.HealthPercent < 90 && m.BuffStacks(Lifebloom) < 1);
            if (needLifeBloom1 != null
                && cast.OnFocusUnit(Lifebloom, needLifeBloom1))
                return;

            // Lifebloom 2
            IWoWPlayer needLifeBloom2 = lisPartyOrdered
                .Find(m => m.HealthPercent < 85 && m.BuffStacks(Lifebloom) < 2);
            if (needLifeBloom2 != null
                && cast.OnFocusUnit(Lifebloom, needLifeBloom2))
                return;

            // Lifebloom 3
            IWoWPlayer needLifeBloom3 = lisPartyOrdered
                .Find(m => m.HealthPercent < 80 && m.BuffStacks(Lifebloom) < 3);
            if (needLifeBloom3 != null
                && cast.OnFocusUnit(Lifebloom, needLifeBloom3))
                return;
        }
    }
}
