using robotManager.Helpful;
using System.Collections.Generic;
using WholesomeTBCAIO.Helpers;
using WholesomeTBCAIO.Managers.PartyManager;
using WholesomeTBCAIO.Managers.UnitCache;
using WholesomeTBCAIO.Managers.UnitCache.Entities;
using WholesomeTBCAIO.Settings;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

namespace WholesomeTBCAIO.Rotations.Shaman
{
    public class TotemManager
    {
        private WoWLocalPlayer Me = ObjectManager.Me;
        private Vector3 _lastTotemPosition = null;
        private Vector3 _fireTotemPosition = null;
        private Cast _cast;
        private ShamanSettings _settings;
        private IPartyManager _partyManager;
        private IUnitCache _unitCache;

        private AIOSpell TotemicCall = new AIOSpell("Totemic Call");

        // Air
        private AIOSpell GraceOfAirTotem = new AIOSpell("Grace of Air Totem");
        private AIOSpell WindfuryTotem = new AIOSpell("Windfury Totem");
        private AIOSpell WrathOfAirTotem = new AIOSpell("Wrath of Air Totem");

        // Fire
        private AIOSpell SearingTotem = new AIOSpell("Searing Totem");
        private AIOSpell MagmaTotem = new AIOSpell("Magma Totem");
        private AIOSpell TotemOfWrath = new AIOSpell("Totem of Wrath");

        // Earth
        private AIOSpell StoneskinTotem = new AIOSpell("Stoneskin Totem");
        private AIOSpell StoneclawTotem = new AIOSpell("Stoneclaw Totem");
        private AIOSpell StrengthOfEarthTotem = new AIOSpell("Strength of Earth Totem");
        private AIOSpell EarthElementalTotem = new AIOSpell("Earth Elemental Totem");
        private AIOSpell TremorTotem = new AIOSpell("Tremor Totem");

        // Water
        private AIOSpell FireResistanceTotem = new AIOSpell("Fire Resistance Totem");
        private AIOSpell ManaSpringTotem = new AIOSpell("Mana Spring Totem");
        private AIOSpell ManaTideTotem = new AIOSpell("Mana Tide Totem");

        public TotemManager(Cast cast, ShamanSettings settings, IPartyManager partyManager, IUnitCache unitCache)
        {
            _cast = cast;
            _settings = settings;
            _partyManager = partyManager;
            _unitCache = unitCache;
        }

        public bool CastTotems(IClassRotation spec)
        {
            if (CastWaterTotem(spec))
                return true;
            if (CastEarthTotem(spec))
                return true;
            if (CastFireTotem(spec))
                return true;
            if (CastAirTotem(spec))
                return true;
            return false;
        }

        public void CheckForTotemicCall()
        {
            if (_settings.UseTotemicCall)
            {
                bool haveFireTotem = Lua.LuaDoString<string>(@"local _, totemName, _, _ = GetTotemInfo(1); return totemName;").Contains("Totem");
                bool haveEarthTotem = Lua.LuaDoString<string>(@"local _, totemName, _, _ = GetTotemInfo(2); return totemName;").Contains("Totem");
                bool haveWaterTotem = Lua.LuaDoString<string>(@"local _, totemName, _, _ = GetTotemInfo(3); return totemName;").Contains("Totem");
                bool haveWindTotem = Lua.LuaDoString<string>(@"local _, totemName, _, _ = GetTotemInfo(4); return totemName;").Contains("Totem");
                bool haveTotem = haveEarthTotem || haveFireTotem || haveWaterTotem || haveWindTotem;

                if (_lastTotemPosition != null
                    && haveTotem
                    && _lastTotemPosition.DistanceTo(Me.Position) > 17
                    && !_unitCache.Me.HasAura("GhostWolf")
                    && !Me.IsMounted
                    && !Me.IsCast
                    && Cast(TotemicCall))
                    return;
            }
        }

