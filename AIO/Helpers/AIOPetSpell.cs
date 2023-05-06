namespace WholesomeTBCAIO.Helpers
{
    internal class AIOPetSpell
    {
        public int Index { get; private set; }
        public string Name { get; private set; }
        public int Rank { get; private set; }
        public int Cost { get; private set; }
        public bool IsFunnel { get; private set; }
        public int PowerType { get; private set; }
        public int CastingTime { get; private set; }
        public int MinRange { get; private set; }
        public int MaxRange { get; private set; }

        public AIOPetSpell(
            int index,
            string name,
            int rank,
            int cost,
            bool isFunnel,
            int powerType,
            int castingTime,
            int minRange,
            int maxRange)
        {
            Index = index;
            Name = name;
            Rank = rank;
            Cost = cost;
            IsFunnel = isFunnel;
            PowerType = powerType;
            CastingTime = castingTime;
            MinRange = minRange;
            MaxRange = maxRange;
        }
    }
}
