using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class FeralTankParty : Druid
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();

            // PARTY Remove Curse
            WoWPlayer needRemoveCurse = AIOParty.Group
                .Find(m => ToolBox.HasCurseDebuff(m.Name));
            if (needRemoveCurse != null && cast.OnFocusPlayer(RemoveCurse, needRemoveCurse))
                return;

            // PARTY Abolish Poison
            WoWPlayer needAbolishPoison = AIOParty.Group
                .Find(m => ToolBox.HasPoisonDebuff(m.Name));
            if (needAbolishPoison != null && cast.OnFocusPlayer(AbolishPoison, needAbolishPoison))
                return;

            // PARTY Mark of the Wild
            WoWPlayer needMotW = AIOParty.Group
                .Find(m => !m.HaveBuff(MarkOfTheWild.Name));
            if (needMotW != null && cast.OnFocusPlayer(MarkOfTheWild, needMotW))
                return;

            // PARTY Thorns
            WoWPlayer needThorns = AIOParty.Group
                .Find(m => !m.HaveBuff(Thorns.Name));
            if (needThorns != null && cast.OnFocusPlayer(Thorns, needThorns))
                return;

            // Omen of Clarity
            if (!Me.HaveBuff("Omen of Clarity") 
                && OmenOfClarity.IsSpellUsable
                && cast.Normal(OmenOfClarity))
                return;

            // PARTY Drink
            ToolBox.PartyDrink(settings.PartyDrinkName, settings.PartyDrinkThreshold);
        }

        protected override void Pull()
        {
            base.Pull();

            _pullFromAfar = true;

            if (ObjectManager.Target.Guid == Me.Guid)
                RangeManager.SetRangeToMelee();

            if (_pullMeleeTimer.ElapsedMilliseconds <= 0
                && ObjectManager.Target.GetDistance <= _pullRange)
                _pullMeleeTimer.Start();

            if (_pullMeleeTimer.ElapsedMilliseconds > 5000)
            {
                Logger.Log("Going in Melee range (pull)");
                RangeManager.SetRangeToMelee();
                ToolBox.CheckAutoAttack(Attack);
                _pullMeleeTimer.Reset();
            }

            // Dire Bear Form
            if (DireBearForm.KnownSpell
                && !Me.HaveBuff("Dire Bear Form")
                && cast.Normal(DireBearForm))
                return;

            // Bear Form
            if (!DireBearForm.KnownSpell
                && !Me.HaveBuff("Bear Form")
                && cast.Normal(BearForm))
                return;

            // Pull from afar
            if (_pullMeleeTimer.ElapsedMilliseconds < 5000
                && ObjectManager.Target.GetDistance <= _pullRange)
            {
                RangeManager.SetRange(_pullRange);
                if (FaerieFireFeral.KnownSpell)
                {
                    Logger.Log("Pulling with Faerie Fire (Feral)");
                    Lua.RunMacroText("/cast Faerie Fire (Feral)()");
                    Thread.Sleep(2000);
                    return;
                }
                else if (Moonfire.KnownSpell
                    && !ObjectManager.Target.HaveBuff("Moonfire")
                    && ObjectManager.Me.Level >= 10)
                {
                    Logger.Log("Pulling with Moonfire (Rank 1)");
                    Lua.RunMacroText("/cast Moonfire(Rank 1)");
                    Usefuls.WaitIsCasting();
                    return;
                }
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();
            bool _inMeleeRange = ObjectManager.Target.GetDistance < 6f;
            WoWUnit Target = ObjectManager.Target;

            if (_shouldBeInterrupted || _pullMeleeTimer.ElapsedMilliseconds <= 0)
                RangeManager.SetRangeToMelee();

            RegainAggro();

            // Check Auto-Attacking
            ToolBox.CheckAutoAttack(Attack);

            // Check if fighting a caster
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

            if ((_shouldBeInterrupted || _meleeTimer.ElapsedMilliseconds > 3000)
                && !RangeManager.CurrentRangeIsMelee())
            {
                Logger.Log("Going in Melee range (combat)");
                RangeManager.SetRangeToMelee();
                _meleeTimer.Stop();
            }

            // Party Tranquility
            if (settings.PartyTranquility && !AIOParty.Group.Any(e => e.IsTargetingMe))
            {
                bool needTranquility = AIOParty.Group
                    .FindAll(m => m.HealthPercent < 50)
                    .Count > 2;
                if (needTranquility
                    && cast.Normal(Tranquility))
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
                if (needRebirth != null && cast.OnFocusPlayer(Rebirth, needRebirth, onDeadTarget: true))
                    return;
            }

            // PARTY Innervate
            if (settings.PartyUseInnervate)
            {
                WoWPlayer needInnervate = AIOParty.Group
                    .Find(m => m.ManaPercentage < 10 && !m.HaveBuff("Innervate"));
                if (needInnervate != null && cast.OnFocusPlayer(Innervate, needInnervate))
                    return;
            }

            if (settings.PartyRemoveCurse)
            {
                // PARTY Remove Curse
                WoWPlayer needRemoveCurse = AIOParty.Group
                    .Find(m => ToolBox.HasCurseDebuff(m.Name));
                if (needRemoveCurse != null && cast.OnFocusPlayer(RemoveCurse, needRemoveCurse))
                    return;
            }

            if (settings.PartyAbolishPoison)
            {
                // PARTY Abolish Poison
                WoWPlayer needAbolishPoison = AIOParty.Group
                    .Find(m => ToolBox.HasPoisonDebuff(m.Name));
                if (needAbolishPoison != null && cast.OnFocusPlayer(AbolishPoison, needAbolishPoison))
                    return;
            }

            // Dire Bear Form
            if (DireBearForm.KnownSpell
                && !Me.HaveBuff("Dire Bear Form")
                && cast.Normal(DireBearForm))
                return;

            // Bear Form
            if (!DireBearForm.KnownSpell
                && !Me.HaveBuff("Bear Form")
                && cast.Normal(BearForm))
                return;

            // Feral Charge
            if (Target.GetDistance > 10
                && cast.Normal(FeralCharge))
                return;

            // Interrupt with Bash
            if (_shouldBeInterrupted
                && cast.Normal(Bash))
                return;

            // Taunt
            if (_inMeleeRange
                && !Target.IsTargetingMe
                && Target.Target > 0
                && cast.Normal(Growl))
                return;

            // Challenging roar
            if (_inMeleeRange
                && !Target.IsTargetingMe
                && Target.Target > 0
                && ToolBox.GetNbEnemiesClose(8) > 2
                && cast.Normal(ChallengingRoar))
                return;

            // Maul
            if (!MaulOn()
                && Me.Rage > 70)
                cast.Normal(Maul);

            // Frenzied Regeneration
            if (Me.HealthPercent < 50
                && cast.Normal(FrenziedRegeneration))
                return;

            // Enrage
            if (settings.UseEnrage
                && cast.Normal(Enrage))
                return;

            // Faerie Fire
            if (!Target.HaveBuff("Faerie Fire (Feral)")
                && FaerieFireFeral.KnownSpell
                && !cast.BannedSpells.Contains("Faerie Fire (Feral)"))
            {
                Lua.RunMacroText("/cast Faerie Fire (Feral)()");
                return;
            }

            // Demoralizing Roar
            if (!Target.HaveBuff("Demoralizing Roar")
                && Target.GetDistance < 9f
                && cast.Normal(DemoralizingRoar))
                return;

            // Mangle
            if (MangleBear.KnownSpell
                && Me.Rage > 15
                && _inMeleeRange
                && !Target.HaveBuff("Mangle (Bear)")
                && Wrath.IsSpellUsable
                && !cast.BannedSpells.Contains("Mangle (Bear)"))
            {
                Logging.WriteFight("[Spell] Cast Mangle (Bear)");
                Lua.RunMacroText("/cast Mangle (Bear)()");
                return;
            }

            // Swipe
            List<WoWUnit> closeEnemies = _partyEnemiesAround
                .Where(e => e.GetDistance < 10 && e.InCombatFlagOnly)
                .ToList();
            if (closeEnemies.Count > 1
                && Target.IsTargetingMe
                && cast.Normal(Swipe))
                return;

            // Lacerate
            if (ToolBox.CountDebuff("Lacerate", "target") < 5
                && cast.Normal(Lacerate))
                return;
        }
    }
}
