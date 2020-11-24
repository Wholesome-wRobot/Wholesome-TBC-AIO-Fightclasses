using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Hunter
{
    public class Hunter : IClassRotation
    {
        public static HunterSettings settings;

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected HunterFoodManager _foodManager = new HunterFoodManager();
        protected BackgroundWorker _petPulseThread = new BackgroundWorker();

        protected Cast cast;

        protected float _distanceRange = 28f;
        protected bool _autoshotRepeating;
        protected bool RangeCheck;
        protected int _backupAttempts = 0;
        protected int _steadyShotSleep = 0;
        protected bool _canOnlyMelee = false;
        protected int _saveDrinkPercent = wManager.wManagerSetting.CurrentSetting.DrinkPercent;
        public static bool haveTamedAPet = true;

        DateTime lastAuto;

        protected Hunter specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = HunterSettings.Current;
            cast = new Cast(RaptorStrike, settings.ActivateCombatDebug, null);

            this.specialization = specialization as Hunter;
            TalentsManager.InitTalents(settings);

            _petPulseThread.DoWork += PetThread;
            _petPulseThread.RunWorkerAsync();

            EventsLuaWithArgs.OnEventsLuaWithArgs += AutoShotEventHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            Rotation();
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
                            if (ObjectManager.Target.Target == Me.Guid
                                && !settings.AutoGrowl
                                && !multiAggroImTargeted)
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

        public void Dispose()
        {
            wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;
            _petPulseThread.DoWork -= PetThread;
            _petPulseThread.Dispose();
            EventsLuaWithArgs.OnEventsLuaWithArgs -= AutoShotEventHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && !Me.IsMounted)
                    {
                        if (_canOnlyMelee)
                            RangeManager.SetRangeToMelee();
                        else
                            RangeManager.SetRange(_distanceRange);

                        if (Me.Level > 10)
                            PetManager();
                    }

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
            // Aspect of the Cheetah
            if (!Me.IsMounted 
                && !Me.HaveBuff("Aspect of the Cheetah")
                && MovementManager.InMoveTo
                && Me.ManaPercentage > 60
                && settings.UseAspectOfTheCheetah)
                if (cast.Normal(AspectCheetah))
                    return;
        }

        protected virtual void Pull()
        {
            // Hunter's Mark
            if (ObjectManager.Pet.IsValid
                && !HuntersMark.TargetHaveBuff
                && ObjectManager.Target.GetDistance > 13f
                && ObjectManager.Target.IsAlive)
                if (cast.Normal(HuntersMark))
                    return;
        }

        protected virtual void CombatRotation()
        {
            double lastAutoInMilliseconds = (DateTime.Now - lastAuto).TotalMilliseconds;

            WoWUnit Target = ObjectManager.Target;

            if (Target.GetDistance < 10f 
                && !cast.IsBackingUp)
                ToolBox.CheckAutoAttack(Attack);

            if (Target.GetDistance > 10f 
                && !cast.IsBackingUp)
                ReenableAutoshot();

            if (Target.GetDistance < 13f 
                && !settings.BackupFromMelee)
                _canOnlyMelee = true;

            // Mend Pet
            if (ObjectManager.Pet.IsAlive
                && ObjectManager.Pet.HealthPercent <= 50
                && !ObjectManager.Pet.HaveBuff("Mend Pet"))
                if (cast.Normal(MendPet))
                    return;

            // Aspect of the viper
            if (!Me.HaveBuff("Aspect of the Viper") 
                && Me.ManaPercentage < 30)
                if (cast.Normal(AspectViper))
                    return;

            // Aspect of the Hawk
            if (!Me.HaveBuff("Aspect of the Hawk")
                && (Me.ManaPercentage > 90 || Me.HaveBuff("Aspect of the Cheetah"))
                || !Me.HaveBuff("Aspect of the Hawk") 
                && !Me.HaveBuff("Aspect of the Cheetah") 
                && !Me.HaveBuff("Aspect of the Viper"))
                if (cast.Normal(AspectHawk))
                    return;

            // Aspect of the Monkey
            if (!Me.HaveBuff("Aspect of the Monkey") 
                && !AspectHawk.KnownSpell)
                if (cast.Normal(AspectMonkey))
                    return;

            // Disengage
            if (settings.UseDisengage
                && ObjectManager.Pet.Target == Me.Target 
                && Target.Target == Me.Guid 
                && Target.GetDistance < 10)
                if (cast.Normal(Disengage))
                    return;

            // Bestial Wrath
            if (Target.GetDistance < 34f 
                && Target.HealthPercent >= 60 
                && Me.ManaPercentage > 10 
                && BestialWrath.IsSpellUsable
                && (settings.BestialWrathOnMulti && ObjectManager.GetUnitAttackPlayer().Count > 1 || !settings.BestialWrathOnMulti))
                if (cast.Normal(BestialWrath))
                    return;

            // Rapid Fire
            if (Target.GetDistance < 34f 
                && Target.HealthPercent >= 80.0
                && (settings.RapidFireOnMulti && ObjectManager.GetUnitAttackPlayer().Count > 1 || !settings.RapidFireOnMulti))
                if (cast.Normal(RapidFire))
                    return;

            // Kill Command
            if (cast.Normal(KillCommand))
                return;

            // Raptor Strike
            if (Target.GetDistance < 6f 
                && !RaptorStrikeOn())
                if (cast.Normal(RaptorStrike))
                    return;

            // Mongoose Bite
            if (Target.GetDistance < 6f)
                if (cast.Normal(MongooseBite))
                    return;

            // Feign Death
            if (Me.HealthPercent < 20
                || (ObjectManager.GetNumberAttackPlayer() > 1 && ObjectManager.GetUnitAttackPlayer().Where(u => u.Target == Me.Guid).Count() > 0))
                if (cast.Normal(FeignDeath))
                {
                    Thread.Sleep(500);
                    Move.Backward(Move.MoveAction.PressKey, 100);
                    return;
                }
            /*
            // Freezing Trap
            if (ObjectManager.Pet.HaveBuff("Mend Pet") 
                && ObjectManager.GetUnitAttackPlayer().Count > 1 
                && settings.UseFreezingTrap)
                if (cast.Normal(FreezingTrap))
                    return;
            */
            // Concussive Shot
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.UseConcussiveShot
                && ConcussiveShot.IsDistanceGood
                && Target.HealthPercent < 20
                && !Target.HaveBuff("Concussive Shot"))
                if (cast.Normal(ConcussiveShot))
                    return;

            // Wing Clip
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.UseConcussiveShot
                && WingClip.IsDistanceGood
                && Target.HealthPercent < 20
                && !Target.HaveBuff("Wing Clip"))
                if (cast.Normal(WingClip))
                    return;

            // Hunter's Mark
            if (ObjectManager.Pet.IsValid 
                && !HuntersMark.TargetHaveBuff 
                && Target.GetDistance > 13f 
                && Target.IsAlive)
                if (cast.Normal(HuntersMark))
                    return;

            // Steady Shot
            if (lastAutoInMilliseconds > 100
                && lastAutoInMilliseconds < 500
                && SteadyShot.KnownSpell 
                && SteadyShot.IsSpellUsable 
                && Me.ManaPercentage > 30 
                && SteadyShot.IsDistanceGood)
                if (cast.Normal(SteadyShot))
                    return;

            // Serpent Sting
            if (!Target.HaveBuff("Serpent Sting")
                && Target.GetDistance < 34f
                && Target.HealthPercent >= 60
                && Me.ManaPercentage > 50u
                && !SteadyShot.KnownSpell
                && Target.GetDistance > 13f)
                if (cast.Normal(SerpentSting))
                    return;

            // Intimidation
            if (Target.GetDistance < 34f 
                && Target.GetDistance > 10f 
                && Target.HealthPercent >= 20
                && Me.ManaPercentage > 10
                && !settings.IntimidationInterrupt)
                if (cast.Normal(Intimidation))
                    return;

            // Intimidation interrupt
            if (Target.GetDistance < 34f
                && ToolBox.EnemyCasting()
                && settings.IntimidationInterrupt)
                if (cast.Normal(Intimidation))
                    return;

            // Arcane Shot
            if (Target.GetDistance < 34f 
                && Target.HealthPercent >= 30 
                && Me.ManaPercentage > 80
                && ArcaneShot.IsDistanceGood
                && !SteadyShot.KnownSpell)
                if (cast.Normal(ArcaneShot))
                    return;
        }

        protected void Feed()
        {
            if (ObjectManager.Pet.IsAlive 
                && !Me.IsCast 
                && !ObjectManager.Pet.HaveBuff("Feed Pet Effect"))
            {
                _foodManager.FeedPet();
                Thread.Sleep(400);
            }
        }

        protected void PetManager()
        {
            // Call Pet
            if (!ObjectManager.Pet.IsValid
                && haveTamedAPet
                && CallPet.KnownSpell
                && CallPet.IsSpellUsable)
            {
                CallPet.Launch();
                Thread.Sleep(Usefuls.Latency + 2000);
            }

            // Make sure we have mana to revive
            if ((!ObjectManager.Pet.IsAlive || !ObjectManager.Pet.IsValid)
                && haveTamedAPet
                && !Me.InCombatFlagOnly
                && RevivePet.KnownSpell
                && !Me.HaveBuff("Drink")
                && ToolBox.GetSpellCost("Revive Pet") > Me.Mana)
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
                && haveTamedAPet
                && RevivePet.KnownSpell
                && RevivePet.IsSpellUsable)
            {
                RevivePet.Launch(true);
                Usefuls.WaitIsCasting();
            }

            // Mend Pet
            if (ObjectManager.Pet.IsAlive 
                && ObjectManager.Pet.IsValid 
                && !ObjectManager.Pet.HaveBuff("Mend Pet")
                && !Me.InCombatFlagOnly
                && Me.IsAlive 
                && MendPet.KnownSpell 
                && MendPet.IsDistanceGood 
                && ObjectManager.Pet.HealthPercent <= 60
                && MendPet.IsSpellUsable)
            {
                MendPet.Launch();
                Thread.Sleep(Usefuls.Latency + 1000);
            }
        }

        private bool RaptorStrikeOn()
        {
            return Lua.LuaDoString<bool>("isAutoRepeat = false; if IsCurrentSpell('Raptor Strike') then isAutoRepeat = true end", "isAutoRepeat");
        }

        private void ReenableAutoshot()
        {
            _autoshotRepeating = Lua.LuaDoString<bool>("isAutoRepeat = false; local name = GetSpellInfo(75); " +
                   "if IsAutoRepeatSpell(name) then isAutoRepeat = true end", "isAutoRepeat");
            if (!_autoshotRepeating)
            {
                Logger.LogDebug("Re-enabling auto shot");
                AutoShot.Launch();
            }
        }

        protected Spell RevivePet = new Spell("Revive Pet");
        protected Spell CallPet = new Spell("Call Pet");
        protected Spell MendPet = new Spell("Mend Pet");
        protected Spell AspectHawk = new Spell("Aspect of the Hawk");
        protected Spell AspectCheetah = new Spell("Aspect of the Cheetah");
        protected Spell AspectMonkey = new Spell("Aspect of the Monkey");
        protected Spell AspectViper = new Spell("Aspect of the Viper");
        protected Spell HuntersMark = new Spell("Hunter's Mark");
        protected Spell ConcussiveShot = new Spell("Concussive Shot");
        protected Spell RaptorStrike = new Spell("Raptor Strike");
        protected Spell MongooseBite = new Spell("Mongoose Bite");
        protected Spell WingClip = new Spell("Wing Clip");
        protected Spell SerpentSting = new Spell("Serpent Sting");
        protected Spell ArcaneShot = new Spell("Arcane Shot");
        protected Spell AutoShot = new Spell("Auto Shot");
        protected Spell RapidFire = new Spell("Rapid Fire");
        protected Spell Intimidation = new Spell("Intimidation");
        protected Spell BestialWrath = new Spell("Bestial Wrath");
        protected Spell FeignDeath = new Spell("Feign Death");
        protected Spell FreezingTrap = new Spell("Freezing Trap");
        protected Spell SteadyShot = new Spell("Steady Shot");
        protected Spell KillCommand = new Spell("Kill Command");
        protected Spell Disengage = new Spell("Disengage");
        protected Spell Attack = new Spell("Attack");

        // EVENT HANDLERS
        private void AutoShotEventHandler(LuaEventsId id, List<string> args)
        {
            if (id == LuaEventsId.COMBAT_LOG_EVENT && args[9] == "Auto Shot")
                lastAuto = DateTime.Now;

            // call pet when you don't have a pet => You do not have a pet
            // call pet when just dead => Not yet recovered
            // call pet when just died => You already control a summoned creature
            if (id == LuaEventsId.COMBAT_LOG_EVENT_UNFILTERED && args[11].Equals("You do not have a pet"))
                haveTamedAPet = false;
            else if (id == LuaEventsId.COMBAT_LOG_EVENT_UNFILTERED && args[11].Equals("Not yet recovered"))
                haveTamedAPet = true;
            else if (id == LuaEventsId.COMBAT_LOG_EVENT_UNFILTERED && args[11].Equals("You already control a summoned creature"))
                haveTamedAPet = true;
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
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

            // Do we need to backup?
            if (ObjectManager.Target.GetDistance < 8f + RangeManager.GetMeleeRangeWithTarget() 
                && ObjectManager.Target.IsTargetingMyPet
                && !MovementManager.InMovement
                && Me.IsAlive
                && ObjectManager.Target.IsAlive
                && !ObjectManager.Pet.HaveBuff("Pacifying Dust")
                && !_canOnlyMelee
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

                // Using CTM
                if (settings.BackupUsingCTM)
                {
                    Vector3 position = ToolBox.BackofVector3(Me.Position, Me, 12f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    Thread.Sleep(500);

                    // Backup loop
                    int limiter = 0;
                    while (MovementManager.InMoveTo
                    && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && ObjectManager.Me.IsAlive
                    && !ObjectManager.Target.IsTargetingMe
                    && ObjectManager.Target.GetDistance < 8f + RangeManager.GetMeleeRangeWithTarget()
                    && limiter < 10)
                    {
                        // Wait follow path
                        Thread.Sleep(300);
                        limiter++;
                    }
                }
                // Using Keyboard
                else
                {
                    int limiter = 0;
                    while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && ObjectManager.Me.IsAlive
                    && !ObjectManager.Target.IsTargetingMe
                    && ObjectManager.Target.GetDistance < 8f + RangeManager.GetMeleeRangeWithTarget()
                    && limiter <= 6)
                    {
                        Move.Backward(Move.MoveAction.PressKey, 500);
                        limiter++;
                    }
                }

                _backupAttempts++;
                Logger.Log($"Backup attempt : {_backupAttempts}");
                cast.IsBackingUp = false;

                if (RaptorStrikeOn())
                    cast.Normal(RaptorStrike);
                ReenableAutoshot();
            }
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