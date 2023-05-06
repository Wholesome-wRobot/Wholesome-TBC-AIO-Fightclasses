using robotManager.Helpful;
using System;
using System.ComponentModel;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Mage
{
    public class Mage : BaseRotation
    {
        protected Mage specialization;
        protected MageSettings settings;
        protected MageFoodManager foodManager;
        protected IWoWUnit _polymorphedEnemy = null;
        protected bool iCanUseWand = WTGear.HaveRangedWeaponEquipped;
        protected bool isPolymorphing;
        protected bool polymorphableEnemyInThisFight = true;
        protected bool knowImprovedScorch = WTTalent.GetTalentRank(2, 9) > 0;

        public Mage(BaseSettings settings) : base(settings) { }

        public override void Initialize(IClassRotation specialization)
        {
            this.specialization = specialization as Mage;
            settings = MageSettings.Current;
            BaseInit(30, Fireball, UseWand, settings);
            foodManager = new MageFoodManager(cast);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightLoop += FightLoopHandler;

            Rotation();
        }

        public override void Dispose()
        {
            cast.IsBackingUp = false;
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            FightEvents.OnFightLoop -= FightLoopHandler;

            BaseDispose();
        }

        private void Rotation()
        {
            while (Main.IsLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && _polymorphedEnemy != null
                        && !Me.InCombatFlagOnly)
                        _polymorphedEnemy = null;

                    if (StatusChecker.OutOfCombat(RotationRole))
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat()
                        && !cast.IsBackingUp
                        && !isPolymorphing
                        && (!Target.HasAura(Polymorph) || unitCache.EnemiesAttackingMe.Count < 1))
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

        protected override void Pull() { }
        protected override void CombatNoTarget() { }
        protected override void HealerCombat() { }

        protected override void BuffRotation()
        {
            foodManager.CheckIfEnoughFoodAndDrinks();
            foodManager.CheckIfThrowFoodAndDrinks();
            foodManager.CheckIfHaveManaStone();

            if (specialization.RotationType == Enums.RotationType.Party)
            {
                // PARTY Arcane Intellect
                IWoWPlayer noAI = unitCache.GroupAndRaid
                    .Find(m => m.Mana > 0 && !m.HasAura(ArcaneIntellect));
                if (noAI != null && cast.OnFocusUnit(ArcaneIntellect, noAI))
                    return;
            }

            // Dampen Magic
            if (!Me.HasAura(DampenMagic)
                && settings.UseDampenMagic
                && DampenMagic.KnownSpell
                && DampenMagic.IsSpellUsable
                && cast.OnSelf(DampenMagic))
                return;
        }

        protected override void CombatRotation()
        {
            if (Pet.IsValid && !Pet.HasTarget)
                Lua.LuaDoString("PetAttack();");

            if (specialization.RotationType == Enums.RotationType.Party)
            {
                if (Me.HealthPercent < 30
                    && cast.OnSelf(IceBlock))
                    return;

                if (Me.HasAura(IceBlock)
                    && Me.HealthPercent <= 50)
                    return;

                if (Me.HasAura(IceBlock)
                    && Me.HealthPercent > 50)
                {
                    WTEffects.TBCCancelPlayerBuff("Ice Block");
                    return;
                }
            }

            // CounterSpell
            if (settings.UseCounterspell
                && WTCombat.TargetIsCasting()
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
        protected AIOSpell Blizzard = new AIOSpell("Blizzard");

        protected bool CheckIceBlock()
        {
            return false;
        }

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            cast.IsBackingUp = false;
            iCanUseWand = false;
            polymorphableEnemyInThisFight = false;
            isPolymorphing = false;
            RangeManager.SetRange(Fireball.MaxRange);

            if (!Fight.InFight
                && Me.InCombatFlagOnly
                && _polymorphedEnemy != null
                && unitCache.EnemiesAttackingMe.Count < 1
                && _polymorphedEnemy.IsAlive)
            {
                Logger.Log($"Starting fight with {_polymorphedEnemy.Name} (polymorphed)");
                Fight.InFight = false;
                Fight.CurrentTarget = _polymorphedEnemy.WowUnit;
                ulong _enemyGUID = _polymorphedEnemy.Guid;
                _polymorphedEnemy = null;
                Fight.StartFight(_enemyGUID);
            }
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            iCanUseWand = WTGear.HaveRangedWeaponEquipped;
        }

        private void FightLoopHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            float minDistance = RangeManager.GetMeleeRangeWithTarget() + 3f;

            // Do we need to backup?
            if ((Target.HasAura("Frostbite") || Target.HasAura(FrostNova))
                && Target.GetDistance < minDistance
                && Me.IsAlive
                && Target.IsAlive
                && !cast.IsBackingUp
                && !Me.IsCast
                && !RangeManager.CurrentRangeIsMelee()
                && Target.HealthPercent > 5
                && !isPolymorphing)
            {
                cast.IsBackingUp = true;
                Timer timer = new Timer(3000);

                // Using CTM
                if (settings.BackupUsingCTM)
                {
                    Vector3 position = WTSpace.BackOfUnit(Me.WowUnit, 15f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    Thread.Sleep(500);

                    // Backup loop
                    while (MovementManager.InMoveTo
                        && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                        && Target.GetDistance < minDistance
                        && Me.IsAlive
                        && Target.IsAlive
                        && (Target.HasAura("Frostbite") || Target.HasAura(FrostNova))
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
                    && Me.IsAlive
                    && Target.IsAlive
                    && Target.GetDistance < minDistance
                    && (Target.HasAura("Frostbite") || Target.HasAura(FrostNova))
                    && !timer.IsReady)
                    {
                        Move.Backward(Move.MoveAction.PressKey, 500);
                    }
                }
                cast.IsBackingUp = false;
            }

            // Polymorph
            if (settings.UsePolymorph
                && unitCache.EnemiesAttackingMe.Count > 1
                && Polymorph.KnownSpell
                && !cast.IsBackingUp
                && !cast.IsApproachingTarget
                && specialization.RotationType != Enums.RotationType.Party
                && polymorphableEnemyInThisFight)
            {
                IWoWUnit myNearbyPolymorphed = null;
                // Detect if a polymorph cast has succeeded
                if (_polymorphedEnemy != null)
                    myNearbyPolymorphed = unitCache.EnemyUnitsNearPlayer.Find(u => u.HasAura(Polymorph) && u.Guid == _polymorphedEnemy.Guid);

                // If we don't have a polymorphed enemy
                if (myNearbyPolymorphed == null)
                {
                    _polymorphedEnemy = null;
                    isPolymorphing = true;
                    IWoWUnit firstTarget = Target;
                    IWoWUnit potentialPolymorphTarget = null;

                    // Select our attackers one by one for potential polymorphs
                    foreach (IWoWUnit enemy in unitCache.EnemiesAttackingMe)
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
                        polymorphableEnemyInThisFight = false;

                    // Polymorph cast
                    if (potentialPolymorphTarget != null
                        && _polymorphedEnemy == null
                        && cast.OnFocusUnit(Polymorph, potentialPolymorphTarget))
                    {
                        Usefuls.WaitIsCasting();
                        _polymorphedEnemy = potentialPolymorphTarget;
                    }

                    isPolymorphing = false;
                }
            }
        }
    }
}