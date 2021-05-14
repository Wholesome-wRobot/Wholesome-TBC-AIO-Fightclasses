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
using System.ComponentModel;
using System.Collections.Generic;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Mage : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static MageSettings settings;

        protected Cast cast;

        protected MageFoodManager _foodManager = new MageFoodManager();

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected WoWUnit _polymorphedEnemy = null;

        protected float _distanceRange = 28f;
        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected bool _isPolymorphing;
        protected bool _polymorphableEnemyInThisFight = true;
        protected bool _knowImprovedScorch = ToolBox.GetTalentRank(2, 9) > 0;
        protected List<WoWUnit> _partyEnemiesAround = new List<WoWUnit>();

        protected Mage specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = MageSettings.Current;
            cast = new Cast(Fireball, settings.ActivateCombatDebug, UseWand, settings.AutoDetectImmunities);

            this.specialization = specialization as Mage;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            _distanceRange = specialization is Fire ? 33f : _distanceRange;

            RangeManager.SetRange(_distanceRange);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            Rotation();
        }

        public void Dispose()
        {
            cast.IsBackingUp = false;
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;
            cast.Dispose();
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && _polymorphedEnemy != null 
                        && !ObjectManager.Me.InCombatFlagOnly)
                            _polymorphedEnemy = null;

                    if (RotationType == Enums.RotationType.Party)
                        _partyEnemiesAround = ToolBox.GetSuroundingEnemies();

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat()
                        && !cast.IsBackingUp
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

            // Dampen Magic
            if (!Me.HaveBuff("Dampen Magic")
                && settings.UseDampenMagic
                && DampenMagic.KnownSpell
                && DampenMagic.IsSpellUsable
                && cast.OnSelf(DampenMagic))
                return;
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
            if (ObjectManager.Pet.IsValid && !ObjectManager.Pet.HasTarget)
                Lua.LuaDoString("PetAttack();", false);

            // CounterSpell
            if (settings.UseCounterspell
                && ToolBox.TargetIsCasting()
                && cast.Normal(CounterSpell))
                return;
        }

        protected AIOSpell FrostArmor = new AIOSpell("Frost Armor");
        protected AIOSpell Fireball = new AIOSpell("Fireball");
        protected AIOSpell Frostbolt = new AIOSpell("Frostbolt");
        protected AIOSpell FireBlast = new AIOSpell("Fire Blast");
        protected AIOSpell ArcaneIntellect = new AIOSpell("Arcane Intellect");
        protected AIOSpell FrostNova = new AIOSpell("Frost Nova");
        protected AIOSpell UseWand = new AIOSpell("Shoot");
        protected AIOSpell IcyVeins = new AIOSpell("Icy Veins");
        protected AIOSpell CounterSpell = new AIOSpell("Counterspell");
        protected AIOSpell ConeOfCold = new AIOSpell("Cone of Cold");
        protected AIOSpell Evocation = new AIOSpell("Evocation");
        protected AIOSpell Blink = new AIOSpell("Blink");
        protected AIOSpell ColdSnap = new AIOSpell("Cold Snap");
        protected AIOSpell Polymorph = new AIOSpell("Polymorph");
        protected AIOSpell IceBarrier = new AIOSpell("Ice Barrier");
        protected AIOSpell SummonWaterElemental = new AIOSpell("Summon Water Elemental");
        protected AIOSpell IceLance = new AIOSpell("Ice Lance");
        protected AIOSpell RemoveCurse = new AIOSpell("Remove Curse");
        protected AIOSpell IceArmor = new AIOSpell("Ice Armor");
        protected AIOSpell ManaShield = new AIOSpell("Mana Shield");
        protected AIOSpell ArcaneMissiles = new AIOSpell("Arcane Missiles");
        protected AIOSpell PresenceOfMind = new AIOSpell("Presence of Mind");
        protected AIOSpell ArcanePower = new AIOSpell("Arcane Power");
        protected AIOSpell Slow = new AIOSpell("Slow");
        protected AIOSpell MageArmor = new AIOSpell("Mage Armor");
        protected AIOSpell ArcaneBlast = new AIOSpell("Arcane Blast");
        protected AIOSpell Combustion = new AIOSpell("Combustion");
        protected AIOSpell DragonsBreath = new AIOSpell("Dragon's Breath");
        protected AIOSpell BlastWave = new AIOSpell("Blast Wave");
        protected AIOSpell Attack = new AIOSpell("Attack");
        protected AIOSpell DampenMagic = new AIOSpell("Dampen Magic");
        protected AIOSpell ArcaneExplosion = new AIOSpell("Arcane Explosion");
        protected AIOSpell MoltenArmor = new AIOSpell("Molten Armor");
        protected AIOSpell Scorch = new AIOSpell("Scorch");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            cast.IsBackingUp = false;
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
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            // Do we need to backup?
            if ((ObjectManager.Target.HaveBuff("Frostbite") || ObjectManager.Target.HaveBuff("Frost Nova"))
                && ObjectManager.Target.GetDistance < 10f
                && Me.IsAlive
                && ObjectManager.Target.IsAlive
                && !cast.IsBackingUp
                && !Me.IsCast
                && !RangeManager.CurrentRangeIsMelee()
                && ObjectManager.Target.HealthPercent > 5
                && !_isPolymorphing)
            {
                cast.IsBackingUp = true;
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
                            cast.Normal(Blink);
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
                    && (ObjectManager.Target.HaveBuff("Frostbite") || ObjectManager.Target.HaveBuff("Frost Nova"))
                    && limiter <= 6)
                    {
                        Move.Backward(Move.MoveAction.PressKey, 700);
                        limiter++;
                    }
                }
                cast.IsBackingUp = false;
            }

            // Polymorph
            if (settings.UsePolymorph
                && ObjectManager.GetNumberAttackPlayer() > 1
                && Polymorph.KnownSpell
                && !cast.IsBackingUp
                && !(specialization is ArcaneParty)
                && !(specialization is FireParty)
                && !(specialization is FrostParty)
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
                        while (!cast.Normal(Polymorph)
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
        }
    }
}