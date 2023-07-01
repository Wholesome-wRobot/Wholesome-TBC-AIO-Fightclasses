using System.Collections.Generic;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class Feral : Druid
    {
        public Feral(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Solo;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            string currentGroundMount = wManager.wManagerSetting.CurrentSetting.GroundMountName;

            // Regrowth
            if (Me.HealthPercent < 70
                && !Me.HasAura(Regrowth)
                && cast.OnSelf(Regrowth))
                return;

            // Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HasAura(Rejuvenation)
                && !Regrowth.KnownSpell
                && cast.OnSelf(Rejuvenation))
                return;

            // Healing Touch
            if (Me.HealthPercent < 40
                && !Regrowth.KnownSpell
                && cast.OnSelf(HealingTouch))
                return;

            // Remove Curse
            if (WTEffects.HasCurseDebuff()
                && cast.OnSelf(RemoveCurse))
                return;

            // Abolish Poison
            if (WTEffects.HasPoisonDebuff()
                && cast.OnSelf(AbolishPoison))
                return;

            // Mark of the Wild
            if (!Me.HasAura(MarkOfTheWild)
                && cast.OnSelf(MarkOfTheWild))
                return;

            // Thorns
            if (!Me.HasAura(Thorns)
                && cast.OnSelf(Thorns))
                return;

            // Omen of Clarity
            if (!Me.HasAura(OmenOfClarity)
                && cast.OnSelf(OmenOfClarity))
                return;

            // Aquatic form
            if (Me.IsSwimming
                && !Me.HasAura(AquaticForm)
                && Me.ManaPercentage > 50
                && cast.OnSelf(AquaticForm))
                return;

            // Travel Form OOC
            if (TravelForm.KnownSpell
                && (currentGroundMount == "" || currentGroundMount == CatForm.Name))
                WTSettings.SetGroundMount(TravelForm.Name);

            // Disable Cat Form OOC
            if (currentGroundMount == CatForm.Name)
                WTSettings.SetGroundMount("");
        }

        protected override void Pull()
        {
            // Bear Form
            if (!CatForm.KnownSpell
                && !Me.HasAura(BearForm)
                && cast.OnSelf(BearForm))
                return;

            // Cat Form
            if (!Me.HasAura(CatForm)
                && Target.Guid > 0
                && cast.OnSelf(CatForm))
                return;

            // Prowl
            if (Me.HasAura(CatForm)
                && !Me.HasAura(Prowl)
                && Target.GetDistance > 15f
                && Target.GetDistance < 25f
                && unitCache.GetClosestHostileFrom(Target, 20) == null
                && settings.SFER_StealthEngage
                && cast.OnSelf(Prowl))
                return;

            // Check if caster in list
            if (casterEnemies.Contains(Target.Name))
                fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.SFER_AlwaysPull, new List<AIOSpell> { FaerieFireFeral, MoonfireRank1, Wrath }, unitCache))
            {
                combatMeleeTimer = new Timer(2000);
                return;
            }

            // Pull Bear/Cat
            if (Me.HasAura(BearForm)
                || Me.HasAura(DireBearForm)
                || Me.HasAura(CatForm))
            {
                RangeManager.SetRangeToMelee();

                // Prowl approach
                if (Me.HasAura(Prowl)
                    && !isStealthApproching)
                {
                    StealthApproach();
                }
                else
                {
                    RangeManager.SetRangeToMelee();
                    cast.OnTarget(Claw);
                }

                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool shouldBeInterrupted = WTCombat.TargetIsCasting();

            // Force melee
            if (combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check if fighting a caster
            if (shouldBeInterrupted)
            {
                fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!casterEnemies.Contains(Target.Name))
                    casterEnemies.Add(Target.Name);
            }

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Innervate
            if (settings.SFER_UseInnervate
                && Me.ManaPercentage < 20
                && cast.OnSelf(Innervate))
                return;

            // Barkskin + Regrowth + Rejuvenation
            if (settings.SFER_UseBarkskin
                && Barkskin.KnownSpell
                && Me.HealthPercent < 50
                && !Me.HasAura(Regrowth)
                && Me.Mana > bigHealComboCost + Barkskin.Cost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Barkskin)
                && cast.OnSelf(Regrowth)
                && cast.OnSelf(Rejuvenation))
                return;

            // Regrowth + Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HasAura(Regrowth)
                && Me.Mana > bigHealComboCost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Regrowth)
                && cast.OnSelf(Rejuvenation))
                return;

            // Regrowth
            if (Me.HealthPercent < 50
                && !Me.HasAura(Regrowth)
                && Me.Mana > smallHealComboCost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25)
                && cast.OnSelf(Regrowth))
                return;

            // Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HasAura(Rejuvenation)
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
            if (!Me.HasAura(CatForm)
                && (unitCache.EnemiesAttackingMe.Count < settings.SFER_NumberOfAttackersBearForm || !BearForm.KnownSpell && !DireBearForm.KnownSpell))
                if (cast.OnSelf(CatForm))
                    return;

            // Bear Form
            if (!Me.HasAura(BearForm)
                && !Me.HasAura(DireBearForm))
            {
                if (!CatForm.KnownSpell)
                {
                    if (cast.OnSelf(DireBearForm) || cast.OnSelf(BearForm))
                        return;
                }
                else if (unitCache.EnemiesAttackingMe.Count >= settings.SFER_NumberOfAttackersBearForm
                        && settings.SFER_NumberOfAttackersBearForm > 1)
                {
                    {
                        if (cast.OnSelf(DireBearForm) || cast.OnSelf(BearForm))
                            return;
                    }
                }
            }


            #region Cat Form Rotation

            // **************** CAT FORM ROTATION ****************

            if (Me.HasAura(CatForm))
            {
                RangeManager.SetRangeToMelee();

                // Shred (when behind)
                if (Target.HasAura(Pounce)
                    && cast.OnTarget(Shred))
                    return;

                // Faerie Fire
                if (!Target.HasAura("Faerie Fire (Feral)")
                    && !Target.HasAura(Pounce)
                    && cast.OnTarget(FaerieFireFeral))
                    return;

                // Rip
                if (!Target.HasAura(Rip)
                    && !Target.HasAura(Pounce))
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
                    && !Target.HasAura(Pounce))
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
                if (!Target.HasAura(Rake)
                    && !Target.HasAura(Pounce)
                    && cast.OnTarget(Rake))
                    return;

                // Tiger's Fury
                if (!TigersFury.HaveBuff
                    && settings.SFER_UseTigersFury
                    && Me.ComboPoint < 1
                    && !Target.HasAura(Pounce)
                    && Me.Energy > 30
                    && cast.OnTarget(TigersFury))
                    return;

                // Mangle
                if (Me.ComboPoint < 5
                    && !Target.HasAura(Pounce)
                    && Me.Energy > 40
                    && MangleCat.KnownSpell
                    && Claw.IsSpellUsable
                    && cast.OnTarget(MangleCat))
                    return;

                // Claw
                if (Me.ComboPoint < 5 && !Target.HasAura(Pounce)
                    && !MangleCat.KnownSpell
                    && cast.OnTarget(Claw))
                    return;
            }

            #endregion

            #region Bear form rotation

            // **************** BEAR FORM ROTATION ****************

            if (Me.HasAura(BearForm) || Me.HasAura(DireBearForm))
            {
                RangeManager.SetRangeToMelee();

                // Frenzied Regeneration
                if (Me.HealthPercent < 50
                    && cast.OnTarget(FrenziedRegeneration))
                    return;

                // Faerie Fire
                if (!Target.HasAura("Faerie Fire (Feral)")
                    && cast.OnTarget(FaerieFireFeral))
                    return;

                // Swipe
                if (unitCache.EnemiesAttackingMe.Count > 1
                    && ToolBox.GetNbEnemiesClose(8f) > 1
                    && cast.OnTarget(Swipe))
                    return;

                // Interrupt with Bash
                if (shouldBeInterrupted)
                {
                    Thread.Sleep(Main.humanReflexTime);
                    if (cast.OnTarget(Bash))
                        return;
                }

                // Enrage
                if (settings.SFER_UseEnrage
                    && cast.OnSelf(Enrage))
                    return;

                // Demoralizing Roar
                if (!Target.HasAura(DemoralizingRoar)
                    && !Target.HasAura("Demoralizing Shout")
                    && Target.GetDistance < 9f
                    && cast.OnTarget(DemoralizingRoar))
                    return;

                // Maul
                if (!WTCombat.IsSpellActive("Maul")
                    && (!fightingACaster || Me.Rage > 30)
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

            if (!Me.HasAura(BearForm)
                && !Me.HasAura(CatForm)
                && !Me.HasAura(DireBearForm))
            {
                // Moonfire
                if (!Target.HasAura(Moonfire)
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
                if (!Target.HasAura(Moonfire)
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
