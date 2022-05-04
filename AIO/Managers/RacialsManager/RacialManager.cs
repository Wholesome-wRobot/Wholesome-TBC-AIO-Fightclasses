using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.PartyManager;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeToolbox;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeTBCAIO.Helpers.Enums;

namespace WholesomeTBCAIO.Managers.RacialsManager
{
    internal class RacialManager : IRacialsManager
    {
        private AIOSpell Cannibalize = new AIOSpell("Cannibalize");
        private AIOSpell WillOfTheForsaken = new AIOSpell("Will of the Forsaken");
        private AIOSpell Berserking = new AIOSpell("Berserking");
        private AIOSpell EscapeArtist = new AIOSpell("Escape Artist");
        private AIOSpell ManaTap = new AIOSpell("Mana Tap");
        private AIOSpell ArcaneTorrent = new AIOSpell("Arcane Torrent");
        private AIOSpell Stoneform = new AIOSpell("Stoneform");
        private AIOSpell GiftOfTheNaaru = new AIOSpell("Gift of the Naaru");
        private AIOSpell WarStomp = new AIOSpell("War Stomp");
        private AIOSpell BloodFury = new AIOSpell("Blood Fury");
        private WoWLocalPlayer Me = ObjectManager.Me;
        private readonly BackgroundWorker _racialsThread = new BackgroundWorker();
        private readonly IPartyManager _partyManager;
        private readonly IUnitCache _unitCache;
        private bool _isRunning;

        public RacialManager(IPartyManager partyManager, IUnitCache unitCache)
        {
            _partyManager = partyManager;
            _unitCache = unitCache;
        }

        public void Initialize()
        {
            _isRunning = true;
            _racialsThread.DoWork += Pulse;
            _racialsThread.RunWorkerAsync();
        }

        public void Dispose()
        {
            _racialsThread.DoWork -= Pulse;
            _racialsThread.Dispose();
            _isRunning = false;
        }

        public void Pulse(object sender, DoWorkEventArgs args)
        {
            while (_isRunning)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        if (StatusChecker.OutOfCombat(RotationRole.None))
                            RacialCannibalize();

                        if (StatusChecker.InCombat())
                        {
                            RacialManaTap();
                            RacialWillOfTheForsaken();
                            RacialEscapeArtist();
                            RacialBerserking();
                            RacialArcaneTorrent();
                            RacialStoneForm();
                            RacialGiftOfTheNaaru();
                            RacialWarStomp();
                            RacialBloodFury();
                        }
                    }
                }
                catch (Exception arg)
                {
                    Logger.LogError(string.Concat(arg));
                }
                Thread.Sleep(500);
            }
        }

        private void RacialBloodFury()
        {
            if (BloodFury.KnownSpell
                && _unitCache.GroupAndRaid.Count <= 1
                && BloodFury.IsSpellUsable
                && ObjectManager.Target.HealthPercent > 70)
            {
                BloodFury.Launch();
            }
        }

        private void RacialWarStomp()
        {
            if (WarStomp.KnownSpell
                && WarStomp.IsSpellUsable
                && !Me.HaveBuff("Bear Form")
                && !Me.HaveBuff("Cat Form")
                && !Me.HaveBuff("Dire Bear Form")
                && _unitCache.EnemiesAttackingMe.Count > 1
                && ObjectManager.Target.GetDistance < 8)
            {
                WarStomp.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialStoneForm()
        {
            if (Stoneform.KnownSpell
                && Stoneform.IsSpellUsable
                && (WTEffects.HasPoisonDebuff() || WTEffects.HasDiseaseDebuff() || Me.HaveBuff("Bleed")))
            {
                Stoneform.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialGiftOfTheNaaru()
        {
            if (GiftOfTheNaaru.KnownSpell
                && GiftOfTheNaaru.IsSpellUsable
                && _unitCache.EnemiesAttackingMe.Count > 1 && Me.HealthPercent < 50)
            {
                GiftOfTheNaaru.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialArcaneTorrent()
        {
            if (ArcaneTorrent.KnownSpell
                && ArcaneTorrent.IsSpellUsable
                && Me.HaveBuff("Mana Tap")
                && (Me.ManaPercentage < 50 || (ObjectManager.Target.IsCast && ObjectManager.Target.GetDistance < 8)))
            {
                ArcaneTorrent.Launch();
            }
        }

        private void RacialBerserking()
        {
            if (Berserking.KnownSpell
                && Berserking.IsSpellUsable
                && ObjectManager.Target.HealthPercent > 70)
            {
                Berserking.Launch();
            }
        }

        private void RacialEscapeArtist()
        {
            if (EscapeArtist.KnownSpell
                && EscapeArtist.IsSpellUsable
                && Me.Rooted || Me.HaveBuff("Frostnova"))
            {
                EscapeArtist.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialWillOfTheForsaken()
        {
            if (WillOfTheForsaken.KnownSpell
                && WillOfTheForsaken.IsSpellUsable
                && Me.HaveBuff("Fear") || Me.HaveBuff("Charm") || Me.HaveBuff("Sleep"))
            {
                WillOfTheForsaken.Launch();
            }
        }

        private void RacialManaTap()
        {
            if (ManaTap.IsDistanceGood
                && ManaTap.IsSpellUsable
                && ManaTap.KnownSpell
                && ObjectManager.Target.Mana > 0
                && ObjectManager.Target.ManaPercentage > 10)
            {
                ManaTap.Launch();
            }
        }

        private void RacialCannibalize()
        {
            // Cannibalize
            if (Cannibalize.KnownSpell
                && Cannibalize.IsSpellUsable
                && Me.HealthPercent < 50
                && !Me.HaveBuff("Drink")
                && !Me.HaveBuff("Food")
                && Me.IsAlive
                && ObjectManager.GetObjectWoWUnit().Where(u => u.GetDistance <= 8 && u.IsDead && (u.CreatureTypeTarget == "Humanoid" || u.CreatureTypeTarget == "Undead")).Count() > 0)
            {
                Cannibalize.Launch();
                Usefuls.WaitIsCasting();
            }
        }
    }
}
