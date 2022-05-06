using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class FeralDPSParty : Druid
    {
        public FeralDPSParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if ((!Me.HasAura("Drink") || Me.ManaPercentage > 95) && Wrath.IsSpellUsable)
            {
                base.BuffRotation();

                // PARTY Remove Curse
                IWoWPlayer needRemoveCurse = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                    return;

                // PARTY Abolish Poison
                IWoWPlayer needAbolishPoison = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                    return;

                // PARTY Mark of the Wild
                IWoWPlayer needMotW = unitCache.GroupAndRaid
                    .Find(m => !m.HasAura(MarkOfTheWild));
                if (needMotW != null && cast.OnFocusUnit(MarkOfTheWild, needMotW))
                    return;

                // PARTY Thorns
                IWoWPlayer needThorns = unitCache.GroupAndRaid
                    .Find(m => !m.HasAura(Thorns));
                if (needThorns != null && cast.OnFocusUnit(Thorns, needThorns))
                    return;

                // Omen of Clarity
                if (!Me.HasAura(OmenOfClarity)
                    && OmenOfClarity.IsSpellUsable
                    && cast.OnSelf(OmenOfClarity))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;

                // Bear Form
                if (!CatForm.KnownSpell
                    && !Me.HasAura(BearForm)
                    && cast.OnSelf(BearForm))
                    return;

                // Cat Form
                if (!Me.HasAura(CatForm)
                    && cast.OnSelf(CatForm))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Bear Form
            if (!CatForm.KnownSpell
                && !Me.HasAura(BearForm)
                && cast.OnSelf(BearForm))
                return;

            // Cat Form
            if (!Me.HasAura(CatForm)
                && cast.OnSelf(CatForm))
                return;

            // Prowl
            if (Me.HasAura(CatForm)
                && !Me.HasAura(Prowl)
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f
                && settings.StealthEngage
                && cast.OnSelf(Prowl))
                return;

            // Pull Bear/Cat
            if (Me.HasAura(BearForm)
                || Me.HasAura(DireBearForm)
                || Me.HasAura(CatForm))
            {
                RangeManager.SetRangeToMelee();

                // Prowl approach
                if (Me.HasAura(Prowl)
                    && ObjectManager.Target.GetDistance > 3f
                    && !isStealthApproching)
                    StealthApproach();

                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool shouldBeInterrupted = WTCombat.TargetIsCasting();
            bool inMeleeRange = Target.GetDistance < 6f;

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

            // Party Tranquility
            if (settings.PartyTranquility && !unitCache.GroupAndRaid.Any(e => e.IsTargetingMe))
            {
                bool needTranquility = unitCache.GroupAndRaid
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
                IWoWPlayer needRebirth = unitCache.GroupAndRaid
                    .Find(m => m.IsDead);
                if (needRebirth != null && cast.OnFocusUnit(Rebirth, needRebirth))
                    return;
            }

            // PARTY Innervate
            if (settings.PartyUseInnervate)
            {
                IWoWPlayer needInnervate = unitCache.GroupAndRaid
                    .Find(m => m.ManaPercentage < 10 && !m.HasAura(Innervate));
                if (needInnervate != null && cast.OnFocusUnit(Innervate, needInnervate))
                    return;
            }

            if (settings.PartyRemoveCurse)
            {
                // PARTY Remove Curse
                IWoWPlayer needRemoveCurse = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                    return;
            }

            if (settings.PartyAbolishPoison)
            {
                // PARTY Abolish Poison
                IWoWPlayer needAbolishPoison = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                    return;
            }

            // Catorm
            if (!Me.HasAura(CatForm)
                && cast.OnSelf(CatForm))
                return;

            // Bear Form
            if (!Me.HasAura(BearForm)
                && !Me.HasAura(DireBearForm)
                && !CatForm.KnownSpell)
            {
                if (cast.OnSelf(DireBearForm) || cast.OnSelf(BearForm))
                    return;
            }


            #region Cat Form Rotation

            // **************** CAT FORM ROTATION ****************

            if (Me.HasAura(CatForm))
            {
                RangeManager.SetRangeToMelee();

                // Shred (when behind)
                if (Me.HasAura("Clearcasting")
                    && Me.Energy < 80
                    && cast.OnTarget(Shred))
                    return;

                // Faerie Fire
                if (!Target.HasAura("Faerie Fire (Feral)")
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
                    && !Target.HasAura("Mangle (Cat)")
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

            if (Me.HasAura(BearForm) || Me.HasAura(DireBearForm))
            {
                RangeManager.SetRangeToMelee();

                // Frenzied Regeneration
                if (Me.HealthPercent < 50
                    && cast.OnSelf(FrenziedRegeneration))
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
                if (settings.UseEnrage
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
