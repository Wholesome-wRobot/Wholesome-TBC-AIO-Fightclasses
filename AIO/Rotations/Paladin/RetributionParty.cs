﻿using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class RetributionParty : Paladin
    {
        public RetributionParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.DPS;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasDrinkAura || Me.ManaPercentage > 95)
            {
                base.BuffRotation();
            }
        }

        protected override void Pull()
        {
            base.Pull();

            ToolBox.CheckAutoAttack(Attack);

            // PARTY Lay On Hands
            List<IWoWPlayer> needsLoH = unitCache.GroupAndRaid
                .FindAll(m => m.HealthPercent < 10)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                return;

            // PARTY Purifiy
            if (settings.PRET_PartyPurify)
            {
                IWoWPlayer needsPurify = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name) || WTEffects.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }

            // PARTY Cleanse
            if (settings.PRET_PartyCleanse)
            {
                IWoWPlayer needsCleanse = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // Judgement
            if ((Me.HasAura(SealOfRighteousness) || Me.HasAura(SealOfCommand) || Me.HasAura(SealOfCommandRank1))
                && Target.GetDistance < Judgement.MaxRange
                && (Me.ManaPercentage >= manaSavePercent || Me.HasAura(SealOfTheCrusader))
                && cast.OnTarget(Judgement))
                return;

            // Seal of the Crusader
            if (!Target.HasAura("Judgement of the Crusader")
                && !Me.HasAura(SealOfTheCrusader)
                && Me.ManaPercentage > manaSavePercent - 20
                && settings.PRET_UseSealOfTheCrusader
                && cast.OnSelf(SealOfTheCrusader))
                return;

            // Seal of Righteousness
            if (!Me.HasAura(SealOfRighteousness)
                && !Me.HasAura(SealOfTheCrusader)
                && !settings.PRET_UseSealOfTheCrusader
                && (Target.HasAura("Judgement of the Crusader") || Me.ManaPercentage > manaSavePercent || !settings.PRET_UseSealOfTheCrusader)
                && (!settings.PRET_UseSealOfCommand || !SealOfCommand.KnownSpell)
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Command
            if (!Me.HasAura(SealOfCommand)
                && !Me.HasAura(SealOfCommandRank1)
                && !Me.HasAura(SealOfTheCrusader)
                && (Target.HasAura("Judgement of the Crusader") || Me.ManaPercentage > manaSavePercent || !settings.PRET_UseSealOfTheCrusader)
                && settings.PRET_UseSealOfCommand
                && SealOfCommand.KnownSpell
                && cast.OnSelf(SealOfCommand))
                return;

            // Seal of Command Rank 1
            if (!Me.HasAura(SealOfRighteousness)
                && !Me.HasAura(SealOfTheCrusader)
                && !Me.HasAura(SealOfCommand)
                && !Me.HasAura(SealOfCommandRank1)
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && Me.Mana < manaSavePercent
                && cast.OnSelf(SealOfCommandRank1))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            RangeManager.SetRangeToMelee();
            ToolBox.CheckAutoAttack(Attack);

            // PARTY Lay On Hands
            List<IWoWPlayer> needsLoH = unitCache.GroupAndRaid
                .FindAll(m => m.HealthPercent < 10)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                return;

            // PARTY Purifiy
            if (settings.PRET_PartyPurify)
            {
                IWoWPlayer needsPurify = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name) || WTEffects.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }

            // PARTY Cleanse
            if (settings.PRET_PartyCleanse)
            {
                IWoWPlayer needsCleanse = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // Hammer of Justice
            if (WTCombat.TargetIsCasting()
                && cast.OnTarget(HammerOfJustice))
                return;

            // Avenging Wrath
            if (cast.OnSelf(AvengingWrath))
                return;

            // Judgement (Crusader)
            if (Me.HasAura(SealOfTheCrusader)
                && Target.GetDistance < Judgement.MaxRange
                && cast.OnTarget(Judgement))
                return;

            // Judgement
            if (cast.OnTarget(Judgement))
                return;

            bool targetHasJOCrsudaer = Target.HasAura("Judgement of the Crusader");
            // Seal of the Crusader
            if (!targetHasJOCrsudaer
                && !Me.HasAura(SealOfTheCrusader)
                && cast.OnSelf(SealOfTheCrusader))
                return;

            // Seal of Blood
            if (targetHasJOCrsudaer
                && !Me.HasAura(SealOfBlood)
                && cast.OnSelf(SealOfBlood))
                return;

            if (!SealOfBlood.KnownSpell)
            {
                // Seal of Righteousness
                if (!Me.HasAura(SealOfRighteousness)
                    && !Me.HasAura(SealOfTheCrusader)
                    && (targetHasJOCrsudaer || !settings.PRET_UseSealOfTheCrusader)
                    && (!settings.PRET_UseSealOfCommand || !SealOfCommand.KnownSpell)
                    && cast.OnSelf(SealOfRighteousness))
                    return;

                // Seal of Command
                if (!Me.HasAura(SealOfCommand)
                    && !Me.HasAura(SealOfCommandRank1)
                    && !Me.HasAura(SealOfTheCrusader)
                    && (targetHasJOCrsudaer || !settings.PRET_UseSealOfTheCrusader)
                    && settings.PRET_UseSealOfCommand
                    && cast.OnSelf(SealOfCommand))
                    return;

                // Seal of Command Rank 1
                if (!Me.HasAura(SealOfRighteousness)
                    && !Me.HasAura(SealOfTheCrusader)
                    && !Me.HasAura(SealOfCommand)
                    && !Me.HasAura(SealOfCommandRank1)
                    && !SealOfCommand.IsSpellUsable
                    && !SealOfRighteousness.IsSpellUsable
                    && cast.OnSelf(SealOfCommandRank1))
                    return;
            }

            // Consecration
            if (Me.ManaPercentage > settings.PRET_PartyRetConsecrationThreshold
                && ToolBox.GetNbEnemiesClose(10f) > 1
                && cast.OnSelf(Consecration))
                return;

            // Hammer of Wrath
            if (settings.PRET_UseHammerOfWrath
                && cast.OnTarget(HammerOfWrath))
                return;

            // Exorcism
            if (Me.ManaPercentage > settings.PRET_PartyRetExorcismThreshold
                && (Target.CreatureTypeTarget == "Undead" || Target.CreatureTypeTarget == "Demon")
                && cast.OnTarget(Exorcism))
                return;

            // Crusader Strike
            if (cast.OnTarget(CrusaderStrike))
                return;
        }
    }
}
