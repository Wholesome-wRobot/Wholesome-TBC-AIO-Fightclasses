using System.Collections.Generic;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Helpers
{
    public class AIOSpell : Spell
    {
        public new string Name { get; set; }
        public int Rank { get; set; }
        public int Cost { get; set; }
        public int PowerType { get; set; }
        public new float CastTime { get; set; }
        public new float MinRange { get; set; }
        public new float MaxRange { get; set; }
        public bool ForceLua { get; set; }
        public bool IsChannel { get; set; }
        public bool IsResurrectionSpell { get; set; }
        public bool PreventDoubleCast { get; set; }
        public bool OnDeadTarget { get; set; }
        public new bool IsSpellUsable {
            get
            {
                if (!ForceLua)
                    return base.IsSpellUsable;
                else
                    return KnownSpell && GetCurrentCooldown < 0;
            }
        }
        private int ForcedCooldown { get; set; }
        private Timer ForcedCooldownTimer { get; set; } = new Timer();

        private static List<AIOSpell> AllSpells = new List<AIOSpell>();


        public AIOSpell(string spellName, int rank = 0) : base(spellName)
        {
            Name = spellName;
            IsChannel = ChannelSpells.Contains(Name);
            PreventDoubleCast = SpellsToKeepFromDoubleCasting.Contains(Name);
            OnDeadTarget = OnDeadSpells.Contains(Name);
            IsResurrectionSpell = ResurrectionSpells.Contains(Name);

            if (Name.Contains("(") || Name.Contains(")"))
                Name += "()";

            RecordSpellInfos(rank);
            ForceLua = rank > 0 || Name.Contains("()");

            ForcedCooldown = ForcedCoolDowns.ContainsKey(Name) ? ForcedCoolDowns[Name] : 0;

            AllSpells.Add(this);
            //LogSpellInfos();
        }

        public new void Launch(bool stopMove, bool waitIsCast = true, bool ignoreIfCast = false, string luaUnitId = "target")
        {
            if (!ForceLua)
                base.Launch(stopMove, waitIsCast, ignoreIfCast, luaUnitId);
            else
            {
                if (stopMove)
                    MovementManager.StopMoveNewThread();

                string rankString = Rank > 0 ? $"(Rank {Rank})" : "()";
                Logger.LogFight($"[Spell-LUA] Cast (on {luaUnitId}) {Name.Replace("()", "")} {rankString}");
                Lua.RunMacroText($"/cast [target={luaUnitId}] {Name.Replace("()", "")}{rankString}");
            }
        }

        public new void Launch()
        {
            if (!ForceLua)
                base.Launch();
            else
            {
                string rankString = Rank > 0 ? $"(Rank {Rank})" : "()";
                Logger.LogFight($"[Spell] Cast (on target) {Name} {rankString}");
                Lua.RunMacroText($"/cast {Name}{rankString}");
            }
        }
        /*
        public AIOSpell(int spellId, int rank = 0) : base(spellId)
        {
            RecordSpellInfos(rank);
            ForceLua = rank > 0 || Name.Contains("(") || Name.Contains(")");
            AllSpells.Add(this);
            //LogSpellInfos();
        }
        */
        public static AIOSpell GetSpellByName(string name) => AllSpells.Find(s => s.Name == name);
        public void StartForcedCooldown()
        {
            if (ForcedCooldown > 0)
                ForcedCooldownTimer = new Timer(ForcedCooldown);
        }
        public bool IsForcedCooldownReady => ForcedCooldownTimer.IsReady;

        public float GetCurrentCooldown => Lua.LuaDoString<float>($@"local startTime, duration, _ = GetSpellCooldown(""{Name.Replace("\"", "\\\"")}"");
            if (startTime == nil) then return 0 end;
            return (duration - (GetTime() - startTime)) * 1000;");

        public void RecordSpellInfos(int rank)
        {
            string rankString = rank > 0 ? $@", ""Rank {rank}""" : "";

            string infos = Lua.LuaDoString<string>($@"
                local name, rank, icon, cost, isFunnel, powerType, castTime, minRange, maxRange = GetSpellInfo(""{Name.Replace("\"", "\\\"")}""{rankString});
                if (name == nil) then return nil end
                if (rank == '' or rank == 'Racial' or rank == 'Shapeshift' or rank == 'Summon') then
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
            if (!int.TryParse(stringToParse, out int result))
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

        private List<string> OnDeadSpells = new List<string>()
        {
            "Revive",
            "Rebirth",
            "Redemption",
            "Resurrection",
            "Ancestral Spirit"
        };

        private List<string> SpellsToKeepFromDoubleCasting = new List<string>()
        {
            "Healing Touch",
            "Regrowth",
            "Revive Pet",
            "Polymorph",
            //"Arcane Blast",
            //"Scorch",
            "Hammer of Wrath",
            "Unstable Affliction",
            "Flash of Light",
            "Holy Light",
            "Redemption",
            "Lesser Heal",
            "Heal",
            "Greater Heal",
            "Holy Fire",
            "Flash Heal",
            "Vampiric Touch",
            "Resurrection",
            "Prayer of Healing",
            "Prayer of Mending",
            "Healing Wave",
            "Lesser Healing Wave",
            "Ghost Wolf",
            "Earth Shield",
            "Chain Heal",
            "Ancestral Spirit",
            "Immolate",
            "Corruption",
            "Summon Imp",
            "Summon Voidwalker",
            "Summon Felguard",
            "Create HealthStone",
            "Create Soulstone",
            "Seed of Corruption"
        };

        private List<string> ChannelSpells = new List<string>()
        {
            "Arcane Missiles",
            "Evocation",
            "Mind Flay",
            "Drain Soul",
            "Drain Life",
            "Drain Mana",
            "Health Funnel",
            "Cannibalize"
        };

        private List<string> ResurrectionSpells = new List<string>()
        {
            "Redemption",
            "Resurrection",
            "Ancestral Spirit"
        };

        private Dictionary<string, int> ForcedCoolDowns = new Dictionary<string, int>()
        {
            {"Redemption", 4000 },
            {"Resurrection", 4000 },
            {"Ancestral Spirit", 4000 },
            {"Call Pet", 5000 },
        };
    }
}
