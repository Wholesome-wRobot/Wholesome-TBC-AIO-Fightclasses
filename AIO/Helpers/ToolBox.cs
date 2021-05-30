using robotManager.Helpful;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using wManager.Wow.Class;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using wManager;
using WholesomeTBCAIO.Rotations;
using WholesomeTBCAIO.Rotations.Druid;
using WholesomeTBCAIO.Rotations.Hunter;
using static WholesomeTBCAIO.Helpers.Enums;
using WholesomeTBCAIO.Rotations.Mage;
using WholesomeTBCAIO.Rotations.Paladin;
using WholesomeTBCAIO.Rotations.Priest;
using WholesomeTBCAIO.Rotations.Rogue;
using WholesomeTBCAIO.Rotations.Shaman;
using WholesomeTBCAIO.Rotations.Warlock;
using WholesomeTBCAIO.Rotations.Warrior;
using System;
using wManager.Wow.Bot.Tasks;

namespace WholesomeTBCAIO.Helpers
{
    public class ToolBox
    {
        #region Combat

        // Pull
        public static bool Pull(Cast cast, bool alwaysPull, List<AIOSpell> spells)
        {
            AIOSpell pullSpell = spells.Find(s => s != null && s.IsSpellUsable && s.KnownSpell);
            if (pullSpell == null)
            {
                RangeManager.SetRangeToMelee();
                return false;
            }

            WoWUnit closestHostileFromTarget = GetClosestHostileFrom(ObjectManager.Target, 20);
            if (closestHostileFromTarget == null && !alwaysPull)
            {
                RangeManager.SetRangeToMelee();
                return false;
            }

            float pullRange = pullSpell.MaxRange;

            if (ObjectManager.Target.GetDistance > pullRange - 2
                || ObjectManager.Target.GetDistance < 6
                || TraceLine.TraceLineGo(ObjectManager.Target.Position))
            {
                RangeManager.SetRangeToMelee();
                return false;
            }

            if (closestHostileFromTarget != null && RangeManager.GetRange() < pullRange)
                Logger.Log($"Pulling from distance (hostile unit {closestHostileFromTarget.Name} is too close)");

            if (ObjectManager.Me.IsMounted)
                MountTask.DismountMount();

            RangeManager.SetRange(pullRange - 1);
            Thread.Sleep(300);

            if (cast.OnTarget(pullSpell))
            {
                Thread.Sleep(500);
                if (pullSpell.GetCurrentCooldown > 0)
                {
                    Usefuls.WaitIsCasting();
                    if (pullSpell.Name == "Shoot" || pullSpell.Name == "Throw" || pullSpell.Name == "Avenger's Shield")
                        Thread.Sleep(1500);
                    return true;
                }
            }

            return false;
        }

        // Accepts resurrect
        public static void AcceptResurrect()
        {
            int timer = new Random().Next(1000, 2000);
            Logger.Log($"Accepting resurrection request in {timer} ms");
            Thread.Sleep(timer);
            Lua.RunMacroText("/script AcceptResurrect(); StaticPopup1Button1: Click(\"left\", true);");
        }

        // Get Corpse Position
        public static Vector3 GetCorpsePosition(WoWCorpse corpse)
        {
            try
            {
                uint baseAddress = corpse.GetBaseAddress;
                const ushort positionOffset = 0xE8;
                float x = wManager.Wow.Memory.WowMemory.Memory.ReadFloat(baseAddress + positionOffset + 0x00);
                float y = wManager.Wow.Memory.WowMemory.Memory.ReadFloat(baseAddress + positionOffset + 0x04);
                float z = wManager.Wow.Memory.WowMemory.Memory.ReadFloat(baseAddress + positionOffset + 0x08);
                return new Vector3(x, y, z);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to read corpse position: " + e.Message);
            }
            return new Vector3(0, 0, 0);
        }

