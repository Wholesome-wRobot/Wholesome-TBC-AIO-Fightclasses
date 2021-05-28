using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public class AIOPartyMember : WoWPlayer
    {
        public string Specialization { get; set; } = null;

        public AIOPartyMember(uint address) : base(address)
        {
        }
    }
}