        private bool CastEarthTotem(IClassRotation spec)
        {
            string currentEarthTotem = Lua.LuaDoString<string>
                (@"local haveTotem, totemName, startTime, duration = GetTotemInfo(2); return totemName;");

            if (spec.RotationType == Enums.RotationType.Solo)
            {
                // Earth Elemental Totem on multiaggro
                if (_unitCache.EnemiesAttackingMe.Count > 1
                    && EarthElementalTotem.KnownSpell
                    && !currentEarthTotem.Contains("Stoneclaw Totem")
                    && !currentEarthTotem.Contains("Earth Elemental Totem")
                    && Cast(EarthElementalTotem))
                    return true;

                // Stoneclaw on multiaggro
                if (_unitCache.EnemiesAttackingMe.Count > 1
                    && StoneclawTotem.KnownSpell
                    && !currentEarthTotem.Contains("Stoneclaw Totem")
                    && !currentEarthTotem.Contains("Earth Elemental Totem")
                    && Cast(StoneclawTotem))
                    return true;
            }

            if (_settings.UseEarthTotems)
            {
                switch (_settings.TotemPreset)
                {
                    case "Melee":
                        if (!currentEarthTotem.Contains("Strength of Earth") && Cast(StrengthOfEarthTotem))
                            return true;
                        break;
                    case "Caster":
                        if (!currentEarthTotem.Contains("Tremor Totem") && Cast(TremorTotem))
                            return true;
                        break;
                    default:
                        // Strenght of Earth totem
                        if ((spec is Enhancement || spec is EnhancementParty || spec is ShamanRestoParty)
                            && (!_settings.UseStoneSkinTotem || !StoneskinTotem.KnownSpell)
                            && !Me.HaveBuff("Strength of Earth")
                            && !currentEarthTotem.Contains("Stoneclaw Totem")
                            && !currentEarthTotem.Contains("Earth Elemental Totem")
                            && (_unitCache.EnemiesAttackingMe.Count < 2 || spec.RotationType == Enums.RotationType.Party)
                            && Cast(StrengthOfEarthTotem))
                            return true;
                        break;
                }
                
                // Stoneskin Totem
                if ((_settings.UseStoneSkinTotem || !StrengthOfEarthTotem.KnownSpell || spec is Elemental || (spec.RotationType == Enums.RotationType.Solo && _unitCache.EnemiesAttackingMe.Count > 1))
                    && !Me.HaveBuff("Stoneskin")
                    && !currentEarthTotem.Contains("Stoneclaw Totem")
                    && !currentEarthTotem.Contains("Earth Elemental Totem")
                    && Cast(StoneskinTotem))
                    return true;

                // Tremor Totem
                if (_settings.UseTremorTotem)
                {
                    if (!currentEarthTotem.Contains("Tremor Totem") && Cast(TremorTotem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CastFireTotem(IClassRotation spec)
        {
            if (_settings.UseFireTotems)
            {
                string currentFireTotem = Lua.LuaDoString<string>
                    (@"local haveTotem, totemName, startTime, duration = GetTotemInfo(1); return totemName;");

                // Magma Totem
                if (_settings.UseMagmaTotem 
                    && _unitCache.EnemiesAttackingMe.Count > 1
                    && Me.ManaPercentage > 50
                    && ObjectManager.Target.GetDistance < 10
                    && !currentFireTotem.Contains("Magma Totem")
                    && Cast(MagmaTotem))
                    return true;

                // Searing Totem
                if ((!currentFireTotem.Contains("Searing Totem") || _fireTotemPosition == null || Me.Position.DistanceTo(_fireTotemPosition) > 15f)
                    && ObjectManager.Target.GetDistance < 15
                    && !currentFireTotem.Contains("Magma Totem")
                    && (!_settings.UseTotemOfWrath || !TotemOfWrath.KnownSpell)
                    && Cast(SearingTotem))
                {
                    _fireTotemPosition = Me.Position;
                    return true;
                }

                // Totem of Wrath
                if (!currentFireTotem.Contains("Totem of Wrath")
                    && !Me.HaveBuff("Totem of Wrath")
                    && _settings.UseTotemOfWrath
                    && Cast(TotemOfWrath))
                    return true;

            }

            return false;
        }

        private bool CastAirTotem(IClassRotation spec)
        {
            if (_settings.UseAirTotems)
            {
                string currentAirTotem = Lua.LuaDoString<string>
                    (@"local _, totemName, _, _ = GetTotemInfo(4); return totemName;");


                switch (_settings.TotemPreset)
                {
                    case "Melee":
                        if (!currentAirTotem.Contains("Windfury") && Cast(WindfuryTotem))
                            return true;
                        break;
                    case "Caster":
                        if (!currentAirTotem.Contains("Wrath of Air") && Cast(WrathOfAirTotem))
                            return true;
                        break;
                    default:
                        // Grace of Air Totem
                        if (!Me.HaveBuff("Grace of Air")
                            && Cast(GraceOfAirTotem))
                            return true;
                        break;
                }
            }

            return false;
        }

        private bool CastWaterTotem(IClassRotation spec)
        {
            if (_settings.UseWaterTotems)
            {
                string currentWaterTotem = Lua.LuaDoString<string>
                    (@"local _, totemName, _, _ = GetTotemInfo(3); return totemName;");

                // Mana Tide Totem
                if (ManaTideTotem.KnownSpell && !currentWaterTotem.Contains("Mana Tide"))
                {
                    if (_unitCache.GroupAndRaid.Count > 5)
                    {
                        // Raid
                        if (Me.ManaPercentage < 40 && Cast(ManaTideTotem))
                        {
                            return true;
                        }
                    } else {
                        //Group
                        List<IWoWPlayer> alliesNeedingMana = _unitCache.GroupAndRaid
                        .FindAll(a => a.ManaPercentage < 20);
                        if ((alliesNeedingMana.Count > 1 || Me.ManaPercentage < 10)
                            && Cast(ManaTideTotem))
                            return true;
                    }
                }

                // Mana Spring Totem
                if (!currentWaterTotem.Contains("Mana Tide")
                    && !Me.HaveBuff("Mana Spring")
                    && Cast(ManaSpringTotem))
                    return true;
            }

            return false;
        }

        private bool Cast(AIOSpell spell)
        {
            if (_cast.OnSelf(spell))
            {
                if (spell.Name.Contains(" Totem") || spell.Name.Contains("Totem of"))
                    _lastTotemPosition = Me.Position;

                return true;
            }

            return false;
        }
    }
}