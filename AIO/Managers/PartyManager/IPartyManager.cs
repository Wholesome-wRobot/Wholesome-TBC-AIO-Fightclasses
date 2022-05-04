using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.UnitCache.Entities;

namespace WholesomeTBCAIO.Managers.PartyManager
{
    public interface IPartyManager
    {
        void SwitchTarget(Cast cast, AIOSpell spell);
        bool PartyDrink(string drinkName, int threshold);
        List<IWoWPlayer> TanksNeedPriorityHeal(List<IWoWPlayer> tanks, List<IWoWPlayer> groupMembers, int priorityPercent);
    }
}
