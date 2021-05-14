using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class HolyPriestParty : Priest
    {
        protected override void BuffRotation()
        {
            // PARTY Resurrection
            List<WoWPlayer> needRes = AIOParty.Group
                .FindAll(m => m.IsDead)
                .OrderBy(m => m.GetDistance)
                .ToList();
            if (needRes.Count > 0 && cast.OnFocusPlayer(Resurrection, needRes[0], onDeadTarget: true))
            {
                Thread.Sleep(3000);
                return;
            } 

            // PARTY Greater heal
            List<WoWPlayer> needGreaterHeal = AIOParty.Group
                .FindAll(m => m.IsAlive && m.HealthPercent < 50)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needGreaterHeal.Count > 0 && cast.OnFocusPlayer(GreaterHeal, needGreaterHeal[0]))
                return;

            // PARTY Heal
            List<WoWPlayer> needHeal = AIOParty.Group
                .FindAll(m => m.HealthPercent < 80)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needHeal.Count > 0 && cast.OnFocusPlayer(FlashHeal, needHeal[0]))
                return;

            // PARTY Renew
            List<WoWPlayer> needRenew = AIOParty.Group
                .FindAll(m => m.HealthPercent < 90 && !m.HaveBuff(Renew.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needRenew.Count > 0 && cast.OnFocusPlayer(Renew, needRenew[0]))
                return;

            // PARTY Power Word Fortitude
            WoWPlayer noPWF = AIOParty.Group
                .Find(m => !m.HaveBuff(PowerWordFortitude.Name));
            if (noPWF != null && cast.OnFocusPlayer(PowerWordFortitude, noPWF))
                return;

            // PARTY Divine Spirit
            WoWPlayer noDS = AIOParty.Group
                .Find(m => !m.HaveBuff(DivineSpirit.Name));
            if (noDS != null && cast.OnFocusPlayer(DivineSpirit, noDS))
                return;

            // OOC Inner Fire
            if (!Me.HaveBuff("Inner Fire")
                && settings.UseInnerFire)
                if (cast.Normal(InnerFire))
                    return;

            // PARTY Shadow Protection
            if (settings.PartyShadowProtection)
            {
                WoWPlayer noShadowProtection = AIOParty.Group
                    .Find(m => !m.HaveBuff(ShadowProtection.Name));
                if (noShadowProtection != null && cast.OnFocusPlayer(ShadowProtection, noShadowProtection))
                    return;
            }

            // OOC Shadow Protection
            if (!Me.HaveBuff("Shadow Protection")
                && settings.UseShadowProtection
                && cast.OnSelf(ShadowProtection))
                return;

            // PARTY Drink
            ToolBox.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold);
        }

        protected override void HealerCombat()
        {
            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // Party Cure Disease
                WoWPlayer needCureDisease = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusPlayer(CureDisease, needCureDisease))
                    return;
            }

            // Party Dispel Magic
            if (settings.PartyDispelMagic)
            {
                WoWPlayer needDispelMagic = AIOParty.Group
                    .Find(m => ToolBox.HasMagicDebuff(m.Name));
                if (needDispelMagic != null && cast.OnFocusPlayer(DispelMagic, needDispelMagic))
                    return;
            }

            // PARTY Flash heal
            List<WoWPlayer> needFlashHeal = AIOParty.Group
                .FindAll(m => m.HealthPercent < 40)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needFlashHeal.Count > 0 && cast.OnFocusPlayer(FlashHeal, needFlashHeal[0]))
                return;

            // PARTY Heal
            if (!FlashHeal.KnownSpell)
            {
                List<WoWPlayer> needHeal = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 60)
                    .OrderBy(m => m.HealthPercent)
                    .ToList();
                if (needHeal.Count > 0 && cast.OnFocusPlayer(Heal, needHeal[0]))
                    return;
            }

            // PARTY Greater heal
            List<WoWPlayer> needGreaterHeal = AIOParty.Group
                .FindAll(m => m.HealthPercent < 60)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needGreaterHeal.Count > 0 && cast.OnFocusPlayer(GreaterHeal, needGreaterHeal[0]))
                return;

            // PARTY Prayer Healing
            List<WoWPlayer> needPrayerOfHealing = AIOParty.Group
                .FindAll(m => m.IsAlive && m.HealthPercent < 70)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needPrayerOfHealing.Count > 2 && cast.Normal(PrayerOfHealing))
                return;

            // PARTY Prayer of Mending
            List<WoWPlayer> needPrayerOfMending = AIOParty.Group
                .FindAll(m => m.IsAlive && m.HealthPercent < 70 && !m.HaveBuff(PrayerOfMending.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needPrayerOfMending.Count > 1 && cast.OnFocusPlayer(PrayerOfMending, needPrayerOfMending[0]))
                return;

            // PARTY Shield
            List<WoWPlayer> neeedShield = AIOParty.Group
                .FindAll(m => m.HealthPercent < 90 && !m.HaveBuff("Power Word: Shield") && !ToolBox.HasDebuff("Weakened Soul", m.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (neeedShield.Count > 0 && cast.OnFocusPlayer(PowerWordShield, neeedShield[0]))
                return;

            // PARTY Renew
            List<WoWPlayer> needRenew = AIOParty.Group
                .FindAll(m => m.HealthPercent < 90 && !m.HaveBuff(Renew.Name))
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needRenew.Count > 0 && cast.OnFocusPlayer(Renew, needRenew[0]))
                return;
        }
    }
}
