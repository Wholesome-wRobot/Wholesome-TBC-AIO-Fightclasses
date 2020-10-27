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

            // Ice Armor
            if (!Me.HaveBuff("Ice Armor"))
                if (Cast(IceArmor))
                    return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor") 
                && !IceArmor.KnownSpell)
                if (Cast(FrostArmor))
                    return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable && !Me.HaveBuff("Ice Barrier"))
                if (Cast(IceBarrier))
                    return;

            // Frost Bolt
            if (_target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand))
                if (CastStopMove(Frostbolt))
                    return;

            // Low level Frost Bolt
            if (_target.GetDistance < _distanceRange
                && _target.HealthPercent > 30
                && Me.Level < 6)
                if (CastStopMove(Frostbolt))
                    return;

            // Low level FireBall
            if (_target.GetDistance < _distanceRange
                && !Frostbolt.KnownSpell
                && _target.HealthPercent > 30)
                if (CastStopMove(Fireball))
                    return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            Lua.LuaDoString("PetAttack();", false);
            WoWUnit Target = ObjectManager.Target;

            // Stop wand use on multipull
            if (_iCanUseWand && ObjectManager.GetNumberAttackPlayer() > 1)
                _iCanUseWand = false;

            // Remove Curse
            if (ToolBox.HasCurseDebuff())
            {
                Thread.Sleep(Main.humanReflexTime);
                if (Cast(RemoveCurse))
                    return;
            }

            // Summon Water Elemental
            if (Target.HealthPercent > 95
                || ObjectManager.GetNumberAttackPlayer() > 1)
                if (Cast(SummonWaterElemental))
                    return;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable
                && !Me.HaveBuff("Ice Barrier"))
                if (Cast(IceBarrier))
                    return;

            // Mana Shield
            if (!Me.HaveBuff("Mana Shield")
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10))
                if (Cast(ManaShield))
                    return;

            // Cold Snap
            if (ObjectManager.GetNumberAttackPlayer() > 1
                && !Me.HaveBuff("Icy Veins")
                && !IcyVeins.IsSpellUsable)
                if (Cast(ColdSnap))
                    return;

            // Icy Veins
            if (ObjectManager.GetNumberAttackPlayer() > 1 && settings.IcyVeinMultiPull
                || !settings.IcyVeinMultiPull)
                if (Cast(IcyVeins))
                    return;

            // Use Mana Stone
            if ((ObjectManager.GetNumberAttackPlayer() > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && _foodManager.ManaStone != "")
            {
                _foodManager.UseManaStone();
                _foodManager.ManaStone = "";
            }

            // Ice Lance
            if (Target.HaveBuff("Frostbite")
                || Target.HaveBuff("Frost Nova"))
                if (Cast(IceLance))
                    return;

            // Frost Nova
            if (Target.GetDistance < 6f
                && Target.HealthPercent > 10
                && !Target.HaveBuff("Frostbite")
                && _polymorphedEnemy == null)
                if (Cast(FrostNova))
                    return;

            // Fire Blast
            if (Target.GetDistance < 20f
                && Target.HealthPercent <= settings.FireblastThreshold
                && !Target.HaveBuff("Frostbite") 
                && !Target.HaveBuff("Frost Nova"))
                if (Cast(FireBlast))
                    return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && !_isBackingUp
                && !MovementManager.InMovement
                && _polymorphedEnemy == null)
                if (Cast(ConeOfCold))
                    return;

            // Frost Bolt
            if (Target.GetDistance < _distanceRange
                && Me.Level >= 6
                && !_isBackingUp
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand))
                if (Cast(Frostbolt, true))
                    return;

            // Low level Frost Bolt
            if (Target.GetDistance < _distanceRange
                && (Target.HealthPercent > 15 || Me.HealthPercent < 50)
                && Me.Level < 6)
                if (Cast(Frostbolt, true))
                    return;

            // Low level FireBall
            if (Target.GetDistance < _distanceRange
                && !Frostbolt.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 50))
                if (Cast(Fireball, true))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && ObjectManager.Target.GetDistance <= _distanceRange
                && !_isBackingUp
                && !MovementManager.InMovement)
            {
                RangeManager.SetRange(_distanceRange);
                if (Cast(UseWand, false))
                    return;
            }

            // Go in melee because nothing else to do
            if (!ToolBox.UsingWand()
                && !UseWand.IsSpellUsable
                && !RangeManager.CurrentRangeIsMelee()
                && !_isBackingUp
                && Target.IsAlive)
            {
                Logger.Log("Going in melee");
                RangeManager.SetRangeToMelee();
                return;
            }
        }
    }
}
