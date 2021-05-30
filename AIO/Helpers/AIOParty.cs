using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;

namespace WholesomeTBCAIO.Helpers
{
    public class AIOParty
    {
        public static bool _isRunning;
        public static List<AIOPartyMember> Group { get; private set; } = new List<AIOPartyMember>();
        public static List<WoWUnit> AllUnits { get; private set; } = new List<WoWUnit>();
        public static List<WoWUnit> EnemiesClose { get; private set; } = new List<WoWUnit>();
        public static bool ActivateSpecRecord { get; set; }
        public static bool InspectTalentReady { get; set; } = true;
        private static Dictionary<string, string> PartySpecsCache { get; set; } = new Dictionary<string, string>();

        public static void DoPartyUpdatePulse(object sender, DoWorkEventArgs args)
        {
            _isRunning = true;
            while (Main.isLaunched && _isRunning)
            {
                try
                {
                    if (StatusChecker.BasicConditions())
                    {
                        bool changed = false;

                        if (Group.Exists(p => p.Name == ""))
                            Group.Clear();

                        List<WoWPlayer> allMembersList = new List<WoWPlayer>();
                        allMembersList.AddRange(Party.GetRaidMembers());
                        allMembersList.AddRange(Party.GetParty());

                        // Add players to my own group list
                        foreach (WoWPlayer player in allMembersList)
                        {
                            if (!Group.Exists(m => m.Guid == player.Guid))
                            {
                                Group.Add(new AIOPartyMember(player.GetBaseAddress));
                                //Logger.Log($"Added {player.Name} to party");
                                changed = true;
                            }
                        }

                        // Add me
                        if (!Group.Exists(m => m.Guid == ObjectManager.Me.Guid))
                        {
                            //Logger.Log($"Added MYSELF to raid/party");
                            Group.Add(new AIOPartyMember(ObjectManager.Me.GetBaseAddress));
                            changed = true;
                        }

                        // Remove players
                        for (int i = Group.Count - 1; i >= 0; i--)
                        {
                            if (Group[i].Guid != ObjectManager.Me.Guid && !allMembersList.Exists(m => m.Guid == Group[i].Guid))
                            {
                                //Logger.Log($"Removing {Group[i].Name} from party");
                                Group.Remove(Group[i]);
                                changed = true;
                            }
                        }

                        if (changed && Group.Count > 1)
                        {
                            string logMessage = "Party detected [";
                            Group.ForEach(m => logMessage += m.Name + "-");
                            logMessage = logMessage.Remove(logMessage.Length - 1);
                            logMessage += "]";
                            Logger.Log(logMessage);
                        }

                        AllUnits = ObjectManager.GetObjectWoWUnit();
                        EnemiesClose = AllUnits.FindAll(e => e.GetDistance < 50);

                        if (ActivateSpecRecord)
                            RecordPartySpecs();
                    }
                }
                catch (Exception arg)
                {
                    Logger.LogError("AIOParty -> " + string.Concat(arg));
                }
                Thread.Sleep(5000);
            }
            _isRunning = false;
        }

        public static List<WoWUnit> EnemiesFighting => EnemiesClose
            .FindAll(e => e.InCombatFlagOnly && e.IsTargetingMeOrMyPetOrPartyMember);

        public static void SwitchTarget(Cast cast, AIOSpell spell)
        {
            if ((ObjectManager.Target.Target == ObjectManager.Me.Guid
                || !ObjectManager.Target.IsAlive
                || !ObjectManager.Target.HasTarget
                || !ObjectManager.Me.HasTarget)
                && !ToolBox.HasDebuff("Taunt", "target")
                && !ToolBox.HasDebuff("Growl", "target"))
            {
                foreach (WoWUnit enemy in EnemiesFighting)
                {
                    WoWPlayer partyMemberToSave = Group.Find(m => enemy.Target == m.Guid && (m.Guid != ObjectManager.Me.Guid || !ObjectManager.Me.HasTarget));

                    if (partyMemberToSave != null)
                    {
                        Logger.Log($"Regaining aggro [{enemy.Name} attacking {partyMemberToSave.Name}]");
                        ObjectManager.Me.Target = enemy.Guid;

                        if (spell != null)
                        {
                            if (spell.Name == "Righteous Defense")
                                cast.OnFocusUnit(spell, partyMemberToSave);
                            if (spell.Name == "Intervene" && enemy.Position.DistanceTo(partyMemberToSave.Position) < 10)
                                cast.OnTarget(spell);
                        }
                        return;
                    }
                }
            }
        }

        public static void RecordPartySpecs()
        {
            foreach (AIOPartyMember p in Group)
            {
                if (p.Guid != ObjectManager.Me.Guid
                    && p.GetDistance < 25
                    && p.IsAlive
                    && p.IsValid
                    && p.Specialization == null)
                {
                    if (PartySpecsCache.ContainsKey(p.Name))
                    {
                        p.Specialization = PartySpecsCache[p.Name];
                        continue;
                    }

                    string spec = ToolBox.GetSpec(p.Name);

                    if (spec == null)
                    {
                        Logger.Log($"Couldn't record {p.Name}'s specialization");
                        p.Specialization = null;
                        break;
                    }

                    if (spec == "retry")
                        break;

                    p.Specialization = spec;
                    PartySpecsCache.Add(p.Name, spec);
                    Logger.Log($"{p.Name}'s specialization is {p.Specialization}");
                    break;
                }
            }
        }

        // Party Drink
        public static bool PartyDrink(string drinkName, int threshold)
        {
            Timer wait = new Timer(2000);
            while (!wait.IsReady && !ObjectManager.Me.InCombatFlagOnly && !Fight.InFight)
                Thread.Sleep(300);

            if (ObjectManager.Me.ManaPercentage < threshold
                && !ObjectManager.Me.HaveBuff("Drink")
                && !MovementManager.InMovement
                && !MovementManager.InMoveTo
                && drinkName.Trim().Length > 0)
            {
                ToolBox.AddToDoNotSellList(drinkName.Trim());
                if (ToolBox.CountItemStacks(drinkName) > 0)
                {
                    ItemsManager.UseItemByNameOrId(drinkName);
                    Logger.Log($"[Party drink] Using {drinkName}");
                    return true;
                }
                else
                {
                    Logger.Log($"Couldn't find any {drinkName} in bags");
                    return false;
                }
            }
            return false;
        }

        public static void InspectTalentReadyHeandler()
        {
            InspectTalentReady = true;
        }
    }
}
