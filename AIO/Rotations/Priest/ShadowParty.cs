using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class ShadowParty : Priest
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

            // Party Cure Disease
            WoWPlayer needCureDisease = AIOParty.Group
                .Find(m => ToolBox.HasDiseaseDebuff(m.Name));
            if (needCureDisease != null && cast.OnFocusPlayer(CureDisease, needCureDisease))
                return;

            // Party Dispel Magic
            WoWPlayer needDispelMagic = AIOParty.Group
                .Find(m => ToolBox.HasMagicDebuff(m.Name));
            if (needDispelMagic != null && cast.OnFocusPlayer(DispelMagic, needDispelMagic))
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
                if (noShadowProtection != null && cast.OnFocusPlayer(ShadowProtection, noShadowProtection))
                    return;
            }

            // OOC Shadow Protection
            if (!Me.HaveBuff("Shadow Protection")
                && settings.UseShadowProtection
                && cast.OnSelf(ShadowProtection))
                return;

            // OOC ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.Normal(Shadowform))
                return;

            // PARTY Drink
            ToolBox.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold);
        }

        protected override void Pull()
        {
            // Pull ShadowForm
            if (!Me.HaveBuff("ShadowForm")
                && cast.Normal(Shadowform))
                return;

            // Vampiric Touch
            if (ObjectManager.Target.GetDistance <= _distanceRange
                && !ObjectManager.Target.HaveBuff("Vampiric Touch")
                && cast.Normal(VampiricTouch))
                return;

            // Shadow Word Pain
            if (ObjectManager.Target.GetDistance <= _distanceRange
                && !ObjectManager.Target.HaveBuff("Shadow Word: Pain")
                && cast.Normal(ShadowWordPain))
                    return;
        }

        protected override void CombatRotation()
        {
            WoWUnit Target = ObjectManager.Target;

            // Inner Focus  + sopell
            if (Me.HaveBuff("Inner Focus") && Target.HealthPercent > 80)
            {
                cast.Normal(DevouringPlague);
                cast.Normal(ShadowWordPain);
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
                && cast.Normal(Silence))
                return;

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

            // ShadowFiend
            if (Me.ManaPercentage < 30
                && cast.Normal(Shadowfiend))
                return;

            // Vampiric Touch
            if (Target.GetDistance <= _distanceRange
                && !Target.HaveBuff("Vampiric Touch")
                && cast.Normal(VampiricTouch))
                return;

            if (settings.PartyVampiricEmbrace)
            {
                // Vampiric Embrace
                if (!Target.HaveBuff("Vampiric Embrace")
                    && Target.HaveBuff("Vampiric Touch")
                    && cast.Normal(VampiricEmbrace))
                    return;
            }

            // Inner Focus
            if (Target.HealthPercent > 80
                && cast.Normal(InnerFocus))
                return;

            // Devouring Plague
            if (!Target.HaveBuff("Devouring Plague")
                && Target.HealthPercent > 80
                && cast.Normal(DevouringPlague))
                return;

            // PARTY Shadow Word Pain
            List<WoWUnit> enemiesWithoutPain = _partyEnemiesAround
                .Where(e => e.InCombatFlagOnly && !e.HaveBuff("Shadow Word: Pain"))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutPain.Count > 0
               && _partyEnemiesAround.Where(e => e.InCombatFlagOnly).ToList().Count - enemiesWithoutPain.Count < 3
               && cast.OnFocusUnit(ShadowWordPain, enemiesWithoutPain[0]))
               return;

            // Mind Blast
            if (Me.ManaPercentage > settings.PartyMindBlastThreshold
                && cast.Normal(MindBlast))
                return;

            // Shadow Word Death
            if (Me.HealthPercent > settings.PartySWDeathThreshold
                && Target.GetDistance < _distanceRange
                && settings.UseShadowWordDeath
                && cast.Normal(ShadowWordDeath))
                return;

            // Mind Flay Range check
            if (Target.GetDistance > MindFlay.MaxRange
                && MindFlay.KnownSpell)
            {
                Logger.LogFight("Approaching to be in Mind Flay range");
                RangeManager.SetRange(MindFlay.MaxRange - 2);
                return;
            }

            // Mind FLay
            if (MindFlay.IsDistanceGood
                && cast.Normal(MindFlay))
                return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && cast.BannedSpells.Contains("Shoot")
                && cast.Normal(UseWand))
                return;

            // Spell if wand banned
            if (cast.BannedSpells.Contains("Shoot")
                && Target.GetDistance < _distanceRange)
                if (cast.Normal(MindBlast) || cast.Normal(Smite))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && Target.GetDistance <= _distanceRange + 2)
            {
                RangeManager.SetRange(_distanceRange);
                if (cast.Normal(UseWand, false))
                    return;
            }
        }
    }
}
