using robotManager.Helpful;
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

            // Bear Form
            if (!CatForm.KnownSpell
                && !Me.HaveBuff(BearForm.Name)
                && cast.Normal(BearForm))
                return;

            // Cat Form
            if (!Me.HaveBuff(CatForm.Name)
                && cast.Normal(CatForm))
                return;

            // Prowl
            if (Me.HaveBuff(CatForm.Name)
                && !Me.HaveBuff(Prowl.Name)
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f
                && settings.StealthEngage
                && cast.Normal(Prowl))
                return;

            // Pull Bear/Cat
            if (Me.HaveBuff(BearForm.Name)
                || Me.HaveBuff(DireBearForm.Name)
                || Me.HaveBuff(CatForm.Name))
            {
                RangeManager.SetRangeToMelee();

                // Prowl approach
                if (Me.HaveBuff(Prowl.Name)
                    && ObjectManager.Target.GetDistance > 3f
                    && !_isStealthApproching)
                {
                    _stealthApproachTimer.Start();
                    _isStealthApproching = true;
                    if (ObjectManager.Me.IsAlive
                        && ObjectManager.Target.IsAlive)
                    {

                        while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                        && (ObjectManager.Target.GetDistance > 2.5f || !Claw.IsSpellUsable)
                        && Fight.InFight
                        && _stealthApproachTimer.ElapsedMilliseconds <= 7000
                        && Me.HaveBuff(Prowl.Name))
                        {
                            Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f);
                            MovementManager.MoveTo(position);
                            Thread.Sleep(50);
                            CastOpener();
                        }

                        if (_stealthApproachTimer.ElapsedMilliseconds > 7000)
                            _pullFromAfar = true;

                        _isStealthApproching = false;
                    }
                }
                return;
            }
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            bool _shouldBeInterrupted = ToolBox.TargetIsCasting();
            bool _inMeleeRange = ObjectManager.Target.GetDistance < 6f;
            WoWUnit Target = ObjectManager.Target;

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

            // Catorm
            if (!Me.HaveBuff(CatForm.Name)
                && cast.Normal(CatForm))
                return;

            // Bear Form
            if (!Me.HaveBuff(BearForm.Name)
                && !Me.HaveBuff(DireBearForm.Name)
                && !CatForm.KnownSpell)
                {
                    if (cast.Normal(DireBearForm) || cast.Normal(BearForm))
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
                    && Shred.KnownSpell)
                {
                    cast.Normal(Shred);
                    return;
                }

                // Faerie Fire
                if (!Target.HaveBuff("Faerie Fire (Feral)")
                    && !cast.BannedSpells.Contains("Faerie Fire (Feral)"))
                {
                    Lua.RunMacroText("/cast Faerie Fire (Feral)()");
                    return;
                }

                // Mangle
                if (Me.ComboPoint < 5
                    && Me.Energy > 40
                    && MangleCat.KnownSpell
                    && Claw.IsSpellUsable
                    && !cast.BannedSpells.Contains("Mangle (Cat)"))
                {
                    Logging.WriteFight("[Spell] Cast Mangle (Cat)");
                    Lua.RunMacroText("/cast Mangle (Cat)()");
                    return;
                }

                // Rip
                if (Me.ComboPoint >= 5
                    && Me.Energy >= 80
                    && cast.Normal(Rip))
                    return;

                // Rip
                if (Me.ComboPoint >= 5
                    && Me.Energy >= 80
                    && cast.Normal(Rip))
                    return;

                // Rip
                if (Me.ComboPoint >= 4
                    && Me.Energy >= 80
                    && !Target.HaveBuff("Mangle (Cat)")
                    && cast.Normal(Rip))
                    return;

                // Claw
                if (Me.ComboPoint < 5
                    && !MangleCat.KnownSpell
                    && cast.Normal(Claw))
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
                    && cast.Normal(FrenziedRegeneration))
                    return;

                // Faerie Fire
                if (!Target.HaveBuff("Faerie Fire (Feral)")
                    && FaerieFireFeral.KnownSpell
                    && !cast.BannedSpells.Contains("Faerie Fire (Feral)"))
                {
                    Lua.RunMacroText("/cast Faerie Fire (Feral)()");
                    return;
                }

                // Swipe
                if (ObjectManager.GetNumberAttackPlayer() > 1 
                    && ToolBox.GetNbEnemiesClose(8f) > 1
                    && cast.Normal(Swipe))
                    return;

                // Interrupt with Bash
                if (_shouldBeInterrupted)
                {
                    Thread.Sleep(Main.humanReflexTime);
                    if (cast.Normal(Bash))
                        return;
                }

                // Enrage
                if (settings.UseEnrage
                    && cast.Normal(Enrage))
                    return;

                // Demoralizing Roar
                if (!Target.HaveBuff("Demoralizing Roar") 
                    && Target.GetDistance < 9f
                    && cast.Normal(DemoralizingRoar))
                    return;

                // Maul
                if (!MaulOn() 
                    && (!_fightingACaster || Me.Rage > 30)
                    && cast.Normal(Maul))
                    return;
            }

            #endregion

            #region Human form rotation

            // **************** HUMAN FORM ROTATION ****************

            // Avoid accidental Human Form stay
            if (CatForm.KnownSpell && ToolBox.GetSpellCost(CatForm.Name) < Me.Mana)
                return;
            if (BearForm.KnownSpell && ToolBox.GetSpellCost(BearForm.Name) < Me.Mana)
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
                    && cast.Normal(Moonfire))
                    return;

                // Wrath
                if (Target.GetDistance <= _pullRange
                    && Me.ManaPercentage > 45
                    && Target.HealthPercent > 30
                    && Me.Level >= 8
                    && cast.Normal(Wrath))
                    return;

                // Moonfire Low level DPS
                if (!Target.HaveBuff(Moonfire.Name)
                    && Me.ManaPercentage > 50
                    && Target.HealthPercent > 30
                    && Me.Level < 8
                    && cast.Normal(Moonfire))
                    return;

                // Wrath Low level DPS
                if (Target.GetDistance <= _pullRange
                    && Me.ManaPercentage > 60
                    && Target.HealthPercent > 30
                    && Me.Level < 8
                    && cast.Normal(Wrath))
                    return;
            }
            #endregion
        }
    }
}
