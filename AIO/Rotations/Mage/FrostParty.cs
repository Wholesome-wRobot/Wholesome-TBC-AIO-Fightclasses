using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class FrostParty : Mage
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Arcane Intellect
            WoWPlayer noAI = AIOParty.Group
                .Find(m => !m.HaveBuff(ArcaneIntellect.Name));
            if (noAI != null && cast.OnFocusPlayer(ArcaneIntellect, noAI))
                return;

            // Ice Armor
            if (!Me.HaveBuff("Ice Armor"))
                if (cast.Normal(IceArmor))
                    return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && !IceArmor.KnownSpell)
                if (cast.Normal(FrostArmor))
                    return;

            // PARTY Drink
            ToolBox.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold);
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Frost Bolt
            if (_target.GetDistance < 30
                && cast.Normal(Frostbolt))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;
            List<WoWUnit> surroundingEnemies = ObjectManager.GetObjectWoWUnit()
                .Where(e => e.IsAttackable && e.IsAlive && e.IsValid && !e.PlayerControlled && e.InCombatFlagOnly)
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

            // Ice Barrier
            if (Me.HealthPercent < 50
                && cast.Normal(IceBarrier))
                return;

            // Ice Lance
            if (Target.HaveBuff("Frostbite")
                || Target.HaveBuff("Frost Nova"))
                if (cast.Normal(IceLance))
                    return;

            // Use Mana Stone
            if (Me.ManaPercentage < 20
                && _foodManager.ManaStone != "")
            {
                _foodManager.UseManaStone();
                _foodManager.ManaStone = "";
            }

            // Evocation
            if (Me.ManaPercentage < 15
                && !surroundingEnemies.Any(e => e.Target == Me.Guid)
                && cast.Normal(Evocation))
            {
                Usefuls.WaitIsCasting();
                return;
            }

            // Cone of Cold
            if (ToolBox.GetNbEnemiesClose(10f) > 2
                && !surroundingEnemies.Any(e => e.Target == Me.Guid)
                && cast.Normal(ConeOfCold))
                return;

            // Icy Veins
            if (Target.HealthPercent < 100
                && Me.ManaPercentage > 10
                && !SummonWaterElemental.IsSpellUsable
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
            if (ToolBox.GetSpellCooldown(SummonWaterElemental.Name) > 0
                && !ObjectManager.Pet.IsValid
                && Me.ManaPercentage > 10
                && !Me.HaveBuff(IcyVeins.Name)
                && cast.Normal(ColdSnap))
                return;

            // Summon Water Elemental
            if (cast.Normal(SummonWaterElemental))
                return;

            // FrostBolt
            if (cast.Normal(Frostbolt))
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
