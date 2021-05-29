using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class RetributionParty : Paladin
    {
        protected override void BuffRotation()
        {
            base.BuffRotation();
        }

        protected override void PullRotation()
        {
            base.PullRotation();

            WoWUnit Target = ObjectManager.Target;

            ToolBox.CheckAutoAttack(Attack);

            // PARTY Lay On Hands
            List<AIOPartyMember> needsLoH = AIOParty.Group
                .FindAll(m => m.HealthPercent < 10)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                return;

            // PARTY Purifiy
            if (settings.PartyPurify)
            {
                WoWPlayer needsPurify = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name) || ToolBox.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }

            // PARTY Cleanse
            if (settings.PartyCleanse)
            {
                WoWPlayer needsCleanse = AIOParty.Group
                    .Find(m => ToolBox.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // Judgement
            if ((Me.HaveBuff("Seal of Righteousness") || Me.HaveBuff("Seal of Command"))
                && Target.GetDistance < Judgement.MaxRange
                && (Me.ManaPercentage >= _manaSavePercent || Me.HaveBuff("Seal of the Crusader"))
                && cast.OnTarget(Judgement))
                return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader")
                && !Me.HaveBuff("Seal of the Crusader")
                && Me.ManaPercentage > _manaSavePercent - 20
                && settings.UseSealOfTheCrusader
                && cast.OnSelf(SealOfTheCrusader))
                return;

            // Seal of Righteousness
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !settings.UseSealOfTheCrusader
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell)
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Command
            if (!Me.HaveBuff("Seal of Command")
                && !Me.HaveBuff("Seal of the Crusader")
                && (Target.HaveBuff("Judgement of the Crusader") || Me.ManaPercentage > _manaSavePercent || !settings.UseSealOfTheCrusader)
                && settings.UseSealOfCommand
                && SealOfCommand.KnownSpell
                && cast.OnSelf(SealOfCommand))
                return;

            // Seal of Command Rank 1
            if (!Me.HaveBuff("Seal of Righteousness")
                && !Me.HaveBuff("Seal of the Crusader")
                && !Me.HaveBuff("Seal of Command")
                && !SealOfCommand.IsSpellUsable
                && !SealOfRighteousness.IsSpellUsable
                && Me.Mana < _manaSavePercent
                && cast.OnSelf(SealOfCommandRank1))
                return;
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;

            ToolBox.CheckAutoAttack(Attack);

            // PARTY Lay On Hands
            List<AIOPartyMember> needsLoH = AIOParty.Group
                .FindAll(m => m.HealthPercent < 10)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                return;

            // PARTY Purifiy
            if (settings.PartyPurify)
            {
                WoWPlayer needsPurify = AIOParty.Group
                    .Find(m => ToolBox.HasDiseaseDebuff(m.Name) || ToolBox.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }

            // PARTY Cleanse
            if (settings.PartyCleanse)
            {
                WoWPlayer needsCleanse = AIOParty.Group
                    .Find(m => ToolBox.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            // Hammer of Justice
            if (ToolBox.TargetIsCasting()
                && cast.OnTarget(HammerOfJustice))
                return;

            // Avenging Wrath
            if (cast.OnSelf(AvengingWrath))
                return;

            // Judgement (Crusader)
            if (Me.HaveBuff("Seal of the Crusader")
                && Target.GetDistance < Judgement.MaxRange
                && cast.OnTarget(Judgement))
                return;

            // Judgement
            if (cast.OnTarget(Judgement))
                return;

            // Seal of the Crusader
            if (!Target.HaveBuff("Judgement of the Crusader")
                && !Me.HaveBuff("Seal of the Crusader")
                && cast.OnSelf(SealOfTheCrusader))
                return;

            // Seal of Blood
            if (Target.HaveBuff("Judgement of the Crusader")
                && !Me.HaveBuff("Seal of Blood")
                && cast.OnSelf(SealOfBlood))
                return;

            if (!SealOfBlood.KnownSpell)
            {
                // Seal of Righteousness
                if (!Me.HaveBuff("Seal of Righteousness")
                    && !Me.HaveBuff("Seal of the Crusader")
                    && (Target.HaveBuff("Judgement of the Crusader") || !settings.UseSealOfTheCrusader)
                    && (!settings.UseSealOfCommand || !SealOfCommand.KnownSpell)
                    && cast.OnSelf(SealOfRighteousness))
                    return;

                // Seal of Command
                if (!Me.HaveBuff("Seal of Command")
                    && !Me.HaveBuff("Seal of the Crusader")
                    && (Target.HaveBuff("Judgement of the Crusader") || !settings.UseSealOfTheCrusader)
                    && settings.UseSealOfCommand
                    && cast.OnSelf(SealOfCommand))
                    return;

                // Seal of Command Rank 1
                if (!Me.HaveBuff("Seal of Righteousness")
                    && !Me.HaveBuff("Seal of the Crusader")
                    && !Me.HaveBuff("Seal of Command")
                    && !SealOfCommand.IsSpellUsable
                    && !SealOfRighteousness.IsSpellUsable
                    && cast.OnSelf(SealOfCommandRank1))
                    return;
            }

            // Consecration
            if (Me.ManaPercentage > settings.PartyRetConsecrationThreshold
                && ToolBox.GetNbEnemiesClose(10f) > 1
                && cast.OnSelf(Consecration))
                return;

            // Hammer of Wrath
            if (settings.UseHammerOfWrath
                && cast.OnTarget(HammerOfWrath))
                return;

            // Exorcism
            if (Me.ManaPercentage > settings.PartyRetExorcismThreshold
                && (Target.CreatureTypeTarget == "Undead" || Target.CreatureTypeTarget == "Demon")
                && cast.OnTarget(Exorcism))
                return;

            // Crusader Strike
            if (cast.OnTarget(CrusaderStrike))
                return;
        }
    }
}
