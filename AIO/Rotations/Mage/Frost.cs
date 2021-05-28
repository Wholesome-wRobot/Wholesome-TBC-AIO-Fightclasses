using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Frost : Mage
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
                && cast.OnSelf(ArcaneIntellect))
                return;

            // Ice Armor
            if (!Me.HaveBuff("Ice Armor"))
                if (cast.OnSelf(IceArmor))
                    return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor") 
                && !IceArmor.KnownSpell)
                if (cast.OnSelf(FrostArmor))
                    return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable && !Me.HaveBuff("Ice Barrier")
                && cast.OnSelf(IceBarrier))
                return;

            // Frost Bolt
            if (Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand)
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

            // Summon Water Elemental
            if (Target.HealthPercent > 95
                || ObjectManager.GetNumberAttackPlayer() > 1)
                if (cast.OnSelf(SummonWaterElemental))
                    return;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable
                && !Me.HaveBuff("Ice Barrier")
                && cast.OnSelf(IceBarrier))
                return;

            // Mana Shield
            if (!Me.HaveBuff("Mana Shield")
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10)
                && cast.OnSelf(ManaShield))
                return;

            // Cold Snap
            if (ObjectManager.GetNumberAttackPlayer() > 1
                && !Me.HaveBuff("Icy Veins")
                && !IcyVeins.IsSpellUsable
                && cast.OnSelf(ColdSnap))
                return;

            // Icy Veins
            if (ObjectManager.GetNumberAttackPlayer() > 1 && settings.IcyVeinMultiPull
                || !settings.IcyVeinMultiPull
                && cast.OnSelf(IcyVeins))
                return;

            // Use Mana Stone
            if ((ObjectManager.GetNumberAttackPlayer() > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && _foodManager.UseManaStone())
                return;

            // Ice Lance
            if ((Target.HaveBuff("Frostbite") || Target.HaveBuff("Frost Nova"))
                && cast.OnTarget(IceLance))
                return;

            // Frost Nova
            if (Target.GetDistance < 6f
                && Target.HealthPercent > 10
                && !Target.HaveBuff("Frostbite")
                && _polymorphedEnemy == null
                && cast.OnSelf(FrostNova))
                return;

            // Fire Blast
            if (Target.HealthPercent <= settings.FireblastThreshold
                && !Target.HaveBuff("Frostbite") 
                && !Target.HaveBuff("Frost Nova")
                && cast.OnTarget(FireBlast))
                return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && Me.IsFacing(Target.Position, 0.5f)
                && _polymorphedEnemy == null
                && cast.OnSelf(ConeOfCold))
                return;

            // Frost Bolt
            if (Me.Level >= 6
                && !cast.IsBackingUp
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
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
            if (ToolBox.UsingWand()
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(Frostbolt) || cast.OnTarget(Fireball) || cast.OnTarget(ArcaneMissiles))
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
