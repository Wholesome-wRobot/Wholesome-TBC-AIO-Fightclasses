using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class Affliction : Warlock
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Life Tap
            if (Me.HealthPercent > Me.ManaPercentage
                && settings.UseLifeTap
                && !Me.IsMounted
                && cast.OnSelf(LifeTap))
                return;

            // Unending Breath
            if (!Me.HaveBuff("Unending Breath")
                && settings.UseUnendingBreath
                && cast.OnSelf(UnendingBreath))
                return;

            // Fel Armor
            if (!Me.HaveBuff("Fel Armor")
                && settings.UseFelArmor
                && cast.OnSelf(FelArmor))
                return;

            // Demon Armor
            if (!Me.HaveBuff("Demon Skin")
                && !Me.HaveBuff("Demon Armor")
                && !Me.HaveBuff("Fel Armor")
                && cast.OnSelf(DemonArmor))
                return;

            // Demon Skin
            if (!Me.HaveBuff("Demon Skin")
                && !Me.HaveBuff("Demon Armor")
                && !Me.HaveBuff("Fel Armor")
                && cast.OnSelf(DemonSkin))
                return;

            // Health Funnel OOC
            if (ObjectManager.Pet.HealthPercent < 50
                && Me.HealthPercent > 40
                && ObjectManager.Pet.GetDistance < 19
                && !ObjectManager.Pet.InCombatFlagOnly
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

            // Use Soul Stone
            if (!Me.HaveBuff("Soulstone Resurrection")
                && CreateSoulstone.KnownSpell
                && ToolBox.HaveOneInList(WarlockPetAndConsumables.SoulStones())
                && ToolBox.GetItemCooldown(WarlockPetAndConsumables.SoulStones()) <= 0)
            {
                MovementManager.StopMoveNewThread();
                MovementManager.StopMoveToNewThread();
                Lua.RunMacroText("/target player");
                ToolBox.UseFirstMatchingItem(WarlockPetAndConsumables.SoulStones());
                Usefuls.WaitIsCasting();
                Lua.RunMacroText("/cleartarget");
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Pet attack
            if (ObjectManager.Pet.Target != ObjectManager.Me.Target)
                Lua.LuaDoString("PetAttack();", false);

            // Life Tap
            if (Me.HealthPercent > Me.ManaPercentage
                && !Me.IsMounted
                && settings.UseLifeTap
                && cast.OnSelf(LifeTap))
                return;

            // Amplify Curse
            if (!Me.HaveBuff("Amplify Curse")
                && cast.OnSelf(AmplifyCurse))
                return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && settings.UseSiphonLife
                && !ObjectManager.Target.HaveBuff("Siphon Life")
                && cast.OnTarget(SiphonLife))
                return;

            // Unstable Affliction
            if (!ObjectManager.Target.HaveBuff("Unstable Affliction")
                && cast.OnTarget(UnstableAffliction))
                return;

            // Curse of Agony
            if (!ObjectManager.Target.HaveBuff("Curse of Agony")
                && cast.OnTarget(CurseOfAgony))
                return;

            // Corruption
            if (!ObjectManager.Target.HaveBuff("Corruption")
                && cast.OnTarget(Corruption))
                return;

            // Immolate
            if (!ObjectManager.Target.HaveBuff("Immolate")
                && !ObjectManager.Target.HaveBuff("Fire Ward")
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

            WoWUnit Me = ObjectManager.Me;
            WoWUnit Target = ObjectManager.Target;
            double _myManaPC = Me.ManaPercentage;
            bool _overLowManaThreshold = _myManaPC > _innerManaSaveThreshold;

            // Drain Soul
            bool _shouldDrainSoul = ToolBox.CountItemStacks("Soul Shard") < settings.NumberOfSoulShards || settings.AlwaysDrainSoul;
            if (_shouldDrainSoul
                && ObjectManager.Target.HealthPercent < settings.DrainSoulHP
                && ObjectManager.Target.Level >= Me.Level - 8
                && !UnitImmunities.Contains(ObjectManager.Target, "Drain Soul(Rank 1)"))
            {
                if (settings.DrainSoulLevel1
                    && cast.OnTarget(DrainSoulRank1))
                    return;
                else if (cast.OnTarget(DrainSoul))
                    return;
            }

            // How of Terror
            if (ToolBox.GetNumberEnemiesAround(10f, Me) > 1
                && cast.OnSelf(HowlOfTerror))
                return;

            // Use Health Stone
            if (Me.HealthPercent < 15)
                WarlockPetAndConsumables.UseHealthstone();

            // Shadow Trance
            if (Me.HaveBuff("Shadow Trance")
                && _overLowManaThreshold
                && cast.OnTarget(ShadowBolt))
                return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && _overLowManaThreshold
                && Target.HealthPercent > 20
                && !Target.HaveBuff("Siphon Life")
                && settings.UseSiphonLife
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
            if (!Target.HaveBuff("Curse of Agony")
                && _overLowManaThreshold
                && Target.HealthPercent > 20
                && cast.OnTarget(CurseOfAgony))
                return;

            // Unstable Affliction
            if (!Target.HaveBuff("Unstable Affliction")
                && _overLowManaThreshold
                && Target.HealthPercent > 30
                && cast.OnTarget(UnstableAffliction))
                return;

            // Corruption
            if (!Target.HaveBuff("Corruption")
                && _overLowManaThreshold
                && Target.HealthPercent > 20
                && cast.OnTarget(Corruption))
                return;

            // Immolate
            if (!Target.HaveBuff("Immolate")
                && !ObjectManager.Target.HaveBuff("Fire Ward")
                && _overLowManaThreshold
                && Target.HealthPercent > 30
                && (settings.UseImmolateHighLevel || !UnstableAffliction.KnownSpell)
                && cast.OnTarget(Immolate))
                return;

            // Drain Life high
            if (Me.HealthPercent < 70
                && Target.HealthPercent > 20
                && cast.OnTarget(DrainLife))
                return;

            // Health Funnel
            if (ObjectManager.Pet.IsValid
                && ObjectManager.Pet.HealthPercent < 30
                && Me.HealthPercent > 30)
            {
                if (RangeManager.GetRange() > 19)
                    RangeManager.SetRange(19f);
                if (HealthFunnel.IsDistanceGood && cast.OnTarget(HealthFunnel))
                    return;
            }

            // Dark Pact
            if (Me.ManaPercentage < 70
                && ObjectManager.Pet.Mana > 0
                && ObjectManager.Pet.ManaPercentage > 60
                && settings.UseDarkPact
                && cast.OnSelf(DarkPact))
                return;

            // Drain Mana
            if (Me.ManaPercentage < 70
                && Target.Mana > 0
                && Target.ManaPercentage > 30
                && cast.OnTarget(DrainMana))
                return;

            // Incinerate
            if (Target.HaveBuff("Immolate")
                && _overLowManaThreshold
                && Target.HealthPercent > 30
                && settings.UseIncinerate
                && cast.OnTarget(Incinerate))
                return;

            // Shadow Bolt
            if ((!settings.PrioritizeWandingOverSB || !_iCanUseWand)
                && (ObjectManager.Target.HealthPercent > 50 || Me.ManaPercentage > 90 && ObjectManager.Target.HealthPercent > 10)
                && _myManaPC > 40
                && cast.OnTarget(ShadowBolt))
                return;

            // Life Tap
            if (Me.HealthPercent > 50
                && Me.ManaPercentage < 40
                && !ObjectManager.Target.IsTargetingMe
                && settings.UseLifeTap
                && cast.OnSelf(LifeTap))
                return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(UseWand))
                return;

            // Spell if wand banned
            if (UnitImmunities.Contains(ObjectManager.Target, "Shoot")
                && cast.OnTarget(ShadowBolt))
                return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && cast.OnTarget(UseWand, false))
                return;

            // Go in melee because nothing else to do
            if (!ToolBox.UsingWand()
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
