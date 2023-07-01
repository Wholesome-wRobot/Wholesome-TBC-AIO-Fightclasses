using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class ShadowParty : Priest
    {
        public ShadowParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasDrinkAura || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // Power Word Fortitude
                if (settings.PSH_UsePowerWordFortitude
                    && cast.Buff(unitCache.GroupAndRaid, PowerWordFortitude))
                    return;

                // Shadow Protection
                if (settings.PSH_UseShadowProtection
                    && cast.Buff(unitCache.GroupAndRaid, ShadowProtection))
                    return;

                // Divine spirit
                if (settings.PSH_UseDivineSpirit
                    && cast.Buff(unitCache.GroupAndRaid, DivineSpirit))
                    return;

                // OOC Inner Fire            
                if (settings.PSH_UseInnerFire
                    && !Me.HasAura(InnerFire)
                    && cast.OnSelf(InnerFire))
                    return;

                // OOC Shadowguard
                if (!Me.HasAura(Shadowguard)
                    && settings.PSH_UseShadowGuard
                    && cast.OnSelf(Shadowguard))
                    return;

                // OOC ShadowForm
                if (!Me.HasAura(Shadowform)
                    && cast.OnSelf(Shadowform))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            // Pull ShadowForm
            if (!Me.HasAura(Shadowform)
                && cast.OnSelf(Shadowform))
                return;

            // Vampiric Touch
            if (!Target.HasAura(VampiricTouch)
                && cast.OnTarget(VampiricTouch))
                return;

            // Shadow Word Pain
            if (!Target.HasAura(ShadowWordPain)
                && cast.OnTarget(ShadowWordPain))
                return;
        }

        protected override void CombatRotation()
        {
            int innerFocusCD = WTCombat.GetSpellCooldown(InnerFocus.Name);

            // Fade
            if (unitCache.EnemyUnitsTargetingPlayer.Count > 0
                && cast.OnSelf(Fade))
                return;

            // Inner Focus  + spell
            if (Me.HasAura(InnerFocus)
                && Target.HealthPercent > 80)
            {
                if (cast.OnTarget(DevouringPlague) || cast.OnTarget(ShadowWordPain))
                 return;
            }

            // Power Word Shield
            if (Me.HealthPercent < 50
                && !Me.HasAura(PowerWordShield)
                && !WTEffects.HasDebuff("Weakened Soul")
                && settings.PSH_UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Silence
            if (WTCombat.TargetIsCasting()
                && cast.OnTarget(Silence))
                return;

            // Cure Disease
            if (settings.PSH_CureDisease)
            {
                // PARTY Cure Disease
                IWoWPlayer needCureDisease = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // PARTY Dispel Magic
            if (settings.PSH_DispelMagic)
            {
                IWoWPlayer needDispelMagic = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needDispelMagic != null && cast.OnFocusUnit(DispelMagic, needDispelMagic))
                    return;
            }

            // Combat ShadowForm
            if (!Me.HasAura(Shadowform)
                && cast.OnSelf(Shadowform))
                return;

            // ShadowFiend
            if (Me.ManaPercentage < 30
                && cast.OnTarget(Shadowfiend))
                return;

            // Vampiric Touch
            if (!Target.HasAura(VampiricTouch)
                && cast.OnTarget(VampiricTouch))
                return;

            if (settings.PSH_VampiricEmbrace)
            {
                // Vampiric Embrace
                if (!Target.HasAura(VampiricEmbrace)
                    && Target.HasAura(VampiricTouch)
                    && cast.OnTarget(VampiricEmbrace))
                    return;
            }

            // Inner Focus
            if (Target.HealthPercent > 80
                && innerFocusCD <= 0
                && !Me.HasAura(InnerFocus)
                && cast.OnSelf(InnerFocus))
                return;

            // Devouring Plague
            if (!Target.HasAura(DevouringPlague)
                && Target.HealthPercent > 80
                && cast.OnTarget(DevouringPlague))
                return;

            // PARTY Shadow Word Pain
            List<IWoWUnit> enemiesWithoutPain = unitCache.EnemiesFighting
                .Where(e => e.InCombatFlagOnly && !e.HasAura(ShadowWordPain))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutPain.Count > 0
               && unitCache.EnemiesFighting.Count - enemiesWithoutPain.Count < 3
               && cast.OnFocusUnit(ShadowWordPain, enemiesWithoutPain[0]))
                return;

            // Mind Blast
            if (Me.ManaPercentage > settings.PSH_MindBlastThreshold
                && cast.OnTarget(MindBlast))
                return;

            // Shadow Word Death
            if (Me.HealthPercent > settings.PSH_SWDeathThreshold
                && cast.OnTarget(ShadowWordDeath))
                return;

            // Mind FLay
            if (cast.OnTarget(MindFlay))
                return;

            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(Target, "Shoot"))
                if (cast.OnTarget(MindBlast) || cast.OnTarget(Smite))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && iCanUseWand
                && cast.OnTarget(UseWand, false))
                return;
        }
    }
}
