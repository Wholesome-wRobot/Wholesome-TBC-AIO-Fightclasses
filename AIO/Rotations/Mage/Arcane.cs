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

            // Mage Armor
            if (!Me.HaveBuff("Mage Armor")
                && settings.ACMageArmor
                && cast.OnSelf(MageArmor)) 
                return;

            // Ice Armor
                if (!Me.HaveBuff("Ice Armor")
                && (!settings.ACMageArmor || !MageArmor.KnownSpell)
                && cast.OnSelf(IceArmor))
                return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && !IceArmor.KnownSpell
                && (!settings.ACMageArmor || !MageArmor.KnownSpell)
                && cast.OnSelf(FrostArmor))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            bool _shouldCastArcaneBlast =
                ArcaneBlast.KnownSpell
                && (Me.ManaPercentage > 70
                || Me.HaveBuff("Clearcasting")
                || (Me.ManaPercentage > 50 && ToolBox.CountDebuff("Arcane Blast") < 3)
                || (Me.ManaPercentage > 35 && ToolBox.CountDebuff("Arcane Blast") < 2)
                || (ToolBox.CountDebuff("Arcane Blast") < 1));

            // Slow
            if (settings.ACSlow
                && !_target.HaveBuff("Slow")
                && Slow.IsDistanceGood
                && cast.OnTarget(Slow))
                return;

            // Arcane Blast
            if (cast.OnTarget(ArcaneBlast))
                return;

            // Arcane Missiles
            if (Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand)
                && cast.OnTarget(ArcaneMissiles))
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
                && _target.HealthPercent > 30)
                if (cast.OnTarget(Fireball))
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

            // Mana Shield
            if (!Me.HaveBuff("Mana Shield")
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10)
                && cast.OnSelf(ManaShield))
                return;

            // Use Mana Stone
            if ((ObjectManager.GetNumberAttackPlayer() > 1 && Me.ManaPercentage < 50 || Me.ManaPercentage < 5)
                && _foodManager.UseManaStone())
                return;

            // Cast presence of mind spell
            if (Me.HaveBuff("Presence of Mind"))
                if (cast.OnTarget(ArcaneBlast) || cast.OnTarget(Fireball))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }

            // Presence of Mind
            if (!Me.HaveBuff("Presence of Mind")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || !settings.PoMOnMulti)
                && Target.HealthPercent > 50
                && cast.OnSelf(PresenceOfMind))
                return;

            // Arcane Power
            if (!Me.HaveBuff("Arcane Power")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || !settings.ArcanePowerOnMulti)
                && Target.HealthPercent > 50
                && cast.OnSelf(ArcanePower))
                return;

            // Slow
            if ((settings.ACSlow || Target.CreatureTypeTarget == "Humanoid")
                && !Target.HaveBuff("Slow")
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

            bool _shouldCastArcaneBlast =
                ArcaneBlast.KnownSpell
                && (Me.ManaPercentage > 70
                || Me.HaveBuff("Clearcasting")
                || (Me.ManaPercentage > 50 && ToolBox.CountDebuff("Arcane Blast") < 3)
                || (Me.ManaPercentage > 35 && ToolBox.CountDebuff("Arcane Blast") < 2));

            // Arcane Blast
            if (_shouldCastArcaneBlast
                && (Target.HealthPercent > settings.WandThreshold || !_iCanUseWand)
                && cast.OnTarget(ArcaneBlast))
                return;

            // Arcane Missiles
            if (Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && cast.OnTarget(ArcaneMissiles))
                return;

            // Frost Bolt
            if (Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
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
            if (ToolBox.UsingWand()
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(ArcaneBlast) || cast.OnTarget(ArcaneMissiles) || cast.OnTarget(Frostbolt) || cast.OnTarget(Fireball))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && !cast.IsBackingUp
                && !MovementManager.InMovement
                && cast.OnTarget(UseWand, false))
                return;

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
