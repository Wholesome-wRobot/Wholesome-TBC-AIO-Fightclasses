using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Hunter
{
    public class Hunter : BaseRotation
    {
        protected HunterSettings settings;
        protected Hunter specialization;
        protected HunterFoodManager foodManager = new HunterFoodManager();
        protected BackgroundWorker petPulseThread = new BackgroundWorker();
        protected bool autoshotRepeating;
        protected bool rangeCheck;
        protected int backupAttempts = 0;
        protected int steadyShotSleep = 0;
        protected bool canOnlyMelee = false;
        protected int saveDrinkPercent = wManager.wManagerSetting.CurrentSetting.DrinkPercent;

        public static DateTime LastAuto { get; set; }
        public static bool PetIsDead { get; set; }

        public Hunter(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Hunter;
            settings = HunterSettings.Current;
            AIOSpell baseSpell = SerpentSting.KnownSpell ? SerpentSting : RaptorStrike;
            BaseInit(28, baseSpell, null, settings);

            petPulseThread.DoWork += PetThread;
            petPulseThread.RunWorkerAsync();
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
            MovementEvents.OnMovementPulse += MovementEventsOnMovementPulse;

            Rotation();
        }

        public override void Dispose()
        {
            wManager.wManagerSetting.CurrentSetting.DrinkPercent = saveDrinkPercent;
            petPulseThread.DoWork -= PetThread;
            petPulseThread.Dispose();
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;

            BaseDispose();
        }

        public override bool AnswerReadyCheck()
        {
            return true;
        }

        // Pet thread
        private void PetThread(object sender, DoWorkEventArgs args)
        {
            while (Main.IsLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && !Me.IsOnTaxi
                        && Pet.IsValid
                        && Pet.IsAlive
                        && !Me.IsMounted)
                    {
                        // OOC
                        if (!Fight.InFight && !Me.InCombatFlagOnly)
                        {
                            // Feed
                            if (WTPet.PetHappiness < 3
                                && settings.FeedPet)
                                Feed();

                            // Switch Auto Growl
                            int growlIndex = WTPet.GetPetSpellIndex("Growl");
                            WTPet.TogglePetSpellAuto(growlIndex, settings.AutoGrowl);

                            int chargeIndex = WTPet.GetPetSpellIndex("Charge");
                            WTPet.TogglePetSpellAuto(chargeIndex, true);
                        }

                        // In fight
                        if ((Fight.InFight || Me.InCombatFlagOnly)
                            && Me.HasTarget
                            && Target.IsAlive
                            && !Pet.HasBuff("Feed Pet Effect"))
                        {
                            bool multiAggroImTargeted = false;

                            // Pet Switch target on multi aggro
                            if (Me.InCombatFlagOnly
                                && RotationType != Enums.RotationType.Party
                                && unitCache.EnemiesAttackingMe.Count > 1)
                            {
                                Lua.LuaDoString("PetDefensiveMode();");
                                // Get list of units targeting me in a multiaggro situation
                                List<IWoWUnit> unitsAttackingMe = unitCache.EnemiesAttackingMe
                                    .OrderBy(u => u.Guid)
                                    .Where(u => u.TargetGuid == Me.Guid)
                                    .ToList();

                                foreach (IWoWUnit unit in unitsAttackingMe)
                                {
                                    multiAggroImTargeted = true;
                                    if (unit.Guid != Pet.TargetGuid
                                        && Pet.GetTargetObject.TargetGuid == Pet.Guid)
                                    {
                                        Logger.Log($"Forcing pet aggro on {unit.Name}");
                                        Me.SetFocus(unit.Guid);
                                        cast.PetSpell("PET_ACTION_ATTACK", true);
                                        cast.PetSpell("Growl", true);
                                        Lua.LuaDoString("ClearFocus();");
                                    }
                                }
                            }

                            // Pet attack on single aggro
                            if ((Me.InCombatFlagOnly || Fight.InFight)
                                && Me.HasTarget
                                && Target.IsAlive
                                && !multiAggroImTargeted)
                                Lua.LuaDoString("PetAttack();", false);

                            // Pet Growl
                            if ((Target.TargetGuid == Me.Guid || Pet.Target != Me.Target)
                                && !settings.AutoGrowl
                                && RotationType != Enums.RotationType.Party)
                                if (cast.PetSpell("Growl"))
                                    continue;

                            // Pet damage spells
                            if (cast.PetSpellIfEnoughForGrowl("Bite", 35))
                                continue;
                            if (cast.PetSpellIfEnoughForGrowl("Gore", 25))
                                continue;
                            if (cast.PetSpellIfEnoughForGrowl("Scorpid Poison", 30))
                                continue;
                            if (cast.PetSpellIfEnoughForGrowl("Claw", 25))
                                continue;
                            if (cast.PetSpellIfEnoughForGrowl("Screech", 20))
                                continue;
                            if (cast.PetSpellIfEnoughForGrowl("Lightning Breath", 50))
                                continue;
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
                    if (Me.HasAura(FeignDeath))
                    {
                        Thread.Sleep(500);
                        Move.Backward(Move.MoveAction.PressKey, 100);
                        cast.OnTarget(AutoShot);
                    }

                    if (StatusChecker.BasicConditions()
                        && !Me.IsMounted
                        && !Me.HasBuff("Food")
                        && !Me.HasBuff("Drink"))
                    {
                        if (canOnlyMelee)
                            RangeManager.SetRangeToMelee();
                        else
                            RangeManager.SetRange(AutoShot.MaxRange - 1);

                        if (Me.Level >= 10)
                            PetManager();
                    }

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

        protected override void BuffRotation() { }
        protected override void Pull() { }
        protected override void CombatRotation() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

        protected void Feed()
        {
            if (Pet.IsAlive
                && !Me.IsCast
                && !Pet.HasBuff("Feed Pet Effect"))
            {
                if (!WTEffects.HasPoisonDebuff("pet"))
                {
                    foodManager.FeedPet();
                    Thread.Sleep(400);
                }
                else
                {
                    Logger.Log("You pet is poisoned, delaying feed");
                    Thread.Sleep(1000);
                }
            }
        }

        protected void PetManager()
        {
            if (!Me.HasBuff("Drink")
                && !Me.HasBuff("Food"))
            {
                // Set pet dead flag
                if (Pet.IsAlive)
                    PetIsDead = false;

                // Call Pet
                if (!Pet.IsValid && !PetIsDead)
                    cast.OnSelf(CallPet);

                // Make sure we have mana to revive
                if ((PetIsDead || Pet.IsDead)
                    && !Me.InCombatFlagOnly
                    && RevivePet.KnownSpell
                    && !Me.HasBuff("Drink")
                    && RevivePet.Cost > Me.Mana)
                {
                    Logger.Log("Not enough mana to summon, forcing regen");
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 95;
                    Thread.Sleep(1000);
                    return;
                }
                else
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = saveDrinkPercent;

                // Revive Pet
                if ((PetIsDead || Pet.IsDead)
                    && !Me.HasBuff("Drink")
                    && (!Me.InCombatFlagOnly || settings.RevivePetInCombat)
                    && cast.OnSelf(RevivePet))
                    return;

                // Mend Pet
                if (Pet.IsAlive
                    && Pet.IsValid
                    && !Pet.HasAura(MendPet)
                    && !Me.InCombatFlagOnly
                    && Me.IsAlive
                    && Pet.HealthPercent <= 60
                    && cast.OnFocusUnit(MendPet, Pet))
                    return;
            }
        }

        protected void ReenableAutoshot()
        {
            if (!WTCombat.IsSpellRepeating("Auto Shot")
                && cast.OnTarget(AutoShot))
                Logger.LogDebug("Re-enabling auto shot"); ;
        }

        protected AIOSpell RevivePet = new AIOSpell("Revive Pet");
        protected AIOSpell CallPet = new AIOSpell("Call Pet");
        protected AIOSpell MendPet = new AIOSpell("Mend Pet");
        protected AIOSpell AspectHawk = new AIOSpell("Aspect of the Hawk");
        protected AIOSpell AspectCheetah = new AIOSpell("Aspect of the Cheetah");
        protected AIOSpell AspectMonkey = new AIOSpell("Aspect of the Monkey");
        protected AIOSpell AspectViper = new AIOSpell("Aspect of the Viper");
        protected AIOSpell HuntersMark = new AIOSpell("Hunter's Mark");
        protected AIOSpell ConcussiveShot = new AIOSpell("Concussive Shot");
        protected AIOSpell RaptorStrike = new AIOSpell("Raptor Strike");
        protected AIOSpell MongooseBite = new AIOSpell("Mongoose Bite");
        protected AIOSpell WingClip = new AIOSpell("Wing Clip");
        protected AIOSpell SerpentSting = new AIOSpell("Serpent Sting");
        protected AIOSpell ArcaneShot = new AIOSpell("Arcane Shot");
        protected AIOSpell AutoShot = new AIOSpell("Auto Shot");
        protected AIOSpell RapidFire = new AIOSpell("Rapid Fire");
        protected AIOSpell Intimidation = new AIOSpell("Intimidation");
        protected AIOSpell BestialWrath = new AIOSpell("Bestial Wrath");
        protected AIOSpell FeignDeath = new AIOSpell("Feign Death");
        protected AIOSpell FreezingTrap = new AIOSpell("Freezing Trap");
        protected AIOSpell SteadyShot = new AIOSpell("Steady Shot");
        protected AIOSpell KillCommand = new AIOSpell("Kill Command");
        protected AIOSpell Disengage = new AIOSpell("Disengage");
        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell MultiShot = new AIOSpell("Multi-Shot");

        // EVENT HANDLERS
        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            // Wait for feed pet
            if (Pet.HasBuff("Feed Pet Effect"))
                Logger.Log("Waiting for pet to be fed");

            while (Pet.HasBuff("Feed Pet Effect")
                && !unitCache.Me.InCombatFlagOnly)
                Thread.Sleep(500);

            if (Target.GetDistance >= 13f
                && !AutoShot.IsSpellUsable
                && !cast.IsBackingUp)
                canOnlyMelee = true;
            else
                canOnlyMelee = false;
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            cast.IsBackingUp = false;

            float minDistance = RangeManager.GetMeleeRangeWithTarget() + settings.BackupDistance;

            // Do we need to backup?
            if (Target.GetDistance < minDistance
                && !Target.IsTargetingMe
                && !MovementManager.InMovement
                && Me.IsAlive
                && Target.IsAlive
                && !Pet.HasBuff("Pacifying Dust")
                && !canOnlyMelee
                && !cast.IsApproachingTarget
                && !Pet.IsStunned
                && !Me.IsCast
                && settings.BackupFromMelee
                && (!WTCombat.IsSpellActive("Raptor Strike") || Target.GetDistance > RangeManager.GetMeleeRangeWithTarget()))
            {
                // Stop trying if we reached the max amount of attempts
                if (backupAttempts >= settings.MaxBackupAttempts)
                {
                    Logger.Log($"Backup failed after {backupAttempts} attempts. Going in melee");
                    canOnlyMelee = true;
                    RangeManager.SetRangeToMelee();
                    return;
                }

                cast.IsBackingUp = true;
                Timer timer = new Timer(3000);

                // Using CTM
                if (settings.BackupUsingCTM)
                {
                    Vector3 position = WTSpace.BackOfUnit(Me.WowUnit, 12f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    Thread.Sleep(500);

                    // Backup loop
                    while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && Me.IsAlive
                    && !Target.IsTargetingMe
                    && Target.GetDistance < minDistance + 1
                    && !timer.IsReady)
                        Thread.Sleep(100);
                }
                // Using Keyboard
                else
                {
                    while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && Me.IsAlive
                    && !Target.IsTargetingMe
                    && Target.GetDistance < minDistance + 1
                    && !timer.IsReady)
                    {
                        Move.Backward(Move.MoveAction.PressKey, 500);
                    }
                }

                backupAttempts++;
                Logger.Log($"Backup attempt : {backupAttempts}");
                //Logger.Log($"FINAL We are {ObjectManager.Target.GetDistance}/{minDistance} away from target");
                cast.IsBackingUp = false;

                if (WTCombat.IsSpellActive("Raptor Strike"))
                    cast.OnTarget(RaptorStrike);
                ReenableAutoshot();
            }
        }

        private void MovementEventsOnMovementPulse(List<Vector3> points, CancelEventArgs cancelable)
        {
            // Wait for feed pet
            if (Pet.HasBuff("Feed Pet Effect"))
                Logger.Log("Waiting for pet to be fed");

            while (Pet.HasBuff("Feed Pet Effect")
                && !Me.InCombatFlagOnly)
                Thread.Sleep(500);
        }

        private void FightEndHandler(ulong guid)
        {
            cast.IsBackingUp = false;
            backupAttempts = 0;
            autoshotRepeating = false;
            canOnlyMelee = false;
        }
    }
}