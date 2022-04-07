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
using wManager.Events;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using static WholesomeTBCAIO.Helpers.Enums;

public class Main : ICustomClass
{
    private static readonly BackgroundWorker _talentThread = new BackgroundWorker();
    private static readonly BackgroundWorker _racialsThread = new BackgroundWorker();
    private static readonly BackgroundWorker _partyThread = new BackgroundWorker();
    private Racials _racials = new Racials();

    public static string wowClass = ObjectManager.Me.WowClass.ToString();
    public static int humanReflexTime = 500;
    public static bool isLaunched;
    public static string version = "3.1.02"; // Must match version in Version.txt
    public static bool HMPrunningAway = false;

    private IClassRotation selectedRotation;

    public float Range => RangeManager.GetRange();

    public void Initialize()
    {
        AIOTBCSettings.Load();
        AutoUpdater.CheckUpdate(version);
        Logger.Log($"Launching version {version} on client {Lua.LuaDoString<string>("v, b, d, t = GetBuildInfo(); return v")}");

        selectedRotation = ChooseRotation();

        if (selectedRotation != null)
        {
            isLaunched = true;

            FightEvents.OnFightLoop += FightLoopHandler;
            FightEvents.OnFightStart += FightStartHandler;
            FightEvents.OnFightEnd += FightEndHandler;
            LoggingEvents.OnAddLog += AddLogHandler;
            EventsLua.AttachEventLua("RESURRECT_REQUEST", e => ResurrectionEventHandler(e));
            EventsLua.AttachEventLua("PLAYER_DEAD", e => PlayerDeadHandler(e));
            EventsLua.AttachEventLua("READY_CHECK", e => ReadyCheckHandler(e));
            EventsLua.AttachEventLua("INSPECT_TALENT_READY", e => AIOParty.InspectTalentReadyHandler());
            EventsLua.AttachEventLua("PARTY_MEMBERS_CHANGED", e => AIOParty.GroupRosterChangedHandler());
            EventsLua.AttachEventLua("PARTY_MEMBER_DISABLE", e => AIOParty.GroupRosterChangedHandler());
            EventsLua.AttachEventLua("PARTY_MEMBER_ENABLE", e => AIOParty.GroupRosterChangedHandler());
            EventsLuaWithArgs.OnEventsLuaStringWithArgs += EventsWithArgsHandler;
            AIOParty.UpdateParty();

            if (!TalentsManager._isRunning)
            {
                _talentThread.DoWork += TalentsManager.DoTalentPulse;
                _talentThread.RunWorkerAsync();
            }

            if (!_racials._isRunning && CombatSettings.UseRacialSkills)
            {
                _racialsThread.DoWork += _racials.DoRacialsPulse;
                _racialsThread.RunWorkerAsync();
            }

            if (!AIORadar._isRunning)
            {
                _partyThread.DoWork += AIORadar.Pulse;
                _partyThread.RunWorkerAsync();
            }

            selectedRotation.Initialize(selectedRotation);
        }
        else
        {
            Logger.LogError("Class not supported.");
        }
    }

    public void Dispose()
    {
        selectedRotation?.Dispose();
        isLaunched = false;

        _talentThread.DoWork -= TalentsManager.DoTalentPulse;
        _talentThread.Dispose();
        TalentsManager._isRunning = false;
        if (CombatSettings.UseRacialSkills)
        {
            _racialsThread.DoWork -= _racials.DoRacialsPulse;
            _racialsThread.Dispose();
            _racials._isRunning = false;
        }

        _partyThread.DoWork -= AIORadar.Pulse;
        _partyThread.Dispose();
        AIORadar._isRunning = false;

        FightEvents.OnFightLoop -= FightLoopHandler;
        FightEvents.OnFightStart -= FightStartHandler;
        FightEvents.OnFightEnd -= FightEndHandler;
        LoggingEvents.OnAddLog -= AddLogHandler;
        EventsLuaWithArgs.OnEventsLuaStringWithArgs -= EventsWithArgsHandler;
    }

    public void ShowConfiguration() => CombatSettings?.ShowConfiguration();

