using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class ShamanRestoParty : Shaman
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            if (!Me.HaveBuff("Ghost Wolf"))
            {
                // PARTY Resurrection
                List<AIOPartyMember> needRes = AIOParty.Group
                    .FindAll(m => m.IsDead)
                    .OrderBy(m => m.GetDistance)
                    .ToList();
                if (needRes.Count > 0 && cast.OnFocusPlayer(AncestralSpirit, needRes[0]))
                {
                    Thread.Sleep(3000);
                    return;
                }

                // Ghost Wolf
                if (settings.GhostWolfMount
                    && wManager.wManagerSetting.CurrentSetting.GroundMountName == ""
                    && GhostWolf.KnownSpell)
                    ToolBox.SetGroundMount(GhostWolf.Name);

                // PARTY Healing Wave
                List<AIOPartyMember> alliesNeedingHealWave = AIOParty.Group
                    .FindAll(a => a.IsAlive && a.HealthPercent < 70)
                    .OrderBy(a => a.HealthPercent)
                    .ToList();
                if (alliesNeedingHealWave.Count > 0
                    && cast.OnFocusPlayer(HealingWave, alliesNeedingHealWave[0]))
                    return;

                // Water Shield
                if (!Me.HaveBuff("Water Shield")
                    && !Me.HaveBuff("Lightning Shield")
                    && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < 20)
                    && cast.OnSelf(WaterShield))
                    return;

                // PARTY Cure poison
                WoWPlayer needCurePoison = AIOParty.Group
                    .Find(m => ToolBox.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusPlayer(CurePoison, needCurePoison))
                    return;

                // PARTY Cure Disease
                WoWPlayer needCureDisease = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusPlayer(CureDisease, needCureDisease))
                    return;

                // PARTY Drink
                if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Remove Ghost Wolf
            if (Me.HaveBuff("Ghost Wolf")
                && cast.OnSelf(GhostWolf))
                return;

            // Water Shield
            if (!Me.HaveBuff("Water Shield")
                && !Me.HaveBuff("Lightning Shield")
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < _lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;
        }

        protected override void HealerCombat()
        {
            base.HealerCombat();

            WoWUnit Target = ObjectManager.Target;

            WoWPlayer allyNeedBigHeal = AIOParty.Group
                .Find(a => a.IsAlive && a.HealthPercent < 40);

            RangeManager.SetRange(25);

            // Remove Ghost Wolf
            if (Me.HaveBuff("Ghost Wolf")
                && cast.OnSelf(GhostWolf))
                return;

            // PARTY Healing Wave with NATURE SWIFTNESS
            if (Me.HaveBuff("Nature's Swiftness"))
            {
                if (allyNeedBigHeal != null && cast.OnFocusPlayer(HealingWave, allyNeedBigHeal))
                    return;
            }

            // Party Nature's Swiftness
            if (allyNeedBigHeal != null
                && !Me.HaveBuff("Nature's Swiftness")
                && cast.OnSelf(NaturesSwiftness))
                return;

            // PARTY Lesser Healing Wave
            List<AIOPartyMember> alliesNeedingLesserHealWave = AIOParty.Group
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyLesserHealingWaveThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            if (alliesNeedingLesserHealWave.Count > 0
                && cast.OnFocusPlayer(LesserHealingWave, alliesNeedingLesserHealWave[0]))
                return;

            // PARTY Healing Wave
            List<AIOPartyMember> alliesNeedingHealWave = AIOParty.Group
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyHealingWaveThreshold)
                .OrderBy(a => a.HealthPercent)
                .ToList();
            if (alliesNeedingHealWave.Count > 0
                && cast.OnFocusPlayer(HealingWave, alliesNeedingHealWave[0]))
                return;

            // PARTY Chain Heal
            List<AIOPartyMember> alliesNeedChainHeal = AIOParty.Group
                .FindAll(a => a.IsAlive && a.HealthPercent < settings.PartyChainHealThreshold)
                .OrderBy(a => a.GetDistance)
                .ToList();
            if (alliesNeedChainHeal.Count >= settings.PartyChainHealAmount)
            {
                if (alliesNeedChainHeal.Exists(p => p.Guid == Me.Guid)
                    && cast.OnSelf(ChainHeal))
                    return;
                if (cast.OnFocusPlayer(ChainHeal, alliesNeedChainHeal[0]))
                    return;
            }

            // PARTY Earth Shield
            if (EarthShield.KnownSpell && !AIOParty.Group.Exists(a => a.HaveBuff("Earth Shield")))
            {
                foreach (WoWPlayer player in AIOParty.Group.FindAll(p => p.IsAlive && p.WowClass != wManager.Wow.Enums.WoWClass.Shaman))
                {
                    List<WoWUnit> enemiesTargetingHim = AIOParty.EnemiesFighting
                        .FindAll(e => e.Target == player.Guid);
                    if (enemiesTargetingHim.Count > 1 && cast.OnFocusPlayer(EarthShield, player))
                        return;
                }
            }

            // PARTY Cure Poison
            if (settings.PartyCurePoison)
            {
                WoWPlayer needCurePoison = AIOParty.Group
                    .Find(m => ToolBox.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusPlayer(CurePoison, needCurePoison))
                    return;
            }

            // PARTY Cure Disease
            if (settings.PartyCureDisease)
            {
                WoWPlayer needCureDisease = AIOParty.Group
                    .Find(m => m.IsAlive && ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusPlayer(CureDisease, needCureDisease))
                    return;
            }

            // Bloodlust
            if (!Me.HaveBuff("Bloodlust")
                && Target.HealthPercent > 80
                && cast.OnSelf(Bloodlust))
                return;

            // Water Shield
            if (!Me.HaveBuff("Water Shield")
                && !Me.HaveBuff("Lightning Shield")
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage <= _lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;

            // Totems
            if (totemManager.CastTotems(specialization))
                return;
        }
    }
}
