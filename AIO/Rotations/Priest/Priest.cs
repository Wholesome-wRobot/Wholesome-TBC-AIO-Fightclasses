using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using robotManager.Helpful;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Settings;
using wManager.Events;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Priest
{
    public class Priest : IClassRotation
    {
        public Enums.RotationType RotationType { get; set; }
        public Enums.RotationRole RotationRole { get; set; }

        public static PriestSettings settings;

        protected Cast cast;

        protected WoWLocalPlayer Me = ObjectManager.Me;
        protected Stopwatch _dispelTimer = new Stopwatch();

        protected readonly float _distanceRange = 26f;
        protected bool _iCanUseWand = ToolBox.HaveRangedWeaponEquipped();
        protected int _innerManaSaveThreshold = 20;
        protected int _wandThreshold;
        protected bool _goInMFRange = false;
        protected List<WoWUnit> _partyEnemiesAround = new List<WoWUnit>();

        protected Priest specialization;

        public void Initialize(IClassRotation specialization)
        {
            settings = PriestSettings.Current;
            cast = new Cast(Smite, settings.ActivateCombatDebug, UseWand, settings.AutoDetectImmunities);

            this.specialization = specialization as Priest;
            (RotationType, RotationRole) = ToolBox.GetRotationType(specialization);
            TalentsManager.InitTalents(settings);

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
            cast.Dispose();
            Logger.Log("Disposed");
        }

        private void Rotation()
        {
            while (Main.isLaunched)
            {
                try
                {
                    if (StatusChecker.BasicConditions() && specialization is Shadow)
                    {
                        if (!RangeManager.CurrentRangeIsMelee())
                        {
                            if (_goInMFRange)
                                RangeManager.SetRange(17f);
                            else
                                RangeManager.SetRange(_distanceRange);
                        }
                    }

                    if (RotationType == Enums.RotationType.Party)
                        _partyEnemiesAround = ToolBox.GetSuroundingEnemies();

                    if (StatusChecker.OutOfCombat())
                        specialization.BuffRotation();

                    if (StatusChecker.InPull())
                        specialization.Pull();

                    if (StatusChecker.InCombat())
                        specialization.CombatRotation();

                    if (AIOParty.Group.Any(p => p.InCombatFlagOnly && p.GetDistance < 50) || ObjectManager.Me.HaveBuff("Spirit of Redemption"))
                        specialization.HealerCombat();

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
        }

        protected virtual void Pull()
        {
        }

        protected virtual void CombatRotation()
        {
        }

        protected virtual void HealerCombat()
        {
        }

        protected AIOSpell Smite = new AIOSpell("Smite");
        protected AIOSpell LesserHeal = new AIOSpell("Lesser Heal");
        protected AIOSpell PowerWordFortitude = new AIOSpell("Power Word: Fortitude");
        protected AIOSpell PowerWordShield = new AIOSpell("Power Word: Shield");
        protected AIOSpell ShadowWordPain = new AIOSpell("Shadow Word: Pain");
        protected AIOSpell ShadowWordDeath = new AIOSpell("Shadow Word: Death");
        protected AIOSpell UseWand = new AIOSpell("Shoot");
        protected AIOSpell Renew = new AIOSpell("Renew");
        protected AIOSpell MindBlast = new AIOSpell("Mind Blast");
        protected AIOSpell InnerFire = new AIOSpell("Inner Fire");
        protected AIOSpell CureDisease = new AIOSpell("Cure Disease");
        protected AIOSpell PsychicScream = new AIOSpell("Psychic Scream");
        protected AIOSpell Heal = new AIOSpell("Heal");
        protected AIOSpell GreaterHeal = new AIOSpell("Greater Heal");
        protected AIOSpell MindFlay = new AIOSpell("Mind Flay");
        protected AIOSpell HolyFire = new AIOSpell("Holy Fire");
        protected AIOSpell DispelMagic = new AIOSpell("Dispel Magic");
        protected AIOSpell FlashHeal = new AIOSpell("Flash Heal");
        protected AIOSpell VampiricEmbrace = new AIOSpell("Vampiric Embrace");
        protected AIOSpell Shadowguard = new AIOSpell("Shadowguard");
        protected AIOSpell ShadowProtection = new AIOSpell("Shadow Protection");
        protected AIOSpell Shadowform = new AIOSpell("Shadowform");
        protected AIOSpell VampiricTouch = new AIOSpell("Vampiric Touch");
        protected AIOSpell InnerFocus = new AIOSpell("Inner Focus");
        protected AIOSpell Shadowfiend = new AIOSpell("Shadowfiend");
        protected AIOSpell Silence = new AIOSpell("Silence");
        protected AIOSpell DivineSpirit = new AIOSpell("Divine Spirit");
        protected AIOSpell DevouringPlague = new AIOSpell("Devouring Plague");
        protected AIOSpell Resurrection = new AIOSpell("Resurrection");
        protected AIOSpell PrayerOfHealing = new AIOSpell("Prayer of Healing");
        protected AIOSpell PrayerOfMending = new AIOSpell("Prayer of Mending");

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