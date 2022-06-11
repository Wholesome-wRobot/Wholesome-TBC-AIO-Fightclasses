using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class FeralTankParty : Druid
    {
        public FeralTankParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Tank;
        }

        protected override void BuffRotation()
        {
            if ((!Me.HasDrinkAura || Me.ManaPercentage > 95) && Wrath.IsSpellUsable)
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

                // Dire Bear Form
                if (!Me.HasAura(DireBearForm)
                    && cast.OnSelf(DireBearForm))
                    return;

                // Bear Form
                if (!DireBearForm.KnownSpell
                    && !Me.HasAura(BearForm)
                    && cast.OnSelf(BearForm))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Dire Bear Form
            if (!Me.HasAura(DireBearForm)
                && cast.OnSelf(DireBearForm))
                return;

            // Bear Form
            if (!DireBearForm.KnownSpell
                && !Me.HasAura(BearForm)
                && cast.OnSelf(BearForm))
                return;

            // Check if caster in list
            if (casterEnemies.Contains(Target.Name))
                fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { FaerieFireFeral, MoonfireRank1, Wrath }, unitCache))
            {
                combatMeleeTimer = new Timer(1000);
                return;
            }
        }

        protected override void CombatNoTarget()
        {
            base.CombatNoTarget();

            if (settings.PartyTankSwitchTarget)
                partyManager.SwitchTarget(cast, null);
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool shouldBeInterrupted = WTCombat.TargetIsCasting();
            bool inMeleeRange = Target.GetDistance < 6f;
            IWoWUnit target = Target;

            if (settings.PartyTankSwitchTarget)
                partyManager.SwitchTarget(cast, null);

            // Force melee
            if (combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            // Check if fighting a caster
            if (shouldBeInterrupted)
            {
                fightingACaster = true;
                RangeManager.SetRangeToMelee();
                if (!casterEnemies.Contains(target.Name))
                    casterEnemies.Add(target.Name);
            }

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Party Tranquility
            if (settings.PartyTranquility && !unitCache.EnemiesFighting.Any(e => e.IsTargetingMe))
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

            // Dire Bear Form
            if (DireBearForm.KnownSpell
                && !Me.HasAura(DireBearForm)
                && cast.OnSelf(DireBearForm))
                return;

            // Bear Form
            if (!DireBearForm.KnownSpell
                && !Me.HasAura(BearForm)
                && cast.OnSelf(BearForm))
                return;

            // Feral Charge
            if (target.GetDistance > 10
                && cast.OnTarget(FeralCharge))
                return;

            // Interrupt with Bash
            if (shouldBeInterrupted
                && cast.OnTarget(Bash))
                return;

            // Taunt
            if (inMeleeRange
                && !target.IsTargetingMe
                && target.Target > 0
                && cast.OnTarget(Growl))
                return;

            // Challenging roar
            if (inMeleeRange
                && !target.IsTargetingMe
                && target.Target > 0
                && ToolBox.GetNbEnemiesClose(8) > 2
                && cast.OnTarget(ChallengingRoar))
                return;

            // Maul
            if (!WTCombat.IsSpellActive("Maul")
                && Me.Rage > 70)
                cast.OnTarget(Maul);

            // Frenzied Regeneration
            if (Me.HealthPercent < 50
                && cast.OnSelf(FrenziedRegeneration))
                return;

            // Enrage
            if (settings.UseEnrage
                && cast.OnSelf(Enrage))
                return;

            // Faerie Fire
            if (!target.HasAura("Faerie Fire (Feral)")
                && cast.OnTarget(FaerieFireFeral))
                return;

            // Demoralizing Roar
            if (!target.HasAura(DemoralizingRoar)
                && !target.HasAura("Demoralizing Shout")
                && target.GetDistance < 9f
                && cast.OnTarget(DemoralizingRoar))
                return;

            // Mangle
            if (MangleBear.KnownSpell
                && Me.Rage > 15
                && inMeleeRange
                && !target.HasAura("Mangle (Bear)")
                && cast.OnTarget(MangleBear))
                return;

            // Swipe
            List<IWoWUnit> closeEnemies = unitCache.EnemiesFighting
                .FindAll(e => e.GetDistance < 10)
                .ToList();
            if (closeEnemies.Count > 1
                && target.IsTargetingMe
                && cast.OnTarget(Swipe))
                return;

            // Lacerate
            if (WTEffects.CountDebuff("Lacerate", "target") < 5
                && cast.OnTarget(Lacerate))
                return;
        }
    }
}
