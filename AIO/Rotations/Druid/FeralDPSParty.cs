using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class FeralDPSParty : Druid
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Remove Curse
            WoWPlayer needRemoveCurse = AIOParty.Group
                .Find(m => ToolBox.HasCurseDebuff(m.Name));
            if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                return;

            // PARTY Abolish Poison
            WoWPlayer needAbolishPoison = AIOParty.Group
                .Find(m => ToolBox.HasPoisonDebuff(m.Name));
            if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                return;

            // PARTY Mark of the Wild
            WoWPlayer needMotW = AIOParty.Group
                .Find(m => !m.HaveBuff(MarkOfTheWild.Name));
            if (needMotW != null && cast.OnFocusUnit(MarkOfTheWild, needMotW))
                return;

            // PARTY Thorns
            WoWPlayer needThorns = AIOParty.Group
                .Find(m => !m.HaveBuff(Thorns.Name));
            if (needThorns != null && cast.OnFocusUnit(Thorns, needThorns))
                return;

            // Omen of Clarity
            if (!Me.HaveBuff("Omen of Clarity")
                && OmenOfClarity.IsSpellUsable
                && cast.OnSelf(OmenOfClarity))
                return;

            // PARTY Drink
            if (AIOParty.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                return;

            // Bear Form
            if (!CatForm.KnownSpell
                && !Me.HaveBuff(BearForm.Name)
                && cast.OnSelf(BearForm))
                return;

            // Cat Form
            if (!Me.HaveBuff(CatForm.Name)
                && cast.OnSelf(CatForm))
                return;
        }

        protected override void Pull()
        {
            base.Pull();

            // Bear Form
            if (!CatForm.KnownSpell
                && !Me.HaveBuff(BearForm.Name)
                && cast.OnSelf(BearForm))
                return;

            // Cat Form
            if (!Me.HaveBuff(CatForm.Name)
                && cast.OnSelf(CatForm))
                return;

            // Prowl
            if (Me.HaveBuff(CatForm.Name)
                && !Me.HaveBuff(Prowl.Name)
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f
                && settings.StealthEngage
                && cast.OnSelf(Prowl))
                return;

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
            bool _inMeleeRange = ObjectManager.Target.GetDistance < 6f;
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

            // Party Tranquility
            if (settings.PartyTranquility && !AIOParty.Group.Any(e => e.IsTargetingMe))
            {
                bool needTranquility = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 50)
                    .Count > 2;
                if (needTranquility
                    && cast.OnTarget(Tranquility))
                {
                    Usefuls.WaitIsCasting();
                    return;
                }
            }

            // PARTY Rebirth
            if (settings.PartyUseRebirth)
            {
                WoWPlayer needRebirth = AIOParty.Group
                    .Find(m => m.IsDead);
                if (needRebirth != null && cast.OnFocusUnit(Rebirth, needRebirth))
                    return;
            }

            // PARTY Innervate
            if (settings.PartyUseInnervate)
            {
                WoWPlayer needInnervate = AIOParty.Group
                    .Find(m => m.ManaPercentage < 10 && !m.HaveBuff("Innervate"));
                if (needInnervate != null && cast.OnFocusUnit(Innervate, needInnervate))
                    return;
            }

            if (settings.PartyRemoveCurse)
            {
                // PARTY Remove Curse
                WoWPlayer needRemoveCurse = AIOParty.Group
                    .Find(m => ToolBox.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                    return;
            }

            if (settings.PartyAbolishPoison)
            {
                // PARTY Abolish Poison
                WoWPlayer needAbolishPoison = AIOParty.Group
                    .Find(m => ToolBox.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                    return;
            }

            // Catorm
            if (!Me.HaveBuff(CatForm.Name)
                && cast.OnSelf(CatForm))
                return;

            // Bear Form
            if (!Me.HaveBuff(BearForm.Name)
                && !Me.HaveBuff(DireBearForm.Name)
                && !CatForm.KnownSpell)
                {
                    if (cast.OnSelf(DireBearForm) || cast.OnSelf(BearForm))
                        return;
                }


            #region Cat Form Rotation

            // **************** CAT FORM ROTATION ****************

            if (Me.HaveBuff(CatForm.Name))
            {
                RangeManager.SetRangeToMelee();

                // Shred (when behind)
                if (Me.HaveBuff("Clearcasting")
                    && Me.Energy < 80
                    && cast.OnTarget(Shred))
                    return;

                // Faerie Fire
                if (!Target.HaveBuff("Faerie Fire (Feral)")
                    && cast.OnTarget(FaerieFireFeral))
                    return;

                // Mangle
                if (Me.ComboPoint < 5
                    && Me.Energy > 40
                    && MangleCat.KnownSpell
                    && cast.OnTarget(MangleCat))
                    return;

                // Rip
                if (Me.ComboPoint >= 5
                    && Me.Energy >= 80
                    && cast.OnTarget(Rip))
                    return;

                // Rip
                if (Me.ComboPoint >= 4
                    && Me.Energy >= 80
                    && !Target.HaveBuff("Mangle (Cat)")
                    && cast.OnTarget(Rip))
                    return;

                // Ferocious bite (Rip is banned)
                if (Me.ComboPoint >= 5
                    && Me.Energy >= 90
                    && cast.OnTarget(FerociousBite))
                    return;

                // Claw
                if (Me.ComboPoint < 5
                    && !MangleCat.KnownSpell
                    && cast.OnTarget(Claw))
                    return;
            }

            #endregion

            #region Bear form rotation

            // **************** BEAR FORM ROTATION ****************

            if (Me.HaveBuff(BearForm.Name) || Me.HaveBuff(DireBearForm.Name))
            {
                RangeManager.SetRangeToMelee();

                // Frenzied Regeneration
                if (Me.HealthPercent < 50
                    && cast.OnSelf(FrenziedRegeneration))
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

            if (!Me.HaveBuff(BearForm.Name)
                && !Me.HaveBuff(CatForm.Name)
                && !Me.HaveBuff(DireBearForm.Name))
            {
                // Moonfire
                if (!Target.HaveBuff(Moonfire.Name)
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
                if (!Target.HaveBuff(Moonfire.Name)
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
