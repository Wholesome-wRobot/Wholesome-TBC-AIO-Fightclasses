using wManager.Wow.Class;
using wManager.Wow.Helpers;

namespace WholesomeTBCAIO.Helpers
{
    public class AIOSpell
    {
        private Spell Spell { get; set; }
        public string Name { get; set; }
        public int Rank { get; set; }
        public int Cost { get; set; }
        public int PowerType { get; set; }
        public int CastTime { get; set; }
        public int MinRange { get; set; }
        public int MaxRange { get; set; }

        public AIOSpell(string spellName)
        {
            Spell = new Spell(spellName);
            RecordSpellInfos();
            LogSpellInfos();
        }

        public float CurrentCooldown() => ToolBox.GetSpellCooldown(Spell.Name);

        public int SpellCost() => Lua.LuaDoString<int>($"local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo('{Spell.Name}'); return cost");

        public void RecordSpellInfos()
        {
            string infos = Lua.LuaDoString<string>($@"
                local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo('{Spell.Name}');
                if (rank == '') then
                    rank = 'Rank 0'
                end
                return name..'$'..rank..'$'..cost..'$'..powerType..'$'..castTime..'$'..minRange..'$'..maxRange;");
            string[] infosArray = infos.Split('$');            
            Name = infosArray[0];
            Rank = int.Parse(infosArray[1].Replace("Rank ", ""));
            Cost = int.Parse(infosArray[2]);
            PowerType = int.Parse(infosArray[3]);
            CastTime = int.Parse(infosArray[4]);
            MinRange = int.Parse(infosArray[5]);
            MaxRange = int.Parse(infosArray[6]);
        }

        public void LogSpellInfos()
        {
            Logger.Log($"Name : {Name}");
            Logger.Log($"Rank : {Rank}");
            Logger.Log($"Cost : {Cost}");
            Logger.Log($"PowerType : {PowerType}");
            Logger.Log($"CastTime : {CastTime}");
            Logger.Log($"MinRange : {MinRange}");
            Logger.Log($"MaxRange : {MaxRange}");
        }
    }
}
