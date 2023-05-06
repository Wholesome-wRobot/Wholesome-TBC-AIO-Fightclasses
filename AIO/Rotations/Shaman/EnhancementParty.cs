using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class EnhancementParty : Shaman
    {
        public EnhancementParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();

            if (!Me.HasAura(GhostWolf) && (!Me.HasDrinkAura || Me.ManaPercentage > 95))
            {
                // Ghost Wolf
                if (settings.PEN_GhostWolfMount
                    && wManager.wManagerSetting.CurrentSetting.GroundMountName == ""
                    && GhostWolf.KnownSpell)
                    WTSettings.SetGroundMount(GhostWolf.Name);

                // Lesser Healing Wave OOC
                if (Me.HealthPercent < settings.PEN_OOCHealThreshold
                    && cast.OnSelf(LesserHealingWave))
                    return;

                // Healing Wave OOC
                if (Me.HealthPercent < settings.PEN_OOCHealThreshold
                    && cast.OnSelf(HealingWave))
                    return;

                // Water Shield
                if (!Me.HasAura(WaterShield)
                    && !Me.HasAura(LightningShield)
                    && (settings.PEN_UseWaterShield || !settings.PEN_UseLightningShield || Me.ManaPercentage < 20)
                    && cast.OnSelf(WaterShield))
                    return;

                // PARTY Cure poison
                IWoWPlayer needCurePoison = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusUnit(CurePoison, needCurePoison))
                    return;

                // PARTY Cure Disease
                IWoWPlayer needCureDisease = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            RangeManager.SetRangeToMelee();

            // Check if caster
            if (casterEnemies.Contains(Target.Name))
                fightingACaster = true;

            // Remove Ghost Wolf
            if (Me.HasAura(GhostWolf)
                && cast.OnSelf(GhostWolf))
                return;

            // Water Shield
            if (!Me.HasAura(WaterShield)
                && !Me.HasAura(LightningShield)
                && (settings.PEN_UseWaterShield || !settings.PEN_UseLightningShield || Me.ManaPercentage < lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;

            // Ligntning Shield
            if (Me.ManaPercentage > lowManaThreshold
                && !Me.HasAura(LightningShield)
                && !Me.HasAura(WaterShield)
                && settings.PEN_UseLightningShield
                && (!WaterShield.KnownSpell || !settings.PEN_UseWaterShield)
                && cast.OnSelf(LightningShield))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            RangeManager.SetRangeToMelee();

            bool isPoisoned = WTEffects.HasPoisonDebuff();
            bool hasDisease = WTEffects.HasDiseaseDebuff();
            bool shouldBeInterrupted = WTCombat.TargetIsCasting();

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            if (shouldBeInterrupted
                && !casterEnemies.Contains(Target.Name))
                casterEnemies.Add(Target.Name);

            // Remove Ghost Wolf
            if (Me.HasAura(GhostWolf)
                && cast.OnSelf(GhostWolf))
                return;

            // PARTY Cure Poison
            if (settings.PEN_CurePoison)
            {
                IWoWPlayer needCurePoison = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusUnit(CurePoison, needCurePoison))
                    return;
            }

            // PARTY Cure Disease
            if (settings.PEN_CureDisease)
            {
                IWoWPlayer needCureDisease = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // Bloodlust
            if (!Me.HasAura(Bloodlust)
                && Target.HealthPercent > 80
                && cast.OnSelf(Bloodlust))
                return;

            // Water Shield
            if (!Me.HasAura(WaterShield)
                && !Me.HasAura(LightningShield)
                && (settings.PEN_UseWaterShield || !settings.PEN_UseLightningShield || Me.ManaPercentage <= lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;

            // Lightning Shield
            if (Me.ManaPercentage > lowManaThreshold
                && !Me.HasAura(LightningShield)
                && !Me.HasAura(WaterShield)
                && settings.PEN_UseLightningShield
                && (!WaterShield.KnownSpell || !settings.PEN_UseWaterShield)
                && cast.OnTarget(LightningShield))
                return;

            // Shamanistic Rage
            if (Me.ManaPercentage < 20
                && cast.OnSelf(ShamanisticRage))
                return;

            // Earth Shock Interrupt
            if (shouldBeInterrupted)
            {
                if (!casterEnemies.Contains(Target.Name))
                    casterEnemies.Add(Target.Name);
                fightingACaster = true;
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(EarthShock))
                    return;
            }

            // Totems
            if (Me.ManaPercentage > 20
                && Target.GetDistance < 20
                && totemManager.CastTotems(specialization))
                return;

            // Flame Shock DPS
            if (Target.GetDistance < 19f
                && !Target.HasAura(FlameShock)
                && cast.OnTarget(FlameShock))
                return;

            // Stormstrike
            if (Stormstrike.IsDistanceGood
                && cast.OnTarget(Stormstrike))
                return;

            // Earth Shock DPS
            if (Target.GetDistance < 19f
                && cast.OnTarget(EarthShock))
                return;
        }
    }
}
