using System.Collections.Generic;
using System.Linq;
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

            // Ice Armor
            if (!Me.HaveBuff("Ice Armor")
                && cast.OnSelf(IceArmor))
                return;

            // Frost Armor
            if (!Me.HaveBuff("Frost Armor")
                && !IceArmor.KnownSpell
                && cast.OnSelf(FrostArmor))
                return;

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Frost Bolt
            if (cast.OnTarget(Frostbolt))
                return;

            // Fireball
            if (!Frostbolt.KnownSpell 
                && cast.OnTarget(Fireball))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;

            // PARTY Remove Curse
            if (settings.PartyRemoveCurse)
            {
                List<AIOPartyMember> needRemoveCurse = AIOParty.Group
                    .FindAll(m => ToolBox.HasCurseDebuff(m.Name))
                    .ToList();
                if (needRemoveCurse.Count > 0 && cast.OnFocusUnit(RemoveCurse, needRemoveCurse[0]))
                    return;
            }

            // Ice Barrier
            if (Me.HealthPercent < 50
                && cast.OnSelf(IceBarrier))
                return;

            // Ice Lance
            if ((Target.HaveBuff("Frostbite") || Target.HaveBuff("Frost Nova"))
                && cast.OnTarget(IceLance))
                return;

            // Use Mana Stone
            if (Me.ManaPercentage < 20
                && _foodManager.UseManaStone())
                return;

            // Evocation
            if (Me.ManaPercentage < 15
                && !AIOParty.EnemiesClose.Any(e => e.Target == Me.Guid)
                && cast.OnSelf(Evocation))
            {
                Usefuls.WaitIsCasting();
                return;
            }

            // Cone of Cold
            if (ToolBox.GetNbEnemiesClose(10f) > 2
                && cast.OnTarget(ConeOfCold))
                return;

            // Icy Veins
            if (Target.HealthPercent < 100
                && Me.ManaPercentage > 10
                && !SummonWaterElemental.IsSpellUsable
                && cast.OnSelf(IcyVeins))
                return;

            // Arcane Power
            if (Target.HealthPercent < 100
                && Me.ManaPercentage > 10
                && cast.OnSelf(ArcanePower))
                return;

            // Presence of Mind
            if (!Me.HaveBuff("Presence of Mind")
                && Target.HealthPercent < 100
                && cast.OnSelf(PresenceOfMind))
                return;
            if (Me.HaveBuff("Presence of Mind"))
                if (cast.OnTarget(ArcaneBlast) || cast.OnTarget(Frostbolt))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }

            // Cold Snap
            if (SummonWaterElemental.GetCurrentCooldown > 0
                && !ObjectManager.Pet.IsValid
                && Me.ManaPercentage > 10
                && !Me.HaveBuff(IcyVeins.Name)
                && cast.OnSelf(ColdSnap))
                return;

            // Summon Water Elemental
            if (cast.OnSelf(SummonWaterElemental))
                return;

            // FrostBolt
            if (cast.OnTarget(Frostbolt))
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
                && !MovementManager.InMovement)
            {
                if (cast.OnTarget(UseWand, false))
                    return;
            }
        }
    }
}
