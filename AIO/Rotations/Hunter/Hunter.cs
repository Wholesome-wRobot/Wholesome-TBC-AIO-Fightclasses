using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Hunter
{
    public class Hunter : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }
        public static DateTime LastAuto { get; set; }

        public static HunterSettings settings;

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected HunterFoodManager _foodManager = new HunterFoodManager();
        protected BackgroundWorker _petPulseThread = new BackgroundWorker();

        protected Cast cast;

        protected bool _autoshotRepeating;
        protected bool RangeCheck;
        protected int _backupAttempts = 0;
        protected int _steadyShotSleep = 0;
        protected bool _canOnlyMelee = false;
        protected int _saveDrinkPercent = wManager.wManagerSetting.CurrentSetting.DrinkPercent;

        protected Hunter specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = HunterSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);

            AIOSpell baseSpell = SerpentSting.KnownSpell ? SerpentSting : RaptorStrike;
            cast = new Cast(baseSpell, null, settings);

            this.specialization = specialization as Hunter;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            _petPulseThread.DoWork += PetThread;
            _petPulseThread.RunWorkerAsync();

            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightLoop += FightLoopHandler;
            MovementEvents.OnMovementPulse += MovementEventsOnMovementPulse;
            
            Rotation();
        }

        public void Dispose()
        {
            wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;
            _petPulseThread.DoWork -= PetThread;
            _petPulseThread.Dispose();
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            cast.Dispose();
            Logger.Log("Disposed");
        }

        // Pet thread
        private void PetThread(object sender, DoWorkEventArgs args)
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && !Me.IsOnTaxi 
                        && ObjectManager.Pet.IsValid
                        && ObjectManager.Pet.IsAlive
                        && !Me.IsMounted)
                    {
                        // OOC
                        if (!Fight.InFight
                            && !Me.InCombatFlagOnly)
                        {
                            // Feed
                            if (Lua.LuaDoString<int>("happiness, damagePercentage, loyaltyRate = GetPetHappiness() return happiness", "") < 3
                                && settings.FeedPet)
                                Feed();

                            // Switch Auto Growl
                            if (ObjectManager.Pet.IsValid
                                && ToolBox.PetSpellIsAutocast("Growl") != settings.AutoGrowl)
                                ToolBox.TogglePetSpellAuto("Growl", settings.AutoGrowl);

                            ToolBox.TogglePetSpellAuto("Charge", true);
                        }

                        // In fight
                        if ((Fight.InFight || Me.InCombatFlagOnly)
                            && !ObjectManager.Pet.HaveBuff("Feed Pet Effect")
                            && Me.Target > 0UL)
                        {
                            bool multiAggroImTargeted = false;

                            // Pet Switch target on multi aggro
                            if (Me.InCombatFlagOnly
                                && !(specialization is BeastMasteryParty)
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
                                        cast.PetSpell("Growl", true);
                                        Lua.LuaDoString("ClearFocus();");
                                    }
                                }
                            }

                            // Pet attack on single aggro
                            if ((Me.InCombatFlagOnly || Fight.InFight)
                                && Me.Target > 0
                                && !multiAggroImTargeted)
                                Lua.LuaDoString("PetAttack();", false);

                            // Pet Growl
                            if ((ObjectManager.Target.Target == Me.Guid || ObjectManager.Pet.Target != Me.Target) 
                                && !settings.AutoGrowl
                                && !(specialization is BeastMasteryParty))
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
            while (Main.isLaunched)
            {
                try
                {
                    if (Me.HaveBuff("Feign Death"))
                    {
                        Thread.Sleep(500);
                        Move.Backward(Move.MoveAction.PressKey, 100);
                        cast.OnTarget(AutoShot);
                    }

                    if (StatusChecker.BasicConditions()
                        && !Me.IsMounted
                        && !Me.HaveBuff("Food")
                        && !Me.HaveBuff("Drink"))
                    {
                        if (_canOnlyMelee)
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

        protected virtual void BuffRotation()
        {
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
        }

        protected void Feed()
        {
            if (ObjectManager.Pet.IsAlive 
                && !Me.IsCast 
                && !ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
            {
                if (!ToolBox.HasPoisonDebuff("pet"))
                {
                    _foodManager.FeedPet();
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
            if (!Me.HaveBuff("Drink")
                && !Me.HaveBuff("Food"))
            {
                // Call Pet
                if (!ObjectManager.Pet.IsValid)
                    cast.OnSelf(CallPet);

                // Make sure we have mana to revive
                if ((!ObjectManager.Pet.IsAlive || !ObjectManager.Pet.IsValid)
                    && !Me.InCombatFlagOnly
                    && RevivePet.KnownSpell
                    && !Me.HaveBuff("Drink")
                    && RevivePet.Cost > Me.Mana)
                {
                    Logger.Log("Not enough mana to summon, forcing regen");
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = 95;
                    Thread.Sleep(1000);
                    return;
                }
                else
                    wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;

                // Revive Pet
                if ((!ObjectManager.Pet.IsAlive || !ObjectManager.Pet.IsValid)
                    && !Me.HaveBuff("Drink")
                    && (!Me.InCombatFlagOnly || specialization.RotationType == Enums.RotationType.Solo)
                    && cast.OnSelf(RevivePet))
                    return;

                // Mend Pet
                if (ObjectManager.Pet.IsAlive
                    && ObjectManager.Pet.IsValid
                    && !ObjectManager.Pet.HaveBuff("Mend Pet")
                    && !Me.InCombatFlagOnly
                    && Me.IsAlive
                    && ObjectManager.Pet.HealthPercent <= 60
                    && cast.OnFocusUnit(MendPet, ObjectManager.Pet))
                    return;
            }
        }

        protected bool RaptorStrikeOn()
        {
            return Lua.LuaDoString<bool>("isAutoRepeat = false; if IsCurrentSpell('Raptor Strike') then isAutoRepeat = true end", "isAutoRepeat");
        }

        protected void ReenableAutoshot()
        {
            _autoshotRepeating = Lua.LuaDoString<bool>("isAutoRepeat = false; local name = GetSpellInfo(75); " +
                   "if IsAutoRepeatSpell(name) then isAutoRepeat = true end", "isAutoRepeat");
            if (!_autoshotRepeating
                && cast.OnTarget(AutoShot))
                Logger.LogDebug("Re-enabling auto shot");;
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

        // EVENT HANDLERS
        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            // Wait for feed pet
            if (ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
                Logger.Log("Waiting for pet to be fed");

            while (ObjectManager.Pet.HaveBuff("Feed Pet Effect")
                && !ObjectManager.Me.InCombatFlagOnly)
                Thread.Sleep(500);

            if (ObjectManager.Target.GetDistance >= 13f 
                && !AutoShot.IsSpellUsable 
                && !cast.IsBackingUp)
                _canOnlyMelee = true;
            else
                _canOnlyMelee = false;
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            cast.IsBackingUp = false;

            float minDistance = RangeManager.GetMeleeRangeWithTarget() + settings.BackupDistance;

            // Do we need to backup?
            if (ObjectManager.Target.GetDistance < minDistance
                && !ObjectManager.Target.IsTargetingMe
                && !MovementManager.InMovement
                && Me.IsAlive
                && ObjectManager.Target.IsAlive
                && !ObjectManager.Pet.HaveBuff("Pacifying Dust")
                && !_canOnlyMelee
                && !cast.IsApproachingTarget
                && !ObjectManager.Pet.IsStunned
                && !Me.IsCast
                && settings.BackupFromMelee
                && (!RaptorStrikeOn() || ObjectManager.Target.GetDistance > RangeManager.GetMeleeRangeWithTarget()))
            {
                // Stop trying if we reached the max amount of attempts
                if (_backupAttempts >= settings.MaxBackupAttempts)
                {
                    Logger.Log($"Backup failed after {_backupAttempts} attempts. Going in melee");
                    _canOnlyMelee = true;
                    RangeManager.SetRangeToMelee();
                    return;
                }

                cast.IsBackingUp = true;
                Timer timer = new Timer(3000);

                // Using CTM
                if (settings.BackupUsingCTM)
                {
                    Vector3 position = ToolBox.BackofVector3(Me.Position, Me, 12f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    Thread.Sleep(500);

                    // Backup loop
                    while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && ObjectManager.Me.IsAlive
                    && !ObjectManager.Target.IsTargetingMe
                    && ObjectManager.Target.GetDistance < minDistance
                    && !timer.IsReady)
                    Thread.Sleep(100);
                }
                // Using Keyboard
                else
                {
                    while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && ObjectManager.Me.IsAlive
                    && !ObjectManager.Target.IsTargetingMe
                    && ObjectManager.Target.GetDistance < minDistance
                    && !timer.IsReady)
                    {
                        Move.Backward(Move.MoveAction.PressKey, 500);
                    }
                }

                _backupAttempts++;
                Logger.Log($"Backup attempt : {_backupAttempts}");
                //Logger.Log($"FINAL We are {ObjectManager.Target.GetDistance}/{minDistance} away from target");
                cast.IsBackingUp = false;

                if (RaptorStrikeOn())
                    cast.OnTarget(RaptorStrike);
                ReenableAutoshot();
            }
        }

        private static void MovementEventsOnMovementPulse(List<Vector3> points, CancelEventArgs cancelable)
        {
            // Wait for feed pet
            if (ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
                Logger.Log("Waiting for pet to be fed");

            while (ObjectManager.Pet.HaveBuff("Feed Pet Effect")
                && !ObjectManager.Me.InCombatFlagOnly)
                Thread.Sleep(500);
        }

        private void FightEndHandler(ulong guid)
        {
            cast.IsBackingUp = false;
            _backupAttempts = 0;
            _autoshotRepeating = false;
            _canOnlyMelee = false;
        }
    }
}