using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Warlock
{
    public class Warlock : IClassRotation
    {
        public static WarlockSettings settings;

        protected BackgroundWorker _petPulseThread = new BackgroundWorker();
        protected Stopwatch _addCheckTimer = new Stopwatch();
        protected WoWLocalPlayer Me = ObjectManager.Me;

        protected float _maxRange = 27f;
        protected int _innerManaSaveThreshold = 20;
        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected int _saveDrinkPercent = wManager.wManagerSetting.CurrentSetting.DrinkPercent;

        protected Warlock specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = WarlockSettings.Current;

            this.specialization = specialization as Warlock;
            Talents.InitTalents(settings);

            _petPulseThread.DoWork += PetThread;
            _petPulseThread.RunWorkerAsync();

            RangeManager.SetRange(_maxRange);

            // Set pet mode
            if (settings.PetInPassiveWhenOOC)
                Lua.LuaDoString("PetPassiveMode();");
            else
                Lua.LuaDoString("PetDefensiveMode();");

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;

            Rotation();
        }

        public void Dispose()
        {
            _petPulseThread.DoWork -= PetThread;
            _petPulseThread.Dispose();
            Lua.LuaDoString("PetPassiveMode();");
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;
            Logger.Log("Disposed");
        }

        // Pet thread
        protected void PetThread(object sender, DoWorkEventArgs args)
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions()
                        && ObjectManager.Pet.IsValid )
                    {
                        // Voidwalker Torment + Felguard Anguish
                        if ((!settings.AutoTorment || !settings.AutoAnguish)
                            && ObjectManager.Target.Target == Me.Guid
                            && Me.InCombatFlagOnly)
                        {
                            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                                ToolBox.PetSpellCast("Torment");
                            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard"))
                                ToolBox.PetSpellCast("Anguish");
                        }

                        // Switch Auto Torment & Suffering off
                        if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                        {
                            ToolBox.TogglePetSpellAuto("Torment", settings.AutoTorment);
                            ToolBox.TogglePetSpellAuto("Suffering", false);
                        }

                        // Switch Felguard Auto Cleave/Anguish
                        if (WarlockPetAndConsumables.MyWarlockPet().Equals("Felguard"))
                        {
                            ToolBox.TogglePetSpellAuto("Cleave", settings.FelguardCleave);
                            ToolBox.TogglePetSpellAuto("Anguish", settings.AutoAnguish);
                        }
                    }
                }
                catch (Exception arg)
                {
                    Logging.WriteError(string.Concat(arg), true);
                }
                Thread.Sleep(300);
            }
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();
                }
                catch (Exception arg)
                {
                    Logging.WriteError("ERROR: " + arg, true);
                }
                Thread.Sleep(ToolBox.GetLatency() + settings.ThreadSleepCycle);
            }
            Logger.Log("Stopped.");
        }

        protected virtual void BuffRotation()
        {
            // Delete additional Soul Shards
            if (ToolBox.CountItemStacks("Soul Shard") > settings.NumberOfSoulShards)
            {
                Logger.Log("Deleting excess Soul Shard");
                ToolBox.LuaDeleteOneItem("Soul Shard");
            }

            // Make sure we have mana to summon
            if (!ObjectManager.Pet.IsValid
                && ObjectManager.Me.ManaPercentage < 95
                && !ObjectManager.Me.HaveBuff("Drink")
                && (SummonVoidwalker.KnownSpell && !SummonVoidwalker.IsSpellUsable && ToolBox.CountItemStacks("Soul Shard") > 0 ||
                SummonImp.KnownSpell && !SummonImp.IsSpellUsable && !SummonVoidwalker.KnownSpell))
            {
                Logger.Log("Not enough mana to summon, forcing regen");
                wManager.wManagerSetting.CurrentSetting.DrinkPercent = 95;
                Thread.Sleep(1000);
                return;
            }
            else
                wManager.wManagerSetting.CurrentSetting.DrinkPercent = _saveDrinkPercent;

            // Summon Felguard
            if ((!ObjectManager.Pet.IsValid
                || WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker") || WarlockPetAndConsumables.MyWarlockPet().Equals("Imp"))
                && SummonFelguard.KnownSpell)
            {
                Thread.Sleep(Usefuls.Latency + 500); // Safety for Mount check
                if (!ObjectManager.Me.IsMounted)
                {
                    if (Cast(FelDomination))
                        Thread.Sleep(200);
                    if (Cast(SummonFelguard))
                        return;
                }
            }

            // Summon Felguard for mana or health
            if (SummonFelguard.KnownSpell
                && (ObjectManager.Pet.ManaPercentage < 20 || ObjectManager.Pet.HealthPercent < 20)
                && ObjectManager.Pet.IsValid)
            {
                Thread.Sleep(Usefuls.Latency + 500); // Safety for Mount check
                if (!ObjectManager.Me.IsMounted)
                {
                    if (CastStopMove(FelDomination))
                        Thread.Sleep(200);
                    if (CastStopMove(SummonFelguard))
                        return;
                }
            }

            // Summon Void Walker
            if ((!ObjectManager.Pet.IsValid || !WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker"))
                && SummonVoidwalker.KnownSpell
                && !SummonFelguard.KnownSpell)
            {
                Thread.Sleep(Usefuls.Latency + 500); // Safety for Mount check
                if (!ObjectManager.Me.IsMounted)
                {
                    if (CastStopMove(FelDomination))
                        Thread.Sleep(200);
                    if (CastStopMove(SummonVoidwalker))
                        return;
                }
            }

            // Summon Void Walker for mana
            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker")
                && SummonVoidwalker.KnownSpell
                && ObjectManager.Pet.ManaPercentage < 20
                && !SummonFelguard.KnownSpell)
            {
                Thread.Sleep(Usefuls.Latency + 500); // Safety for Mount check
                if (!ObjectManager.Me.IsMounted)
                {
                    if (CastStopMove(FelDomination))
                        Thread.Sleep(200);
                    if (CastStopMove(SummonVoidwalker))
                        return;
                }
            }

            // Summon Imp
            if (!ObjectManager.Pet.IsValid && SummonImp.KnownSpell
                && (!SummonVoidwalker.KnownSpell || ToolBox.CountItemStacks("Soul Shard") < 1))
            {
                Thread.Sleep(Usefuls.Latency + 500); // Safety for Mount check
                if (!ObjectManager.Me.IsMounted)
                {
                    if (CastStopMove(FelDomination))
                        Thread.Sleep(200);
                    if (CastStopMove(SummonImp))
                        return;
                }
            }

            // Life Tap
            if (Me.HealthPercent > Me.ManaPercentage
                && settings.UseLifeTap
                && !Me.IsMounted)
                if (Cast(LifeTap))
                    return;

            // Unending Breath
            if (!Me.HaveBuff("Unending Breath")
                && UnendingBreath.KnownSpell
                && UnendingBreath.IsSpellUsable
                && settings.UseUnendingBreath)
            {
                if (CastOnSelf(UnendingBreath))
                    return;
            }

            // Demon Skin
            if (!Me.HaveBuff("Demon Skin")
                && !DemonArmor.KnownSpell
                && DemonSkin.KnownSpell)
                if (Cast(DemonSkin))
                    return;

            // Demon Armor
            if ((!Me.HaveBuff("Demon Armor") || Me.HaveBuff("Demon Skin"))
                && DemonArmor.KnownSpell
                && (!FelArmor.KnownSpell || FelArmor.KnownSpell && !settings.UseFelArmor))
                if (Cast(DemonArmor))
                    return;

            // Soul Link
            if (SoulLink.KnownSpell
                && !Me.HaveBuff("Soul Link"))
                if (Cast(SoulLink))
                    return;

            // Fel Armor
            if (!Me.HaveBuff("Fel Armor")
                && FelArmor.KnownSpell
                && settings.UseFelArmor)
                if (Cast(FelArmor))
                    return;

            // Health Funnel
            if (ObjectManager.Pet.HealthPercent < 50
                && Me.HealthPercent > 40
                && ObjectManager.Pet.GetDistance < 19
                && !ObjectManager.Pet.InCombatFlagOnly
                && HealthFunnel.KnownSpell)
            {
                Fight.StopFight();
                if (WarlockPetAndConsumables.MyWarlockPet().Equals("Voidwalker")
                    && ToolBox.GetPetSpellIndex("Consume Shadows") != 0)
                {
                    ToolBox.PetSpellCast("Consume Shadows");
                    Usefuls.WaitIsCasting();
                    Thread.Sleep(500);
                }


                ToolBox.StopWandWaitGCD(UseWand, ShadowBolt);
                if (CastStopMove(HealthFunnel))
                {
                    Thread.Sleep(500);
                    Usefuls.WaitIsCasting();
                    return;
                }
            }

            // Health Stone
            if (!WarlockPetAndConsumables.HaveHealthstone())
                if (Cast(CreateHealthStone))
                    return;

            // Create Soul Stone
            if (!WarlockPetAndConsumables.HaveSoulstone()
                && CreateSoulstone.KnownSpell)
            {
                if (CastStopMove(CreateSoulstone))
                    return;
            }

            // Use Soul Stone
            if (!Me.HaveBuff("Soulstone Resurrection")
                && settings.UseSoulStone
                && CreateSoulstone.KnownSpell
                && ToolBox.HaveOneInList(WarlockPetAndConsumables.SoulStones())
                && ToolBox.GetItemCooldown(WarlockPetAndConsumables.SoulStones()) <= 0)
            {
                Lua.RunMacroText("/target player");
                WarlockPetAndConsumables.UseSoulstone();
                Usefuls.WaitIsCasting();
                Lua.RunMacroText("/cleartarget");
            }
        }

        protected virtual void Pull()
        {
            // Curse of Agony
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !ObjectManager.Target.HaveBuff("Curse of Agony"))
                if (Cast(CurseOfAgony))
                    return;

            // Corruption
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !ObjectManager.Target.HaveBuff("Corruption"))
                if (Cast(Corruption))
                    return;

            // Immolate
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !ObjectManager.Target.HaveBuff("Immolate")
                && !ObjectManager.Target.HaveBuff("Fire Ward")
                && !Corruption.KnownSpell
                && ToolBox.CanBleed(ObjectManager.Target))
                if (Cast(Immolate))
                    return;

            // Shadow Bolt
            if (ObjectManager.Target.GetDistance < _maxRange + 2
                && !Immolate.KnownSpell)
                if (Cast(ShadowBolt))
                    return;
        }

        protected virtual void CombatRotation()
        {
            WoWUnit Me = ObjectManager.Me;
            WoWUnit Target = ObjectManager.Target;
            double _myManaPC = Me.ManaPercentage;
            bool _overLowManaThreshold = _myManaPC > _innerManaSaveThreshold;

            // Pet attack
            if (ObjectManager.Pet.Target != ObjectManager.Me.Target)
                Lua.LuaDoString("PetAttack();", false);

            // Drain Soul
            bool _shouldDrainSoul = ToolBox.CountItemStacks("Soul Shard") < settings.NumberOfSoulShards || settings.AlwaysDrainSoul;
            if (_shouldDrainSoul
                && Target.HealthPercent < settings.DrainSoulHP
                && DrainSoul.KnownSpell)
                if (settings.DrainSoulLevel1)
                {
                    Lua.RunMacroText("/cast Drain Soul(Rank 1)");
                    Usefuls.WaitIsCasting();
                }
                else
                {
                    if (Cast(DrainSoul))
                        return;
                }

            // How of Terror
            if (HowlOfTerror.KnownSpell
                && HowlOfTerror.IsSpellUsable
                && ToolBox.GetNumberEnemiesAround(10f, Me) > 1)
                if (Cast(HowlOfTerror))
                    return;

            // Use Health Stone
            if (Me.HealthPercent < 15)
                WarlockPetAndConsumables.UseHealthstone();

            // Shadow Trance
            if (Me.HaveBuff("Shadow Trance") && _overLowManaThreshold)
                if (Cast(ShadowBolt))
                    return;

            // Siphon Life
            if (Me.HealthPercent < 90
                && _overLowManaThreshold
                && Target.HealthPercent > 20
                && !Target.HaveBuff("Siphon Life")
                && settings.UseSiphonLife)
                if (Cast(SiphonLife))
                    return;

            // Death Coil
            if (Me.HealthPercent < 20)
                if (Cast(DeathCoil))
                    return;

            // Drain Life low
            if (Me.HealthPercent < 30
                && Target.HealthPercent > 20)
                if (Cast(DrainLife))
                    return;

            // Curse of Agony
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Curse of Agony")
                && _overLowManaThreshold
                && Target.HealthPercent > 20)
                if (Cast(CurseOfAgony))
                    return;

            // Unstable Affliction
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Unstable Affliction")
                && _overLowManaThreshold
                && Target.HealthPercent > 30)
                if (Cast(UnstableAffliction))
                    return;

            // Corruption
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Corruption")
                && _overLowManaThreshold
                && Target.HealthPercent > 20)
                if (Cast(Corruption))
                    return;

            // Immolate
            if (ObjectManager.Target.GetDistance < _maxRange
                && !Target.HaveBuff("Immolate")
                && !ObjectManager.Target.HaveBuff("Fire Ward")
                && _overLowManaThreshold
                && Target.HealthPercent > 30
                && (settings.UseImmolateHighLevel || !UnstableAffliction.KnownSpell)
                && ToolBox.CanBleed(ObjectManager.Target))
                if (Cast(Immolate))
                    return;

            // Drain Life high
            if (Me.HealthPercent < 70
                && Target.HealthPercent > 20)
                if (Cast(DrainLife))
                    return;

            // Health Funnel
            if (ObjectManager.Pet.IsValid
                && ObjectManager.Pet.HealthPercent < 30
                && Me.HealthPercent > 30)
            {
                RangeManager.SetRange(19f);
                if (HealthFunnel.IsDistanceGood && Cast(HealthFunnel))
                    return;
            }

            // Dark Pact
            if (Me.ManaPercentage < 70
                && ObjectManager.Pet.Mana > 0
                && ObjectManager.Pet.ManaPercentage > 60
                && settings.UseDarkPact)
                if (Cast(DarkPact))
                    return;

            // Drain Mana
            if (Me.ManaPercentage < 70
                && Target.Mana > 0
                && Target.ManaPercentage > 30)
                if (Cast(DrainMana))
                    return;

            // Incinerate
            if (ObjectManager.Target.GetDistance < _maxRange 
                && Target.HaveBuff("Immolate")
                && _overLowManaThreshold
                && Target.HealthPercent > 30
                && settings.UseIncinerate)
                if (Cast(Incinerate))
                    return;

            // Shadow Bolt
            if ((!settings.PrioritizeWandingOverSB || !_iCanUseWand)
                && (ObjectManager.Target.HealthPercent > 50 || Me.ManaPercentage > 90 && ObjectManager.Target.HealthPercent > 10)
                && _myManaPC > 40
                && ObjectManager.Target.GetDistance < _maxRange)
                if (Cast(ShadowBolt))
                    return;

            // Life Tap
            if (Me.HealthPercent > 50
                && Me.ManaPercentage < 40
                && !ObjectManager.Target.IsTargetingMe
                && settings.UseLifeTap)
                if (Cast(LifeTap))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand()
                && _iCanUseWand
                && ObjectManager.Target.GetDistance <= _maxRange + 2)
            {
                RangeManager.SetRange(_maxRange);
                if (Cast(UseWand, false))
                    return;
            }

            // Go in melee because nothing else to do
            if (!ToolBox.UsingWand() 
                && !UseWand.IsSpellUsable
                && !RangeManager.CurrentRangeIsMelee()
                && Target.IsAlive)
            {
                Logger.Log("Going in melee");
                RangeManager.SetRangeToMelee();
                return;
            }
        }

        protected Spell DemonSkin = new Spell("Demon Skin");
        protected Spell DemonArmor = new Spell("Demon Armor");
        protected Spell ShadowBolt = new Spell("Shadow Bolt");
        protected Spell UseWand = new Spell("Shoot");
        protected Spell Fear = new Spell("Fear");
        protected Spell Immolate = new Spell("Immolate");
        protected Spell Corruption = new Spell("Corruption");
        protected Spell LifeTap = new Spell("Life Tap");
        protected Spell SummonImp = new Spell("Summon Imp");
        protected Spell SummonVoidwalker = new Spell("Summon Voidwalker");
        protected Spell SummonFelguard = new Spell("Summon Felguard");
        protected Spell CurseOfAgony = new Spell("Curse of Agony");
        protected Spell DrainSoul = new Spell("Drain Soul");
        protected Spell DrainLife = new Spell("Drain Life");
        protected Spell CreateHealthStone = new Spell("Create HealthStone");
        protected Spell HealthFunnel = new Spell("Health Funnel");
        protected Spell CreateSoulstone = new Spell("Create Soulstone");
        protected Spell AmplifyCurse = new Spell("Amplify Curse");
        protected Spell UnendingBreath = new Spell("Unending Breath");
        protected Spell SiphonLife = new Spell("Siphon Life");
        protected Spell DrainMana = new Spell("Drain Mana");
        protected Spell DarkPact = new Spell("Dark Pact");
        protected Spell UnstableAffliction = new Spell("Unstable Affliction");
        protected Spell DeathCoil = new Spell("Death Coil");
        protected Spell FelArmor = new Spell("Fel Armor");
        protected Spell Incinerate = new Spell("Incinerate");
        protected Spell SoulShatter = new Spell("Soulshatter");
        protected Spell FelDomination = new Spell("Fel Domination");
        protected Spell SoulLink = new Spell("Soul Link");
        protected Spell HowlOfTerror = new Spell("Howl of Terror");

        protected bool Cast(Spell s, bool castEvenIfWanding = true)
        {
            return AdvancedCast(s, castEvenIfWanding);
        }

        protected bool CastOnSelf(Spell s, bool castEvenIfWanding = true, bool stopMove = false)
        {
            return AdvancedCast(s, castEvenIfWanding, stopMove, true);
        }

        protected bool CastStopMove(Spell s, bool castEvenIfWanding = true)
        {
            return AdvancedCast(s, castEvenIfWanding, true);
        }

        protected bool AdvancedCast(Spell s, bool castEvenIfWanding = true, bool stopmove = false, bool onSelf = false)
        {
            if (!s.KnownSpell)
                return false;

            CombatDebug("*----------- INTO CAST FOR " + s.Name);
            float _spellCD = ToolBox.GetSpellCooldown(s.Name);
            CombatDebug("Cooldown is " + _spellCD);

            if (ToolBox.GetSpellCost(s.Name) > Me.Mana)
            {
                CombatDebug(s.Name + ": Not enough mana, SKIPPING");
                return false;
            }

            if (ToolBox.UsingWand() && !castEvenIfWanding)
            {
                CombatDebug("Didn't cast because we were backing up or wanding");
                return false;
            }

            if (_spellCD >= 2f)
            {
                CombatDebug("Didn't cast because cd is too long");
                return false;
            }

            if (ToolBox.UsingWand() && castEvenIfWanding)
                ToolBox.StopWandWaitGCD(UseWand, ShadowBolt);

            if (_spellCD < 2f && _spellCD > 0f)
            {
                if (ToolBox.GetSpellCastTime(s.Name) < 1f)
                {
                    CombatDebug(s.Name + " is instant and low CD, recycle");
                    return true;
                }

                int t = 0;
                while (ToolBox.GetSpellCooldown(s.Name) > 0)
                {
                    Thread.Sleep(50);
                    t += 50;
                    if (t > 2000)
                    {
                        CombatDebug(s.Name + ": waited for tool long, give up");
                        return false;
                    }
                }
                Thread.Sleep(100 + Usefuls.Latency);
                CombatDebug(s.Name + ": waited " + (t + 100) + " for it to be ready");
            }

            if (!s.IsSpellUsable)
            {
                CombatDebug("Didn't cast because spell somehow not usable");
                return false;
            }

            CombatDebug("Launching");
            if (ObjectManager.Target.IsAlive || !Fight.InFight && ObjectManager.Target.Guid < 1)
            {
                s.Launch(stopmove, true, true);
            }
            return true;
        }

        protected void CombatDebug(string s)
        {
            if (settings.ActivateCombatDebug)
                Logger.CombatDebug(s);
        }

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _iCanUseWand = false;
            RangeManager.SetRange(_maxRange);
            _addCheckTimer.Reset();
            if (settings.PetInPassiveWhenOOC)
                Lua.LuaDoString("PetPassiveMode();");
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            if (UseWand.IsSpellUsable)
                _iCanUseWand = true;
            Lua.LuaDoString("PetDefensiveMode();");

            // Imp Firebolt
            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Imp"))
                ToolBox.TogglePetSpellAuto("Firebolt", true);

            // Imp BloodPact
            if (WarlockPetAndConsumables.MyWarlockPet().Equals("Imp"))
                ToolBox.TogglePetSpellAuto("Blood Pact", true);
        }
    }
}