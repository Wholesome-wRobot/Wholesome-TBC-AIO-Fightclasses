using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class FireParty : Mage
    {
        public FireParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                if (_knowImprovedScorch)
                    RangeManager.SetRange(Scorch.MaxRange);
                else
                    RangeManager.SetRange(Fireball.MaxRange);

                // Molten Armor
                if (!Me.HaveBuff("Molten Armor")
                    && cast.OnSelf(MoltenArmor))
                    return;

                // Mage Armor
                if (!Me.HaveBuff("Mage Armor")
                    && !MoltenArmor.KnownSpell
                    && cast.OnSelf(MageArmor))
                    return;

                // Ice Armor
                if (!Me.HaveBuff("Ice Armor")
                    && (!MoltenArmor.KnownSpell && !MageArmor.KnownSpell)
                    && cast.OnSelf(IceArmor))
                    return;

                // Frost Armor
                if (!Me.HaveBuff("Frost Armor")
                    && (!MoltenArmor.KnownSpell && !MageArmor.KnownSpell && !IceArmor.KnownSpell)
                    && cast.OnSelf(FrostArmor))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit _target = ObjectManager.Target;

            // Scorch
            if (_knowImprovedScorch
                && cast.OnTarget(Scorch))
                return;

            // Fireball
            if (cast.OnTarget(Fireball))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;

            // PARTY Remove Curse
            if (settings.PartyRemoveCurse)
            {
                List<AIOPartyMember> needRemoveCurse = partyManager.GroupAndRaid
                    .FindAll(m => m.InCombatFlagOnly && WTEffects.HasCurseDebuff(m.Name))
                    .ToList();
                if (needRemoveCurse.Count > 0 && cast.OnFocusUnit(RemoveCurse, needRemoveCurse[0]))
                    return;
            }

            // Use Mana Stone
            if (Me.ManaPercentage < 20
                && foodManager.UseManaStone())
                return;

            // Evocation
            if (Me.ManaPercentage < 20
                && unitCache.EnemyUnitsTargetingPlayer.Length <= 0
                && cast.OnSelf(Evocation))
            {
                Usefuls.WaitIsCasting();
                return;
            }

            // Dragon's Breath
            if (ToolBox.GetNbEnemiesClose(10f) > 2
                && cast.OnSelf(DragonsBreath))
                return;

            // Blast Wave
            if (ToolBox.GetNbEnemiesClose(10f) > 2
                && cast.OnSelf(BlastWave))
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
                if (cast.OnTarget(Fireball) || cast.OnTarget(Frostbolt))
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

            // Scorch first
            int wantedScorchCount = Target.IsBoss ? 5 : 2;
            int nbScorchDebuffOnTarget = WTEffects.CountDebuff("Fire Vulnerability", "target");
            if (_knowImprovedScorch
                && (nbScorchDebuffOnTarget < wantedScorchCount)
                && cast.OnTarget(Scorch))
                return;

            // Scorch renewal
            if (_knowImprovedScorch
                && nbScorchDebuffOnTarget >= wantedScorchCount 
                && WTEffects.DeBuffTimeLeft("Fire Vulnerability", "target") < 10
                && cast.OnTarget(Scorch))
            {
                Thread.Sleep(1000);
                return;
            }

            // Combustion
            if (!Me.HaveBuff("Combustion")
                && Combustion.GetCurrentCooldown <= 0
                && WTEffects.DeBuffTimeLeft("Fire Vulnerability", "target") > 20
                && WTEffects.CountDebuff("Fire Vulnerability", "target") >= wantedScorchCount
                && cast.OnSelf(Combustion))
                return;

            // Fire Blast
            if (!_knowImprovedScorch
                && cast.OnTarget(FireBlast))
                return;

            // Fireball
            if (cast.OnTarget(Fireball))
                return;


            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot"))
                if (cast.OnTarget(Frostbolt) || cast.OnTarget(Fireball) || cast.OnTarget(ArcaneBlast) || cast.OnTarget(ArcaneMissiles))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && _iCanUseWand
                && !cast.IsBackingUp
                && !MovementManager.InMovement
                && cast.OnTarget(UseWand, false))
                return;
        }
    }
}
