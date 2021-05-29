using System;
using System.Threading;
using robotManager.Helpful;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;
using System.ComponentModel;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Mage : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static MageSettings settings;

        protected Cast cast;

        protected MageFoodManager _foodManager;

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected WoWUnit _polymorphedEnemy = null;

        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected bool _isPolymorphing;
        protected bool _polymorphableEnemyInThisFight = true;
        protected bool _knowImprovedScorch = ToolBox.GetTalentRank(2, 9) > 0;

        protected Mage specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = MageSettings.Current;
            if (settings.PartyDrinkName != "")
                ToolBox.AddToDoNotSellList(settings.PartyDrinkName);
            cast = new Cast(Fireball, UseWand, settings);
            _foodManager = new MageFoodManager(cast);

            this.specialization = specialization as Mage;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

            RangeManager.SetRange(30);

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

                    if (StatusChecker.OutOfCombat(RotationRole))
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

            if (specialization.RotationType == Enums.RotationType.Party)
            {
                // PARTY Arcane Intellect
                WoWPlayer noAI = AIOParty.Group
                    .Find(m => m.Mana > 0 && !m.HaveBuff(ArcaneIntellect.Name));
                if (noAI != null && cast.OnFocusUnit(ArcaneIntellect, noAI))
                    return;
            }

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

            if (specialization.RotationType == Enums.RotationType.Party)
            {
                if (Me.HealthPercent < 30
                    && cast.OnSelf(IceBlock))
                    return;

                if (Me.HaveBuff("Ice Block")
                    && Me.HealthPercent <= 50)
                    return;

                if (Me.HaveBuff("Ice Block")
                    && Me.HealthPercent > 50)
                {
                    ToolBox.CancelPlayerBuff("Ice Block");
                    return;
                }
            }

            // CounterSpell
            if (settings.UseCounterspell
                && ToolBox.TargetIsCasting()
                && cast.OnTarget(CounterSpell))
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
        protected AIOSpell IceBlock = new AIOSpell("Ice Block");

        protected bool CheckIceBlock()
        {
            return false;
        }

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            cast.IsBackingUp = false;
            _iCanUseWand = false;
            _polymorphableEnemyInThisFight = false;
            _isPolymorphing = false;
            RangeManager.SetRange(Fireball.MaxRange);

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
            float minDistance = RangeManager.GetMeleeRangeWithTarget() + 3f;

            // Do we need to backup?
            if ((ObjectManager.Target.HaveBuff("Frostbite") || ObjectManager.Target.HaveBuff("Frost Nova"))
                && ObjectManager.Target.GetDistance < minDistance
                && Me.IsAlive
                && ObjectManager.Target.IsAlive
                && !cast.IsBackingUp
                && !Me.IsCast
                && !RangeManager.CurrentRangeIsMelee()
                && ObjectManager.Target.HealthPercent > 5
                && !_isPolymorphing)
            {
                cast.IsBackingUp = true;
                Timer timer = new Timer(3000);

                // Using CTM
                if (settings.BackupUsingCTM)
                {
                    Vector3 position = ToolBox.BackofVector3(Me.Position, Me, 15f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    Thread.Sleep(500);

                    // Backup loop
                    while (MovementManager.InMoveTo
                        && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                        && ObjectManager.Target.GetDistance < minDistance
                        && ObjectManager.Me.IsAlive
                        && ObjectManager.Target.IsAlive
                        && (ObjectManager.Target.HaveBuff("Frostbite") || ObjectManager.Target.HaveBuff("Frost Nova"))
                        && !timer.IsReady)
                    {
                        // Wait follow path
                        Thread.Sleep(300);
                        if (settings.BlinkWhenBackup)
                            cast.OnSelf(Blink);
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
                    && ObjectManager.Target.GetDistance < minDistance
                    && (ObjectManager.Target.HaveBuff("Frostbite") || ObjectManager.Target.HaveBuff("Frost Nova"))
                    && !timer.IsReady)
                    {
                        Move.Backward(Move.MoveAction.PressKey, 500);
                    }
                }
                cast.IsBackingUp = false;
            }

            // Polymorph
            if (settings.UsePolymorph
                && ObjectManager.GetNumberAttackPlayer() > 1
                && Polymorph.KnownSpell
                && !cast.IsBackingUp
                && !cast.IsApproachingTarget
                && specialization.RotationType != Enums.RotationType.Party
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
                    if (potentialPolymorphTarget != null 
                        && _polymorphedEnemy == null
                        && cast.OnFocusUnit(Polymorph, potentialPolymorphTarget))
                    {
                        Usefuls.WaitIsCasting();
                        _polymorphedEnemy = potentialPolymorphTarget;
                    }

                    _isPolymorphing = false;
                }
            }
        }
    }
}