using System;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Rotations.Hunter
{
    public class BeastMasteryParty : Hunter
    {
        public BeastMasteryParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasAura("Drink") || Me.ManaPercentage > 95)
            {
                // Aspect of the Cheetah
                if (!Me.IsMounted
                && !Me.HasAura(AspectCheetah)
                && MovementManager.InMoveTo
                && Me.ManaPercentage > 60
                && settings.UseAspectOfTheCheetah
                && cast.OnSelf(AspectCheetah))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;
            }
        }

        protected override void Pull()
        {
            // Hunter's Mark
            if (Pet.IsValid
                && !HuntersMark.TargetHaveBuff
                && cast.OnTarget(HuntersMark))
                return;

            // Steady Shot
            double lastAutoInMilliseconds = (DateTime.Now - LastAuto).TotalMilliseconds;
            if (lastAutoInMilliseconds > 0
                && lastAutoInMilliseconds < 700
                && cast.OnTarget(SteadyShot))
                return;

            // Serpent Sting
            if (!Target.HasAura(SerpentSting)
                && Target.GetDistance > 13f
                && !SteadyShot.KnownSpell
                && cast.OnTarget(SerpentSting))
                return;
        }

        protected override void CombatRotation()
        {
            float minRange = RangeManager.GetMeleeRangeWithTarget() + settings.BackupDistance;
            /*
            Logger.LogError($"Current range is {RangeManager.GetRange()}");
            Logger.LogError($"Target is {ObjectManager.Target.GetDistance} yards away");
            */
            if (Target.GetDistance < minRange
                && !cast.IsBackingUp)
                ToolBox.CheckAutoAttack(Attack);

            if (Target.GetDistance >= minRange
                && !cast.IsBackingUp)
                ReenableAutoshot();

            if (Target.GetDistance < minRange
                && !settings.BackupFromMelee)
                canOnlyMelee = true;

            // Mend Pet
            if (Pet.IsAlive
                && Pet.IsValid
                && Pet.HealthPercent <= 50
                && !Pet.HasAura(MendPet)
                && cast.OnFocusUnit(MendPet, Pet))
                return;

            // Aspect of the viper
            if (!Me.HasAura(AspectViper)
                && Me.ManaPercentage < 30
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
                && cast.OnTarget(AspectMonkey))
                return;

            // Disengage
            if (settings.UseDisengage
                && Target.Target == Me.Guid
                && Target.GetDistance < minRange
                && cast.OnTarget(Disengage))
                return;

            // Bestial Wrath
            if (Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent < 100
                && Pet.IsAlive
                && Pet.IsValid
                && Me.ManaPercentage > 10
                && cast.OnSelf(BestialWrath))
                return;

            // Rapid Fire
            if (Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent < 100
                && cast.OnSelf(RapidFire))
                return;

            // Kill Command
            if (Pet.IsAlive
                && Pet.IsValid)
                cast.OnTarget(KillCommand);

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
            if ((Me.HealthPercent < 20 || unitCache.EnemiesFighting.Any(e => e.IsTargetingMe))
                && cast.OnSelf(FeignDeath))
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

            // Multi-Shot
            if (Target.GetDistance > minRange
                && unitCache.EnemiesFighting.FindAll(e => e.PositionWithoutType.DistanceTo(Target.PositionWithoutType) < 15).Count > settings.MultishotCount
                && cast.OnTarget(MultiShot))
                return;

            // Steady Shot
            double lastAutoInMilliseconds = (DateTime.Now - LastAuto).TotalMilliseconds;
            if (lastAutoInMilliseconds > 0
                && lastAutoInMilliseconds < 500
                && Me.ManaPercentage > 10
                && cast.OnTarget(SteadyShot))
                return;

            // Serpent Sting
            if (!Target.HasAura(SerpentSting)
                && Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent >= 10
                && !SteadyShot.KnownSpell
                && Target.GetDistance > minRange
                && cast.OnTarget(SerpentSting))
                return;

            // Intimidation interrupt
            if (Target.GetDistance < AutoShot.MaxRange
                && Pet.IsValid
                && Pet.IsAlive
                && WTCombat.TargetIsCasting()
                && settings.IntimidationInterrupt
                && cast.OnTarget(Intimidation))
                return;

            // Arcane Shot
            if (Target.GetDistance < AutoShot.MaxRange
                && !SteadyShot.KnownSpell
                && cast.OnTarget(ArcaneShot))
                return;
        }
    }
}
