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

public class Main : ICustomClass
{
    private static readonly BackgroundWorker _talentThread = new BackgroundWorker();
    private static readonly BackgroundWorker _racialsThread = new BackgroundWorker();
    private Racials _racials = new Racials();

    public static string wowClass = ObjectManager.Me.WowClass.ToString();
    public static int humanReflexTime = 500;
    public static bool isLaunched; 
    public static string version = "2.1.52"; // Must match version in Version.txt
    public static bool HMPrunningAway = false;

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
            robotManager.Events.LoggingEvents.OnAddLog += AddLogHandler;

            if (!Talents._isRunning)
            {
                _talentThread.DoWork += Talents.DoTalentPulse;
                _talentThread.RunWorkerAsync();
            }

            if(!_racials._isRunning)
            {
                _racialsThread.DoWork += _racials.DoRacialsPulse;
                _racialsThread.RunWorkerAsync();
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

        _talentThread.DoWork -= Talents.DoTalentPulse;
        _talentThread.Dispose();
        Talents._isRunning = false;
        _racialsThread.DoWork -= _racials.DoRacialsPulse;
        _racialsThread.Dispose();
        _racials._isRunning = false;

        FightEvents.OnFightLoop -= FightLoopHandler;
        FightEvents.OnFightStart -= FightStartHandler;
        FightEvents.OnFightEnd -= FightEndHandler;
        robotManager.Events.LoggingEvents.OnAddLog -= AddLogHandler;
    }

    public void ShowConfiguration() => CombatSettings?.ShowConfiguration();

    private IClassRotation ChooseRotation()
    {
        string spec = CombatSettings.Specialization;
        switch (spec)
        {
            case "Enhancement": return new Enhancement();
            case "Elemental": return new Elemental();
            case "Feral": return new Feral();
            case "BeastMaster": return new BeastMastery();
            case "Frost": return new Frost();
            case "Arcane": return new Arcane();
            case "Fire": return new Fire();
            case "Retribution": return new Retribution();
            case "Shadow": return new Shadow();
            case "Combat": return new Combat();
            case "Affliction": return new Affliction();
            case "Demonology": return new Demonology();
            case "Fury": return new Fury();
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
}