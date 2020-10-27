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

            // Combustion
            if (!Me.HaveBuff("Combustion"))
                if (Cast(Combustion))
                    return;

            // Fireball
            if (_target.GetDistance < 33f
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand))
                if (CastStopMove(Fireball))
                    return;
        }

        protected override void CombatRotation()
        {
            // Reactivate auto attack (after dragon's breath)
            if (!ToolBox.UsingWand())
                ToolBox.CheckAutoAttack(Attack);

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

            // Mana Shield
            if (!Me.HaveBuff("Mana Shield")
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10))
                if (Cast(ManaShield))
                    return;

            // Use Mana Stone
            if ((ObjectManager.GetNumberAttackPlayer() > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && _foodManager.ManaStone != "")
            {
                _foodManager.UseManaStone();
                _foodManager.ManaStone = "";
            }

            // Combustion
            if (!Me.HaveBuff("Combustion"))
                if (Cast(Combustion))
                    return;

            // Blast Wave
            if (settings.BlastWaveOnMulti
                && ToolBox.CheckIfEnemiesClose(10)
                && ObjectManager.GetNumberAttackPlayer() > 1)
                if (Cast(BlastWave))
                    return;

            // Dragon's Breath
            if (Target.GetDistance <= 10f
                && settings.UseDragonsBreath
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && _polymorphedEnemy == null)
                if (Cast(DragonsBreath))
                    return;

            // Fire Blast
            if (Target.GetDistance < 20f
                && Target.HealthPercent <= settings.FireblastThreshold
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && !Target.HaveBuff("Polymorph"))
                if (Cast(FireBlast))
                    return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && _polymorphedEnemy == null)
                if (Cast(ConeOfCold))
                    return;

            // FireBall
            if (Target.GetDistance < 33f
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && !Target.HaveBuff("Polymorph"))
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
