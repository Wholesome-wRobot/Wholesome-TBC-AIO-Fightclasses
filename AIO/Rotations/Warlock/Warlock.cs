﻿using robotManager.Helpful;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class Warlock : BaseRotation
    {
        protected WarlockSettings settings;
        protected Warlock specialization;
        protected BackgroundWorker _petPulseThread = new BackgroundWorker();
        protected Stopwatch _addCheckTimer = new Stopwatch();
        protected int _innerManaSaveThreshold = 20;
        protected bool _iCanUseWand = WTGear.HaveRangedWeaponEquipped;
        protected int _saveDrinkPercent = wManager.wManagerSetting.CurrentSetting.DrinkPercent;

        public Warlock(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Warlock;
            settings = WarlockSettings.Current;
            BaseInit(ShadowBolt.MaxRange, ShadowBolt, UseWand, settings);
            WarlockPetAndConsumables.Setup();

            _petPulseThread.DoWork += PetThread;
            _petPulseThread.RunWorkerAsync();

            // Set pet mode
            if (settings.PetInPassiveWhenOOC)
                Lua.LuaDoString("PetPassiveMode();");
            else
                Lua.LuaDoString("PetDefensiveMode();");

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;

            Rotation();
        }

        public override void Dispose()
        {
            _petPulseThread.DoWork -= PetThread;
            _petPulseThread.Dispose();
            Lua.LuaDoString("PetPassiveMode();");
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;

            BaseDispose();
        }

        public override bool AnswerReadyCheck()
        {
            return true;
        }

        // Pet thread
        protected void PetThread(object sender, DoWorkEventArgs args)
        {
            while (Main.IsLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && Pet.IsValid
                        && Pet.IsAlive)
                    {
                        bool multiAggroImTargeted = false;
                        // Pet Switch target on multi aggro
                        if (Me.InCombatFlagOnly
                            && unitCache.EnemiesAttackingMe.Count > 1)
                        {
                            Lua.LuaDoString("PetDefensiveMode();");

                            foreach (IWoWUnit unit in unitCache.EnemiesAttackingMe)
                            {
                                multiAggroImTargeted = true;
                                if (unit.Guid != Pet.TargetGuid)
                                {
                                    // first check if pet has aggro on his current target
                                    IWoWUnit petTarget = unitCache.EnemiesFighting.Find(enemy => enemy.Guid == Pet.TargetGuid);
                                    if (petTarget != null && petTarget.TargetGuid == Pet.Guid)
                                    {
                                        Logger.Log($"Forcing pet aggro on {unit.Name}");
                                        Me.SetFocus(unit.Guid);
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
                        }

                        // Pet attack on single aggro
                        if ((Me.InCombatFlagOnly || Fight.InFight)
                            && Me.Target > 0
                            && !multiAggroImTargeted)
                            Lua.LuaDoString("PetAttack();");

                        // Voidwalker Torment + Felguard Anguish
                        if ((!settings.AutoTorment || !settings.AutoAnguish)
                            && Target.TargetGuid == Me.Guid
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
                            int tormentIndex = WTPet.GetPetSpellIndex("Torment");
                            WTPet.TogglePetSpellAuto(tormentIndex, settings.AutoTorment);
                            int sufferingIndex = WTPet.GetPetSpellIndex("Suffering");
                            WTPet.TogglePetSpellAuto(sufferingIndex, false);
                        }

                        // Switch Felguard Auto Cleave/Anguish
                        if (WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard") && specialization.RotationType == Enums.RotationType.Solo)
                        {
                            int cleaveIndex = WTPet.GetPetSpellIndex("Cleave");
                            WTPet.TogglePetSpellAuto(cleaveIndex, settings.FelguardCleave);
                            int anguishIndex = WTPet.GetPetSpellIndex("Anguish");
                            WTPet.TogglePetSpellAuto(anguishIndex, settings.AutoAnguish);
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
            while (Main.IsLaunched)
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

        protected override void BuffRotation()
        {
            // Delete additional Soul Shards
            if (WTItem.CountItemStacks("Soul Shard") > settings.NumberOfSoulShards)
            {
                Logger.Log("Deleting excess Soul Shard");
                WTItem.DeleteItemByName("Soul Shard");
            }

            // Define the demon to summon
            AIOSpell SummonSpell = null;
            bool shouldSummon = false;
            if (SummonImp.KnownSpell)
            {
                if (WTItem.CountItemStacks("Soul Shard") < 1 || !SummonVoidwalker.KnownSpell && !SummonFelguard.KnownSpell)
                    SummonSpell = SummonImp;

                if (SummonVoidwalker.KnownSpell && !SummonFelguard.KnownSpell)
                    SummonSpell = SummonVoidwalker;

                if (specialization.RotationType == Enums.RotationType.Party)
                    SummonSpell = SummonImp;

                if (SummonFelguard.KnownSpell)
                    SummonSpell = SummonFelguard;

                if (!Pet.IsValid
                    || Pet.ManaPercentage < settings.ManaThresholdResummon && SummonSpell != SummonImp
                    || Pet.HealthPercent < settings.HealthThresholdResummon
                    || !SummonSpell.Name.Contains(WarlockPetAndConsumables.MyWarlockPet()))
                    shouldSummon = true;
            }

            if (shouldSummon)
            {
                // Make sure we have mana to summon
                if (Me.Mana < SummonSpell.Cost
                    && !Me.HasDrinkBuff
                    && !Me.InCombatFlagOnly)
                {
                    Logger.Log($"Not enough mana to summon {SummonSpell.Name}, forcing regen");
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 95;
                    Thread.Sleep(1000);
                    return;
                }

                Thread.Sleep(Usefuls.Latency + 500); // Safety for Mount check
                if (!Me.IsMounted && !Me.IsOnTaxi)
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
        protected override void Pull() { }
        protected override void CombatRotation() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

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
            {
                int fireboltIndex = WTPet.GetPetSpellIndex("Firebolt");
                WTPet.TogglePetSpellAuto(fireboltIndex, true);
                int bloodPactIndex = WTPet.GetPetSpellIndex("Blood Pact");
                WTPet.TogglePetSpellAuto(bloodPactIndex, true);
            }
        }
    }
}