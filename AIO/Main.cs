using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using robotManager.Helpful;
using wManager.Events;
using System.ComponentModel;
using WholesomeTBCAIO;
using WholesomeTBCAIO.Rotations;
using WholesomeTBCAIO.Settings;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Rotations.Shaman;
using WholesomeTBCAIO.Rotations.Druid;
using WholesomeTBCAIO.Rotations.Hunter;
using WholesomeTBCAIO.Rotations.Mage;
using WholesomeTBCAIO.Rotations.Paladin;
using WholesomeTBCAIO.Rotations.Priest;
using WholesomeTBCAIO.Rotations.Rogue;
using WholesomeTBCAIO.Rotations.Warlock;
using WholesomeTBCAIO.Rotations.Warrior;
using wManager.Wow.Enums;
using System.Linq;
using robotManager.FiniteStateMachine;
using robotManager.Events;
using System.Threading;

public class Main : ICustomClass
{
    private static readonly BackgroundWorker _talentThread = new BackgroundWorker();
    private static readonly BackgroundWorker _racialsThread = new BackgroundWorker();
    private static readonly BackgroundWorker _partyThread = new BackgroundWorker();
    private Racials _racials = new Racials();

    public static string wowClass = ObjectManager.Me.WowClass.ToString();
    public static int humanReflexTime = 500;
    public static bool isLaunched;
    public static string version = "2.1.95"; // Must match version in Version.txt
    public static bool HMPrunningAway = false;
    public static State currentState;

    private IClassRotation selectedRotation;

    public float Range
    {
        get { return RangeManager.GetRange(); }
    }

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
            FiniteStateMachineEvents.OnRunState += OnRunStateEvent;
            EventsLua.AttachEventLua("RESURRECT_REQUEST", e => OnEventWithArgsHandler(e));

            if (!TalentsManager._isRunning)
            {
                _talentThread.DoWork += TalentsManager.DoTalentPulse;
                _talentThread.RunWorkerAsync();
            }

            if (!_racials._isRunning)
            {
                _racialsThread.DoWork += _racials.DoRacialsPulse;
                _racialsThread.RunWorkerAsync();
            }

            if (!AIOParty._isRunning)
            {
                _partyThread.DoWork += AIOParty.DoPartyUpdatePulse;
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
        _racialsThread.DoWork -= _racials.DoRacialsPulse;
        _racialsThread.Dispose();
        _racials._isRunning = false;
        _partyThread.DoWork -= AIOParty.DoPartyUpdatePulse;
        _partyThread.Dispose();
        AIOParty._isRunning = false;

        FightEvents.OnFightLoop -= FightLoopHandler;
        FightEvents.OnFightStart -= FightStartHandler;
        FightEvents.OnFightEnd -= FightEndHandler;
        FiniteStateMachineEvents.OnRunState -= OnRunStateEvent;
        LoggingEvents.OnAddLog -= AddLogHandler;
    }

    public void ShowConfiguration() => CombatSettings?.ShowConfiguration();

    private IClassRotation ChooseRotation()
    {
        string spec = CombatSettings.Specialization;

        if (!Enums.SpecNames.ContainsKey(CombatSettings.Specialization))
        {
            Logger.LogError($"Couldn't find spec {CombatSettings.Specialization} in the dictionary");
            return null;
        }

        switch (Enums.SpecNames[CombatSettings.Specialization])
        {
            // Shaman
            case Enums.Specs.ShamanEnhancement: return new Enhancement();
            case Enums.Specs.ShamanElemental: return new Elemental();
            // Druid
            case Enums.Specs.DruidFeral: return new Feral();
            case Enums.Specs.DruidFeralDPSParty: return new FeralDPSParty();
            case Enums.Specs.DruidFeralTankParty: return new FeralTankParty();
            case Enums.Specs.DruidRestorationParty: return new RestorationParty();
            // Hunter
            case Enums.Specs.HunterBeastMaster: return new BeastMastery();
            case Enums.Specs.HunterBeastMasterParty: return new BeastMasteryParty();
            // Mage
            case Enums.Specs.MageFrost: return new Frost();
            case Enums.Specs.MageFrostParty: return new FrostParty();
            case Enums.Specs.MageArcane: return new Arcane();
            case Enums.Specs.MageArcaneParty: return new ArcaneParty();
            case Enums.Specs.MageFire: return new Fire();
            case Enums.Specs.MageFireParty: return new FireParty();
            // Paladin
            case Enums.Specs.PaladinRetribution: return new Retribution();
            // Priest
            case Enums.Specs.PriestShadow: return new Shadow();
            case Enums.Specs.PriestShadowParty: return new ShadowParty();
            case Enums.Specs.PriestHolyParty: return new HolyPriestParty();
            // Rogue
            case Enums.Specs.RogueCombat: return new Combat();
            // Warlock
            case Enums.Specs.WarlockAffliction: return new Affliction();
            case Enums.Specs.WarlockDemonology: return new Demonology();
            // Warrior
            case Enums.Specs.WarriorFury: return new Fury();
            case Enums.Specs.WarriorFuryParty: return new FuryParty();
            case Enums.Specs.WarriorProtectionParty: return new ProtectionWarrior();

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
    private void OnRunStateEvent(Engine engine, State state, CancelEventArgs cancelable)
    {
        if (engine.States.Count > 5)
            currentState = state;
    }

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

    private void OnEventWithArgsHandler(object context)
    {
        Logger.Log("Accepting resurrection request in 2000 ms");
        Thread.Sleep(2000);
        ToolBox.AcceptResurrect();
    }
}