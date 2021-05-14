using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warrior
{
    public class ProtectionWarrior : Warrior
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // Defensive Stance
            if (InBattleStance())
                cast.Normal(DefensiveStance);
        }

        protected override void Pull()
        {
            base.Pull();

            // Defensive Stance
            if (InBattleStance()
                && cast.Normal(DefensiveStance))
                return;

            // Check if surrounding enemies
            if (ObjectManager.Target.GetDistance < _pullRange && !_pullFromAfar)
                _pullFromAfar = ToolBox.CheckIfEnemiesAround(ObjectManager.Target, _pullRange);

            // Pull from afar
            if (_pullFromAfar
                && _pullMeleeTimer.ElapsedMilliseconds < 5000 || settings.AlwaysPull
                && ObjectManager.Target.GetDistance < 24f)
            {
                AIOSpell pullMethod = null;

                if (Shoot.IsSpellUsable
                    && Shoot.KnownSpell)
                    pullMethod = Shoot;

                if (Throw.IsSpellUsable
                    && Throw.KnownSpell)
                    pullMethod = Throw;

                if (pullMethod == null)
                {
                    Logger.Log("Can't pull from distance. Please equip a ranged weapon in order to Throw or Shoot.");
                    _pullFromAfar = false;
                }
                else
                {
                    if (Me.IsMounted)
                        MountTask.DismountMount();

                    RangeManager.SetRange(_pullRange + 10);
                    Thread.Sleep(200);
                    if (cast.Normal(pullMethod))
                        Thread.Sleep(2000);
                    RangeManager.SetRange(_pullRange);
                }
            }

            // Melee ?
            if (_pullMeleeTimer.ElapsedMilliseconds <= 0
                && ObjectManager.Target.GetDistance <= _pullRange + 3)
                _pullMeleeTimer.Start();

            if (_pullMeleeTimer.ElapsedMilliseconds > 3000)
            {
                Logger.LogDebug("Going in Melee range");
                RangeManager.SetRangeToMelee();
                _pullMeleeTimer.Reset();
            }

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();
            WoWUnit Target = ObjectManager.Target;
            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();
            bool _inMeleeRange = Target.GetDistance < 6f;

            RegainAggro();

            // Defensive Stance
                if (InBattleStance())
                cast.Normal(DefensiveStance);

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Check if we need to interrupt
            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
            }

            // Melee ?
            if (_pullMeleeTimer.ElapsedMilliseconds > 0)
                _pullMeleeTimer.Reset();

            if (_meleeTimer.ElapsedMilliseconds <= 0
                && _pullFromAfar)
                _meleeTimer.Start();

            if ((_shouldBeInterrupted || _meleeTimer.ElapsedMilliseconds > 5000)
                && !RangeManager.CurrentRangeIsMelee())
            {
                Logger.LogDebug("Going in Melee range 2");
                RangeManager.SetRangeToMelee();
                _meleeTimer.Stop();
            }

            // Taunt
            if (_inMeleeRange
                && !Target.IsTargetingMe
                && Target.Target > 0
                && cast.Normal(Taunt))
                return;

            // Cleave
            List<WoWUnit> closeEnemies = _partyEnemiesAround
                .Where(e => e.GetDistance < 10 && e.InCombatFlagOnly)
                .ToList();
            if (_inMeleeRange
                && closeEnemies.Count > 1
                && ObjectManager.Me.Rage > 70)
                cast.Normal(Cleave);

            // Heroic Strike
            if (_inMeleeRange
                && !HeroicStrikeOn()
                && Me.Rage > 90)
                cast.Normal(HeroicStrike);

            // Last Stand
            if (Me.HealthPercent < 20
                && cast.Normal(LastStand))
                return;

            // Shied Bash
            if (ToolBox.TargetIsCasting()
                && cast.Normal(ShieldBash))
                return;

            // Demoralizing Shout
            if (settings.UseDemoralizingShout
                && !Target.HaveBuff("Demoralizing Shout")
                && _inMeleeRange
                && cast.Normal(DemoralizingShout))
                return;

            // Thunderclap
            if (_inMeleeRange
                && !ObjectManager.Target.HaveBuff(ThunderClap.Name)
                && cast.Normal(ThunderClap))
                return;

            // Shield Slam
            if (_inMeleeRange
                && Me.Rage > 70
                && ShieldSlam.IsSpellUsable
                && cast.Normal(ShieldSlam))
                return;

            // Revenge
            if (_inMeleeRange
                && ToolBox.GetPetSpellCooldown(Revenge.Name) <= 0
                && cast.Normal(Revenge))
                return;

            // Devastate
            if (_inMeleeRange
                && cast.Normal(Devastate))
                return;

            // Sunder Armor
            if (_inMeleeRange
                && cast.Normal(SunderArmor))
                return;

            // Shield Block
            if (ObjectManager.Me.HealthPercent < 50
                && cast.Normal(ShieldBlock))
                return;

            // Spell Reflection
            if (ToolBox.TargetIsCasting()
                && cast.Normal(SpellReflection))
                return;

            // Commanding Shout
            if (!Me.HaveBuff("Commanding Shout")
                && settings.UseCommandingShout
                && cast.Normal(CommandingShout))
                return;
        }
    }
}
