using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    internal class Racials
    {
        public bool _isRunning;
        private WoWLocalPlayer Me = ObjectManager.Me;

        private Spell Cannibalize = new Spell("Cannibalize");
        private Spell WillOfTheForsaken = new Spell("Will of the Forsaken");
        private Spell Berserking = new Spell("Berserking");
        private Spell EscapeArtist = new Spell("Escape Artist");
        private Spell ManaTap = new Spell("Mana Tap");
        private Spell ArcaneTorrent = new Spell("Arcane Torrent");
        private Spell Stoneform = new Spell("Stoneform");
        private Spell GiftOfTheNaaru = new Spell("Gift of the Naaru");
        private Spell WarStomp = new Spell("War Stomp");
        private Spell BloodFury = new Spell("Blood Fury");

        public void DoRacialsPulse(object sender, DoWorkEventArgs args)
        {
            _isRunning = true;
            while (Main.isLaunched && _isRunning)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        if (StatusChecker.OutOfCombat())
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
            _isRunning = false;
        }

        private void RacialBloodFury()
        {
            if (BloodFury.KnownSpell
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
                && ObjectManager.GetNumberAttackPlayer() > 1
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
                && (ToolBox.HasPoisonDebuff() || ToolBox.HasDiseaseDebuff() || Me.HaveBuff("Bleed")))
            {
                Stoneform.Launch();
                Usefuls.WaitIsCasting();
            }
        }

        private void RacialGiftOfTheNaaru()
        {
            if (GiftOfTheNaaru.KnownSpell
                && GiftOfTheNaaru.IsSpellUsable
                && ObjectManager.GetNumberAttackPlayer() > 1 && Me.HealthPercent < 50)
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
