using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Fire : Mage
    {
        public Fire(BaseSettings settings) : base(settings)
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

            // Ice Armor
            if (!Me.HasAura(IceArmor)
                && cast.OnSelf(IceArmor))
                return;

            // Frost Armor
            if (!Me.HasAura(FrostArmor)
                && !IceArmor.KnownSpell
                && cast.OnSelf(FrostArmor))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Combustion
            if (!Me.HasAura(Combustion)
                && cast.OnSelf(Combustion))
                return;

            // Fireball
            if (_target.GetDistance < 33f
                && (_target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 30 || !iCanUseWand)
                && cast.OnTarget(Fireball))
                return;
        }

        protected override void CombatRotation()
        {
            // Reactivate auto attack (after dragon's breath)
            if (!WTCombat.IsSpellRepeating(5019))
                ToolBox.CheckAutoAttack(Attack);

            base.CombatRotation();

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
                && cast.OnTarget(ManaShield))
                return;

            // Use Mana Stone
            if ((unitCache.EnemiesAttackingMe.Count > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && foodManager.UseManaStone())
                return;

            // Combustion
            if (!Me.HasAura(Combustion)
                && cast.OnSelf(Combustion))
                return;

            // Blast Wave
            if (settings.BlastWaveOnMulti
                && ToolBox.GetNbEnemiesClose(10) > 1
                && unitCache.EnemiesAttackingMe.Count > 1
                && cast.OnSelf(BlastWave))
                return;

            // Dragon's Breath
            if (Target.GetDistance <= 10f
                && settings.UseDragonsBreath
                && (Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 40 || !iCanUseWand)
                && _polymorphedEnemy == null
                && cast.OnSelf(DragonsBreath))
                return;

            // Fire Blast
            if (Target.HealthPercent <= settings.FireblastThreshold
                && (Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 40 || !iCanUseWand)
                && !Target.HasAura(Polymorph)
                && cast.OnTarget(FireBlast))
                return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && _polymorphedEnemy == null
                && cast.OnTarget(ConeOfCold))
                return;

            // FireBall
            if ((Target.HealthPercent > settings.WandThreshold || unitCache.EnemiesAttackingMe.Count > 1 || Me.HealthPercent < 40 || !iCanUseWand)
                && !Target.HasAura(Polymorph)
                && cast.OnTarget(Fireball))
                return;

            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(Target, "Shoot"))
                if (cast.OnTarget(UseWand))
                    return;

            // Spell if wand banned
            if (UnitImmunities.Contains(Target, "Shoot"))
                if (cast.OnTarget(Fireball) || cast.OnTarget(Frostbolt) || cast.OnTarget(ArcaneMissiles))
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
