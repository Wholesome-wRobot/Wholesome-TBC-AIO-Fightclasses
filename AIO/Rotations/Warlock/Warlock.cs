using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
        protected bool IsWotlk = WTLua.GetWoWVersion.StartsWith("3");

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

        // Pet thread
        protected void PetThread(object sender, DoWorkEventArgs args)
        {
            while (Main.IsLaunched)
            {
                Thread.Sleep(300);
                try
                {
                    if (StatusChecker.BasicConditions()
                        && !Me.IsOnTaxi
                        && Pet.IsValid
                        && Pet.IsAlive
                        && !Me.IsMounted)
                    {
                        // In fight
                        List<IWoWUnit> enemiesFighting = new List<IWoWUnit>(
                                unitCache.EnemiesFighting
                                    .OrderBy(unit => unit.PositionWithoutType.DistanceTo(Me.PositionWithoutType))
                            );
                        if (enemiesFighting.Count > 0)
                        {
                            IWoWUnit petTarget = enemiesFighting.Find(unit => unit.Guid == Pet.Target);
                            IWoWUnit enemyTargetingMe = enemiesFighting.Find(unit => unit.Target == Me.Guid);
                            IWoWUnit myTarget = enemiesFighting.Find(unit => Me.Target == unit.Guid);

                            // Not attacking anything, select closest
                            if (petTarget == null)
                            {
                                IWoWUnit unitToAttack = enemiesFighting.FirstOrDefault();
                                if (unitToAttack != null)
                                {
                                    PetAttackFocus(unitToAttack);
                                    continue;
                                }
                            }

                            // Switch to regain aggro
                            if (enemyTargetingMe != null
                                && petTarget.TargetGuid == Pet.Guid
                                && petTarget.Guid != enemyTargetingMe.Guid)
                            {
                                PetAttackFocus(enemyTargetingMe);
                                continue;
                            }

                            // Switch to attack my target
                            if (myTarget != null
                                && enemyTargetingMe == null
                                && petTarget.Guid != myTarget.Guid)
                            {
                                PetAttackFocus(myTarget);
                                continue;
                            }

                            // Pet torment/anguish
                            if (petTarget != null
                                && petTarget.Target != Pet.Guid
                                && RotationType != Enums.RotationType.Party)
                            {
                                ObjectManager.Me.FocusGuid = petTarget.Guid;
                                if (!settings.AutoTorment
                                    && WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                                {
                                    cast.PetSpell("Torment", true);
                                    cast.PetSpell("Suffering", true);
                                }
                                if (!settings.AutoAnguish
                                    && WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard"))
                                {
                                    cast.PetSpell("Anguish", true);
                                }
                                Lua.LuaDoString("ClearFocus();");
                            }
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
            }
        }

        private void PetAttackFocus(IWoWUnit unit)
        {
            Me.SetFocus(unit.Guid);
            Lua.LuaDoString($"CastPetAction(1, \"focus\")");
            Lua.LuaDoString("ClearFocus();");
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
            if (WTItem.CountItemStacks("Soul Shard") > settings.CommonNumberOfSoulShards)
            {
                Logger.Log("Deleting excess Soul Shard");
                WTItem.DeleteItemByName("Soul Shard");
            }

            // Define the demon to summon
            AIOSpell SummonSpell = null;
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
            }

            if (SummonSpell != null)
            {
                if (!Pet.IsValid
                    || Pet.ManaPercentage < settings.ManaThresholdResummon && SummonSpell != SummonImp
                    || Pet.HealthPercent < settings.HealthThresholdResummon
                    || !SummonSpell.Name.Contains(WarlockPetAndConsumables.MyWarlockPet()))
                {
                    // Make sure we have mana to summon
                    if (Me.Mana < SummonSpell.Cost
                        && !Me.HasDrinkAura
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
                        {
                            Thread.Sleep(200);
                        }
                        if (cast.OnSelf(SummonSpell))
                        {
                            Usefuls.WaitIsCasting();
                            Thread.Sleep(1000); // Prevent double summon
                            return;
                        }
                    }
                }
                else
                {
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;
                }
            }
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
            {
                _iCanUseWand = true;
            }

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