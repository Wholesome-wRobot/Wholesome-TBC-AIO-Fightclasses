using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class RestorationParty : Druid
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Remove Curse
            WoWPlayer needRemoveCurse = AIOParty.Group
                .Find(m => ToolBox.HasCurseDebuff(m.Name));
            if (needRemoveCurse != null && cast.OnFocusPlayer(RemoveCurse, needRemoveCurse))
                return;

            // PARTY Abolish Poison
            WoWPlayer needAbolishPoison = AIOParty.Group
                .Find(m => ToolBox.HasPoisonDebuff(m.Name));
            if (needAbolishPoison != null && cast.OnFocusPlayer(AbolishPoison, needAbolishPoison))
                return;

            // PARTY Mark of the Wild
            WoWPlayer needMotW = AIOParty.Group
                .Find(m => !m.HaveBuff(MarkOfTheWild.Name));
            if (needMotW != null && cast.OnFocusPlayer(MarkOfTheWild, needMotW))
                return;

            // PARTY Thorns
            WoWPlayer needThorns = AIOParty.Group
                .Find(m => !m.HaveBuff(Thorns.Name));
            if (needThorns != null && cast.OnFocusPlayer(Thorns, needThorns))
                return;

            // Omen of Clarity
            if (!Me.HaveBuff("Omen of Clarity")
                && OmenOfClarity.IsSpellUsable
                && cast.OnTarget(OmenOfClarity))
                return;

            // Regrowth
            WoWPlayer needRegrowth = AIOParty.Group
                .Find(m => m.HealthPercent < 80 && !m.HaveBuff("Regrowth"));
            if (needRegrowth != null
                && cast.OnFocusPlayer(Regrowth, needRegrowth))
                return;

            // Rejuvenation
            WoWPlayer needRejuvenation = AIOParty.Group
                .Find(m => m.HealthPercent < 80 && !m.HaveBuff("Rejuvenation"));
            if (needRejuvenation != null
                && cast.OnFocusPlayer(Rejuvenation, needRejuvenation))
                return;

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;

            // Tree form
            if (!Me.HaveBuff("Tree of Life")
                && cast.OnSelf(TreeOfLife))
                return;
        }

        protected override void HealerCombat()
        {
            base.HealerCombat();

            WoWUnit Target = ObjectManager.Target;
            List<AIOPartyMember> lisPartyOrdered = AIOParty.Group
                .OrderBy(m => m.HealthPercent)
                .ToList();

            // Party Tranquility
            if (settings.PartyTranquility && !AIOParty.Group.Any(e => e.IsTargetingMe))
            {
                bool needTranquility = AIOParty.Group
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
                WoWPlayer needRebirth = lisPartyOrdered
                    .Find(m => m.IsDead);
                if (needRebirth != null && cast.OnFocusPlayer(Rebirth, needRebirth))
                    return;
            }

            // PARTY Innervate
            if (settings.PartyUseInnervate)
            {
                WoWPlayer needInnervate = lisPartyOrdered
                    .Find(m => m.ManaPercentage < 10 && !m.HaveBuff("Innervate"));
                if (needInnervate != null && cast.OnFocusPlayer(Innervate, needInnervate))
                    return;
            }

            if (settings.PartyRemoveCurse)
            {
                // PARTY Remove Curse
                WoWPlayer needRemoveCurse = lisPartyOrdered
                    .Find(m => ToolBox.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusPlayer(RemoveCurse, needRemoveCurse))
                    return;
            }

            if (settings.PartyAbolishPoison)
            {
                // PARTY Abolish Poison
                WoWPlayer needAbolishPoison = lisPartyOrdered
                    .Find(m => ToolBox.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusPlayer(AbolishPoison, needAbolishPoison))
                    return;
            }

            // PARTY Burst Heal
            WoWPlayer needBurstHeal = lisPartyOrdered
                .Find(m => m.HealthPercent < 30);
            if (needBurstHeal != null
                && Me.ManaPercentage > 10
                && cast.OnTarget(NaturesSwiftness))
                return;
            if (needBurstHeal != null
                && Me.HaveBuff("Nature's Swiftness"))
            {
                cast.OnFocusPlayer(HealingTouch, needBurstHeal);
                return;
            }

            // Tree form
            if (!Me.HaveBuff("Tree of Life")
                && Me.ManaPercentage > 20
                && cast.OnSelf(TreeOfLife))
                return;

            // Swiftmend
            WoWPlayer needBigHeal = lisPartyOrdered
                .Find(m => m.HealthPercent < 60 && (m.HaveBuff("Regrowth") || m.HaveBuff("Rejuvenation")));
            if (needBigHeal != null
                && cast.OnFocusPlayer(Swiftmend, needBigHeal))
                return;

            // Healing Touch
            if (!Me.HaveBuff("Tree of Life"))
            {
                if (needBigHeal != null
                    && cast.OnFocusPlayer(HealingTouch, needBigHeal))
                    return;
            }

            // Regrowth
            WoWPlayer needRegrowth = lisPartyOrdered
                .Find(m => m.HealthPercent < 70 && !m.HaveBuff("Regrowth"));
            if (needRegrowth != null
                && cast.OnFocusPlayer(Regrowth, needRegrowth))
                return;

            // Rejuvenation
            WoWPlayer needRejuvenation = lisPartyOrdered
                .Find(m => m.HealthPercent < 80 && !m.HaveBuff("Rejuvenation"));
            if (needRejuvenation != null
                && cast.OnFocusPlayer(Rejuvenation, needRejuvenation))
                return;

            // Lifebloom 1
            WoWPlayer needLifeBloom1 = lisPartyOrdered
                .Find(m => m.HealthPercent < 95 && ToolBox.CountBuff("Lifebloom", m.Name) < 1);
            if (needLifeBloom1 != null
                && cast.OnFocusPlayer(Lifebloom, needLifeBloom1))
                return;

            // Lifebloom 2
            WoWPlayer needLifeBloom2 = lisPartyOrdered
                .Find(m => m.HealthPercent < 90 && ToolBox.CountBuff("Lifebloom", m.Name) < 2);
            if (needLifeBloom2 != null
                && cast.OnFocusPlayer(Lifebloom, needLifeBloom2))
                return;

            // Lifebloom 3
            WoWPlayer needLifeBloom3 = lisPartyOrdered
                .Find(m => m.HealthPercent < 85 && ToolBox.CountBuff("Lifebloom", m.Name) < 3);
            if (needLifeBloom3 != null
                && cast.OnFocusPlayer(Lifebloom, needLifeBloom3))
                return;
        }
    }
}
