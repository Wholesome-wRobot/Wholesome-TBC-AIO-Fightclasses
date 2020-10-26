using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Arcane : Mage
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Mage Armor
            if (!Me.HaveBuff("Mage Armor")
                && settings.ACMageArmor)
                if (Cast(MageArmor)) 
                    return;

            // Ice Armor
                if (!Me.HaveBuff("Ice Armor")
                && (!settings.ACMageArmor || !MageArmor.KnownSpell))
                if (Cast(IceArmor))
                    return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && !IceArmor.KnownSpell
                && (!settings.ACMageArmor || !MageArmor.KnownSpell))
                if (Cast(FrostArmor))
                    return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Slow
            if (settings.ACSlow
                && !_target.HaveBuff("Slow")
                && Slow.IsDistanceGood)
                if (Cast(Slow))
                    return;

            // Arcane Blast
            if (_target.GetDistance < _distanceRange)
                if (Cast(ArcaneBlast))
                    return;

            // Arcane Missiles
            if (_target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand))
                if (CastStopMove(ArcaneMissiles))
                    return;

            // Frost Bolt
            if (_target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand))
                if (Cast(Frostbolt))
                    return;

            // Low level Frost Bolt
            if (_target.GetDistance < _distanceRange
                && _target.HealthPercent > 30
                && Me.Level < 6)
                if (Cast(Frostbolt))
                    return;

            // Low level FireBall
            if (_target.GetDistance < _distanceRange
                && !Frostbolt.KnownSpell
                && _target.HealthPercent > 30)
                if (Cast(Fireball))
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

            // Cast presence of mind spell
            if (Me.HaveBuff("Presence of Mind"))
                if (Cast(ArcaneBlast) || Cast(Fireball))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }

            // Presence of Mind
            if (!Me.HaveBuff("Presence of Mind")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || !settings.PoMOnMulti)
                && Target.HealthPercent > 50)
                if (Cast(PresenceOfMind))
                    return;

            // Arcane Power
            if (!Me.HaveBuff("Arcane Power")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || !settings.ArcanePowerOnMulti)
                && Target.HealthPercent > 50)
                if (Cast(ArcanePower))
                    return;

            // Slow
            if ((settings.ACSlow || Target.CreatureTypeTarget == "Humanoid")
                && !Target.HaveBuff("Slow")
                && Slow.IsDistanceGood)
                if (Cast(Slow))
                    return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && _polymorphedEnemy == null)
                if (Cast(ConeOfCold))
                    return;

            // Fire Blast
            if (Target.GetDistance < 20f
                && Target.HealthPercent <= settings.FireblastThreshold
                && _polymorphedEnemy == null)
                if (Cast(FireBlast))
                    return;

            bool _shouldCastArcaneBlast =
                ArcaneBlast.KnownSpell
                && (Me.ManaPercentage > 70
                || Me.HaveBuff("Clearcasting")
                || (Me.ManaPercentage > 50 && ToolBox.CountDebuff("Arcane Blast") < 3)
                || (Me.ManaPercentage > 35 && ToolBox.CountDebuff("Arcane Blast") < 2));

            // Arcane Blast
            if (_shouldCastArcaneBlast
                && Target.GetDistance < _distanceRange
                && (Target.HealthPercent > settings.WandThreshold || !_iCanUseWand))
                if (Cast(ArcaneBlast))
                    return;

            // Arcane Missiles
            if (Target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand))
                if (Cast(ArcaneMissiles, true))
                    return;

            // Frost Bolt
            if (Target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && _polymorphedEnemy == null)
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
