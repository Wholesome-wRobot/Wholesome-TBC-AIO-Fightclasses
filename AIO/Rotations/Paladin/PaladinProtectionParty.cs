using System.Collections.Generic;
using System.Linq;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Rotations.Paladin
{
    public class PaladinProtectionParty : Paladin
    {
        public PaladinProtectionParty(BaseSettings settings) : base(settings)
        {
            RotationType = Enums.RotationType.Party;
            RotationRole = Enums.RotationRole.Tank;
        }

        protected override void BuffRotation()
        {
            if (!Me.HasDrinkAura || Me.ManaPercentage > 95)
            {
                // Righteous Fury
                if (!Me.HasAura(RighteousFury)
                && cast.OnSelf(RighteousFury))
                    return;

                base.BuffRotation();
            }
        }

        protected override void Pull()
        {
            base.Pull();

            WoWUnit Target = ObjectManager.Target;
            ToolBox.CheckAutoAttack(Attack);

            // Seal of Righteousness
            if (Me.ManaPercentage > settings.PartyProtSealOfWisdom
                && !Me.HasAura(SealOfRighteousness)
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Wisdom
            if (Me.ManaPercentage <= settings.PartyProtSealOfWisdom
                && !Me.HasAura(SealOfWisdom)
                && cast.OnSelf(SealOfWisdom))
                return;

            AIOSpell avengersShield = settings.PartyAvengersShieldnRank1 ? AvengersShieldRank1 : AvengersShield;
            // Pull logic
            if (ToolBox.Pull(cast, true, new List<AIOSpell> { avengersShield, Judgement }, unitCache))
            {
                combatMeleeTimer = new Timer(500);
                return;
            }
        }

        protected override void CombatNoTarget()
        {
            base.CombatNoTarget();

            if (settings.PartyTankSwitchTarget)
                partyManager.SwitchTarget(cast, RighteousDefense);
        }

        protected override void CombatRotation()
        {
            base.CombatRotation();

            // Force melee
            if (combatMeleeTimer.IsReady)
                RangeManager.SetRangeToMelee();

            ToolBox.CheckAutoAttack(Attack);

            if (settings.PartyTankSwitchTarget)
                partyManager.SwitchTarget(cast, RighteousDefense);

            // Righteous Defense
            if (Me.HasTarget
                && Target.HasTarget
                && !Target.IsTargetingMe)
            {
                IWoWPlayer memberToSave = unitCache.GroupAndRaid.Find(member => member.Guid == Target.TargetGuid);
                if (memberToSave != null && cast.OnFocusUnit(RighteousDefense, memberToSave))
                    return;
            }

            // Righteous Fury
            if (!Me.HasAura(RighteousFury)
                && cast.OnSelf(RighteousFury))
                return;

            // PARTY Lay On Hands
            List<IWoWPlayer> needsLoH = unitCache.GroupAndRaid
                .FindAll(m => m.HealthPercent < 10)
                .OrderBy(m => m.HealthPercent)
                .ToList();
            if (needsLoH.Count > 0 && cast.OnFocusUnit(LayOnHands, needsLoH[0]))
                return;

            // PARTY Purifiy
            if (settings.PartyPurify)
            {
                IWoWPlayer needsPurify = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasDiseaseDebuff(m.Name) || WTEffects.HasPoisonDebuff(m.Name));
                if (needsPurify != null && cast.OnFocusUnit(Purify, needsPurify))
                    return;
            }

            // PARTY Cleanse
            if (settings.PartyCleanse)
            {
                IWoWPlayer needsCleanse = unitCache.GroupAndRaid
                    .Find(m => WTEffects.HasMagicDebuff(m.Name));
                if (needsCleanse != null && cast.OnFocusUnit(Cleanse, needsCleanse))
                    return;
            }

            int nbEnemiesClose = unitCache.EnemiesFighting.FindAll(unit => !unit.IsTargetingMe && unit.GetDistance < 8).Count;
            // Consecration
            if (!settings.PartyConsecrationRank1
                && nbEnemiesClose > 1
                && cast.OnSelf(Consecration))
                return;

            // Consecration Rank 1
            if (settings.PartyConsecrationRank1
                && nbEnemiesClose > 1
                && cast.OnSelf(ConsecrationRank1))
                return;

            // Avenging Wrath
            if (cast.OnSelf(AvengingWrath))
                return;

            // Hammer of Justice
            if (WTCombat.TargetIsCasting()
                && cast.OnTarget(HammerOfJustice))
                return;

            // Judgement
            if (Target.GetDistance < Judgement.MaxRange
                && (!Target.HasAura("Judgement of Wisdom") || Me.ManaPercentage > settings.PartyProtSealOfWisdom)
                && cast.OnTarget(Judgement))
                return;

            // Seal of Righteousness
            if (Me.ManaPercentage > settings.PartyProtSealOfWisdom
                && !Me.HasAura(SealOfRighteousness)
                && cast.OnSelf(SealOfRighteousness))
                return;

            // Seal of Wisdom
            if (Me.ManaPercentage <= settings.PartyProtSealOfWisdom
                && !Me.HasAura(SealOfWisdom)
                && cast.OnSelf(SealOfWisdom))
                return;

            // Holy Shield
            if (!settings.PartyHolyShieldRank1
                && !Me.HasAura(HolyShield)
                && cast.OnSelf(HolyShield))
                return;

            // Holy Shield Rank 1
            if (settings.PartyHolyShieldRank1
                && !Me.HasAura(HolyShieldRank1)
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
