using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class ArcaneParty : Mage
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Arcane Intellect
            WoWPlayer noAI = AIOParty.Group
                .Find(m => !m.HaveBuff(ArcaneIntellect.Name));
            if (noAI != null && cast.OnFocusPlayer(ArcaneIntellect, noAI))
                return;

            // Mage Armor
            if (!Me.HaveBuff("Mage Armor")
                && settings.ACMageArmor
                && cast.Normal(MageArmor))
                return;

            // Ice Armor
            if (!Me.HaveBuff("Ice Armor")
                && (!settings.ACMageArmor || !MageArmor.KnownSpell)
                && cast.Normal(IceArmor))
                return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && !IceArmor.KnownSpell
                && (!settings.ACMageArmor || !MageArmor.KnownSpell)
                && cast.Normal(FrostArmor))
                return;

            // PARTY Drink
            ToolBox.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold);
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Slow
            if (settings.ACSlow
                && !_target.HaveBuff("Slow")
                && Slow.IsDistanceGood
                && cast.Normal(Slow))
                return;

            // Arcane Blast
            if (_target.GetDistance < _distanceRange
                && cast.Normal(ArcaneBlast))
                return;

            // Frost Bolt
            if (_target.GetDistance < _distanceRange
                && cast.Normal(Frostbolt))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            Lua.LuaDoString("PetAttack();", false);
            WoWUnit Target = ObjectManager.Target;
            List<WoWUnit> surroundingEnemies = ObjectManager.GetObjectWoWUnit()
                .Where(e => e.IsAlive && e.IsValid && !e.PlayerControlled && e.IsAttackable && e.InCombatFlagOnly)
                .ToList();

            // PARTY Remove Curse
            if (settings.PartyRemoveCurse)
            {
                List<WoWPlayer> needRemoveCurse = AIOParty.Group
                    .FindAll(m => ToolBox.HasCurseDebuff(m.Name))
                    .ToList();
                if (needRemoveCurse.Count > 0 && cast.OnFocusPlayer(RemoveCurse, needRemoveCurse[0]))
                    return;
            }

            // Use Mana Stone
            if (Me.ManaPercentage < 20
                && _foodManager.ManaStone != "")
            {
                _foodManager.UseManaStone();
                _foodManager.ManaStone = "";
            }

            // Evocation
            if (Me.ManaPercentage < 20
                && !surroundingEnemies.Any(e => e.Target == Me.Guid)
                && cast.Normal(Evocation))
            {
                Usefuls.WaitIsCasting();
                return;
            }

            // Arcane Explosion
            if (ToolBox.GetNbEnemiesClose(8f) > 2
                && !surroundingEnemies.Any(e => e.Target == Me.Guid)
                && Me.Mana > 10
                && cast.Normal(ArcaneExplosion))
                return;

            // Icy Veins
            if (Target.HealthPercent < 100
                && Me.ManaPercentage > 10
                && cast.Normal(IcyVeins))
                return;

            // Arcane Power
            if (Target.HealthPercent < 100
                && Me.ManaPercentage > 10
                && cast.Normal(ArcanePower))
                return;

            // Presence of Mind
            if (!Me.HaveBuff("Presence of Mind")
                && Target.HealthPercent < 100
                && cast.Normal(PresenceOfMind))
                return;
            if (Me.HaveBuff("Presence of Mind"))
                if (cast.Normal(ArcaneBlast) || cast.Normal(Frostbolt))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }

            // Cold Snap
            if (IcyVeins.GetCurrentCooldown > 0
                && Me.ManaPercentage > 10
                && !Me.HaveBuff(IcyVeins.Name)
                && cast.Normal(ColdSnap))
                return;

            // Slow
            if (Target.CreatureTypeTarget == "Humanoid"
                && !Target.HaveBuff("Slow")
                && Target.HealthPercent < 10
                && Me.ManaPercentage > 10
                && Slow.IsDistanceGood
                && cast.Normal(Slow))
                return;

            bool _shouldCastArcaneBlast =
                ArcaneBlast.KnownSpell
                && (Me.ManaPercentage > 70
                || Me.HaveBuff("Clearcasting")
                || (Me.ManaPercentage > 50 && ToolBox.CountDebuff("Arcane Blast") < 3)
                || (Me.ManaPercentage > 35 && ToolBox.CountDebuff("Arcane Blast") < 2)
                || (ToolBox.CountDebuff("Arcane Blast") < 1));

            // Arcane Blast
            if (_shouldCastArcaneBlast
                && Target.GetDistance < _distanceRange
                && cast.Normal(ArcaneBlast))
                return;

            // Frost Bolt
            if (Target.GetDistance < _distanceRange
                && Me.ManaPercentage > 10
                && cast.Normal(Frostbolt))
                return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && cast.BannedSpells.Contains("Shoot")
                && cast.Normal(UseWand))
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
        }
    }
}
