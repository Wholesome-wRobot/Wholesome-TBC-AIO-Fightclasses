using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class Enhancement : Shaman
    {
        public Enhancement(BaseSettings settings) : base(settings)
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
            if (casterEnemies.Contains(Target.Name))
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

            // Pull logic
            if (ToolBox.Pull(cast, settings.ENAlwaysPullWithLightningBolt, new List<AIOSpell> { settings.ENPullRankOneLightningBolt ? LightningBoltRank1 : null, LightningBolt }, unitCache))
            {
                combatMeleeTimer = new Timer(2000);
                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool isPoisoned = WTEffects.HasPoisonDebuff();
            bool hasDisease = WTEffects.HasDiseaseDebuff();
            bool shouldBeInterrupted = WTCombat.TargetIsCasting();

            // Force melee
            if (combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            if (shouldBeInterrupted)
            {
                fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!casterEnemies.Contains(Target.Name))
                    casterEnemies.Add(Target.Name);
            }

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
                && CureDisease.KnownSpell
                && hasDisease
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

            // Shamanistic Rage
            if (Me.ManaPercentage < mediumManaThreshold
                && (Target.HealthPercent > 80 && !settings.ENShamanisticRageOnMultiOnly || unitCache.EnemiesAttackingMe.Count > 1)
                && cast.OnSelf(ShamanisticRage))
                return;

            // Earth Shock Focused
            if (Me.HasAura("Focused")
                && Target.GetDistance < 19f
                && cast.OnTarget(EarthShock))
                return;

            // Frost Shock
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.ENFrostShockHumanoids
                && Target.HealthPercent < 40
                && !Target.HasAura(FrostShock)
                && cast.OnTarget(FrostShock))
                return;

            // Earth Shock Interupt Rank 1
            if (shouldBeInterrupted
                && (settings.ENInterruptWithRankOne || Me.ManaPercentage <= lowManaThreshold))
            {
                fightingACaster = true;
                if (!casterEnemies.Contains(Target.Name))
                    casterEnemies.Add(Target.Name);
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(EarthShockRank1))
                    return;
            }

            // Earth Shock Interupt
            if (shouldBeInterrupted
                && !settings.ENInterruptWithRankOne)
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
            if (Me.ManaPercentage > lowManaThreshold
                && Target.GetDistance < 19f
                && !Target.HasAura(FlameShock)
                && Target.HealthPercent > 20
                && settings.UseFlameShock
                && cast.OnTarget(FlameShock))
                return;

            // Stormstrike
            if (Me.ManaPercentage > lowManaThreshold
                && cast.OnTarget(Stormstrike))
                return;

            // Earth Shock DPS
            if (Me.ManaPercentage > lowManaThreshold
                && Target.GetDistance < 19f
                && !FlameShock.KnownSpell
                && Target.HealthPercent > 25
                && Me.ManaPercentage > 30
                && cast.OnTarget(EarthShock))
                return;

            // Low level lightning bolt
            if (!EarthShock.KnownSpell
                && Me.ManaPercentage > lowManaThreshold
                && Target.HealthPercent > 40
                && cast.OnTarget(LightningBolt))
                return;
        }
    }
}
