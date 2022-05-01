using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
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
            if (!Me.HaveBuff("Drink") || Me.ManaPercentage > 95)
            {
                base.BuffRotation();

                // PARTY Remove Curse
                WoWPlayer needRemoveCurse = partyManager.GroupAndRaid
                    .Find(m => WTEffects.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                    return;

                // PARTY Abolish Poison
                WoWPlayer needAbolishPoison = partyManager.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                    return;

                // PARTY Mark of the Wild
                WoWPlayer needMotW = partyManager.GroupAndRaid
                    .Find(m => !m.HaveBuff(MarkOfTheWild.Name));
                if (needMotW != null && cast.OnFocusUnit(MarkOfTheWild, needMotW))
                    return;

                // PARTY Thorns
                WoWPlayer needThorns = partyManager.GroupAndRaid
                    .Find(m => !m.HaveBuff(Thorns.Name));
                if (needThorns != null && cast.OnFocusUnit(Thorns, needThorns))
                    return;

                // Omen of Clarity
                if (!Me.HaveBuff("Omen of Clarity")
                    && OmenOfClarity.IsSpellUsable
                    && cast.OnSelf(OmenOfClarity))
                    return;

                // PARTY Drink
                if (partyManager.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold))
                    return;

                // Dire Bear Form
                if (!Me.HaveBuff("Dire Bear Form")
                    && cast.OnSelf(DireBearForm))
                    return;

                // Bear Form
                if (!DireBearForm.KnownSpell
                    && !Me.HaveBuff("Bear Form")
                    && cast.OnSelf(BearForm))
                    return;
            }
        }

        protected override void Pull()
        {
            base.Pull();

            // Dire Bear Form
            if (!Me.HaveBuff("Dire Bear Form")
                && cast.OnSelf(DireBearForm))
                return;

            // Bear Form
            if (!DireBearForm.KnownSpell
                && !Me.HaveBuff("Bear Form")
                && cast.OnSelf(BearForm))
                return;

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Pull logic
            if (ToolBox.Pull(cast, settings.AlwaysPull, new List<AIOSpell> { FaerieFireFeral, MoonfireRank1, Wrath }))
            {
                _combatMeleeTimer = new Timer(1000);
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

            bool _shouldBeInterrupted = WTCombat.TargetIsCasting();
            bool _inMeleeRange = ObjectManager.Target.GetDistance < 6f;
            WoWUnit Target = ObjectManager.Target;

            if (settings.PartyTankSwitchTarget)
                partyManager.SwitchTarget(cast, null);

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
            if (settings.PartyTranquility && !partyManager.GroupAndRaid.Any(e => e.IsTargetingMe))
            {
                bool needTranquility = partyManager.GroupAndRaid
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
                WoWPlayer needRebirth = partyManager.GroupAndRaid
                    .Find(m => m.IsDead);
                if (needRebirth != null && cast.OnFocusUnit(Rebirth, needRebirth))
                    return;
            }

            // PARTY Innervate
            if (settings.PartyUseInnervate)
            {
                WoWPlayer needInnervate = partyManager.GroupAndRaid
                    .Find(m => m.ManaPercentage < 10 && !m.HaveBuff("Innervate"));
                if (needInnervate != null && cast.OnFocusUnit(Innervate, needInnervate))
                    return;
            }

            if (settings.PartyRemoveCurse)
            {
                // PARTY Remove Curse
                WoWPlayer needRemoveCurse = partyManager.GroupAndRaid
                    .Find(m => WTEffects.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusUnit(RemoveCurse, needRemoveCurse))
                    return;
            }

            if (settings.PartyAbolishPoison)
            {
                // PARTY Abolish Poison
                WoWPlayer needAbolishPoison = partyManager.GroupAndRaid
                    .Find(m => WTEffects.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusUnit(AbolishPoison, needAbolishPoison))
                    return;
            }

            // Dire Bear Form
            if (DireBearForm.KnownSpell
                && !Me.HaveBuff("Dire Bear Form")
                && cast.OnSelf(DireBearForm))
                return;

            // Bear Form
            if (!DireBearForm.KnownSpell
                && !Me.HaveBuff("Bear Form")
                && cast.OnSelf(BearForm))
                return;

            // Feral Charge
            if (Target.GetDistance > 10
                && cast.OnTarget(FeralCharge))
                return;

            // Interrupt with Bash
            if (_shouldBeInterrupted
                && cast.OnTarget(Bash))
                return;

            // Taunt
            if (_inMeleeRange
                && !Target.IsTargetingMe
                && Target.Target > 0
                && cast.OnTarget(Growl))
                return;

            // Challenging roar
            if (_inMeleeRange
                && !Target.IsTargetingMe
                && Target.Target > 0
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
            if (!Target.HaveBuff("Faerie Fire (Feral)")
                && cast.OnTarget(FaerieFireFeral))
                return;

            // Demoralizing Roar
            if (!Target.HaveBuff("Demoralizing Roar")
                && !Target.HaveBuff("Demoralizing Shout")
                && Target.GetDistance < 9f
                && cast.OnTarget(DemoralizingRoar))
                return;

            // Mangle
            if (MangleBear.KnownSpell
                && Me.Rage > 15
                && _inMeleeRange
                && !Target.HaveBuff("Mangle (Bear)")
                && cast.OnTarget(MangleBear))
                return;

            // Swipe
            List<WoWUnit> closeEnemies = partyManager.EnemiesFighting
                .FindAll(e => e.GetDistance < 10)
                .ToList();
            if (closeEnemies.Count > 1
                && Target.IsTargetingMe
                && cast.OnTarget(Swipe))
                return;

            // Lacerate
            if (WTEffects.CountDebuff("Lacerate", "target") < 5
                && cast.OnTarget(Lacerate))
                return;
        }
    }
}
