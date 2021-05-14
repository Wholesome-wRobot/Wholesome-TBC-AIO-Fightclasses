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

        public bool IsSpellUsable => Spell.IsSpellUsable;
        public bool KnownSpell => Spell.KnownSpell;
        public bool TargetHaveBuff => Spell.TargetHaveBuff;
        public bool IsDistanceGood => Spell.IsDistanceGood;
        public bool HaveBuff => Spell.HaveBuff;

        public AIOSpell(string spellName)
        {
            Spell = new Spell(spellName);
            Name = Spell.Name;
            RecordSpellInfos();
            //LogSpellInfos();
        }

        public float GetCurrentCooldown => Lua.LuaDoString<float>($"local startTime, duration, enable = GetSpellCooldown('{Name}'); return duration - (GetTime() - startTime);");

        public int SpellCost() => Lua.LuaDoString<int>($"local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo('{Spell.Name}'); return cost");

        public void RecordSpellInfos()
        {
            string infos = Lua.LuaDoString<string>($@"
                local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo('{Spell.Name}');
                if (rank == '' or rank == 'Racial' or rank == 'Shapeshift') then
                    rank = 'Rank 0'
                end
                return name..'$'..rank..'$'..cost..'$'..powerType..'$'..castTime..'$'..minRange..'$'..maxRange;");
            string[] infosArray = infos.Split('$');

            if (infosArray.Length > 1)
            {
                Rank = ParseInt(infosArray[1].Replace("Rank ", ""));
                Cost = ParseInt(infosArray[2]);
                PowerType = ParseInt(infosArray[3]);
                CastTime = ParseInt(infosArray[4]);
                MinRange = ParseInt(infosArray[5]);
                MaxRange = ParseInt(infosArray[6]);
            }
        }

        private int ParseInt(string stringToParse)
        {
            int result = 0;
            if (!int.TryParse(stringToParse, out result))
                Logger.LogError($"Couldn't parse spell info {stringToParse}");
            return result;

        }

        public void LogSpellInfos()
        {
            Logger.Log($"**************************");
            Logger.Log($"Name : {Name}");
            Logger.Log($"Rank : {Rank}");
            Logger.Log($"Cost : {Cost}");
            Logger.Log($"PowerType : {PowerType}");
            Logger.Log($"CastTime : {CastTime}");
            Logger.Log($"MinRange : {MinRange}");
            Logger.Log($"MaxRange : {MaxRange}");
        }

        public void Launch(bool stopMove, bool waitIsCast = true, bool ignoreIfCast = false)
        {
            Spell.Launch(stopMove, waitIsCast, ignoreIfCast);
        }

        public void Launch()
        {
            Spell.Launch();
        }
    }
}
