using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Managers.PartyManager
{
    public interface IPartyManager : ICycleable
    {
        Dictionary<int, List<AIOPartyMember>> RaidGroups { get; }
        List<AIOPartyMember> GroupAndRaid { get; }
        List<AIOPartyMember> ClosePartyMembers { get; }
        List<WoWUnit> EnemiesFighting { get; }
        List<WoWUnit> TargetedByEnemies { get; }

        void SwitchTarget(Cast cast, AIOSpell spell);
        bool PartyDrink(string drinkName, int threshold);
    }
}
