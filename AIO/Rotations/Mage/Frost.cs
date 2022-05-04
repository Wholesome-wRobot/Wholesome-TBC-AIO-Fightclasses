using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Frost : Mage
    {
        public Frost(BaseSettings settings) : base(settings)
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
                && cast.OnSelf(ArcaneIntellect))
                return;

            // Ice Armor
            if (!Me.HasAura(IceArmor))
                if (cast.OnSelf(IceArmor))
                    return;

            // Frost Armor
            if (!Me.HasAura(FrostArmor)
                && !IceArmor.KnownSpell)
                if (cast.OnSelf(FrostArmor))
                    return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable
                && !Me.HasAura(IceBarrier)
                && cast.OnSelf(IceBarrier))
                return;

            // Frost Bolt
            if (Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 30 || !iCanUseWand)
                && cast.OnTarget(Frostbolt))
                return;

            // Low level Frost Bolt
            if (_target.HealthPercent > 30
                && Me.Level < 6
                && cast.OnTarget(Frostbolt))
                return;

            // Low level FireBall
            if (!Frostbolt.KnownSpell
                && _target.HealthPercent > 30
                && cast.OnTarget(Fireball))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            // Stop wand use on multipull
            if (iCanUseWand && unitCache.EnemiesAttackingMe.Count > 1 && Me.ManaPercentage > 10)
                iCanUseWand = false;

            // Remove Curse
            if (WTEffects.HasCurseDebuff())
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(RemoveCurse))
                    return;
            }

            // Summon Water Elemental
            if (Target.HealthPercent > 95
                || unitCache.EnemiesAttackingMe.Count > 1)
                if (cast.OnSelf(SummonWaterElemental))
                    return;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable
                && !Me.HasAura(IceBarrier)
                && cast.OnSelf(IceBarrier))
                return;

            // Mana Shield
            if (!Me.HasAura(ManaShield)
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50 || Me.HealthPercent < 10)
                && cast.OnSelf(ManaShield))
                return;

            // Cold Snap
            if (unitCache.EnemiesAttackingMe.Count > 1
                && !Me.HasAura(IcyVeins)
                && !IcyVeins.IsSpellUsable
                && cast.OnSelf(ColdSnap))
                return;

            // Icy Veins
            if (unitCache.EnemiesAttackingMe.Count > 1 && settings.IcyVeinMultiPull
                || !settings.IcyVeinMultiPull
                && cast.OnSelf(IcyVeins))
                return;

            // Use Mana Stone
            if ((unitCache.EnemiesAttackingMe.Count > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && foodManager.UseManaStone())
                return;

            bool targetHasFrostBite = Target.HasBuff("Frostbite");
            // Ice Lance
            if ((targetHasFrostBite || Target.HasAura(FrostNova))
                && cast.OnTarget(IceLance))
                return;

            // Frost Nova
            if (Target.GetDistance < 6f
                && Target.HealthPercent > 10
                && !targetHasFrostBite
                && _polymorphedEnemy == null
                && cast.OnSelf(FrostNova))
                return;

            // Fire Blast
            if (Target.HealthPercent <= settings.FireblastThreshold
                && !targetHasFrostBite
                && !Target.HasAura(FrostNova)
                && cast.OnTarget(FireBlast))
                return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && Me.IsFacing(Target.PositionWithoutType, 0.5f)
                && _polymorphedEnemy == null
                && cast.OnSelf(ConeOfCold))
                return;

            // Frost Bolt
            if (Me.Level >= 6
                && !cast.IsBackingUp
                && (Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 40 || !iCanUseWand)
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
                if (cast.OnTarget(Frostbolt) || cast.OnTarget(Fireball) || cast.OnTarget(ArcaneMissiles))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && iCanUseWand
                && !cast.IsBackingUp
                && !MovementManager.InMovement)
            {
                if (cast.OnTarget(UseWand, false))
                    return;
            }

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
