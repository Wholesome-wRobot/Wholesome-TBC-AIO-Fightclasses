using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Arcane : Mage
    {
        public Arcane(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Evocation
            if (Me.ManaPercentage < 30
                && cast.OnSelf(Evocation))
                return;

            // Arcane Intellect
            if (!Me.HasAura(ArcaneIntellect)
                && ArcaneIntellect.KnownSpell
                && ArcaneIntellect.IsSpellUsable
                && cast.OnSelf(ArcaneIntellect))
                return;

            // Mage Armor
            if (!Me.HasAura(MageArmor)
                && settings.ACMageArmor
                && cast.OnSelf(MageArmor))
                return;

            // Ice Armor
            if (!Me.HasAura(IceArmor)
            && (!settings.ACMageArmor || !MageArmor.KnownSpell)
            && cast.OnSelf(IceArmor))
                return;

            // Frost Armor
            if (!Me.HasAura(FrostArmor)
                && !IceArmor.KnownSpell
                && (!settings.ACMageArmor || !MageArmor.KnownSpell)
                && cast.OnSelf(FrostArmor))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            // Slow
            if (settings.ACSlow
                && !Target.HasAura(Slow)
                && Slow.IsDistanceGood
                && cast.OnTarget(Slow))
                return;

            // Arcane Blast
            if (cast.OnTarget(ArcaneBlast))
                return;

            // Arcane Missiles
            if (Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 30 || !iCanUseWand)
                && cast.OnTarget(ArcaneMissiles))
                return;

            // Frost Bolt
            if (Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 30 || !iCanUseWand)
                && cast.OnTarget(Frostbolt))
                return;

            // Low level Frost Bolt
            if (Target.HealthPercent > 30
                && Me.Level < 6
                && cast.OnTarget(Frostbolt))
                return;

            // Low level FireBall
            if (!Frostbolt.KnownSpell
                && Target.HealthPercent > 30)
                if (cast.OnTarget(Fireball))
                    return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            Lua.LuaDoString("PetAttack();");
            int presenceOfMindCD = WTCombat.GetSpellCooldown(PresenceOfMind.Name);

            // Stop wand use on multipull
            if (iCanUseWand && unitCache.EnemiesAttackingMe.Count > 1)
                iCanUseWand = false;

            // Remove Curse
            if (WTEffects.HasCurseDebuff())
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(RemoveCurse))
                    return;
            }

            // Mana Shield
            if (!Me.HasAura(ManaShield)
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10)
                && cast.OnSelf(ManaShield))
                return;

            // Use Mana Stone
            if ((unitCache.EnemiesAttackingMe.Count > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && foodManager.UseManaStone())
                return;

            // Cast presence of mind spell
            if (Me.HasAura(PresenceOfMind))
                if (cast.OnTarget(ArcaneBlast) || cast.OnTarget(Fireball))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }

            // Presence of Mind
            if (presenceOfMindCD <= 0
                && !Me.HasAura(PresenceOfMind)
                && (unitCache.EnemiesAttackingMe.Count > 1 || !settings.PoMOnMulti)
                && Target.HealthPercent > 50
                && cast.OnSelf(PresenceOfMind))
                return;

            // Arcane Power
            if (!Me.HasAura(ArcanePower)
                && (unitCache.EnemiesAttackingMe.Count > 1 || !settings.ArcanePowerOnMulti)
                && Target.HealthPercent > 50
                && cast.OnSelf(ArcanePower))
                return;

            // Slow
            if ((settings.ACSlow || Target.CreatureTypeTarget == "Humanoid")
                && !Target.HasAura(Slow)
                && cast.OnTarget(Slow))
                return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && _polymorphedEnemy == null
                && cast.OnTarget(ConeOfCold))
                return;

            // Fire Blast
            if (Target.HealthPercent <= settings.FireblastThreshold
                && _polymorphedEnemy == null
                && cast.OnTarget(FireBlast))
                return;

            int nbArcaneBlastDebuffs = ArcaneBlast.KnownSpell ? WTEffects.CountDebuff("Arcane Blast") : -1;
            bool _shouldCastArcaneBlast =
                nbArcaneBlastDebuffs > -1
                && (Me.ManaPercentage > 70
                || Me.HasBuff("Clearcasting")
                || (Me.ManaPercentage > 50 && nbArcaneBlastDebuffs < 3)
                || (Me.ManaPercentage > 35 && nbArcaneBlastDebuffs < 2));

            // Arcane Blast
            if (_shouldCastArcaneBlast
                && (Target.HealthPercent > settings.WandThreshold || !iCanUseWand)
                && cast.OnTarget(ArcaneBlast))
                return;

            // Arcane Missiles
            if (Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 40 || !iCanUseWand)
                && cast.OnTarget(ArcaneMissiles))
                return;

            // Frost Bolt
            if (Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 40 || !iCanUseWand)
                && _polymorphedEnemy == null
                && cast.OnTarget(Frostbolt))
                return;

            // Low level Frost Bolt
            if ((Target.HealthPercent > 15 || Me.HealthPercent < 50)
                && Me.Level < 6
                && cast.OnTarget(Frostbolt))
                return;

            // Low level FireBall
            if (!Frostbolt.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 50)
                && cast.OnTarget(Fireball))
                return;

            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(Target, "Shoot"))
                if (cast.OnTarget(ArcaneBlast) || cast.OnTarget(ArcaneMissiles) || cast.OnTarget(Frostbolt) || cast.OnTarget(Fireball))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && iCanUseWand
                && !cast.IsBackingUp
                && !MovementManager.InMovement
                && cast.OnTarget(UseWand, false))
                return;

            // Go in melee because nothing else to do
            if (!WTCombat.IsSpellRepeating(5019)
                && !UseWand.IsSpellUsable
                && !RangeManager.CurrentRangeIsMelee()
                && !cast.IsBackingUp
                && Target.IsAlive)
            {
                Logger.Log("Going in melee");
                RangeManager.SetRangeToMelee();
                return;
            }
        }
    }
}
