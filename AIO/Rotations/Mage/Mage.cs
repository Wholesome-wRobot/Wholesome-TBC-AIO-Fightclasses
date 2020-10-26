using System;
using System.Threading;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Events;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Mage : IClassRotation
    {
        public static MageSettings settings;

        protected MageFoodManager _foodManager = new MageFoodManager();

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected WoWUnit _polymorphedEnemy = null;

        protected float _distanceRange = 28f;
        protected bool _isBackingUp = false;
        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected bool _isPolymorphing;
        protected bool _polymorphableEnemyInThisFight = true;

        protected Mage specialization;

        public void Initialize(IClassRotation specialization)
        {
            Logger.Log("Initialized.");
            settings = MageSettings.Current;

            this.specialization = specialization as Mage;
            Talents.InitTalents(settings);

            _distanceRange = specialization is Fire ? 33f : _distanceRange;

            RangeManager.SetRange(_distanceRange);

            // Fight end
            FightEvents.OnFightEnd += (guid) =>
            {
                _isBackingUp = false;
                _iCanUseWand = false;
                _polymorphableEnemyInThisFight = false;
                _isPolymorphing = false;
                RangeManager.SetRange(_distanceRange);

                if (!Fight.InFight
                && Me.InCombatFlagOnly
                && _polymorphedEnemy != null
                && ObjectManager.GetNumberAttackPlayer() < 1
                && _polymorphedEnemy.IsAlive)
                {
                    Logger.Log($"Starting fight with {_polymorphedEnemy.Name} (polymorphed)");
                    Fight.InFight = false;
                    Fight.CurrentTarget = _polymorphedEnemy;
                    ulong _enemyGUID = _polymorphedEnemy.Guid;
                    _polymorphedEnemy = null;
                    Fight.StartFight(_enemyGUID);
                }
            };

            // Fight start
            FightEvents.OnFightStart += (unit, cancelable) =>
            {
                _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
            };

            // Fight Loop
            FightEvents.OnFightLoop += (unit, cancelable) =>
            {
                // Do we need to backup?
                if ((ObjectManager.Target.HaveBuff("Frostbite") || ObjectManager.Target.HaveBuff("Frost Nova"))
                    && ObjectManager.Target.GetDistance < 10f
                    && Me.IsAlive
                    && ObjectManager.Target.IsAlive
                    && !_isBackingUp
                    && !Me.IsCast
                    && !RangeManager.CurrentRangeIsMelee()
                    && ObjectManager.Target.HealthPercent > 5
                    && !_isPolymorphing)
                {
                    _isBackingUp = true;
                    int limiter = 0;

                    // Using CTM
                    if (settings.BackupUsingCTM)
                    {
                        Vector3 position = ToolBox.BackofVector3(Me.Position, Me, 15f);
                        MovementManager.Go(PathFinder.FindPath(position), false);
                        Thread.Sleep(500);

                        // Backup loop
                        while (MovementManager.InMoveTo
                            && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                            && ObjectManager.Target.GetDistance < 15f
                            && ObjectManager.Me.IsAlive
                            && ObjectManager.Target.IsAlive
                            && (ObjectManager.Target.HaveBuff("Frostbite") || ObjectManager.Target.HaveBuff("Frost Nova"))
                            && limiter < 10)
                        {
                            // Wait follow path
                            Thread.Sleep(300);
                            limiter++;
                            if (settings.BlinkWhenBackup)
                                Cast(Blink);
                        }
                        MovementManager.StopMove();
                        Thread.Sleep(500);
                    }
                    // Using Keyboard
                    else
                    {
                        while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                        && ObjectManager.Me.IsAlive
                        && ObjectManager.Target.IsAlive
                        && ObjectManager.Target.GetDistance < 15f
                        && limiter <= 6)
                        {
                            Move.Backward(Move.MoveAction.PressKey, 700);
                            limiter++;
                        }
                    }
                    _isBackingUp = false;
                }

                // Polymorph
                if (settings.UsePolymorph
                    && ObjectManager.GetNumberAttackPlayer() > 1
                    && Polymorph.KnownSpell
                    && !_isBackingUp
                    && _polymorphableEnemyInThisFight)
                {
                    WoWUnit myNearbyPolymorphed = null;
                    // Detect if a polymorph cast has succeeded
                    if (_polymorphedEnemy != null)
                        myNearbyPolymorphed = ObjectManager.GetObjectWoWUnit().Find(u => u.HaveBuff("Polymorph") && u.Guid == _polymorphedEnemy.Guid);

                    // If we don't have a polymorphed enemy
                    if (myNearbyPolymorphed == null)
                    {
                        _polymorphedEnemy = null;
                        _isPolymorphing = true;
                        WoWUnit firstTarget = ObjectManager.Target;
                        WoWUnit potentialPolymorphTarget = null;

                        // Select our attackers one by one for potential polymorphs
                        foreach (WoWUnit enemy in ObjectManager.GetUnitAttackPlayer())
                        {
                            Interact.InteractGameObject(enemy.GetBaseAddress);

                            if ((enemy.CreatureTypeTarget == "Beast" || enemy.CreatureTypeTarget == "Humanoid")
                            && enemy.Guid != firstTarget.Guid)
                            {
                                potentialPolymorphTarget = enemy;
                                break;
                            }
                        }

                        if (potentialPolymorphTarget == null)
                            _polymorphableEnemyInThisFight = false;

                        // Polymorph cast
                        if (potentialPolymorphTarget != null && _polymorphedEnemy == null)
                        {
                            Interact.InteractGameObject(potentialPolymorphTarget.GetBaseAddress);
                            while (!Cast(Polymorph)
                       && ObjectManager.Target.IsAlive
                       && ObjectManager.Me.IsAlive
                       && Main.isLaunched
                       && !Products.InPause)
                            {
                                Thread.Sleep(200);
                            }
                            _polymorphedEnemy = potentialPolymorphTarget;
                            Usefuls.WaitIsCasting();
                            Thread.Sleep(500);
                        }

                        // Get back to actual target
                        Interact.InteractGameObject(firstTarget.GetBaseAddress);
                        _isPolymorphing = false;
                    }
                }
            };

            Rotation();
        }

        public void Dispose()
        {
            _isBackingUp = false;
            Logger.Log("Stopped in progress.");
        }

        private void Rotation()
        {
            Logger.Log("Started");
            while (Main.isLaunched)
            {
                try
                {

                    if (StatusChecker.BasicConditions())
                    {
                        if (_polymorphedEnemy != null && !ObjectManager.Me.InCombatFlagOnly)
                            _polymorphedEnemy = null;
                    }

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat()
                        && !_isBackingUp
                        && !_isPolymorphing
                        && (!ObjectManager.Target.HaveBuff("Polymorph") || ObjectManager.GetNumberAttackPlayer() < 1))
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
            _foodManager.CheckIfEnoughFoodAndDrinks();
            _foodManager.CheckIfThrowFoodAndDrinks();
            _foodManager.CheckIfHaveManaStone();

            // Arcane Intellect
            if (!Me.HaveBuff("Arcane Intellect") && ArcaneIntellect.KnownSpell && ArcaneIntellect.IsSpellUsable)
            {
                Lua.RunMacroText("/target player");
                if (Cast(ArcaneIntellect))
                {
                    Lua.RunMacroText("/cleartarget");
                    return;
                }
            }

            // Evocation
            if (Me.ManaPercentage < 30)
                if (Cast(Evocation))
                    return;
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
            // CounterSpell
            if (settings.UseCounterspell
                && ToolBox.EnemyCasting())
                if (Cast(CounterSpell))
                    return;
        }

        protected bool CastStopMove(Spell s, bool castEvenIfWanding = true)
        {
            MovementManager.StopMove();
            return Cast(s, castEvenIfWanding);
        }

        protected bool Cast(Spell s, bool castEvenIfWanding = true)
        {
            if (!s.KnownSpell)
                return false;

            CombatDebug("*----------- INTO CAST FOR " + s.Name);
            float _spellCD = ToolBox.GetSpellCooldown(s.Name);
            CombatDebug("Cooldown is " + _spellCD);

            if (ToolBox.GetSpellCost(s.Name) > Me.Mana)
            {
                CombatDebug(s.Name + ": Not enough mana, SKIPPING");
                return false;
            }

            if (ToolBox.UsingWand() && !castEvenIfWanding 
                || _isBackingUp && !s.Name.Equals("Blink"))
            {
                CombatDebug("Didn't cast because we were backing up or wanding");
                return false;
            }

            if (_spellCD >= 2f)
            {
                CombatDebug("Didn't cast because cd is too long");
                return false;
            }

            if (ToolBox.UsingWand() 
                && castEvenIfWanding)
                ToolBox.StopWandWaitGCD(UseWand, Fireball);

            if (_spellCD < 2f && _spellCD > 0f)
            {
                if (ToolBox.GetSpellCastTime(s.Name) < 1f)
                {
                    CombatDebug(s.Name + " is instant and low CD, recycle");
                    return true;
                }

                int t = 0;
                while (ToolBox.GetSpellCooldown(s.Name) > 0)
                {
                    Thread.Sleep(50);
                    t += 50;
                    if (t > 2000)
                    {
                        CombatDebug(s.Name + ": waited for tool long, give up");
                        return false;
                    }
                }
                Thread.Sleep(100 + Usefuls.Latency);
                CombatDebug(s.Name + ": waited " + (t + 100) + " for it to be ready");
            }

            if (!s.IsSpellUsable)
            {
                CombatDebug("Didn't cast because spell somehow not usable");
                return false;
            }

            CombatDebug("Launching");
            if (ObjectManager.Target.IsAlive || !Fight.InFight && ObjectManager.Target.Guid < 1)
                s.Launch();
            return true;
        }

        protected void CombatDebug(string s)
        {
            if (settings.ActivateCombatDebug)
                Logger.CombatDebug(s);
        }

        protected Spell FrostArmor = new Spell("Frost Armor");
        protected Spell Fireball = new Spell("Fireball");
        protected Spell Frostbolt = new Spell("Frostbolt");
        protected Spell FireBlast = new Spell("Fire Blast");
        protected Spell ArcaneIntellect = new Spell("Arcane Intellect");
        protected Spell FrostNova = new Spell("Frost Nova");
        protected Spell UseWand = new Spell("Shoot");
        protected Spell IcyVeins = new Spell("Icy Veins");
        protected Spell CounterSpell = new Spell("Counterspell");
        protected Spell ConeOfCold = new Spell("Cone of Cold");
        protected Spell Evocation = new Spell("Evocation");
        protected Spell Blink = new Spell("Blink");
        protected Spell ColdSnap = new Spell("Cold Snap");
        protected Spell Polymorph = new Spell("Polymorph");
        protected Spell IceBarrier = new Spell("Ice Barrier");
        protected Spell SummonWaterElemental = new Spell("Summon Water Elemental");
        protected Spell IceLance = new Spell("Ice Lance");
        protected Spell RemoveCurse = new Spell("Remove Curse");
        protected Spell IceArmor = new Spell("Ice Armor");
        protected Spell ManaShield = new Spell("Mana Shield");
        protected Spell ArcaneMissiles = new Spell("Arcane Missiles");
        protected Spell PresenceOfMind = new Spell("Presence of Mind");
        protected Spell ArcanePower = new Spell("Arcane Power");
        protected Spell Slow = new Spell("Slow");
        protected Spell MageArmor = new Spell("Mage Armor");
        protected Spell ArcaneBlast = new Spell("Arcane Blast");
        protected Spell Combustion = new Spell("Combustion");
        protected Spell DragonsBreath = new Spell("Dragon's Breath");
        protected Spell BlastWave = new Spell("Blast Wave");
    }
}