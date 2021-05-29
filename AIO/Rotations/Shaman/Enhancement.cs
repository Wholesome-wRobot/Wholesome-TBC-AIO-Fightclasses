using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class Enhancement : Shaman
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            if (!Me.HaveBuff("Ghost Wolf"))
            {
                // Ghost Wolf
                if (settings.GhostWolfMount
                    && wManager.wManagerSetting.CurrentSetting.GroundMountName == ""
                    && GhostWolf.KnownSpell)
                    ToolBox.SetGroundMount(GhostWolf.Name);

                // Lesser Healing Wave OOC
                if (Me.HealthPercent < settings.OOCHealThreshold
                    && cast.OnSelf(LesserHealingWave))
                    return;

                // Healing Wave OOC
                if (Me.HealthPercent < settings.OOCHealThreshold
                    && cast.OnSelf(HealingWave))
                    return;

                // Water Shield
                if (!Me.HaveBuff("Water Shield")
                    && !Me.HaveBuff("Lightning Shield")
                    && (settings.UseWaterShield || !settings.UseLightningShield || Me.ManaPercentage < 20)
                    && cast.OnSelf(WaterShield))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Check if caster
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

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

            // Ligntning Shield
            if (Me.ManaPercentage > _lowManaThreshold
                && !Me.HaveBuff("Lightning Shield")
                && !Me.HaveBuff("Water Shield")
                && settings.UseLightningShield
                && (!WaterShield.KnownSpell || !settings.UseWaterShield)
                && cast.OnTarget(LightningShield))
                return;

            // Pull logic
            if (ToolBox.Pull(cast, settings.ENAlwaysPullWithLightningBolt, new List<AIOSpell> { settings.ENPullRankOneLightningBolt ? LightningBoltRank1 : null, LightningBolt }))
            {
                _combatMeleeTimer = new Timer(2000);
                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;
            bool _isPoisoned = ToolBox.HasPoisonDebuff();
            bool _hasDisease = ToolBox.HasDiseaseDebuff();
            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();

            // Force melee
            if (_combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
            }

            // Remove Ghost Wolf
            if (Me.HaveBuff("Ghost Wolf")
                && cast.OnSelf(GhostWolf))
                return;

            // Healing Wave + Lesser Healing Wave
            if (Me.HealthPercent < settings.HealThreshold
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.OnSelf(LesserHealingWave) || cast.OnSelf(HealingWave))
                    return;

            // Cure Poison
            if (settings.CurePoison
                && _isPoisoned
                && CurePoison.KnownSpell
                && Me.ManaPercentage > _lowManaThreshold)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CurePoison))
                    return;
            }

            // Cure Disease
            if (settings.CureDisease
                && CureDisease.KnownSpell
                && _hasDisease
                && Me.ManaPercentage > _lowManaThreshold)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CureDisease))
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

            // Lightning Shield
            if (Me.ManaPercentage > _lowManaThreshold
                && !Me.HaveBuff("Lightning Shield")
                && !Me.HaveBuff("Water Shield")
                && settings.UseLightningShield
                && (!WaterShield.KnownSpell || !settings.UseWaterShield)
                && cast.OnTarget(LightningShield))
                return;

            // Shamanistic Rage
            if (Me.ManaPercentage < _mediumManaThreshold
                && (Target.HealthPercent > 80 && !settings.ENShamanisticRageOnMultiOnly || ObjectManager.GetNumberAttackPlayer() > 1)
                && cast.OnSelf(ShamanisticRage))
                return;

            // Earth Shock Focused
            if (Me.HaveBuff("Focused")
                && Target.GetDistance < 19f
                && cast.OnTarget(EarthShock))
                return;

            // Frost Shock
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.ENFrostShockHumanoids
                && Target.HealthPercent < 40
                && !Target.HaveBuff("Frost Shock")
                && cast.OnTarget(FrostShock))
                return;

            // Earth Shock Interupt Rank 1
            if (_shouldBeInterrupted
                && (settings.ENInterruptWithRankOne || Me.ManaPercentage <= _lowManaThreshold))
            {
                _fightingACaster = true;
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(EarthShockRank1))
                    return;
            }

            // Earth Shock Interupt
            if (_shouldBeInterrupted
                && !settings.ENInterruptWithRankOne)
            {
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
                _fightingACaster = true;
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(EarthShock))
                    return;
            }

            // Totems
            if (Me.ManaPercentage > 20
                && Target.GetDistance < 20
                && _totemManager.CastTotems(specialization))
                return;

            // Flame Shock DPS
            if (Me.ManaPercentage > _lowManaThreshold
                && Target.GetDistance < 19f
                && !Target.HaveBuff("Flame Shock")
                && Target.HealthPercent > 20
                && settings.UseFlameShock
                && cast.OnTarget(FlameShock))
                return;

            // Stormstrike
            if (Me.ManaPercentage > _lowManaThreshold
                && cast.OnTarget(Stormstrike))
                return;

            // Earth Shock DPS
            if (Me.ManaPercentage > _lowManaThreshold
                && Target.GetDistance < 19f
                && !FlameShock.KnownSpell
                && Target.HealthPercent > 25
                && Me.ManaPercentage > 30
                && cast.OnTarget(EarthShock))
                return;

            // Low level lightning bolt
            if (!EarthShock.KnownSpell
                && Me.ManaPercentage > _lowManaThreshold
                && Target.HealthPercent > 40
                && cast.OnTarget(LightningBolt))
                return;
        }
    }
}
