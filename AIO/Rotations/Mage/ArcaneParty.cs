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

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Slow
            if (settings.ACSlow
                && !_target.HaveBuff("Slow")
                && cast.OnTarget(Slow))
                return;

            // Arcane Blast
            if (cast.OnTarget(ArcaneBlast))
                return;

            // Frost Bolt
            if (cast.OnTarget(Frostbolt))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            Lua.LuaDoString("PetAttack();", false);
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

            // Use Mana Stone
            if (Me.ManaPercentage < 20
                && _foodManager.UseManaStone())
                return;

            // Evocation
            if (Me.ManaPercentage < 20
                && !AIOParty.EnemiesClose.Any(e => e.Target == Me.Guid)
                && cast.OnSelf(Evocation))
            {
                Usefuls.WaitIsCasting();
                return;
            }
            
            // Arcane Explosion
            if (ToolBox.GetNbEnemiesClose(8f) > 2
                && !AIOParty.EnemiesClose.Any(e => e.Target == Me.Guid)
                && Me.Mana > 10
                && cast.OnSelf(ArcaneExplosion))
                return;

            // Icy Veins
            if (Target.HealthPercent < 100
                && Me.ManaPercentage > 10
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
            if (IcyVeins.GetCurrentCooldown > 0
                && Me.ManaPercentage > 10
                && !Me.HaveBuff(IcyVeins.Name)
                && cast.OnSelf(ColdSnap))
                return;

            // Slow
            if (Target.CreatureTypeTarget == "Humanoid"
                && !Target.HaveBuff("Slow")
                && Target.HealthPercent < 10
                && Me.ManaPercentage > 10
                && cast.OnTarget(Slow))
                return;

            int arcaneBlastDebuffCount = ToolBox.CountDebuff("Arcane Blast");
            bool _shouldCastArcaneBlast =
                ArcaneBlast.KnownSpell
                && (Me.ManaPercentage > 70
                || Me.HaveBuff("Clearcasting")
                || (Me.ManaPercentage > 50 && arcaneBlastDebuffCount < 3)
                || (Me.ManaPercentage > 35 && arcaneBlastDebuffCount < 2)
                || (arcaneBlastDebuffCount < 1));

            // Arcane Blast
            if (_shouldCastArcaneBlast
                && cast.OnTarget(ArcaneBlast))
                return;

            // Frost Bolt
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
                && !MovementManager.InMovement
                && cast.OnTarget(UseWand, false))
                return;
        }
    }
}
