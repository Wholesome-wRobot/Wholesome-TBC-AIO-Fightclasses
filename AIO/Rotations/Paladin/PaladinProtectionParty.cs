using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class PaladinProtectionParty : Paladin
    {
        protected override void BuffRotation()
        {
            // Righteous Fury
            if (!Me.HaveBuff("Righteous Fury")
                && cast.OnSelf(RighteousFury))
                return;

            base.BuffRotation();
        }

        protected override void PullRotation()
        {
            base.PullRotation();

            WoWUnit Target = ObjectManager.Target;

            ToolBox.CheckAutoAttack(Attack);

            // Seal of Righteousness
            if (Me.ManaPercentage > settings.PartyProtSealOfWisdom
                && !Me.HaveBuff("Seal of Righteousness")
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Wisdom
            if (Me.ManaPercentage <= settings.PartyProtSealOfWisdom
                && !Me.HaveBuff("Seal of Wisdom")
                && cast.OnSelf(SealOfWisdom))
                return;

            AIOSpell avengersShield = settings.PartyAvengersShieldnRank1 ? AvengersShieldRank1 : AvengersShield;
            // Pull logic
            if (ToolBox.Pull(cast, true, new List<AIOSpell> { avengersShield, Judgement }))
            {
                _combatMeleeTimer = new Timer(500);
                return;
            }
        }

        protected override void CombatNoTarget()
        {
            base.CombatNoTarget();

            if (settings.PartyTankSwitchTarget)
                AIOParty.SwitchTarget(cast, RighteousDefense);
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            WoWUnit Target = ObjectManager.Target;

            // Force melee
            if (_combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            ToolBox.CheckAutoAttack(Attack);

            if (settings.PartyTankSwitchTarget)
                AIOParty.SwitchTarget(cast, RighteousDefense);

            // Righteous Defense
            if (!Target.IsTargetingMe
                && Target.Target > 0
                && AIOParty.Group.Contains(Target.TargetObject)
                && cast.OnFocusUnit(RighteousDefense, Target.TargetObject))
                return;

            // Righteous Fury
            if (!Me.HaveBuff("Righteous Fury")
                && cast.OnSelf(RighteousFury))
                return;

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

            // Consecration
            if (!settings.PartyConsecrationRank1
                && ToolBox.GetNbEnemiesClose(7) > 1
                && cast.OnSelf(Consecration))
                return;

            // Consecration Rank 1
            if (settings.PartyConsecrationRank1
                && ToolBox.GetNbEnemiesClose(7) > 1
                && cast.OnSelf(ConsecrationRank1))
                return;

            // Avenging Wrath
            if (cast.OnSelf(AvengingWrath))
                return;

            // Hammer of Justice
            if (ToolBox.TargetIsCasting()
                && cast.OnTarget(HammerOfJustice))
                return;

            // Judgement
            if (Target.GetDistance < Judgement.MaxRange
                && (!Target.HaveBuff("Judgement of Wisdom") || Me.ManaPercentage > settings.PartyProtSealOfWisdom)
                && cast.OnTarget(Judgement))
                return;

            // Seal of Righteousness
            if (Me.ManaPercentage > settings.PartyProtSealOfWisdom
                && !Me.HaveBuff("Seal of Righteousness")
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Wisdom
            if (Me.ManaPercentage <= settings.PartyProtSealOfWisdom
                && !Me.HaveBuff("Seal of Wisdom")
                && cast.OnSelf(SealOfWisdom))
                return;

            // Holy Shield
            if (!settings.PartyHolyShieldRank1
                && !Me.HaveBuff("Holy Shield")
                && cast.OnSelf(HolyShield))
                return;

            // Holy Shield Rank 1
            if (settings.PartyHolyShieldRank1
                && !Me.HaveBuff("Holy Shield")
                && cast.OnSelf(HolyShieldRank1))
                return;

            // Avenger's Shield
            if (!settings.PartyAvengersShieldnRank1
                && !Target.IsTargetingMe
                && cast.OnTarget(AvengersShield))
                return;

            // Avenger's Shield Rank 1
            if (settings.PartyAvengersShieldnRank1
                && !Target.IsTargetingMe
                && cast.OnTarget(AvengersShieldRank1))
                return;

            // Hammer of Wrath
            if (settings.UseHammerOfWrath
                && cast.OnTarget(HammerOfWrath))
                return;

            // Exorcism
            if ((Target.CreatureTypeTarget == "Undead" || Target.CreatureTypeTarget == "Demon")
                && settings.UseExorcism
                && cast.OnTarget(Exorcism))
                return;
        }
    }
}
