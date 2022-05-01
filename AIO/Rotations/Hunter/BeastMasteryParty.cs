﻿using System;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

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
            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                // Aspect of the Cheetah
                if (!Me.IsMounted
                && !Me.HaveBuff("Aspect of the Cheetah")
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
            if (ObjectManager.Pet.IsValid
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
            if (!ObjectManager.Target.HaveBuff("Serpent Sting")
                && ObjectManager.Target.GetDistance > 13f
                && !SteadyShot.KnownSpell
                && cast.OnTarget(SerpentSting))
                return;
        }

        protected override void CombatRotation()
        {
            WoWUnit Target = ObjectManager.Target;
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
                _canOnlyMelee = true;

            // Mend Pet
            if (ObjectManager.Pet.IsAlive
                && ObjectManager.Pet.IsValid
                && ObjectManager.Pet.HealthPercent <= 50
                && !ObjectManager.Pet.HaveBuff("Mend Pet")
                && cast.OnFocusUnit(MendPet, ObjectManager.Pet))
                return;

            // Aspect of the viper
            if (!Me.HaveBuff("Aspect of the Viper")
                && Me.ManaPercentage < 30
                && cast.OnSelf(AspectViper))
                return;

            // Aspect of the Hawk
            if (!Me.HaveBuff("Aspect of the Hawk")
                && (Me.ManaPercentage > 90 || Me.HaveBuff("Aspect of the Cheetah"))
                || !Me.HaveBuff("Aspect of the Hawk")
                && !Me.HaveBuff("Aspect of the Cheetah")
                && !Me.HaveBuff("Aspect of the Viper"))
                if (cast.OnSelf(AspectHawk))
                    return;

            // Aspect of the Monkey
            if (!Me.HaveBuff("Aspect of the Monkey")
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
                && ObjectManager.Pet.IsAlive
                && ObjectManager.Pet.IsValid
                && Me.ManaPercentage > 10
                && cast.OnSelf(BestialWrath))
                return;

            // Rapid Fire
            if (Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent < 100
                && cast.OnSelf(RapidFire))
                return;

            // Kill Command
            if (ObjectManager.Pet.IsAlive
                && ObjectManager.Pet.IsValid)
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
            if ((Me.HealthPercent < 20 || partyManager.EnemiesFighting.Any(e => e.IsTargetingMe))
                && cast.OnSelf(FeignDeath))
                return;

            // Wing Clip
            if ((Target.CreatureTypeTarget == "Humanoid" || Target.Name.Contains("Plainstrider"))
                && settings.UseConcussiveShot
                && Target.HealthPercent < 20
                && !Target.HaveBuff("Wing Clip")
                && cast.OnTarget(WingClip))
                return;

            // Hunter's Mark
            if (ObjectManager.Pet.IsValid
                && !HuntersMark.TargetHaveBuff
                && Target.GetDistance > minRange
                && Target.IsAlive
                && cast.OnTarget(HuntersMark))
                return;

            // Multi-Shot
            if (Target.GetDistance > minRange
                && partyManager.EnemiesFighting.FindAll(e => e.Position.DistanceTo(Target.Position) < 15).Count > settings.MultishotCount
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
            if (!Target.HaveBuff("Serpent Sting")
                && Target.GetDistance < AutoShot.MaxRange
                && Target.HealthPercent >= 10
                && !SteadyShot.KnownSpell
                && Target.GetDistance > minRange
                && cast.OnTarget(SerpentSting))
                return;

            // Intimidation interrupt
            if (Target.GetDistance < AutoShot.MaxRange
                && ObjectManager.Pet.IsValid
                && ObjectManager.Pet.IsAlive
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
