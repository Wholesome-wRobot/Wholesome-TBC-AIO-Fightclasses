using robotManager.Events;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using WholesomeTBCAIO;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Rotations;
using WholesomeTBCAIO.Rotations.Druid;
using WholesomeTBCAIO.Rotations.Hunter;
using WholesomeTBCAIO.Rotations.Mage;
using WholesomeTBCAIO.Rotations.Paladin;
using WholesomeTBCAIO.Rotations.Priest;
using WholesomeTBCAIO.Rotations.Rogue;
using WholesomeTBCAIO.Rotations.Shaman;
using WholesomeTBCAIO.Rotations.Warlock;
using WholesomeTBCAIO.Rotations.Warrior;
using WholesomeTBCAIO.Settings;
using WholesomeToolbox;
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeTBCAIO.Helpers.Enums;

public class Main : ICustomClass
{
    private IClassRotation _selectedRotation;
    public static string wowClass = ObjectManager.Me.WowClass.ToString();
    public static int humanReflexTime = 500;
    public static string version = "3.2.00"; // Must match version in Version.txt
    public static bool HMPrunningAway = false;

    public static bool IsLaunched { get; private set; }
    public float Range => RangeManager.GetRange();

    public void Initialize()
    {
        AIOTBCSettings.Load();
        AutoUpdater.CheckUpdate(version);
        Logger.Log($"Launching version {version} on client {WTLua.GetWoWVersion}");
        IsLaunched = true;

        _selectedRotation = ChooseRotation();

        if (_selectedRotation != null)
        {
            FightEvents.OnFightLoop += FightLoopHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightEnd += FightEndHandler;
            LoggingEvents.OnAddLog += AddLogHandler;
            EventsLua.AttachEventLua("RESURRECT_REQUEST", e => ResurrectionEventHandler(e));
            EventsLua.AttachEventLua("PLAYER_DEAD", e => PlayerDeadHandler(e));
            EventsLua.AttachEventLua("READY_CHECK", e => ReadyCheckHandler(e));
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsWithArgsHandler;

            _selectedRotation.Initialize(_selectedRotation);
        }
        else
        {
            Logger.LogError("Class not supported.");
        }
    }

    public void Dispose()
    {
        _selectedRotation?.Dispose();
        IsLaunched = false;
        FightEvents.OnFightLoop -= FightLoopHandler;
        FightEvents.OnFightStart -= FightStartHandler;
        FightEvents.OnFightEnd -= FightEndHandler;
        LoggingEvents.OnAddLog -= AddLogHandler;
        EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsWithArgsHandler;
    }

    public void ShowConfiguration() => baseSettings?.ShowConfiguration();

    private IClassRotation ChooseRotation()
    {
        string spec = baseSettings.Specialization;
        Dictionary<string, Specs> mySpecDictionary = GetSpecDictionary();

        if (!mySpecDictionary.ContainsKey(spec))
        {
            Logger.LogError($"Couldn't find spec {spec} in the class dictionary");
            return null;
        }

        switch (mySpecDictionary[spec])
        {
            // Shaman
            case Specs.ShamanEnhancement: return new Enhancement(baseSettings);
            case Specs.ShamanEnhancementParty: return new EnhancementParty(baseSettings);
            case Specs.ShamanElemental: return new Elemental(baseSettings);
            case Specs.ShamanRestoParty: return new ShamanRestoParty(baseSettings);
            // Druid
            case Specs.DruidFeral: return new Feral(baseSettings);
            case Specs.DruidFeralDPSParty: return new FeralDPSParty(baseSettings);
            case Specs.DruidFeralTankParty: return new FeralTankParty(baseSettings);
            case Specs.DruidRestorationParty: return new RestorationParty(baseSettings);
            // Hunter
            case Specs.HunterBeastMaster: return new BeastMastery(baseSettings);
            case Specs.HunterBeastMasterParty: return new BeastMasteryParty(baseSettings);
            // Mage
            case Specs.MageFrost: return new Frost(baseSettings);
            case Specs.MageFrostParty: return new FrostParty(baseSettings);
            case Specs.MageArcane: return new Arcane(baseSettings);
            case Specs.MageArcaneParty: return new ArcaneParty(baseSettings);
            case Specs.MageFire: return new Fire(baseSettings);
            case Specs.MageFireParty: return new FireParty(baseSettings);
            // Paladin
            case Specs.PaladinRetribution: return new Retribution(baseSettings);
            case Specs.PaladinHolyParty: return new PaladinHolyParty(baseSettings);
            case Specs.PaladinHolyRaid: return new PaladinHolyRaid(baseSettings);
            case Specs.PaladinRetributionParty: return new RetributionParty(baseSettings);
            case Specs.PaladinProtectionParty: return new PaladinProtectionParty(baseSettings);
            // Priest
            case Specs.PriestShadow: return new Shadow(baseSettings);
            case Specs.PriestShadowParty: return new ShadowParty(baseSettings);
            case Specs.PriestHolyParty: return new HolyPriestParty(baseSettings);
            case Specs.PriestHolyRaid: return new HolyPriestRaid(baseSettings);
            // Rogue
            case Specs.RogueCombat: return new Combat(baseSettings);
            case Specs.RogueCombatParty: return new RogueCombatParty(baseSettings);
            // Warlock
            case Specs.WarlockAffliction: return new Affliction(baseSettings);
            case Specs.WarlockDemonology: return new Demonology(baseSettings);
            case Specs.WarlockAfflictionParty: return new AfflictionParty(baseSettings);
            // Warrior
            case Specs.WarriorFury: return new Fury(baseSettings);
            case Specs.WarriorFuryParty: return new FuryParty(baseSettings);
            case Specs.WarriorProtectionParty: return new ProtectionWarrior(baseSettings);

            default: return null;
        }
    }

