using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class Demonology : Warlock
    {
        public Demonology(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Life Tap
            if (Me.HealthPercent > Me.ManaPercentage
                && settings.DEM_LifeTap
                && !Me.IsMounted
                && cast.OnSelf(LifeTap))
                return;

            // Unending Breath
            if (!Me.HasAura(UnendingBreath)
                && settings.DEM_UnendingBreath
                && cast.OnSelf(UnendingBreath))
                return;

            // Fel Armor
            if (!Me.HasAura(FelArmor)
                && settings.DEM_FelArmor
                && cast.OnSelf(FelArmor))
                return;

            // Demon Armor
            if (!Me.HasAura(DemonSkin)
                && !Me.HasAura(DemonArmor)
                && !Me.HasAura(FelArmor)
                && cast.OnSelf(DemonArmor))
                return;

            // Demon Skin
            if (!Me.HasAura(DemonSkin)
                && !Me.HasAura(DemonArmor)
                && !Me.HasAura(FelArmor)
                && cast.OnSelf(DemonSkin))
                return;

            // Soul Link
            if (!Me.HasAura(SoulLink)
                && Pet.IsAlive
                && cast.OnSelf(SoulLink))
                return;

            // Health Funnel OOC
            if (Pet.HealthPercent < 50
                && Me.HealthPercent > 40
                && Pet.GetDistance < 19
                && !Pet.InCombatFlagOnly
                && settings.DEM_HealthFunnelOOC)
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

            // Use Soul Stone
            if (!Me.HasAura("Soulstone Resurrection")
                && CreateSoulstone.KnownSpell
                && WTItem.HaveOneInList(WarlockPetAndConsumables.SOULSTONES)
                && ToolBox.GetItemCooldown(WarlockPetAndConsumables.SOULSTONES) <= 0)
            {
                MovementManager.StopMoveNewThread();
                MovementManager.StopMoveToNewThread();
                Lua.RunMacroText("/target player");
                ToolBox.UseFirstMatchingItem(WarlockPetAndConsumables.SOULSTONES);
                Usefuls.WaitIsCasting();
                Lua.RunMacroText("/cleartarget");
            }
        }

        protected override void Pull()
        {
            base.Pull();
            
            // Pet attack
            if (Pet.Target != Me.Target)
                Lua.LuaDoString("PetAttack();");

            // Life Tap
            if (Me.HealthPercent > Me.ManaPercentage
                && !Me.IsMounted
                && settings.DEM_LifeTap
                && cast.OnSelf(LifeTap))
                return;

            // Amplify Curse
            if (!Me.HasAura(AmplifyCurse)
                && cast.OnSelf(AmplifyCurse))
                return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && settings.DEM_SiphonLife
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
                && cast.OnTarget(Corruption))
                return;

            // Immolate
            if (!Target.HasAura(Immolate)
                && !Target.HasAura("Fire Ward")
                && !Corruption.KnownSpell
                && cast.OnTarget(Immolate))
                return;

            // Shadow Bolt
            if (!Immolate.KnownSpell
                && cast.OnTarget(ShadowBolt))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            double myManaPC = Me.ManaPercentage;
            bool overLowManaThreshold = myManaPC > _innerManaSaveThreshold;

            // Drain Soul
            bool _shouldDrainSoul = WTItem.CountItemStacks("Soul Shard") < settings.CommonNumberOfSoulShards || settings.DEM_AlwaysDrainSoul;
            if (_shouldDrainSoul
                && Target.HealthPercent < settings.DEM_DrainSoulHP
                && Target.Level >= Me.Level - 8
                && !UnitImmunities.Contains(Target, "Drain Soul(Rank 1)"))
            {
                if (settings.DEM_DrainSoulLevel1
                    && cast.OnTarget(DrainSoulRank1))
                    return;
                else if (cast.OnTarget(DrainSoul))
                    return;
            }

            // How of Terror
            if (unitCache.EnemiesAttackingMe.FindAll(unit => unit.GetDistance < 10).Count > 1
                && cast.OnSelf(HowlOfTerror))
                return;

            // Use Health Stone
            if (Me.HealthPercent < 15)
                WarlockPetAndConsumables.UseHealthstone();

            // Shadow Trance
            if (Me.HasAura("Shadow Trance")
                && overLowManaThreshold
                && cast.OnTarget(ShadowBolt))
                return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && overLowManaThreshold
                && Target.HealthPercent > 20
                && !Target.HasAura(SiphonLife)
                && settings.DEM_SiphonLife
                && cast.OnTarget(SiphonLife))
                return;

            // Death Coil
            if (Me.HealthPercent < 20
                && cast.OnTarget(DeathCoil))
                return;

            // Drain Life low
            if (Me.HealthPercent < 30
                && Target.HealthPercent > 20
                && cast.OnTarget(DrainLife))
                return;

            // Curse of Agony
            if (!Target.HasAura(CurseOfAgony)
                && overLowManaThreshold
                && Target.HealthPercent > 20
                && cast.OnTarget(CurseOfAgony))
                return;

            // Unstable Affliction
            if (!Target.HasAura(UnstableAffliction)
                && overLowManaThreshold
                && Target.HealthPercent > 30
                && cast.OnTarget(UnstableAffliction))
                return;

            // Corruption
            if (!Target.HasAura(Corruption)
                && overLowManaThreshold
                && Target.HealthPercent > 20
                && cast.OnTarget(Corruption))
                return;

            // Immolate
            if (!Target.HasAura(Immolate)
                && !Target.HasAura("Fire Ward")
                && overLowManaThreshold
                && Target.HealthPercent > 30
                && (settings.DEM_ImmolateHighLevel || !UnstableAffliction.KnownSpell)
                && cast.OnTarget(Immolate))
                return;

            // Drain Life high
            if (Me.HealthPercent < 70
                && Target.HealthPercent > 20
                && cast.OnTarget(DrainLife))
                return;

            // Health Funnel
            if (Pet.IsValid
                && Pet.HealthPercent < 30
                && Me.HealthPercent > 30)
            {
                if (RangeManager.GetRange() > 19)
                    RangeManager.SetRange(19f);
                if (HealthFunnel.IsDistanceGood && cast.OnTarget(HealthFunnel))
                    return;
            }

            // Dark Pact
            if (Me.ManaPercentage < 70
                && Pet.Mana > 0
                && Pet.ManaPercentage > 60
                && settings.DEM_DarkPact
                && cast.OnSelf(DarkPact))
                return;

            // Drain Mana
            if (Me.ManaPercentage < 70
                && Target.Mana > 0
                && Target.ManaPercentage > 30
                && cast.OnTarget(DrainMana))
                return;

            // Incinerate
            if (Target.HasAura(Immolate)
                && overLowManaThreshold
                && Target.HealthPercent > 30
                && settings.DEM_Incinerate
                && cast.OnTarget(Incinerate))
                return;

            // Shadow Bolt
            if ((!settings.DEM_WandingOverSB || !_iCanUseWand)
                && (Target.HealthPercent > 50 || Me.ManaPercentage > 90 && Target.HealthPercent > 10)
                && myManaPC > 40
                && cast.OnTarget(ShadowBolt))
                return;

            // Life Tap
            if (Me.HealthPercent > 50
                && Me.ManaPercentage < 40
                && !Target.IsTargetingMe
                && settings.DEM_LifeTap
                && cast.OnSelf(LifeTap))
                return;

            // Stop wand if banned
            if (WTCombat.IsSpellRepeating(5019)
                && UnitImmunities.Contains(Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(Target, "Shoot")
                && cast.OnTarget(ShadowBolt))
                return;

            // Use Wand
            if (!WTCombat.IsSpellRepeating(5019)
                && _iCanUseWand
                && cast.OnTarget(UseWand, false))
                return;

            // Go in melee because nothing else to do
            if (!WTCombat.IsSpellRepeating(5019)
                && !UseWand.IsSpellUsable
                && !RangeManager.CurrentRangeIsMelee()
                && Target.IsAlive)
            {
                Logger.Log("Going in melee");
                RangeManager.SetRangeToMelee();
                return;
            }
        }
    }
}
