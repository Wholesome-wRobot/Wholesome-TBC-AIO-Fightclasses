using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class AfflictionParty : Warlock
    {
        public AfflictionParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasAura("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // Unending Breath
                if (!Me.HasAura(UnendingBreath)
                    && settings.UseUnendingBreath
                    && cast.OnSelf(UnendingBreath))
                    return;

                // Demon Skin
                if (!Me.HasAura(DemonSkin)
                    && !DemonArmor.KnownSpell
                    && !FelArmor.KnownSpell
                    && cast.OnSelf(DemonSkin))
                    return;

                // Demon Armor
                if (!Me.HasAura(DemonArmor)
                    && !FelArmor.KnownSpell
                    && cast.OnSelf(DemonArmor))
                    return;

                // Fel Armor
                if (!Me.HasAura(FelArmor)
                    && cast.OnSelf(FelArmor))
                    return;

                // Health Funnel OOC
                if (Pet.HealthPercent < 50
                    && Me.HealthPercent > 40
                    && Pet.GetDistance < 19
                    && !Pet.InCombatFlagOnly
                    && settings.HealthFunnelOOC)
                {
                    Lua.LuaDoString("PetWait();");
                    MovementManager.StopMove();
                    Fight.StopFight();

                    if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                        cast.PetSpell("Consume Shadows", false, true);

                    if (cast.OnSelf(HealthFunnel))
                    {
                        Thread.Sleep(500);
                        Usefuls.WaitIsCasting();
                        Lua.LuaDoString("PetFollow();");
                        return;
                    }
                    Lua.LuaDoString("PetFollow();");
                }

                // Health Stone
                if (!WarlockPetAndConsumables.HaveHealthstone()
                    && cast.OnSelf(CreateHealthStone))
                    return;

                // Create Soul Stone
                if (!WarlockPetAndConsumables.HaveSoulstone()
                    && cast.OnSelf(CreateSoulstone))
                    return;

                // PARTY Soul Stone
                if (CreateSoulstone.KnownSpell
                    && WTItem.HaveOneInList(WarlockPetAndConsumables.SOULSTONES)
                    && ToolBox.GetItemCooldown(WarlockPetAndConsumables.SOULSTONES) <= 0)
                {
                    IWoWPlayer noSoulstone = unitCache.GroupAndRaid
                        .Find(m => !m.HasAura("Soulstone Resurrection")
                            && m.GetDistance < 20
                            && m.Name != "Unknown"
                            && !TraceLine.TraceLineGo(Me.PositionWithoutType, m.PositionWithoutType)
                            && (m.WowClass == WoWClass.Paladin || m.WowClass == WoWClass.Priest || m.WowClass == WoWClass.Shaman || m.WowClass == WoWClass.Druid));
                    if (noSoulstone != null)
                    {
                        Logger.Log($"Using Soulstone on {noSoulstone.Name}");
                        MovementManager.StopMoveNewThread();
                        MovementManager.StopMoveToNewThread();
                        Lua.RunMacroText($"/target {noSoulstone.Name}");
                        if (Target.Name == noSoulstone.Name)
                        {
                            ToolBox.UseFirstMatchingItem(WarlockPetAndConsumables.SOULSTONES);
                            Usefuls.WaitIsCasting();
                            Lua.RunMacroText("/cleartarget");
                            ToolBox.ClearCursor();
                        }
                    }
                }

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;

                // Life Tap
                if (Me.HealthPercent > Me.ManaPercentage
                    && settings.UseLifeTap
                    && cast.OnSelf(LifeTap))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Pet attack
            if (Pet.Target != Me.Target)
                Lua.LuaDoString("PetAttack();");

            // PARTY Seed of Corruption
            if (unitCache.EnemiesFighting.Count >= settings.PartySeedOfCorruptionAmount
                && SeedOfCorruption.KnownSpell)
            {
                List<IWoWUnit> enemiesWithoutSeedOfCorruption = unitCache.EnemiesFighting
                    .Where(e => !e.HasAura(SeedOfCorruption))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutSeedOfCorruption.Count > 0
                   && cast.OnFocusUnit(SeedOfCorruption, enemiesWithoutSeedOfCorruption[0]))
                {
                    Thread.Sleep(1000);
                    return;
                }
            }

            // Curse of The Elements
            if (!Target.HasAura(CurseOfTheElements)
                && settings.PartyCurseOfTheElements
                && cast.OnTarget(CurseOfTheElements))
                return;

            // Amplify Curse
            if (!Me.HasAura(AmplifyCurse)
                && cast.OnSelf(AmplifyCurse))
                return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && settings.UseSiphonLife
                && !Target.HasAura(SiphonLife)
                && cast.OnTarget(SiphonLife))
                return;

            // Unstable Affliction
            if (!Target.HasAura(UnstableAffliction)
                && cast.OnTarget(UnstableAffliction))
                return;

            // Curse of Agony
            if (!Target.HasAura(CurseOfAgony)
                && cast.OnTarget(CurseOfAgony))
                return;

            // Corruption
            if (!Target.HasAura(Corruption)
                && Target.HasAura(SeedOfCorruption)
                && cast.OnTarget(Corruption))
                return;

            // Immolate
            if (!Target.HasAura(Immolate)
                && !Corruption.KnownSpell
                && cast.OnTarget(Immolate))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            // Soulshatter
            if (SoulShatter.IsSpellUsable
                && settings.UseSoulShatter
                && unitCache.EnemyUnitsTargetingPlayer.Count > 0
                && WTItem.CountItemStacks("Soul Shard") > 0
                && cast.OnSelf(SoulShatter))
                return;

            // Life Tap
            if (Me.ManaPercentage < settings.PartyLifeTapManaThreshold
                && Me.HealthPercent > settings.PartyLifeTapHealthThreshold
                && settings.UseLifeTap
                && cast.OnSelf(LifeTap))
                return;

            // Shadow Trance
            if (Me.HasAura("Shadow Trance")
                && cast.OnTarget(ShadowBolt))
                return;

            // PARTY Seed of Corruption
            if (unitCache.EnemiesFighting.Count >= settings.PartySeedOfCorruptionAmount
                && SeedOfCorruption.KnownSpell)
            {
                List<IWoWUnit> enemiesWithoutSeedOfCorruption = unitCache.EnemiesFighting
                    .Where(e => !e.HasAura(SeedOfCorruption))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutSeedOfCorruption.Count > 0
                   && cast.OnFocusUnit(SeedOfCorruption, enemiesWithoutSeedOfCorruption[0]))
                {
                    Thread.Sleep(1000);
                    return;
                }
            }

            if (CurseOfTheElements.KnownSpell
                && settings.PartyCurseOfTheElements)
            {
                // PARTY Curse of the Elements
                List<IWoWUnit> enemiesWithoutCurseOfTheElements = unitCache.EnemiesFighting
                    .Where(e => !e.HasAura(CurseOfTheElements))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutCurseOfTheElements.Count > 0
                   && unitCache.EnemiesFighting.Count - enemiesWithoutCurseOfTheElements.Count < 3
                   && cast.OnFocusUnit(CurseOfTheElements, enemiesWithoutCurseOfTheElements[0]))
                    return;
            }
            else
            {
                // PARTY Curse of Agony
                List<IWoWUnit> enemiesWithoutCurseOfAgony = unitCache.EnemiesFighting
                    .Where(e => !e.HasAura(CurseOfAgony))
                    .OrderBy(e => e.GetDistance)
                    .ToList();
                if (enemiesWithoutCurseOfAgony.Count > 0
                   && unitCache.EnemiesFighting.Count - enemiesWithoutCurseOfAgony.Count < 3
                   && cast.OnFocusUnit(CurseOfAgony, enemiesWithoutCurseOfAgony[0]))
                    return;
            }

            // PARTY Unstable Affliction
            List<IWoWUnit> enemiesWithoutUnstableAff = unitCache.EnemiesFighting
                .Where(e => !e.HasAura(UnstableAffliction))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutUnstableAff.Count > 0
               && unitCache.EnemiesFighting.Count - enemiesWithoutUnstableAff.Count < 3
               && cast.OnFocusUnit(UnstableAffliction, enemiesWithoutUnstableAff[0]))
                return;

            // PARTY Corruption
            List<IWoWUnit> enemiesWithoutCorruption = unitCache.EnemiesFighting
                .Where(e => !e.HasAura(Corruption) && !e.HasAura(SeedOfCorruption))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutCorruption.Count > 0
               && unitCache.EnemiesFighting.Count - enemiesWithoutCorruption.Count < 3
               && cast.OnFocusUnit(Corruption, enemiesWithoutCorruption[0]))
                return;

            // PARTY Immolate
            List<IWoWUnit> enemiesWithoutImmolate = unitCache.EnemiesFighting
                .Where(e => !e.HasAura(Immolate))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutImmolate.Count > 0
               && unitCache.EnemiesFighting.Count - enemiesWithoutImmolate.Count < 3
               && cast.OnFocusUnit(Immolate, enemiesWithoutImmolate[0]))
                return;

            // PARTY Siphon Life
            List<IWoWUnit> enemiesWithoutSiphonLife = unitCache.EnemiesFighting
                .Where(e => !e.HasAura(SiphonLife))
                .OrderBy(e => e.GetDistance)
                .ToList();
            if (enemiesWithoutSiphonLife.Count > 0
               && unitCache.EnemiesFighting.Count - enemiesWithoutSiphonLife.Count < 3
               && cast.OnFocusUnit(SiphonLife, enemiesWithoutSiphonLife[0]))
                return;

            // Drain Soul
            bool _shouldDrainSoul = WTItem.CountItemStacks("Soul Shard") < settings.NumberOfSoulShards || settings.AlwaysDrainSoul;
            if (_shouldDrainSoul
                && Target.HealthPercent < settings.DrainSoulHP
                && Target.Level >= Me.Level - 8
                && !UnitImmunities.Contains(Target, "Drain Soul(Rank 1)"))
            {
                if (settings.DrainSoulLevel1
                    && cast.OnTarget(DrainSoulRank1))
                    return;
                else if (cast.OnTarget(DrainSoul))
                    return;
            }

            // Shadow Bolt
            if (cast.OnTarget(ShadowBolt))
                return;

            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(Target, "Shoot"))
                if (cast.OnTarget(ShadowBolt))
                    return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && _iCanUseWand
                && cast.OnTarget(UseWand, false))
                return;
        }
    }
}
