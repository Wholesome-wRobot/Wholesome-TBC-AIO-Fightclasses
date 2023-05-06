using System;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Hunter
{
    public class BeastMastery : Hunter
    {
        public BeastMastery(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            // Aspect of the Cheetah
            if (!Me.IsMounted
                && !Me.HasAura(AspectCheetah)
                && MovementManager.InMoveTo
                && Me.ManaPercentage > 60
                && settings.UseAspectOfTheCheetah
                && cast.OnSelf(AspectCheetah))
                return;
        }
        protected override void Pull()
        {
            // Hunter's Mark
            if (Pet.IsValid
                && !HuntersMark.TargetHaveBuff
                && cast.OnTarget(HuntersMark))
                return;

            // Serpent Sting
            if (!Target.HasAura(SerpentSting)
                && Target.GetDistance < AutoShot.MaxRange
                && Target.GetDistance > 13f
                && cast.OnTarget(SerpentSting))
                return;
        }
        protected override void CombatRotation()
        {
            float minRange = RangeManager.GetMeleeRangeWithTarget() + settings.BackupDistance;

            if (Target.GetDistance < minRange
                && !cast.IsBackingUp)
                ToolBox.CheckAutoAttack(Attack);

            if (Target.GetDistance > minRange
                && !cast.IsBackingUp)
                ReenableAutoshot();

            if (Target.GetDistance < minRange
                && !settings.BackupFromMelee)
                canOnlyMelee = true;

            // Mend Pet
            if (Pet.IsAlive
                && Pet.HealthPercent <= 80
                && !Pet.HasAura(MendPet)
                && cast.OnFocusUnit(MendPet, Pet))
                return;

            // Aspect of the viper
            if (!Me.HasAura(AspectViper)
                && Me.ManaPercentage < 35
                && cast.OnSelf(AspectViper))
                return;

            // Aspect of the Hawk
            if (!Me.HasAura(AspectHawk)
                && (Me.ManaPercentage > 90 || Me.HasAura(AspectCheetah))
                || !Me.HasAura(AspectHawk)
                && !Me.HasAura(AspectCheetah)
                && !Me.HasAura(AspectViper))
                if (cast.OnSelf(AspectHawk))
                    return;

            // Aspect of the Monkey
            if (!Me.HasAura(AspectMonkey)
                && !AspectHawk.KnownSpell
                && cast.OnSelf(AspectMonkey))
                return;

            // Disengage
            if (settings.UseDisengage
                && Pet.Target == Me.Target
                && Target.IsTargetingMe
                && Target.GetDistance < minRange
                && cast.OnTarget(Disengage))
                return;

            // Bestial Wrath
            if (Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent >= 60
                && Me.ManaPercentage > 10
                && BestialWrath.IsSpellUsable
                && (settings.BestialWrathOnMulti && unitCache.EnemiesAttackingMe.Count > 1 || !settings.BestialWrathOnMulti)
                && cast.OnSelf(BestialWrath))
                return;

            // Rapid Fire
            if (Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent >= 80.0
                && (settings.RapidFireOnMulti && unitCache.EnemiesAttackingMe.Count > 1 || !settings.RapidFireOnMulti)
                && cast.OnSelf(RapidFire))
                return;

            // Kill Command
            if (Pet.IsAlive
                && cast.OnTarget(KillCommand))
                return;

            // Raptor Strike
            if (settings.UseRaptorStrike
                && Target.GetDistance < minRange
                && !WTCombat.IsSpellActive("Raptor Strike")
                && cast.OnTarget(RaptorStrike))
                return;

            // Mongoose Bite
            if (Target.GetDistance < minRange
                && cast.OnTarget(MongooseBite))
                return;

            // Feign Death
            if ((Me.HealthPercent < 20
                || (unitCache.EnemiesAttackingMe.Count > 1 && unitCache.EnemiesAttackingMe.Where(u => u.Target == Me.Guid).Count() > 0))
                && cast.OnSelf(FeignDeath))
                return;

            // Concussive Shot
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.UseConcussiveShot
                && Target.HealthPercent < 20
                && !ConcussiveShot.TargetHaveBuff
                && cast.OnTarget(ConcussiveShot))
                return;

            // Wing Clip
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.UseConcussiveShot
                && Target.HealthPercent < 20
                && !Target.HasAura(WingClip)
                && cast.OnTarget(WingClip))
                return;

            // Hunter's Mark
            if (Pet.IsValid
                && !HuntersMark.TargetHaveBuff
                && Target.GetDistance > minRange
                && Target.IsAlive
                && cast.OnTarget(HuntersMark))
                return;

            double lastAutoInMilliseconds = (DateTime.Now - LastAuto).TotalMilliseconds;
            // Steady Shot
            if (lastAutoInMilliseconds > 0
                && lastAutoInMilliseconds < 500
                && Me.ManaPercentage > 30
                && cast.OnTarget(SteadyShot))
                return;

            // Serpent Sting
            if (!Target.HasAura(SerpentSting)
                && Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent >= 60
                && Me.ManaPercentage > 50u
                && !SteadyShot.KnownSpell
                && Target.GetDistance > minRange
                && cast.OnTarget(SerpentSting))
                return;

            // Intimidation
            if (Target.GetDistance < AutoShot.MaxRange
                && Target.GetDistance > minRange
                && Target.HealthPercent >= 20
                && Me.ManaPercentage > 10
                && !settings.IntimidationInterrupt
                && cast.OnSelf(Intimidation))
                return;

            // Intimidation interrupt
            if (Target.GetDistance < AutoShot.MaxRange
                && WTCombat.TargetIsCasting()
                && settings.IntimidationInterrupt
                && cast.OnSelf(Intimidation))
                return;

            // Arcane Shot
            if (Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent >= 30
                && Me.ManaPercentage > 80
                && !SteadyShot.KnownSpell
                && cast.OnTarget(ArcaneShot))
                return;
        }
    }
}