    private BaseSettings baseSettings
    {
        get
        {
            switch (wowClass)
            {
                case "Shaman": return ShamanSettings.Current;
                case "Druid": return DruidSettings.Current;
                case "Hunter": return HunterSettings.Current;
                case "Mage": return MageSettings.Current;
                case "Paladin": return PaladinSettings.Current;
                case "Priest": return PriestSettings.Current;
                case "Rogue": return RogueSettings.Current;
                case "Warlock": return WarlockSettings.Current;
                case "Warrior": return WarriorSettings.Current;
                default: throw new Exception($"WoWClass not recognized");
            }
        }
    }

    // EVENT HANDLERS
    private void AddLogHandler(Logging.Log log)
    {
        if (log.Text == "[HumanMasterPlugin] Starting to run away")
        {
            Logger.Log("HMP's running away feature detected. Disabling FightClass");
            HMPrunningAway = true;
        }
        else if (log.Text == "[HumanMasterPlugin] Stop fleeing, allow attacks again")
        {
            Logger.Log("Reenabling FightClass");
            HMPrunningAway = false;
        }
    }

    private void FightEndHandler(ulong guid)
    {
        HMPrunningAway = false;
    }

    private void FightStartHandler(WoWUnit unit, CancelEventArgs cancelable)
    {
        wManager.wManagerSetting.CurrentSetting.CalcuCombatRange = false;
        HMPrunningAway = false;
    }

    private void FightLoopHandler(WoWUnit woWPlayer, CancelEventArgs cancelable)
    {
        // Switch target if attacked by other faction player
        WoWPlayer player = ObjectManager.GetNearestWoWPlayer(ObjectManager.GetObjectWoWPlayer().Where(o => o.IsAttackable).ToList());
        if (player == null || !player.IsValid || !player.IsAlive || player.Faction == ObjectManager.Me.Faction || player.IsFlying || player.IsMyTarget || woWPlayer.Guid == player.Guid)
            return;
        if (player.InCombatWithMe && ObjectManager.Target.Type != WoWObjectType.Player)
        {
            cancelable.Cancel = true;
            Fight.StartFight(player.Guid, robotManager.Products.Products.ProductName != "WRotation", false);
        }
    }

    private void ReadyCheckHandler(object context)
    {
        var delay = 1000 + new Random().Next(1, 2000);
        string isReady = _selectedRotation.AnswerReadyCheck() ? "true" : "false";
        Logger.Log($"Answering ReadyCheck ({isReady}), in {delay} ms");
        Thread.Sleep(delay);
        // Test with static button instead
        Lua.LuaDoString($"ConfirmReadyCheck({isReady});");
        Lua.LuaDoString($"GetClickFrame('ReadyCheckFrame'):Hide();");
    }

    private void ResurrectionEventHandler(object context)
    {
        WTCombat.AcceptResurrect();
    }

    private void PlayerDeadHandler(object context)
    {
        Thread.Sleep(1000);
        Lua.LuaDoString("UseSoulstone();");
        Thread.Sleep(1000);
    }

    private void EventsWithArgsHandler(string id, List<string> args)
    {
        if (_selectedRotation is Hunter
            && id == "UNIT_SPELLCAST_SUCCEEDED"
            && args[0] == "player"
            && args[1] == "Auto Shot")
            Hunter.LastAuto = DateTime.Now;

        if (id == "UI_ERROR_MESSAGE")
        {
            if ((args[0] == "Your pet is dead." || args[0] == "You already control a summoned creature"))
                Hunter.PetIsDead = true;
        }
    }
}