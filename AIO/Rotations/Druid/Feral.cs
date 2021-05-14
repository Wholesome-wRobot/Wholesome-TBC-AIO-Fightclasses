using robotManager.Helpful;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Druid
{
    public class Feral : Druid
    {
        protected override void BuffRotation()
        {
            string currentGroundMount = wManager.wManagerSetting.CurrentSetting.GroundMountName;

            // Regrowth
            if (Me.HealthPercent < 70
                && !Me.HaveBuff("Regrowth"))
                if (cast.Normal(Regrowth))
                    return;

            // Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Rejuvenation")
                && !Regrowth.KnownSpell)
                if (cast.Normal(Rejuvenation))
                    return;

            // Healing Touch
            if (Me.HealthPercent < 40
                && !Regrowth.KnownSpell)
                if (cast.Normal(HealingTouch))
                    return;

            // Remove Curse
            if (ToolBox.HasCurseDebuff()
                && RemoveCurse.KnownSpell
                && RemoveCurse.IsSpellUsable)
            {
                Thread.Sleep(200);
                if (cast.Normal(RemoveCurse, true))
                    return;
            }

            // Abolish Poison
            if (ToolBox.HasPoisonDebuff()
                && RemoveCurse.KnownSpell
                && RemoveCurse.IsSpellUsable)
            {
                Thread.Sleep(200);
                if (cast.Normal(AbolishPoison, true))
                    return;
            }

            // Mark of the Wild
            if (!Me.HaveBuff("Mark of the Wild")
                && MarkOfTheWild.KnownSpell
                && MarkOfTheWild.IsSpellUsable)
            {
                Thread.Sleep(200);
                if (cast.OnSelf(MarkOfTheWild))
                    return;
            }

            // Thorns
            if (!Me.HaveBuff("Thorns")
                && Thorns.KnownSpell
                && Thorns.IsSpellUsable)
            {
                Thread.Sleep(200);
                if (cast.OnSelf(Thorns))
                    return;
            }

            // Omen of Clarity
            if (!Me.HaveBuff("Omen of Clarity") && OmenOfClarity.IsSpellUsable)
                if (cast.Normal(OmenOfClarity))
                    return;

            // Aquatic form
            if (Me.IsSwimming
                && !Me.HaveBuff("Aquatic Form")
                && Me.ManaPercentage > 50)
                if (cast.Normal(AquaticForm))
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
            if (!BearForm.KnownSpell
                && !CatForm.KnownSpell)
                _pullFromAfar = true;

            // Check if surrounding enemies
            if (!_pullFromAfar)
                _pullFromAfar = ToolBox.CheckIfEnemiesAround(ObjectManager.Target, 20f);

            // Get in pull distance
            if (_pullFromAfar && ObjectManager.Target.GetDistance >= _pullRange)
                return;

            // Pull from afar
            if ((_pullFromAfar && _pullMeleeTimer.ElapsedMilliseconds < 5000 || settings.AlwaysPull)
                && ObjectManager.Target.GetDistance <= _pullRange)
                if (PullSpell())
                    return;

            //Main.Log("pullfromafar is " + _pullFromAfar + " timer : " + _pullMeleeTimer.ElapsedMilliseconds);

            // Melee ?
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

            // Check if caster in list
            if (_casterEnemies.Contains(ObjectManager.Target.Name))
                _fightingACaster = true;

            // Bear Form
            if (!CatForm.KnownSpell
                && !Me.HaveBuff("Bear Form")
                && !_pullFromAfar)
                if (cast.Normal(BearForm))
                    return;

            // Cat Form
            if (!Me.HaveBuff("Cat Form")
                && !_pullFromAfar
                && ObjectManager.Target.Guid > 0)
                if (cast.Normal(CatForm))
                    return;

            // Prowl
            if (Me.HaveBuff("Cat Form")
                && !_pullFromAfar
                && ObjectManager.Target.GetDistance > 15f
                && ObjectManager.Target.GetDistance < 25f
                && settings.StealthEngage)
                if (cast.Normal(Prowl))
                    return;

            // Pull Bear/Cat
            if (Me.HaveBuff("Bear Form")
                || Me.HaveBuff("Dire Bear Form")
                || Me.HaveBuff("Cat Form")
                || !_pullFromAfar)
            {
                RangeManager.SetRangeToMelee();

                // Prowl approach
                if (Me.HaveBuff("Prowl")
                    && ObjectManager.Target.GetDistance > 3f
                    && !_isStealthApproching)
                {
                    float desiredDistance = RangeManager.GetMeleeRangeWithTarget() - 4f;
                    _stealthApproachTimer.Start();
                    _isStealthApproching = true;
                    if (ObjectManager.Me.IsAlive
                        && ObjectManager.Target.IsAlive)
                    {

                        while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                        && (ObjectManager.Target.GetDistance > 2.5f || !Claw.IsSpellUsable)
                        && !ToolBox.CheckIfEnemiesAround(ObjectManager.Target, 20f)
                        && Fight.InFight
                        && _stealthApproachTimer.ElapsedMilliseconds <= 7000
                        && Me.HaveBuff("Prowl"))
                        {
                            Vector3 position = ToolBox.BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2.5f);
                            MovementManager.MoveTo(position);
                            Thread.Sleep(50);
                            CastOpener();
                        }

                        if (_stealthApproachTimer.ElapsedMilliseconds > 7000)
                            _pullFromAfar = true;

                        //ToolBox.CheckAutoAttack(Attack);
                        _isStealthApproching = false;
                    }
                }
                return;
            }

            // Pull from distance
            if (_pullFromAfar
                && ObjectManager.Target.GetDistance <= _pullRange)
                if (PullSpell())
                    return;
        }

        protected override void CombatRotation()
        {
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

            // Innervate
            if (settings.UseInnervate
                && Me.ManaPercentage < 20)
                if (cast.Normal(Innervate))
                    return;

            // Barkskin + Regrowth + Rejuvenation
            if (settings.UseBarkskin
                && Barkskin.KnownSpell
                && Me.HealthPercent < 50
                && !Me.HaveBuff("Regrowth")
                && Me.Mana > bigHealComboCost + ToolBox.GetSpellCost("Barkskin")
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.Normal(Barkskin) && cast.Normal(Regrowth) && cast.Normal(Rejuvenation))
                    return;

            // Regrowth + Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Regrowth")
                && Me.Mana > bigHealComboCost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.Normal(Regrowth) && cast.Normal(Rejuvenation))
                    return;

            // Regrowth
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Regrowth")
                && Me.Mana > smallHealComboCost
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.Normal(Regrowth))
                    return;

            // Rejuvenation
            if (Me.HealthPercent < 50
                && !Me.HaveBuff("Rejuvenation")
                && !Regrowth.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.Normal(Rejuvenation))
                    return;

            // Healing Touch
            if (Me.HealthPercent < 30
                && !Regrowth.KnownSpell
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.Normal(HealingTouch))
                    return;

            // Catorm
            if (!Me.HaveBuff("Cat Form")
                && (ObjectManager.GetNumberAttackPlayer() < settings.NumberOfAttackersBearForm || !BearForm.KnownSpell && !DireBearForm.KnownSpell))
                if (cast.Normal(CatForm))
                    return;

            // Bear Form
            if (!Me.HaveBuff("Bear Form")
                && !Me.HaveBuff("Dire Bear Form"))
            {
                if (!CatForm.KnownSpell)
                {
                    if (cast.Normal(DireBearForm) || cast.Normal(BearForm))
                        return;
                }
                else if (ObjectManager.GetNumberAttackPlayer() >= settings.NumberOfAttackersBearForm
                        && settings.NumberOfAttackersBearForm > 1)
                {
                    {
                        if (cast.Normal(DireBearForm) || cast.Normal(BearForm))
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
                if (Target.HaveBuff("Pounce"))
                    if (cast.Normal(Shred))
                        return;

                // Faerie Fire
                if (!Target.HaveBuff("Faerie Fire (Feral)")
                    && FaerieFireFeral.KnownSpell
                    && !Target.HaveBuff("Pounce")
                    && !cast.BannedSpells.Contains("Faerie Fire (Feral)"))
                {
                    Lua.RunMacroText("/cast Faerie Fire (Feral)()");
                    return;
                }

                // Rip
                if (!Target.HaveBuff("Rip")
                    && !Target.HaveBuff("Pounce"))
                {
                    if (Me.ComboPoint >= 3
                        && Target.HealthPercent > 60)
                        if (cast.Normal(Rip))
                            return;

                    if (Me.ComboPoint >= 1
                        && Target.HealthPercent <= 60)
                        if (cast.Normal(Rip))
                            return;
                }

                // Ferocious Bite
                if (FerociousBite.KnownSpell
                    && !Target.HaveBuff("Pounce"))
                {
                    if (Me.ComboPoint >= 3
                        && Target.HealthPercent > 60)
                        if (cast.Normal(FerociousBite))
                            return;

                    if (Me.ComboPoint >= 1
                        && Target.HealthPercent <= 60)
                        if (cast.Normal(FerociousBite))
                            return;
                }

                // Rake
                if (!Target.HaveBuff("Rake")
                    && !Target.HaveBuff("Pounce"))
                    if (cast.Normal(Rake))
                        return;

                // Tiger's Fury
                if (!TigersFury.HaveBuff
                    && settings.UseTigersFury
                    && Me.ComboPoint < 1
                    && !Target.HaveBuff("Pounce")
                    && Me.Energy > 30)
                    if (cast.Normal(TigersFury))
                        return;

                // Mangle
                if (Me.ComboPoint < 5
                    && !Target.HaveBuff("Pounce")
                    && Me.Energy > 40
                    && MangleCat.KnownSpell
                    && Claw.IsSpellUsable
                    && !cast.BannedSpells.Contains("Mangle (Cat)"))
                {
                    Logging.WriteFight("[Spell] Cast Mangle (Mangle)");
                    Lua.RunMacroText("/cast Mangle (Cat)()");
                    return;
                }

                // Claw
                if (Me.ComboPoint < 5 && !Target.HaveBuff("Pounce")
                    && !MangleCat.KnownSpell)
                    if (cast.Normal(Claw))
                        return;
            }

            #endregion

            #region Bear form rotation

            // **************** BEAR FORM ROTATION ****************

            if (Me.HaveBuff("Bear Form") || Me.HaveBuff("Dire Bear Form"))
            {
                RangeManager.SetRangeToMelee();

                // Frenzied Regeneration
                if (Me.HealthPercent < 50)
                    if (cast.Normal(FrenziedRegeneration))
                        return;

                // Faerie Fire
                if (!Target.HaveBuff("Faerie Fire (Feral)")
                    && FaerieFireFeral.KnownSpell
                    && !cast.BannedSpells.Contains("Faerie Fire (Feral)"))
                {
                    Lua.RunMacroText("/cast Faerie Fire (Feral)()");
                }

                // Swipe
                if (ObjectManager.GetNumberAttackPlayer() > 1 && ToolBox.GetNbEnemiesClose(8f) > 1)
                    if (cast.Normal(Swipe))
                        return;

                // Interrupt with Bash
                if (_shouldBeInterrupted)
                {
                    Thread.Sleep(Main.humanReflexTime);
                    if (cast.Normal(Bash))
                        return;
                }

                // Enrage
                if (settings.UseEnrage)
                    if (cast.Normal(Enrage))
                        return;

                // Demoralizing Roar
                if (!Target.HaveBuff("Demoralizing Roar") && Target.GetDistance < 9f)
                    if (cast.Normal(DemoralizingRoar))
                        return;

                // Maul
                if (!MaulOn() && (!_fightingACaster || Me.Rage > 30))
                    if (cast.Normal(Maul))
                        return;
            }

            #endregion

            #region Human form rotation

            // **************** HUMAN FORM ROTATION ****************

            // Avoid accidental Human Form stay
            if (CatForm.KnownSpell && ToolBox.GetSpellCost("Cat Form") < Me.Mana)
                return;
            if (BearForm.KnownSpell && ToolBox.GetSpellCost("Bear Form") < Me.Mana)
                return;

            if (!Me.HaveBuff("Bear Form")
                && !Me.HaveBuff("Cat Form")
                && !Me.HaveBuff("Dire Bear Form"))
            {
                // Moonfire
                if (!Target.HaveBuff("Moonfire")
                    && Me.ManaPercentage > 15
                    && Target.HealthPercent > 15
                    && Me.Level >= 8)
                    if (cast.Normal(Moonfire))
                        return;

                // Wrath
                if (Target.GetDistance <= _pullRange
                    && Me.ManaPercentage > 45
                    && Target.HealthPercent > 30
                    && Me.Level >= 8)
                    if (cast.Normal(Wrath))
                        return;

                // Moonfire Low level DPS
                if (!Target.HaveBuff("Moonfire")
                    && Me.ManaPercentage > 50
                    && Target.HealthPercent > 30
                    && Me.Level < 8)
                    if (cast.Normal(Moonfire))
                        return;

                // Wrath Low level DPS
                if (Target.GetDistance <= _pullRange
                    && Me.ManaPercentage > 60
                    && Target.HealthPercent > 30
                    && Me.Level < 8)
                    if (cast.Normal(Wrath))
                        return;
            }
            #endregion
        }
    }
}
