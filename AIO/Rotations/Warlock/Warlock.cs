using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using robotManager.Events;
using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class Warlock : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static WarlockSettings settings;

        protected Cast cast;

        protected BackgroundWorker _petPulseThread = new BackgroundWorker();
        protected Stopwatch _addCheckTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected float _maxRange = 27f;
        protected int _innerManaSaveThreshold = 20;
        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected int _saveDrinkPercent = wManager.wManagerSetting.CurrentSetting.DrinkPercent;
        protected List<WoWUnit> _partyEnemiesAround = new List<WoWUnit>();

        protected Warlock specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = WarlockSettings.Current;
            cast = new Cast(ShadowBolt, settings.ActivateCombatDebug, UseWand, settings.AutoDetectImmunities);

            this.specialization = specialization as Warlock;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);
            
            _petPulseThread.DoWork += PetThread;
            _petPulseThread.RunWorkerAsync();
            
            RangeManager.SetRange(_maxRange);

            // Set pet mode
            if (settings.PetInPassiveWhenOOC)
                Lua.LuaDoString("PetPassiveMode();");
            else
                Lua.LuaDoString("PetDefensiveMode();");

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FiniteStateMachineEvents.OnRunState += OnRunStateHandler;

            Rotation();
        }

        public void Dispose()
        {
            _petPulseThread.DoWork -= PetThread;
            _petPulseThread.Dispose();
            Lua.LuaDoString("PetPassiveMode();");
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FiniteStateMachineEvents.OnRunState -= OnRunStateHandler;
            wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;
            cast.Dispose();
            Logger.Log("Disposed");
        }

        // Pet thread
        protected void PetThread(object sender, DoWorkEventArgs args)
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && ObjectManager.Pet.IsValid
                        && ObjectManager.Pet.IsAlive)
                    {
                        bool multiAggroImTargeted = false;
                        // Pet Switch target on multi aggro
                        if (Me.InCombatFlagOnly
                            && ObjectManager.GetNumberAttackPlayer() > 1)
                        {
                            Lua.LuaDoString("PetDefensiveMode();");
                            // Get list of units targeting me in a multiaggro situation
                            List<WoWUnit> unitsAttackingMe = ObjectManager.GetUnitAttackPlayer()
                                .OrderBy(u => u.Guid)
                                .Where(u => u.TargetObject.Guid == Me.Guid)
                                .ToList();

                            foreach (WoWUnit unit in unitsAttackingMe)
                            {
                                multiAggroImTargeted = true;
                                if (unit.Guid != ObjectManager.Pet.TargetObject.Guid
                                    && ObjectManager.Pet.TargetObject.Target == ObjectManager.Pet.Guid)
                                { 
                                    Logger.Log($"Forcing pet aggro on {unit.Name}");
                                    Me.FocusGuid = unit.Guid;
                                    cast.PetSpell("PET_ACTION_ATTACK", true);
                                    if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                                    {
                                        cast.PetSpell("Torment", true);
                                        cast.PetSpell("Suffering", true);
                                    }
                                    if (WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard"))
                                        cast.PetSpell("Anguish", true);
                                    Lua.LuaDoString("ClearFocus();");
                                }
                            }
                        }

                        // Pet attack on single aggro
                        if ((Me.InCombatFlagOnly || Fight.InFight)
                            && Me.Target > 0
                            && !multiAggroImTargeted)
                            Lua.LuaDoString("PetAttack();", false);

                        // Voidwalker Torment + Felguard Anguish
                        if ((!settings.AutoTorment || !settings.AutoAnguish)
                            && ObjectManager.Target.Target == Me.Guid
                            && Me.InCombatFlagOnly)
                        {
                            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                                if (cast.PetSpell("Torment") || cast.PetSpell("Suffering"))
                                    continue;
                            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard"))
                                if (cast.PetSpell("Anguish"))
                                    continue;
                        }

                        // Switch Auto Torment & Suffering off
                        if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                        {
                            ToolBox.TogglePetSpellAuto("Torment", settings.AutoTorment);
                            ToolBox.TogglePetSpellAuto("Suffering", false);
                        }

                        // Switch Felguard Auto Cleave/Anguish
                        if (WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard"))
                        {
                            ToolBox.TogglePetSpellAuto("Cleave", settings.FelguardCleave);
                            ToolBox.TogglePetSpellAuto("Anguish", settings.AutoAnguish);
                        }
                    }
                }
                catch (Exception arg)
                {
                    Logging.WriteError(string.Concat(arg), true);
                }
                Thread.Sleep(300);
            }
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (RotationType == Enums.RotationType.Party)
                        _partyEnemiesAround = ToolBox.GetSuroundingEnemies();

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();
                }
                catch (Exception arg)
                {
                    Logging.WriteError("ERROR: " + arg, true);
                }
                Thread.Sleep(ToolBox.GetLatency() + settings.ThreadSleepCycle);
            }
            Logger.Log("Stopped.");
        }

        protected virtual void BuffRotation()
        {
            
            // Delete additional Soul Shards
            if (ToolBox.CountItemStacks("Soul Shard") > settings.NumberOfSoulShards)
            {
                Logger.Log("Deleting excess Soul Shard");
                ToolBox.LuaDeleteOneItem("Soul Shard");
            }

            // Define the demon to summon
            AIOSpell SummonSpell = null;
            bool shouldSummon = false;
            if (SummonImp.KnownSpell)
            {
                if (ToolBox.CountItemStacks("Soul Shard") < 1 || !SummonVoidwalker.KnownSpell && !SummonFelguard.KnownSpell)
                    SummonSpell = SummonImp;

                if (SummonVoidwalker.KnownSpell && !SummonFelguard.KnownSpell)
                    SummonSpell = SummonVoidwalker;

                if (SummonFelguard.KnownSpell)
                    SummonSpell = SummonFelguard;

                if (!ObjectManager.Pet.IsValid 
                    || ObjectManager.Pet.ManaPercentage < settings.ManaThresholdResummon && SummonSpell != SummonImp
                    || ObjectManager.Pet.HealthPercent < settings.HealthThresholdResummon
                    || !SummonSpell.Name.Contains(WarlockPetAndConsumables.MyWarlockPet()))
                    shouldSummon = true;
            }

            if (shouldSummon)
            {
                // Make sure we have mana to summon
                if (ObjectManager.Me.Mana < SummonSpell.Cost
                    && !ObjectManager.Me.HaveBuff("Drink")
                    && !Me.InCombatFlagOnly)
                {
                    Logger.Log($"Not enough mana to summon {SummonSpell.Name}, forcing regen");
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 95;
                    Thread.Sleep(1000);
                    return;
                }

                Thread.Sleep(Usefuls.Latency + 500); // Safety for Mount check
                if (!ObjectManager.Me.IsMounted && !ObjectManager.Me.IsOnTaxi)
                {
                    if (cast.Normal(FelDomination))
                        Thread.Sleep(200);
                    if (cast.Normal(SummonSpell))
                        return;
                }
            }
            else
                wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;
            
            // Life Tap
            if (Me.HealthPercent > Me.ManaPercentage
                && settings.UseLifeTap
                && !Me.IsMounted)
                if (cast.Normal(LifeTap))
                    return;
            
            // Unending Breath
            if (!Me.HaveBuff("Unending Breath")
                && UnendingBreath.KnownSpell
                && UnendingBreath.IsSpellUsable
                && settings.UseUnendingBreath)
            {
                if (cast.OnSelf(UnendingBreath))
                    return;
            }
            
            // Demon Skin
            if (!Me.HaveBuff("Demon Skin")
                && !DemonArmor.KnownSpell
                && DemonSkin.KnownSpell)
                if (cast.Normal(DemonSkin))
                    return;
            
            // Demon Armor
            if ((!Me.HaveBuff("Demon Armor") || Me.HaveBuff("Demon Skin"))
                && DemonArmor.KnownSpell
                && (!FelArmor.KnownSpell || FelArmor.KnownSpell && !settings.UseFelArmor))
                if (cast.Normal(DemonArmor))
                    return;
            
            // Soul Link
            if (SoulLink.KnownSpell
                && !Me.HaveBuff("Soul Link")
                && ObjectManager.Pet.IsAlive)
                if (cast.Normal(SoulLink))
                    return;
            
            // Fel Armor
            if (!Me.HaveBuff("Fel Armor")
                && FelArmor.KnownSpell
                && settings.UseFelArmor)
                if (cast.Normal(FelArmor))
                    return;
            
            // Health Funnel OOC
            if (ObjectManager.Pet.HealthPercent < 50
                && Me.HealthPercent > 40
                && ObjectManager.Pet.GetDistance < 19
                && !ObjectManager.Pet.InCombatFlagOnly
                && HealthFunnel.KnownSpell
                && settings.HealthFunnelOOC
                && HealthFunnel.IsSpellUsable)
            {
                Lua.LuaDoString("PetWait();");
                MovementManager.StopMove();
                Fight.StopFight();

                if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                    cast.PetSpell("Consume Shadows", false, true);

                if (cast.Normal(HealthFunnel))
                {
                    Thread.Sleep(500);
                    Usefuls.WaitIsCasting();
                    Lua.LuaDoString("PetFollow();");
                    return;
                }
                Lua.LuaDoString("PetFollow();");
            }
            
            // Health Stone
            if (!WarlockPetAndConsumables.HaveHealthstone())
                if (cast.Normal(CreateHealthStone))
                    return;
            
            // Create Soul Stone
            if (!WarlockPetAndConsumables.HaveSoulstone()
                && CreateSoulstone.KnownSpell)
            {
                if (cast.Normal(CreateSoulstone))
                    return;
            }

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

        protected virtual void Pull()
        {
            // Curse of Agony
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !ObjectManager.Target.HaveBuff("Curse of Agony"))
                if (cast.Normal(CurseOfAgony))
                    return;

            // Corruption
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !ObjectManager.Target.HaveBuff("Corruption"))
                if (cast.Normal(Corruption))
                    return;

            // Immolate
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !ObjectManager.Target.HaveBuff("Immolate")
                && !ObjectManager.Target.HaveBuff("Fire Ward")
                && !Corruption.KnownSpell
                /*&& ToolBox.CanBleed(ObjectManager.Target)*/)
                if (cast.Normal(Immolate))
                    return;

            // Shadow Bolt
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !Immolate.KnownSpell)
                if (cast.Normal(ShadowBolt))
                    return;
        }

        protected virtual void CombatRotation()
        {
            WoWUnit Me = ObjectManager.Me;
            WoWUnit Target = ObjectManager.Target;
            double _myManaPC = Me.ManaPercentage;
            bool _overLowManaThreshold = _myManaPC > _innerManaSaveThreshold;

            // Drain Soul
            bool _shouldDrainSoul = ToolBox.CountItemStacks("Soul Shard") < settings.NumberOfSoulShards || settings.AlwaysDrainSoul;
            if (_shouldDrainSoul
                && Target.HealthPercent < settings.DrainSoulHP
                && Target.Level >= Me.Level - 8
                && DrainSoul.KnownSpell
                && !cast.BannedSpells.Contains("Drain Soul(Rank 1)"))
                if (settings.DrainSoulLevel1)
                {
                    Lua.RunMacroText("/cast Drain Soul(Rank 1)");
                    Usefuls.WaitIsCasting();
                }
                else
                {
                    if (cast.Normal(DrainSoul))
                        return;
                }

            // How of Terror
            if (HowlOfTerror.KnownSpell
                && HowlOfTerror.IsSpellUsable
                && ToolBox.GetNumberEnemiesAround(10f, Me) > 1)
                if (cast.Normal(HowlOfTerror))
                    return;

            // Use Health Stone
            if (Me.HealthPercent < 15)
                WarlockPetAndConsumables.UseHealthstone();

            // Shadow Trance
            if (Me.HaveBuff("Shadow Trance") && _overLowManaThreshold)
                if (cast.Normal(ShadowBolt))
                    return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && _overLowManaThreshold
                && Target.HealthPercent > 20
                && !Target.HaveBuff("Siphon Life")
                && settings.UseSiphonLife)
                if (cast.Normal(SiphonLife))
                    return;

            // Death Coil
            if (Me.HealthPercent < 20)
                if (cast.Normal(DeathCoil))
                    return;

            // Drain Life low
            if (Me.HealthPercent < 30
                && Target.HealthPercent > 20)
                if (cast.Normal(DrainLife))
                    return;

            // Curse of Agony
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Curse of Agony")
                && _overLowManaThreshold
                && Target.HealthPercent > 20)
                if (cast.Normal(CurseOfAgony))
                    return;

            // Unstable Affliction
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Unstable Affliction")
                && _overLowManaThreshold
                && Target.HealthPercent > 30)
                if (cast.Normal(UnstableAffliction))
                    return;

            // Corruption
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Corruption")
                && _overLowManaThreshold
                && Target.HealthPercent > 20)
                if (cast.Normal(Corruption))
                    return;

            // Immolate
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Immolate")
                && !ObjectManager.Target.HaveBuff("Fire Ward")
                && _overLowManaThreshold
                && Target.HealthPercent > 30
                && (settings.UseImmolateHighLevel || !UnstableAffliction.KnownSpell)
                /*&& ToolBox.CanBleed(ObjectManager.Target)*/)
                if (cast.Normal(Immolate))
                    return;

            // Drain Life high
            if (Me.HealthPercent < 70
                && Target.HealthPercent > 20)
                if (cast.Normal(DrainLife))
                    return;

            // Health Funnel
            if (ObjectManager.Pet.IsValid
                && ObjectManager.Pet.HealthPercent < 30
                && Me.HealthPercent > 30)
            {
                if (RangeManager.GetRange() > 19)
                    RangeManager.SetRange(19f);
                if (HealthFunnel.IsDistanceGood && cast.Normal(HealthFunnel))
                    return;
            }

            // Dark Pact
            if (Me.ManaPercentage < 70
                && ObjectManager.Pet.Mana > 0
                && ObjectManager.Pet.ManaPercentage > 60
                && settings.UseDarkPact)
                if (cast.Normal(DarkPact))
                    return;

            // Drain Mana
            if (Me.ManaPercentage < 70
                && Target.Mana > 0
                && Target.ManaPercentage > 30)
                if (cast.Normal(DrainMana))
                    return;

            // Incinerate
            if (ObjectManager.Target.GetDistance < _maxRange 
                && Target.HaveBuff("Immolate")
                && _overLowManaThreshold
                && Target.HealthPercent > 30
                && settings.UseIncinerate)
                if (cast.Normal(Incinerate))
                    return;

            // Shadow Bolt
            if ((!settings.PrioritizeWandingOverSB || !_iCanUseWand)
                && (ObjectManager.Target.HealthPercent > 50 || Me.ManaPercentage > 90 && ObjectManager.Target.HealthPercent > 10)
                && _myManaPC > 40
                && ObjectManager.Target.GetDistance < _maxRange)
                if (cast.Normal(ShadowBolt))
                    return;

            // Life Tap
            if (Me.HealthPercent > 50
                && Me.ManaPercentage < 40
                && !ObjectManager.Target.IsTargetingMe
                && settings.UseLifeTap)
                if (cast.Normal(LifeTap))
                    return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && cast.BannedSpells.Contains("Shoot"))
                if (cast.Normal(UseWand))
                    return;

            // Spell if wand banned
            if (cast.BannedSpells.Contains("Shoot")
                && ObjectManager.Target.GetDistance < _maxRange)
                if (cast.Normal(ShadowBolt))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && ObjectManager.Target.GetDistance <= _maxRange + 2)
            {
                RangeManager.SetRange(_maxRange);
                if (cast.Normal(UseWand, false))
                    return;
            }

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

        protected AIOSpell DemonSkin = new AIOSpell("Demon Skin");
        protected AIOSpell DemonArmor = new AIOSpell("Demon Armor");
        protected AIOSpell LifeTap = new AIOSpell("Life Tap");
        protected AIOSpell ShadowBolt = new AIOSpell("Shadow Bolt");
        protected AIOSpell UseWand = new AIOSpell("Shoot");
        protected AIOSpell Immolate = new AIOSpell("Immolate");
        protected AIOSpell Corruption = new AIOSpell("Corruption");
        protected AIOSpell CurseOfAgony = new AIOSpell("Curse of Agony");
        protected AIOSpell DrainSoul = new AIOSpell("Drain Soul");
        protected AIOSpell DrainLife = new AIOSpell("Drain Life");
        protected AIOSpell Fear = new AIOSpell("Fear");
        protected AIOSpell SummonImp = new AIOSpell("Summon Imp");
        protected AIOSpell SummonVoidwalker = new AIOSpell("Summon Voidwalker");
        protected AIOSpell SummonFelguard = new AIOSpell("Summon Felguard");
        protected AIOSpell CreateHealthStone = new AIOSpell("Create HealthStone");
        protected AIOSpell HealthFunnel = new AIOSpell("Health Funnel");
        protected AIOSpell CreateSoulstone = new AIOSpell("Create Soulstone");
        protected AIOSpell AmplifyCurse = new AIOSpell("Amplify Curse");
        protected AIOSpell UnendingBreath = new AIOSpell("Unending Breath");
        protected AIOSpell SiphonLife = new AIOSpell("Siphon Life");
        protected AIOSpell DrainMana = new AIOSpell("Drain Mana");
        protected AIOSpell DarkPact = new AIOSpell("Dark Pact");
        protected AIOSpell UnstableAffliction = new AIOSpell("Unstable Affliction");
        protected AIOSpell DeathCoil = new AIOSpell("Death Coil");
        protected AIOSpell FelArmor = new AIOSpell("Fel Armor");
        protected AIOSpell Incinerate = new AIOSpell("Incinerate");
        protected AIOSpell SoulShatter = new AIOSpell("Soulshatter");
        protected AIOSpell FelDomination = new AIOSpell("Fel Domination");
        protected AIOSpell SoulLink = new AIOSpell("Soul Link");
        protected AIOSpell HowlOfTerror = new AIOSpell("Howl of Terror");

        // EVENT HANDLERS
        private void OnRunStateHandler(Engine engine, State state, CancelEventArgs cancelable)
        {
            if (state is wManager.Wow.Bot.States.Resurrect 
                || state is wManager.Wow.Bot.States.ResurrectBG
                || state.DisplayName.Contains("Resurrect"))
            {
                Thread.Sleep(1000);
                Lua.LuaDoString("UseSoulstone();");
                Thread.Sleep(1000);
            }
        }

        private void FightEndHandler(ulong guid)
        {
            _iCanUseWand = false;
            RangeManager.SetRange(_maxRange);
            _addCheckTimer.Reset();
            if (settings.PetInPassiveWhenOOC)
                Lua.LuaDoString("PetPassiveMode();");
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            if (UseWand.IsSpellUsable)
                _iCanUseWand = true;
            Lua.LuaDoString("PetDefensiveMode();");

            // Imp Firebolt
            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Imp"))
                ToolBox.TogglePetSpellAuto("Firebolt", true);

            // Imp BloodPact
            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Imp"))
                ToolBox.TogglePetSpellAuto("Blood Pact", true);
        }
    }
}