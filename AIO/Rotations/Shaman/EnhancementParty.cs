using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class EnhancementParty : Shaman
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

                // PARTY Cure poison
                WoWPlayer needCurePoison = AIOParty.Group
                    .Find(m => ToolBox.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusUnit(CurePoison, needCurePoison))
                    return;

                // PARTY Cure Disease
                WoWPlayer needCureDisease = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;

                // PARTY Drink
                if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            RangeManager.SetRangeToMelee();

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
                && cast.OnSelf(LightningShield))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            RangeManager.SetRangeToMelee();

            WoWUnit Target = ObjectManager.Target;
            bool _isPoisoned = ToolBox.HasPoisonDebuff();
            bool _hasDisease = ToolBox.HasDiseaseDebuff();
            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            if (_shouldBeInterrupted
                && !_casterEnemies.Contains(Target.Name))
                _casterEnemies.Add(Target.Name);

            // Remove Ghost Wolf
            if (Me.HaveBuff("Ghost Wolf")
                && cast.OnSelf(GhostWolf))
                return;

            // PARTY Cure Poison
            if (settings.PartyCurePoison)
            {
                WoWPlayer needCurePoison = AIOParty.Group
                    .Find(m => ToolBox.HasPoisonDebuff(m.Name));
                if (needCurePoison != null && cast.OnFocusUnit(CurePoison, needCurePoison))
                    return;
            }

            // PARTY Cure Disease
            if (settings.CureDisease)
            {
                WoWPlayer needCureDisease = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
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
            if (Me.ManaPercentage < 20
                && cast.OnSelf(ShamanisticRage))
                return;

            // Earth Shock Interrupt
            if (_shouldBeInterrupted)
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
            if (Target.GetDistance < 19f
                && !Target.HaveBuff("Flame Shock")
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
