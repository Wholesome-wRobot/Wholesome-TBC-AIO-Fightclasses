using System.Threading;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class Shadow : Priest
    {
        protected override void BuffRotation()
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

        protected override void Pull()
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

        protected override void CombatRotation()
        {
            bool _hasMagicDebuff = ToolBox.HasMagicDebuff();
            bool _hasDisease = ToolBox.HasDiseaseDebuff();
            bool _hasWeakenedSoul = ToolBox.HasDebuff("Weakened Soul");
            double _myManaPC = Me.ManaPercentage;
            bool _inShadowForm = Me.HaveBuff("ShadowForm");
            int _mindBlastCD = Lua.LuaDoString<int>("local start, duration, enabled = GetSpellCooldown(\"Mind Blast\"); return start + duration - GetTime();");
            int _innerFocusCD = Lua.LuaDoString<int>("local start, duration, enabled = GetSpellCooldown(\"Inner Focus\"); return start + duration - GetTime();");
            bool _shoulBeInterrupted = ToolBox.TargetIsCasting();
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
    }
}
