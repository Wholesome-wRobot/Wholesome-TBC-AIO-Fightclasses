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
                && cast.Normal(Evocation))
                return;

            // Arcane Intellect
            if (!Me.HaveBuff("Arcane Intellect")
                && ArcaneIntellect.KnownSpell
                && ArcaneIntellect.IsSpellUsable
                && cast.OnSelf(ArcaneIntellect))
                return;

            // Mage Armor
            if (!Me.HaveBuff("Mage Armor")
                && settings.ACMageArmor)
                if (cast.Normal(MageArmor)) 
                    return;

            // Ice Armor
                if (!Me.HaveBuff("Ice Armor")
                && (!settings.ACMageArmor || !MageArmor.KnownSpell))
                if (cast.Normal(IceArmor))
                    return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && !IceArmor.KnownSpell
                && (!settings.ACMageArmor || !MageArmor.KnownSpell))
                if (cast.Normal(FrostArmor))
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
                if (cast.Normal(Slow))
                    return;

            // Arcane Blast
            if (_target.GetDistance < _distanceRange)
                if (cast.Normal(ArcaneBlast))
                    return;

            // Arcane Missiles
            if (_target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (_target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 30 || !_iCanUseWand))
                if (cast.Normal(ArcaneMissiles))
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

            // Mana Shield
            if (!Me.HaveBuff("Mana Shield")
                && (Me.HealthPercent < 30 && Me.ManaPercentage > 50
                || Me.HealthPercent < 10))
                if (cast.Normal(ManaShield))
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
                if (cast.Normal(ArcaneBlast) || cast.Normal(Fireball))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }

            // Presence of Mind
            if (!Me.HaveBuff("Presence of Mind")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || !settings.PoMOnMulti)
                && Target.HealthPercent > 50)
                if (cast.Normal(PresenceOfMind))
                    return;

            // Arcane Power
            if (!Me.HaveBuff("Arcane Power")
                && (ObjectManager.GetNumberAttackPlayer() > 1 || !settings.ArcanePowerOnMulti)
                && Target.HealthPercent > 50)
                if (cast.Normal(ArcanePower))
                    return;

            // Slow
            if ((settings.ACSlow || Target.CreatureTypeTarget == "Humanoid")
                && !Target.HaveBuff("Slow")
                && Slow.IsDistanceGood)
                if (cast.Normal(Slow))
                    return;

            // Cone of Cold
            if (Target.GetDistance < 10
                && settings.UseConeOfCold
                && _polymorphedEnemy == null)
                if (cast.Normal(ConeOfCold))
                    return;

            // Fire Blast
            if (Target.GetDistance < 20f
                && Target.HealthPercent <= settings.FireblastThreshold
                && _polymorphedEnemy == null)
                if (cast.Normal(FireBlast))
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
                if (cast.Normal(ArcaneBlast))
                    return;

            // Arcane Missiles
            if (Target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand))
                if (cast.Normal(ArcaneMissiles, true))
                    return;

            // Frost Bolt
            if (Target.GetDistance < _distanceRange
                && Me.Level >= 6
                && (Target.HealthPercent > settings.WandThreshold || ObjectManager.GetNumberAttackPlayer() > 1 || Me.HealthPercent < 40 || !_iCanUseWand)
                && _polymorphedEnemy == null)
                if (cast.Normal(Frostbolt, true))
                    return;

            // Low level Frost Bolt
            if (Target.GetDistance < _distanceRange
                && (Target.HealthPercent > 15 || Me.HealthPercent < 50)
                && Me.Level < 6)
                if (cast.Normal(Frostbolt, true))
                    return;

            // Low level FireBall
            if (Target.GetDistance < _distanceRange
                && !Frostbolt.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 50))
                if (cast.Normal(Fireball, true))
                    return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && cast.BannedSpells.Contains("Shoot"))
                if (cast.Normal(UseWand))
                    return;

            // Spell if wand banned
            if (cast.BannedSpells.Contains("Shoot")
                && Target.GetDistance < _distanceRange)
                if (cast.Normal(ArcaneBlast) || cast.Normal(ArcaneMissiles) || cast.Normal(Frostbolt) || cast.Normal(Fireball))
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
