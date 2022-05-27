using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

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
            if (!Me.HasDrinkBuff || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                if (knowImprovedScorch)
                    RangeManager.SetRange(Scorch.MaxRange);
                else
                    RangeManager.SetRange(Fireball.MaxRange);

                // Molten Armor
                if (!Me.HasAura(MoltenArmor)
                    && cast.OnSelf(MoltenArmor))
                    return;

                // Mage Armor
                if (!Me.HasAura(MageArmor)
                    && !MoltenArmor.KnownSpell
                    && cast.OnSelf(MageArmor))
                    return;

                // Ice Armor
                if (!Me.HasAura(IceArmor)
                    && (!MoltenArmor.KnownSpell && !MageArmor.KnownSpell)
                    && cast.OnSelf(IceArmor))
                    return;

                // Frost Armor
                if (!Me.HasAura(FrostArmor)
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

            // Scorch
            if (knowImprovedScorch
                && cast.OnTarget(Scorch))
                return;

            // Fireball
            if (cast.OnTarget(Fireball))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            int presenceOfMindCD = WTCombat.GetSpellCooldown(PresenceOfMind.Name);

            // PARTY Remove Curse
            if (settings.PartyRemoveCurse)
            {
                List<IWoWPlayer> needRemoveCurse = unitCache.GroupAndRaid
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
                && unitCache.EnemyUnitsTargetingPlayer.Count <= 0
                && cast.OnSelf(Evocation))
            {
                Usefuls.WaitIsCasting();
                return;
            }

            // Dragon's Breath
            if (unitCache.EnemyUnitsNearPlayer.FindAll(enemy => enemy.GetDistance < 10).Count > 2
                && cast.OnSelf(DragonsBreath))
                return;

            // Blast Wave
            if (unitCache.EnemyUnitsNearPlayer.FindAll(enemy => enemy.GetDistance < 10).Count > 2
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
            if (presenceOfMindCD <= 0
                && !Me.HasAura(PresenceOfMind)
                && Target.HealthPercent < 100
                && cast.OnSelf(PresenceOfMind))
                return;
            if (Me.HasAura(PresenceOfMind))
                if (cast.OnTarget(Fireball) || cast.OnTarget(Frostbolt))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }

            // Cold Snap
            if (IcyVeins.GetCurrentCooldown > 0
                && Me.ManaPercentage > 10
                && !Me.HasAura(IcyVeins)
                && cast.OnSelf(ColdSnap))
                return;

            // Scorch first
            int wantedScorchCount = Target.IsBoss ? 5 : 2;
            int nbScorchDebuffOnTarget = WTEffects.CountDebuff("Fire Vulnerability", "target");
            int scorchTimeLeft = WTEffects.DeBuffTimeLeft("Fire Vulnerability", "target");
            if (knowImprovedScorch
                && (nbScorchDebuffOnTarget < wantedScorchCount)
                && cast.OnTarget(Scorch))
                return;

            // Scorch renewal
            if (knowImprovedScorch
                && nbScorchDebuffOnTarget >= wantedScorchCount
                && scorchTimeLeft < 10
                && cast.OnTarget(Scorch))
            {
                Thread.Sleep(1000);
                return;
            }

            // Combustion
            if (!Me.HasAura(Combustion)
                && Combustion.GetCurrentCooldown <= 0
                && scorchTimeLeft > 20
                && nbScorchDebuffOnTarget >= wantedScorchCount
                && cast.OnSelf(Combustion))
                return;

            // Fire Blast
            if (!knowImprovedScorch
                && cast.OnTarget(FireBlast))
                return;

            // Fireball
            if (cast.OnTarget(Fireball))
                return;


            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(Target, "Shoot"))
                if (cast.OnTarget(Frostbolt) || cast.OnTarget(Fireball) || cast.OnTarget(ArcaneBlast) || cast.OnTarget(ArcaneMissiles))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && iCanUseWand
                && !cast.IsBackingUp
                && !MovementManager.InMovement
                && cast.OnTarget(UseWand, false))
                return;
        }
    }
}
