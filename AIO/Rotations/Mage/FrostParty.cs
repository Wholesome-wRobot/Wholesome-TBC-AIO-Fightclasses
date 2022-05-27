using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class FrostParty : Mage
    {
        public FrostParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasDrinkBuff || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // Ice Armor
                if (!Me.HasAura(IceArmor)
                    && cast.OnSelf(IceArmor))
                    return;

                // Frost Armor
                if (!Me.HasAura(FrostArmor)
                    && !IceArmor.KnownSpell
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
            int presenceOfMindCD = WTCombat.GetSpellCooldown(PresenceOfMind.Name);

            // PARTY Remove Curse
            if (settings.PartyRemoveCurse)
            {
                List<IWoWPlayer> needRemoveCurse = unitCache.GroupAndRaid
                    .FindAll(m => WTEffects.HasCurseDebuff(m.Name))
                    .ToList();
                if (needRemoveCurse.Count > 0 && cast.OnFocusUnit(RemoveCurse, needRemoveCurse[0]))
                    return;
            }

            // Ice Barrier
            if (Me.HealthPercent < 50
                && cast.OnSelf(IceBarrier))
                return;

            // Blizzard
            if (Blizzard.KnownSpell && unitCache.EnemyUnitsTargetingPlayer.Count <= 0)
            {
                Vector3 center = WTSpace.FindAggregatedCenter(unitCache.EnemiesFighting.Select(unit => unit.PositionWithoutType).ToList(), 15, 3);
                if (center != null
                    && cast.OnLocation(Blizzard, center))
                    return;
            }

            // Ice Lance
            if ((Target.HasAura("Frostbite") || Target.HasAura(FrostNova))
                && cast.OnTarget(IceLance))
                return;

            // Use Mana Stone
            if (Me.ManaPercentage < 20
                && foodManager.UseManaStone())
                return;

            // Evocation
            if (Me.ManaPercentage < 15
                && unitCache.EnemyUnitsTargetingPlayer.Count <= 0
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
            if (SummonWaterElemental.GetCurrentCooldown > 0
                && !ObjectManager.Pet.IsValid
                && Me.ManaPercentage > 10
                && !Me.HasAura(IcyVeins)
                && cast.OnSelf(ColdSnap))
                return;

            // Summon Water Elemental
            if (cast.OnSelf(SummonWaterElemental))
                return;

            // FrostBolt
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
                && !MovementManager.InMovement)
            {
                if (cast.OnTarget(UseWand, false))
                    return;
            }
        }
    }
}
