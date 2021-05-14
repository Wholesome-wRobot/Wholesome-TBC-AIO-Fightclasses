using System;
using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class FireParty : Mage
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Arcane Intellect
            WoWPlayer noAI = AIOParty.Group
                .Find(m => !m.HaveBuff(ArcaneIntellect.Name));
            if (noAI != null && cast.OnFocusPlayer(ArcaneIntellect, noAI))
                return;

            // Molten Armor
            if (!Me.HaveBuff("Molten Armor")
                && cast.Normal(MoltenArmor))
                return;

            // Mage Armor
            if (!Me.HaveBuff("Mage Armor")
                && !MoltenArmor.KnownSpell
                && cast.Normal(MageArmor))
                return;

            // Ice Armor
            if (!Me.HaveBuff("Ice Armor")
                && (!MoltenArmor.KnownSpell && !MageArmor.KnownSpell)
                && cast.Normal(IceArmor))
                return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && (!MoltenArmor.KnownSpell && !MageArmor.KnownSpell && !IceArmor.KnownSpell)
                && cast.Normal(FrostArmor))
                return;

            // PARTY Drink
            ToolBox.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold);
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Scorch
            if (_knowImprovedScorch
                && _target.GetDistance < 30f
                && cast.Normal(Scorch))
                return;

            // Fireball
            if (_target.GetDistance < 30f
                && cast.Normal(Fireball))
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
                    .FindAll(m => m.InCombatFlagOnly && ToolBox.HasCurseDebuff(m.Name))
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

            // Dragon's Breath
            if (ToolBox.GetNbEnemiesClose(10f) > 2
                && cast.Normal(DragonsBreath))
                return;

            // Blast Wave
            if (ToolBox.GetNbEnemiesClose(10f) > 2
                && cast.Normal(BlastWave))
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
                if (cast.Normal(Fireball) || cast.Normal(Frostbolt))
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

            // Scorch
            int scorchCount = Target.IsBoss ? 5 : 2;
            if (_knowImprovedScorch
                && Scorch.Cost < Me.Mana
                && (ToolBox.CountDebuff("Fire Vulnerability", "target") < scorchCount || ToolBox.DeBuffTimeLeft("Fire Vulnerability", "target") < 10)
                && cast.Normal(Scorch))
                return;

            // Combustion
            if (!Me.HaveBuff("Combustion")
                && Combustion.GetCurrentCooldown <= 0
                && ToolBox.DeBuffTimeLeft("Fire Vulnerability", "target") > 20
                && ToolBox.CountDebuff("Fire Vulnerability", "target") >= scorchCount
                && cast.Normal(Combustion))
                return;

            // Fireball
            if (cast.Normal(Fireball))
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
