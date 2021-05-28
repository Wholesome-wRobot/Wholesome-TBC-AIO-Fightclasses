using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO.Settings;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Helpers
{
    public class TalentsManager
    {
        private static bool _isAssigning = false;
        private static bool _isInitialized = false;
        public static bool _isRunning = false;
        public static List<string> _talentsCodes = new List<string> { };
        private static int _talentPulseTimer = 60000 * 5; // 5 minutes

        // Talent initialization
        public static void InitTalents(BaseSettings settings)
        {
            if (settings.AssignTalents)
            {
                if (settings.UseDefaultTalents)
                {
                    SetTalentCodes(Enums.GetSpecDictionary()[settings.Specialization]);
                }
                else
                {
                    SetTalentCodes(settings.TalentCodes);
                }

                if (_talentsCodes.Count() > 0)
                    Logger.Log($"Talents code [{settings.Specialization}]: {_talentsCodes.Last()}");
                else
                    Logger.LogError("No talent code");

                _isInitialized = true;
            }
        }

        // Set the default talents codes to use
        public static void SetTalentCodes(Enums.Specs spec)
        {
            switch (spec)
            {
                // DRUID
                case Enums.Specs.DruidFeral:
                    _talentsCodes = new List<string>
                    {
                        "00000000000000000000050302203002000000000000000000000000000000",
                        "00000000000000000000050302203032010000000000000000000000000000",
                        "00000000000000000000050302203032212500120000000000000000000000",
                        "00000000000000000000050302203032212520125100000000000000000000",
                        "00000000000000000000050302203032212520125105050001000000000000",
                        "00000000000000000000052303203232212530125105053001000000000000"
                    };
                    break;
                case Enums.Specs.DruidFeralDPSParty:
                    _talentsCodes = new List<string>
                    {
                        "00000000000000000000050302203002000000000000000000000000000000",
                        "00000000000000000000050302203032010000000000000000000000000000",
                        "00000000000000000000050302203032212500120000000000000000000000",
                        "00000000000000000000050302203032212520125100000000000000000000",
                        "00000000000000000000050302203032212520125105050001000000000000",
                        "00000000000000000000052303203232212530125105053001000000000000"
                    };
                    break;
                case Enums.Specs.DruidFeralTankParty:
                    _talentsCodes = new List<string>
                    {
                        "00000000000000000000050302010000000000000000000000000000000000",
                        "00000000000000000000050303211030000000000000000000000000000000",
                        "00000000000000000000050303213030010000000000000000000000000000",
                        "00000000000000000000052303213032010500120000000000000000000000",
                        "00000000000000000000052303213032010520125100000000000000000000",
                        "00000000000000000000055303213232010530125105500001000000000000",
                        "00000000000000000000055303213232010530125105500301000000000000"
                    };
                    break;
                case Enums.Specs.DruidRestorationParty:
                    _talentsCodes = new List<string>
                    {
                        "00000000000000000000000000000000000000000050050320030000000000",
                        "00000000000000000000000000000000000000000050050320231500401000",
                        "00000000000000000000000000000000000000000050050340431500401051",
                        "00000000000000000000000000000000000000000050050350531500531351",
                        "01432001000000000000000000000000000000000050050350531500531351"
                    };
                    break;
                // HUNTER
                case Enums.Specs.HunterBeastMaster:
                    _talentsCodes = new List<string>
                    {
                        "5020122120501000000000000000000000000000000000000000000000000000",
                        "5020122122501205010000000000000000000000000000000000000000000000",
                        "5020122142501225010510000000000000000000000000000000000000000000",
                        "5020122142501225010510550200000000000000000000000000000000000000",
                        "5020322152501225010510555200000000000000000000000000000000000000"
                    };
                    break;
                case Enums.Specs.HunterBeastMasterParty:
                    _talentsCodes = new List<string>
                    {
                        "5020122120501000000000000000000000000000000000000000000000000000",
                        "5020122122501205010000000000000000000000000000000000000000000000",
                        "5020122142501225010510000000000000000000000000000000000000000000",
                        "5020122142501225010510550200000000000000000000000000000000000000",
                        "5020322152501225010510555200000000000000000000000000000000000000"
                    };
                    break;
                // MAGE
                case Enums.Specs.MageFrost:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000000000000000000000000000500320010000000000000",
                        "0000000000000000000000000000000000000000000000504320010005010000000",
                        "0000000000000000000000000000000000000000000000505323010005010051000",
                        "0000000000000000000000000000000000000000000000505323310005010051241",
                        "0000000000000000000000000000000000000000000000535323310035013251551",
                        "0000000000000000000000005000000000000000000000535323310035013251551"
                    };
                    break;
                case Enums.Specs.MageFrostParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000000000000000000000000000502300010000000000000",
                        "0000000000000000000000000000000000000000000000503300310030000000000",
                        "0000000000000000000000000000000000000000000000503320310030010000000",
                        "0000000000000000000000000000000000000000000000505320310032010051000",
                        "0000000000000000000000000000000000000000000000535320310033010051051",
                        "2300050000000000000000000000000000000000000000535323310033010251551"
                    };
                    break;
                case Enums.Specs.MageArcane:
                    _talentsCodes = new List<string>
                    {
                        "0050050000000000000000000000000000000000000000000000000000000000000",
                        "0052050310030000000000000000000000000000000000000000000000000000000",
                        "0152050310030140320120000000000000000000000000000000000000000000000",
                        "0152050310030150330125000000000000000000000000000000000000000000000",
                        "0152050310030150330125100000000000000000000000000000000000000000000",
                        "2552050312030152333125100000000000000000000000000000000000000000000",
                        "2552050312030152333125105002000000000000000000000000000000000000000"
                    };
                    break;
                case Enums.Specs.MageArcaneParty:
                    _talentsCodes = new List<string>
                    {
                        "2300050300000000000000000000000000000000000000000000000000000000000",
                        "2500052300030150310120000000000000000000000000000000000000000000000",
                        "2500052300030150330125000000000000000000000000535000010000000000000",
                        "2500052300030150330125000000000000000000000000535000310030010000000"
                    };
                    break;
                case Enums.Specs.MageFire:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000050500000200000000000000000000000000000000000",
                        "0000000000000000000000053500000200300000000000000000000000000000000",
                        "0000000000000000000000055500000200303000000000000000000000000000000",
                        "0000000000000000000000055500001200303105010000000000000000000000000",
                        "0000000000000000000000055500001200303105311510000000000000000000000",
                        "0000000000000000000000055500001200333105312510030000000000000000000",
                        "2300050010000000000000055500001200333105312510030000000000000000000",
                        "2302050010000000000000055500001200333105312510030000000000000000000"
                    };
                    break;
                case Enums.Specs.MageFireParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000050500001000000000000000000000000000000000000",
                        "0000000000000000000000050500201230203000000000000000000000000000000",
                        "0000000000000000000000050500201230303105010000000000000000000000000",
                        "0000000000000000000000050500201230303105311510030000000000000000000",
                        "2300050000000000000000050521201230333105312510030000000000000000000"
                    };
                    break;
                // PALADIN
                case Enums.Specs.PaladinRetribution:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000000000000000000000000523005130000000000000",
                        "0000000000000000000000000000000000000000001523005130000100000000",
                        "0000000000000000000000000000000000000000001523005130003115321041",
                        "5500300000000000000000000000000000000000005523005130003125331051"
                    };
                    break;
                case Enums.Specs.PaladinRetributionParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000000000000000000000000523000100000000000000",
                        "0000000000000000000000000000000000000000000523005100030000000000",
                        "0000000000000000000000000000000000000000000523005110033115001000",
                        "0000000000000000000000000000000000000000000523005110033115131051",
                        "0000000000000000000050300000000000000000000523005110033115131051",
                        "0000000000000000000050300000000000000000000523005120033125331051",
                        "0000000000000000000050320100000000000000000523005120033125331051",
                        "5000000000000000000050320100000000000000000523005120033125331051"
                    };
                    break;
                case Enums.Specs.PaladinProtectionParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000005005003000000000000000000000000000000000000",
                        "0000000000000000000005005113500001000000000000000000000000000000",
                        "0000000000000000000005205123500011005010000000000000000000000000",
                        "0000000000000000000005205123500011005014510000000000000000000000",
                        "0000000000000000000005205123500011025215510500500000000000000000",
                        "0000000000000000000005205123500011025215510520500000000000000000",
                        "0000000000000000000005305133500021025215510520500000000000000000"
                    };
                    break;
                case Enums.Specs.PaladinHolyParty:
                    _talentsCodes = new List<string>
                    {
                        "0550311050013000000000000000000000000000000000000000000000000000",
                        "0550311051013050100000000000000000000000000000000000000000000000",
                        "0550311052013053105150320100000000000000005000000000000000000000",
                        "0550312152013253105150320100000000000000005000000000000000000000"
                    };
                    break;
                // PRIEST
                case Enums.Specs.PriestShadow:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000000000000000000000000500230010000000000000",
                        "0000000000000000000000000000000000000000000500232310041120000000",
                        "0000000000000000000000000000000000000000000500232310041121051451",
                        "0500320130000000000000000000000000000000000500232510051123051551"
                    };
                    break;
                case Enums.Specs.PriestShadowParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000000000000000000000000503200310000000000000",
                        "0000000000000000000000000000000000000000000503210310050103000000",
                        "0000000000000000000000000000000000000000000503220310050103051451",
                        "5002300130000000000000000000000000000000000503250310050123051551"
                    };
                    break;
                case Enums.Specs.PriestHolyParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000032050030000000000000000000000000000000000",
                        "0000000000000000000000232050030300000000000000000000000000000000",
                        "0000000000000000000000234050030300140530000000000000000000000000",
                        "0000000000000000000000235050030300150530030000000000000000000000",
                        "5002301130500120000000235050030300150530030000000000000000000000"
                    };
                    break;
                // ROGUE
                case Enums.Specs.RogueCombat:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000230050000000000000000000000000000000000000000",
                        "0000000000000000000000230550100040100000000000000000000000000000000",
                        "0000000000000000000000230550100050130020000000000000000000000000000",
                        "0000000000000000000000230550100050150021010000000000000000000000000",
                        "0000000000000000000000230550100050150023210510000000000000000000000",
                        "0000000000000000000000230550100050150023210515000300000000000000000",
                        "3053001000000000000000230550100050150023210515000300000000000000000"
                    };
                    break;
                case Enums.Specs.RogueCombatParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000230050000000000000000000000000000000000000000",
                        "0000000000000000000000233050020050140000000000000000000000000000000",
                        "0000000000000000000000233050020050140023010000000000000000000000000",
                        "0000000000000000000000233050020050150023211510000000000000000000000",
                        "0053201054000000000000233050020050150023211510000000000000000000000"
                    };
                    break;
                // SHAMAN
                case Enums.Specs.ShamanEnhancement:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000050052301040010000000000000000000000000000",
                        "0000000000000000000050052301050013050110000000000000000000000",
                        "0000000000000000000050052301050013053115100000000000000000000",
                        "2500310000000000000050052321450013353115100000000000000000000"
                    };
                    break;
                case Enums.Specs.ShamanEnhancementParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000050050001000000000000000000000000000000000",
                        "0000000000000000000050050021000000000000000000000000000000000",
                        "0000000000000000000050052021050013000000000000000000000000000",
                        "0000000000000000000050052021050113050110000000000000000000000",
                        "0000000000000000000050052021050113053115100000000000000000000",
                        "2500305020000000000050052021050113353115100000000000000000000"
                    };
                    break;
                case Enums.Specs.ShamanElemental:
                    _talentsCodes = new List<string>
                    {
                        "5200310503000000000000000000000000000000000000000000000000000",
                        "5300310503001405100000000000000000000000000000000000000000000",
                        "5500310503001515105100000000000000000000000000000000000000000",
                        "5500310503001535105150000300000000000000005005000000000000000",
                    };
                    break;
                case Enums.Specs.ShamanRestoParty:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000000000000000000000052030001000000000000",
                        "0000000000000000000000000000000000000000055030001150010000000",
                        "0000000000000000000000000000000000000000055030001250310510000",
                        "0000000000000000000000000000000000000000055030001354310510311",
                        "5000000000000000000000000000000000000000055035001355310510321",
                        "5000000000000000000000000000000000000000055035051355310510321",
                        "5003000000000000000000000000000000000000055035051355310510321"
                    };
                    break;
                // WARLOCK
                case Enums.Specs.WarlockDemonology:
                    _talentsCodes = new List<string>
                    {
                        "0000000000000000000000052300100000000000000000000000000000000000",
                        "0000000000000000000000052310130050100000000000000000000000000000",
                        "0000000000000000000000052330132050100501000000000000000000000000",
                        "0000000000000000000000052330132050102501251000000000000000000000",
                        "0000000000000000000000052330133050102531351000000000000000000000",
                        "1500222210000000000000052330133050102531351000000000000000000000"
                    };
                    break;
                case Enums.Specs.WarlockAffliction:
                    _talentsCodes = new List<string>
                    {
                        "0502210010000000000000000000000000000000000000000000000000000000",
                        "0502222110230100000000000000000000000000000000000000000000000000",
                        "0502222110234105500000000000000000000000000000000000000000000000",
                        "0502222510234105510010000000000000000000000000000000000000000000",
                        "0502222510234105510010052300100000000000000000000000000000000000",
                        "0502222510234105510010052300100000000000000000000000000000000000",
                        "1502222510235105510010052330130100000000000000000000000000000000"
                    };
                    break;
                case Enums.Specs.WarlockAfflictionParty:
                    _talentsCodes = new List<string>
                    {
                        "0502210410230100000000000000000000000000000000000000000000000000",
                        "0502210410234105500300000000000000000000000000000000000000000000",
                        "0502210411235105500310000000000000000000000000000000000000000000",
                        "0502210411235105500310000000000000000000000505000500200000000000",
                        "0502210412235105500310200000000000000000000505000500200000000000"
                    };
                    break;
                // WARRIOR
                case Enums.Specs.WarriorFury:
                    _talentsCodes = new List<string>
                    {
                        "000000000000000000000000505000500501200000000000000000000000000000",
                        "000000000000000000000000505000520501205010000000000000000000000000",
                        "000000000000000000000000505000520501205311510000000000000000000000",
                        "000000000000000000000000505000550501205311510000000000000000000000",
                        "320230013000000000000000505000550501205311510000000000000000000000",
                        "320240013020000000000000505000550501205311510000000000000000000000"
                    };
                    break;
                case Enums.Specs.WarriorFuryParty:
                    _talentsCodes = new List<string>
                    {
                        "000000000000000000000000505000500501200000000000000000000000000000",
                        "000000000000000000000000505000520501205010000000000000000000000000",
                        "000000000000000000000000505000520501205311510000000000000000000000",
                        "000000000000000000000000505000550501205311510000000000000000000000",
                        "320230013000000000000000505000550501205311510000000000000000000000",
                        "320240013020000000000000505000550501205311510000000000000000000000"
                    };
                    break;
                case Enums.Specs.WarriorProtectionParty:
                    _talentsCodes = new List<string>
                    {
                        "000000000000000000000000000000000000000000000055011000000000000000",
                        "000000000000000000000000000000000000000000000055211033000100000000",
                        "000000000000000000000000000000000000000000001055511033000103201300",
                        "000000000000000000000000000000000000000000001055511033000103231331",
                        "050000000000000000000000000000000000000000001055511033000103231331",
                        "350000000000000000000000501000000000000000002055511033000103531351"
                    };
                    break;

                default:
                    Logger.LogError($"Couldn't find talent codes for {spec}.");
                    break;
            }
        }

        // Set the custom talents codes to use
        public static void SetTalentCodes(List<string> talentsCodes)
        {
            _talentsCodes = talentsCodes;
        }

        // Talent pulse
        public static void DoTalentPulse(object sender, DoWorkEventArgs args)
        {
            _isRunning = true;
            while (Main.isLaunched && _isRunning)
            {
                Thread.Sleep(3000);
                try
                {
                    if (Conditions.InGameAndConnectedAndProductStartedNotInPause
                        && ObjectManager.Me.IsAlive 
                        && Main.isLaunched 
                        && !_isAssigning 
                        && _isInitialized 
                        && _isRunning)
                    {
                        Logger.LogDebug("Assigning Talents");
                        _isAssigning = true;
                        AssignTalents(_talentsCodes);
                        _isAssigning = false;
                    }
                }
                catch (Exception arg)
                {
                    Logging.WriteError(string.Concat(arg), true);
                }
                Thread.Sleep(_talentPulseTimer);
            }
            _isRunning = false;
        }

        // Talent assignation 
        public static void AssignTalents(List<string> TalentCodes)
        {
            // Number of talents in each tree
            List<int> NumTalentsInTrees = new List<int>()
            {
                Lua.LuaDoString<int>("return GetNumTalents(1)"),
                Lua.LuaDoString<int>("return GetNumTalents(2)"),
                Lua.LuaDoString<int>("return GetNumTalents(3)")
            };

            if (!_isInitialized)
            {
                Thread.Sleep(500);
            }
            else if (TalentCodes.Count() <= 0)
            {
                Logger.LogError("No talent code");
            }
            else if (Lua.LuaDoString<int>("local unspentTalentPoints, _ = UnitCharacterPoints('player'); return unspentTalentPoints;") <= 0)
            {
                Logger.LogDebug("No talent point to spend");
            }
            else
            {
                bool stop = false;

                // Loop for each TalentCode in list
                foreach (string talentsCode in TalentCodes)
                {
                    if (stop)
                        break;

                    // check if talent code length is correct
                    if ((NumTalentsInTrees[0] + NumTalentsInTrees[1] + NumTalentsInTrees[2]) != talentsCode.Length)
                    {
                        Logger.LogError("WARNING: Your talents code length is incorrect. Please use " +
                            "http://armory.twinstar.cz/talent-calc.php to generate valid codes.");
                        Logger.LogError("Talents code : " + talentsCode);
                        stop = true;
                        break;
                    }

                    // TalentCode per tree
                    List<string> TalentCodeTrees = new List<string>()
                {
                    talentsCode.Substring(0, NumTalentsInTrees[0]),
                    talentsCode.Substring(NumTalentsInTrees[0], NumTalentsInTrees[1]),
                    talentsCode.Substring(NumTalentsInTrees[0] + NumTalentsInTrees[1], NumTalentsInTrees[2])
                };

                    // loop in 3 trees
                    for (int k = 1; k <= 3; k++)
                    {
                        if (stop)
                            break;

                        // loop for each talent
                        for (int i = 0; i < NumTalentsInTrees[k - 1]; i++)
                        {
                            if (stop)
                                break;

                            int _talentNumber = i + 1;
                            string _talentName = Lua.LuaDoString<string>("local name, _, _, _, _, _, _, _ = GetTalentInfo(" + k + ", " + _talentNumber + "); return name;");
                            int _currentRank = Lua.LuaDoString<int>("_, _, _, _, currentRank, _, _, _ = GetTalentInfo(" + k + ", " + _talentNumber + "); return currentRank;");
                            int _realMaxRank = Lua.LuaDoString<int>("_, _, _, _, _, maxRank, _, _ = GetTalentInfo(" + k + ", " + _talentNumber + "); return maxRank;");

                            int _pointsToAssignInTalent = Convert.ToInt16(TalentCodeTrees[k - 1].Substring(i, 1));

                            if (_currentRank > _pointsToAssignInTalent && TalentCodes.Last().Equals(talentsCode))
                            {
                                Logger.LogError("WARNING: Your assigned talent points don't match your talent code. Please reset your talents or review your talents code." +
                                    " You have " + _currentRank + " point(s) in " + _talentName + " where you should have " + _pointsToAssignInTalent + " point(s)");
                                Logger.LogError("Talents code : " + talentsCode);
                                stop = true;
                            }
                            else if (_pointsToAssignInTalent > _realMaxRank)
                            {
                                Logger.LogError($"WARNING : You're trying to assign {_pointsToAssignInTalent} points into {_talentName}," +
                                    $" maximum is {_realMaxRank} points for this talent. Please check your talent code.");
                                Logger.LogError("Talents code : " + talentsCode);
                                stop = true;
                            }
                            else if (_currentRank != _pointsToAssignInTalent)
                            {
                                // loop for individual talent rank
                                for (int j = 0; j < _pointsToAssignInTalent - _currentRank; j++)
                                {
                                    if (!Main.isLaunched)
                                        stop = true;
                                    if (stop)
                                        break;
                                    Lua.LuaDoString("LearnTalent(" + k + ", " + _talentNumber + ")");
                                    Thread.Sleep(500 + Usefuls.Latency);
                                    int _newRank = Lua.LuaDoString<int>("_, _, _, _, currentRank, _, _, _ = GetTalentInfo(" + k + ", " + _talentNumber + "); return currentRank;");
                                    Logger.Log("Assigned talent: " + _talentName + " : " + _newRank + "/" + _pointsToAssignInTalent, Color.SteelBlue);
                                    if (Lua.LuaDoString<int>("local unspentTalentPoints, _ = UnitCharacterPoints('player'); return unspentTalentPoints;") <= 0)
                                        stop = true;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}