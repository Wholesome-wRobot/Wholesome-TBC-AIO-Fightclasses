using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class Feral : Druid
    {
        protected override void BuffRotation()
        {
            string currentGroundMount = wManager.wManagerSetting.CurrentSetting.GroundMountName;

            // Regrowth
            if (Me.HealthPercent < 70
                && !Me.HaveBuff("Regrowth")
                && cast.OnSelf(Regrowth))
                return;

            // Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Rejuvenation")
                && !Regrowth.KnownSpell
                && cast.OnSelf(Rejuvenation))
                return;

            // Healing Touch
            if (Me.HealthPercent < 40
                && !Regrowth.KnownSpell
                && cast.OnSelf(HealingTouch))
                return;

            // Remove Curse
            if (ToolBox.HasCurseDebuff()
                && cast.OnSelf(RemoveCurse))
                return;

            // Abolish Poison
            if (ToolBox.HasPoisonDebuff()
                && cast.OnSelf(AbolishPoison))
                return;

            // Mark of the Wild
            if (!Me.HaveBuff("Mark of the Wild")
                && cast.OnSelf(MarkOfTheWild))
                return;

            // Thorns
            if (!Me.HaveBuff("Thorns")
                && cast.OnSelf(Thorns))
                return;

            // Omen of Clarity
            if (!Me.HaveBuff("Omen of Clarity")
                && cast.OnSelf(OmenOfClarity))
                return;

            // Aquatic form
            if (Me.IsSwimming
                && !Me.HaveBuff("Aquatic Form")
                && Me.ManaPercentage > 50
                && cast.OnSelf(AquaticForm))
                return;

            // Travel Form OOC
            if (TravelForm.KnownSpell
                && (currentGroundMount == "" || currentGroundMount == CatForm.Name))
                ToolBox.SetGroundMount(TravelForm.Name);

            // Disable Cat Form OOC
            if (currentGroundMount == CatForm.Name)
                ToolBox.SetGroundMount("");
        }

        protected override void Pull()
        {
            // Bear Form
            if (!CatForm.KnownSpell
                && !Me.HaveBuff("Bear Form")
                && cast.OnSelf(BearForm))
                return;

            // Cat Form
            if (!Me.HaveBuff("Cat Form")
                && ObjectManager.Target.Guid > 0
                && cast.OnSelf(CatForm))
                return;

            // Prowl
            if (Me.HaveBuff("Cat Form")
                && !Me.HaveBuff(Prowl.Name)
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f
                && ToolBox.GetClosestHostileFrom(ObjectManager.Target, 20) == null
                && settings.StealthEngage
                && cast.OnSelf(Prowl))
                return;

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { FaerieFireFeral, MoonfireRank1, Wrath }))
            {
                _combatMeleeTimer = new Timer(2000);
                return;
            }

            // Pull Bear/Cat
            if (Me.HaveBuff("Bear Form")
                || Me.HaveBuff("Dire Bear Form")
                || Me.HaveBuff("Cat Form"))
            {
                RangeManager.SetRangeToMelee();

                // Prowl approach
                if (Me.HaveBuff("Prowl")
                    && ObjectManager.Target.GetDistance > 3f
                    && !_isStealthApproching)
                    StealthApproach();

                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();
            WoWUnit Target = ObjectManager.Target;

            // Force melee
            if (_combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check if fighting a caster
            if (_shouldBeInterrupted)
            {
                _fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!_casterEnemies.Contains(Target.Name))
                    _casterEnemies.Add(Target.Name);
            }

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Innervate
            if (settings.UseInnervate
                && Me.ManaPercentage < 20
                && cast.OnSelf(Innervate))
                return;

            // Barkskin + Regrowth + Rejuvenation
            if (settings.UseBarkskin
                && Barkskin.KnownSpell
                && Me.HealthPercent < 50
                && !Me.HaveBuff("Regrowth")
                && Me.Mana > bigHealComboCost + Barkskin.Cost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Barkskin) 
                && cast.OnSelf(Regrowth)
                && cast.OnSelf(Rejuvenation))
                return;

            // Regrowth + Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Regrowth")
                && Me.Mana > bigHealComboCost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Regrowth) 
                && cast.OnSelf(Rejuvenation))
                return;

            // Regrowth
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Regrowth")
                && Me.Mana > smallHealComboCost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Regrowth))
                return;

            // Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Rejuvenation")
                && !Regrowth.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Rejuvenation))
                return;

            // Healing Touch
            if (Me.HealthPercent < 30
                && !Regrowth.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(HealingTouch))
                return;

            // Catorm
            if (!Me.HaveBuff("Cat Form")
                && (ObjectManager.GetNumberAttackPlayer() < settings.NumberOfAttackersBearForm || !BearForm.KnownSpell && !DireBearForm.KnownSpell))
                if (cast.OnSelf(CatForm))
                    return;

            // Bear Form
            if (!Me.HaveBuff("Bear Form")
                && !Me.HaveBuff("Dire Bear Form"))
            {
                if (!CatForm.KnownSpell)
                {
                    if (cast.OnSelf(DireBearForm) || cast.OnSelf(BearForm))
                        return;
                }
                else if (ObjectManager.GetNumberAttackPlayer() >= settings.NumberOfAttackersBearForm
                        && settings.NumberOfAttackersBearForm > 1)
                {
                    {
                        if (cast.OnSelf(DireBearForm) || cast.OnSelf(BearForm))
                            return;
                    }
                }
            }


            #region Cat Form Rotation

            // **************** CAT FORM ROTATION ****************

            if (Me.HaveBuff("Cat Form"))
            {
                RangeManager.SetRangeToMelee();

                // Shred (when behind)
                if (Target.HaveBuff("Pounce")
                    && cast.OnTarget(Shred))
                    return;

                // Faerie Fire
                if (!Target.HaveBuff("Faerie Fire (Feral)")
                    && !Target.HaveBuff("Pounce")
                    && cast.OnTarget(FaerieFireFeral))
                    return;

                // Rip
                if (!Target.HaveBuff("Rip")
                    && !Target.HaveBuff("Pounce"))
                {
                    if (Me.ComboPoint >= 3
                        && Target.HealthPercent > 60
                        && cast.OnTarget(Rip))
                        return;

                    if (Me.ComboPoint >= 1
                        && Target.HealthPercent <= 60
                        && cast.OnTarget(Rip))
                        return;
                }

                // Ferocious Bite
                if (FerociousBite.KnownSpell
                    && !Target.HaveBuff("Pounce"))
                {
                    if (Me.ComboPoint >= 3
                        && Target.HealthPercent > 60
                        && cast.OnTarget(FerociousBite))
                        return;

                    if (Me.ComboPoint >= 1
                        && Target.HealthPercent <= 60
                        && cast.OnTarget(FerociousBite))
                        return;
                }

                // Rake
                if (!Target.HaveBuff("Rake")
                    && !Target.HaveBuff("Pounce")
                    && cast.OnTarget(Rake))
                    return;

                // Tiger's Fury
                if (!TigersFury.HaveBuff
                    && settings.UseTigersFury
                    && Me.ComboPoint < 1
                    && !Target.HaveBuff("Pounce")
                    && Me.Energy > 30
                    && cast.OnTarget(TigersFury))
                    return;

                // Mangle
                if (Me.ComboPoint < 5
                    && !Target.HaveBuff("Pounce")
                    && Me.Energy > 40
                    && MangleCat.KnownSpell
                    && Claw.IsSpellUsable
                    && cast.OnTarget(MangleCat))
                    return;

                // Claw
                if (Me.ComboPoint < 5 && !Target.HaveBuff("Pounce")
                    && !MangleCat.KnownSpell
                    && cast.OnTarget(Claw))
                    return;
            }

            #endregion

            #region Bear form rotation

            // **************** BEAR FORM ROTATION ****************

            if (Me.HaveBuff("Bear Form") || Me.HaveBuff("Dire Bear Form"))
            {
                RangeManager.SetRangeToMelee();

                // Frenzied Regeneration
                if (Me.HealthPercent < 50
                    && cast.OnTarget(FrenziedRegeneration))
                    return;

                // Faerie Fire
                if (!Target.HaveBuff("Faerie Fire (Feral)")
                    && cast.OnTarget(FaerieFireFeral))
                    return;

                // Swipe
                if (ObjectManager.GetNumberAttackPlayer() > 1 
                    && ToolBox.GetNbEnemiesClose(8f) > 1
                    && cast.OnTarget(Swipe))
                    return;

                // Interrupt with Bash
                if (_shouldBeInterrupted)
                {
                    Thread.Sleep(Main.humanReflexTime);
                    if (cast.OnTarget(Bash))
                        return;
                }

                // Enrage
                if (settings.UseEnrage
                    && cast.OnSelf(Enrage))
                    return;

                // Demoralizing Roar
                if (!Target.HaveBuff("Demoralizing Roar")
                    && !Target.HaveBuff("Demoralizing Shout")
                    && Target.GetDistance < 9f
                    && cast.OnTarget(DemoralizingRoar))
                    return;

                // Maul
                if (!MaulOn() 
                    && (!_fightingACaster || Me.Rage > 30)
                    && cast.OnTarget(Maul))
                    return;
            }

            #endregion

            #region Human form rotation

            // **************** HUMAN FORM ROTATION ****************

            // Avoid accidental Human Form stay
            if (CatForm.KnownSpell && CatForm.Cost < Me.Mana)
                return;
            if (BearForm.KnownSpell && BearForm.Cost < Me.Mana)
                return;

            if (!Me.HaveBuff("Bear Form")
                && !Me.HaveBuff("Cat Form")
                && !Me.HaveBuff("Dire Bear Form"))
            {
                // Moonfire
                if (!Target.HaveBuff("Moonfire")
                    && Me.ManaPercentage > 15
                    && Target.HealthPercent > 15
                    && Me.Level >= 8
                    && cast.OnTarget(Moonfire))
                    return;

                // Wrath
                if (Me.ManaPercentage > 45
                    && Target.HealthPercent > 30
                    && Me.Level >= 8
                    && cast.OnTarget(Wrath))
                    return;

                // Moonfire Low level DPS
                if (!Target.HaveBuff("Moonfire")
                    && Me.ManaPercentage > 50
                    && Target.HealthPercent > 30
                    && Me.Level < 8
                    && cast.OnTarget(Moonfire))
                    return;

                // Wrath Low level DPS
                if (Me.ManaPercentage > 60
                    && Target.HealthPercent > 30
                    && Me.Level < 8
                    && cast.OnTarget(Wrath))
                    return;
            }
            #endregion
        }
    }
}