    private IClassRotation ChooseRotation()
    {
        string spec = CombatSettings.Specialization;
        Dictionary<string, Specs> mySpecDictionary = GetSpecDictionary();

        if (!mySpecDictionary.ContainsKey(CombatSettings.Specialization))
        {
            Logger.LogError($"Couldn't find spec {CombatSettings.Specialization} in the class dictionary");
            return null;
        }

        switch (mySpecDictionary[CombatSettings.Specialization])
        {
            // Shaman
            case Specs.ShamanEnhancement: return new Enhancement();
            case Specs.ShamanEnhancementParty: return new EnhancementParty();
            case Specs.ShamanElemental: return new Elemental();
            case Specs.ShamanRestoParty: return new ShamanRestoParty();
            // Druid
            case Specs.DruidFeral: return new Feral();
            case Specs.DruidFeralDPSParty: return new FeralDPSParty();
            case Specs.DruidFeralTankParty: return new FeralTankParty();
            case Specs.DruidRestorationParty: return new RestorationParty();
            // Hunter
            case Specs.HunterBeastMaster: return new BeastMastery();
            case Specs.HunterBeastMasterParty: return new BeastMasteryParty();
            // Mage
            case Specs.MageFrost: return new Frost();
            case Specs.MageFrostParty: return new FrostParty();
            case Specs.MageArcane: return new Arcane();
            case Specs.MageArcaneParty: return new ArcaneParty();
            case Specs.MageFire: return new Fire();
            case Specs.MageFireParty: return new FireParty();
            // Paladin
            case Specs.PaladinRetribution: return new Retribution();
            case Specs.PaladinHolyParty: return new PaladinHolyParty();
            case Specs.PaladinRetributionParty: return new RetributionParty();
            case Specs.PaladinProtectionParty: return new PaladinProtectionParty();
            // Priest
            case Specs.PriestShadow: return new Shadow();
            case Specs.PriestShadowParty: return new ShadowParty();
            case Specs.PriestHolyParty: return new HolyPriestParty();
            case Specs.PriestHolyRaid: return new HolyPriestRaid();
            // Rogue
            case Specs.RogueCombat: return new Combat();
            case Specs.RogueCombatParty: return new RogueCombatParty();
            // Warlock
            case Specs.WarlockAffliction: return new Affliction();
            case Specs.WarlockDemonology: return new Demonology();
            case Specs.WarlockAfflictionParty: return new AfflictionParty();
            // Warrior
            case Specs.WarriorFury: return new Fury();
            case Specs.WarriorFuryParty: return new FuryParty();
            case Specs.WarriorProtectionParty: return new ProtectionWarrior();

            default: return null;
        }
    }

    private BaseSettings CombatSettings
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
                default: return null;
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
        string isReady = selectedRotation.AnswerReadyCheck() ? "true" : "false";
        Logger.Log($"Answering ReadyCheck ({isReady}), in {delay} ms");
        Thread.Sleep(delay);
        // Test with static instead
        Lua.LuaDoString($"ConfirmReadyCheck({isReady});");
        Lua.LuaDoString($"GetClickFrame('ReadyCheckFrame'):Hide();");
    }

    private void ResurrectionEventHandler(object context)
    {
        ToolBox.AcceptResurrect();
    }

    private void PlayerDeadHandler(object context)
    {
        Thread.Sleep(1000);
        Lua.LuaDoString("UseSoulstone();");
        Thread.Sleep(1000);
    }

    private void EventsWithArgsHandler(string id, List<string> args)
    {
        if (selectedRotation is Hunter
            && id == "UNIT_SPELLCAST_SUCCEEDED"
            && args[0] == "player"
            && args[1] == "Auto Shot")
            Hunter.LastAuto = DateTime.Now;

        if (selectedRotation is Paladin
            && args.Count >= 10
            && args[1] == "SPELL_CAST_SUCCESS"
            && id == "COMBAT_LOG_EVENT_UNFILTERED"
            && (args[9] == "Blessing of Might" || args[9] == "Blessing of Kings" || args[9] == "Blessing of Wisdom"))
            Paladin.RecordBlessingCast(args[3], args[9], args[6]);

        if (id == "UI_ERROR_MESSAGE")
        {
            if ((args[0] == "Your pet is dead." || args[0] == "You already control a summoned creature"))
                Hunter.PetIsDead = true;
        }
    }
}