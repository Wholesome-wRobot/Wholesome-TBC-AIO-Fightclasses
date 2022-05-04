using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.PartyManager;
using WholesomeTBCAIO.Managers.RacialsManager;
using WholesomeTBCAIO.Managers.TalentManager;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using static WholesomeTBCAIO.Helpers.Enums;

namespace WholesomeTBCAIO.Rotations
{
    public abstract class BaseRotation : IClassRotation
    {
        protected Cast cast;
        protected IPartyManager partyManager;
        protected IUnitCache unitCache;
        protected IRacialsManager racialsManager;
        protected ITalentManager talentManager;

        public RotationType RotationType { get; protected set; }
        public RotationRole RotationRole { get; protected set; }

        public BaseRotation(BaseSettings settings)
        {
            unitCache = new UnitCache();
            unitCache.Initialize();

            partyManager = new PartyManager(unitCache);

            if (settings.UseRacialSkills)
            {
                racialsManager = new RacialManager(partyManager, unitCache);
                racialsManager.Initialize();
            }

            if (settings.AssignTalents)
            {
                talentManager = new TalentManager(settings);
                talentManager.Initialize();
            }
        }

        protected IWoWLocalPlayer Me => unitCache.Me;
        protected IWoWUnit Target => unitCache.Target;
        protected IWoWUnit Pet => unitCache.Pet;

        public abstract bool AnswerReadyCheck();
        protected abstract void BuffRotation();
        protected abstract void Pull();
        protected abstract void CombatRotation();
        protected abstract void CombatNoTarget();
        protected abstract void HealerCombat();
        public abstract void Dispose();
        public abstract void Initialize(IClassRotation specialization);

        public void BaseDispose()
        {
            cast.Dispose();
            unitCache.Dispose();
            racialsManager?.Dispose();
            talentManager?.Dispose();
            Logger.Log($"Disposed");
        }

        protected void BaseInit(
            float range,
            AIOSpell baseSpell,
            AIOSpell wandSpell,
            BaseSettings settings)
        {
            RangeManager.SetRange(range);
            cast = new Cast(baseSpell, wandSpell, settings, unitCache);

            if (settings.PartyDrinkName != "")
            {
                WTSettings.AddToDoNotSellList(settings.PartyDrinkName);
            }
        }
    }
}