        // Check if we're currently wanding
        public static bool UsingWand()
        {
            return Lua.LuaDoString<bool>("isAutoRepeat = false; local name = GetSpellInfo(5019); " +
                "if IsAutoRepeatSpell(name) then isAutoRepeat = true end", "isAutoRepeat");
        }

        // Reactivates auto attack if it's off. Must pass the Attack spell as argument
        public static void CheckAutoAttack(AIOSpell attack)
        {
            bool _autoAttacking = Lua.LuaDoString<bool>("isAutoRepeat = false; if IsCurrentSpell('Attack') then isAutoRepeat = true end", "isAutoRepeat");
            if (!_autoAttacking && ObjectManager.Target.IsAlive)
            {
                Logger.LogDebug("Re-activating attack");
                attack.Launch();
            }
        }

        // Cancels a player buff (TBC only)
        public static void CancelPlayerBuff(string buffName)
        {
            Logger.Log($"Removing buff {buffName}");
            Lua.LuaDoString($@"CancelPlayerBuff(""{buffName.Replace("\"", "\\\"")}"")");
        }

        // Returns whether the unit is poisoned
        public static bool HasPoisonDebuff(string unit = "player")
        {
            return Lua.LuaDoString<bool>
                (@$"for i=1,25 do 
	                local _, _, _, _, d  = UnitDebuff('{unit}',i);
	                if d == 'Poison' then
                    return true
                    end
                end");
        }

        // Returns whether the unit has a disease
        public static bool HasDiseaseDebuff(string unit = "player")
        {
            return Lua.LuaDoString<bool>
                (@$"for i=1,25 do 
	                local _, _, _, _, d  = UnitDebuff('{unit}',i);
	                if d == 'Disease' then
                    return true
                    end
                end");
        }

        // Returns whether the unit has a curse
        public static bool HasCurseDebuff(string unit = "player")
        {
            return Lua.LuaDoString<bool>
                (@$"for i=1,25 do 
	                local _, _, _, _, d  = UnitDebuff('{unit}',i);
	                if d == 'Curse' then
                    return true
                    end
                end");
        }

        // Returns whether the unit has a magic debuff
        public static bool HasMagicDebuff(string unit = "player")
        {
            return Lua.LuaDoString<bool>
                (@$"for i=1,25 do 
	            local _, _, _, _, d  = UnitDebuff('{unit}',i);
	            if d == 'Magic' then
                return true
                end
            end");
        }

        // Returns the type of debuff the unit has as a string
        public static string GetDebuffType(string unit = "player")
        {
            return Lua.LuaDoString<string>
                (@$"for i=1,25 do 
	                local _, _, _, _, d  = UnitDebuff('{unit}',i);
	                if (d == 'Poison' or d == 'Magic' or d == 'Curse' or d == 'Disease') then
                    return d
                    end
                end");
        }

        // Returns whether the player has the debuff passed as a string (ex: Weakened Soul)
        public static bool HasDebuff(string debuffName, string unitName = "player")
        {
            return Lua.LuaDoString<bool>
                (@$"for i=1,25 do
                    local n, _, _, _, _  = UnitDebuff('{unitName}',i);
                    if n == '{debuffName}' then
                    return true
                    end
                end");
        }

        // Returns the amount of stacks of a specific buff passed as a string (ex: Arcane Blast)
        public static int CountBuff(string buffName, string unit = "player")
        {
            return Lua.LuaDoString<int>
                (@$"for i=1,25 do
                    local n, _, _, c, _  = UnitBuff('{unit}',i);
                    if n == '{buffName}' then
                    return c
                    end
                end");
        }

        // Returns the time left on a buff in seconds, buff name is passed as string
        public static int BuffTimeLeft(string buffName, string unit = "player")
        {
            return Lua.LuaDoString<int>
                (@$"for i=1,25 do
                    local n, _, _, _, _, duration, _  = UnitBuff('{unit}',i);
                    if n == ""{buffName}"" then
                    return duration
                    end
                end");
        }

        // Returns the time left on a debuff in seconds, debuff name is passed as string
        public static int DeBuffTimeLeft(string debuffName, string unit = "player")
        {
            return Lua.LuaDoString<int>
                (@$"for i=1,25 do
                    local n, _, _, _, _, _, expirationTime  = UnitDebuff('{unit}',i);
                    if n == '{debuffName}' then
                    return expirationTime
                    end
                end");
        }

        // Returns the amount of stacks of a specific debuff passed as a string (ex: Arcane Blast)
        public static int CountDebuff(string debuffName, string unit = "player")
        {
            return Lua.LuaDoString<int>
                (@$"for i=1,25 do
                    local n, _, _, c, _  = UnitDebuff('{unit}',i);
                    if n == '{debuffName}' then
                    return c
                    end
                end");
        }

        // Returns true if the enemy is either casting or channeling (good for interrupts)
        public static bool TargetIsCasting()
        {
            int channelTimeLeft = Lua.LuaDoString<int>($@"local spell, _, _, _, endTimeMS = UnitChannelInfo('target')
                                    if spell then
                                     local finish = endTimeMS - GetTime() * 1000
                                     return finish
                                    end");
            return channelTimeLeft < 0 || ObjectManager.Target.CastingTimeLeft > Usefuls.Latency;
        }

        public static int GetChannelTimeLeft(string unit = "target")
        {
            return Lua.LuaDoString<int>($@"local spell, _, _, startTimeMS, endTimeMS = UnitChannelInfo('{unit}')
                    if spell then
                        local finish = endTimeMS - GetTime() * 1000
                        return finish
                    end");
        }

        // Waits for GlobalCooldown to be off, must pass the most basic spell avalailable at lvl1 (ex: Smite for priest)
        public static void WaitGlobalCoolDown(Spell s)
        {
            int c = 0;
            while (!s.IsSpellUsable)
            {
                c += 50;
                Thread.Sleep(50);
                if (c >= 2000)
                    return;
            }
            Logger.LogDebug("Waited for GCD : " + c);
        }

        #endregion

        #region Misc

        public static void ClearCursor()
        {
            Lua.LuaDoString("ClearCursor();");
        }

        public static (RotationType, RotationRole) GetRotationType(IClassRotation rotation)
        {
            // DRUID
            if (rotation is Feral) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is FeralDPSParty) return (RotationType.Party, RotationRole.DPS);
            if (rotation is FeralTankParty) return (RotationType.Party, RotationRole.Tank);
            if (rotation is RestorationParty) return (RotationType.Party, RotationRole.Healer);
            // HUNTER
            if (rotation is BeastMastery) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is BeastMasteryParty) return (RotationType.Party, RotationRole.DPS);
            // MAGE
            if (rotation is Arcane) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is Fire) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is Frost) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is ArcaneParty) return (RotationType.Party, RotationRole.DPS);
            if (rotation is FireParty) return (RotationType.Party, RotationRole.DPS);
            if (rotation is FrostParty) return (RotationType.Party, RotationRole.DPS);
            // PALADIN
            if (rotation is Retribution) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is RetributionParty) return (RotationType.Party, RotationRole.DPS);
            if (rotation is PaladinHolyParty) return (RotationType.Party, RotationRole.Healer);
            if (rotation is PaladinProtectionParty) return (RotationType.Party, RotationRole.Tank);
            // PRIEST
            if (rotation is Shadow) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is ShadowParty) return (RotationType.Party, RotationRole.DPS);
            if (rotation is HolyPriestParty) return (RotationType.Party, RotationRole.Healer);
            // ROGUE
            if (rotation is Combat) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is RogueCombatParty) return (RotationType.Party, RotationRole.DPS);
            // SHAMAN
            if (rotation is Elemental) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is Enhancement) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is EnhancementParty) return (RotationType.Party, RotationRole.DPS);
            if (rotation is ShamanRestoParty) return (RotationType.Party, RotationRole.Healer);
            // WARLOCK
            if (rotation is Affliction) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is Demonology) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is AfflictionParty) return (RotationType.Party, RotationRole.DPS);
            // WARRIOR
            if (rotation is Fury) return (RotationType.Solo, RotationRole.DPS);
            if (rotation is FuryParty) return (RotationType.Party, RotationRole.DPS);
            if (rotation is ProtectionWarrior) return (RotationType.Party, RotationRole.Tank);

            Logger.LogError($"Couldn't find Rotation Type");
            return (RotationType.Solo, RotationRole.DPS);
        }

        public static void SetGroundMount(string mountName)
        {
            wManagerSetting.CurrentSetting.GroundMountName = mountName;
            wManagerSetting.CurrentSetting.Save();
            Logger.Log($"Setting mount to {mountName}");
        }

        public static List<WoWUnit> GetSuroundingEnemies()
        {
            return ObjectManager.GetObjectWoWUnit()
                .Where(e => e.IsAlive && e.IsValid && !e.PlayerControlled && e.IsAttackable)
                .OrderBy(e => e.GetDistance)
                .ToList();
        }

        public static int GetNumberEnemiesAround(float distance, WoWUnit unit)
        {
            List<WoWUnit> surroundingEnemies = ObjectManager.GetObjectWoWUnit();

            int result = 0;

            foreach (WoWUnit unitAround in surroundingEnemies)
            {
                if (unitAround.IsAlive 
                    && !unitAround.IsTapDenied 
                    && unitAround.IsValid 
                    && !unitAround.IsTaggedByOther 
                    && unitAround.IsAttackable 
                    && unitAround.Position.DistanceTo(unit.Position) < distance
                    && unitAround.Guid != ObjectManager.Target.Guid)
                {
                    result++;
                }
            }
            return result;
        }

        // Returns whether units, hostile or not, are close to the player. Distance must be passed as argument
        public static int GetNbEnemiesClose(float distance)
        {
            List<WoWUnit> surroundingEnemies = ObjectManager.GetObjectWoWUnit();
            int result = 0;
            foreach (WoWUnit unit in surroundingEnemies)
            {
                if (unit.IsAlive 
                    && !unit.IsTapDenied 
                    && unit.IsValid 
                    && !unit.IsTaggedByOther 
                    && !unit.PlayerControlled
                    && unit.IsAttackable 
                    && unit.GetDistance < distance)
                    result++;
            }
            return result;
        }

        // Returns whether hostile units are close to the target. Target and distance must be passed as argument
        public static WoWUnit GetClosestHostileFrom(WoWUnit target, float distance)
        {
            List<WoWUnit> surroundingEnemies = ObjectManager.GetObjectWoWUnit();

            foreach (WoWUnit unit in ObjectManager.GetObjectWoWUnit().Where(e => e.Position.DistanceTo(target.Position) < distance))
            {
                if (unit.IsAlive
                    && !unit.IsTapDenied
                    && unit.IsValid
                    && !unit.IsTaggedByOther
                    && !unit.PlayerControlled
                    && unit.IsAttackable
                    && unit.Reaction.ToString().Equals("Hostile")
                    && unit.Guid != target.Guid)
                    return unit;
            }

            return null;
        }

        // Get Talent Rank
        public static int GetTalentRank(int tabIndex, int talentIndex)
        {
            int rank = Lua.LuaDoString<int>($"local _, _, _, _, currentRank, _, _, _ = GetTalentInfo({tabIndex}, {talentIndex}); return currentRank;");
            return rank;
        }

        // Gets Character's specialization (talents)
        public static string GetSpec(string inspectUnitName = null)
        {
            string inspectString = inspectUnitName == null ? "false" : "true";

            int highestTalents = 0;
            Dictionary<string, int> Talents = new Dictionary<string, int>();

            if (inspectUnitName != null)
            {
                Lua.LuaDoString($"InspectUnit('{inspectUnitName}');");
                Thread.Sleep(500 + GetLatency());
                if (!AIOParty.InspectTalentReady)
                {
                    Lua.RunMacroText("/Click InspectFrameCloseButton");
                    return "retry";
                }
            }

            for (int i = 1; i <= 3; i++)
            {
                Talents.Add(
                    Lua.LuaDoString<string>($"local name, _, _ = GetTalentTabInfo('{i}', {inspectString}); return name"),
                    Lua.LuaDoString<int>($"local _, _, pointsSpent = GetTalentTabInfo('{i}', {inspectString}); return pointsSpent")
                );
            }
            highestTalents = Talents.Max(x => x.Value);
            /*
            foreach (KeyValuePair<string, int> pair in Talents)
            {
                Logger.Log($"{pair.Key} -> {pair.Value}");
            }
            */
            if (inspectUnitName != null)
            {
                Lua.RunMacroText("/Click InspectFrameCloseButton");
                AIOParty.InspectTalentReady = false;
            }

            if (highestTalents == 0)
                return null;

            return Talents.Where(t => t.Value == highestTalents).FirstOrDefault().Key;
        }

        // Returns the latency
        public static int GetLatency()
        {
            int worldLatency = Lua.LuaDoString<int>($"local down, up, lagHome, lagWorld = GetNetStats(); return lagWorld");
            int homeLatency = Lua.LuaDoString<int>($"local down, up, lagHome, lagWorld = GetNetStats(); return lagHome");
            return worldLatency + homeLatency;
        }

        #endregion

        #region Items
               
        // Add to not sell  list
        public static void AddToDoNotSellList(string itemName)
        {
            if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(itemName.Trim()))
            {
                Logger.LogDebug($"Adding item {itemName} to Do not Sell List");
                wManagerSetting.CurrentSetting.DoNotSellList.Add(itemName.Trim());
            }
        }
        public static void AddToDoNotSellList(List<string> items)
        {
            items.ForEach(item =>
            {
                if (!wManagerSetting.CurrentSetting.DoNotSellList.Contains(item.Trim()))
                {
                    Logger.LogDebug($"Adding item {item} to Do not Sell List");
                    wManagerSetting.CurrentSetting.DoNotSellList.Add(item.Trim());
                }
            });
        }

        // Return Main hand weapon type as a string
        public static string GetMHWeaponType()
        {
            return Lua.LuaDoString<string>(@"local _, _, _, _, _, _, weapontype = 
                                            GetItemInfo(GetInventoryItemLink('player', 16)); return weapontype;");
        }

        // Check if range weapon (wand, bow, gun) equipped
        public static bool HaveRangedWeaponEquipped()
        {
            return ObjectManager.Me.GetEquipedItemBySlot(wManager.Wow.Enums.InventorySlot.INVSLOT_RANGED) != 0;
        }

        // Deletes items passed as string
        public static void LuaDeleteAllItems(string item)
        {
            Lua.LuaDoString("for bag = 0, 4, 1 do for slot = 1, 32, 1 do local name = GetContainerItemLink(bag, slot); " +
                "if name and string.find(name, \"" + item + "\") then PickupContainerItem(bag, slot); " +
                "DeleteCursorItem(); end; end; end", false);
        }

        // Deletes items passed as string
        public static void LuaDeleteOneItem(string item)
        {
            Lua.LuaDoString("for bag = 0, 4, 1 do for slot = 1, 32, 1 do local name = GetContainerItemLink(bag, slot); " +
                "if name and string.find(name, \"" + item + "\") then PickupContainerItem(bag, slot); " +
                "DeleteCursorItem(); return; end; end; end", false);
        }

        // Count the amount of the specified item stacks in your bags
        public static int CountItemStacks(string itemArg)
        {
            return Lua.LuaDoString<int>("local count = GetItemCount('" + itemArg + "'); return count");
        }

        // Checks if you have any of the listed items in your bags
        public static bool HaveOneInList(List<string> list)
        {
            List<WoWItem> _bagItems = Bag.GetBagItem();
            bool _haveItem = false;
            foreach (WoWItem item in _bagItems)
            {
                if (list.Contains(item.Name))
                    _haveItem = true;
            }
            return _haveItem;
        }

        // Get item ID in bag from a list passed as argument (good to check CD)
        public static int GetItemEntry(List<string> list)
        {
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
                if (list.Contains(item.Name))
                    return item.Entry;

            return 0;
        }

        // Get item ID in bag from a string passed as argument (good to check CD)
        public static int GetItemEntry(string itemName)
        {
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
                if (itemName.Equals(item.Name))
                    return item.Entry;

            return 0;
        }

        // Get item Cooldown (must pass item string as arg)
        public static int GetItemCooldown(string itemName)
        {
            int entry = GetItemEntry(itemName);
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
                if (entry == item.Entry)
                    return Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + entry + "); " +
                        "return duration - (GetTime() - startTime)");

            Logger.Log("Couldn't find item " + itemName);
            return 0;
        }

        // Get item Cooldown from list (must pass item list as arg)
        public static int GetItemCooldown(List<string> itemList)
        {
            int entry = GetItemEntry(itemList);
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
                if (entry == item.Entry)
                    return Lua.LuaDoString<int>("local startTime, duration, enable = GetItemCooldown(" + entry + "); " +
                        "return duration - (GetTime() - startTime)");

            Logger.Log("Couldn't find item");
            return 0;
        }

        // Uses the first item found in your bags that matches any element from the list
        public static void UseFirstMatchingItem(List<string> list)
        {
            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
            {
                if (list.Contains(item.Name))
                {
                    ItemsManager.UseItemByNameOrId(item.Name);
                    Logger.Log("Using " + item.Name);
                    return;
                }
            }
        }

        // Returns the item found in your bags that matches the latest element from the list
        public static string GetBestMatchingItem(List<string> list)
        {
            string _bestItem = null;
            int index = 0;

            List<WoWItem> _bagItems = Bag.GetBagItem();
            foreach (WoWItem item in _bagItems)
            {
                if (list.Contains(item.Name))
                {
                    int itemIndex = list.IndexOf(item.Name);
                    if (itemIndex >= index)
                        _bestItem = item.Name;
                }
            }
            return _bestItem;
        }

        #endregion

        #region Pet

        // Returns wether your pet knows the skill
        public static bool PetKnowsSpell(string spellName)
        {
            bool knowsSpell = false;
            knowsSpell = Lua.LuaDoString<bool>
                ($"for i=1,10 do " +
                    "local name, _, _, _, _, _, _ = GetPetActionInfo(i); " +
                    "if name == '" + spellName + "' then " +
                    "return true " +
                    "end " +
                "end");

            return knowsSpell;
        }
        /*
        // Casts pet dmg spell if he has over X focus
        public static void CastPetSpellIfEnoughForGrowl(string spellName, uint spellCost)
        {
            if (ObjectManager.Pet.Focus >= spellCost + 15
                && ObjectManager.Pet.HasTarget
                && ObjectManager.Me.InCombatFlagOnly
                && PetKnowsSpell(spellName))
                PetSpellCast(spellName);
        }
        */
        // Returns the index of the pet spell passed as argument
        public static int GetPetSpellIndex(string spellName)
        {
            int spellindex = Lua.LuaDoString<int>
                ($"for i=1,10 do " +
                    "local name, _, _, _, _, _, _ = GetPetActionInfo(i); " +
                    "if name == '" + spellName + "' then " +
                    "return i " +
                    "end " +
                "end");

            return spellindex;
        }

        // Returns the cooldown of the pet spell passed as argument
        public static int GetPetSpellCooldown(string spellName)
        {
            int _spellIndex = GetPetSpellIndex(spellName);
            return Lua.LuaDoString<int>("local startTime, duration, enable = GetPetActionCooldown(" + _spellIndex + "); return duration - (GetTime() - startTime)");
        }

        // Returns whether a pet spell is available (off cooldown)
        public static bool GetPetSpellReady(string spellName)
        {
            return GetPetSpellCooldown(spellName) < 0;
        }
        /*
        // Casts the pet spell passed as argument
        public static void PetSpellCast(string spellName)
        {
            int spellIndex = GetPetSpellIndex(spellName);
            if (PetKnowsSpell(spellName)
                && GetPetSpellReady(spellName))
            {
                Thread.Sleep(GetLatency() + 100);
                Lua.LuaDoString("CastPetAction(" + spellIndex + ");");
            }
        }
        */
        // Toggles Pet spell autocast (pass true as second argument to toggle on, or false to toggle off)
        public static void TogglePetSpellAuto(string spellName, bool toggle)
        {
            if (PetKnowsSpell(spellName))
            {
                string spellIndex = GetPetSpellIndex(spellName).ToString();

                if (!spellIndex.Equals("0"))
                {
                    if ((toggle && !PetSpellIsAutocast(spellName)) || (!toggle && PetSpellIsAutocast(spellName)))
                    {
                        //Lua.LuaDoString("ToggleSpellAutocast(" + spellIndex + ", 'pet');");
                        Lua.LuaDoString("ToggleSpellAutocast('" + spellName + "', 'pet');");
                    }
                }
            }
        }

        public static bool PetSpellIsAutocast(string spellName)
        {
            if (PetKnowsSpell(spellName))
            {
                string spellIndex = GetPetSpellIndex(spellName).ToString();

                if (!spellIndex.Equals("0"))
                {
                    // Lua.LuaDoString<bool>("local _, autostate = GetSpellAutocast(" + spellIndex + ", 'pet'); return autostate == 1")
                    return Lua.LuaDoString<bool>("local _, autostate = GetSpellAutocast('" + spellName + "', 'pet'); return autostate == 1");
                }
            }
            return false;
        }

        #endregion

        #region Movement

        // get the position behind the target
        public static Vector3 BackofVector3(Vector3 from, WoWUnit targetObject, float radius)
        {
            if (from != null && from != Vector3.Empty)
            {
                float rotation = -robotManager.Helpful.Math.DegreeToRadian(robotManager.Helpful.Math.RadianToDegree(targetObject.Rotation) + 90);
                return new Vector3((System.Math.Sin(rotation) * radius) + from.X, (System.Math.Cos(rotation) * radius) + from.Y, from.Z);
            }
            return new Vector3(0, 0, 0);
        }

        // Determines if me is behind the Target
        public static bool MeBehindTarget()
        {
            float Pi = (float)System.Math.PI;
            bool backLeft = false;
            bool backRight = false;
            float target_x = ObjectManager.Target.Position.X;
            float target_y = ObjectManager.Target.Position.Y;
            float target_r = ObjectManager.Target.Rotation;
            float player_x = ObjectManager.Me.Position.X;
            float player_y = ObjectManager.Me.Position.Y;
            float d = (float)System.Math.Atan2((target_y - player_y), (target_x - player_x));
            float r = d - target_r;

            if (r < 0) r = r + (Pi * 2);
            if (r > 1.5 * Pi) backLeft = true;
            if (r < 0.5 * Pi) backRight = true;
            if (backLeft || backRight) return true; else return false;
        }

        // Move behind Target
        public static bool StandBehindTargetCombat()
        {
            if (ObjectManager.Me.IsAlive
                && !ObjectManager.Me.IsCast
                && ObjectManager.Target.IsAlive
                && ObjectManager.Target.HasTarget
                && !ObjectManager.Target.IsTargetingMe
                && !MovementManager.InMovement)
            {
                int limit = 5;
                Vector3 position = BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2f);
                while (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause
                    && ObjectManager.Me.Position.DistanceTo(position) > 1
                    && limit >= 0)
                {
                    position = BackofVector3(ObjectManager.Target.Position, ObjectManager.Target, 2f);
                    MovementManager.Go(PathFinder.FindPath(position), false);
                    // Wait follow path
                    Thread.Sleep(500);
                    limit--;
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}