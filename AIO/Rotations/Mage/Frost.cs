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
                if (cast.Normal(IceArmor))
                    return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor") 
                && !IceArmor.KnownSpell)
                if (cast.Normal(FrostArmor))
                    return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable && !Me.HaveBuff("Ice Barrier"))
                if (cast.Normal(IceBarrier))
                    return;

            // Frost Bolt
            if (_target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand))
                if (cast.Normal(Frostbolt))
                    return;

            // Low level Frost Bolt
            if (_target.GetDistance < _distanceRange
                && _target.HealthPercent > 30
                && Me.Level < 6)
                if (cast.Normal(Frostbolt))
                    return;

            // Low level FireBall
            if (_target.GetDistance < _distanceRange
                && !Frostbolt.KnownSpell
                && _target.HealthPercent > 30)
                if (cast.Normal(Fireball))
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
                if (cast.OnSelf(RemoveCurse))
                    return;
            }

            // Summon Water Elemental
            if (Target.HealthPercent > 95
                || ObjectManager.GetNumberAttackPlayer() > 1)
                if (cast.Normal(SummonWaterElemental))
                    return;

            // Ice Barrier
            if (IceBarrier.IsSpellUsable
                && !Me.HaveBuff("Ice Barrier"))
                if (cast.Normal(IceBarrier))
                    return;

            // Mana Shield
            if (!Me.HaveBuff("Mana Shield")
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10))
                if (cast.Normal(ManaShield))
                    return;

            // Cold Snap
            if (ObjectManager.GetNumberAttackPlayer() > 1
                && !Me.HaveBuff("Icy Veins")
                && !IcyVeins.IsSpellUsable)
                if (cast.Normal(ColdSnap))
                    return;

            // Icy Veins
            if (ObjectManager.GetNumberAttackPlayer() > 1 && settings.IcyVeinMultiPull
                || !settings.IcyVeinMultiPull)
                if (cast.Normal(IcyVeins))
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
                if (cast.Normal(IceLance))
                    return;

            // Frost Nova
            if (Target.GetDistance < 6f
                && Target.HealthPercent > 10
                && !Target.HaveBuff("Frostbite")
                && _polymorphedEnemy == null)
                if (cast.Normal(FrostNova))
                    return;

            // Fire Blast
            if (Target.GetDistance < 20f
                && Target.HealthPercent <= settings.FireblastThreshold
                && !Target.HaveBuff("Frostbite") 
                && !Target.HaveBuff("Frost Nova"))
                if (cast.Normal(FireBlast))
                    return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && !cast.IsBackingUp
                && !MovementManager.InMovement
                && _polymorphedEnemy == null)
                if (cast.Normal(ConeOfCold))
                    return;

            // Frost Bolt
            if (Target.GetDistance < _distanceRange
                && Me.Level >= 6
                && !cast.IsBackingUp
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand))
                if (cast.Normal(Frostbolt))
                    return;

            // Low level Frost Bolt
            if (Target.GetDistance < _distanceRange
                && (Target.HealthPercent > 15 || Me.HealthPercent < 50)
                && Me.Level < 6)
                if (cast.Normal(Frostbolt))
                    return;

            // Low level FireBall
            if (Target.GetDistance < _distanceRange
                && !Frostbolt.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 50))
                if (cast.Normal(Fireball))
                    return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && cast.BannedSpells.Contains("Shoot"))
                if (cast.Normal(UseWand))
                    return;

            // Spell if wand banned
            if (cast.BannedSpells.Contains("Shoot")
                && Target.GetDistance < _distanceRange)
                if (cast.Normal(Frostbolt) || cast.Normal(Fireball) || cast.Normal(ArcaneMissiles))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && ObjectManager.Target.GetDistance <= _distanceRange
                && !cast.IsBackingUp
                && !MovementManager.InMovement)
            {
                RangeManager.SetRange(_distanceRange);
                if (cast.Normal(UseWand, false))
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
