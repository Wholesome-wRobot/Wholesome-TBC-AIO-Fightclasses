using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class Shadow : Priest
    {
        public Shadow(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            // OOC Inner Fire            
            if (settings.SSH_UseInnerFire
                && !Me.HasAura(InnerFire)
                && cast.OnSelf(InnerFire))
                return;

            // Power Word Fortitude
            if (settings.SSH_UsePowerWordFortitude
                && !Me.HasAura(PowerWordFortitude)
                && !Me.HasAura(PrayerOfFortitude)
                && cast.OnSelf(PowerWordFortitude))
                return;

            // OOC Cure Disease
            if (WTEffects.HasDiseaseDebuff()
                && cast.OnSelf(CureDisease))
                return;

            // OOC Renew
            if (Me.HealthPercent < 70
                && !Me.HasAura(Renew)
                && cast.OnSelf(Renew))
                return;

            // OOC Power Word Shield
            if (Me.HealthPercent < 50
                && !Me.HasAura(PowerWordShield)
                && !WTEffects.HasDebuff("Weakened Soul")
                && unitCache.EnemiesAttackingMe.Count > 0
                && settings.SSH_UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // OOC Psychic Scream
            if (Me.HealthPercent < 30
                && unitCache.EnemiesAttackingMe.Count > 1
                && cast.OnSelf(PsychicScream))
                return;

            // OOC Divine Spirit
            if (settings.SSH_UseDivineSpirit
                && !Me.HasAura(DivineSpirit)
                && cast.OnSelf(DivineSpirit))
                return;

            // OOC Shadowguard
            if (!Me.HasAura(Shadowguard)
                && settings.SSH_UseShadowGuard
                && cast.OnSelf(Shadowguard))
                return;

            // OOC Shadow Protection
            if (settings.SSH_UseShadowProtection
                && !Me.HasAura(ShadowProtection)
                && !Me.HasAura(PrayerOfShadowProtection)
                && cast.OnSelf(ShadowProtection))
                return;

            // OOC ShadowForm
            if (!Me.HasAura(Shadowform)
                && unitCache.EnemiesAttackingMe.Count < 1
                && cast.OnSelf(Shadowform))
                return;
        }

        protected override void Pull()
        {
            // Pull ShadowForm
            if (!Me.HasAura(Shadowform)
                && cast.OnSelf(Shadowform))
                return;

            // Power Word Shield
            if (!WTEffects.HasDebuff("Weakened Soul")
                && settings.SSH_UseShieldOnPull
                && !Me.HasAura(PowerWordShield)
                && settings.SSH_UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Vampiric Touch
            if (Me.HasAura(Shadowform)
                && !Target.HasAura(VampiricTouch)
                && cast.OnTarget(VampiricTouch))
                return;

            // MindBlast
            if (Me.HasAura(Shadowform)
                && !VampiricTouch.KnownSpell
                && cast.OnTarget(MindBlast))
                return;

            // Shadow Word Pain
            if (Me.HasAura(Shadowform)
                && (!MindBlast.KnownSpell || !MindBlast.IsSpellUsable)
                && !Target.HasAura(ShadowWordPain)
                && cast.OnTarget(ShadowWordPain))
                return;

            // Holy Fire
            if (!Me.HasAura(Shadowform)
                && cast.OnTarget(HolyFire))
                return;

            // Smite
            if (!HolyFire.KnownSpell
                && !Me.HasAura(Shadowform)
                && cast.OnTarget(Smite))
                return;
        }

        protected override void CombatRotation()
        {
            bool hasMagicDebuff = settings.SSH_DispelMagic ? WTEffects.HasMagicDebuff() : false;
            bool hasDisease = settings.SSH_CureDisease ? WTEffects.HasDiseaseDebuff() : false;
            bool hasWeakenedSoul = WTEffects.HasDebuff("Weakened Soul");
            double myManaPC = Me.ManaPercentage;
            bool inShadowForm = Me.HasAura(Shadowform);
            int mindBlastCD = WTCombat.GetSpellCooldown(MindBlast.Name);
            int innerFocusCD = WTCombat.GetSpellCooldown(InnerFocus.Name);
            bool shoulBeInterrupted = WTCombat.TargetIsCasting();
            int nbAttackingMe = unitCache.EnemiesAttackingMe.Count;

            // Power Word Shield on multi aggro
            if (!Me.HasAura(PowerWordShield)
                && !hasWeakenedSoul
                && unitCache.EnemiesAttackingMe.Count > 1
                && settings.SSH_UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Power Word Shield
            if (Me.HealthPercent < 50
                && !Me.HasAura(PowerWordShield)
                && !hasWeakenedSoul
                && settings.SSH_UsePowerWordShield
                && cast.OnSelf(PowerWordShield))
                return;

            // Renew
            if (Me.HealthPercent < 70
                && !Me.HasAura(Renew)
                && !inShadowForm
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Renew))
                return;

            // Psychic Scream
            if (Me.HealthPercent < 50
                && unitCache.EnemiesAttackingMe.Count > 1
                && cast.OnSelf(PsychicScream))
                return;

            // Flash Heal
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(FlashHeal))
                return;

            // Heal
            if (Me.HealthPercent < 50
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Heal))
                return;

            // Lesser Heal
            if (Me.HealthPercent < 50
                && !FlashHeal.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(LesserHeal))
                return;

            // Silence
            if (shoulBeInterrupted)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnTarget(Silence))
                    return;
            }

            // Cure Disease
            if (hasDisease && !inShadowForm)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CureDisease))
                    return;
            }

            // Dispel Magic self
            if (hasMagicDebuff
                && myManaPC > 10
                && DispelMagic.KnownSpell
                && DispelMagic.IsSpellUsable
                && (dispelTimer.ElapsedMilliseconds > 10000 || dispelTimer.ElapsedMilliseconds <= 0))
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(DispelMagic))
                    return;
            }

            // Vampiric Touch multi
            if (nbAttackingMe > 1)
            {
                List<IWoWUnit> enemiesWithoutVT = unitCache.EnemiesAttackingMe
                    .Where(e => !e.HasAura(VampiricTouch))
                    .ToList();
                if (enemiesWithoutVT.Count > 0
                    && cast.OnFocusUnit(VampiricTouch, enemiesWithoutVT[0]))
                    return;
            }

            // Vampiric Touch
            if (!Target.HasAura(VampiricTouch)
                && myManaPC > innerManaSaveThreshold
                && Target.HealthPercent > settings.SSH_WandThreshold
                && cast.OnTarget(VampiricTouch))
                return;
            
            // Vampiric Embrace
            if (!Target.HasAura(VampiricEmbrace) 
                && !Me.HasAura(VampiricEmbrace) // wotlk
                && myManaPC > innerManaSaveThreshold
                && cast.OnTarget(VampiricEmbrace))
                return;

            // ShadowFiend
            if (nbAttackingMe > 1
                && cast.OnTarget(Shadowfiend))
                return;

            // Shadow Word Pain multi
            if (nbAttackingMe > 1)
            {
                List<IWoWUnit> enemiesWithoutPain = unitCache.EnemiesAttackingMe
                .Where(e => !e.HasAura(ShadowWordPain))
                .ToList();
                if (enemiesWithoutPain.Count > 0
                    && cast.OnFocusUnit(ShadowWordPain, enemiesWithoutPain[0]))
                    return;
            }
            
            // Shadow Word Pain
            if (myManaPC > 10
                && Target.HealthPercent > 15
                && !Target.HasAura(ShadowWordPain)
                && cast.OnTarget(ShadowWordPain))
                return;
            
            // Inner Fire
            if (!Me.HasAura(InnerFire)
                && settings.SSH_UseInnerFire
                && InnerFire.KnownSpell
                && myManaPC > innerManaSaveThreshold
                && Target.HealthPercent > settings.SSH_WandThreshold
                && cast.OnSelf(InnerFire))
                return;

            // Shadowguard
            if (!Me.HasAura(Shadowguard)
                && myManaPC > innerManaSaveThreshold
                && settings.SSH_UseShadowGuard
                && Target.HealthPercent > settings.SSH_WandThreshold
                && cast.OnSelf(Shadowguard))
                return;

            // Shadow Protection
            if (!Me.HasAura(ShadowProtection)
                && myManaPC > 70
                && settings.SSH_UseShadowProtection
                && cast.OnSelf(ShadowProtection))
                return;

            // Devouring Plague
            if (!Target.HasAura(DevouringPlague)
                && Target.HealthPercent > settings.SSH_DevouringPlagueThreshold
                && cast.OnTarget(DevouringPlague))
                return;
            
            // Shadow Word Death
            if (myManaPC > innerManaSaveThreshold
                && settings.SSH_UseShadowWordDeath
                && Target.HealthPercent < 15
                && cast.OnTarget(ShadowWordDeath))
                return;

            // Mind Blast + Inner Focus
            if (!inShadowForm
                && myManaPC > innerManaSaveThreshold
                && Target.HealthPercent > 50
                && mindBlastCD <= 0
                && (Target.HealthPercent > settings.SSH_WandThreshold || !iCanUseWand))
            {
                if (InnerFocus.KnownSpell && innerFocusCD <= 0)
                    cast.OnSelf(InnerFocus);

                if (cast.OnTarget(MindBlast))
                    return;
            }

            // Shadow Form Mind Blast + Inner Focus
            if (inShadowForm
                && myManaPC > innerManaSaveThreshold
                && mindBlastCD <= 0
                && Target.HealthPercent > settings.SSH_WandThreshold)
            {
                if (InnerFocus.KnownSpell && innerFocusCD <= 0)
                    cast.OnSelf(InnerFocus);

                if (cast.OnTarget(MindBlast))
                    return;
            }

            // Mind FLay
            if ((Me.HasAura(PowerWordShield) || !settings.SSH_UsePowerWordShield)
                && myManaPC > innerManaSaveThreshold
                && Target.HealthPercent > settings.SSH_WandThreshold
                && cast.OnTarget(MindFlay))
                return;

            // Low level Smite
            if (Me.Level < 5 && (Target.HealthPercent > 30 || Me.ManaPercentage > 80)
                && myManaPC > innerManaSaveThreshold
                && cast.OnTarget(Smite))
                return;

            // Smite
            if (!inShadowForm
                && myManaPC > innerManaSaveThreshold
                && Me.Level >= 5
                && Target.HealthPercent > 20
                && (Target.HealthPercent > settings.SSH_WandThreshold || !iCanUseWand)
                && cast.OnTarget(Smite))
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

            // Go in melee because nothing else to do
            if (!WTCombat.IsSpellRepeating(5019)
                && !iCanUseWand
                && !RangeManager.CurrentRangeIsMelee()
                && Target.IsAlive)
            {
                Logger.Log("Going in melee");
                RangeManager.SetRangeToMelee();
                return;
            }
        }
    }
}
