using WholesomeTBCAIO.Helpers;
using wManager.Wow.Class;

namespace WholesomeTBCAIO.Managers.UnitCache.Entities
{
    public class CachedAura : IAura
    {
        public int Stacks { get; }
        public int TimeLeft { get; }
        public string Name { get; }

        public CachedAura(Aura aura)
        {
            Stacks = aura.Stack;
            TimeLeft = aura.TimeLeft;
            Name = aura.ToString();
        }
    }
}
