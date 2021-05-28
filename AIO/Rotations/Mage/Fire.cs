using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Fire : Mage
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Evocation
            if (Me.ManaPercentage < 30
                && cast.OnSelf(Evocation))
                return;

            // Arcane Intellect
            if (!Me.HaveBuff("Arcane Intellect")
                && ArcaneIntellect.KnownSpell
                && ArcaneIntellect.IsSpellUsable
                && cast.OnSelf(ArcaneIntellect))
                return;

            // Ice Armor
            if (!Me.HaveBuff("Ice Armor")
                && cast.OnSelf(IceArmor))
                return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && !IceArmor.KnownSpell
                && cast.OnSelf(FrostArmor))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Combustion
            if (!Me.HaveBuff("Combustion")
                && cast.OnSelf(Combustion))
                return;

            // Fireball
            if (_target.GetDistance < 33f
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand)
                && cast.OnTarget(Fireball))
                return;
        }

        protected override void CombatRotation()
        {
            // Reactivate auto attack (after dragon's breath)
            if (!ToolBox.UsingWand())
                ToolBox.CheckAutoAttack(Attack);

            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;

            // Stop wand use on multipull
            if (_iCanUseWand && ObjectManager.GetNumberAttackPlayer() > 1)
                _iCanUseWand = false;

            // Remove Curse
            if (ToolBox.HasCurseDebuff())
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(RemoveCurse))
                    return;
            }

            // Mana Shield
            if (!Me.HaveBuff("Mana Shield")
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10)
                && cast.OnTarget(ManaShield))
                return;

            // Use Mana Stone
            if ((ObjectManager.GetNumberAttackPlayer() > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && _foodManager.UseManaStone())
                return;

            // Combustion
            if (!Me.HaveBuff("Combustion")
                && cast.OnSelf(Combustion))
                return;

            // Blast Wave
            if (settings.BlastWaveOnMulti
                && ToolBox.GetNbEnemiesClose(10) > 1
                && ObjectManager.GetNumberAttackPlayer() > 1
                && cast.OnSelf(BlastWave))
                return;

            // Dragon's Breath
            if (Target.GetDistance <= 10f
                && settings.UseDragonsBreath
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && _polymorphedEnemy == null
                && cast.OnSelf(DragonsBreath))
                return;

            // Fire Blast
            if (Target.HealthPercent <= settings.FireblastThreshold
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && !Target.HaveBuff("Polymorph")
                && cast.OnTarget(FireBlast))
                return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && _polymorphedEnemy == null
                && cast.OnTarget(ConeOfCold))
                return;

            // FireBall
            if ((Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && !Target.HaveBuff("Polymorph")
                && cast.OnTarget(Fireball))
                return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(UseWand))
                    return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(Fireball) || cast.OnTarget(Frostbolt) || cast.OnTarget(ArcaneMissiles))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && !cast.IsBackingUp
                && !MovementManager.InMovement)
            {
                if (cast.OnTarget(UseWand, false))
                    return;
            }

            // Go in melee because nothing else to do
            if (!ToolBox.UsingWand()
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
