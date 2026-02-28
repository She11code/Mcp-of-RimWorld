using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace RimWorldAI.Core
{
    /// <summary>
    /// 游戏状态提供者 - 基于 MapPawns 真实 API
    /// </summary>
    public static class GameStateProvider
    {
        #region 地图

        /// <summary>
        /// 获取当前地图
        /// </summary>
        public static Map GetCurrentMap()
        {
            return Find.CurrentMap;
        }

        /// <summary>
        /// 获取所有地图
        /// </summary>
        public static List<Map> GetAllMaps()
        {
            return Find.Maps;
        }

        #endregion

        #region 时间和天气 - 基于 GenDate, GenCelestial, WeatherManager API

        /// <summary>
        /// 获取时间和昼夜信息
        /// 基于 GenDate 和 GenCelestial API
        /// </summary>
        public static Dictionary<string, object> GetTimeInfo(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No current map available"
                };
            }

            int ticksAbs = Find.TickManager.TicksAbs;
            Vector2 longLat = Find.WorldGrid.LongLatOf(map.Tile);

            float sunGlow = GenCelestial.CurCelestialSunGlow(map);
            bool isDaytime = GenCelestial.IsDaytime(sunGlow);

            return new Dictionary<string, object>
            {
                ["tick"] = Find.TickManager.TicksGame,
                ["hour"] = GenDate.HourOfDay(ticksAbs, longLat.x),
                ["isDaytime"] = isDaytime,
                ["sunGlow"] = sunGlow,
                ["dayOfSeason"] = GenDate.DayOfSeason(ticksAbs, longLat.x),
                ["season"] = GenDate.Season(ticksAbs, longLat.y, longLat.x).ToString(),
                ["quadrum"] = GenDate.Quadrum(ticksAbs, longLat.x).ToString(),
                ["year"] = GenDate.Year(ticksAbs, longLat.x),
                ["daysPassed"] = GenDate.DaysPassed
            };
        }

        /// <summary>
        /// 获取天气信息
        /// 基于 WeatherManager 和 WeatherDef API
        /// </summary>
        public static Dictionary<string, object> GetWeatherInfo(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No current map available"
                };
            }

            var weatherManager = map.weatherManager;

            return new Dictionary<string, object>
            {
                ["current"] = new Dictionary<string, object>
                {
                    ["defName"] = weatherManager.curWeather.defName,
                    ["label"] = weatherManager.curWeather.label ?? "",
                    ["description"] = weatherManager.curWeather.description ?? ""
                },
                ["last"] = new Dictionary<string, object>
                {
                    ["defName"] = weatherManager.lastWeather.defName,
                    ["label"] = weatherManager.lastWeather.label ?? ""
                },
                ["rainRate"] = weatherManager.RainRate,
                ["snowRate"] = weatherManager.SnowRate,
                ["windSpeedFactor"] = weatherManager.CurWindSpeedFactor,
                ["moveSpeedMultiplier"] = weatherManager.CurMoveSpeedMultiplier,
                ["accuracyMultiplier"] = weatherManager.CurWeatherAccuracyMultiplier,
                ["isBadWeather"] = weatherManager.curWeather.isBad
            };
        }

        #endregion

        #region Pawn - 基于 MapPawns API

        /// <summary>
        /// 获取所有已生成的 Pawn
        /// 对应: map.mapPawns.AllPawnsSpawned
        /// </summary>
        public static IReadOnlyList<Pawn> GetAllPawnsSpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.AllPawnsSpawned;
        }

        /// <summary>
        /// 获取所有 Pawn (包括未生成的)
        /// 对应: map.mapPawns.AllPawns
        /// </summary>
        public static List<Pawn> GetAllPawns(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.AllPawns;
        }

        /// <summary>
        /// 获取自由殖民者 (包括未生成的)
        /// 对应: map.mapPawns.FreeColonists
        /// </summary>
        public static List<Pawn> GetFreeColonists(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.FreeColonists;
        }

        /// <summary>
        /// 获取已生成的自由殖民者
        /// 对应: map.mapPawns.FreeColonistsSpawned
        /// </summary>
        public static List<Pawn> GetFreeColonistsSpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.FreeColonistsSpawned;
        }

        /// <summary>
        /// 获取成年殖民者
        /// 对应: map.mapPawns.FreeAdultColonistsSpawned
        /// </summary>
        public static List<Pawn> GetFreeAdultColonistsSpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.FreeAdultColonistsSpawned;
        }

        /// <summary>
        /// 获取囚犯
        /// 对应: map.mapPawns.PrisonersOfColony
        /// </summary>
        public static List<Pawn> GetPrisonersOfColony(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.PrisonersOfColony;
        }

        /// <summary>
        /// 获取已生成的囚犯
        /// 对应: map.mapPawns.PrisonersOfColonySpawned
        /// </summary>
        public static List<Pawn> GetPrisonersOfColonySpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.PrisonersOfColonySpawned;
        }

        /// <summary>
        /// 获取奴隶
        /// 对应: map.mapPawns.SlavesOfColonySpawned
        /// </summary>
        public static List<Pawn> GetSlavesOfColonySpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.SlavesOfColonySpawned;
        }

        /// <summary>
        /// 获取殖民者和囚犯
        /// 对应: map.mapPawns.FreeColonistsAndPrisoners
        /// </summary>
        public static List<Pawn> GetFreeColonistsAndPrisoners(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.FreeColonistsAndPrisoners;
        }

        /// <summary>
        /// 获取已生成的殖民者和囚犯
        /// 对应: map.mapPawns.FreeColonistsAndPrisonersSpawned
        /// </summary>
        public static List<Pawn> GetFreeColonistsAndPrisonersSpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.FreeColonistsAndPrisonersSpawned;
        }

        /// <summary>
        /// 获取所有人形生物
        /// 对应: map.mapPawns.AllHumanlike
        /// </summary>
        public static List<Pawn> GetAllHumanlike(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.AllHumanlike;
        }

        /// <summary>
        /// 获取已生成的人形生物
        /// 对应: map.mapPawns.AllHumanlikeSpawned
        /// </summary>
        public static List<Pawn> GetAllHumanlikeSpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.AllHumanlikeSpawned;
        }

        /// <summary>
        /// 获取殖民地动物
        /// 对应: map.mapPawns.ColonyAnimals
        /// </summary>
        public static List<Pawn> GetColonyAnimals(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.ColonyAnimals;
        }

        /// <summary>
        /// 获取已生成的殖民地动物
        /// 对应: map.mapPawns.SpawnedColonyAnimals
        /// </summary>
        public static List<Pawn> GetSpawnedColonyAnimals(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.SpawnedColonyAnimals;
        }

        /// <summary>
        /// 获取已生成的殖民地机甲
        /// 对应: map.mapPawns.SpawnedColonyMechs
        /// </summary>
        public static List<Pawn> GetSpawnedColonyMechs(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.SpawnedColonyMechs;
        }

        /// <summary>
        /// 获取倒地的 Pawn
        /// 对应: map.mapPawns.SpawnedDownedPawns
        /// </summary>
        public static List<Pawn> GetSpawnedDownedPawns(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.SpawnedDownedPawns;
        }

        /// <summary>
        /// 获取饥饿的 Pawn
        /// 对应: map.mapPawns.SpawnedHungryPawns
        /// </summary>
        public static List<Pawn> GetSpawnedHungryPawns(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.SpawnedHungryPawns;
        }

        /// <summary>
        /// 获取某阵营的所有 Pawn
        /// 对应: map.mapPawns.PawnsInFaction(faction)
        /// </summary>
        public static List<Pawn> GetPawnsInFaction(Faction faction, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.PawnsInFaction(faction);
        }

        /// <summary>
        /// 获取某阵营已生成的 Pawn
        /// 对应: map.mapPawns.SpawnedPawnsInFaction(faction)
        /// </summary>
        public static List<Pawn> GetSpawnedPawnsInFaction(Faction faction, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.SpawnedPawnsInFaction(faction);
        }

        /// <summary>
        /// 获取敌人 (敌对阵营的 Pawn)
        /// </summary>
        public static List<Pawn> GetEnemies(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<Pawn>();

            return map.mapPawns.AllPawnsSpawned
                .Where(p => p.HostileTo(Faction.OfPlayer))
                .ToList();
        }

        #endregion

        #region 计数属性

        /// <summary>
        /// 殖民者数量
        /// 对应: map.mapPawns.ColonistCount
        /// </summary>
        public static int GetColonistCount(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.ColonistCount ?? 0;
        }

        /// <summary>
        /// 已生成的 Pawn 数量
        /// 对应: map.mapPawns.AllPawnsSpawnedCount
        /// </summary>
        public static int GetAllPawnsSpawnedCount(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.AllPawnsSpawnedCount ?? 0;
        }

        /// <summary>
        /// 自由殖民者数量
        /// 对应: map.mapPawns.FreeColonistsCount
        /// </summary>
        public static int GetFreeColonistsCount(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.FreeColonistsCount ?? 0;
        }

        /// <summary>
        /// 是否有殖民者
        /// 对应: map.mapPawns.AnyColonistSpawned
        /// </summary>
        public static bool HasAnyColonistSpawned(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.mapPawns?.AnyColonistSpawned ?? false;
        }

        #endregion

        #region Pawn 详细信息

        /// <summary>
        /// 获取 Pawn 的详细信息（包含技能、特质、心情、健康等完整信息）
        /// </summary>
        public static Dictionary<string, object> GetPawnInfo(Pawn pawn)
        {
            if (pawn == null) return null;

            var result = new Dictionary<string, object>
            {
                // ========== 基础信息 ==========
                ["id"] = pawn.thingIDNumber,
                ["name"] = pawn.LabelShort,
                ["position"] = new { x = pawn.Position.x, z = pawn.Position.z },
                ["faction"] = pawn.Faction?.Name ?? "None",
                ["gender"] = pawn.gender.ToString(),
                ["biologicalAge"] = pawn.ageTracker?.AgeBiologicalYears ?? 0,
                ["chronologicalAge"] = pawn.ageTracker?.AgeChronologicalYears ?? 0,

                // ========== 状态标志 ==========
                ["isColonist"] = pawn.IsColonist,
                ["isPrisoner"] = pawn.IsPrisoner,
                ["isSlave"] = pawn.IsSlave,
                ["isAnimal"] = pawn.RaceProps?.Animal ?? false,
                ["isHumanlike"] = pawn.RaceProps?.Humanlike ?? false,
                ["isDowned"] = pawn.Downed,
                ["isAsleep"] = !pawn.Awake(),
                ["isDead"] = pawn.Dead,
                ["curJob"] = pawn.CurJob?.def?.defName ?? "None",

                // ========== 健康 ==========
                ["healthPercent"] = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,
            };

            // ========== 技能信息 ==========
            if (pawn.skills != null && pawn.skills.skills != null)
            {
                var skillsList = new List<Dictionary<string, object>>();
                foreach (var skill in pawn.skills.skills)
                {
                    if (skill != null && skill.def != null)
                    {
                        skillsList.Add(new Dictionary<string, object>
                        {
                            ["defName"] = skill.def.defName,
                            ["label"] = skill.def.label ?? skill.def.defName,
                            ["level"] = skill.Level,
                            ["xpSinceLastLevel"] = skill.xpSinceLastLevel,
                            ["passion"] = skill.passion.ToString() // None, Minor, Major
                        });
                    }
                }
                result["skills"] = skillsList;
            }

            // ========== 特质信息 ==========
            if (pawn.story?.traits?.allTraits != null)
            {
                var traitsList = new List<Dictionary<string, object>>();
                foreach (var trait in pawn.story.traits.allTraits)
                {
                    if (trait != null && trait.def != null)
                    {
                        traitsList.Add(new Dictionary<string, object>
                        {
                            ["defName"] = trait.def.defName,
                            ["label"] = trait.LabelCap ?? trait.def.defName,
                            ["degree"] = trait.Degree
                        });
                    }
                }
                result["traits"] = traitsList;
            }

            // ========== 心情/需求 ==========
            if (pawn.needs != null)
            {
                var needsInfo = new Dictionary<string, object>();

                // 心情
                if (pawn.needs.mood != null)
                {
                    needsInfo["mood"] = new Dictionary<string, object>
                    {
                        ["curLevel"] = pawn.needs.mood.CurLevel,
                        ["curLevelPercentage"] = pawn.needs.mood.CurLevelPercentage,
                        ["isMentalBreakImminent"] = pawn.InMentalState
                    };
                }

                // 食物
                var foodNeed = pawn.needs.TryGetNeed<Need_Food>();
                if (foodNeed != null)
                {
                    needsInfo["food"] = new Dictionary<string, object>
                    {
                        ["curLevel"] = foodNeed.CurLevel,
                        ["curLevelPercentage"] = foodNeed.CurLevelPercentage,
                        ["isStarving"] = foodNeed.Starving
                    };
                }

                // 休息
                var restNeed = pawn.needs.TryGetNeed<Need_Rest>();
                if (restNeed != null)
                {
                    needsInfo["rest"] = new Dictionary<string, object>
                    {
                        ["curLevel"] = restNeed.CurLevel,
                        ["curLevelPercentage"] = restNeed.CurLevelPercentage
                    };
                }

                // 娱乐
                var joyNeed = pawn.needs.TryGetNeed<Need_Joy>();
                if (joyNeed != null)
                {
                    needsInfo["joy"] = new Dictionary<string, object>
                    {
                        ["curLevel"] = joyNeed.CurLevel,
                        ["curLevelPercentage"] = joyNeed.CurLevelPercentage
                    };
                }

                result["needs"] = needsInfo;
            }

            // ========== 健康详情（伤害/疾病） ==========
            if (pawn.health?.hediffSet?.hediffs != null && pawn.health.hediffSet.hediffs.Count > 0)
            {
                var hediffsList = new List<Dictionary<string, object>>();
                foreach (var hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff != null && hediff.def != null)
                    {
                        hediffsList.Add(new Dictionary<string, object>
                        {
                            ["defName"] = hediff.def.defName,
                            ["label"] = hediff.LabelCap ?? hediff.def.defName,
                            ["severity"] = hediff.Severity,
                            ["part"] = hediff.Part?.def?.label ?? "WholeBody"
                        });
                    }
                }
                result["hediffs"] = hediffsList;
            }

            // ========== 装备信息 ==========
            var equipmentInfo = new Dictionary<string, object>();

            // 主武器
            if (pawn.equipment?.Primary != null)
            {
                equipmentInfo["primaryWeapon"] = new Dictionary<string, object>
                {
                    ["defName"] = pawn.equipment.Primary.def.defName,
                    ["label"] = pawn.equipment.Primary.Label,
                    ["hitPoints"] = pawn.equipment.Primary.HitPoints
                };
            }

            // 穿着的衣物数量
            int apparelCount = pawn.apparel?.WornApparel?.Count ?? 0;
            equipmentInfo["apparelCount"] = apparelCount;

            result["equipment"] = equipmentInfo;

            // ========== 精神状态 ==========
            if (pawn.InMentalState && pawn.MentalStateDef != null)
            {
                result["mentalState"] = new Dictionary<string, object>
                {
                    ["defName"] = pawn.MentalStateDef.defName,
                    ["label"] = pawn.MentalStateDef.label ?? pawn.MentalStateDef.defName
                };
            }

            // ========== 灵感 ==========
            if (pawn.Inspired && pawn.InspirationDef != null)
            {
                result["inspiration"] = new Dictionary<string, object>
                {
                    ["defName"] = pawn.InspirationDef.defName,
                    ["label"] = pawn.InspirationDef.label ?? pawn.InspirationDef.defName
                };
            }

            return result;
        }

        #endregion

        #region 地形分析 - 基于 TerrainGrid 和 ListerThings API

        /// <summary>
        /// 获取指定位置的地形定义
        /// 对应: map.terrainGrid.TerrainAt(cell)
        /// </summary>
        public static TerrainDef GetTerrainAt(IntVec3 cell, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.terrainGrid?.TerrainAt(cell);
        }

        /// <summary>
        /// 获取所有水域格子
        /// 对应: TerrainDef.IsWater (HasTag("Water"))
        /// </summary>
        public static List<IntVec3> GetWaterCells(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<IntVec3>();

            var result = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain.IsWater)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有河流格子
        /// 对应: TerrainDef.IsRiver (HasTag("River"))
        /// </summary>
        public static List<IntVec3> GetRiverCells(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<IntVec3>();

            var result = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain.IsRiver)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有海洋格子
        /// 对应: TerrainDef.IsOcean (HasTag("Ocean"))
        /// </summary>
        public static List<IntVec3> GetOceanCells(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<IntVec3>();

            var result = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain.IsOcean)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有天然岩石地形格子
        /// 对应: TerrainDef.IsRock (HasTag("NaturalRock"))
        /// </summary>
        public static List<IntVec3> GetRockTerrainCells(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<IntVec3>();

            var result = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain.IsRock)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有土壤格子
        /// 对应: TerrainDef.IsSoil (HasTag("Soil"))
        /// </summary>
        public static List<IntVec3> GetSoilCells(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<IntVec3>();

            var result = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain.IsSoil)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取所有道路格子
        /// 对应: TerrainDef.IsRoad (HasTag("Road"))
        /// </summary>
        public static List<IntVec3> GetRoadCells(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<IntVec3>();

            var result = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain.IsRoad)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据自定义标签获取地形格子
        /// 对应: TerrainDef.HasTag(tag)
        /// </summary>
        public static List<IntVec3> GetTerrainByTag(string tag, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<IntVec3>();

            var result = new List<IntVec3>();
            foreach (IntVec3 cell in map.AllCells)
            {
                TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain != null && terrain.HasTag(tag))
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        #endregion

        #region 物体查询 - 基于 ListerThings API

        /// <summary>
        /// 获取所有植物 (包括树木、农作物等)
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
        /// </summary>
        public static List<Thing> GetAllPlants(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Plant);
        }

        /// <summary>
        /// 获取可收获的植物
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.HarvestablePlant)
        /// </summary>
        public static List<Thing> GetHarvestablePlants(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.HarvestablePlant);
        }

        /// <summary>
        /// 获取所有岩石块 (可搬运的碎石)
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.Chunk)
        /// </summary>
        public static List<Thing> GetRockChunks(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Chunk);
        }

        /// <summary>
        /// 获取所有建筑
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
        /// </summary>
        public static List<Thing> GetBuildings(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
        }

        /// <summary>
        /// 获取所有蓝图
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint)
        /// </summary>
        public static List<Thing> GetBlueprints(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Blueprint);
        }

        /// <summary>
        /// 获取所有可搬运物品
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver)
        /// </summary>
        public static List<Thing> GetHaulableThings(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.HaulableEver);
        }

        /// <summary>
        /// 获取所有食物来源
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.FoodSource)
        /// </summary>
        public static List<Thing> GetFoodSources(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.FoodSource);
        }

        /// <summary>
        /// 获取所有尸体
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
        /// </summary>
        public static List<Thing> GetCorpses(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Corpse);
        }

        /// <summary>
        /// 获取所有武器
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.Weapon)
        /// </summary>
        public static List<Thing> GetWeapons(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon);
        }

        /// <summary>
        /// 获取所有服装
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel)
        /// </summary>
        public static List<Thing> GetApparel(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Apparel);
        }

        /// <summary>
        /// 获取所有火焰
        /// 对应: map.listerThings.ThingsInGroup(ThingRequestGroup.Fire)
        /// </summary>
        public static List<Thing> GetFires(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Fire);
        }

        /// <summary>
        /// 根据自定义 ThingRequestGroup 获取物体
        /// </summary>
        public static List<Thing> GetThingsInGroup(ThingRequestGroup group, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerThings?.ThingsInGroup(group);
        }

        #endregion

        #region 地形统计

        /// <summary>
        /// 获取地形类型统计
        /// </summary>
        public static Dictionary<string, int> GetTerrainStatistics(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new Dictionary<string, int>();

            var stats = new Dictionary<string, int>
            {
                ["water"] = GetWaterCells(map).Count,
                ["river"] = GetRiverCells(map).Count,
                ["ocean"] = GetOceanCells(map).Count,
                ["rock_terrain"] = GetRockTerrainCells(map).Count,
                ["soil"] = GetSoilCells(map).Count,
                ["road"] = GetRoadCells(map).Count,
                ["total_cells"] = map.cellIndices.NumGridCells
            };

            return stats;
        }

        /// <summary>
        /// 获取物体统计
        /// </summary>
        public static Dictionary<string, int> GetThingsStatistics(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new Dictionary<string, int>();

            var stats = new Dictionary<string, int>
            {
                ["plants"] = GetAllPlants(map)?.Count ?? 0,
                ["harvestable_plants"] = GetHarvestablePlants(map)?.Count ?? 0,
                ["rock_chunks"] = GetRockChunks(map)?.Count ?? 0,
                ["buildings"] = GetBuildings(map)?.Count ?? 0,
                ["blueprints"] = GetBlueprints(map)?.Count ?? 0,
                ["haulable"] = GetHaulableThings(map)?.Count ?? 0,
                ["food_sources"] = GetFoodSources(map)?.Count ?? 0,
                ["corpses"] = GetCorpses(map)?.Count ?? 0,
                ["fires"] = GetFires(map)?.Count ?? 0
            };

            return stats;
        }

        #endregion

        #region 物品属性查询

        /// <summary>
        /// 获取所有物品（基础信息列表）
        /// </summary>
        public static List<Dictionary<string, object>> GetAllThingsBasicInfo(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<Dictionary<string, object>>();

            var result = new List<Dictionary<string, object>>();
            foreach (var thing in map.listerThings.AllThings)
            {
                result.Add(GetThingBasicInfo(thing));
            }
            return result;
        }

        /// <summary>
        /// 获取物品的基础信息（Thing + ThingDef 公共属性）
        /// </summary>
        public static Dictionary<string, object> GetThingBasicInfo(Thing thing)
        {
            if (thing == null) return null;

            var def = thing.def;

            // 安全获取分类
            string categoryStr = "Uncategorized";
            try { if (def != null && def.category != null) categoryStr = def.category.ToString(); } catch { }

            return new Dictionary<string, object>
            {
                // ===== Thing 基础属性 =====
                ["id"] = thing.thingIDNumber,
                ["defName"] = def?.defName ?? "Unknown",
                ["label"] = thing.Label ?? def?.label ?? "Unknown",
                ["position"] = new { x = thing.Position.x, z = thing.Position.z },
                ["rotation"] = thing.Rotation.AsInt,

                // 状态
                ["spawned"] = thing.Spawned,
                ["destroyed"] = thing.Destroyed,

                // 堆叠
                ["stackCount"] = thing.stackCount,
                ["maxStackCount"] = def?.stackLimit ?? 1,

                // 耐久度
                ["hitPoints"] = thing.HitPoints,
                ["maxHitPoints"] = thing.MaxHitPoints,
                ["healthPercent"] = thing.MaxHitPoints > 0 ? (float)thing.HitPoints / thing.MaxHitPoints : 0f,

                // ===== ThingDef 分类属性 =====
                ["category"] = categoryStr,
                ["thingClass"] = def?.thingClass?.Name ?? "Unknown",

                // 类型判断
                ["isWeapon"] = SafeGetIsWeapon(def),
                ["isApparel"] = SafeGetIsApparel(def),
                ["isDrug"] = SafeGetIsDrug(def),
                ["isMedicine"] = SafeGetIsMedicine(def),
                ["isFood"] = SafeGetIsFood(def),
                ["isBuilding"] = def?.building != null,
                ["isPlant"] = def?.plant != null,
                ["isChunk"] = SafeGetIsChunk(def),
                ["isCorpse"] = thing is Corpse,

                // 描述
                ["description"] = def?.description?.Replace("\n", " ") ?? ""
            };
        }

        // 安全访问辅助方法
        // 参考 PawnWeaponGenerator.Reset() 中的官方 IsWeapon 定义:
        // equipmentType == EquipmentType.Primary && !weaponTags.NullOrEmpty()
        // 原木等物品虽然有 tools (可作为临时武器)，但 equipmentType 不是 Primary，也没有 weaponTags
        private static bool SafeGetIsWeapon(ThingDef def)
        {
            try
            {
                if (def == null) return false;
                return def.equipmentType == EquipmentType.Primary && def.weaponTags != null && def.weaponTags.Count > 0;
            }
            catch { return false; }
        }
        private static bool SafeGetIsApparel(ThingDef def) { try { return def?.IsApparel ?? false; } catch { return false; } }
        private static bool SafeGetIsDrug(ThingDef def) { try { return def?.IsDrug ?? false; } catch { return false; } }
        private static bool SafeGetIsMedicine(ThingDef def) { try { return def?.IsMedicine ?? false; } catch { return false; } }
        private static bool SafeGetIsFood(ThingDef def) { try { return def?.IsNutritionGivingIngestible ?? false; } catch { return false; } }
        private static bool SafeGetIsChunk(ThingDef def) { try { return def?.thingCategories != null && def.thingCategories.Any(c => c.defName.Contains("Chunk")); } catch { return false; } }

        /// <summary>
        /// 获取物品完整信息（包含特定类型扩展属性）
        /// 自动检测物品类型并返回对应属性
        /// </summary>
        public static Dictionary<string, object> GetThingInfo(Thing thing)
        {
            if (thing == null) return null;

            // 获取基础信息
            var info = GetThingBasicInfo(thing);

            // 根据类型添加扩展属性
            if (thing is Plant plant)
            {
                info["type"] = "Plant";
                info["plantInfo"] = GetPlantSpecificInfo(plant);
            }
            else if (thing is Building building)
            {
                info["type"] = "Building";
                info["buildingInfo"] = GetBuildingSpecificInfo(building);
            }
            else if (thing is Pawn pawn)
            {
                info["type"] = "Pawn";
                info["pawnInfo"] = GetPawnBasicInfo(pawn);
            }
            else if (SafeGetIsWeapon(thing.def))
            {
                info["type"] = "Weapon";
            }
            else if (SafeGetIsApparel(thing.def))
            {
                info["type"] = "Apparel";
            }
            else if (thing is Corpse corpse)
            {
                info["type"] = "Corpse";
                info["corpseInfo"] = GetCorpseSpecificInfo(corpse);
            }
            else
            {
                info["type"] = "Item";
            }

            return info;
        }

        /// <summary>
        /// 获取植物特有属性
        /// </summary>
        public static Dictionary<string, object> GetPlantSpecificInfo(Plant plant)
        {
            if (plant == null) return null;

            var def = plant.def;
            var plantDef = def?.plant;

            // 安全获取 PlantDef 属性
            float growDays = 0f, fertilityMin = 0f, fertilitySensitivity = 0f;
            float wildClusterRadius = 0f, wildClusterWeight = 0f, harvestYield = 0f;

            try
            {
                if (plantDef != null)
                {
                    growDays = plantDef.growDays;
                    fertilityMin = plantDef.fertilityMin;
                    fertilitySensitivity = plantDef.fertilitySensitivity;
                    wildClusterRadius = plantDef.wildClusterRadius;
                    wildClusterWeight = plantDef.wildClusterWeight;
                    harvestYield = plantDef.harvestYield;
                }
            }
            catch { }

            return new Dictionary<string, object>
            {
                // ===== Plant 生长属性 =====
                ["growth"] = plant.Growth,                    // 生长进度 0-1
                ["age"] = plant.Age,                          // 年龄（ticks）
                ["lifeStage"] = plant.LifeStage.ToString(),   // 生命阶段

                // 状态
                ["harvestableNow"] = plant.HarvestableNow,    // 现在可收获
                ["blighted"] = plant.Blighted,                // 是否患病
                ["isCrop"] = plant.IsCrop,                    // 是农作物

                // 生长相关
                ["growthRate"] = plant.GrowthRate,            // 生长速率

                // ===== PlantDef 属性 =====
                ["growDays"] = growDays,
                ["harvestYield"] = harvestYield,
                ["fertilityMin"] = fertilityMin,
                ["fertilitySensitivity"] = fertilitySensitivity,
                ["wildClusterRadius"] = wildClusterRadius,
                ["wildClusterWeight"] = wildClusterWeight
            };
        }

        /// <summary>
        /// 获取建筑特有属性
        /// </summary>
        public static Dictionary<string, object> GetBuildingSpecificInfo(Building building)
        {
            if (building == null) return null;

            var def = building.def;
            var info = new Dictionary<string, object>
            {
                // ===== Building 基础属性 =====
                ["isAirtight"] = building.IsAirtight,
                ["maxItemsInCell"] = building.MaxItemsInCell,

                // ===== BuildingDef 属性 =====
                ["size"] = new { x = def?.size.x ?? 1, z = def?.size.z ?? 1 },
                ["rotatable"] = def?.rotatable ?? false,
                ["destroyable"] = def?.destroyable ?? true,
                ["minifiable"] = def?.minifiedDef != null,
                ["leaveResourcesWhenKilled"] = def?.leaveResourcesWhenKilled ?? true
            };

            // ===== Comp 属性 =====

            // 电源组件
            var powerComp = building.PowerComp;
            if (powerComp != null)
            {
                try
                {
                    info["powerComp"] = new Dictionary<string, object>
                    {
                        ["transmitsPower"] = powerComp.Props?.transmitsPower ?? false
                    };
                }
                catch { }
            }

            // 工作台组件
            var workTable = building as Building_WorkTable;
            if (workTable != null)
            {
                try
                {
                    info["workTable"] = new Dictionary<string, object>
                    {
                        ["currentlyUsable"] = workTable.CurrentlyUsableForBills(),
                        ["billCount"] = workTable.BillStack?.Count ?? 0
                    };
                }
                catch { }
            }

            // 储存组件
            var storage = building as Building_Storage;
            if (storage != null)
            {
                try
                {
                    info["storage"] = new Dictionary<string, object>
                    {
                        ["slotGroup"] = storage.GetSlotGroup() != null,
                        ["settings"] = storage.settings != null
                    };
                }
                catch { }
            }

            // 门
            var door = building as Building_Door;
            if (door != null)
            {
                try
                {
                    info["door"] = new Dictionary<string, object>
                    {
                        ["open"] = door.Open,
                        ["holdOpen"] = door.HoldOpen
                    };
                }
                catch { }
            }

            // 温控设备
            var tempControl = building.TryGetComp<CompTempControl>();
            if (tempControl != null)
            {
                try
                {
                    info["tempControl"] = new Dictionary<string, object>
                    {
                        ["targetTemperature"] = tempControl.targetTemperature,
                        ["energyPerSecond"] = tempControl.Props?.energyPerSecond ?? 0f
                    };
                }
                catch { }
            }

            return info;
        }

        /// <summary>
        /// 获取尸体特有属性
        /// </summary>
        private static Dictionary<string, object> GetCorpseSpecificInfo(Corpse corpse)
        {
            if (corpse == null) return null;

            return new Dictionary<string, object>
            {
                ["innerPawnId"] = corpse.InnerPawn?.thingIDNumber ?? -1,
                ["innerPawnName"] = corpse.InnerPawn?.LabelShort ?? "Unknown",
                ["innerPawnRace"] = corpse.InnerPawn?.def?.label ?? "Unknown",
                ["timeOfDeath"] = corpse.timeOfDeath,
                ["rottable"] = corpse.GetComp<CompRottable>() != null,
                ["age"] = corpse.Age
            };
        }

        /// <summary>
        /// 获取 Pawn 基础信息（用于物品列表）
        /// </summary>
        private static Dictionary<string, object> GetPawnBasicInfo(Pawn pawn)
        {
            if (pawn == null) return null;

            return new Dictionary<string, object>
            {
                ["name"] = pawn.LabelShort,
                ["race"] = pawn.def?.label ?? "Unknown",
                ["faction"] = pawn.Faction?.Name ?? "None",
                ["isColonist"] = pawn.IsColonist,
                ["isPrisoner"] = pawn.IsPrisoner,
                ["isAnimal"] = pawn.RaceProps?.Animal ?? false,
                ["isHumanlike"] = pawn.RaceProps?.Humanlike ?? false,
                ["downed"] = pawn.Downed,
                ["dead"] = pawn.Dead
            };
        }

        /// <summary>
        /// 根据ID获取物品
        /// </summary>
        public static Thing GetThingById(int thingId, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return null;

            return map.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
        }

        /// <summary>
        /// 根据类型筛选物品
        /// </summary>
        public static List<Thing> GetThingsByType(string type, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return new List<Thing>();

            var allThings = map.listerThings.AllThings;

            switch (type.ToLower())
            {
                case "plant":
                    return allThings.Where(t => t is Plant).ToList();
                case "building":
                    return allThings.Where(t => t is Building).ToList();
                case "weapon":
                    return allThings.Where(t => SafeGetIsWeapon(t.def)).ToList();
                case "apparel":
                    return allThings.Where(t => SafeGetIsApparel(t.def)).ToList();
                case "food":
                    return allThings.Where(t => SafeGetIsFood(t.def)).ToList();
                case "drug":
                    return allThings.Where(t => SafeGetIsDrug(t.def)).ToList();
                case "medicine":
                    return allThings.Where(t => SafeGetIsMedicine(t.def)).ToList();
                case "chunk":
                    // 依据: ThingRequestGroup.cs 第66行 Chunk
                    return allThings.Where(t => t.def?.thingClass?.Name == "Chunk").ToList();
                case "corpse":
                    return allThings.Where(t => t is Corpse).ToList();
                case "pawn":
                    return allThings.Where(t => t is Pawn).ToList();
                case "blueprint":
                    // 依据: ThingRequestGroup.cs 第14行 Blueprint
                    // 蓝图 - 玩家放置但未开始建造的
                    return map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint);
                case "frame":
                    // 依据: ThingRequestGroup.cs 第16行 BuildingFrame
                    // 框架 - 正在建造中的（已运送材料，建造进行中）
                    return allThings.Where(t => t is Frame).ToList();
                case "filth":
                    // 依据: ThingRequestGroup.cs 第20行 Filth
                    // 污垢 - 地面上的污渍，需要清洁
                    return allThings.Where(t => t is Filth).ToList();
                default:
                    return allThings.ToList();
            }
        }

        #endregion

        #region Zone 区域系统 - 基于 ZoneManager API

        /// <summary>
        /// 获取所有区域
        /// 对应: map.zoneManager.AllZones
        /// </summary>
        public static List<Zone> GetAllZones(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.zoneManager?.AllZones;
        }

        /// <summary>
        /// 获取储存区列表
        /// </summary>
        public static List<Zone_Stockpile> GetStockpileZones(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null) return new List<Zone_Stockpile>();

            return map.zoneManager.AllZones
                .OfType<Zone_Stockpile>()
                .ToList();
        }

        /// <summary>
        /// 获取种植区列表
        /// </summary>
        public static List<Zone_Growing> GetGrowingZones(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null) return new List<Zone_Growing>();

            return map.zoneManager.AllZones
                .OfType<Zone_Growing>()
                .ToList();
        }

        /// <summary>
        /// 获取区域的基础信息
        /// </summary>
        public static Dictionary<string, object> GetZoneBasicInfo(Zone zone)
        {
            if (zone == null) return null;

            var info = new Dictionary<string, object>
            {
                ["id"] = zone.ID,
                ["label"] = zone.label ?? "Unnamed",
                ["cellCount"] = zone.CellCount,
                ["hidden"] = zone.Hidden,
                ["position"] = zone.Position.IsValid ? new { x = zone.Position.x, z = zone.Position.z } : null,
                ["type"] = zone.GetType().Name
            };

            // 计算区域边界
            if (zone.cells != null && zone.cells.Count > 0)
            {
                int minX = int.MaxValue, maxX = int.MinValue;
                int minZ = int.MaxValue, maxZ = int.MinValue;
                foreach (var cell in zone.cells)
                {
                    if (cell.x < minX) minX = cell.x;
                    if (cell.x > maxX) maxX = cell.x;
                    if (cell.z < minZ) minZ = cell.z;
                    if (cell.z > maxZ) maxZ = cell.z;
                }
                info["bounds"] = new { minX, maxX, minZ, maxZ };
            }

            return info;
        }

        /// <summary>
        /// 获取区域的详细信息（包含物品统计）
        /// </summary>
        public static Dictionary<string, object> GetZoneDetailedInfo(Zone zone)
        {
            if (zone == null) return null;

            var info = GetZoneBasicInfo(zone);

            // 物品统计
            info["heldThingsCount"] = zone.HeldThingsCount;

            // 物品分类统计
            var thingStats = new Dictionary<string, int>();
            try
            {
                foreach (var thing in zone.AllContainedThings)
                {
                    if (thing == null) continue;
                    string category = (thing.def != null) ? thing.def.category.ToString() : "Unknown";
                    if (!thingStats.ContainsKey(category))
                        thingStats[category] = 0;
                    thingStats[category]++;
                }
            }
            catch { }
            info["thingStats"] = thingStats;

            // 储存区特定信息
            if (zone is Zone_Stockpile stockpile)
            {
                info["zoneType"] = "Stockpile";
                try
                {
                    var slotGroup = stockpile.slotGroup;
                    string parentLabel = "Unknown";
                    try
                    {
                        if (slotGroup?.parent != null)
                        {
                            // Label 可能是方法或属性，尝试多种方式
                            var parentObj = slotGroup.parent;
                            var labelProp = parentObj.GetType().GetProperty("Label");
                            if (labelProp != null)
                                parentLabel = labelProp.GetValue(parentObj)?.ToString() ?? "Unknown";
                            else
                                parentLabel = parentObj.ToString() ?? "Unknown";
                        }
                    }
                    catch { }

                    info["slotGroup"] = new
                    {
                        cellsCount = slotGroup?.CellsList?.Count ?? 0,
                        parent = parentLabel
                    };
                }
                catch { }
            }
            // 种植区特定信息
            else if (zone is Zone_Growing growing)
            {
                info["zoneType"] = "Growing";
                try
                {
                    info["growing"] = new Dictionary<string, object>
                    {
                        ["plantDef"] = growing.GetPlantDefToGrow()?.defName ?? "None",
                        ["plantLabel"] = growing.GetPlantDefToGrow()?.label ?? "None"
                    };
                }
                catch { }
            }

            return info;
        }

        /// <summary>
        /// 获取所有区域的统计信息
        /// </summary>
        public static Dictionary<string, object> GetZonesStatistics(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null) return new Dictionary<string, object>();

            var zones = map.zoneManager.AllZones;
            var stockpiles = zones.OfType<Zone_Stockpile>().ToList();
            var growings = zones.OfType<Zone_Growing>().ToList();

            return new Dictionary<string, object>
            {
                ["totalZones"] = zones.Count,
                ["stockpileCount"] = stockpiles.Count,
                ["growingCount"] = growings.Count,
                ["stockpileCells"] = stockpiles.Sum(z => z.CellCount),
                ["growingCells"] = growings.Sum(z => z.CellCount)
            };
        }

        /// <summary>
        /// 创建新区域
        /// 对应: new Zone_Stockpile/Zone_Growing + zoneManager.RegisterZone()
        /// </summary>
        public static Dictionary<string, object> CreateZone(string zoneType, List<IntVec3> cells = null, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
                return new Dictionary<string, object> { ["error"] = "No map available" };

            Zone zone;
            string baseLabel;

            try
            {
                if (zoneType == "stockpile")
                {
                    // 创建储存区，使用默认预设
                    zone = new Zone_Stockpile(StorageSettingsPreset.DefaultStockpile, map.zoneManager);
                    baseLabel = "Stockpile";
                }
                else if (zoneType == "growing")
                {
                    // 创建种植区
                    zone = new Zone_Growing(map.zoneManager);
                    baseLabel = "GrowingZone";
                }
                else
                {
                    return new Dictionary<string, object> { ["error"] = $"Unknown zone type: {zoneType}. Supported types: stockpile, growing" };
                }

                // 添加格子，记录详细失败原因
                var skippedCells = new List<Dictionary<string, object>>();
                int requestedCount = 0;

                if (cells != null && cells.Count > 0)
                {
                    requestedCount = cells.Count;
                    foreach (var cell in cells)
                    {
                        if (!cell.InBounds(map))
                        {
                            skippedCells.Add(new Dictionary<string, object>
                            {
                                ["x"] = cell.x,
                                ["z"] = cell.z,
                                ["reason"] = "Out_Of_Bounds",
                                ["detail"] = "坐标超出地图范围"
                            });
                            continue;
                        }

                        // 检查地形
                        TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                        string terrainRejectReason = GetTerrainRejectReason(terrain, zoneType);
                        if (terrainRejectReason != null)
                        {
                            skippedCells.Add(new Dictionary<string, object>
                            {
                                ["x"] = cell.x,
                                ["z"] = cell.z,
                                ["reason"] = terrainRejectReason,
                                ["detail"] = GetTerrainRejectDetail(terrain, terrainRejectReason),
                                ["terrain"] = terrain?.defName ?? "Unknown"
                            });
                            continue;
                        }

                        // 检查是否被建筑阻挡
                        var things = map.thingGrid.ThingsListAt(cell);
                        bool blocked = false;
                        string blockedBy = "";
                        foreach (var thing in things)
                        {
                            if (!thing.def.CanOverlapZones)
                            {
                                blocked = true;
                                blockedBy = thing.def.label ?? thing.def.defName;
                                break;
                            }
                        }
                        if (blocked)
                        {
                            skippedCells.Add(new Dictionary<string, object>
                            {
                                ["x"] = cell.x,
                                ["z"] = cell.z,
                                ["reason"] = "Building_Blocked",
                                ["detail"] = $"被建筑阻挡: {blockedBy}"
                            });
                            continue;
                        }

                        // 尝试添加格子
                        try
                        {
                            zone.AddCell(cell);
                        }
                        catch (Exception ex)
                        {
                            skippedCells.Add(new Dictionary<string, object>
                            {
                                ["x"] = cell.x,
                                ["z"] = cell.z,
                                ["reason"] = "Add_Failed",
                                ["detail"] = ex.Message
                            });
                        }
                    }
                }

                // 如果没有格子成功添加，尝试找一个可用的格子
                if (zone.CellCount == 0 && requestedCount > 0)
                {
                    // 在地图中心附近找一个可用的格子
                    var center = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2);
                    bool foundFallback = false;
                    for (int dx = -5; dx <= 5 && !foundFallback; dx++)
                    {
                        for (int dz = -5; dz <= 5 && !foundFallback; dz++)
                        {
                            var testCell = center + new IntVec3(dx, 0, dz);
                            if (!testCell.InBounds(map)) continue;

                            TerrainDef terrain = map.terrainGrid.TerrainAt(testCell);
                            if (GetTerrainRejectReason(terrain, zoneType) != null) continue;

                            var things = map.thingGrid.ThingsListAt(testCell);
                            bool blocked = false;
                            foreach (var thing in things)
                            {
                                if (!thing.def.CanOverlapZones) { blocked = true; break; }
                            }
                            if (blocked) continue;

                            try
                            {
                                zone.AddCell(testCell);
                                foundFallback = true;
                            }
                            catch { }
                        }
                    }
                }

                // 注册区域
                map.zoneManager.RegisterZone(zone);

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["zoneId"] = zone.ID,
                    ["label"] = zone.label,
                    ["type"] = zone.GetType().Name,
                    ["cellCount"] = zone.CellCount,
                    ["requestedCount"] = requestedCount,
                    ["skippedCount"] = skippedCells.Count,
                    ["skippedCells"] = skippedCells
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { ["error"] = $"Failed to create zone: {ex.Message}" };
            }
        }

        /// <summary>
        /// 检查地形是否支持区域，返回拒绝原因（null表示支持）
        /// </summary>
        private static string GetTerrainRejectReason(TerrainDef terrain, string zoneType)
        {
            if (terrain == null) return "Unknown_Terrain";

            // 水面不支持任何区域
            if (terrain.IsWater) return "Water_Terrain";

            // 河流不支持区域
            if (terrain.IsRiver) return "River_Terrain";

            // 岩石不支持区域
            if (terrain.IsRock) return "Rock_Terrain";

            // 海洋不支持区域
            if (terrain.IsOcean) return "Ocean_Terrain";

            // 种植区额外检查：需要土壤或可种植地形
            if (zoneType == "growing")
            {
                // 检查是否支持种植（肥沃度 > 0 或标签包含 Soil）
                if (!terrain.IsSoil && terrain.fertility < 0.1f)
                {
                    return "Not_Fertile";
                }
            }

            return null; // 支持该地形
        }

        /// <summary>
        /// 获取地形拒绝的详细描述
        /// </summary>
        private static string GetTerrainRejectDetail(TerrainDef terrain, string reason)
        {
            switch (reason)
            {
                case "Water_Terrain":
                    return $"水面地形({terrain?.label ?? "未知"})不支持区域";
                case "River_Terrain":
                    return $"河流地形({terrain?.label ?? "未知"})不支持区域";
                case "Rock_Terrain":
                    return $"岩石地形({terrain?.label ?? "未知"})不支持区域";
                case "Ocean_Terrain":
                    return $"海洋地形({terrain?.label ?? "未知"})不支持区域";
                case "Not_Fertile":
                    return $"地形({terrain?.label ?? "未知"})肥沃度不足，无法种植";
                default:
                    return $"地形({terrain?.label ?? "未知"})不支持区域";
            }
        }

        /// <summary>
        /// 删除区域
        /// 对应: zone.Delete()
        /// </summary>
        public static Dictionary<string, object> DeleteZone(int zoneId, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
                return new Dictionary<string, object> { ["error"] = "No map available" };

            var zone = map.zoneManager.AllZones.FirstOrDefault(z => z.ID == zoneId);
            if (zone == null)
                return new Dictionary<string, object> { ["error"] = $"Zone with id {zoneId} not found" };

            try
            {
                string zoneLabel = zone.label;
                string zoneType = zone.GetType().Name;

                zone.Delete();

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["deletedZoneId"] = zoneId,
                    ["deletedLabel"] = zoneLabel,
                    ["deletedType"] = zoneType
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { ["error"] = $"Failed to delete zone: {ex.Message}" };
            }
        }

        /// <summary>
        /// 向区域添加格子
        /// 对应: zone.AddCell()
        /// </summary>
        public static Dictionary<string, object> AddCellsToZone(int zoneId, List<IntVec3> cells, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
                return new Dictionary<string, object> { ["error"] = "No map available" };

            if (cells == null || cells.Count == 0)
                return new Dictionary<string, object> { ["error"] = "No cells provided" };

            var zone = map.zoneManager.AllZones.FirstOrDefault(z => z.ID == zoneId);
            if (zone == null)
                return new Dictionary<string, object> { ["error"] = $"Zone with id {zoneId} not found" };

            // 确定区域类型用于地形检查
            string zoneType = zone is Zone_Growing ? "growing" : "stockpile";

            int addedCount = 0;
            int skippedCount = 0;
            var skippedCells = new List<Dictionary<string, object>>();
            var errors = new List<string>();

            foreach (var cell in cells)
            {
                try
                {
                    // 检查是否在地图范围内
                    if (!cell.InBounds(map))
                    {
                        skippedCount++;
                        skippedCells.Add(new Dictionary<string, object>
                        {
                            ["x"] = cell.x,
                            ["z"] = cell.z,
                            ["reason"] = "Out_Of_Bounds",
                            ["detail"] = "坐标超出地图范围"
                        });
                        continue;
                    }

                    // 检查是否已在区域中
                    if (zone.ContainsCell(cell))
                    {
                        skippedCount++;
                        skippedCells.Add(new Dictionary<string, object>
                        {
                            ["x"] = cell.x,
                            ["z"] = cell.z,
                            ["reason"] = "Already_In_Zone",
                            ["detail"] = "该格子已在区域中"
                        });
                        continue;
                    }

                    // 检查地形是否支持区域
                    TerrainDef terrain = map.terrainGrid.TerrainAt(cell);
                    string terrainRejectReason = GetTerrainRejectReason(terrain, zoneType);
                    if (terrainRejectReason != null)
                    {
                        skippedCount++;
                        skippedCells.Add(new Dictionary<string, object>
                        {
                            ["x"] = cell.x,
                            ["z"] = cell.z,
                            ["reason"] = terrainRejectReason,
                            ["detail"] = GetTerrainRejectDetail(terrain, terrainRejectReason),
                            ["terrain"] = terrain?.defName ?? "Unknown"
                        });
                        continue;
                    }

                    // 检查格子上是否有不兼容的建筑
                    var things = map.thingGrid.ThingsListAt(cell);
                    bool blocked = false;
                    string blockedBy = "";
                    foreach (var thing in things)
                    {
                        if (!thing.def.CanOverlapZones)
                        {
                            blocked = true;
                            blockedBy = thing.def.label ?? thing.def.defName;
                            break;
                        }
                    }

                    if (blocked)
                    {
                        skippedCount++;
                        skippedCells.Add(new Dictionary<string, object>
                        {
                            ["x"] = cell.x,
                            ["z"] = cell.z,
                            ["reason"] = "Building_Blocked",
                            ["detail"] = $"被建筑阻挡: {blockedBy}"
                        });
                        continue;
                    }

                    // 添加格子
                    zone.AddCell(cell);
                    addedCount++;
                }
                catch (Exception ex)
                {
                    skippedCount++;
                    errors.Add($"Failed to add cell ({cell.x},{cell.z}): {ex.Message}");
                    skippedCells.Add(new Dictionary<string, object>
                    {
                        ["x"] = cell.x,
                        ["z"] = cell.z,
                        ["reason"] = "Add_Failed",
                        ["detail"] = ex.Message
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["zoneId"] = zoneId,
                ["addedCount"] = addedCount,
                ["skippedCount"] = skippedCount,
                ["totalCells"] = zone.CellCount,
                ["skippedCells"] = skippedCells,
                ["errors"] = errors
            };
        }

        /// <summary>
        /// 从区域移除格子
        /// 对应: zone.RemoveCell()
        /// </summary>
        public static Dictionary<string, object> RemoveCellsFromZone(int zoneId, List<IntVec3> cells, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
                return new Dictionary<string, object> { ["error"] = "No map available" };

            if (cells == null || cells.Count == 0)
                return new Dictionary<string, object> { ["error"] = "No cells provided" };

            var zone = map.zoneManager.AllZones.FirstOrDefault(z => z.ID == zoneId);
            if (zone == null)
                return new Dictionary<string, object> { ["error"] = $"Zone with id {zoneId} not found" };

            int removedCount = 0;
            int skippedCount = 0;

            foreach (var cell in cells)
            {
                try
                {
                    if (zone.ContainsCell(cell))
                    {
                        zone.RemoveCell(cell);
                        removedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                catch
                {
                    skippedCount++;
                }
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["zoneId"] = zoneId,
                ["removedCount"] = removedCount,
                ["skippedCount"] = skippedCount,
                ["totalCells"] = zone.CellCount,
                ["zoneDeleted"] = zone.CellCount == 0  // 区域为空时自动删除
            };
        }

        /// <summary>
        /// 设置种植区的作物
        /// </summary>
        public static Dictionary<string, object> SetGrowingZonePlant(int zoneId, string plantDefName, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
                return new Dictionary<string, object> { ["error"] = "No map available" };

            var zone = map.zoneManager.AllZones.FirstOrDefault(z => z.ID == zoneId);
            if (zone == null)
                return new Dictionary<string, object> { ["error"] = $"Zone with id {zoneId} not found" };

            if (!(zone is Zone_Growing growingZone))
                return new Dictionary<string, object> { ["error"] = $"Zone {zoneId} is not a growing zone" };

            try
            {
                var plantDef = DefDatabase<ThingDef>.GetNamedSilentFail(plantDefName);
                if (plantDef == null)
                    return new Dictionary<string, object> { ["error"] = $"Plant def '{plantDefName}' not found" };

                growingZone.SetPlantDefToGrow(plantDef);

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["zoneId"] = zoneId,
                    ["plantDef"] = plantDef.defName,
                    ["plantLabel"] = plantDef.label
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object> { ["error"] = $"Failed to set plant: {ex.Message}" };
            }
        }

        #endregion

        #region Area 区域限制系统 - 基于 AreaManager API

        /// <summary>
        /// 获取所有区域
        /// </summary>
        public static List<Area> GetAllAreas(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.areaManager?.AllAreas;
        }

        /// <summary>
        /// 获取可分配的区域（允许区域）
        /// </summary>
        public static List<Area> GetAllowedAreas(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.areaManager == null) return new List<Area>();

            return map.areaManager.AllAreas
                .Where(a => a.AssignableAsAllowed())
                .ToList();
        }

        /// <summary>
        /// 获取区域信息
        /// </summary>
        public static Dictionary<string, object> GetAreaInfo(Area area)
        {
            if (area == null) return null;

            return new Dictionary<string, object>
            {
                ["id"] = area.ID,
                ["label"] = area.Label ?? "Unnamed",
                ["cellCount"] = area.TrueCount,
                ["color"] = new { r = (int)(area.Color.r * 255), g = (int)(area.Color.g * 255), b = (int)(area.Color.b * 255) },
                ["assignableAsAllowed"] = area.AssignableAsAllowed(),
                ["mutable"] = area.Mutable,
                ["type"] = area.GetType().Name
            };
        }

        #endregion

        #region Room 房间系统 - 基于 Room API

        /// <summary>
        /// 获取指定位置的房间
        /// </summary>
        public static Room GetRoomAt(IntVec3 cell, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null || !cell.InBounds(map)) return null;
            return cell.GetRoom(map);
        }

        /// <summary>
        /// 获取房间信息
        /// </summary>
        public static Dictionary<string, object> GetRoomInfo(Room room)
        {
            if (room == null) return null;

            var info = new Dictionary<string, object>
            {
                ["id"] = room.ID,
                ["cellCount"] = room.CellCount,
                ["temperature"] = room.Temperature,
                ["role"] = room.Role?.defName ?? "None",
                ["roleLabel"] = room.Role?.label ?? "None",
                ["properRoom"] = room.ProperRoom,
                ["touchesMapEdge"] = room.TouchesMapEdge,
                ["openRoofCount"] = room.OpenRoofCount,
                ["psychologicallyOutdoors"] = room.PsychologicallyOutdoors,
                ["outdoorsForWork"] = room.OutdoorsForWork,
                ["isDoorway"] = room.IsDoorway,
                ["isPrisonCell"] = room.IsPrisonCell,
                ["fogged"] = room.Fogged
            };

            // 拥有者
            try
            {
                var owners = room.Owners.ToList();
                info["ownerCount"] = owners.Count;
                info["owners"] = owners.Select(p => p.LabelShort).ToList();
            }
            catch
            {
                info["ownerCount"] = 0;
                info["owners"] = new List<string>();
            }

            // 区域边界
            try
            {
                var extents = room.ExtentsClose;
                info["extents"] = new
                {
                    minX = extents.minX,
                    maxX = extents.maxX,
                    minZ = extents.minZ,
                    maxZ = extents.maxZ
                };
            }
            catch { }

            return info;
        }

        #endregion

        #region ResourceCounter 资源统计系统

        /// <summary>
        /// 获取资源计数
        /// 对应: map.resourceCounter
        /// </summary>
        public static Dictionary<ThingDef, int> GetAllResourceCounts(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.resourceCounter?.AllCountedAmounts;
        }

        /// <summary>
        /// 获取资源统计摘要（按类别分组）
        /// </summary>
        public static Dictionary<string, object> GetResourceSummary(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.resourceCounter == null) return new Dictionary<string, object>();

            var resources = map.resourceCounter.AllCountedAmounts;
            if (resources == null) return new Dictionary<string, object>();

            // 按类别分组
            var byCategory = new Dictionary<string, Dictionary<string, int>>();
            var totalItems = 0;

            foreach (var kvp in resources)
            {
                if (kvp.Value <= 0) continue;

                var def = kvp.Key;
                string category = "Other";

                // 分类逻辑
                if (def.IsNutritionGivingIngestible)
                    category = "Food";
                else if (def.IsWeapon)
                    category = "Weapons";
                else if (def.IsApparel)
                    category = "Apparel";
                else if (def.IsDrug)
                    category = "Drugs";
                else if (def.IsMedicine)
                    category = "Medicine";
                else if (def.IsStuff)
                    category = "Materials";
                else if (def.mineable)
                    category = "Mineable";
                else if (def.category == ThingCategory.Item)
                    category = "Items";

                if (!byCategory.ContainsKey(category))
                    byCategory[category] = new Dictionary<string, int>();

                byCategory[category][def.defName] = kvp.Value;
                totalItems += kvp.Value;
            }

            return new Dictionary<string, object>
            {
                ["totalItemTypes"] = resources.Count(kvp => kvp.Value > 0),
                ["totalItems"] = totalItems,
                ["byCategory"] = byCategory,
                ["foodNutrition"] = GetTotalFoodNutrition(map)
            };
        }

        /// <summary>
        /// 获取总食物营养值
        /// </summary>
        public static float GetTotalFoodNutrition(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.resourceCounter == null) return 0f;

            float totalNutrition = 0f;
            var resources = map.resourceCounter.AllCountedAmounts;

            foreach (var kvp in resources)
            {
                if (kvp.Value <= 0) continue;
                var def = kvp.Key;
                if (def.IsNutritionGivingIngestible && def.ingestible != null)
                {
                    totalNutrition += def.ingestible.CachedNutrition * kvp.Value;
                }
            }

            return totalNutrition;
        }

        /// <summary>
        /// 获取特定资源的数量
        /// </summary>
        public static int GetResourceCount(ThingDef resourceDef, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.resourceCounter == null) return 0;

            var counts = map.resourceCounter.AllCountedAmounts;
            if (counts != null && counts.TryGetValue(resourceDef, out int count))
                return count;
            return 0;
        }

        /// <summary>
        /// 获取关键资源概览（AI决策最关心的资源）
        /// </summary>
        public static Dictionary<string, object> GetCriticalResourcesOverview(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.resourceCounter == null) return new Dictionary<string, object>();

            var resources = map.resourceCounter.AllCountedAmounts;
            var overview = new Dictionary<string, object>();

            // 关键食物类型 - 使用 ThingDefOf 中存在的定义
            var foodDefs = new List<ThingDef>();
            try { if (ThingDefOf.MealSimple != null) foodDefs.Add(ThingDefOf.MealSimple); } catch { }
            try { if (ThingDefOf.MealFine != null) foodDefs.Add(ThingDefOf.MealFine); } catch { }
            try { if (ThingDefOf.MealNutrientPaste != null) foodDefs.Add(ThingDefOf.MealNutrientPaste); } catch { }
            try { if (ThingDefOf.MealSurvivalPack != null) foodDefs.Add(ThingDefOf.MealSurvivalPack); } catch { }
            try { if (ThingDefOf.RawPotatoes != null) foodDefs.Add(ThingDefOf.RawPotatoes); } catch { }

            var foodStats = new Dictionary<string, int>();
            int totalFood = 0;
            foreach (var def in foodDefs)
            {
                if (def != null && resources.TryGetValue(def, out int count) && count > 0)
                {
                    foodStats[def.defName] = count;
                    totalFood += count;
                }
            }
            overview["food"] = new Dictionary<string, object>
            {
                ["totalItems"] = totalFood,
                ["totalNutrition"] = GetTotalFoodNutrition(map),
                ["byType"] = foodStats
            };

            // 关键材料
            var materialStats = new Dictionary<string, int>();
            try
            {
                var stuffDefs = DefDatabase<ThingDef>.AllDefs
                    .Where(d => d.IsStuff && d.stuffProps?.categories != null)
                    .Take(20); // 限制数量

                foreach (var def in stuffDefs)
                {
                    if (resources.TryGetValue(def, out int count) && count > 0)
                    {
                        materialStats[def.defName] = count;
                    }
                }
            }
            catch { }
            overview["materials"] = materialStats;

            // 药物
            var medicineDefs = new[] { ThingDefOf.MedicineHerbal, ThingDefOf.MedicineIndustrial, ThingDefOf.MedicineUltratech };
            var medStats = new Dictionary<string, int>();
            int totalMeds = 0;
            foreach (var def in medicineDefs)
            {
                if (def != null && resources.TryGetValue(def, out int count) && count > 0)
                {
                    medStats[def.defName] = count;
                    totalMeds += count;
                }
            }
            overview["medicine"] = new Dictionary<string, object>
            {
                ["total"] = totalMeds,
                ["byType"] = medStats
            };

            return overview;
        }

        #endregion

        #region Wealth 财富系统

        /// <summary>
        /// 获取殖民地财富概览
        /// </summary>
        public static Dictionary<string, object> GetWealthOverview(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.wealthWatcher == null) return new Dictionary<string, object>();

            var watcher = map.wealthWatcher;

            return new Dictionary<string, object>
            {
                ["wealthTotal"] = watcher.WealthTotal,
                ["wealthItems"] = watcher.WealthItems,
                ["wealthBuildings"] = watcher.WealthBuildings,
                ["wealthPawns"] = watcher.WealthPawns
            };
        }

        #endregion

        #region Work 工作/任务系统 - 基于 Pawn_WorkSettings API

        /// <summary>
        /// 获取角色的所有工作优先级
        /// </summary>
        public static Dictionary<string, object> GetPawnWorkPriorities(Pawn pawn)
        {
            if (pawn == null || pawn.workSettings == null)
                return new Dictionary<string, object> { ["error"] = "Pawn has no work settings" };

            var settings = pawn.workSettings;
            var priorities = new List<Dictionary<string, object>>();

            foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs)
            {
                try
                {
                    int priority = settings.GetPriority(workType);
                    bool isDisabled = pawn.WorkTypeIsDisabled(workType);

                    // 获取相关技能列表
                    var relevantSkills = new List<string>();
                    if (workType.relevantSkills != null)
                    {
                        foreach (var skill in workType.relevantSkills)
                        {
                            relevantSkills.Add(skill.defName);
                        }
                    }

                    priorities.Add(new Dictionary<string, object>
                    {
                        ["defName"] = workType.defName,
                        ["label"] = workType.label ?? workType.defName,
                        ["labelShort"] = workType.labelShort ?? workType.label ?? workType.defName,
                        ["priority"] = priority,
                        ["isActive"] = priority > 0,
                        ["isDisabled"] = isDisabled,
                        ["naturalPriority"] = workType.naturalPriority,
                        ["relevantSkills"] = relevantSkills
                    });
                }
                catch { }
            }

            return new Dictionary<string, object>
            {
                ["pawnId"] = pawn.thingIDNumber,
                ["pawnName"] = pawn.LabelShort,
                ["everWork"] = settings.EverWork,
                ["initialized"] = settings.Initialized,
                ["priorities"] = priorities.OrderBy(p => (int)p["priority"]).ThenBy(p => (string)p["defName"]).ToList()
            };
        }

        /// <summary>
        /// 设置角色的单个工作优先级
        /// </summary>
        public static bool SetPawnWorkPriority(Pawn pawn, WorkTypeDef workType, int priority)
        {
            if (pawn == null || pawn.workSettings == null)
                return false;

            if (priority < 0 || priority > 4)
                return false;

            if (pawn.WorkTypeIsDisabled(workType))
                return false;

            try
            {
                pawn.workSettings.SetPriority(workType, priority);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取所有工作类型定义
        /// </summary>
        public static List<Dictionary<string, object>> GetAllWorkTypes()
        {
            var result = new List<Dictionary<string, object>>();

            foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs.OrderBy(w => w.naturalPriority))
            {
                // 获取相关技能列表
                var relevantSkills = new List<string>();
                if (workType.relevantSkills != null)
                {
                    foreach (var skill in workType.relevantSkills)
                    {
                        relevantSkills.Add(skill.defName);
                    }
                }

                result.Add(new Dictionary<string, object>
                {
                    ["defName"] = workType.defName,
                    ["label"] = workType.label ?? workType.defName,
                    ["labelShort"] = workType.labelShort ?? workType.label ?? workType.defName,
                    ["pawnLabel"] = workType.pawnLabel ?? "",
                    ["gerundLabel"] = workType.gerundLabel ?? "",
                    ["verb"] = workType.verb ?? "",
                    ["workTags"] = workType.workTags.ToString(),
                    ["naturalPriority"] = workType.naturalPriority,
                    ["visible"] = workType.visible,
                    ["alwaysStartActive"] = workType.alwaysStartActive,
                    ["requireCapableColonist"] = workType.requireCapableColonist,
                    ["disabledForSlaves"] = workType.disabledForSlaves,
                    ["relevantSkills"] = relevantSkills,
                    ["workGiverCount"] = workType.workGiversByPriority?.Count ?? 0,
                    ["description"] = workType.description ?? ""
                });
            }

            return result;
        }

        /// <summary>
        /// 获取角色可用的 WorkGiver 列表（按优先级排序）
        /// </summary>
        public static List<Dictionary<string, object>> GetPawnWorkGivers(Pawn pawn)
        {
            if (pawn == null || pawn.workSettings == null)
                return new List<Dictionary<string, object>>();

            var result = new List<Dictionary<string, object>>();
            var settings = pawn.workSettings;

            // 正常工作顺序
            try
            {
                foreach (var workGiver in settings.WorkGiversInOrderNormal)
                {
                    if (workGiver?.def == null) continue;

                    result.Add(new Dictionary<string, object>
                    {
                        ["defName"] = workGiver.def.defName,
                        ["label"] = workGiver.def.label ?? workGiver.def.defName,
                        ["gerund"] = workGiver.def.gerund ?? "",
                        ["workType"] = workGiver.def.workType?.defName ?? "Unknown",
                        ["workTypeLabel"] = workGiver.def.workType?.label ?? "Unknown",
                        ["priority"] = settings.GetPriority(workGiver.def.workType),
                        ["priorityInType"] = workGiver.def.priorityInType,
                        ["emergency"] = workGiver.def.emergency
                    });
                }
            }
            catch { }

            return result;
        }

        #endregion

        #region Haul 搬运系统 - 基于 ListerHaulables API

        /// <summary>
        /// 获取所有需要搬运的物品（不同于 GetHaulableThings，这个只返回当前需要搬运的）
        /// </summary>
        public static List<Thing> GetThingsNeedingHauling(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.listerHaulables?.ThingsPotentiallyNeedingHauling()?.ToList();
        }

        /// <summary>
        /// 获取需要搬运物品的统计信息
        /// </summary>
        public static Dictionary<string, object> GetHaulablesOverview(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.listerHaulables == null)
                return new Dictionary<string, object> { ["count"] = 0, ["items"] = new List<object>() };

            var haulables = map.listerHaulables.ThingsPotentiallyNeedingHauling();
            if (haulables == null || haulables.Count == 0)
                return new Dictionary<string, object> { ["count"] = 0, ["items"] = new List<object>() };

            // 按类型分组统计
            var byDef = new Dictionary<string, int>();
            var byCategory = new Dictionary<string, int>();
            int totalStackCount = 0;

            foreach (var thing in haulables)
            {
                if (thing == null) continue;

                string defName = thing.def?.defName ?? "Unknown";
                if (!byDef.ContainsKey(defName)) byDef[defName] = 0;
                byDef[defName] += thing.stackCount;
                totalStackCount += thing.stackCount;

                string category = (thing.def != null) ? thing.def.category.ToString() : "Unknown";
                if (!byCategory.ContainsKey(category)) byCategory[category] = 0;
                byCategory[category]++;
            }

            // 取前20个物品作为示例
            var sampleItems = haulables
                .Take(20)
                .Select(t => new Dictionary<string, object>
                {
                    ["id"] = t.thingIDNumber,
                    ["defName"] = t.def?.defName ?? "Unknown",
                    ["label"] = t.Label ?? "Unknown",
                    ["stackCount"] = t.stackCount,
                    ["position"] = new { x = t.Position.x, z = t.Position.z },
                    ["isForbidden"] = t.IsForbidden(Faction.OfPlayer)
                })
                .ToList();

            return new Dictionary<string, object>
            {
                ["count"] = haulables.Count,
                ["totalStackCount"] = totalStackCount,
                ["byDef"] = byDef.OrderByDescending(kvp => kvp.Value).Take(20).ToDictionary(k => k.Key, v => v.Value),
                ["byCategory"] = byCategory,
                ["sampleItems"] = sampleItems
            };
        }

        /// <summary>
        /// 为角色分配搬运任务
        /// </summary>
        public static bool AssignHaulJob(Pawn pawn, Thing thing)
        {
            if (pawn == null || thing == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 检查物品是否可搬运
                if (!thing.def.EverHaulable)
                    return false;

                // 创建搬运工作
                var job = HaulAIUtility.HaulToStorageJob(pawn, thing, true);
                if (job == null)
                    return false;

                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为角色分配建造任务（针对蓝图/框架）
        /// </summary>
        public static bool AssignBuildJob(Pawn pawn, Thing blueprintOrFrame)
        {
            if (pawn == null || blueprintOrFrame == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 检查是否是蓝图或框架
                if (!(blueprintOrFrame is Blueprint) && !(blueprintOrFrame is Frame))
                    return false;

                // 使用 WorkGiver_Scanner 的通用接口创建 Job
                // 遍历 Construction 相关的 WorkGiver
                foreach (var workGiver in pawn.workSettings.WorkGiversInOrderNormal)
                {
                    if (workGiver is WorkGiver_Scanner scanner)
                    {
                        // 检查 WorkGiver 是否能处理这个目标
                        if (scanner.HasJobOnThing(pawn, blueprintOrFrame, forced: true))
                        {
                            var job = scanner.JobOnThing(pawn, blueprintOrFrame, forced: true);
                            if (job != null)
                            {
                                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为角色分配维修任务
        /// </summary>
        public static bool AssignRepairJob(Pawn pawn, Building building)
        {
            if (pawn == null || building == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 检查是否需要维修
                if (building.HitPoints >= building.MaxHitPoints)
                    return false;

                // 检查是否可以维修
                if (!RepairUtility.PawnCanRepairNow(pawn, building))
                    return false;

                // 创建维修工作
                var job = JobMaker.MakeJob(JobDefOf.Repair, building);
                if (job == null)
                    return false;

                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为角色分配清洁任务
        /// </summary>
        public static bool AssignCleanJob(Pawn pawn, Thing filth)
        {
            if (pawn == null || filth == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 检查是否是污垢
                if (!(filth is Filth))
                    return false;

                // 创建清洁工作
                var job = JobMaker.MakeJob(JobDefOf.Clean);
                if (job == null)
                    return false;

                job.AddQueuedTarget(TargetIndex.A, filth);
                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为角色分配采矿任务
        /// </summary>
        public static bool AssignMineJob(Pawn pawn, Thing mineable)
        {
            if (pawn == null || mineable == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 使用 MineAIUtility 创建采矿工作
                var job = MineAIUtility.JobOnThing(pawn, mineable, forced: true);
                if (job == null)
                    return false;

                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为角色分配收割任务
        /// </summary>
        public static bool AssignHarvestJob(Pawn pawn, Plant plant)
        {
            if (pawn == null || plant == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 检查植物是否可收割
                if (!plant.HarvestableNow)
                    return false;

                // 创建收割工作
                var job = JobMaker.MakeJob(JobDefOf.Harvest);
                if (job == null)
                    return false;

                job.AddQueuedTarget(TargetIndex.A, plant);
                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为角色分配切割植物任务
        /// </summary>
        public static bool AssignCutPlantJob(Pawn pawn, Plant plant)
        {
            if (pawn == null || plant == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 创建切割工作
                var job = JobMaker.MakeJob(JobDefOf.CutPlant, plant);
                if (job == null)
                    return false;

                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 为角色分配播种任务
        /// </summary>
        public static bool AssignSowJob(Pawn pawn, IntVec3 cell, ThingDef plantDef)
        {
            if (pawn == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 创建播种工作
                var job = JobMaker.MakeJob(JobDefOf.Sow, cell);
                if (job == null)
                    return false;

                job.plantDefToSow = plantDef;
                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 通用任务分配 - 根据 Thing 自动匹配合适的工作
        /// </summary>
        public static bool AssignJobOnThing(Pawn pawn, Thing thing, bool forced = true)
        {
            if (pawn == null || thing == null)
                return false;

            if (pawn.workSettings == null || !pawn.workSettings.EverWork)
                return false;

            try
            {
                // 遍历角色可用的所有 WorkGiver
                foreach (var workGiver in pawn.workSettings.WorkGiversInOrderNormal)
                {
                    if (workGiver is WorkGiver_Scanner scanner)
                    {
                        try
                        {
                            if (scanner.HasJobOnThing(pawn, thing, forced))
                            {
                                var job = scanner.JobOnThing(pawn, thing, forced);
                                if (job != null)
                                {
                                    pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                                    return true;
                                }
                            }
                        }
                        catch
                        {
                            // 继续尝试下一个 WorkGiver
                            continue;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 蓝图管理系统 - Blueprint Management

        /// <summary>
        /// 获取所有待建造的蓝图列表（详细信息）
        /// </summary>
        public static Dictionary<string, object> GetBlueprintsInfo(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
                return new Dictionary<string, object> { ["error"] = "No map available" };

            var blueprints = new List<Dictionary<string, object>>();
            int totalCount = 0;
            int totalWorkLeft = 0;

            // 遍历地图上的所有 Blueprint
            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing is Blueprint blueprint)
                {
                    try
                    {
                        var blueprintInfo = new Dictionary<string, object>
                        {
                            ["id"] = thing.thingIDNumber,
                            ["defName"] = thing.def?.defName ?? "Unknown",
                            ["label"] = thing.Label ?? thing.def?.label ?? "Unknown",
                            ["position"] = new { x = thing.Position.x, z = thing.Position.z },
                            ["rotation"] = thing.Rotation.AsInt,
                            ["type"] = blueprint is Blueprint_Install ? "Install" :
                                      blueprint is Blueprint_Build ? "Build" : "Unknown"
                        };

                        // 获取建筑定义
                        var buildableDef = blueprint.def;
                        if (buildableDef is ThingDef thingDef)
                        {
                            blueprintInfo["buildingDef"] = thingDef.defName;
                            blueprintInfo["size"] = new { x = thingDef.size.x, z = thingDef.size.z };

                            // 获取材料需求
                            var costList = thingDef.CostList;
                            if (costList != null)
                            {
                                var materials = new List<Dictionary<string, object>>();
                                foreach (var cost in costList)
                                {
                                    materials.Add(new Dictionary<string, object>
                                    {
                                        ["defName"] = cost.thingDef?.defName ?? "Unknown",
                                        ["label"] = cost.thingDef?.label ?? "Unknown",
                                        ["count"] = cost.count
                                    });
                                }
                                blueprintInfo["materialsNeeded"] = materials;
                            }
                        }

                        // 获取材料 Stuff（如果有）
                        if (thing is Blueprint_Build bpBuild && bpBuild.stuffToUse != null)
                        {
                            blueprintInfo["stuffDef"] = bpBuild.stuffToUse.defName;
                        }

                        blueprints.Add(blueprintInfo);
                        totalCount++;
                    }
                    catch { }
                }

                // Frame（正在建造中的框架）
                if (thing is Frame frame)
                {
                    try
                    {
                        var frameInfo = new Dictionary<string, object>
                        {
                            ["id"] = thing.thingIDNumber,
                            ["defName"] = thing.def?.defName ?? "Unknown",
                            ["label"] = thing.Label ?? thing.def?.label ?? "Unknown",
                            ["position"] = new { x = thing.Position.x, z = thing.Position.z },
                            ["type"] = "Frame",
                            ["workLeft"] = frame.WorkLeft,
                            ["progressPercent"] = Math.Round((1f - frame.WorkLeft / 100f) * 100, 1)  // 简化计算
                        };

                        // 获取材料需求
                        var costList = frame.def?.CostList;
                        if (costList != null)
                        {
                            var materials = new List<Dictionary<string, object>>();
                            foreach (var cost in costList)
                            {
                                materials.Add(new Dictionary<string, object>
                                {
                                    ["defName"] = cost.thingDef?.defName ?? "Unknown",
                                    ["label"] = cost.thingDef?.label ?? "Unknown",
                                    ["count"] = cost.count
                                });
                            }
                            frameInfo["materialsNeeded"] = materials;
                        }

                        // 获取已投入的材料
                        var containedResources = new List<Dictionary<string, object>>();
                        if (frame.resourceContainer != null)
                        {
                            foreach (var resource in frame.resourceContainer)
                            {
                                if (resource != null)
                                {
                                    containedResources.Add(new Dictionary<string, object>
                                    {
                                        ["defName"] = resource.def?.defName,
                                        ["label"] = resource.Label,
                                        ["count"] = resource.stackCount
                                    });
                                }
                            }
                        }
                        frameInfo["materialsContained"] = containedResources;

                        blueprints.Add(frameInfo);
                        totalCount++;
                        totalWorkLeft += (int)frame.WorkLeft;
                    }
                    catch { }
                }
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["blueprintCount"] = blueprints.Count(b => b["type"]?.ToString() != "Frame"),
                ["frameCount"] = blueprints.Count(b => b["type"]?.ToString() == "Frame"),
                ["totalCount"] = totalCount,
                ["totalWorkLeft"] = totalWorkLeft,
                ["blueprints"] = blueprints
            };
        }

        /// <summary>
        /// 取消指定的建造蓝图
        /// </summary>
        public static Dictionary<string, object> CancelBlueprint(int blueprintId, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
                return new Dictionary<string, object> { ["error"] = "No map available" };

            // 查找蓝图
            foreach (var thing in map.listerThings.AllThings)
            {
                if (thing is Blueprint blueprint && thing.thingIDNumber == blueprintId)
                {
                    try
                    {
                        string label = thing.Label ?? thing.def?.label ?? "Unknown";
                        var position = new { x = thing.Position.x, z = thing.Position.z };

                        // 删除蓝图 - 退还材料
                        blueprint.Destroy(DestroyMode.Cancel);

                        return new Dictionary<string, object>
                        {
                            ["success"] = true,
                            ["message"] = $"已取消蓝图: {label}",
                            ["cancelledBlueprint"] = new
                            {
                                id = blueprintId,
                                label = label,
                                position = position
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        return new Dictionary<string, object> { ["error"] = $"Failed to cancel blueprint: {ex.Message}" };
                    }
                }

                // Frame 也可以取消
                if (thing is Frame frame && thing.thingIDNumber == blueprintId)
                {
                    try
                    {
                        string label = thing.Label ?? thing.def?.label ?? "Unknown";
                        var position = new { x = thing.Position.x, z = thing.Position.z };
                        float workLost = frame.WorkLeft;

                        // 删除 Frame - 退还材料
                        frame.Destroy(DestroyMode.Cancel);

                        return new Dictionary<string, object>
                        {
                            ["success"] = true,
                            ["message"] = $"已取消建造: {label}",
                            ["cancelledFrame"] = new
                            {
                                id = blueprintId,
                                label = label,
                                position = position,
                                workLost = workLost
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        return new Dictionary<string, object> { ["error"] = $"Failed to cancel frame: {ex.Message}" };
                    }
                }
            }

            return new Dictionary<string, object> { ["error"] = $"Blueprint with id {blueprintId} not found" };
        }

        /// <summary>
        /// 获取可建造的建筑定义列表
        /// </summary>
        public static Dictionary<string, object> GetBuildableDefs(string category = null)
        {
            var buildables = new List<Dictionary<string, object>>();

            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                // 只返回可建造的建筑
                if (def.category != ThingCategory.Building)
                    continue;

                // 必须可研究或已解锁
                if (def.researchPrerequisites != null && def.researchPrerequisites.Any(r => !r.IsFinished))
                    continue;

                // 过滤类别
                if (!string.IsNullOrEmpty(category))
                {
                    string defCategory = GetBuildingCategory(def);
                    if (defCategory != category.ToLower())
                        continue;
                }

                try
                {
                    var buildableInfo = new Dictionary<string, object>
                    {
                        ["defName"] = def.defName,
                        ["label"] = def.label ?? def.defName,
                        ["description"] = def.description ?? "",
                        ["category"] = GetBuildingCategory(def),
                        ["size"] = new { x = def.size.x, z = def.size.z },
                        ["rotatable"] = def.rotatable,
                        ["minifiable"] = def.Minifiable
                    };

                    // 获取建造成本
                    if (def.CostList != null && def.CostList.Count > 0)
                    {
                        var costs = new List<Dictionary<string, object>>();
                        foreach (var cost in def.CostList)
                        {
                            costs.Add(new Dictionary<string, object>
                            {
                                ["thingDef"] = cost.thingDef?.defName,
                                ["label"] = cost.thingDef?.label ?? cost.thingDef?.defName,
                                ["count"] = cost.count
                            });
                        }
                        buildableInfo["costList"] = costs;
                    }

                    // 获取建造工作量（使用 CostList 计算或默认值）
                    float workAmount = 0f;
                    if (def.CostList != null)
                    {
                        // 简单估算：每个材料10工作量
                        foreach (var cost in def.CostList)
                            workAmount += cost.count * 10f;
                    }
                    buildableInfo["workToBuild"] = workAmount;

                    // 获取可选的材料
                    if (def.stuffCategories != null && def.stuffCategories.Count > 0)
                    {
                        buildableInfo["stuffCategories"] = def.stuffCategories.Select(s => s.defName).ToList();
                    }

                    // 标签信息
                    if (def.building != null)
                    {
                        buildableInfo["buildingTags"] = def.building.buildingTags;
                    }

                    buildables.Add(buildableInfo);
                }
                catch { }
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["count"] = buildables.Count,
                ["category"] = category ?? "all",
                ["buildables"] = buildables
            };
        }

        /// <summary>
        /// 获取建筑的类别
        /// </summary>
        private static string GetBuildingCategory(ThingDef def)
        {
            if (def == null) return "other";

            // 检查建造标签
            if (def.building != null)
            {
                var tags = def.building.buildingTags;

                if (tags != null)
                {
                    if (tags.Contains("Production")) return "production";
                    if (tags.Contains("Power")) return "power";
                    if (tags.Contains("Defense")) return "defense";
                }
            }

            // 根据 defName 或 label 判断
            string defName = def.defName.ToLower();
            string label = (def.label ?? "").ToLower();

            // 家具
            if (defName.Contains("bed") || defName.Contains("chair") || defName.Contains("table") ||
                defName.Contains("stool") || defName.Contains("armchair") || label.Contains("床") ||
                label.Contains("椅") || label.Contains("桌"))
                return "furniture";

            // 电力
            if (defName.Contains("power") || defName.Contains("generator") || defName.Contains("battery") ||
                defName.Contains("solar") || defName.Contains("wind") || defName.Contains("wire"))
                return "power";

            // 生产
            if (defName.Contains("table") || defName.Contains("bench") || defName.Contains("stove") ||
                defName.Contains("furnace") || defName.Contains("machinetable") || label.Contains("工作台") ||
                label.Contains("灶"))
                return "production";

            // 防御
            if (defName.Contains("turret") || defName.Contains("trap") || defName.Contains("sandbag") ||
                defName.Contains("wall") || defName.Contains("barricade") || label.Contains("炮") ||
                label.Contains("陷阱") || label.Contains("墙"))
                return "defense";

            // 储存
            if (defName.Contains("shelf") || defName.Contains("storage") || label.Contains("架") ||
                label.Contains("储存"))
                return "storage";

            // 种植
            if (defName.Contains("plant") || defName.Contains("pot") || defName.Contains("hydroponics"))
                return "agriculture";

            return "other";
        }

        #endregion

        #region Plan 建造计划系统 - 基于 PlanManager API

        /// <summary>
        /// 获取所有建造计划
        /// </summary>
        public static List<Plan> GetAllPlans(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            return map?.planManager?.AllPlans;
        }

        #endregion

        #region AvailableWork 可分配工作系统

        /// <summary>
        /// 获取角色可分配的工作
        /// </summary>
        public static Dictionary<string, object> GetAvailableWorkForPawn(Pawn pawn)
        {
            if (pawn == null)
                return new Dictionary<string, object> { ["error"] = "Pawn is null" };

            var result = new Dictionary<string, object>
            {
                ["pawnId"] = pawn.thingIDNumber,
                ["pawnName"] = pawn.LabelShort
            };

            var availableJobs = new List<Dictionary<string, object>>();

            try
            {
                // 获取角色的 WorkGiver 列表
                var workGivers = pawn.workSettings?.WorkGiversInOrderNormal;
                if (workGivers == null)
                {
                    result["availableJobs"] = availableJobs;
                    return result;
                }

                var map = pawn.Map;
                if (map == null)
                {
                    result["availableJobs"] = availableJobs;
                    return result;
                }

                foreach (var workGiver in workGivers)
                {
                    if (workGiver?.def == null) continue;
                    if (workGiver is WorkGiver_Scanner scanner)
                    {
                        try
                        {
                            // 检查是否有可用工作
                            var things = scanner.PotentialWorkThingsGlobal(pawn);
                            int thingCount = 0;
                            var sampleTargets = new List<Dictionary<string, object>>();

                            if (things != null)
                            {
                                foreach (var thing in things)
                                {
                                    if (thing == null) continue;
                                    if (scanner.HasJobOnThing(pawn, thing))
                                    {
                                        thingCount++;
                                        if (sampleTargets.Count < 5)
                                        {
                                            sampleTargets.Add(new Dictionary<string, object>
                                            {
                                                ["thingId"] = thing.thingIDNumber,
                                                ["label"] = thing.Label ?? thing.def?.label ?? "Unknown",
                                                ["defName"] = thing.def?.defName ?? "Unknown",
                                                ["position"] = new { x = thing.Position.x, z = thing.Position.z }
                                            });
                                        }
                                    }
                                }
                            }

                            // 检查格子上是否有工作
                            int cellCount = 0;
                            try
                            {
                                var cells = scanner.PotentialWorkCellsGlobal(pawn);
                                if (cells != null)
                                {
                                    foreach (var cell in cells.Take(100)) // 限制检查数量
                                    {
                                        if (scanner.HasJobOnCell(pawn, cell))
                                            cellCount++;
                                    }
                                }
                            }
                            catch { }

                            if (thingCount > 0 || cellCount > 0)
                            {
                                availableJobs.Add(new Dictionary<string, object>
                                {
                                    ["workGiverDef"] = workGiver.def.defName,
                                    ["label"] = workGiver.def.label ?? workGiver.def.defName,
                                    ["gerund"] = workGiver.def.gerund ?? "",
                                    ["workType"] = workGiver.def.workType?.defName ?? "Unknown",
                                    ["workTypeLabel"] = workGiver.def.workType?.label ?? "Unknown",
                                    ["thingCount"] = thingCount,
                                    ["cellCount"] = cellCount,
                                    ["total"] = thingCount + cellCount,
                                    ["sampleTargets"] = sampleTargets
                                });
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            result["availableJobs"] = availableJobs.OrderByDescending(j => (int)j["total"]).ToList();
            result["jobTypeCount"] = availableJobs.Count;
            result["totalJobs"] = availableJobs.Sum(j => (int)j["total"]);

            return result;
        }

        #endregion

        #region 植物分类查询

        /// <summary>
        /// 获取所有植物类型列表（按defName分组）
        /// 返回每种植物的统计信息
        /// </summary>
        public static Dictionary<string, object> GetAllPlantTypes(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);
            if (allPlants == null || allPlants.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalTypes"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var thing in allPlants)
            {
                if (!(thing is Plant plant)) continue;

                var defName = plant.def?.defName ?? "Unknown";
                var label = plant.def?.label ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    // 判断植物大类
                    string category = GetPlantCategoryName(plant);

                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = label,
                        ["category"] = category,          // crop/tree/wildHarvestable/wildPlant
                        ["count"] = 0,
                        ["harvestableNow"] = 0,
                        ["blighted"] = 0,
                        ["avgGrowth"] = 0f
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                if (plant.HarvestableNow)
                    types[defName]["harvestableNow"] = (int)types[defName]["harvestableNow"] + 1;
                if (plant.Blighted)
                    types[defName]["blighted"] = (int)types[defName]["blighted"] + 1;
            }

            // 计算平均生长度
            foreach (var kvp in types.ToList())
            {
                var plantsOfType = allPlants.OfType<Plant>()
                    .Where(p => p.def?.defName == kvp.Key)
                    .ToList();
                if (plantsOfType.Count > 0)
                {
                    kvp.Value["avgGrowth"] = plantsOfType.Average(p => p.Growth);
                }
            }

            return new Dictionary<string, object>
            {
                ["totalTypes"] = types.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取植物分类名称
        /// </summary>
        private static string GetPlantCategoryName(Plant plant)
        {
            if (plant == null) return "unknown";

            var plantDef = plant.def?.plant;
            if (plantDef == null) return "unknown";

            // 树木
            if (plantDef.IsTree)
                return "tree";

            // 农作物 (可种植)
            if (plantDef.Sowable)
                return "crop";

            // 野生可收获
            if (plantDef.Harvestable)
                return "wildHarvestable";

            // 普通野生植物
            return "wildPlant";
        }

        /// <summary>
        /// 按具体defName获取植物统计
        /// </summary>
        public static Dictionary<string, object> GetPlantsByDefName(string defName, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var matchedPlants = allPlants?.OfType<Plant>()
                .Where(p => p.def?.defName == defName)
                .ToList() ?? new List<Plant>();

            if (matchedPlants.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["defName"] = defName,
                    ["count"] = 0
                };
            }

            // 获取分类
            var firstPlant = matchedPlants.First();
            string category = GetPlantCategoryName(firstPlant);

            return new Dictionary<string, object>
            {
                ["defName"] = defName,
                ["label"] = firstPlant.def?.label,
                ["category"] = category,
                ["count"] = matchedPlants.Count,
                ["harvestableNow"] = matchedPlants.Count(p => p.HarvestableNow),
                ["blighted"] = matchedPlants.Count(p => p.Blighted),
                ["avgGrowth"] = matchedPlants.Average(p => p.Growth)
            };
        }

        /// <summary>
        /// 获取农作物类型列表（所有可种植的植物）
        /// </summary>
        public static Dictionary<string, object> GetCropTypes(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var crops = allPlants?.OfType<Plant>()
                .Where(p => p.def?.plant?.Sowable == true)
                .ToList() ?? new List<Plant>();

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var crop in crops)
            {
                var defName = crop.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = crop.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["harvestableNow"] = 0,
                        ["blighted"] = 0,
                        ["avgGrowth"] = 0f,
                        ["growDays"] = crop.def?.plant?.growDays ?? 0,
                        ["harvestYield"] = crop.def?.plant?.harvestYield ?? 0,
                        ["harvestedThingDef"] = crop.def?.plant?.harvestedThingDef?.defName
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                if (crop.HarvestableNow)
                    types[defName]["harvestableNow"] = (int)types[defName]["harvestableNow"] + 1;
                if (crop.Blighted)
                    types[defName]["blighted"] = (int)types[defName]["blighted"] + 1;
            }

            // 计算平均生长度
            foreach (var kvp in types.ToList())
            {
                var plantsOfType = crops.Where(p => p.def?.defName == kvp.Key).ToList();
                if (plantsOfType.Count > 0)
                {
                    kvp.Value["avgGrowth"] = plantsOfType.Average(p => p.Growth);
                }
            }

            return new Dictionary<string, object>
            {
                ["totalTypes"] = types.Count,
                ["totalCount"] = crops.Count,
                ["totalHarvestableNow"] = crops.Count(p => p.HarvestableNow),
                ["totalBlighted"] = crops.Count(p => p.Blighted),
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取树木类型列表
        /// </summary>
        public static Dictionary<string, object> GetTreeTypes(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var trees = allPlants?.OfType<Plant>()
                .Where(p => p.def?.plant?.IsTree == true)
                .ToList() ?? new List<Plant>();

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var tree in trees)
            {
                var defName = tree.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = tree.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["harvestableNow"] = 0,
                        ["avgGrowth"] = 0f,
                        ["woodYield"] = tree.def?.plant?.harvestYield ?? 0
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                if (tree.HarvestableNow)
                    types[defName]["harvestableNow"] = (int)types[defName]["harvestableNow"] + 1;
            }

            // 计算平均生长度
            foreach (var kvp in types.ToList())
            {
                var plantsOfType = trees.Where(p => p.def?.defName == kvp.Key).ToList();
                if (plantsOfType.Count > 0)
                {
                    kvp.Value["avgGrowth"] = plantsOfType.Average(p => p.Growth);
                }
            }

            return new Dictionary<string, object>
            {
                ["totalTypes"] = types.Count,
                ["totalCount"] = trees.Count,
                ["totalHarvestableNow"] = trees.Count(p => p.HarvestableNow),
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取野生可收获植物类型列表
        /// </summary>
        public static Dictionary<string, object> GetWildHarvestableTypes(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var wildHarvest = allPlants?.OfType<Plant>()
                .Where(p => p.def?.plant?.Sowable == false && p.def?.plant?.Harvestable == true)
                .ToList() ?? new List<Plant>();

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var plant in wildHarvest)
            {
                var defName = plant.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = plant.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["harvestableNow"] = 0,
                        ["avgGrowth"] = 0f,
                        ["harvestYield"] = plant.def?.plant?.harvestYield ?? 0,
                        ["harvestedThingDef"] = plant.def?.plant?.harvestedThingDef?.defName
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                if (plant.HarvestableNow)
                    types[defName]["harvestableNow"] = (int)types[defName]["harvestableNow"] + 1;
            }

            // 计算平均生长度
            foreach (var kvp in types.ToList())
            {
                var plantsOfType = wildHarvest.Where(p => p.def?.defName == kvp.Key).ToList();
                if (plantsOfType.Count > 0)
                {
                    kvp.Value["avgGrowth"] = plantsOfType.Average(p => p.Growth);
                }
            }

            return new Dictionary<string, object>
            {
                ["totalTypes"] = types.Count,
                ["totalCount"] = wildHarvest.Count,
                ["totalHarvestableNow"] = wildHarvest.Count(p => p.HarvestableNow),
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取普通野生植物类型列表
        /// </summary>
        public static Dictionary<string, object> GetWildPlantTypes(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var wildPlants = allPlants?.OfType<Plant>()
                .Where(p => p.def?.plant?.Sowable == false && p.def?.plant?.Harvestable == false && p.def?.plant?.IsTree == false)
                .ToList() ?? new List<Plant>();

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var plant in wildPlants)
            {
                var defName = plant.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = plant.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["avgGrowth"] = 0f
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
            }

            // 计算平均生长度
            foreach (var kvp in types.ToList())
            {
                var plantsOfType = wildPlants.Where(p => p.def?.defName == kvp.Key).ToList();
                if (plantsOfType.Count > 0)
                {
                    kvp.Value["avgGrowth"] = plantsOfType.Average(p => p.Growth);
                }
            }

            return new Dictionary<string, object>
            {
                ["totalTypes"] = types.Count,
                ["totalCount"] = wildPlants.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取所有树木统计信息（按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetTreesDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var trees = allPlants?.OfType<Plant>()
                .Where(p => p.def?.plant?.IsTree == true)
                .ToList() ?? new List<Plant>();

            if (trees.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["totalHarvestable"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var tree in trees)
            {
                var defName = tree.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = tree.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["harvestableNow"] = 0,
                        ["woodYield"] = tree.def?.plant?.harvestYield ?? 0
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                if (tree.HarvestableNow)
                    types[defName]["harvestableNow"] = (int)types[defName]["harvestableNow"] + 1;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = trees.Count,
                ["totalHarvestable"] = trees.Count(t => t.HarvestableNow),
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取所有农作物统计信息（按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetCropsDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var crops = allPlants?.OfType<Plant>()
                .Where(p => p.def?.plant?.Sowable == true)
                .ToList() ?? new List<Plant>();

            if (crops.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["totalHarvestable"] = 0,
                    ["totalBlighted"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var crop in crops)
            {
                var defName = crop.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = crop.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["harvestableNow"] = 0,
                        ["blighted"] = 0,
                        ["growDays"] = crop.def?.plant?.growDays ?? 0,
                        ["harvestYield"] = crop.def?.plant?.harvestYield ?? 0,
                        ["harvestedThingDef"] = crop.def?.plant?.harvestedThingDef?.defName,
                        ["harvestedThingLabel"] = crop.def?.plant?.harvestedThingDef?.label
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                if (crop.HarvestableNow)
                    types[defName]["harvestableNow"] = (int)types[defName]["harvestableNow"] + 1;
                if (crop.Blighted)
                    types[defName]["blighted"] = (int)types[defName]["blighted"] + 1;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = crops.Count,
                ["totalHarvestable"] = crops.Count(c => c.HarvestableNow),
                ["totalBlighted"] = crops.Count(c => c.Blighted),
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取野生可收获植物统计信息（按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetWildHarvestableDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allPlants = GetAllPlants(map);

            var wildHarvest = allPlants?.OfType<Plant>()
                .Where(p => p.def?.plant?.Sowable == false && p.def?.plant?.Harvestable == true)
                .ToList() ?? new List<Plant>();

            if (wildHarvest.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["totalHarvestable"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var plant in wildHarvest)
            {
                var defName = plant.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = plant.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["harvestableNow"] = 0,
                        ["harvestYield"] = plant.def?.plant?.harvestYield ?? 0,
                        ["harvestedThingDef"] = plant.def?.plant?.harvestedThingDef?.defName,
                        ["harvestedThingLabel"] = plant.def?.plant?.harvestedThingDef?.label
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                if (plant.HarvestableNow)
                    types[defName]["harvestableNow"] = (int)types[defName]["harvestableNow"] + 1;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = wildHarvest.Count,
                ["totalHarvestable"] = wildHarvest.Count(p => p.HarvestableNow),
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        #endregion

        #region 物品分类查询

        /// <summary>
        /// 获取所有食物（按defName细分，包含每个物品的位置和数量）
        /// </summary>
        public static Dictionary<string, object> GetFoodDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var foodSources = map?.listerThings?.ThingsInGroup(ThingRequestGroup.FoodSource);

            if (foodSources == null || foodSources.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var thing in foodSources)
            {
                var defName = thing.def?.defName ?? "Unknown";
                if (defName.StartsWith("Plant_")) continue; // 跳过植物类食物源

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = thing.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["totalStackCount"] = 0,
                        ["nutrition"] = thing.GetStatValue(StatDefOf.Nutrition)
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                types[defName]["totalStackCount"] = (int)types[defName]["totalStackCount"] + thing.stackCount;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = foodSources.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取所有武器统计信息（按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetWeaponsDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var weapons = map?.listerThings?.ThingsInGroup(ThingRequestGroup.Weapon);

            if (weapons == null || weapons.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var thing in weapons)
            {
                var defName = thing.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = thing.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["totalStackCount"] = 0
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                types[defName]["totalStackCount"] = (int)types[defName]["totalStackCount"] + thing.stackCount;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = weapons.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取所有衣物统计信息（按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetApparelDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var apparels = map?.listerThings?.ThingsInGroup(ThingRequestGroup.Apparel);

            if (apparels == null || apparels.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var thing in apparels)
            {
                var defName = thing.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = thing.def?.label ?? "Unknown",
                        ["count"] = 0
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = apparels.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取所有药品统计信息（按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetMedicineDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var medicines = map?.listerThings?.ThingsInGroup(ThingRequestGroup.Medicine);

            if (medicines == null || medicines.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var thing in medicines)
            {
                var defName = thing.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = thing.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["totalStackCount"] = 0,
                        ["medicalPotency"] = thing.def?.statBases?.FirstOrDefault(s => s.stat == StatDefOf.MedicalPotency)?.value ?? 0
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                types[defName]["totalStackCount"] = (int)types[defName]["totalStackCount"] + thing.stackCount;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = medicines.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 获取所有材料类物品统计信息（按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetMaterialsDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var haulables = map?.listerThings?.ThingsInGroup(ThingRequestGroup.HaulableEver);

            if (haulables == null || haulables.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            // 筛选材料类（非食物、非武器、非衣物、非药品）
            var materials = haulables.Where(t =>
                t.def?.category == ThingCategory.Item &&
                t.def?.ingestible == null &&
                !t.def.IsWeapon &&
                t.def?.apparel == null &&
                !t.def.IsMedicine
            ).ToList();

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var thing in materials)
            {
                var defName = thing.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = thing.def?.label ?? "Unknown",
                        ["count"] = 0,
                        ["totalStackCount"] = 0,
                        ["stackLimit"] = thing.def?.stackLimit ?? 1
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
                types[defName]["totalStackCount"] = (int)types[defName]["totalStackCount"] + thing.stackCount;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = materials.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        /// <summary>
        /// 按defName获取特定物品统计
        /// </summary>
        public static Dictionary<string, object> GetItemByDefName(string defName, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var allItems = map?.listerThings?.ThingsInGroup(ThingRequestGroup.HaulableEver);

            var matchedItems = allItems?.Where(t => t.def?.defName == defName).ToList() ?? new List<Thing>();

            if (matchedItems.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["defName"] = defName,
                    ["count"] = 0
                };
            }

            var firstItem = matchedItems.First();

            return new Dictionary<string, object>
            {
                ["defName"] = defName,
                ["label"] = firstItem.def?.label,
                ["count"] = matchedItems.Count,
                ["totalStackCount"] = matchedItems.Sum(t => t.stackCount)
            };
        }

        #endregion

        #region 建筑分类查询

        /// <summary>
        /// 获取所有生产建筑（工作台等，按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetProductionBuildingsDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var buildings = map?.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingArtificial);

            // 筛选有配方的建筑（工作台）
            var productionBuildings = buildings?.Where(b =>
                b is Building building &&
                building.def?.inspectorTabs != null &&
                building.def.inspectorTabs.Any(t => t == typeof(ITab_Bills))
            ).ToList() ?? new List<Thing>();

            return GroupBuildingsByDefName(productionBuildings, "production");
        }

        /// <summary>
        /// 获取所有电力建筑（发电机、电池等，按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetPowerBuildingsDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var buildings = map?.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingArtificial);

            // 筛选有电力组件的建筑
            var powerBuildings = buildings?.Where(b =>
                b is Building building &&
                (building.PowerComp != null || building.def?.HasComp(typeof(CompPower)) == true)
            ).ToList() ?? new List<Thing>();

            return GroupBuildingsByDefName(powerBuildings, "power");
        }

        /// <summary>
        /// 获取所有防御建筑（炮塔、陷阱等，按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetDefenseBuildingsDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var buildings = map?.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingArtificial);

            // 筛选防御建筑（炮塔、沙袋、陷阱等）
            var defenseBuildings = buildings?.Where(b =>
                b is Building building &&
                (building.def?.building?.IsTurret == true ||
                 building.def?.building?.isTrap == true ||
                 building.def?.defName?.Contains("Barricade") == true ||
                 building.def?.defName?.Contains("Sandbags") == true ||
                 building.def?.defName?.Contains("Turret") == true)
            ).ToList() ?? new List<Thing>();

            return GroupBuildingsByDefName(defenseBuildings, "defense");
        }

        /// <summary>
        /// 获取所有储存建筑（货架、储存区等，按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetStorageBuildingsDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var buildings = map?.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingArtificial);

            // 筛选储存建筑
            var storageBuildings = buildings?.Where(b =>
                b is Building building &&
                (building is Building_Storage ||
                 building.def?.building?.fixedStorageSettings != null ||
                 building.def?.defName?.Contains("Shelf") == true)
            ).ToList() ?? new List<Thing>();

            return GroupBuildingsByDefName(storageBuildings, "storage");
        }

        /// <summary>
        /// 获取所有家具（床、椅子、桌子等，按defName细分）
        /// </summary>
        public static Dictionary<string, object> GetFurnitureDetailed(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var buildings = map?.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingArtificial);

            // 筛选家具（床、椅子、桌子等）
            var furniture = buildings?.Where(b =>
                b is Building building &&
                (building.def?.building?.isSittable == true ||
                 building is Building_Bed ||
                 building.def?.defName?.Contains("Table") == true ||
                 building.def?.defName?.Contains("Chair") == true ||
                 building.def?.defName?.Contains("Stool") == true ||
                 building.def?.defName?.Contains("Bed") == true)
            ).ToList() ?? new List<Thing>();

            return GroupBuildingsByDefName(furniture, "furniture");
        }

        /// <summary>
        /// 按defName获取特定建筑统计
        /// </summary>
        public static Dictionary<string, object> GetBuildingByDefName(string defName, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            var buildings = map?.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingArtificial);

            var matchedBuildings = buildings?.Where(b => b.def?.defName == defName).ToList() ?? new List<Thing>();

            if (matchedBuildings.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["defName"] = defName,
                    ["count"] = 0
                };
            }

            var firstBuilding = matchedBuildings.First();

            return new Dictionary<string, object>
            {
                ["defName"] = defName,
                ["label"] = firstBuilding.def?.label,
                ["count"] = matchedBuildings.Count
            };
        }

        /// <summary>
        /// 辅助函数：按defName分组建筑（仅统计）
        /// </summary>
        private static Dictionary<string, object> GroupBuildingsByDefName(List<Thing> buildings, string category)
        {
            if (buildings.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["totalCount"] = 0,
                    ["types"] = new Dictionary<string, object>()
                };
            }

            var types = new Dictionary<string, Dictionary<string, object>>();

            foreach (var building in buildings)
            {
                var defName = building.def?.defName ?? "Unknown";

                if (!types.ContainsKey(defName))
                {
                    types[defName] = new Dictionary<string, object>
                    {
                        ["defName"] = defName,
                        ["label"] = building.def?.label ?? "Unknown",
                        ["category"] = category,
                        ["count"] = 0
                    };
                }

                types[defName]["count"] = (int)types[defName]["count"] + 1;
            }

            return new Dictionary<string, object>
            {
                ["totalCount"] = buildings.Count,
                ["types"] = types.Values.ToList()  // 转换为列表以正确序列化
            };
        }

        #endregion

        #region 调试

        /// <summary>
        /// 打印所有 Pawn 列表 (调试用)
        /// 对应: map.mapPawns.LogListedPawns()
        /// </summary>
        public static void LogAllPawns(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            map?.mapPawns?.LogListedPawns();
        }

        #endregion

        #region 工作分配

        /// <summary>
        /// 触发指定类型的工作，自动分配空闲殖民者
        /// </summary>
        /// <param name="workType">工作类型: CutPlant, Harvest, Mine, Construct, Repair, Clean, Haul</param>
        /// <param name="map">目标地图，默认当前地图</param>
        /// <returns>分配结果</returns>
        public static Dictionary<string, object> TriggerWork(string workType, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "没有可用地图"
                };
            }

            // 解析工作类型
            WorkTypeDef workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(workType);
            if (workTypeDef == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"未知的工作类型: {workType}",
                    ["supportedTypes"] = new List<string> { "CutPlant", "Harvest", "Mine", "Construct", "Repair", "Clean", "Haul" }
                };
            }

            // 特殊处理：CutPlant 工作需要先标记植物
            Dictionary<string, object> designationResult = null;
            if (workType.Equals("PlantCutting", System.StringComparison.OrdinalIgnoreCase) ||
                workType.Equals("CutPlant", System.StringComparison.OrdinalIgnoreCase))
            {
                designationResult = AutoDesignateTreesForCutting(map);
            }

            // 获取所有已生成的自由殖民者
            List<Pawn> colonists = map.mapPawns?.FreeColonistsSpawned;
            if (colonists == null || colonists.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "没有可用的殖民者"
                };
            }

            List<Dictionary<string, object>> assignedColonists = new List<Dictionary<string, object>>();
            List<string> skipReasons = new List<string>();
            int assignedCount = 0;

            foreach (Pawn pawn in colonists)
            {
                // 检查殖民者是否可以被分配工作
                // 只跳过正在执行重要任务的殖民者（倒地、睡觉、医疗、战斗等）
                // 允许打断"空闲"状态（Wait, GotoWander 等）
                if (!pawn.CanTakeOrder)
                    continue;

                // 检查是否正在执行不可打断的任务
                var curJob = pawn.jobs?.curJob;
                if (curJob != null)
                {
                    // 获取当前任务的 defName
                    string curJobDefName = curJob.def?.defName ?? "";

                    // 这些任务可以被打断（空闲/漫游/等待类）
                    var interruptibleJobs = new HashSet<string>
                    {
                        "Wait", "Wait_Wander", "GotoWander", "Wait_MaintainPosture",
                        "Wait_Combat", "Wait_SafeTemperature", "Wait_Bubble"
                    };

                    // 如果当前任务不在可打断列表中，跳过该殖民者
                    if (!interruptibleJobs.Contains(curJobDefName))
                        continue;
                }

                // 检查殖民者是否支持该工作类型（优先级 > 0）
                int workPriority = pawn.workSettings?.GetPriority(workTypeDef) ?? 0;
                if (workPriority == 0)
                {
                    skipReasons.Add($"{pawn.LabelShort}: 工作优先级为0");
                    continue;
                }

                // 检查殖民者是否有必要的能力
                PawnCapacityDef missingCapacity = workTypeDef.workGiversByPriority?
                    .FirstOrDefault(wg => wg != null)?.Worker?
                    .MissingRequiredCapacity(pawn);
                if (missingCapacity != null)
                {
                    skipReasons.Add($"{pawn.LabelShort}: 缺少能力 {missingCapacity.defName}");
                    continue;
                }

                // 尝试通过 WorkGiver 系统分配工作
                bool jobAssigned = TryAssignWorkThroughWorkGiver(pawn, workTypeDef, map, out string failReason);

                assignedColonists.Add(new Dictionary<string, object>
                {
                    ["pawnId"] = pawn.thingIDNumber,
                    ["name"] = pawn.LabelShort,
                    ["curJob"] = curJob?.def?.defName ?? "None",
                    ["jobAssigned"] = jobAssigned,
                    ["failReason"] = jobAssigned ? null : (failReason ?? "未找到可执行的任务")
                });

                if (jobAssigned)
                    assignedCount++;
            }

            string workTypeDisplayName = GetWorkTypeDisplayName(workType);

            // 生成详细的失败原因汇总
            string detailedMessage = assignedCount > 0
                ? $"已分配 {assignedCount} 个殖民者去{workTypeDisplayName}"
                : $"没有空闲殖民者可分配去{workTypeDisplayName}";

            if (assignedCount == 0 && skipReasons.Count > 0)
            {
                detailedMessage += $"。原因: {string.Join("; ", skipReasons.Take(3))}";
            }

            // 检查工作前置条件
            var preconditionCheck = CheckWorkPreconditions(workType, map);

            var resultData = new Dictionary<string, object>
            {
                ["workType"] = workType,
                ["workTypeDisplay"] = workTypeDisplayName,
                ["assignedColonists"] = assignedColonists,
                ["assignedCount"] = assignedCount,
                ["skipReasons"] = skipReasons,
                ["message"] = detailedMessage,
                ["preconditionCheck"] = preconditionCheck
            };

            // 如果有自动标记结果，添加到返回数据中
            if (designationResult != null)
            {
                resultData["designationResult"] = designationResult;
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["data"] = resultData
            };
        }

        /// <summary>
        /// 自动标记树木砍伐 - 用于 trigger_work(CutPlant) 时自动计算并标记树木
        /// 使用 GenRadial.RadialPattern 高效遍历殖民地附近的树木
        /// </summary>
        private static Dictionary<string, object> AutoDesignateTreesForCutting(Map map, int radius = 40, int maxCount = 30)
        {
            if (map == null) return null;

            try
            {
                // 以殖民地中心为起点
                IntVec3 center = map.Center;
                int numCells = GenRadial.NumCellsInRadius(radius);
                int designatedCount = 0;
                int totalWoodYield = 0;

                // 使用 GenRadial.RadialPattern 高效遍历圆形区域
                for (int i = 0; i < numCells && designatedCount < maxCount; i++)
                {
                    IntVec3 cell = center + GenRadial.RadialPattern[i];

                    // 检查是否在地图边界内
                    if (!cell.InBounds(map))
                        continue;

                    // 获取该格子的植物
                    Plant plant = cell.GetPlant(map);
                    if (plant == null)
                        continue;

                    // 只标记树木
                    if (!plant.def.plant.IsTree)
                        continue;

                    // 检查是否已有砍伐标记
                    if (map.designationManager.DesignationOn(plant, DesignationDefOf.CutPlant) != null)
                        continue;

                    try
                    {
                        // 添加砍伐标记
                        var designation = new Designation(plant, DesignationDefOf.CutPlant);
                        map.designationManager.AddDesignation(designation);

                        designatedCount++;
                        totalWoodYield += (int)(plant.def.plant.harvestYield * plant.Growth);
                    }
                    catch (System.Exception ex)
                    {
                        Verse.Log.Warning($"Failed to designate plant at {cell}: {ex.Message}");
                    }
                }

                if (designatedCount > 0)
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = true,
                        ["designatedCount"] = designatedCount,
                        ["estimatedWoodYield"] = totalWoodYield,
                        ["searchRadius"] = radius,
                        ["message"] = $"自动标记了 {designatedCount} 棵树，预计产出约 {totalWoodYield} 木材"
                    };
                }
                else
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["message"] = $"在半径 {radius} 格内未找到可标记的树木"
                    };
                }
            }
            catch (System.Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"自动标记失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 检查工作类型的前置条件
        /// </summary>
        private static Dictionary<string, object> CheckWorkPreconditions(string workType, Map map)
        {
            var conditions = new Dictionary<string, object>();
            var missing = new List<string>();
            string suggestion = null;

            switch (workType)
            {
                case "Hauling":
                case "Haul":
                    {
                        bool hasStockpile = map.zoneManager?.AllZones?.Any(z => z is Zone_Stockpile) ?? false;
                        int haulableCount = map.listerThings?.ThingsInGroup(ThingRequestGroup.HaulableAlways)?.Count ?? 0;
                        conditions["hasStockpile"] = hasStockpile;
                        conditions["haulableCount"] = haulableCount;
                        if (!hasStockpile) missing.Add("stockpile");
                        if (!hasStockpile) suggestion = "需要先创建储存区: create_zone(type='stockpile', cells=[...])";
                        else if (haulableCount == 0) suggestion = "没有需要搬运的物品";
                    }
                    break;

                case "Growing":
                case "Grow":
                    {
                        var growingZones = map.zoneManager?.AllZones?.OfType<Zone_Growing>()?.ToList();
                        int growingZoneCount = growingZones?.Count ?? 0;
                        int unsetZoneCount = growingZones?.Count(z => z.GetPlantDefToGrow() == null) ?? 0;
                        conditions["growingZoneCount"] = growingZoneCount;
                        conditions["unsetZoneCount"] = unsetZoneCount;
                        if (growingZoneCount == 0) missing.Add("growingZone");
                        if (growingZoneCount > 0 && unsetZoneCount > 0) missing.Add("plantDefNotSet");
                        if (growingZoneCount == 0) suggestion = "需要先创建种植区: create_zone(type='growing', cells=[...])";
                        else if (unsetZoneCount > 0) suggestion = "需要设置种植区的作物: set_growing_zone_plant(zoneId, plantDefName)";
                    }
                    break;

                case "Construct":
                case "Construction":
                    {
                        int blueprintCount = map.listerThings?.ThingsInGroup(ThingRequestGroup.Blueprint)?.Count ?? 0;
                        int frameCount = map.listerThings?.ThingsInGroup(ThingRequestGroup.BuildingFrame)?.Count ?? 0;
                        conditions["blueprintCount"] = blueprintCount;
                        conditions["frameCount"] = frameCount;
                        if (blueprintCount == 0 && frameCount == 0) missing.Add("blueprint");
                        if (blueprintCount == 0 && frameCount == 0) suggestion = "需要先放置建造蓝图: place_blueprint(defName, x, z)";
                    }
                    break;

                case "Mining":
                case "Mine":
                    {
                        // 直接遍历所有格子寻找可挖掘的矿石
                        int mineableCount = 0;
                        foreach (var cell in map.AllCells)
                        {
                            var edifice = cell.GetEdifice(map);
                            if (edifice != null && edifice.def.mineable)
                                mineableCount++;
                        }
                        conditions["mineableCount"] = mineableCount;
                        if (mineableCount == 0)
                        {
                            missing.Add("mineable");
                            suggestion = "没有可挖掘的矿石";
                        }
                    }
                    break;

                case "Cleaning":
                case "Clean":
                    {
                        // 检查是否有污秽
                        var filth = map.listerThings?.ThingsInGroup(ThingRequestGroup.Filth);
                        conditions["filthCount"] = filth?.Count ?? 0;
                    }
                    break;

                case "PlantCutting":
                case "CutPlant":
                    {
                        int plantCount = map.listerThings?.ThingsInGroup(ThingRequestGroup.Plant)?.Count ?? 0;
                        conditions["plantCount"] = plantCount;
                        if (plantCount == 0) missing.Add("plants");
                    }
                    break;

                case "Hunting":
                case "Hunt":
                    {
                        var huntables = map.mapPawns?.AllPawns?.Where(p => p.RaceProps?.Animal == true && !p.Dead && p.Faction != Faction.OfPlayer);
                        conditions["huntableCount"] = huntables?.Count() ?? 0;
                    }
                    break;

                case "Cooking":
                case "Cook":
                    {
                        bool hasCookStation = map.listerBuildings?.allBuildingsColonist?.Any(b => b.def.defName.Contains("Stove") || b.def.defName.Contains("Cook")) ?? false;
                        conditions["hasCookStation"] = hasCookStation;
                        if (!hasCookStation) missing.Add("cookStation");
                    }
                    break;

                case "Research":
                    {
                        bool hasResearchBench = map.listerBuildings?.allBuildingsColonist?.Any(b => b is Building_ResearchBench) ?? false;
                        conditions["hasResearchBench"] = hasResearchBench;
                        if (!hasResearchBench) missing.Add("researchBench");
                    }
                    break;
            }

            return new Dictionary<string, object>
            {
                ["workType"] = workType,
                ["conditions"] = conditions,
                ["missing"] = missing,
                ["canWork"] = missing.Count == 0,
                ["suggestion"] = suggestion
            };
        }

        /// <summary>
        /// 尝试通过 WorkGiver 系统为殖民者分配工作
        /// </summary>
        private static bool TryAssignWorkThroughWorkGiver(Pawn pawn, WorkTypeDef workTypeDef, Map map, out string failReason)
        {
            failReason = null;

            if (workTypeDef.workGiversByPriority == null || workTypeDef.workGiversByPriority.Count == 0)
            {
                failReason = "该工作类型没有可用的 WorkGiver";
                return false;
            }

            foreach (WorkGiverDef workGiverDef in workTypeDef.workGiversByPriority)
            {
                if (workGiverDef == null || workGiverDef.Worker == null)
                    continue;

                WorkGiver workGiver = workGiverDef.Worker;

                // 检查是否应该跳过
                if (workGiver.ShouldSkip(pawn, forced: false))
                {
                    failReason = failReason ?? $"WorkGiver {workGiverDef.defName} 跳过";
                    continue;
                }

                // 尝试 NonScanJob（某些工作类型使用这个）
                Job nonScanJob = workGiver.NonScanJob(pawn);
                if (nonScanJob != null)
                {
                    pawn.jobs.StartJob(nonScanJob, JobCondition.InterruptOptional);
                    return true;
                }

                // 如果是 Scanner 类型的工作
                if (workGiver is WorkGiver_Scanner scanner)
                {
                    // 尝试通过 Thing 请求找工作
                    ThingRequest thingRequest = scanner.PotentialWorkThingRequest;
                    if (thingRequest.group != ThingRequestGroup.Undefined)
                    {
                        List<Thing> things = map.listerThings.ThingsMatching(thingRequest);
                        int thingCount = things?.Count ?? 0;
                        if (thingCount == 0)
                        {
                            failReason = failReason ?? "没有找到可操作的目标物品";
                        }
                        else
                        {
                            foreach (Thing thing in things)
                            {
                                if (thing == null || !scanner.HasJobOnThing(pawn, thing, forced: false))
                                    continue;

                                Job job = scanner.JobOnThing(pawn, thing, forced: false);
                                if (job != null)
                                {
                                    pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                                    return true;
                                }
                            }
                            failReason = failReason ?? $"找到了 {thingCount} 个物品，但都无法执行任务";
                        }
                    }

                    // 尝试通过 Cell 请求找工作
                    IEnumerable<IntVec3> workCells = scanner.PotentialWorkCellsGlobal(pawn);
                    if (workCells != null)
                    {
                        int cellCount = 0;
                        foreach (IntVec3 cell in workCells)
                        {
                            cellCount++;
                            if (!scanner.HasJobOnCell(pawn, cell, forced: false))
                                continue;

                            Job job = scanner.JobOnCell(pawn, cell, forced: false);
                            if (job != null)
                            {
                                pawn.jobs.StartJob(job, JobCondition.InterruptOptional);
                                return true;
                            }
                        }
                        if (cellCount == 0)
                        {
                            failReason = failReason ?? "没有找到可操作的单元格";
                        }
                    }
                }
            }

            failReason = failReason ?? "未找到可执行的任务";
            return false;
        }

        /// <summary>
        /// 获取工作类型的中文显示名称
        /// </summary>
        private static string GetWorkTypeDisplayName(string workType)
        {
            if (workType == "CutPlant") return "砍树/切割植物";
            if (workType == "Harvest") return "收获作物";
            if (workType == "Mine") return "挖矿";
            if (workType == "Construct") return "建造";
            if (workType == "Repair") return "修理";
            if (workType == "Clean") return "清洁";
            if (workType == "Haul") return "搬运";
            if (workType == "Grow") return "种植";
            if (workType == "Research") return "研究";
            if (workType == "Doctor") return "医疗";
            if (workType == "Cook") return "烹饪";
            if (workType == "Hunt") return "狩猎";
            return workType;
        }

        /// <summary>
        /// 获取所有支持的工作类型列表
        /// </summary>
        public static Dictionary<string, object> GetSupportedWorkTypes()
        {
            List<Dictionary<string, object>> workTypes = new List<Dictionary<string, object>>();

            foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs)
            {
                if (def == null || def.workGiversByPriority == null || def.workGiversByPriority.Count == 0)
                    continue;

                workTypes.Add(new Dictionary<string, object>
                {
                    ["defName"] = def.defName,
                    ["label"] = def.labelShort ?? def.label ?? def.defName,
                    ["description"] = def.description ?? "",
                    ["workGiverCount"] = def.workGiversByPriority.Count
                });
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["data"] = new Dictionary<string, object>
                {
                    ["count"] = workTypes.Count,
                    ["workTypes"] = workTypes
                }
            };
        }

        #endregion

        #region 储存系统管理

        /// <summary>
        /// 获取储存区设置详情
        /// </summary>
        public static Dictionary<string, object> GetStorageSettings(int zoneId, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "No map or zone manager available"
                };
            }

            // 查找储存区
            var stockpile = map.zoneManager.AllZones
                .OfType<Zone_Stockpile>()
                .FirstOrDefault(z => z.ID == zoneId);

            if (stockpile == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"Stockpile zone with ID {zoneId} not found"
                };
            }

            var settings = stockpile.settings;
            if (settings == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "Zone has no storage settings"
                };
            }

            var filter = settings.filter;

            // 收集允许的类别
            var allowedCategories = new List<string>();
            var allowedDefs = new List<string>();

            if (filter != null)
            {
                // 获取允许的物品定义
                foreach (var def in filter.AllowedThingDefs)
                {
                    allowedDefs.Add(def.defName);
                }

                // 尝试推断允许的类别（基于允许的物品所属的顶层类别）
                var topCategories = new HashSet<ThingCategoryDef>();
                foreach (var def in filter.AllowedThingDefs)
                {
                    if (def.FirstThingCategory != null)
                    {
                        // 获取顶层类别
                        var cat = def.FirstThingCategory;
                        while (cat.parent != null && cat.parent != ThingCategoryDefOf.Root)
                        {
                            cat = cat.parent;
                        }
                        topCategories.Add(cat);
                    }
                }
                allowedCategories = topCategories.Select(c => c.defName).ToList();
            }

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["zoneId"] = zoneId,
                ["zoneLabel"] = stockpile.label,
                ["priority"] = settings.Priority.ToString(),
                ["priorityValue"] = (int)settings.Priority,
                ["filter"] = new Dictionary<string, object>
                {
                    ["allowedDefCount"] = allowedDefs.Count,
                    ["allowedCategories"] = allowedCategories,
                    // 如果允许的物品太多，只返回摘要
                    ["allowedDefsSample"] = allowedDefs.Take(50).ToList(),
                    ["allowedDefsTruncated"] = allowedDefs.Count > 50,
                    ["hitPointsRange"] = new List<float>
                    {
                        filter != null ? filter.AllowedHitPointsPercents.min : 0,
                        filter != null ? filter.AllowedHitPointsPercents.max : 1
                    },
                    ["qualityRange"] = new List<string>
                    {
                        filter != null ? filter.AllowedQualityLevels.min.ToString() : "Awful",
                        filter != null ? filter.AllowedQualityLevels.max.ToString() : "Legendary"
                    }
                },
                ["spaceRemaining"] = stockpile.SpaceRemaining,
                ["cellCount"] = stockpile.CellCount
            };
        }

        /// <summary>
        /// 设置储存区物品过滤
        /// </summary>
        public static Dictionary<string, object> SetStorageFilter(
            int zoneId,
            string mode = null,
            List<string> categories = null,
            List<string> defs = null,
            bool allow = true,
            Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "No map or zone manager available"
                };
            }

            // 查找储存区
            var stockpile = map.zoneManager.AllZones
                .OfType<Zone_Stockpile>()
                .FirstOrDefault(z => z.ID == zoneId);

            if (stockpile == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"Stockpile zone with ID {zoneId} not found"
                };
            }

            var settings = stockpile.settings;
            if (settings == null || settings.filter == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "Zone has no storage settings"
                };
            }

            var filter = settings.filter;
            var messages = new List<string>();

            try
            {
                // 处理模式
                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode.ToLower())
                    {
                        case "allowall":
                            filter.SetAllowAll(null);
                            messages.Add("已允许所有物品");
                            break;

                        case "disallowall":
                            filter.SetDisallowAll();
                            messages.Add("已禁止所有物品");
                            break;

                        case "clear":
                            filter.SetDisallowAll();
                            messages.Add("已清空过滤设置");
                            break;
                    }
                }

                // 设置类别
                if (categories != null && categories.Count > 0)
                {
                    var validCategories = new List<string>();
                    var invalidCategories = new List<string>();

                    foreach (var catName in categories)
                    {
                        var catDef = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(catName);
                        if (catDef != null)
                        {
                            filter.SetAllow(catDef, allow);
                            validCategories.Add(catName);
                        }
                        else
                        {
                            invalidCategories.Add(catName);
                        }
                    }

                    if (validCategories.Count > 0)
                    {
                        messages.Add($"{(allow ? "允许" : "禁止")}类别: {string.Join(", ", validCategories)}");
                    }
                    if (invalidCategories.Count > 0)
                    {
                        messages.Add($"未找到类别: {string.Join(", ", invalidCategories)}");
                    }
                }

                // 设置特定物品定义
                if (defs != null && defs.Count > 0)
                {
                    var validDefs = new List<string>();
                    var invalidDefs = new List<string>();

                    foreach (var defName in defs)
                    {
                        var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                        if (thingDef != null)
                        {
                            filter.SetAllow(thingDef, allow);
                            validDefs.Add(defName);
                        }
                        else
                        {
                            invalidDefs.Add(defName);
                        }
                    }

                    if (validDefs.Count > 0)
                    {
                        messages.Add($"{(allow ? "允许" : "禁止")}物品: {string.Join(", ", validDefs.Take(10))}{(validDefs.Count > 10 ? "..." : "")}");
                    }
                    if (invalidDefs.Count > 0)
                    {
                        messages.Add($"未找到物品定义: {string.Join(", ", invalidDefs.Take(10))}");
                    }
                }

                // 通知设置已更改
                stockpile.Notify_SettingsChanged();

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["zoneId"] = zoneId,
                    ["zoneLabel"] = stockpile.label,
                    ["message"] = string.Join("; ", messages),
                    ["allowedDefCount"] = filter.AllowedDefCount
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"Failed to set filter: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 设置储存区优先级
        /// </summary>
        public static Dictionary<string, object> SetStoragePriority(int zoneId, string priority, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "No map or zone manager available"
                };
            }

            // 查找储存区
            var stockpile = map.zoneManager.AllZones
                .OfType<Zone_Stockpile>()
                .FirstOrDefault(z => z.ID == zoneId);

            if (stockpile == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"Stockpile zone with ID {zoneId} not found"
                };
            }

            var settings = stockpile.settings;
            if (settings == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "Zone has no storage settings"
                };
            }

            // 解析优先级
            StoragePriority newPriority;
            if (int.TryParse(priority, out int priorityValue))
            {
                // 数值方式
                if (priorityValue < 0 || priorityValue > 5)
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = "Priority value must be between 0 (Unstored) and 5 (Critical)"
                    };
                }
                newPriority = (StoragePriority)priorityValue;
            }
            else
            {
                // 名称方式
                if (!Enum.TryParse(priority, true, out newPriority))
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = $"Invalid priority name: {priority}. Valid values: Unstored, Low, Normal, Preferred, Important, Critical"
                    };
                }
            }

            var oldPriority = settings.Priority;
            settings.Priority = newPriority;

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["zoneId"] = zoneId,
                ["zoneLabel"] = stockpile.label,
                ["oldPriority"] = oldPriority.ToString(),
                ["oldPriorityValue"] = (int)oldPriority,
                ["newPriority"] = newPriority.ToString(),
                ["newPriorityValue"] = (int)newPriority
            };
        }

        /// <summary>
        /// 获取物品类别树
        /// </summary>
        public static Dictionary<string, object> GetThingCategories(string parentCategory = null)
        {
            try
            {
                ThingCategoryDef parentDef = null;

                if (!string.IsNullOrEmpty(parentCategory))
                {
                    parentDef = DefDatabase<ThingCategoryDef>.GetNamedSilentFail(parentCategory);
                    if (parentDef == null)
                    {
                        return new Dictionary<string, object>
                        {
                            ["success"] = false,
                            ["error"] = $"Category '{parentCategory}' not found"
                        };
                    }
                }
                else
                {
                    // 使用根类别
                    parentDef = ThingCategoryDefOf.Root;
                }

                var categories = new List<Dictionary<string, object>>();

                // 获取子类别
                var children = parentDef?.childCategories ?? new List<ThingCategoryDef>();
                foreach (var child in children)
                {
                    if (child == null) continue;

                    var childInfo = new Dictionary<string, object>
                    {
                        ["defName"] = child.defName,
                        ["label"] = child.label ?? child.defName,
                        ["description"] = child.description ?? "",
                        ["childCount"] = child.childCategories?.Count ?? 0,
                        ["thingCount"] = child.DescendantThingDefs?.Count() ?? 0
                    };

                    // 获取直接子类别名称列表
                    if (child.childCategories != null && child.childCategories.Count > 0)
                    {
                        childInfo["children"] = child.childCategories
                            .Where(c => c != null)
                            .Select(c => c.defName)
                            .ToList();
                    }

                    categories.Add(childInfo);
                }

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["parentCategory"] = parentDef?.defName ?? "Root",
                    ["parentLabel"] = parentDef?.label ?? "Root",
                    ["count"] = categories.Count,
                    ["categories"] = categories
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"Failed to get categories: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 获取常用储存预设
        /// </summary>
        public static Dictionary<string, object> GetStoragePresets()
        {
            var presets = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    ["name"] = "all",
                    ["description"] = "允许所有物品",
                    ["categories"] = new List<string>()
                },
                new Dictionary<string, object>
                {
                    ["name"] = "food",
                    ["description"] = "只允许食物",
                    ["categories"] = new List<string> { "Foods" }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "materials",
                    ["description"] = "只允许原材料",
                    ["categories"] = new List<string> { "ResourcesRaw" }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "weapons",
                    ["description"] = "只允许武器",
                    ["categories"] = new List<string> { "Weapons" }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "apparel",
                    ["description"] = "只允许衣物",
                    ["categories"] = new List<string> { "Apparel" }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "corpses",
                    ["description"] = "只允许尸体（垃圾区）",
                    ["categories"] = new List<string> { "Corpses" }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "chunks",
                    ["description"] = "只允许碎石块",
                    ["categories"] = new List<string> { "Chunks" }
                },
                new Dictionary<string, object>
                {
                    ["name"] = "medicine",
                    ["description"] = "只允许药品",
                    ["categories"] = new List<string> { "Medicine" }
                }
            };

            return new Dictionary<string, object>
            {
                ["success"] = true,
                ["count"] = presets.Count,
                ["presets"] = presets
            };
        }

        /// <summary>
        /// 应用储存预设到储存区
        /// </summary>
        public static Dictionary<string, object> ApplyStoragePreset(int zoneId, string presetName, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map?.zoneManager == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "No map or zone manager available"
                };
            }

            var stockpile = map.zoneManager.AllZones
                .OfType<Zone_Stockpile>()
                .FirstOrDefault(z => z.ID == zoneId);

            if (stockpile == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"Stockpile zone with ID {zoneId} not found"
                };
            }

            var settings = stockpile.settings;
            if (settings == null || settings.filter == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "Zone has no storage settings"
                };
            }

            var filter = settings.filter;

            try
            {
                switch (presetName.ToLower())
                {
                    case "all":
                        filter.SetAllowAll(null);
                        break;

                    case "food":
                        filter.SetDisallowAll();
                        filter.SetAllow(ThingCategoryDefOf.Foods, true);
                        break;

                    case "materials":
                        filter.SetDisallowAll();
                        filter.SetAllow(ThingCategoryDefOf.ResourcesRaw, true);
                        break;

                    case "weapons":
                        filter.SetDisallowAll();
                        filter.SetAllow(ThingCategoryDefOf.Weapons, true);
                        break;

                    case "apparel":
                        filter.SetDisallowAll();
                        filter.SetAllow(ThingCategoryDefOf.Apparel, true);
                        break;

                    case "corpses":
                        filter.SetDisallowAll();
                        filter.SetAllow(ThingCategoryDefOf.Corpses, true);
                        break;

                    case "chunks":
                        filter.SetDisallowAll();
                        filter.SetAllow(ThingCategoryDefOf.Chunks, true);
                        break;

                    case "medicine":
                        filter.SetDisallowAll();
                        // 药品类别的 defName
                        var medCat = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("Medicine");
                        if (medCat != null)
                        {
                            filter.SetAllow(medCat, true);
                        }
                        else
                        {
                            return new Dictionary<string, object>
                            {
                                ["success"] = false,
                                ["error"] = "Medicine category not found"
                            };
                        }
                        break;

                    default:
                        return new Dictionary<string, object>
                        {
                            ["success"] = false,
                            ["error"] = $"Unknown preset: {presetName}. Use get_storage_presets to see available presets."
                        };
                }

                stockpile.Notify_SettingsChanged();

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["zoneId"] = zoneId,
                    ["zoneLabel"] = stockpile.label,
                    ["preset"] = presetName,
                    ["allowedDefCount"] = filter.AllowedDefCount
                };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"Failed to apply preset: {ex.Message}"
                };
            }
        }

        #endregion

        #region 植物标记命令 (Designation)

        /// <summary>
        /// 标记树木/植物砍伐 - 让殖民者自动砍伐指定的植物
        /// 对应游戏中的 Orders -> Cut Plants 操作
        /// 使用 GenRadial.RadialPattern 高效遍历圆形区域
        /// </summary>
        /// <param name="centerX">中心X坐标（可选，默认为殖民地中心）</param>
        /// <param name="centerZ">中心Z坐标（可选，默认为殖民地中心）</param>
        /// <param name="radius">半径（可选，默认30格）</param>
        /// <param name="maxCount">最大标记数量（可选，默认20棵）</param>
        /// <param name="treesOnly">是否只标记树木（默认true）</param>
        /// <param name="map">目标地图</param>
        /// <returns>标记结果</returns>
        public static Dictionary<string, object> DesignateCutPlants(
            int? centerX = null, int? centerZ = null,
            int radius = 30, int maxCount = 20, bool treesOnly = true,
            Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "No map available"
                };
            }

            try
            {
                // 确定中心点
                IntVec3 center;
                if (centerX.HasValue && centerZ.HasValue)
                {
                    center = new IntVec3(centerX.Value, 0, centerZ.Value);
                }
                else
                {
                    // 默认使用殖民地的中心位置
                    center = map.Center;
                }

                // 计算需要遍历的格子数量
                int numCells = GenRadial.NumCellsInRadius(radius);
                int designatedCount = 0;
                int totalWoodYield = 0;
                var designatedList = new List<Dictionary<string, object>>();

                // 使用 GenRadial.RadialPattern 高效遍历圆形区域
                for (int i = 0; i < numCells && designatedCount < maxCount; i++)
                {
                    IntVec3 cell = center + GenRadial.RadialPattern[i];

                    // 检查是否在地图边界内
                    if (!cell.InBounds(map))
                        continue;

                    // 获取该格子的植物
                    Plant plant = cell.GetPlant(map);
                    if (plant == null)
                        continue;

                    // 检查是否已有砍伐标记
                    if (map.designationManager.DesignationOn(plant, DesignationDefOf.CutPlant) != null)
                        continue;

                    // 检查是否为树木
                    if (treesOnly && !plant.def.plant.IsTree)
                        continue;

                    try
                    {
                        // 添加砍伐标记
                        var designation = new Designation(plant, DesignationDefOf.CutPlant);
                        map.designationManager.AddDesignation(designation);

                        designatedCount++;
                        int woodYield = (int)(plant.def.plant.harvestYield * plant.Growth);
                        totalWoodYield += woodYield;

                        designatedList.Add(new Dictionary<string, object>
                        {
                            ["thingId"] = plant.thingIDNumber,
                            ["defName"] = plant.def?.defName,
                            ["label"] = plant.def?.label,
                            ["x"] = plant.Position.x,
                            ["z"] = plant.Position.z,
                            ["growth"] = plant.Growth,
                            ["woodYield"] = woodYield
                        });
                    }
                    catch (System.Exception ex)
                    {
                        // 忽略单个标记失败
                        Verse.Log.Warning($"Failed to designate plant at {cell}: {ex.Message}");
                    }
                }

                if (designatedCount == 0)
                {
                    return new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = treesOnly
                            ? $"在半径 {radius} 格内未找到可标记的树木（中心: {center.x}, {center.z}）"
                            : $"在半径 {radius} 格内未找到可标记的植物（中心: {center.x}, {center.z}）",
                        ["searchRadius"] = radius,
                        ["centerX"] = center.x,
                        ["centerZ"] = center.z,
                        ["designatedCount"] = 0
                    };
                }

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["designatedCount"] = designatedCount,
                    ["estimatedWoodYield"] = totalWoodYield,
                    ["searchRadius"] = radius,
                    ["centerX"] = center.x,
                    ["centerZ"] = center.z,
                    ["treesOnly"] = treesOnly,
                    ["message"] = $"成功标记 {designatedCount} 棵{(treesOnly ? "树" : "植物")}砍伐，预计产出约 {totalWoodYield} 木材",
                    ["plants"] = designatedList
                };
            }
            catch (System.Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"标记砍伐失败: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 取消植物的砍伐标记
        /// </summary>
        public static Dictionary<string, object> UndesignateCutPlants(
            int? centerX = null, int? centerZ = null,
            int radius = 30, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "No map available"
                };
            }

            try
            {
                IntVec3 center = (centerX.HasValue && centerZ.HasValue)
                    ? new IntVec3(centerX.Value, 0, centerZ.Value)
                    : map.Center;

                int numCells = GenRadial.NumCellsInRadius(radius);
                int removedCount = 0;

                for (int i = 0; i < numCells; i++)
                {
                    IntVec3 cell = center + GenRadial.RadialPattern[i];
                    if (!cell.InBounds(map))
                        continue;

                    Plant plant = cell.GetPlant(map);
                    if (plant == null)
                        continue;

                    var designation = map.designationManager.DesignationOn(plant, DesignationDefOf.CutPlant);
                    if (designation != null)
                    {
                        map.designationManager.RemoveDesignation(designation);
                        removedCount++;
                    }
                }

                return new Dictionary<string, object>
                {
                    ["success"] = true,
                    ["removedCount"] = removedCount,
                    ["searchRadius"] = radius,
                    ["message"] = $"取消了 {removedCount} 个砍伐标记"
                };
            }
            catch (System.Exception ex)
            {
                return new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = $"取消标记失败: {ex.Message}"
                };
            }
        }

        #endregion

        #region ESDF 和 Voronoi 地图分析

        // ESDF 缓存
        private static Dictionary<int, MapAnalysis.ESDFGrid> esdfCache = new Dictionary<int, MapAnalysis.ESDFGrid>();
        private static Dictionary<int, MapAnalysis.VoronoiSkeleton> voronoiCache = new Dictionary<int, MapAnalysis.VoronoiSkeleton>();
        private static Dictionary<int, int> lastESDFTicks = new Dictionary<int, int>();
        private static Dictionary<int, int> lastVoronoiTicks = new Dictionary<int, int>();
        private const int CacheValidTicks = 2500; // 缓存有效期（约1游戏小时）

        /// <summary>
        /// 获取或创建 ESDF 网格
        /// </summary>
        public static MapAnalysis.ESDFGrid GetOrCreateESDF(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return null;

            int mapId = map.Tile;  // 使用 Tile ID 替代 GetHashCode
            int currentTick = Find.TickManager.TicksGame;

            // 初始化该地图的 tick 记录
            if (!lastESDFTicks.ContainsKey(mapId))
            {
                lastESDFTicks[mapId] = -CacheValidTicks - 1;  // 确保首次计算
            }

            if (esdfCache.ContainsKey(mapId) && currentTick - lastESDFTicks[mapId] < CacheValidTicks)
            {
                return esdfCache[mapId];
            }

            var esdf = new MapAnalysis.ESDFGrid(map);
            esdf.Recalculate();
            esdfCache[mapId] = esdf;
            lastESDFTicks[mapId] = currentTick;

            return esdf;
        }

        /// <summary>
        /// 获取或创建 Voronoi 骨架
        /// </summary>
        public static MapAnalysis.VoronoiSkeleton GetOrCreateVoronoi(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return null;

            int mapId = map.Tile;  // 使用 Tile ID 替代 GetHashCode
            int currentTick = Find.TickManager.TicksGame;

            // 初始化该地图的 tick 记录
            if (!lastVoronoiTicks.ContainsKey(mapId))
            {
                lastVoronoiTicks[mapId] = -CacheValidTicks - 1;  // 确保首次计算
            }

            if (voronoiCache.ContainsKey(mapId) && currentTick - lastVoronoiTicks[mapId] < CacheValidTicks)
            {
                return voronoiCache[mapId];
            }

            var esdf = GetOrCreateESDF(map);
            var voronoi = new MapAnalysis.VoronoiSkeleton(map);
            voronoi.Calculate(esdf);
            voronoiCache[mapId] = voronoi;
            lastVoronoiTicks[mapId] = currentTick;

            return voronoi;
        }

        /// <summary>
        /// 获取 ESDF 地图信息
        /// </summary>
        public static Dictionary<string, object> GetESDFMap(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            var esdf = GetOrCreateESDF(map);
            var stats = esdf.GetStatistics();

            // 获取安全区域（距离 >= 3）
            var safeAreas = esdf.FindSafeAreas(3, 20);

            // 获取障碍物类型分布
            var obstacleDistribution = esdf.GetObstacleTypeDistribution();

            // 获取障碍物样本
            var obstacleSamples = esdf.GetObstacleSamples(20);

            // 获取地理分析
            var geographicAnalysis = esdf.GetGeographicAnalysis();

            return new Dictionary<string, object>
            {
                ["mapSize"] = new { x = map.Size.x, z = map.Size.z },
                ["summary"] = new Dictionary<string, object>
                {
                    ["maxDistance"] = stats.maxDistance,
                    ["avgDistance"] = Mathf.Round(stats.avgDistance * 10) / 10,
                    ["safeAreaPercent"] = Mathf.Round(stats.safeAreaPercent * 10) / 10,
                    ["obstacleCount"] = stats.obstacleCount,
                    ["walkableCount"] = stats.walkableCount,
                    ["geographicSummary"] = geographicAnalysis.geographicSummary
                },
                ["distanceDistribution"] = stats.distanceDistribution,
                ["obstacleTypeDistribution"] = obstacleDistribution,
                ["obstacleCategoryCounts"] = geographicAnalysis.categoryCounts,
                ["sampleObstacles"] = obstacleSamples.Select(o => o.ToDictionary()).ToList(),
                ["geography"] = new Dictionary<string, object>
                {
                    ["oreDeposits"] = geographicAnalysis.oreDeposits,
                    ["rockFormations"] = geographicAnalysis.rockFormations,
                    ["waterBodies"] = geographicAnalysis.waterBodies,
                    ["structures"] = geographicAnalysis.structures
                },
                ["safeAreas"] = safeAreas.Take(10).Select(a => new Dictionary<string, object>
                {
                    ["minX"] = a.minX,
                    ["minZ"] = a.minZ,
                    ["maxX"] = a.maxX,
                    ["maxZ"] = a.maxZ,
                    ["area"] = a.area,
                    ["minDistance"] = a.minDistance
                }).ToList()
            };
        }

        /// <summary>
        /// 获取指定位置的详细信息
        /// </summary>
        public static Dictionary<string, object> GetCellInfo(int x, int z, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            var cell = new IntVec3(x, 0, z);
            if (!cell.InBounds(map))
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "Cell out of bounds"
                };
            }

            var esdf = GetOrCreateESDF(map);
            var nearestObstacle = esdf.GetNearestObstacle(cell);
            var terrain = map.terrainGrid.TerrainAt(cell);
            var building = map.edificeGrid[cell];
            var room = cell.GetRoom(map);

            return new Dictionary<string, object>
            {
                ["position"] = new { x, z },
                ["esdf"] = new Dictionary<string, object>
                {
                    ["distance"] = esdf.GetDistance(cell),
                    ["nearestObstacle"] = nearestObstacle?.ToDictionary()
                },
                ["terrain"] = terrain != null ? new Dictionary<string, object>
                {
                    ["defName"] = terrain.defName,
                    ["label"] = terrain.LabelCap,
                    ["passability"] = terrain.passability.ToString(),
                    ["isWater"] = terrain.IsWater,
                    ["isRiver"] = terrain.IsRiver
                } : null,
                ["building"] = building != null ? new Dictionary<string, object>
                {
                    ["defName"] = building.def.defName,
                    ["label"] = building.def.LabelCap,
                    ["passability"] = building.def.passability.ToString()
                } : null,
                ["room"] = room != null ? new Dictionary<string, object>
                {
                    ["id"] = room.ID,
                    ["role"] = room.Role?.defName ?? "None",
                    ["label"] = room.Role?.LabelCap ?? "无",
                    ["isIndoor"] = !room.PsychologicallyOutdoors && !room.IsDoorway,
                    ["cellCount"] = room.CellCount
                } : null
            };
        }

        /// <summary>
        /// 获取房间边界建造状态
        /// 学习 RimWorld 的 BaseGen.SymbolResolver_EdgeWalls 做法：使用 CellRect.EdgeCells 遍历边缘格子
        /// </summary>
        public static Dictionary<string, object> GetRoomBoundary(int centerX, int centerZ, int width, int height, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            // 1. 创建矩形区域（使用 RimWorld 的 CellRect.CenteredOn）
            var rect = CellRect.CenteredOn(new IntVec3(centerX, 0, centerZ), width, height);

            // 2. 遍历边缘格子
            var edgeCells = new List<Dictionary<string, object>>();
            var missingCells = new List<Dictionary<string, object>>();
            int builtCount = 0;

            foreach (var cell in rect.EdgeCells)
            {
                if (!cell.InBounds(map)) continue;

                var status = GetCellBuildStatus(cell, map);
                var cellInfo = new Dictionary<string, object>
                {
                    ["x"] = cell.x,
                    ["z"] = cell.z,
                    ["status"] = status.status
                };

                if (status.buildingDef != null)
                {
                    cellInfo["defName"] = status.buildingDef;
                }

                edgeCells.Add(cellInfo);

                if (status.status == "empty")
                {
                    missingCells.Add(new Dictionary<string, object>
                    {
                        ["x"] = cell.x,
                        ["z"] = cell.z
                    });
                }
                else
                {
                    builtCount++;
                }
            }

            // 3. 返回结果
            return new Dictionary<string, object>
            {
                ["rect"] = new Dictionary<string, object>
                {
                    ["minX"] = rect.minX,
                    ["maxX"] = rect.maxX,
                    ["minZ"] = rect.minZ,
                    ["maxZ"] = rect.maxZ
                },
                ["totalEdgeCells"] = edgeCells.Count,
                ["edgeCells"] = edgeCells,
                ["missingCells"] = missingCells,
                ["completionPercent"] = edgeCells.Count > 0
                    ? Mathf.Round((float)builtCount / edgeCells.Count * 100 * 10) / 10  // 保留1位小数
                    : 0
            };
        }

        /// <summary>
        /// 获取格子建造状态
        /// </summary>
        private static (string status, string buildingDef) GetCellBuildStatus(IntVec3 cell, Map map)
        {
            // 检查已建成建筑（不可通行的建筑视为墙）
            var building = map.edificeGrid[cell];
            if (building != null && building.def.passability == Traversability.Impassable)
            {
                return ("built", building.def.defName);
            }

            // 检查蓝图和框架
            var things = map.thingGrid.ThingsListAt(cell);
            foreach (var thing in things)
            {
                // 检查蓝图
                if (thing is Blueprint_Build blueprint)
                {
                    var defToBuild = blueprint.def.entityDefToBuild;
                    return ("blueprint", defToBuild?.defName ?? thing.def.defName);
                }
                // 检查建造框架
                if (thing is Frame frame)
                {
                    var defToBuild = frame.def.entityDefToBuild;
                    return ("frame", defToBuild?.defName ?? thing.def.defName);
                }
            }

            return ("empty", null);
        }

        /// <summary>
        /// 批量扫描区域内部格子（替代多次 get_cell_info 调用，提高效率）
        /// 学习 RimWorld 的 BaseGen 做法：使用 CellRect.Cells 遍历内部格子
        /// </summary>
        public static Dictionary<string, object> ScanArea(int centerX, int centerZ, int width, int height, Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            // 1. 创建矩形区域（使用 RimWorld 的 CellRect.CenteredOn）
            var rect = CellRect.CenteredOn(new IntVec3(centerX, 0, centerZ), width, height);
            rect = rect.ClipInsideMap(map);

            // 2. 遍历内部格子
            var cells = new List<Dictionary<string, object>>();
            var buildings = new List<Dictionary<string, object>>();
            var blueprints = new List<Dictionary<string, object>>();
            var frames = new List<Dictionary<string, object>>();
            var items = new List<Dictionary<string, object>>();
            var terrainStats = new Dictionary<string, int>();
            var seenThings = new HashSet<int>();

            foreach (var cell in rect.Cells)
            {
                if (!cell.InBounds(map)) continue;

                // 获取地形
                var terrain = map.terrainGrid.TerrainAt(cell);
                string terrainDefName = terrain?.defName ?? "Unknown";
                if (!terrainStats.ContainsKey(terrainDefName))
                    terrainStats[terrainDefName] = 0;
                terrainStats[terrainDefName]++;

                // 获取建筑
                var building = map.edificeGrid[cell];
                string buildingDefName = building?.def?.defName;

                // 获取蓝图/框架/物品
                string blueprintDefName = null;
                string frameDefName = null;
                var cellItems = new List<Dictionary<string, object>>();

                var things = map.thingGrid.ThingsListAt(cell);
                foreach (var thing in things)
                {
                    if (thing is Blueprint_Build bp && !seenThings.Contains(bp.thingIDNumber))
                    {
                        seenThings.Add(bp.thingIDNumber);
                        blueprintDefName = bp.def.entityDefToBuild?.defName ?? bp.def.defName;
                        blueprints.Add(new Dictionary<string, object>
                        {
                            ["x"] = cell.x,
                            ["z"] = cell.z,
                            ["defName"] = blueprintDefName
                        });
                    }
                    else if (thing is Frame f && !seenThings.Contains(f.thingIDNumber))
                    {
                        seenThings.Add(f.thingIDNumber);
                        frameDefName = f.def.entityDefToBuild?.defName ?? f.def.defName;
                        frames.Add(new Dictionary<string, object>
                        {
                            ["x"] = cell.x,
                            ["z"] = cell.z,
                            ["defName"] = frameDefName
                        });
                    }
                    else if (thing.def.category == ThingCategory.Item && !seenThings.Contains(thing.thingIDNumber))
                    {
                        seenThings.Add(thing.thingIDNumber);
                        items.Add(new Dictionary<string, object>
                        {
                            ["x"] = cell.x,
                            ["z"] = cell.z,
                            ["defName"] = thing.def.defName,
                            ["label"] = thing.def.LabelCap,
                            ["stackCount"] = thing.stackCount
                        });
                    }
                }

                // 记录建筑（去重）
                if (building != null && !seenThings.Contains(building.thingIDNumber))
                {
                    seenThings.Add(building.thingIDNumber);
                    buildings.Add(new Dictionary<string, object>
                    {
                        ["x"] = cell.x,
                        ["z"] = cell.z,
                        ["defName"] = buildingDefName,
                        ["label"] = building.def.LabelCap
                    });
                }

                // 简化的格子状态
                string status = "empty";
                if (building != null) status = "built";
                else if (blueprintDefName != null) status = "blueprint";
                else if (frameDefName != null) status = "frame";

                cells.Add(new Dictionary<string, object>
                {
                    ["x"] = cell.x,
                    ["z"] = cell.z,
                    ["status"] = status,
                    ["terrain"] = terrainDefName,
                    ["passable"] = terrain?.passability != Traversability.Impassable,
                    ["building"] = buildingDefName,
                    ["blueprint"] = blueprintDefName,
                    ["frame"] = frameDefName
                });
            }

            // 3. 返回结果
            return new Dictionary<string, object>
            {
                ["rect"] = new Dictionary<string, object>
                {
                    ["minX"] = rect.minX,
                    ["maxX"] = rect.maxX,
                    ["minZ"] = rect.minZ,
                    ["maxZ"] = rect.maxZ
                },
                ["totalCells"] = cells.Count,
                ["cells"] = cells,
                ["buildings"] = buildings,
                ["blueprints"] = blueprints,
                ["frames"] = frames,
                ["items"] = items.Take(100).ToList(),  // 限制物品数量
                ["terrainStats"] = terrainStats
            };
        }

        /// <summary>
        /// 获取 Voronoi 骨架地图信息
        /// </summary>
        public static Dictionary<string, object> GetVoronoiMap(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            var voronoi = GetOrCreateVoronoi(map);
            var stats = voronoi.GetStatistics();

            return new Dictionary<string, object>
            {
                ["mapSize"] = new { x = map.Size.x, z = map.Size.z },
                ["summary"] = new Dictionary<string, object>
                {
                    ["totalNodes"] = stats.totalNodes,
                    ["totalEdges"] = stats.totalEdges,
                    ["totalRegions"] = stats.totalRegions,
                    ["avgNodeDistance"] = Mathf.Round(stats.avgNodeDistance * 10) / 10,
                    ["maxNodeDistance"] = stats.maxNodeDistance,
                    ["avgEdgeLength"] = Mathf.Round(stats.avgEdgeLength * 10) / 10,
                    ["avgRegionArea"] = Mathf.Round(stats.avgRegionArea * 10) / 10
                },
                ["nodes"] = voronoi.Nodes.Select(n => n.ToDictionary()).ToList(),
                ["edges"] = voronoi.Edges.Select(e => new Dictionary<string, object>
                {
                    ["from"] = e.fromNode,
                    ["to"] = e.toNode,
                    ["length"] = e.length
                }).ToList(),
                ["regions"] = voronoi.Regions.Select(r => r.ToDictionary()).ToList()
            };
        }

        /// <summary>
        /// 查找适合建造的位置
        /// </summary>
        public static Dictionary<string, object> FindBuildLocations(
            string buildingDefName = null,
            int minDistanceFromWall = 1,
            bool preferIndoor = true,
            int limit = 10,
            Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            var esdf = GetOrCreateESDF(map);
            var results = new List<Dictionary<string, object>>();

            // 遍历所有格子
            foreach (var cell in map.AllCells)
            {
                int dist = esdf.GetDistance(cell);

                // 检查最小距离要求
                if (dist < minDistanceFromWall)
                {
                    continue;
                }

                // 检查是否已有建筑
                if (map.edificeGrid[cell] != null)
                {
                    continue;
                }

                // 检查地形
                var terrain = map.terrainGrid.TerrainAt(cell);
                if (terrain == null || terrain.passability == Traversability.Impassable)
                {
                    continue;
                }

                // 检查是否在室内
                var room = cell.GetRoom(map);
                bool isIndoor = room != null && !room.PsychologicallyOutdoors && !room.IsDoorway;

                // 计算评分
                float score = dist / 10f; // 基础分：距离越远越好
                if (preferIndoor && isIndoor)
                {
                    score += 5f; // 室内加分
                }

                // 获取最近障碍物信息
                var nearestObstacle = esdf.GetNearestObstacle(cell);

                // 获取附近障碍物（4个方向，距离<=10格内的障碍物）
                var nearbyObstacles = GetNearbyObstacles(map, esdf, cell, 10, 4);

                results.Add(new Dictionary<string, object>
                {
                    ["x"] = cell.x,
                    ["z"] = cell.z,
                    ["safetyDistance"] = dist,
                    ["isIndoor"] = isIndoor,
                    ["score"] = Mathf.Round(score * 100) / 100,
                    ["terrainDef"] = terrain?.defName ?? "Unknown",
                    ["nearestObstacle"] = nearestObstacle?.ToDictionary(),
                    ["nearbyObstacles"] = nearbyObstacles
                });
            }

            // 按评分排序并返回前N个
            var topResults = results
                .OrderByDescending(r => (float)r["score"])
                .Take(limit)
                .ToList();

            return new Dictionary<string, object>
            {
                ["totalCandidates"] = results.Count,
                ["returnedCount"] = topResults.Count,
                ["locations"] = topResults
            };
        }

        /// <summary>
        /// 获取指定位置附近的障碍物
        /// </summary>
        private static List<Dictionary<string, object>> GetNearbyObstacles(
            Map map,
            MapAnalysis.ESDFGrid esdf,
            IntVec3 center,
            int maxDistance,
            int maxResults)
        {
            var result = new List<Dictionary<string, object>>();
            var seenDefNames = new HashSet<string>();

            // 8方向搜索
            var directions = new[]
            {
                new IntVec3(1, 0, 0),   // 东
                new IntVec3(-1, 0, 0),  // 西
                new IntVec3(0, 0, 1),   // 北
                new IntVec3(0, 0, -1),  // 南
                new IntVec3(1, 0, 1),   // 东北
                new IntVec3(-1, 0, 1),  // 西北
                new IntVec3(1, 0, -1),  // 东南
                new IntVec3(-1, 0, -1)  // 西南
            };

            var directionNames = new[] { "east", "west", "north", "south", "northeast", "northwest", "southeast", "southwest" };

            for (int d = 0; d < directions.Length && result.Count < maxResults; d++)
            {
                var dir = directions[d];
                for (int dist = 1; dist <= maxDistance && result.Count < maxResults; dist++)
                {
                    var checkCell = center + dir * dist;
                    if (!checkCell.InBounds(map))
                    {
                        break;
                    }

                    var obstacle = esdf.GetNearestObstacle(checkCell);
                    if (obstacle != null && obstacle.x == checkCell.x && obstacle.z == checkCell.z)
                    {
                        // 这个格子本身就是障碍物
                        if (!seenDefNames.Contains(obstacle.defName) || result.Count < 2)
                        {
                            seenDefNames.Add(obstacle.defName);
                            result.Add(new Dictionary<string, object>
                            {
                                ["type"] = obstacle.type,
                                ["defName"] = obstacle.defName,
                                ["label"] = obstacle.label,
                                ["direction"] = directionNames[d],
                                ["distance"] = dist,
                                ["x"] = obstacle.x,
                                ["z"] = obstacle.z,
                                ["category"] = obstacle.category
                            });
                        }
                        break; // 这个方向找到障碍物后停止
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 强制刷新 ESDF 缓存
        /// </summary>
        public static void RefreshESDF(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null) return;

            int mapId = map.Tile;
            if (esdfCache.ContainsKey(mapId))
            {
                esdfCache[mapId].MarkDirty();
            }
            // 重置该地图的 tick 记录，强制下次重新计算
            lastESDFTicks[mapId] = -CacheValidTicks - 1;
        }

        /// <summary>
        /// 获取河流信息（使用 BFS 识别连通区域）
        /// </summary>
        public static Dictionary<string, object> GetRiverInfo(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            int mapSizeX = map.Size.x;
            int mapSizeZ = map.Size.z;
            var visited = new bool[mapSizeX, mapSizeZ];

            var riverSegments = new List<Dictionary<string, object>>();
            int totalRiverCells = 0;
            int totalFordCells = 0;

            // 4方向邻居偏移
            var neighborOffsets = new[]
            {
                new IntVec3(1, 0, 0),
                new IntVec3(-1, 0, 0),
                new IntVec3(0, 0, 1),
                new IntVec3(0, 0, -1)
            };

            // BFS 找所有河流连通区域
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    if (visited[x, z]) continue;

                    var cell = new IntVec3(x, 0, z);
                    var terrain = map.terrainGrid.TerrainAt(cell);

                    if (terrain == null) continue;

                    // 检查是否为河流格子（包括河流和浅水）
                    bool isRiverCell = terrain.IsRiver ||
                        (terrain.IsWater && !string.IsNullOrEmpty(terrain.defName) &&
                         terrain.defName.Contains("Shallow"));
                    if (!isRiverCell) continue;

                    // BFS 找连通的河流区域
                    var segment = BfsFindRiverSegment(map, cell, visited, neighborOffsets);

                    if (segment.cells.Count >= 5)  // 至少5格才算河段
                    {
                        totalRiverCells += segment.cells.Count;
                        riverSegments.Add(segment.ToDictionary());
                    }
                }
            }

            // 单独统计浅滩（可穿越点）
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    var terrain = map.terrainGrid.TerrainAt(new IntVec3(x, 0, z));
                    if (terrain != null && !string.IsNullOrEmpty(terrain.defName) && terrain.defName.Contains("Ford"))
                    {
                        totalFordCells++;
                    }
                }
            }

            // 判断是否有河流
            bool hasRiver = totalRiverCells > 0;

            // 计算整体边界和走向
            int overallMinX = int.MaxValue, overallMaxX = int.MinValue;
            int overallMinZ = int.MaxValue, overallMaxZ = int.MinValue;

            foreach (var segment in riverSegments)
            {
                var bounds = segment["bounds"] as Dictionary<string, int>;
                if (bounds != null)
                {
                    if (bounds["minX"] < overallMinX) overallMinX = bounds["minX"];
                    if (bounds["maxX"] > overallMaxX) overallMaxX = bounds["maxX"];
                    if (bounds["minZ"] < overallMinZ) overallMinZ = bounds["minZ"];
                    if (bounds["maxZ"] > overallMaxZ) overallMaxZ = bounds["maxZ"];
                }
            }

            string direction = "unknown";
            if (hasRiver)
            {
                int widthX = overallMaxX - overallMinX;
                int widthZ = overallMaxZ - overallMinZ;
                direction = widthZ > widthX ? "north-south" : "east-west";
            }

            return new Dictionary<string, object>
            {
                ["hasRiver"] = hasRiver,
                ["totalRiverCells"] = totalRiverCells,
                ["totalFordCells"] = totalFordCells,
                ["segmentCount"] = riverSegments.Count,
                ["direction"] = direction,
                ["bounds"] = hasRiver ? new Dictionary<string, int>
                {
                    ["minX"] = overallMinX,
                    ["maxX"] = overallMaxX,
                    ["minZ"] = overallMinZ,
                    ["maxZ"] = overallMaxZ
                } : null,
                ["segments"] = riverSegments
            };
        }

        /// <summary>
        /// BFS 查找连通的河流区域
        /// </summary>
        private static RiverSegment BfsFindRiverSegment(Map map, IntVec3 start, bool[,] visited, IntVec3[] neighborOffsets)
        {
            var segment = new RiverSegment
            {
                cells = new List<IntVec3>()
            };

            var queue = new Queue<IntVec3>();
            queue.Enqueue(start);
            visited[start.x, start.z] = true;

            int minX = start.x, maxX = start.x;
            int minZ = start.z, maxZ = start.z;
            long sumX = 0, sumZ = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                segment.cells.Add(current);

                // 更新边界
                if (current.x < minX) minX = current.x;
                if (current.x > maxX) maxX = current.x;
                if (current.z < minZ) minZ = current.z;
                if (current.z > maxZ) maxZ = current.z;
                sumX += current.x;
                sumZ += current.z;

                // 扩展邻居
                foreach (var offset in neighborOffsets)
                {
                    var neighbor = current + offset;

                    if (neighbor.x < 0 || neighbor.x >= visited.GetLength(0) ||
                        neighbor.z < 0 || neighbor.z >= visited.GetLength(1))
                        continue;

                    if (visited[neighbor.x, neighbor.z]) continue;

                    var terrain = map.terrainGrid.TerrainAt(neighbor);
                    if (terrain != null)
                    {
                        // 检查是否为河流格子（包括河流和浅水）
                        bool isRiverNeighbor = terrain.IsRiver ||
                            (terrain.IsWater && !string.IsNullOrEmpty(terrain.defName) &&
                             terrain.defName.Contains("Shallow"));
                        if (isRiverNeighbor)
                        {
                            visited[neighbor.x, neighbor.z] = true;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            segment.minX = minX;
            segment.maxX = maxX;
            segment.minZ = minZ;
            segment.maxZ = maxZ;
            segment.centerX = segment.cells.Count > 0 ? (int)(sumX / segment.cells.Count) : 0;
            segment.centerZ = segment.cells.Count > 0 ? (int)(sumZ / segment.cells.Count) : 0;

            // 计算走向
            int widthX = maxX - minX;
            int widthZ = maxZ - minZ;
            segment.direction = widthZ > widthX ? "north-south" : "east-west";

            return segment;
        }

        /// <summary>
        /// 河流段信息
        /// </summary>
        private class RiverSegment
        {
            public List<IntVec3> cells;
            public int minX, maxX, minZ, maxZ;
            public int centerX, centerZ;
            public string direction;

            public Dictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>
                {
                    ["cellCount"] = cells.Count,
                    ["bounds"] = new Dictionary<string, int>
                    {
                        ["minX"] = minX,
                        ["maxX"] = maxX,
                        ["minZ"] = minZ,
                        ["maxZ"] = maxZ
                    },
                    ["center"] = new Dictionary<string, int>
                    {
                        ["x"] = centerX,
                        ["z"] = centerZ
                    },
                    ["direction"] = direction,
                    ["sampleCells"] = cells.Take(10).Select(c => new { x = c.x, z = c.z }).ToList()
                };
            }
        }

        /// <summary>
        /// 获取沼泽信息（使用 BFS 识别连通区域）
        /// </summary>
        public static Dictionary<string, object> GetMarshInfo(Map map = null)
        {
            map = map ?? Find.CurrentMap;
            if (map == null)
            {
                return new Dictionary<string, object>
                {
                    ["error"] = "No map available"
                };
            }

            int mapSizeX = map.Size.x;
            int mapSizeZ = map.Size.z;
            var visited = new bool[mapSizeX, mapSizeZ];

            var marshSegments = new List<Dictionary<string, object>>();
            int totalMarshCells = 0;

            var neighborOffsets = new[]
            {
                new IntVec3(1, 0, 0),
                new IntVec3(-1, 0, 0),
                new IntVec3(0, 0, 1),
                new IntVec3(0, 0, -1)
            };

            // BFS 找所有沼泽连通区域
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    if (visited[x, z]) continue;

                    var cell = new IntVec3(x, 0, z);
                    var terrain = map.terrainGrid.TerrainAt(cell);

                    if (terrain == null || string.IsNullOrEmpty(terrain.defName)) continue;

                    bool isMarsh = terrain.defName.Contains("Marsh") || terrain.defName.Contains("Swamp");
                    if (!isMarsh) continue;

                    var segment = BfsFindMarshSegment(map, cell, visited, neighborOffsets);

                    if (segment.cells.Count >= 3)
                    {
                        totalMarshCells += segment.cells.Count;
                        marshSegments.Add(segment.ToDictionary());
                    }
                }
            }

            bool hasMarsh = totalMarshCells > 0;

            return new Dictionary<string, object>
            {
                ["hasMarsh"] = hasMarsh,
                ["totalMarshCells"] = totalMarshCells,
                ["segmentCount"] = marshSegments.Count,
                ["segments"] = marshSegments
            };
        }

        /// <summary>
        /// BFS 查找连通的沼泽区域
        /// </summary>
        private static MarshSegment BfsFindMarshSegment(Map map, IntVec3 start, bool[,] visited, IntVec3[] neighborOffsets)
        {
            var segment = new MarshSegment
            {
                cells = new List<IntVec3>()
            };

            var queue = new Queue<IntVec3>();
            queue.Enqueue(start);
            visited[start.x, start.z] = true;

            int minX = start.x, maxX = start.x;
            int minZ = start.z, maxZ = start.z;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                segment.cells.Add(current);

                if (current.x < minX) minX = current.x;
                if (current.x > maxX) maxX = current.x;
                if (current.z < minZ) minZ = current.z;
                if (current.z > maxZ) maxZ = current.z;

                foreach (var offset in neighborOffsets)
                {
                    var neighbor = current + offset;

                    if (neighbor.x < 0 || neighbor.x >= visited.GetLength(0) ||
                        neighbor.z < 0 || neighbor.z >= visited.GetLength(1))
                        continue;

                    if (visited[neighbor.x, neighbor.z]) continue;

                    var terrain = map.terrainGrid.TerrainAt(neighbor);
                    if (terrain != null && !string.IsNullOrEmpty(terrain.defName) &&
                        (terrain.defName.Contains("Marsh") || terrain.defName.Contains("Swamp")))
                    {
                        visited[neighbor.x, neighbor.z] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            segment.minX = minX;
            segment.maxX = maxX;
            segment.minZ = minZ;
            segment.maxZ = maxZ;

            return segment;
        }

        /// <summary>
        /// 沼泽段信息
        /// </summary>
        private class MarshSegment
        {
            public List<IntVec3> cells;
            public int minX, maxX, minZ, maxZ;

            public Dictionary<string, object> ToDictionary()
            {
                return new Dictionary<string, object>
                {
                    ["cellCount"] = cells.Count,
                    ["bounds"] = new Dictionary<string, int>
                    {
                        ["minX"] = minX,
                        ["maxX"] = maxX,
                        ["minZ"] = minZ,
                        ["maxZ"] = maxZ
                    }
                };
            }
        }

        #endregion
    }
}
