using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
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

        protected int _innerManaSaveThreshold = 20;
        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected int _saveDrinkPercent = wManager.wManagerSetting.CurrentSetting.DrinkPercent;

        protected Warlock specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = WarlockSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);
            cast = new Cast(ShadowBolt, UseWand, settings);

            this.specialization = specialization as Warlock;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            WarlockPetAndConsumables.Setup();

            _petPulseThread.DoWork += PetThread;
            _petPulseThread.RunWorkerAsync();
            
            RangeManager.SetRange(ShadowBolt.MaxRange);

            // Set pet mode
            if (settings.PetInPassiveWhenOOC)
                Lua.LuaDoString("PetPassiveMode();");
            else
                Lua.LuaDoString("PetDefensiveMode();");

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;

            Rotation();
        }

        public void Dispose()
        {
            _petPulseThread.DoWork -= PetThread;
            _petPulseThread.Dispose();
            Lua.LuaDoString("PetPassiveMode();");
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
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
                        if (WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard") && specialization.RotationType == Enums.RotationType.Solo)
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
                    if (StatusChecker.OutOfCombat(RotationRole))
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

                if (specialization.RotationType == Enums.RotationType.Party)
                    SummonSpell = SummonImp;

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
                    if (cast.OnSelf(FelDomination))
                        Thread.Sleep(200);
                    if (cast.OnSelf(SummonSpell))
                    {
                        Usefuls.WaitIsCasting();
                        Thread.Sleep(1000); // Prevent double summon
                        return;
                    }
                }
            }
            else
                wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
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
        protected AIOSpell DrainSoulRank1 = new AIOSpell("Drain Soul", 1);
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
        protected AIOSpell CurseOfTheElements = new AIOSpell("Curse of the Elements");
        protected AIOSpell CurseOfRecklessness = new AIOSpell("Curse of Recklessness");
        protected AIOSpell CurseOfDoom = new AIOSpell("Curse of Doom");
        protected AIOSpell SeedOfCorruption = new AIOSpell("Seed of Corruption");
        protected AIOSpell Soulshatter = new AIOSpell("Soulshatter");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _iCanUseWand = false;
            RangeManager.SetRange(ShadowBolt.MaxRange);
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