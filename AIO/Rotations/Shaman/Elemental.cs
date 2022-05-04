using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class Elemental : Shaman
    {
        public Elemental(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();

            if (!Me.HasAura(GhostWolf))
            {
                // Ghost Wolf
                if (settings.GhostWolfMount
                    && wManager.wManagerSetting.CurrentSetting.GroundMountName == ""
                    && GhostWolf.KnownSpell)
                    WTSettings.SetGroundMount(GhostWolf.Name);

                // Lesser Healing Wave OOC
                if (Me.HealthPercent < settings.OOCHealThreshold
                    && cast.OnSelf(LesserHealingWave))
                    return;

                // Healing Wave OOC
                if (Me.HealthPercent < settings.OOCHealThreshold
                    && cast.OnSelf(HealingWave))
                    return;

                // Water Shield
                if (!Me.HasAura(WaterShield)
                    && !Me.HasAura(LightningShield)
                    && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < 20)
                    && cast.OnSelf(WaterShield))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Check if caster
            if (casterEnemies.Contains(ObjectManager.Target.Name))
                fightingACaster = true;

            // Remove Ghost Wolf
            if (Me.HasAura(GhostWolf)
                && cast.OnSelf(GhostWolf))
                return;

            // Water Shield
            if (!Me.HasAura(WaterShield)
                && !Me.HasAura(LightningShield)
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;

            // Ligntning Shield
            if (Me.ManaPercentage > lowManaThreshold
                && !Me.HasAura(LightningShield)
                && !Me.HasAura(WaterShield)
                && settings.UseLightningShield
                && (!WaterShield.KnownSpell || !settings.UseWaterShield)
                && cast.OnTarget(LightningShield))
                return;

            // Totems
            if (Me.ManaPercentage > lowManaThreshold
                && ObjectManager.Target.GetDistance < 30
                && totemManager.CastTotems(specialization))
                return;

            // Elemental Mastery
            if (!Me.HasAura(ElementalMastery)
                && cast.OnSelf(ElementalMastery))
                return;

            // Lightning Bolt
            if (cast.OnTarget(LightningBolt))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool isPoisoned = WTEffects.HasPoisonDebuff();
            bool hasDisease = WTEffects.HasDiseaseDebuff();

            // Remove Ghost Wolf
            if (Me.HasAura(GhostWolf)
                && cast.OnSelf(GhostWolf))
                return;

            // Healing Wave + Lesser Healing Wave
            if (Me.HealthPercent < settings.HealThreshold
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.OnSelf(LesserHealingWave) || cast.OnSelf(HealingWave))
                    return;

            // Cure Poison
            if (settings.CurePoison
                && isPoisoned
                && CurePoison.KnownSpell
                && Me.ManaPercentage > lowManaThreshold)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CurePoison))
                    return;
            }

            // Cure Disease
            if (settings.CureDisease
                && hasDisease
                && CureDisease.KnownSpell
                && Me.ManaPercentage > lowManaThreshold)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CureDisease))
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
                && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage <= lowManaThreshold)
                && cast.OnSelf(WaterShield))
                return;

            // Lightning Shield
            if (Me.ManaPercentage > lowManaThreshold
                && !Me.HasAura(LightningShield)
                && !Me.HasAura(WaterShield)
                && settings.UseLightningShield
                && (!WaterShield.KnownSpell || !settings.UseWaterShield)
                && cast.OnTarget(LightningShield))
                return;

            // Earth Shock Interupt
            if (WTCombat.TargetIsCasting())
            {
                if (!casterEnemies.Contains(Target.Name))
                    casterEnemies.Add(Target.Name);
                fightingACaster = true;
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(EarthShock))
                    return;
            }

            bool focusedCasting = Me.HasBuff("Focused Casting");
            // Frost Shock
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.ENFrostShockHumanoids
                && Target.HealthPercent < 40
                && !Target.HasAura(FrostShock)
                && !focusedCasting
                && cast.OnTarget(FrostShock))
                return;

            // Totems
            if (Me.ManaPercentage > lowManaThreshold
                && Target.GetDistance < 20
                && totemManager.CastTotems(specialization))
                return;

            // Chain Lightning
            if (settings.ELChainLightningOnMulti
                && unitCache.EnemiesAttackingMe.Count > 1
                && Me.ManaPercentage > 20
                && cast.OnTarget(ChainLightning))
                return;

            // Earth Shock DPS
            if (Target.GetDistance < 19f
                && (!FlameShock.KnownSpell || !settings.UseFlameShock)
                && !fightingACaster
                && Target.HealthPercent > 25
                && Me.ManaPercentage > settings.ELShockDPSMana
                && !focusedCasting
                && cast.OnTarget(EarthShock))
                return;

            // Flame Shock DPS
            if (Target.GetDistance < 19f
                && !Target.HasAura(FlameShock)
                && Target.HealthPercent > 20
                && !fightingACaster
                && settings.UseFlameShock
                && Me.ManaPercentage > settings.ELShockDPSMana
                && !focusedCasting
                && cast.OnTarget(FlameShock))
                return;

            // Lightning Bolt
            if (ObjectManager.Target.GetDistance <= pullRange
                && (Target.HealthPercent > settings.ELLBHealthThreshold || Me.HasBuff("Clearcasting") || focusedCasting)
                && Me.ManaPercentage > 15
                && cast.OnTarget(LightningBolt))
                return;

            // Default melee
            if (!RangeManager.CurrentRangeIsMelee())
            {
                Logger.Log("Going in melee because nothing else to do");
                RangeManager.SetRangeToMelee();
            }
        }
    }
}
