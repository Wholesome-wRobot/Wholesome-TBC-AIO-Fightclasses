using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class ArcaneParty : Mage
    {
        public ArcaneParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasDrinkAura || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // Mage Armor
                if (!Me.HasAura(MageArmor)
                    && settings.PARC_ACMageArmor
                    && cast.OnSelf(MageArmor))
                    return;

                // Ice Armor
                if (!Me.HasAura(IceArmor)
                    && (!settings.PARC_ACMageArmor || !MageArmor.KnownSpell)
                    && cast.OnSelf(IceArmor))
                    return;

                // Frost Armor
                if (!Me.HasAura(FrostArmor)
                    && !IceArmor.KnownSpell
                    && (!settings.PARC_ACMageArmor || !MageArmor.KnownSpell)
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

            // Slow
            if (settings.PARC_ACSlow
                && !Target.HasAura(Slow)
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
            Lua.LuaDoString("PetAttack();");
            int presenceOfMindCD = WTCombat.GetSpellCooldown(PresenceOfMind.Name);

            // PARTY Remove Curse
            if (settings.PARC_PartyRemoveCurse)
            {
                List<IWoWPlayer> needRemoveCurse = unitCache.GroupAndRaid
                    .FindAll(m => WTEffects.HasCurseDebuff(m.Name))
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

            // Arcane Explosion
            if (unitCache.EnemiesFighting.FindAll(unit => unit.GetDistance < 8).Count > 2
                && unitCache.EnemyUnitsTargetingPlayer.Count <= 0
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
            if (presenceOfMindCD <= 0
                && !Me.HasAura(PresenceOfMind)
                && Target.HealthPercent < 100
                && cast.OnSelf(PresenceOfMind))
                return;
            if (Me.HasAura(PresenceOfMind))
                if (cast.OnTarget(ArcaneBlast) || cast.OnTarget(Frostbolt))
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

            // Slow
            if (Target.CreatureTypeTarget == "Humanoid"
                && !Target.HasAura(Slow)
                && Target.HealthPercent < 10
                && Me.ManaPercentage > 10
                && cast.OnTarget(Slow))
                return;

            int arcaneBlastDebuffCount = WTEffects.CountDebuff("Arcane Blast");
            bool _shouldCastArcaneBlast =
                ArcaneBlast.KnownSpell
                && (Me.ManaPercentage > 70
                || Me.HasAura("Clearcasting")
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
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(Target, "Shoot"))
                if (cast.OnTarget(ArcaneBlast) || cast.OnTarget(ArcaneMissiles) || cast.OnTarget(Frostbolt) || cast.OnTarget(Fireball))
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
