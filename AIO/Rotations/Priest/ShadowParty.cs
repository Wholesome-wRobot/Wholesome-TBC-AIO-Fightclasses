using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class ShadowParty : Priest
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Party Cure Disease
            WoWPlayer needCureDisease = AIOParty.Group
                .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
            if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                return;

            // Party Dispel Magic
            WoWPlayer needDispelMagic = AIOParty.Group
                .Find(m => ToolBox.HasMagicDebuff(m.Name));
            if (needDispelMagic != null && cast.OnFocusUnit(DispelMagic, needDispelMagic))
                return;

            // PARTY Power Word Fortitude
            WoWPlayer noPWF = AIOParty.Group
                .Find(m => !m.HaveBuff(PowerWordFortitude.Name));
            if (noPWF != null && cast.OnFocusUnit(PowerWordFortitude, noPWF))
                return;

            // PARTY Divine Spirit
            WoWPlayer noDS = AIOParty.Group
                .Find(m => !m.HaveBuff(DivineSpirit.Name));
            if (noDS != null && cast.OnFocusUnit(DivineSpirit, noDS))
                return;

            // OOC Inner Fire
            if (!Me.HaveBuff("Inner Fire")
                && settings.UseInnerFire
                && cast.OnSelf(InnerFire))
                return;

            // OOC Shadowguard
            if (!Me.HaveBuff("Shadowguard")
                && settings.UseShadowGuard
                && cast.OnSelf(Shadowguard))
                return;

            // PARTY Shadow Protection
            if (settings.PartyShadowProtection)
            {
                WoWPlayer noShadowProtection = AIOParty.Group
                    .Find(m => !m.HaveBuff(ShadowProtection.Name));
                if (noShadowProtection != null && cast.OnFocusUnit(ShadowProtection, noShadowProtection))
                    return;
            }

            // OOC Shadow Protection
            if (!Me.HaveBuff("Shadow Protection")
                && settings.UseShadowProtection
                && cast.OnSelf(ShadowProtection))
                return;
           
            // OOC ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.OnSelf(Shadowform))
                return;

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;
        }

        protected override void Pull()
        {
            // Pull ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.OnSelf(Shadowform))
                return;

            // Vampiric Touch
            if (!ObjectManager.Target.HaveBuff("Vampiric Touch")
                && cast.OnTarget(VampiricTouch))
                return;

            // Shadow Word Pain
            if (!ObjectManager.Target.HaveBuff("Shadow Word: Pain")
                && cast.OnTarget(ShadowWordPain))
                return;
        }

        protected override void CombatRotation()
        {
            WoWUnit Target = ObjectManager.Target;

            // Fade
            if (AIOParty.EnemiesClose.Exists(m => m.IsTargetingMe)
                && cast.OnSelf(Fade))
                return;

            // Inner Focus  + spell
            if (Me.HaveBuff("Inner Focus") 
                && Target.HealthPercent > 80)
            {
                cast.OnTarget(DevouringPlague);
                cast.OnTarget(ShadowWordPain);
                return;
            }

            // Power Word Shield
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Power Word: Shield")
                && !ToolBox.HasDebuff("Weakened Soul")
                && settings.UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Silence
            if (ToolBox.TargetIsCasting()
                && cast.OnTarget(Silence))
                return;

            // Cure Disease
            if (settings.PartyCureDisease)
            {
                // PARTY Cure Disease
                WoWPlayer needCureDisease = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
                if (needCureDisease != null && cast.OnFocusUnit(CureDisease, needCureDisease))
                    return;
            }

            // PARTY Dispel Magic
            if (settings.PartyDispelMagic)
            {
                WoWPlayer needDispelMagic = AIOParty.Group
                    .Find(m => ToolBox.HasMagicDebuff(m.Name));
                if (needDispelMagic != null && cast.OnFocusUnit(DispelMagic, needDispelMagic))
                    return;
            }

            // Combat ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.OnSelf(Shadowform))
                return;

            // ShadowFiend
            if (Me.ManaPercentage < 30
                && cast.OnTarget(Shadowfiend))
                return;

            // Vampiric Touch
            if (!Target.HaveBuff("Vampiric Touch")
                && cast.OnTarget(VampiricTouch))
                return;

            if (settings.PartyVampiricEmbrace)
            {
                // Vampiric Embrace
                if (!Target.HaveBuff("Vampiric Embrace")
                    && Target.HaveBuff("Vampiric Touch")
                    && cast.OnTarget(VampiricEmbrace))
                    return;
            }

            // Inner Focus
            if (Target.HealthPercent > 80
                && cast.OnSelf(InnerFocus))
                return;

            // Devouring Plague
            if (!Target.HaveBuff("Devouring Plague")
                && Target.HealthPercent > 80
                && cast.OnTarget(DevouringPlague))
                return;

            // PARTY Shadow Word Pain
            List<WoWUnit> enemiesWithoutPain = AIOParty.EnemiesFighting
                .Where(e => e.InCombatFlagOnly && !e.HaveBuff("Shadow Word: Pain"))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutPain.Count > 0
               && AIOParty.EnemiesFighting.Count - enemiesWithoutPain.Count < 3
               && cast.OnFocusUnit(ShadowWordPain, enemiesWithoutPain[0]))
               return;

            // Mind Blast
            if (Me.ManaPercentage > settings.PartyMindBlastThreshold
                && cast.OnTarget(MindBlast))
                return;

            // Shadow Word Death
            if (Me.HealthPercent > settings.PartySWDeathThreshold
                && settings.UseShadowWordDeath
                && cast.OnTarget(ShadowWordDeath))
                return;

            // Mind FLay
            if (cast.OnTarget(MindFlay))
                return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(MindBlast) || cast.OnTarget(Smite))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && cast.OnTarget(UseWand, false))
                return;
        }
    }
}
