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

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class Priest : IClassRotation
    {
        public static PriestSettings settings;

        protected Cast cast;

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected Stopwatch _dispelTimer = new Stopwatch();

        protected readonly float _distanceRange = 26f;
        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected int _innerManaSaveThreshold = 20;
        protected int _wandThreshold;
        protected bool _goInMFRange = false;

        protected Priest specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = PriestSettings.Current;
            cast = new Cast(Smite, settings.ActivateCombatDebug, UseWand);

            this.specialization = specialization as Priest;
            Talents.InitTalents(settings);

            _wandThreshold = settings.WandThreshold > 100 ? 50 : settings.WandThreshold;
            RangeManager.SetRange(_distanceRange);

            FightEvents.OnFightEnd += FightEndHandler;
            FightEvents.OnFightStart += FightStartHandler;

            Rotation();
        }


        public void Dispose()
        {
            FightEvents.OnFightEnd -= FightEndHandler;
            FightEvents.OnFightStart -= FightStartHandler;
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        if (!RangeManager.CurrentRangeIsMelee())
                        {
                            if (_goInMFRange)
                                RangeManager.SetRange(17f);
                            else
                                RangeManager.SetRange(_distanceRange);
                        }
                    }

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
            if (!Me.IsMounted)
            {
                // OOC Cure Disease
                if (ToolBox.HasDiseaseDebuff())
                    if (cast.OnSelf(CureDisease))
                        return;

                // OOC Renew
                if (Me.HealthPercent < 70 
                    && !Me.HaveBuff("Renew"))
                    if (cast.OnSelf(Renew))
                        return;

                // OOC Power Word Shield
                if (Me.HealthPercent < 50 
                    && !Me.HaveBuff("Power Word: Shield") 
                    && !ToolBox.HasDebuff("Weakened Soul")
                    && ObjectManager.GetNumberAttackPlayer() > 0 
                    && settings.UsePowerWordShield)
                    if (cast.OnSelf(PowerWordShield))
                        return;

                // OOC Psychic Scream
                if (Me.HealthPercent < 30 
                    && ObjectManager.GetNumberAttackPlayer() > 1)
                    if (cast.Normal(PsychicScream))
                        return;

                // OOC Power Word Fortitude
                if (!Me.HaveBuff("Power Word: Fortitude") 
                    && PowerWordFortitude.KnownSpell 
                    && PowerWordFortitude.IsSpellUsable)
                {
                    if (cast.OnSelf(PowerWordFortitude))
                        return;
                }

                // OOC Divine Spirit
                if (!Me.HaveBuff("Divine Spirit") 
                    && DivineSpirit.KnownSpell 
                    && DivineSpirit.IsSpellUsable)
                {
                    if (cast.OnSelf(DivineSpirit))
                        return;
                }

                // OOC Inner Fire
                if (!Me.HaveBuff("Inner Fire") 
                    && settings.UseInnerFire)
                    if (cast.Normal(InnerFire))
                        return;

                // OOC Shadowguard
                if (!Me.HaveBuff("Shadowguard") 
                    && settings.UseShadowGuard 
                    && Shadowguard.KnownSpell 
                    && Shadowguard.IsSpellUsable)
                {
                    if (cast.OnSelf(Shadowguard))
                        return;
                }

                // OOC Shadow Protection
                if (!Me.HaveBuff("Shadow Protection") 
                    && ShadowProtection.KnownSpell 
                    && settings.UseShadowProtection
                    && ShadowProtection.KnownSpell 
                    && ShadowProtection.IsSpellUsable)
                {
                    if (cast.OnSelf(ShadowProtection))
                        return;
                }

                // OOC ShadowForm
                if (!Me.HaveBuff("ShadowForm") 
                    && ObjectManager.GetNumberAttackPlayer() < 1 
                    && Shadowform.IsSpellUsable)
                    if (cast.Normal(Shadowform))
                        return;
            }
        }

        protected virtual void Pull()
        {
            // Pull ShadowForm
            if (!Me.HaveBuff("ShadowForm"))
                if (cast.Normal(Shadowform))
                    return;

            // Power Word Shield
            if (!ToolBox.HasDebuff("Weakened Soul") 
                && settings.UseShieldOnPull
                && !Me.HaveBuff("Power Word: Shield")
                && settings.UsePowerWordShield)
                if (cast.OnSelf(PowerWordShield))
                    return;

            // Vampiric Touch
            if (Me.HaveBuff("ShadowForm") 
                && ObjectManager.Target.GetDistance <= _distanceRange
                && !ObjectManager.Target.HaveBuff("Vampiric Touch"))
                if (cast.Normal(VampiricTouch))
                    return;

            // MindBlast
            if (Me.HaveBuff("ShadowForm") 
                && ObjectManager.Target.GetDistance <= _distanceRange
                && !VampiricTouch.KnownSpell)
                if (cast.Normal(MindBlast))
                    return;

            // Shadow Word Pain
            if (Me.HaveBuff("ShadowForm") 
                && ObjectManager.Target.GetDistance <= _distanceRange
                && (!MindBlast.KnownSpell || !MindBlast.IsSpellUsable)
                && !ObjectManager.Target.HaveBuff("Shadow Word: Pain"))
                if (cast.Normal(ShadowWordPain))
                    return;

            // Holy Fire
            if (ObjectManager.Target.GetDistance <= _distanceRange 
                && HolyFire.KnownSpell
                && HolyFire.IsSpellUsable 
                && !Me.HaveBuff("ShadowForm"))
                if (cast.Normal(HolyFire))
                    return;

            // Smite
            if (ObjectManager.Target.GetDistance <= _distanceRange 
                && Smite.KnownSpell
                && !HolyFire.KnownSpell 
                && Smite.IsSpellUsable 
                && !Me.HaveBuff("ShadowForm"))
                if (cast.Normal(Smite))
                    return;
        }

        protected virtual void CombatRotation()
        {
            bool _hasMagicDebuff = ToolBox.HasMagicDebuff();
            bool _hasDisease = ToolBox.HasDiseaseDebuff();
            bool _hasWeakenedSoul = ToolBox.HasDebuff("Weakened Soul");
            double _myManaPC = Me.ManaPercentage;
            bool _inShadowForm = Me.HaveBuff("ShadowForm");
            int _mindBlastCD = Lua.LuaDoString<int>("local start, duration, enabled = GetSpellCooldown(\"Mind Blast\"); return start + duration - GetTime();");
            int _innerFocusCD = Lua.LuaDoString<int>("local start, duration, enabled = GetSpellCooldown(\"Inner Focus\"); return start + duration - GetTime();");
            bool _shoulBeInterrupted = ToolBox.EnemyCasting();
            WoWUnit Target = ObjectManager.Target;

            // Power Word Shield on multi aggro
            if (!Me.HaveBuff("Power Word: Shield") 
                && !_hasWeakenedSoul
                && ObjectManager.GetNumberAttackPlayer() > 1 
                && settings.UsePowerWordShield)
                if (cast.OnSelf(PowerWordShield))
                    return;

            // Power Word Shield
            if (Me.HealthPercent < 50 
                && !Me.HaveBuff("Power Word: Shield")
                && !_hasWeakenedSoul 
                && settings.UsePowerWordShield)
                if (cast.OnSelf(PowerWordShield))
                    return;

            // Renew
            if (Me.HealthPercent < 70 
                && !Me.HaveBuff("Renew") 
                && !_inShadowForm
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.OnSelf(Renew))
                    return;

            // Psychic Scream
            if (Me.HealthPercent < 50 
                && ObjectManager.GetNumberAttackPlayer() > 1)
                if (cast.Normal(PsychicScream))
                    return;

            // Flash Heal
            if (Me.HealthPercent < 50 
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.OnSelf(FlashHeal))
                    return;

            // Heal
            if (Me.HealthPercent < 50 
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.OnSelf(Heal))
                    return;

            // Lesser Heal
            if (Me.HealthPercent < 50 
                && !FlashHeal.KnownSpell 
                && (Target.HealthPercent > 15 || Me.HealthPercent < 25))
                if (cast.OnSelf(LesserHeal))
                    return;

            // Silence
            if (_shoulBeInterrupted)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.Normal(Silence))
                    return;
            }

            // Cure Disease
            if (_hasDisease && !_inShadowForm)
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(CureDisease))
                    return;
            }

            // Dispel Magic self
            if (_hasMagicDebuff 
                && _myManaPC > 10 
                && DispelMagic.KnownSpell 
                && DispelMagic.IsSpellUsable
                && (_dispelTimer.ElapsedMilliseconds > 10000 || _dispelTimer.ElapsedMilliseconds <= 0))
            {
                Thread.Sleep(Main.humanReflexTime);
                if (cast.OnSelf(DispelMagic))
                    return;
            }

            // Vampiric Touch
            if (Target.GetDistance <= _distanceRange 
                && !Target.HaveBuff("Vampiric Touch")
                && _myManaPC > _innerManaSaveThreshold 
                && Target.HealthPercent > _wandThreshold)
                if (cast.Normal(VampiricTouch))
                    return;

            // Vampiric Embrace
            if (!Target.HaveBuff("Vampiric Embrace") 
                && _myManaPC > _innerManaSaveThreshold)
                if (cast.Normal(VampiricEmbrace))
                    return;

            // ShadowFiend
            if (ObjectManager.GetNumberAttackPlayer() > 1)
                if (cast.Normal(Shadowfiend))
                    return;

            // Shadow Word Pain
            if (_myManaPC > 10 
                && Target.GetDistance < _distanceRange 
                && Target.HealthPercent > 15
                && !Target.HaveBuff("Shadow Word: Pain"))
                if (cast.Normal(ShadowWordPain))
                    return;

            // Inner Fire
            if (!Me.HaveBuff("Inner Fire") 
                && settings.UseInnerFire 
                && InnerFire.KnownSpell
                && _myManaPC > _innerManaSaveThreshold 
                && Target.HealthPercent > _wandThreshold)
                if (cast.Normal(InnerFire))
                    return;

            // Shadowguard
            if (!Me.HaveBuff("Shadowguard") 
                && _myManaPC > _innerManaSaveThreshold
                && settings.UseShadowGuard 
                && Target.HealthPercent > _wandThreshold)
                if (cast.OnSelf(Shadowguard))
                    return;

            // Shadow Protection
            if (!Me.HaveBuff("Shadow Protection") 
                && _myManaPC > 70 
                && settings.UseShadowProtection)
                if (cast.OnSelf(ShadowProtection))
                    return;

            // Devouring Plague
            if (!Target.HaveBuff("Devouring Plague") 
                && Target.HealthPercent > 80)
                if (cast.Normal(DevouringPlague))
                    return;

            // Shadow Word Death
            if (_myManaPC > _innerManaSaveThreshold 
                && Target.GetDistance < _distanceRange
                && settings.UseShadowWordDeath 
                && Target.HealthPercent < 15)
                if (cast.Normal(ShadowWordDeath))
                    return;

            // Mind Blast + Inner Focus
            if (!_inShadowForm 
                && _myManaPC > _innerManaSaveThreshold 
                && Target.GetDistance < _distanceRange
                && Target.HealthPercent > 50 
                && _mindBlastCD <= 0
                && (Target.HealthPercent > _wandThreshold || !_iCanUseWand))
            {
                if (InnerFocus.KnownSpell && _innerFocusCD <= 0)
                    cast.Normal(InnerFocus);

                if (cast.Normal(MindBlast))
                    return;
            }

            // Shadow Form Mind Blast + Inner Focus
            if (_inShadowForm 
                && _myManaPC > _innerManaSaveThreshold 
                && Target.GetDistance < _distanceRange
                && _mindBlastCD <= 0 
                && Target.HealthPercent > _wandThreshold)
            {
                if (InnerFocus.KnownSpell && _innerFocusCD <= 0)
                    cast.Normal(InnerFocus);

                if (cast.Normal(MindBlast))
                    return;
            }

            // Mind Flay Range check
            if (_inShadowForm 
                && !MindFlay.IsDistanceGood 
                && (Me.HaveBuff("Power Word: Shield") || !settings.UsePowerWordShield))
            {
                Logger.LogDebug("Approaching to be in Mind Flay range");
                _goInMFRange = true;
                return;
            }

            // Mind FLay
            if ((Me.HaveBuff("Power Word: Shield") || !settings.UsePowerWordShield) 
                && MindFlay.IsDistanceGood
                && _myManaPC > _innerManaSaveThreshold 
                && Target.HealthPercent > _wandThreshold)
                if (cast.Normal(MindFlay))
                    return;

            // Low level Smite
            if (Me.Level < 5 && (Target.HealthPercent > 30 || Me.ManaPercentage > 80) 
                && _myManaPC > _innerManaSaveThreshold
                && Target.GetDistance < _distanceRange)
                if (cast.Normal(Smite))
                    return;

            // Smite
            if (!_inShadowForm 
                && _myManaPC > _innerManaSaveThreshold 
                && Target.GetDistance < _distanceRange
                && Me.Level >= 5 
                && Target.HealthPercent > 20 
                && (Target.HealthPercent > settings.WandThreshold || !_iCanUseWand))
                if (cast.Normal(Smite))
                    return;

            // Stop wand if banned
            if (ToolBox.UsingWand()
                && cast.BannedSpells.Contains("Shoot"))
                if (cast.Normal(UseWand))
                    return;

            // Spell if wand banned
            if (cast.BannedSpells.Contains("Shoot")
                && Target.GetDistance < _distanceRange)
                if (cast.Normal(MindBlast) || cast.Normal(Smite))
                    return;

            // Use Wand
            if (!ToolBox.UsingWand() 
                && _iCanUseWand 
                && Target.GetDistance <= _distanceRange + 2)
            {
                RangeManager.SetRange(_distanceRange);
                if (cast.Normal(UseWand, false))
                    return;
            }

            // Go in melee because nothing else to do
            if (!ToolBox.UsingWand()
                && !_iCanUseWand
                && !RangeManager.CurrentRangeIsMelee()
                && Target.IsAlive)
            {
                Logger.Log("Going in melee");
                RangeManager.SetRangeToMelee();
                return;
            }
        }

        protected Spell Smite = new Spell("Smite");
        protected Spell LesserHeal = new Spell("Lesser Heal");
        protected Spell PowerWordFortitude = new Spell("Power Word: Fortitude");
        protected Spell PowerWordShield = new Spell("Power Word: Shield");
        protected Spell ShadowWordPain = new Spell("Shadow Word: Pain");
        protected Spell ShadowWordDeath = new Spell("Shadow Word: Death");
        protected Spell UseWand = new Spell("Shoot");
        protected Spell Renew = new Spell("Renew");
        protected Spell MindBlast = new Spell("Mind Blast");
        protected Spell InnerFire = new Spell("Inner Fire");
        protected Spell CureDisease = new Spell("Cure Disease");
        protected Spell PsychicScream = new Spell("Psychic Scream");
        protected Spell Heal = new Spell("Heal");
        protected Spell MindFlay = new Spell("Mind Flay");
        protected Spell HolyFire = new Spell("Holy Fire");
        protected Spell DispelMagic = new Spell("Dispel Magic");
        protected Spell FlashHeal = new Spell("Flash Heal");
        protected Spell VampiricEmbrace = new Spell("Vampiric Embrace");
        protected Spell Shadowguard = new Spell("Shadowguard");
        protected Spell ShadowProtection = new Spell("Shadow Protection");
        protected Spell Shadowform = new Spell("Shadowform");
        protected Spell VampiricTouch = new Spell("Vampiric Touch");
        protected Spell InnerFocus = new Spell("Inner Focus");
        protected Spell Shadowfiend = new Spell("Shadowfiend");
        protected Spell Silence = new Spell("Silence");
        protected Spell DivineSpirit = new Spell("Divine Spirit");
        protected Spell DevouringPlague = new Spell("Devouring Plague");

        // EVENT HANDLERS
        private void FightEndHandler(ulong guid)
        {
            _goInMFRange = false;
            _dispelTimer.Reset();
            _iCanUseWand = false;
            RangeManager.SetRange(_distanceRange);
        }

        private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
        {
            _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        }
    }
}