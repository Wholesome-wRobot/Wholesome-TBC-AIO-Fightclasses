using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class Elemental : Shaman
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

            // Totems
            if (Me.ManaPercentage > _lowManaThreshold
                && ObjectManager.Target.GetDistance < 30
                && _totemManager.CastTotems(specialization))
                return;

            // Elemental Mastery
            if (!Me.HaveBuff("Elemental Mastery")
                && cast.OnSelf(ElementalMastery))
                return;

            // Lightning Bolt
            if (cast.OnTarget(LightningBolt))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;
            bool _isPoisoned = ToolBox.HasPoisonDebuff();
            bool _hasDisease = ToolBox.HasDiseaseDebuff();
            bool _shouldBeInterrupted = false;

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
                && _hasDisease
                && CureDisease.KnownSpell
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

            // Check if we need to interrupt
            int channelTimeLeft = Lua.LuaDoString<int>(@"local spell, _, _, _, endTimeMS = UnitChannelInfo('target')
                                    if spell then
                                     local finish = endTimeMS / 1000 - GetTime()
                                     return finish
                                    end");
            if (channelTimeLeft < 0 || Target.CastingTimeLeft > ToolBox.GetLatency())
                _shouldBeInterrupted = true;

            // Earth Shock Interupt
            if (_shouldBeInterrupted)
            {
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
                _fightingACaster = true;
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(EarthShock))
                    return;
            }

            // Frost Shock
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.ENFrostShockHumanoids
                && Target.HealthPercent < 40
                && !Target.HaveBuff("Frost Shock")
                && !Me.HaveBuff("Focused Casting")
                && cast.OnTarget(FrostShock))
                return;

            // Totems
            if (Me.ManaPercentage > _lowManaThreshold
                && Target.GetDistance < 20
                && _totemManager.CastTotems(specialization))
                return;

            // Chain Lightning
            if (settings.ELChainLightningOnMulti
                && ObjectManager.GetNumberAttackPlayer() > 1
                && Me.ManaPercentage > 20
                && cast.OnTarget(ChainLightning))
                return;

            // Earth Shock DPS
            if (Target.GetDistance < 19f
                && (!FlameShock.KnownSpell || !settings.UseFlameShock)
                && !_fightingACaster
                && Target.HealthPercent > 25
                && Me.ManaPercentage > settings.ELShockDPSMana
                && !Me.HaveBuff("Focused Casting")
                && cast.OnTarget(EarthShock))
                return;

            // Flame Shock DPS
            if (Target.GetDistance < 19f
                && !Target.HaveBuff("Flame Shock")
                && Target.HealthPercent > 20
                && !_fightingACaster
                && settings.UseFlameShock
                && Me.ManaPercentage > settings.ELShockDPSMana
                && !Me.HaveBuff("Focused Casting")
                && cast.OnTarget(FlameShock))
                return;

            // Lightning Bolt
            if (ObjectManager.Target.GetDistance <= _pullRange
                && (Target.HealthPercent > settings.ELLBHealthThreshold || Me.HaveBuff("Clearcasting") || Me.HaveBuff("Focused Casting"))
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
