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
    /// 游戏状态查询和控制处理类
    /// 处理来自 AI Agent 的状态查询和控制请求
    /// </summary>
    public static class GameStateQuery
    {
        /// <summary>
        /// 处理查询/控制请求
        /// </summary>
        public static string HandleQuery(string jsonMessage)
        {
            try
            {
                var query = SimpleJson.DeserializeObject(jsonMessage);
                if (query == null || !query.ContainsKey("action"))
                {
                    return ErrorResponse("Missing 'action' field");
                }

                string action = query["action"]?.ToString();

                switch (action)
                {
                    // 查询命令
                    case "ping":
                        return SuccessResponse(new { status = "ok", timestamp = DateTime.Now.ToString("o") });

                    case "get_all_pawns":
                        return GetAllPawns();

                    case "get_colonists":
                        return GetColonists();

                    case "get_prisoners":
                        return GetPrisoners();

                    case "get_enemies":
                        return GetEnemies();

                    case "get_animals":
                        return GetAnimals();

                    case "get_pawn_info":
                        return GetPawnInfo(query);

                    case "get_game_state":
                        return GetGameState();

                    // 时间和天气查询
                    case "get_time_info":
                        return GetTimeInfo();

                    case "get_weather_info":
                        return GetWeatherInfo();

                    // 物品查询
                    case "get_thing_info":
                        return GetThingInfo(query);

                    // 植物分类查询（无参数，直接获取详细信息）
                    case "get_trees":
                        return GetTrees(query);

                    case "get_crops":
                        return GetCrops(query);

                    case "get_wild_harvestable":
                        return GetWildHarvestable(query);

                    case "get_plant_by_def":
                        return GetPlantByDefName(query);

                    // 物品分类查询（无参数，直接获取详细信息）
                    case "get_food":
                        return GetFood(query);

                    case "get_weapons":
                        return GetWeapons(query);

                    case "get_apparel":
                        return GetApparel(query);

                    case "get_medicine":
                        return GetMedicine(query);

                    case "get_materials":
                        return GetMaterials(query);

                    case "get_item_by_def":
                        return GetItemByDefName(query);

                    // 建筑分类查询（无参数，直接获取详细信息）
                    case "get_production_buildings":
                        return GetProductionBuildings(query);

                    case "get_power_buildings":
                        return GetPowerBuildings(query);

                    case "get_defense_buildings":
                        return GetDefenseBuildings(query);

                    case "get_storage_buildings":
                        return GetStorageBuildings(query);

                    case "get_furniture":
                        return GetFurniture(query);

                    case "get_building_by_def":
                        return GetBuildingByDefName(query);

                    // 控制命令
                    case "move_pawn":
                        return MovePawn(query);

                    case "stop_pawn":
                        return StopPawn(query);

                    case "attack_target":
                        return AttackTarget(query);

                    case "equip_tool":
                        return EquipTool(query);

                    case "unlock_things":
                        return UnlockThings(query);

                    case "place_blueprint":
                        return PlaceBlueprint(query);

                    // 建造管理
                    case "get_blueprints":
                        return GetBlueprints(query);

                    case "cancel_blueprint":
                        return CancelBlueprint(query);

                    case "get_buildable_defs":
                        return GetBuildableDefs(query);

                    // 区域和资源查询
                    case "get_zones":
                        return GetZones(query);

                    case "get_zone_info":
                        return GetZoneInfo(query);

                    // 区域管理控制命令
                    case "create_zone":
                        return CreateZone(query);

                    case "delete_zone":
                        return DeleteZone(query);

                    case "add_cells_to_zone":
                        return AddCellsToZone(query);

                    case "remove_cells_from_zone":
                        return RemoveCellsFromZone(query);

                    case "set_growing_zone_plant":
                        return SetGrowingZonePlant(query);

                    // 储存系统管理
                    case "get_storage_settings":
                        return GetStorageSettings(query);

                    case "set_storage_filter":
                        return SetStorageFilter(query);

                    case "set_storage_priority":
                        return SetStoragePriority(query);

                    case "get_thing_categories":
                        return GetThingCategories(query);

                    case "get_storage_presets":
                        return GetStoragePresets();

                    case "apply_storage_preset":
                        return ApplyStoragePreset(query);

                    case "get_areas":
                        return GetAreas(query);

                    case "get_room_info":
                        return GetRoomInfo(query);

                    case "get_resources":
                        return GetResources(query);

                    case "get_critical_resources":
                        return GetCriticalResources();

                    case "get_wealth":
                        return GetWealth();

                    // 工作系统
                    case "get_work_priorities":
                        return GetWorkPriorities(query);

                    case "set_work_priority":
                        return SetWorkPriority(query);

                    case "get_work_types":
                        return GetWorkTypes();

                    case "get_available_work":
                        return GetAvailableWork(query);

                    // 搬运系统
                    case "get_haulables":
                        return GetHaulables();

                    // 批量工作触发
                    case "trigger_work":
                        return TriggerWork(query);

                    case "get_supported_work_types":
                        return GetSupportedWorkTypes();

                    // ESDF 和 Voronoi 地图分析
                    case "get_esdf_map":
                        return GetESDFMap();

                    case "get_voronoi_map":
                        return GetVoronoiMap();

                    case "get_river":
                        return GetRiverInfo();

                    case "get_marsh":
                        return GetMarshInfo();

                    case "find_build_locations":
                        return FindBuildLocations(query);

                    case "get_room_boundary":
                        return GetRoomBoundary(query);

                    case "scan_area":
                        return ScanArea(query);

                    default:
                        return ErrorResponse($"Unknown action: {action}");
                }
            }
            catch (Exception ex)
            {
                return ErrorResponse($"Error processing query: {ex.Message}");
            }
        }

        #region 查询方法

        /// <summary>
        /// 获取所有已生成的 Pawn
        /// </summary>
        private static string GetAllPawns()
        {
            var pawns = GameStateProvider.GetAllPawnsSpawned();
            if (pawns == null)
            {
                return ErrorResponse("No map available");
            }

            var pawnList = pawns.Select(p => GetPawnSummary(p)).ToList();
            return SuccessResponse(new { count = pawnList.Count, pawns = pawnList });
        }

        /// <summary>
        /// 获取殖民者
        /// </summary>
        private static string GetColonists()
        {
            var colonists = GameStateProvider.GetFreeColonistsSpawned();
            if (colonists == null)
            {
                return ErrorResponse("No map available");
            }

            var pawnList = colonists.Select(p => GetPawnSummary(p)).ToList();
            return SuccessResponse(new { count = pawnList.Count, colonists = pawnList });
        }

        /// <summary>
        /// 获取囚犯
        /// </summary>
        private static string GetPrisoners()
        {
            var prisoners = GameStateProvider.GetPrisonersOfColonySpawned();
            if (prisoners == null)
            {
                return ErrorResponse("No map available");
            }

            var pawnList = prisoners.Select(p => GetPawnSummary(p)).ToList();
            return SuccessResponse(new { count = pawnList.Count, prisoners = pawnList });
        }

        /// <summary>
        /// 获取敌人
        /// </summary>
        private static string GetEnemies()
        {
            var enemies = GameStateProvider.GetEnemies();
            var pawnList = enemies.Select(p => GetPawnSummary(p)).ToList();
            return SuccessResponse(new { count = pawnList.Count, enemies = pawnList });
        }

        /// <summary>
        /// 获取殖民地动物
        /// </summary>
        private static string GetAnimals()
        {
            var animals = GameStateProvider.GetSpawnedColonyAnimals();
            if (animals == null)
            {
                return ErrorResponse("No map available");
            }

            var pawnList = animals.Select(p => GetPawnSummary(p)).ToList();
            return SuccessResponse(new { count = pawnList.Count, animals = pawnList });
        }

        /// <summary>
        /// 获取 Pawn 详细信息
        /// - 不传 pawnId：返回所有殖民者的完整信息
        /// - 传 pawnId：返回指定角色的完整信息
        /// </summary>
        private static string GetPawnInfo(Dictionary<string, object> query)
        {
            var allPawns = GameStateProvider.GetAllPawnsSpawned();
            if (allPawns == null)
            {
                return ErrorResponse("No map available");
            }

            // 如果没有传 pawnId，返回所有殖民者的详细信息
            if (!query.ContainsKey("pawnId") || string.IsNullOrEmpty(query["pawnId"]?.ToString()))
            {
                var colonists = GameStateProvider.GetFreeColonistsSpawned();
                if (colonists == null || colonists.Count == 0)
                {
                    return SuccessResponse(new { count = 0, colonists = new List<object>() });
                }

                var colonistInfoList = colonists.Select(p => GetDetailedPawnInfo(p)).ToList();
                return SuccessResponse(new { count = colonistInfoList.Count, colonists = colonistInfoList });
            }

            // 传了 pawnId，返回指定角色的详细信息
            int pawnId;
            if (!int.TryParse(query["pawnId"]?.ToString(), out pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }

            var pawn = allPawns.FirstOrDefault(p => p.thingIDNumber == pawnId);
            if (pawn == null)
            {
                return ErrorResponse($"Pawn with id {pawnId} not found");
            }

            var info = GetDetailedPawnInfo(pawn);
            return SuccessResponse(info);
        }

        /// <summary>
        /// 获取整体游戏状态
        /// </summary>
        private static string GetGameState()
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            var colonists = GameStateProvider.GetFreeColonistsSpawned();
            var enemies = GameStateProvider.GetEnemies();

            var state = new Dictionary<string, object>
            {
                ["map"] = new
                {
                    size = new { x = map.Size.x, y = map.Size.y, z = map.Size.z },
                    tile = map.Tile
                },
                ["colonists"] = new
                {
                    count = colonists != null ? colonists.Count : 0,
                    list = colonists != null ? colonists.Select(p => GetPawnSummary(p)).ToList() : new List<Dictionary<string, object>>()
                },
                ["enemies"] = new
                {
                    count = enemies.Count,
                    list = enemies.Select(p => GetPawnSummary(p)).ToList()
                }
            };

            return SuccessResponse(state);
        }

        /// <summary>
        /// 获取时间和昼夜信息
        /// </summary>
        private static string GetTimeInfo()
        {
            var result = GameStateProvider.GetTimeInfo();
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取天气信息
        /// </summary>
        private static string GetWeatherInfo()
        {
            var result = GameStateProvider.GetWeatherInfo();
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取单个物品详细信息
        /// 参数: thingId (必需) - 物品ID
        /// </summary>
        private static string GetThingInfo(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("thingId"))
            {
                return ErrorResponse("Missing 'thingId' field");
            }

            int thingId;
            if (!int.TryParse(query["thingId"]?.ToString(), out thingId))
            {
                return ErrorResponse("Invalid 'thingId' format");
            }

            var thing = GameStateProvider.GetThingById(thingId);
            if (thing == null)
            {
                return ErrorResponse($"Thing with id {thingId} not found");
            }

            var info = GameStateProvider.GetThingInfo(thing);
            return SuccessResponse(info);
        }

        /// <summary>
        /// 获取所有树木（无参数）
        /// 返回按类型细分的树木列表（橡树、松树、白杨等），包含每棵树的详细信息
        /// </summary>
        private static string GetTrees(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            return SuccessResponse(GameStateProvider.GetTreesDetailed(map));
        }

        /// <summary>
        /// 获取所有农作物（无参数）
        /// 返回按类型细分的农作物列表（玉米、土豆、水稻等），包含每棵作物的详细信息
        /// </summary>
        private static string GetCrops(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            return SuccessResponse(GameStateProvider.GetCropsDetailed(map));
        }

        /// <summary>
        /// 获取野生可收获植物（无参数）
        /// 返回按类型细分的野生可收获植物列表（野生浆果、仙人掌等）
        /// </summary>
        private static string GetWildHarvestable(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            return SuccessResponse(GameStateProvider.GetWildHarvestableDetailed(map));
        }

        /// <summary>
        /// 按defName获取特定类型的植物（带参数）
        /// 参数: defName - 植物定义名，如 Plant_Corn, Plant_TreeOak
        /// </summary>
        private static string GetPlantByDefName(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("defName"))
            {
                return ErrorResponse("Missing required field: defName (e.g., Plant_Corn, Plant_TreeOak)");
            }

            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            string defName = query["defName"].ToString();
            return SuccessResponse(GameStateProvider.GetPlantsByDefName(defName, map));
        }

        #region 物品分类查询

        /// <summary>
        /// 获取所有食物（无参数）
        /// </summary>
        private static string GetFood(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetFoodDetailed(map));
        }

        /// <summary>
        /// 获取所有武器（无参数）
        /// </summary>
        private static string GetWeapons(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetWeaponsDetailed(map));
        }

        /// <summary>
        /// 获取所有衣物（无参数）
        /// </summary>
        private static string GetApparel(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetApparelDetailed(map));
        }

        /// <summary>
        /// 获取所有药品（无参数）
        /// </summary>
        private static string GetMedicine(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetMedicineDetailed(map));
        }

        /// <summary>
        /// 获取所有材料（无参数）
        /// </summary>
        private static string GetMaterials(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetMaterialsDetailed(map));
        }

        /// <summary>
        /// 按defName获取特定物品
        /// </summary>
        private static string GetItemByDefName(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("defName"))
                return ErrorResponse("Missing required field: defName");

            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            string defName = query["defName"].ToString();
            return SuccessResponse(GameStateProvider.GetItemByDefName(defName, map));
        }

        #endregion

        #region 建筑分类查询

        /// <summary>
        /// 获取所有生产建筑（无参数）
        /// </summary>
        private static string GetProductionBuildings(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetProductionBuildingsDetailed(map));
        }

        /// <summary>
        /// 获取所有电力建筑（无参数）
        /// </summary>
        private static string GetPowerBuildings(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetPowerBuildingsDetailed(map));
        }

        /// <summary>
        /// 获取所有防御建筑（无参数）
        /// </summary>
        private static string GetDefenseBuildings(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetDefenseBuildingsDetailed(map));
        }

        /// <summary>
        /// 获取所有储存建筑（无参数）
        /// </summary>
        private static string GetStorageBuildings(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetStorageBuildingsDetailed(map));
        }

        /// <summary>
        /// 获取所有家具（无参数）
        /// </summary>
        private static string GetFurniture(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            return SuccessResponse(GameStateProvider.GetFurnitureDetailed(map));
        }

        /// <summary>
        /// 按defName获取特定建筑
        /// </summary>
        private static string GetBuildingByDefName(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("defName"))
                return ErrorResponse("Missing required field: defName");

            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
                return ErrorResponse("No current map available");

            string defName = query["defName"].ToString();
            return SuccessResponse(GameStateProvider.GetBuildingByDefName(defName, map));
        }

        #endregion

        /// <summary>
        /// 获取所有区域信息
        /// 参数: detailed (可选) - 是否返回详细信息
        /// </summary>
        private static string GetZones(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            bool detailed = query.ContainsKey("detailed") && query["detailed"]?.ToString().ToLower() == "true";

            var zones = GameStateProvider.GetAllZones(map);
            if (zones == null || zones.Count == 0)
            {
                return SuccessResponse(new { count = 0, zones = new List<object>(), stats = GameStateProvider.GetZonesStatistics(map) });
            }

            var zoneList = zones.Select(z => detailed
                ? GameStateProvider.GetZoneDetailedInfo(z)
                : GameStateProvider.GetZoneBasicInfo(z)).ToList();

            return SuccessResponse(new
            {
                count = zoneList.Count,
                stats = GameStateProvider.GetZonesStatistics(map),
                zones = zoneList
            });
        }

        /// <summary>
        /// 获取单个区域详细信息
        /// 参数: zoneId (必需)
        /// </summary>
        private static string GetZoneInfo(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            int zoneId;
            if (!int.TryParse(query["zoneId"]?.ToString(), out zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            var zones = GameStateProvider.GetAllZones();
            var zone = zones?.FirstOrDefault(z => z.ID == zoneId);

            if (zone == null)
            {
                return ErrorResponse($"Zone with id {zoneId} not found");
            }

            return SuccessResponse(GameStateProvider.GetZoneDetailedInfo(zone));
        }

        /// <summary>
        /// 创建新区域
        /// </summary>
        private static string CreateZone(Dictionary<string, object> query)
        {
            // 验证必需参数
            if (!query.ContainsKey("type"))
            {
                return ErrorResponse("Missing 'type' field. Supported types: stockpile, growing");
            }

            string zoneType = query["type"]?.ToString()?.ToLower();

            // 解析可选的格子列表
            List<IntVec3> cells = null;
            if (query.ContainsKey("cells"))
            {
                cells = ParseCellsList(query["cells"]);
            }

            var result = GameStateProvider.CreateZone(zoneType, cells);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 删除区域
        /// </summary>
        private static string DeleteZone(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            int zoneId;
            if (!int.TryParse(query["zoneId"]?.ToString(), out zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            var result = GameStateProvider.DeleteZone(zoneId);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 向区域添加格子
        /// </summary>
        private static string AddCellsToZone(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            if (!query.ContainsKey("cells"))
            {
                return ErrorResponse("Missing 'cells' field");
            }

            int zoneId;
            if (!int.TryParse(query["zoneId"]?.ToString(), out zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            var cells = ParseCellsList(query["cells"]);
            if (cells == null || cells.Count == 0)
            {
                return ErrorResponse("Invalid 'cells' format. Expected array of {x, z} objects");
            }

            var result = GameStateProvider.AddCellsToZone(zoneId, cells);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 从区域移除格子
        /// </summary>
        private static string RemoveCellsFromZone(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            if (!query.ContainsKey("cells"))
            {
                return ErrorResponse("Missing 'cells' field");
            }

            int zoneId;
            if (!int.TryParse(query["zoneId"]?.ToString(), out zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            var cells = ParseCellsList(query["cells"]);
            if (cells == null || cells.Count == 0)
            {
                return ErrorResponse("Invalid 'cells' format. Expected array of {x, z} objects");
            }

            var result = GameStateProvider.RemoveCellsFromZone(zoneId, cells);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 设置种植区的作物
        /// </summary>
        private static string SetGrowingZonePlant(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            if (!query.ContainsKey("plantDefName"))
            {
                return ErrorResponse("Missing 'plantDefName' field");
            }

            int zoneId;
            if (!int.TryParse(query["zoneId"]?.ToString(), out zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            string plantDefName = query["plantDefName"]?.ToString();

            var result = GameStateProvider.SetGrowingZonePlant(zoneId, plantDefName);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 解析格子坐标列表
        /// </summary>
        /// <param name="cellsObj">cells参数，可能是字符串或IList</param>
        /// <returns>IntVec3列表</returns>
        ///
        /// <remarks>
        /// [重要]SimpleJson的局限性
        /// ============================
        /// 本项目使用自定义的 SimpleJson.DeserializeObject 来解析JSON。
        /// 该实现有一个已知限制：对于JSON数组，它只返回原始字符串，而不是解析后的数组对象。
        ///
        /// 例如，当收到: {"cells": [{"x":100,"z":100}]}
        /// SimpleJson会将 cells 解析为字符串: "[{\"x\":100,\"z\":100}]"
        /// 而不是 List<object> 或 object[]
        ///
        /// 因此，本函数需要手动解析这种字符串格式的JSON数组。
        ///
        /// [支持的格式]
        /// - 对象格式: [{"x":100,"z":100}, {"x":101,"z":101}]
        /// - 数组格式: [[100,100], [101,101]]
        /// </remarks>
        ///
        /// <history>
        /// 2024-02: 修复 - 增加 strings格式的JSON数组手动解析，解决 add_cells_to_zone 报错问题
        /// </history>
        private static List<IntVec3> ParseCellsList(object cellsObj)
        {
            var result = new List<IntVec3>();

            if (cellsObj == null) return result;

            try
            {
                // ========================================
                // 情况1: cells 是字符串（SimpleJson的默认行为）
                // 需要手动解析JSON数组字符串
                // ========================================
                string cellsStr = cellsObj as string;
                if (cellsStr != null)
                {
                    cellsStr = cellsStr.Trim();

                    // 确认是数组格式 [...]
                    if (!cellsStr.StartsWith("[") || !cellsStr.EndsWith("]"))
                        return result;

                    // 去掉外层的 []
                    cellsStr = cellsStr.Substring(1, cellsStr.Length - 2).Trim();

                    // 遍历数组中的每个元素
                    int i = 0;
                    while (i < cellsStr.Length)
                    {
                        // 跳过空白和逗号分隔符
                        while (i < cellsStr.Length && (char.IsWhiteSpace(cellsStr[i]) || cellsStr[i] == ','))
                            i++;

                        if (i >= cellsStr.Length) break;

                        // ----------------------------------------
                        // 元素是对象格式: {"x":100,"z":100}
                        // ----------------------------------------
                        if (cellsStr[i] == '{')
                        {
                            // 提取整个 {...} 块（处理嵌套和字符串内的特殊字符）
                            int depth = 1;
                            int start = i;
                            i++;
                            while (i < cellsStr.Length && depth > 0)
                            {
                                if (cellsStr[i] == '{') depth++;
                                else if (cellsStr[i] == '}') depth--;
                                else if (cellsStr[i] == '"')
                                {
                                    // 跳过字符串内容
                                    i++;
                                    while (i < cellsStr.Length && cellsStr[i] != '"')
                                    {
                                        if (cellsStr[i] == '\\') i++; // 跳过转义字符
                                        i++;
                                    }
                                }
                                i++;
                            }
                            string objStr = cellsStr.Substring(start, i - start);

                            // 使用 SimpleJson 解析这个对象字符串
                            int x = 0, z = 0;
                            var objDict = SimpleJson.DeserializeObject(objStr);
                            if (objDict != null)
                            {
                                if (objDict.ContainsKey("x")) int.TryParse(objDict["x"]?.ToString(), out x);
                                if (objDict.ContainsKey("z")) int.TryParse(objDict["z"]?.ToString(), out z);
                                result.Add(new IntVec3(x, 0, z));
                            }
                        }
                        // ----------------------------------------
                        // 元素是数组格式: [100,100]
                        // ----------------------------------------
                        else if (cellsStr[i] == '[')
                        {
                            // 提取整个 [...] 块
                            int depth = 1;
                            int start = i;
                            i++;
                            while (i < cellsStr.Length && depth > 0)
                            {
                                if (cellsStr[i] == '[') depth++;
                                else if (cellsStr[i] == ']') depth--;
                                i++;
                            }
                            string arrStr = cellsStr.Substring(start, i - start);

                            // 解析 [x, z] 格式
                            arrStr = arrStr.TrimStart('[').TrimEnd(']');
                            var parts = arrStr.Split(',');
                            if (parts.Length >= 2)
                            {
                                int x = 0, z = 0;
                                int.TryParse(parts[0].Trim(), out x);
                                int.TryParse(parts[1].Trim(), out z);
                                result.Add(new IntVec3(x, 0, z));
                            }
                        }
                        else
                        {
                            // 未知格式，跳过
                            i++;
                        }
                    }
                    return result;
                }

                // ========================================
                // 情况2: cells 是 IList（备用，如果将来改进了SimpleJson）
                // ========================================
                if (cellsObj is System.Collections.IList list)
                {
                    foreach (var item in list)
                    {
                        if (item is Dictionary<string, object> dict)
                        {
                            int x = 0, z = 0;
                            if (dict.ContainsKey("x") && int.TryParse(dict["x"]?.ToString(), out x) &&
                                dict.ContainsKey("z") && int.TryParse(dict["z"]?.ToString(), out z))
                            {
                                result.Add(new IntVec3(x, 0, z));
                            }
                        }
                        else if (item is System.Collections.IList coordList && coordList.Count >= 2)
                        {
                            int x = 0, z = 0;
                            if (int.TryParse(coordList[0]?.ToString(), out x) &&
                                int.TryParse(coordList[1]?.ToString(), out z))
                            {
                                result.Add(new IntVec3(x, 0, z));
                            }
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        /// <summary>
        /// 获取所有活动区域
        /// </summary>
        private static string GetAreas(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            var areas = GameStateProvider.GetAllAreas(map);
            if (areas == null || areas.Count == 0)
            {
                return SuccessResponse(new { count = 0, areas = new List<object>() });
            }

            var areaList = areas.Select(a => GameStateProvider.GetAreaInfo(a)).ToList();

            return SuccessResponse(new
            {
                count = areaList.Count,
                areas = areaList
            });
        }

        /// <summary>
        /// 获取指定位置的房间信息
        /// 参数: x, z (必需)
        /// </summary>
        private static string GetRoomInfo(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("x") || !query.ContainsKey("z"))
            {
                return ErrorResponse("Missing required fields: x, z");
            }

            int x, z;
            if (!int.TryParse(query["x"]?.ToString(), out x) || !int.TryParse(query["z"]?.ToString(), out z))
            {
                return ErrorResponse("Invalid 'x' or 'z' format");
            }

            var room = GameStateProvider.GetRoomAt(new IntVec3(x, 0, z));
            if (room == null)
            {
                return SuccessResponse(new { room = (object)null, message = "No room at this location" });
            }

            return SuccessResponse(GameStateProvider.GetRoomInfo(room));
        }

        /// <summary>
        /// 获取资源统计
        /// 参数: summary (可选) - 是否返回摘要形式
        /// </summary>
        private static string GetResources(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            bool summary = !query.ContainsKey("summary") || query["summary"]?.ToString().ToLower() != "false";

            if (summary)
            {
                return SuccessResponse(GameStateProvider.GetResourceSummary(map));
            }
            else
            {
                var resources = GameStateProvider.GetAllResourceCounts(map);
                var resourceList = resources?
                    .Where(kvp => kvp.Value > 0)
                    .Select(kvp => new Dictionary<string, object>
                    {
                        ["defName"] = kvp.Key.defName,
                        ["label"] = kvp.Key.label ?? kvp.Key.defName,
                        ["count"] = kvp.Value
                    }).ToList() ?? new List<Dictionary<string, object>>();

                return SuccessResponse(new
                {
                    count = resourceList.Count,
                    resources = resourceList
                });
            }
        }

        /// <summary>
        /// 获取关键资源概览
        /// </summary>
        private static string GetCriticalResources()
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            return SuccessResponse(GameStateProvider.GetCriticalResourcesOverview(map));
        }

        /// <summary>
        /// 获取财富概览
        /// </summary>
        private static string GetWealth()
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            return SuccessResponse(GameStateProvider.GetWealthOverview(map));
        }

        #endregion

        #region 工作系统查询

        /// <summary>
        /// 获取角色工作优先级
        /// 参数: pawnId (必需)
        /// </summary>
        private static string GetWorkPriorities(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("pawnId"))
            {
                return ErrorResponse("Missing 'pawnId' field");
            }

            if (!int.TryParse(query["pawnId"]?.ToString(), out int pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }

            var pawn = FindPawnById(pawnId, out var pawnError);
            if (pawn == null)
            {
                return ErrorResponse(pawnError);
            }

            return SuccessResponse(GameStateProvider.GetPawnWorkPriorities(pawn));
        }

        /// <summary>
        /// 设置工作优先级
        /// 参数: pawnId, workType (必需), priority (必需, 0-4)
        /// </summary>
        private static string SetWorkPriority(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("pawnId") || !query.ContainsKey("workType") || !query.ContainsKey("priority"))
            {
                return ErrorResponse("Missing required fields: pawnId, workType, priority");
            }

            if (!int.TryParse(query["pawnId"]?.ToString(), out int pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }

            string workTypeDefName = query["workType"]?.ToString();
            var workType = DefDatabase<WorkTypeDef>.GetNamedSilentFail(workTypeDefName);
            if (workType == null)
            {
                return ErrorResponse($"WorkType '{workTypeDefName}' not found");
            }

            if (!int.TryParse(query["priority"]?.ToString(), out int priority) || priority < 0 || priority > 4)
            {
                return ErrorResponse("Invalid 'priority' (must be 0-4)");
            }

            var pawn = FindPawnById(pawnId, out var pawnError);
            if (pawn == null)
            {
                return ErrorResponse(pawnError);
            }

            bool success = GameStateProvider.SetPawnWorkPriority(pawn, workType, priority);
            if (!success)
            {
                return ErrorResponse($"Failed to set work priority (pawn may have disabled work type)");
            }

            return SuccessResponse(new
            {
                message = $"Set {workType.label} priority to {priority} for {pawn.LabelShort}",
                pawnId = pawnId,
                workType = workTypeDefName,
                priority = priority
            });
        }

        /// <summary>
        /// 获取所有工作类型
        /// </summary>
        private static string GetWorkTypes()
        {
            return SuccessResponse(new
            {
                count = GameStateProvider.GetAllWorkTypes().Count,
                workTypes = GameStateProvider.GetAllWorkTypes()
            });
        }

        /// <summary>
        /// 获取可分配的工作
        /// 参数: pawnId (必需)
        /// </summary>
        private static string GetAvailableWork(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("pawnId"))
            {
                return ErrorResponse("Missing 'pawnId' field");
            }

            if (!int.TryParse(query["pawnId"]?.ToString(), out int pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }

            var pawn = FindPawnById(pawnId, out var pawnError);
            if (pawn == null)
            {
                return ErrorResponse(pawnError);
            }

            return SuccessResponse(GameStateProvider.GetAvailableWorkForPawn(pawn));
        }

        #endregion

        #region 搬运系统查询

        /// <summary>
        /// 获取需要搬运的物品
        /// </summary>
        private static string GetHaulables()
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No current map available");
            }

            return SuccessResponse(GameStateProvider.GetHaulablesOverview(map));
        }


        /// <summary>
        /// 触发指定类型的工作，自动分配空闲殖民者
        /// </summary>
        private static string TriggerWork(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("workType"))
            {
                return ErrorResponse("Missing required field: workType. Supported types: CutPlant, Harvest, Mine, Construct, Repair, Clean, Haul");
            }

            string workType = query["workType"]?.ToString();
            if (string.IsNullOrEmpty(workType))
            {
                return ErrorResponse("workType cannot be empty");
            }

            var result = GameStateProvider.TriggerWork(workType);
            return SimpleJson.SerializeObject(result);
        }

        /// <summary>
        /// 获取所有支持的工作类型
        /// </summary>
        private static string GetSupportedWorkTypes()
        {
            var result = GameStateProvider.GetSupportedWorkTypes();
            return SimpleJson.SerializeObject(result);
        }

        #endregion

        #region 控制命令

        /// <summary>
        /// 移动角色到指定位置
        /// 需要: pawnId, x, z
        /// </summary>
        private static string MovePawn(Dictionary<string, object> query)
        {
            // 参数验证
            if (!query.ContainsKey("pawnId") || !query.ContainsKey("x") || !query.ContainsKey("z"))
            {
                return ErrorResponse("Missing required fields: pawnId, x, z");
            }

            int pawnId;
            if (!int.TryParse(query["pawnId"]?.ToString(), out pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }

            int x, z;
            if (!int.TryParse(query["x"]?.ToString(), out x) || !int.TryParse(query["z"]?.ToString(), out z))
            {
                return ErrorResponse("Invalid 'x' or 'z' format");
            }

            // 查找 Pawn
            var allPawns = GameStateProvider.GetAllPawnsSpawned();
            if (allPawns == null)
            {
                return ErrorResponse("No map available");
            }

            var pawn = allPawns.FirstOrDefault(p => p.thingIDNumber == pawnId);
            if (pawn == null)
            {
                return ErrorResponse($"Pawn with id {pawnId} not found");
            }

            // 检查角色状态
            if (pawn.Downed)
            {
                return ErrorResponse($"Pawn {pawn.LabelShort} is downed and cannot move");
            }
            if (pawn.Dead)
            {
                return ErrorResponse($"Pawn {pawn.LabelShort} is dead");
            }

            // 创建目标位置
            var targetCell = new IntVec3(x, 0, z);
            var map = GameStateProvider.GetCurrentMap();

            // 检查目标位置是否有效
            if (!targetCell.InBounds(map))
            {
                return ErrorResponse($"Target position ({x}, {z}) is out of map bounds");
            }

            // 创建移动任务
            Job gotoJob = JobMaker.MakeJob(JobDefOf.Goto, new LocalTargetInfo(targetCell));
            gotoJob.locomotionUrgency = LocomotionUrgency.Jog;

            // 启动任务
            pawn.jobs.StartJob(gotoJob, JobCondition.InterruptOptional);

            return SuccessResponse(new
            {
                message = $"Moving {pawn.LabelShort} to ({x}, {z})",
                pawnId = pawnId,
                targetPosition = new { x = x, z = z }
            });
        }

        /// <summary>
        /// 停止角色当前任务
        /// 需要: pawnId
        /// </summary>
        private static string StopPawn(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("pawnId"))
            {
                return ErrorResponse("Missing required field: pawnId");
            }

            int pawnId;
            if (!int.TryParse(query["pawnId"]?.ToString(), out pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }

            var allPawns = GameStateProvider.GetAllPawnsSpawned();
            if (allPawns == null)
            {
                return ErrorResponse("No map available");
            }

            var pawn = allPawns.FirstOrDefault(p => p.thingIDNumber == pawnId);
            if (pawn == null)
            {
                return ErrorResponse($"Pawn with id {pawnId} not found");
            }

            // 结束当前任务
            if (pawn.jobs.curJob != null)
            {
                string prevJob = pawn.jobs.curJob.def?.defName ?? "Unknown";
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);

                return SuccessResponse(new
                {
                    message = $"Stopped {pawn.LabelShort}'s current job",
                    pawnId = pawnId,
                    previousJob = prevJob
                });
            }
            else
            {
                return SuccessResponse(new
                {
                    message = $"{pawn.LabelShort} has no current job",
                    pawnId = pawnId
                });
            }
        }

        /// <summary>
        /// 攻击目标
        /// 需要: pawnId, targetId
        /// </summary>
        private static string AttackTarget(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("pawnId") || !query.ContainsKey("targetId"))
            {
                return ErrorResponse("Missing required fields: pawnId, targetId");
            }

            int pawnId, targetId;
            if (!int.TryParse(query["pawnId"]?.ToString(), out pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }
            if (!int.TryParse(query["targetId"]?.ToString(), out targetId))
            {
                return ErrorResponse("Invalid 'targetId' format");
            }

            var allPawns = GameStateProvider.GetAllPawnsSpawned();
            if (allPawns == null)
            {
                return ErrorResponse("No map available");
            }

            var attacker = allPawns.FirstOrDefault(p => p.thingIDNumber == pawnId);
            if (attacker == null)
            {
                return ErrorResponse($"Attacker pawn with id {pawnId} not found");
            }

            var target = allPawns.FirstOrDefault(p => p.thingIDNumber == targetId);
            if (target == null)
            {
                return ErrorResponse($"Target pawn with id {targetId} not found");
            }

            // 检查攻击者状态
            if (attacker.Downed)
            {
                return ErrorResponse($"Attacker {attacker.LabelShort} is downed");
            }
            if (attacker.Dead)
            {
                return ErrorResponse($"Attacker {attacker.LabelShort} is dead");
            }

            // 选择攻击类型（根据武器）
            JobDef attackDef = JobDefOf.AttackMelee;
            if (attacker.equipment?.Primary != null)
            {
                attackDef = JobDefOf.AttackStatic;
            }

            // 创建攻击任务
            Job attackJob = JobMaker.MakeJob(attackDef, new LocalTargetInfo(target));
            attacker.jobs.StartJob(attackJob, JobCondition.InterruptOptional);

            return SuccessResponse(new
            {
                message = $"{attacker.LabelShort} is attacking {target.LabelShort}",
                attackerId = pawnId,
                targetId = targetId,
                attackType = attackDef.defName
            });
        }

        /// <summary>
        /// 装备武器/工具
        /// 参数: pawnId, thingId
        /// 使用 JobDefOf.Equip 让 pawn 走到物品位置并装备
        /// </summary>
        private static string EquipTool(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("pawnId") || !query.ContainsKey("thingId"))
            {
                return ErrorResponse("Missing required fields: pawnId, thingId");
            }

            int pawnId, thingId;
            if (!int.TryParse(query["pawnId"]?.ToString(), out pawnId))
            {
                return ErrorResponse("Invalid 'pawnId' format");
            }
            if (!int.TryParse(query["thingId"]?.ToString(), out thingId))
            {
                return ErrorResponse("Invalid 'thingId' format");
            }

            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No map available");
            }

            // 获取 pawn
            var pawn = GameStateProvider.GetAllPawnsSpawned()?.FirstOrDefault(p => p.thingIDNumber == pawnId);
            if (pawn == null)
            {
                return ErrorResponse($"Pawn with id {pawnId} not found");
            }

            // 检查 pawn 状态
            if (pawn.Downed)
            {
                return ErrorResponse($"{pawn.LabelShort} is downed");
            }
            if (pawn.Dead)
            {
                return ErrorResponse($"{pawn.LabelShort} is dead");
            }

            // 获取要装备的物品
            var thing = map.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
            if (thing == null)
            {
                return ErrorResponse($"Thing with id {thingId} not found");
            }

            // 检查是否为可装备物品
            if (!(thing is ThingWithComps thingWithComps))
            {
                return ErrorResponse($"{thing.LabelShort} is not equippable");
            }

            // 检查物品类型（必须是 Primary 装备）
            if (thing.def.equipmentType != EquipmentType.Primary)
            {
                return ErrorResponse($"{thing.LabelShort} is not a primary equipment (type: {thing.def.equipmentType})");
            }

            // 记录当前武器（用于响应）
            var currentWeapon = pawn.equipment?.Primary;
            string previousWeapon = currentWeapon?.LabelShort ?? "None";

            // 创建装备任务 - JobDefOf.Equip 会让 pawn 走到物品位置并装备
            Job equipJob = JobMaker.MakeJob(JobDefOf.Equip, new LocalTargetInfo(thing));
            pawn.jobs.StartJob(equipJob, JobCondition.InterruptOptional);

            return SuccessResponse(new
            {
                message = $"{pawn.LabelShort} is equipping {thing.LabelShort}",
                pawnId = pawnId,
                thingId = thingId,
                thingLabel = thing.LabelShort,
                previousWeapon = previousWeapon
            });
        }

        /// <summary>
        /// 解锁被禁止的物品
        /// 参数: thingId (可选, 指定物品ID), all (可选, 解锁所有被禁止的物品)
        /// 使用 ForbidUtility.SetForbidden(false) 解锁物品
        /// </summary>
        private static string UnlockThings(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No map available");
            }

            var unlockedItems = new List<object>();

            // 检查是否解锁特定物品
            if (query.ContainsKey("thingId") && int.TryParse(query["thingId"]?.ToString(), out int thingId))
            {
                var thing = map.listerThings.AllThings.FirstOrDefault(t => t.thingIDNumber == thingId);
                if (thing == null)
                {
                    return ErrorResponse($"Thing with id {thingId} not found");
                }

                // 检查物品是否被禁止
                if (!thing.IsForbidden(Faction.OfPlayer))
                {
                    return SuccessResponse(new
                    {
                        message = $"Thing {thing.Label} is already unlocked",
                        thingId = thingId,
                        wasForbidden = false
                    });
                }

                // 解锁物品 - 使用 ForbidUtility.SetForbidden 扩展方法
                thing.SetForbidden(false);

                unlockedItems.Add(new
                {
                    id = thing.thingIDNumber,
                    label = thing.Label,
                    position = new { x = thing.Position.x, z = thing.Position.z }
                });
            }
            else
            {
                // 解锁所有被禁止的物品（当 all=true 或未指定 thingId 时）
                bool unlockAll = true;
                if (query.ContainsKey("all"))
                {
                    bool.TryParse(query["all"]?.ToString(), out unlockAll);
                }

                if (!unlockAll)
                {
                    return ErrorResponse("No valid parameters provided. Use 'thingId' to unlock specific item or 'all=true' to unlock all forbidden items");
                }

                // 获取所有被禁止的物品
                var forbiddenThings = map.listerThings.AllThings
                    .Where(t => t.IsForbidden(Faction.OfPlayer) && t.def.EverHaulable)
                    .ToList();

                foreach (var thing in forbiddenThings)
                {
                    try
                    {
                        // 解锁物品
                        thing.SetForbidden(false);

                        unlockedItems.Add(new
                        {
                            id = thing.thingIDNumber,
                            label = thing.Label,
                            position = new { x = thing.Position.x, z = thing.Position.z }
                        });
                    }
                    catch (Exception ex)
                    {
                        // 记录但继续处理其他物品
                        Log.Warning($"[RimWorldAI] Failed to unlock thing {thing.ThingID}: {ex.Message}");
                    }
                }
            }

            return SuccessResponse(new
            {
                message = $"Unlocked {unlockedItems.Count} forbidden item(s)",
                count = unlockedItems.Count,
                items = unlockedItems
            });
        }

        /// <summary>
        /// 放置建造蓝图
        /// 使用 GenConstruct.PlaceBlueprintForBuild API
        /// </summary>
        private static string PlaceBlueprint(Dictionary<string, object> query)
        {
            var map = GameStateProvider.GetCurrentMap();
            if (map == null)
            {
                return ErrorResponse("No map available");
            }

            // 获取必需参数
            if (!query.ContainsKey("defName") || string.IsNullOrEmpty(query["defName"]?.ToString()))
            {
                return ErrorResponse("Missing required parameter: defName");
            }

            if (!query.ContainsKey("x") || !query.ContainsKey("z"))
            {
                return ErrorResponse("Missing required parameters: x, z");
            }

            string defName = query["defName"].ToString();

            // 解析坐标
            if (!int.TryParse(query["x"]?.ToString(), out int x) || !int.TryParse(query["z"]?.ToString(), out int z))
            {
                return ErrorResponse("Invalid coordinates: x and z must be integers");
            }

            // 获取建筑定义
            // 依据: DefDatabase<T>.GetNamed 用于通过 defName 获取定义
            ThingDef buildingDef = DefDatabase<ThingDef>.GetNamed(defName, false);
            if (buildingDef == null)
            {
                return ErrorResponse($"Building definition not found: {defName}");
            }

            // 检查是否是可以建造的建筑
            if (buildingDef.category != ThingCategory.Building)
            {
                return ErrorResponse($"Definition '{defName}' is not a building (category: {buildingDef.category})");
            }

            // 创建位置
            IntVec3 position = new IntVec3(x, 0, z);

            // 检查位置是否有效
            if (!position.InBounds(map))
            {
                return ErrorResponse($"Position ({x}, {z}) is out of map bounds");
            }

            // 获取可选的材料定义
            ThingDef stuffDef = null;
            if (query.ContainsKey("stuffDefName") && !string.IsNullOrEmpty(query["stuffDefName"]?.ToString()))
            {
                string stuffDefName = query["stuffDefName"].ToString();
                stuffDef = DefDatabase<ThingDef>.GetNamed(stuffDefName, false);
                if (stuffDef == null)
                {
                    return ErrorResponse($"Stuff definition not found: {stuffDefName}");
                }
            }

            // 获取可选的旋转参数
            Rot4 rotation = Rot4.North;
            if (query.ContainsKey("rotation") && !string.IsNullOrEmpty(query["rotation"]?.ToString()))
            {
                string rotStr = query["rotation"].ToString().ToLower();
                switch (rotStr)
                {
                    case "0":
                    case "north":
                        rotation = Rot4.North;
                        break;
                    case "1":
                    case "east":
                        rotation = Rot4.East;
                        break;
                    case "2":
                    case "south":
                        rotation = Rot4.South;
                        break;
                    case "3":
                    case "west":
                        rotation = Rot4.West;
                        break;
                    default:
                        return ErrorResponse($"Invalid rotation: {rotStr}. Use 0-3 or north/east/south/west");
                }
            }

            try
            {
                // 检查是否可以在此位置放置蓝图
                // 依据: GenConstruct.CanPlaceBlueprintAt 用于检查蓝图放置条件
                var acceptanceReport = GenConstruct.CanPlaceBlueprintAt(buildingDef, position, rotation, map, stuffDef: stuffDef);
                if (!acceptanceReport.Accepted)
                {
                    // 获取位置上的阻挡物信息，提供更详细的错误原因
                    var blockingThings = position.GetThingList(map);
                    var blockingInfo = new List<string>();
                    foreach (var thing in blockingThings)
                    {
                        if (thing != null)
                        {
                            blockingInfo.Add($"{thing.def?.defName ?? "Unknown"}({thing.LabelShort ?? thing.def?.label ?? "unknown"})");
                        }
                    }

                    string blockingDetail = blockingInfo.Count > 0
                        ? $" 已有物体: [{string.Join(", ", blockingInfo)}]"
                        : "";

                    // 检查地形类型
                    var terrain = position.GetTerrain(map);
                    string terrainInfo = terrain != null ? $" 地形: {terrain.defName}" : "";

                    return ErrorResponse($"无法在({x}, {z})放置蓝图: {acceptanceReport.Reason}{blockingDetail}{terrainInfo}");
                }

                // 放置蓝图
                // 依据: GenConstruct.PlaceBlueprintForBuild 签名:
                // PlaceBlueprintForBuild(BuildableDef sourceDef, IntVec3 center, Map map, Rot4 rotation, Faction faction, ThingDef stuff, ...)
                Blueprint_Build blueprint = GenConstruct.PlaceBlueprintForBuild(buildingDef, position, map, rotation, Faction.OfPlayer, stuffDef);

                if (blueprint == null)
                {
                    return ErrorResponse("放置蓝图失败 - 未知错误");
                }

                return SuccessResponse(new
                {
                    message = $"蓝图放置成功",
                    blueprint = new
                    {
                        id = blueprint.thingIDNumber,
                        defName = blueprint.def.defName,
                        label = blueprint.Label,
                        position = new { x = blueprint.Position.x, z = blueprint.Position.z },
                        rotation = rotation.AsInt
                    },
                    buildingDef = defName,
                    stuffDef = stuffDef?.defName,
                    costList = buildingDef.CostList?.Select(c => new
                    {
                        thingDef = c.thingDef?.defName,
                        count = c.count
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return ErrorResponse($"放置蓝图异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有待建造的蓝图列表
        /// </summary>
        private static string GetBlueprints(Dictionary<string, object> query)
        {
            var result = GameStateProvider.GetBlueprintsInfo();
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 取消指定的建造蓝图
        /// </summary>
        private static string CancelBlueprint(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("blueprintId") || !int.TryParse(query["blueprintId"]?.ToString(), out int blueprintId))
            {
                return ErrorResponse("Missing or invalid 'blueprintId' parameter");
            }

            var result = GameStateProvider.CancelBlueprint(blueprintId);
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取可建造的建筑定义列表
        /// </summary>
        private static string GetBuildableDefs(Dictionary<string, object> query)
        {
            string category = null;
            if (query.ContainsKey("category"))
            {
                category = query["category"]?.ToString()?.ToLower();
            }

            var result = GameStateProvider.GetBuildableDefs(category);
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取 Pawn 摘要信息（用于列表）
        /// 基于反编译的 Pawn 类，返回所有已验证的布尔属性
        /// </summary>
        private static Dictionary<string, object> GetPawnSummary(Pawn pawn)
        {
            if (pawn == null) return null;

            return new Dictionary<string, object>
            {
                // 基础信息
                ["id"] = pawn.thingIDNumber,
                ["name"] = pawn.LabelShort,
                ["position"] = new { x = pawn.Position.x, z = pawn.Position.z },
                ["faction"] = pawn.Faction?.Name ?? "None",

                // 健康状态
                ["healthPercent"] = pawn.health?.summaryHealth?.SummaryHealthPercent ?? 1f,

                // 身份状态（已验证）
                ["isColonist"] = pawn.IsColonist,
                ["isPrisoner"] = pawn.IsPrisoner,
                ["isSlave"] = pawn.IsSlave,
                ["isFreeColonist"] = pawn.IsFreeColonist,
                ["isFreeNonSlaveColonist"] = pawn.IsFreeNonSlaveColonist,

                // 身体状态（已验证）
                ["isDowned"] = pawn.Downed,
                ["isAsleep"] = !pawn.Awake(),  // 是否在睡觉
                ["isDead"] = pawn.Dead,
                ["isDrafted"] = pawn.Drafted,
                ["inBed"] = pawn.InBed(),  // 是否在床上
                ["canTakeOrder"] = pawn.CanTakeOrder,  // 是否可以接受命令
                ["isCrawling"] = pawn.Crawling,  // 是否在爬行

                // 种族属性（已验证）
                ["isAnimal"] = pawn.RaceProps?.Animal ?? false,
                ["isHumanlike"] = pawn.RaceProps?.Humanlike ?? false,
                ["isMechanoid"] = pawn.RaceProps?.IsMechanoid ?? false,

                // 心理状态（已验证）
                ["isInspired"] = pawn.Inspired != null,
                ["inMentalState"] = pawn.InMentalState,

                // 当前任务
                ["curJob"] = pawn.CurJob?.def?.defName ?? "None",
                ["curJobReport"] = pawn.jobs?.curDriver?.GetReport() ?? "无"  // 当前任务描述（如"正在睡觉"）
            };
        }

        /// <summary>
        /// 获取 Pawn 详细信息
        /// 基于反编译的 Pawn 类，返回完整的角色属性
        /// </summary>
        private static Dictionary<string, object> GetDetailedPawnInfo(Pawn pawn)
        {
            if (pawn == null) return null;

            var info = new Dictionary<string, object>
            {
                // ==================== 基础信息 ====================
                ["id"] = pawn.thingIDNumber,
                ["name"] = pawn.LabelShort ?? "Unknown",
                ["fullName"] = pawn.Name?.ToStringFull ?? pawn.LabelShort ?? "Unknown",
                ["race"] = pawn.def?.label ?? "Unknown",
                ["raceDefName"] = pawn.def?.defName ?? "Unknown",
                ["kindDef"] = pawn.kindDef?.defName ?? "Unknown",
                ["gender"] = pawn.gender.ToString(),
                ["position"] = new { x = pawn.Position.x, y = pawn.Position.y, z = pawn.Position.z },
                ["faction"] = SafeGetFactionInfo(pawn),

                // ==================== 年龄信息 ====================
                ["age"] = SafeCall(() => GetAgeInfo(pawn)),

                // ==================== 身体属性 ====================
                ["bodyStats"] = SafeCall(() => new Dictionary<string, object>
                {
                    ["bodySize"] = pawn.BodySize,
                    ["healthScale"] = pawn.HealthScale,
                    ["developmentalStage"] = pawn.DevelopmentalStage.ToString()
                }),

                // ==================== 身份状态 ====================
                ["identity"] = SafeCall(() => new Dictionary<string, object>
                {
                    ["isColonist"] = pawn.IsColonist,
                    ["isPrisoner"] = pawn.IsPrisoner,
                    ["isSlave"] = pawn.IsSlave,
                    ["isFreeColonist"] = pawn.IsFreeColonist,
                    ["isFreeNonSlaveColonist"] = pawn.IsFreeNonSlaveColonist,
                    ["isMutant"] = pawn.IsMutant,
                    ["isShambler"] = pawn.IsShambler,
                    ["isGhoul"] = pawn.IsGhoul,
                    ["guestStatus"] = pawn.GuestStatus?.ToString() ?? "None",
                    ["canTakeOrder"] = pawn.CanTakeOrder
                }),

                // ==================== 身体状态 ====================
                ["bodyState"] = SafeCall(() => new Dictionary<string, object>
                {
                    ["isDowned"] = pawn.Downed,
                    ["isAsleep"] = !pawn.Awake(),  // 是否在睡觉
                    ["isDead"] = pawn.Dead,
                    ["isDrafted"] = pawn.Drafted,
                    ["inBed"] = pawn.InBed(),  // 是否在床上
                    ["canTakeOrder"] = pawn.CanTakeOrder,  // 是否可以接受命令
                    ["deathresting"] = pawn.Deathresting,
                    ["teleporting"] = pawn.teleporting,
                    ["flying"] = pawn.Flying,
                    ["swimming"] = pawn.Swimming,
                    ["crawling"] = pawn.Crawling
                }),

                // ==================== 健康状态 ====================
                ["health"] = SafeCall(() => GetHealthInfo(pawn)),

                // ==================== 种族属性 ====================
                ["raceProps"] = SafeCall(() => new Dictionary<string, object>
                {
                    ["isAnimal"] = pawn.RaceProps?.Animal ?? false,
                    ["isHumanlike"] = pawn.RaceProps?.Humanlike ?? false,
                    ["isMechanoid"] = pawn.RaceProps?.IsMechanoid ?? false,
                    ["isFlesh"] = pawn.RaceProps?.IsFlesh ?? false,
                    ["intelligence"] = pawn.RaceProps != null ? pawn.RaceProps.intelligence.ToString() : "Unknown"
                }),

                // ==================== 心理状态 ====================
                ["mentalState"] = SafeCall(() => new Dictionary<string, object>
                {
                    ["isInspired"] = pawn.Inspired,
                    ["inspirationDefName"] = pawn.Inspiration?.def?.defName ?? "None",
                    ["inspirationLabel"] = pawn.Inspiration?.def?.label ?? "None",
                    ["inMentalState"] = pawn.InMentalState,
                    ["mentalStateDefName"] = pawn.MentalStateDef?.defName ?? "None",
                    ["mentalStateLabel"] = pawn.MentalStateDef?.label ?? "None",
                    ["inAggroMentalState"] = pawn.InAggroMentalState
                }),

                // ==================== 当前任务 ====================
                ["job"] = SafeCall(() => GetJobInfo(pawn)),

                // ==================== 技能信息 ====================
                ["skills"] = SafeCall(() => GetSkillsInfo(pawn)),

                // ==================== 需求信息 ====================
                ["needs"] = SafeCall(() => GetNeedsInfo(pawn)),

                // ==================== 装备信息 ====================
                ["equipment"] = SafeCall(() => GetEquipmentInfo(pawn)),

                // ==================== 服装信息 ====================
                ["apparel"] = SafeCall(() => GetApparelInfo(pawn)),

                // ==================== 物品栏信息 ====================
                ["inventory"] = SafeCall(() => GetInventoryInfo(pawn)),

                // ==================== 特性信息 ====================
                ["traits"] = SafeCall(() => GetTraitsInfo(pawn)),

                // ==================== 能力信息 ====================
                ["abilities"] = SafeCall(() => GetAbilitiesInfo(pawn)),

                // ==================== 基因信息 ====================
                ["genes"] = SafeCall(() => GetGenesInfo(pawn)),

                // ==================== 皇权信息 ====================
                ["royalty"] = SafeCall(() => GetRoyaltyInfo(pawn)),

                // ==================== 信仰信息 ====================
                ["ideo"] = SafeCall(() => GetIdeoInfo(pawn)),

                // ==================== 工作设置 ====================
                ["workSettings"] = SafeCall(() => GetWorkSettingsInfo(pawn)),

                // ==================== 精神能力 ====================
                ["psychic"] = SafeCall(() => GetPsychicInfo(pawn)),

                // ==================== 社交关系 ====================
                ["relations"] = SafeCall(() => GetRelationsInfo(pawn)),

                // ==================== 思想/心情因素 ====================
                ["thoughts"] = SafeCall(() => GetThoughtsInfo(pawn)),

                // ==================== 环境感知 ====================
                ["environment"] = SafeCall(() => GetEnvironmentInfo(pawn)),

                // ==================== 详细心情信息 ====================
                ["moodDetail"] = SafeCall(() => GetMoodDetailInfo(pawn))
            };

            return info;
        }

        /// <summary>
        /// 安全调用辅助方法，捕获异常
        /// </summary>
        private static object SafeCall(Func<object> func)
        {
            try
            {
                return func();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 安全获取阵营信息
        /// </summary>
        private static object SafeGetFactionInfo(Pawn pawn)
        {
            try
            {
                return new Dictionary<string, object>
                {
                    ["name"] = pawn.Faction?.Name ?? "None",
                    ["defName"] = pawn.Faction?.def?.defName ?? "None",
                    ["isPlayer"] = pawn.Faction == Faction.OfPlayer
                };
            }
            catch
            {
                return new Dictionary<string, object>
                {
                    ["name"] = "None",
                    ["defName"] = "None",
                    ["isPlayer"] = false
                };
            }
        }

        #region 详细信息辅助方法

        /// <summary>
        /// 获取年龄信息
        /// </summary>
        private static Dictionary<string, object> GetAgeInfo(Pawn pawn)
        {
            var ageTracker = pawn.ageTracker;
            if (ageTracker == null) return null;

            return new Dictionary<string, object>
            {
                ["biologicalAgeYears"] = (float)ageTracker.AgeBiologicalYears,
                ["chronologicalAgeYears"] = (float)ageTracker.AgeChronologicalYears,
                ["lifeStageIndex"] = ageTracker.CurLifeStageIndex,
                ["lifeStageDef"] = ageTracker.CurLifeStage?.defName ?? "Unknown"
            };
        }

        /// <summary>
        /// 获取健康详细信息
        /// </summary>
        private static Dictionary<string, object> GetHealthInfo(Pawn pawn)
        {
            var health = pawn.health;
            if (health == null) return null;

            // 获取可见的健康问题
            var hediffs = new List<Dictionary<string, object>>();
            if (health.hediffSet?.hediffs != null)
            {
                foreach (var hediff in health.hediffSet.hediffs)
                {
                    if (hediff.Visible)
                    {
                        hediffs.Add(new Dictionary<string, object>
                        {
                            ["def"] = hediff.def?.defName ?? "Unknown",
                            ["label"] = hediff.Label ?? "",
                            ["severity"] = hediff.Severity,
                            ["part"] = hediff.Part?.Label ?? ""
                        });
                    }
                }
            }

            return new Dictionary<string, object>
            {
                ["summaryHealthPercent"] = health.summaryHealth?.SummaryHealthPercent ?? 1f,
                ["isDead"] = health.Dead,
                ["isDowned"] = health.Downed,
                ["hediffCount"] = hediffs.Count,
                ["hediffs"] = hediffs,
                ["painLevel"] = health.hediffSet?.PainTotal ?? 0f
            };
        }

        /// <summary>
        /// 获取工作信息
        /// </summary>
        private static Dictionary<string, object> GetJobInfo(Pawn pawn)
        {
            var job = pawn.CurJob;
            if (job == null)
            {
                return new Dictionary<string, object>
                {
                    ["curJob"] = "None",
                    ["curJobDef"] = "None",
                    ["report"] = "无任务",
                    ["target"] = (object)null
                };
            }

            var targetInfo = new Dictionary<string, object>();
            if (job.targetA.HasThing)
            {
                targetInfo["targetA"] = new
                {
                    type = "Thing",
                    def = job.targetA.Thing?.def?.defName ?? "Unknown",
                    label = job.targetA.Thing?.LabelShort ?? "Unknown",
                    position = new { x = job.targetA.Cell.x, z = job.targetA.Cell.z }
                };
            }
            else if (job.targetA.Cell.IsValid)
            {
                targetInfo["targetA"] = new
                {
                    type = "Cell",
                    position = new { x = job.targetA.Cell.x, z = job.targetA.Cell.z }
                };
            }

            return new Dictionary<string, object>
            {
                ["curJob"] = job.def?.defName ?? "None",
                ["curJobDef"] = job.def?.defName ?? "None",
                ["report"] = pawn.jobs?.curDriver?.GetReport() ?? "未知",  // 人类可读的任务描述
                ["target"] = targetInfo,
                ["locomotionUrgency"] = job.locomotionUrgency.ToString()
            };
        }

        /// <summary>
        /// 获取技能信息
        /// </summary>
        private static Dictionary<string, object> GetSkillsInfo(Pawn pawn)
        {
            var skills = pawn.skills;
            if (skills == null) return null;

            var skillList = new List<Dictionary<string, object>>();
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
            {
                var skillRecord = skills.GetSkill(skillDef);
                if (skillRecord != null)
                {
                    skillList.Add(new Dictionary<string, object>
                    {
                        ["def"] = skillDef.defName,
                        ["label"] = skillDef.label ?? skillDef.defName,
                        ["level"] = skillRecord.levelInt,
                        ["xpSinceLastLevel"] = skillRecord.xpSinceLastLevel,
                        ["passion"] = skillRecord.passion.ToString(),
                        ["totallyDisabled"] = skillRecord.TotallyDisabled
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["count"] = skillList.Count,
                ["skills"] = skillList
            };
        }

        /// <summary>
        /// 获取需求信息
        /// </summary>
        private static Dictionary<string, object> GetNeedsInfo(Pawn pawn)
        {
            var needs = pawn.needs;
            if (needs == null) return null;

            var needList = new List<Dictionary<string, object>>();
            if (needs.AllNeeds != null)
            {
                foreach (var need in needs.AllNeeds)
                {
                    needList.Add(new Dictionary<string, object>
                    {
                        ["def"] = need.def?.defName ?? "Unknown",
                        ["label"] = need.def?.label ?? "Unknown",
                        ["curLevel"] = need.CurLevel,
                        ["curLevelPercentage"] = need.CurLevelPercentage
                    });
                }
            }

            // 心情特殊处理
            var mood = needs.mood;
            var moodInfo = mood != null ? new Dictionary<string, object>
            {
                ["curLevel"] = mood.CurLevel,
                ["curLevelPercentage"] = mood.CurLevelPercentage
            } : null;

            return new Dictionary<string, object>
            {
                ["count"] = needList.Count,
                ["needs"] = needList,
                ["mood"] = moodInfo
            };
        }

        /// <summary>
        /// 获取装备信息
        /// </summary>
        private static Dictionary<string, object> GetEquipmentInfo(Pawn pawn)
        {
            var equipment = pawn.equipment;
            if (equipment == null) return null;

            var equipList = new List<Dictionary<string, object>>();
            if (equipment.AllEquipmentListForReading != null)
            {
                foreach (var eq in equipment.AllEquipmentListForReading)
                {
                    equipList.Add(new Dictionary<string, object>
                    {
                        ["def"] = eq.def?.defName ?? "Unknown",
                        ["label"] = eq.LabelShort ?? "Unknown",
                        ["hitPoints"] = eq.HitPoints,
                        ["maxHitPoints"] = eq.MaxHitPoints
                    });
                }
            }

            var primary = equipment.Primary;
            var primaryInfo = primary != null ? new Dictionary<string, object>
            {
                ["def"] = primary.def?.defName ?? "Unknown",
                ["label"] = primary.LabelShort ?? "Unknown",
                ["hitPoints"] = primary.HitPoints,
                ["maxHitPoints"] = primary.MaxHitPoints
            } : null;

            return new Dictionary<string, object>
            {
                ["count"] = equipList.Count,
                ["equipment"] = equipList,
                ["primary"] = primaryInfo
            };
        }

        /// <summary>
        /// 获取服装信息
        /// </summary>
        private static Dictionary<string, object> GetApparelInfo(Pawn pawn)
        {
            var apparel = pawn.apparel;
            if (apparel == null) return null;

            var apparelList = new List<Dictionary<string, object>>();
            if (apparel.WornApparel != null)
            {
                foreach (var app in apparel.WornApparel)
                {
                    apparelList.Add(new Dictionary<string, object>
                    {
                        ["def"] = app.def?.defName ?? "Unknown",
                        ["label"] = app.LabelShort ?? "Unknown",
                        ["hitPoints"] = app.HitPoints,
                        ["maxHitPoints"] = app.MaxHitPoints,
                        ["layer"] = app.def?.apparel?.LastLayer?.ToString() ?? "Unknown"
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["count"] = apparelList.Count,
                ["apparel"] = apparelList
            };
        }

        /// <summary>
        /// 获取物品栏信息
        /// </summary>
        private static Dictionary<string, object> GetInventoryInfo(Pawn pawn)
        {
            var inventory = pawn.inventory;
            if (inventory == null) return null;

            var itemList = new List<Dictionary<string, object>>();
            if (inventory.innerContainer != null)
            {
                foreach (var item in inventory.innerContainer)
                {
                    itemList.Add(new Dictionary<string, object>
                    {
                        ["def"] = item.def?.defName ?? "Unknown",
                        ["label"] = item.LabelShort ?? "Unknown",
                        ["stackCount"] = item.stackCount
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["count"] = itemList.Count,
                ["totalStackCount"] = itemList.Sum(i => (int)i["stackCount"]),
                ["items"] = itemList
            };
        }

        /// <summary>
        /// 获取特性信息
        /// </summary>
        private static Dictionary<string, object> GetTraitsInfo(Pawn pawn)
        {
            var story = pawn.story;
            if (story?.traits == null) return null;

            var traitList = new List<Dictionary<string, object>>();
            foreach (var trait in story.traits.allTraits)
            {
                traitList.Add(new Dictionary<string, object>
                {
                    ["def"] = trait.def?.defName ?? "Unknown",
                    ["label"] = trait.LabelCap ?? "Unknown",
                    ["degree"] = trait.Degree
                });
            }

            return new Dictionary<string, object>
            {
                ["count"] = traitList.Count,
                ["traits"] = traitList
            };
        }

        /// <summary>
        /// 获取能力信息
        /// </summary>
        private static Dictionary<string, object> GetAbilitiesInfo(Pawn pawn)
        {
            var abilities = pawn.abilities;
            if (abilities == null) return null;

            var abilityList = new List<Dictionary<string, object>>();
            if (abilities.AllAbilitiesForReading != null)
            {
                foreach (var ability in abilities.AllAbilitiesForReading)
                {
                    abilityList.Add(new Dictionary<string, object>
                    {
                        ["def"] = ability.def?.defName ?? "Unknown",
                        ["label"] = ability.def?.label ?? "Unknown",
                        ["canCast"] = ability.CanCast
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["count"] = abilityList.Count,
                ["abilities"] = abilityList
            };
        }

        /// <summary>
        /// 获取基因信息
        /// </summary>
        private static Dictionary<string, object> GetGenesInfo(Pawn pawn)
        {
            var genes = pawn.genes;
            if (genes == null) return null;

            var geneList = new List<Dictionary<string, object>>();
            if (genes.GenesListForReading != null)
            {
                foreach (var gene in genes.GenesListForReading)
                {
                    geneList.Add(new Dictionary<string, object>
                    {
                        ["def"] = gene.def?.defName ?? "Unknown",
                        ["label"] = gene.def?.label ?? "Unknown",
                        ["isOverridden"] = gene.Overridden
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["xenotypeDef"] = genes.Xenotype?.defName ?? "Baseliner",
                ["xenotypeLabel"] = genes.XenotypeLabel ?? "Baseliner",
                ["count"] = geneList.Count,
                ["genes"] = geneList
            };
        }

        /// <summary>
        /// 获取皇权信息
        /// </summary>
        private static Dictionary<string, object> GetRoyaltyInfo(Pawn pawn)
        {
            var royalty = pawn.royalty;
            if (royalty == null) return null;

            var titleList = new List<Dictionary<string, object>>();
            if (royalty.AllTitlesForReading != null)
            {
                foreach (var title in royalty.AllTitlesForReading)
                {
                    titleList.Add(new Dictionary<string, object>
                    {
                        ["def"] = title.def?.defName ?? "Unknown",
                        ["label"] = title.def?.label ?? "Unknown",
                        ["faction"] = title.faction?.Name ?? "Unknown"
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["hasTitle"] = titleList.Count > 0,
                ["titles"] = titleList
            };
        }

        /// <summary>
        /// 获取信仰信息
        /// </summary>
        private static Dictionary<string, object> GetIdeoInfo(Pawn pawn)
        {
            var ideo = pawn.ideo;
            if (ideo == null) return null;

            var ideoDef = ideo.Ideo;

            return new Dictionary<string, object>
            {
                ["hasIdeo"] = ideoDef != null,
                ["name"] = ideoDef?.name ?? "None",
                ["memberName"] = ideoDef?.memberName ?? "None",
                ["certainty"] = ideo.Certainty
            };
        }

        /// <summary>
        /// 获取工作设置信息
        /// </summary>
        private static Dictionary<string, object> GetWorkSettingsInfo(Pawn pawn)
        {
            var workSettings = pawn.workSettings;
            if (workSettings == null) return null;

            var workPriorities = new List<Dictionary<string, object>>();
            foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefs)
            {
                var priority = workSettings.GetPriority(workTypeDef);
                if (priority > 0)
                {
                    workPriorities.Add(new Dictionary<string, object>
                    {
                        ["def"] = workTypeDef.defName,
                        ["label"] = workTypeDef.labelShort ?? workTypeDef.defName,
                        ["priority"] = priority
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["everWork"] = workSettings.EverWork,
                ["count"] = workPriorities.Count,
                ["priorities"] = workPriorities
            };
        }

        /// <summary>
        /// 获取精神能力信息
        /// </summary>
        private static Dictionary<string, object> GetPsychicInfo(Pawn pawn)
        {
            var psychicEntropy = pawn.psychicEntropy;
            if (psychicEntropy == null)
            {
                return new Dictionary<string, object>
                {
                    ["hasPsylink"] = pawn.HasPsylink,
                    ["psylinkLevel"] = 0,
                    ["entropyValue"] = 0f,
                    ["maxEntropy"] = 0f
                };
            }

            return new Dictionary<string, object>
            {
                ["hasPsylink"] = pawn.HasPsylink,
                ["psylinkLevel"] = psychicEntropy.Psylink?.level ?? 0,
                ["entropyValue"] = psychicEntropy.EntropyValue,
                ["maxEntropy"] = psychicEntropy.MaxEntropy
            };
        }

        /// <summary>
        /// 获取社交关系信息
        /// </summary>
        private static Dictionary<string, object> GetRelationsInfo(Pawn pawn)
        {
            var relations = pawn.relations;
            if (relations == null) return null;

            var relationList = new List<Dictionary<string, object>>();
            var directRelations = relations.DirectRelations;
            if (directRelations != null)
            {
                foreach (var relation in directRelations)
                {
                    relationList.Add(new Dictionary<string, object>
                    {
                        ["def"] = relation.def?.defName ?? "Unknown",
                        ["label"] = relation.def?.label ?? "Unknown",
                        ["otherPawnId"] = relation.otherPawn?.thingIDNumber ?? -1,
                        ["otherPawnName"] = relation.otherPawn?.LabelShort ?? "Unknown"
                    });
                }
            }

            return new Dictionary<string, object>
            {
                ["count"] = relationList.Count,
                ["relations"] = relationList
            };
        }

        /// <summary>
        /// 获取思想/心情因素信息
        /// </summary>
        private static Dictionary<string, object> GetThoughtsInfo(Pawn pawn)
        {
            var thoughtsHandler = pawn.needs?.mood?.thoughts;
            if (thoughtsHandler == null) return null;

            var thoughtList = new List<Dictionary<string, object>>();

            // 获取记忆类思想 (Memories) - 这是最重要的心情因素
            var memories = thoughtsHandler.memories?.Memories;
            if (memories != null)
            {
                foreach (var thought in memories)
                {
                    if (thought == null) continue;

                    try
                    {
                        var moodOffset = thought.MoodOffset();
                        var label = thought.def?.label;
                        if (string.IsNullOrEmpty(label))
                        {
                            try { label = thought.LabelCap; } catch { label = "Unknown"; }
                        }

                        var thoughtInfo = new Dictionary<string, object>
                        {
                            ["defName"] = thought.def?.defName ?? "Unknown",
                            ["label"] = label ?? "Unknown",
                            ["description"] = thought.Description ?? "",
                            ["moodOffset"] = moodOffset,
                            ["stage"] = thought.CurStage?.label ?? "",
                            ["stageIndex"] = thought.CurStageIndex,
                            ["type"] = "memory",
                            ["category"] = moodOffset > 0 ? "positive" : (moodOffset < 0 ? "negative" : "neutral"),
                            ["age"] = thought.age,
                            ["duration"] = thought.def?.DurationTicks ?? 0
                        };

                        thoughtList.Add(thoughtInfo);
                    }
                    catch
                    {
                        // 跳过无法处理的思想
                    }
                }
            }

            // 按心情影响排序（负面在前）
            var sortedThoughts = thoughtList.OrderBy(t => t.TryGetValue("moodOffset", out var offset) ? Convert.ToSingle(offset) : 0).ToList();

            // 统计
            var positiveCount = sortedThoughts.Count(t => t.TryGetValue("category", out var cat) && cat?.ToString() == "positive");
            var negativeCount = sortedThoughts.Count(t => t.TryGetValue("category", out var cat) && cat?.ToString() == "negative");
            var totalMoodOffset = sortedThoughts.Sum(t => t.TryGetValue("moodOffset", out var offset) ? Convert.ToSingle(offset) : 0);

            return new Dictionary<string, object>
            {
                ["count"] = sortedThoughts.Count,
                ["positiveCount"] = positiveCount,
                ["negativeCount"] = negativeCount,
                ["totalMoodOffset"] = totalMoodOffset,
                ["thoughts"] = sortedThoughts
            };
        }

        /// <summary>
        /// 获取环境感知信息（温度、室内外、湿身状态等）
        /// </summary>
        private static Dictionary<string, object> GetEnvironmentInfo(Pawn pawn)
        {
            if (!pawn.Spawned) return null;

            var map = pawn.Map;
            var position = pawn.Position;

            // 温度信息 - 使用 AmbientTemperature
            float ambientTemp = 0f;
            try { ambientTemp = GenTemperature.GetTemperatureForCell(position, map); } catch { }

            // 舒适温度范围 - 简化处理
            float comfortTempMin = -999f;
            float comfortTempMax = 999f;
            // 不使用 GetComfortableTemperatureRange，使用固定值作为后备

            // 判断温度状态
            string tempStatus = "comfortable";
            if (ambientTemp < comfortTempMin)
                tempStatus = "too_cold";
            else if (ambientTemp > comfortTempMax)
                tempStatus = "too_hot";

            // 室内外状态
            bool isOutdoors = position.UsesOutdoorTemperature(map);
            bool hasRoof = false;
            try { hasRoof = map.roofGrid.Roofed(position); } catch { }

            // 房间信息
            var room = position.GetRoom(map);
            var roomInfo = room != null ? new Dictionary<string, object>
            {
                ["id"] = room.ID,
                ["temperature"] = room.Temperature,
                ["roomType"] = room.Role?.defName ?? "Unknown",
                ["cellCount"] = room.CellCount,
                ["isHuge"] = room.IsHuge,
                ["isPrisonCell"] = room.IsPrisonCell
            } : null;

            // 湿身状态 - 检查是否有 "Wet" 相关的 Hediff
            bool isWet = false;
            float wetnessLevel = 0f;
            try
            {
                // 尝试通过名称查找 Wet hediff
                var hediffs = pawn.health?.hediffSet?.hediffs;
                if (hediffs != null)
                {
                    foreach (var hediff in hediffs)
                    {
                        if (hediff?.def?.defName?.Contains("Wet") == true)
                        {
                            isWet = true;
                            wetnessLevel = hediff.Severity;
                            break;
                        }
                    }
                }
            } catch { }

            // 其他环境状态
            bool isInWater = false;
            try { isInWater = position.GetTerrain(map)?.defName?.Contains("Water") ?? false; } catch { }

            bool isForbidden = false;
            try { isForbidden = position.IsForbidden(pawn); } catch { }

            return new Dictionary<string, object>
            {
                // 温度
                ["ambientTemperature"] = ambientTemp,
                ["comfortTempMin"] = comfortTempMin,
                ["comfortTempMax"] = comfortTempMax,
                ["tempStatus"] = tempStatus,  // comfortable/too_cold/too_hot

                // 位置
                ["isOutdoors"] = isOutdoors,
                ["hasRoof"] = hasRoof,

                // 房间
                ["room"] = roomInfo,

                // 湿身
                ["isWet"] = isWet,
                ["wetnessLevel"] = wetnessLevel,  // 0-1

                // 其他
                ["isInWater"] = isInWater,
                ["isForbiddenArea"] = isForbidden
            };
        }

        /// <summary>
        /// 获取详细心情信息
        /// </summary>
        private static Dictionary<string, object> GetMoodDetailInfo(Pawn pawn)
        {
            var needs = pawn.needs;
            if (needs == null) return null;

            var mood = needs.mood;
            if (mood == null) return null;

            // 心情值 (0-100)
            float moodLevel = mood.CurLevel * 100f;  // 转换为0-100
            float moodPercentage = mood.CurLevelPercentage * 100f;

            // 崩溃阈值
            float mentalBreakThreshold = 0.3f;
            float majorBreakThreshold = 0.2f;
            float extremeBreakThreshold = 0.1f;
            bool canBreak = false;

            try
            {
                var breaker = pawn.mindState?.mentalBreaker;
                if (breaker != null)
                {
                    mentalBreakThreshold = breaker.BreakThresholdMinor;
                    majorBreakThreshold = breaker.BreakThresholdMajor;
                    extremeBreakThreshold = breaker.BreakThresholdExtreme;
                }
            } catch { }

            // 判断心情状态
            string moodStatus = "stable";
            if (mood.CurLevel < extremeBreakThreshold)
                moodStatus = "critical";  // 极度危险
            else if (mood.CurLevel < majorBreakThreshold)
                moodStatus = "danger";    // 危险
            else if (mood.CurLevel < mentalBreakThreshold)
                moodStatus = "warning";   // 警告
            else if (mood.CurLevel < 0.5f)
                moodStatus = "low";       // 偏低

            // 距离崩溃的距离
            float distanceToBreak = mood.CurLevel - mentalBreakThreshold;

            return new Dictionary<string, object>
            {
                // 心情值
                ["moodLevel"] = moodLevel,           // 0-100
                ["moodPercentage"] = moodPercentage, // 0-100

                // 崩溃阈值 (百分比格式)
                ["breakThresholdMinor"] = mentalBreakThreshold * 100f,    // 轻度崩溃阈值
                ["breakThresholdMajor"] = majorBreakThreshold * 100f,     // 中度崩溃阈值
                ["breakThresholdExtreme"] = extremeBreakThreshold * 100f, // 极端崩溃阈值

                // 心情状态
                ["moodStatus"] = moodStatus,  // stable/low/warning/danger/critical

                // 距离崩溃
                ["distanceToBreak"] = distanceToBreak * 100f,  // 正数=安全，负数=已低于阈值

                // 是否可能崩溃
                ["canHaveMentalBreak"] = canBreak
            };
        }

        #endregion

        /// <summary>
        /// 成功响应
        /// </summary>
        private static string SuccessResponse(object data)
        {
            var response = new Dictionary<string, object>
            {
                ["success"] = true,
                ["data"] = data
            };
            return SimpleJson.SerializeObject(response);
        }

        /// <summary>
        /// 错误响应
        /// </summary>
        private static string ErrorResponse(string message)
        {
            var response = new Dictionary<string, object>
            {
                ["success"] = false,
                ["error"] = message
            };
            return SimpleJson.SerializeObject(response);
        }

        /// <summary>
        /// 根据 ID 查找 Pawn
        /// </summary>
        /// <param name="pawnId">Pawn 的 thingIDNumber</param>
        /// <param name="error">如果未找到，返回错误信息</param>
        /// <returns>找到的 Pawn，或 null</returns>
        private static Pawn FindPawnById(int pawnId, out string error)
        {
            var pawn = GameStateProvider.GetAllPawnsSpawned()?.FirstOrDefault(p => p.thingIDNumber == pawnId);
            if (pawn == null)
            {
                error = $"Pawn with id {pawnId} not found";
            }
            else
            {
                error = null;
            }
            return pawn;
        }

        #endregion

        #region 储存系统管理

        /// <summary>
        /// 获取储存区设置
        /// 参数: zoneId (必需)
        /// </summary>
        private static string GetStorageSettings(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            if (!int.TryParse(query["zoneId"]?.ToString(), out int zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            var result = GameStateProvider.GetStorageSettings(zoneId);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 设置储存区物品过滤
        /// 参数: zoneId (必需), mode (可选), categories (可选), defs (可选), allow (可选)
        /// </summary>
        private static string SetStorageFilter(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            if (!int.TryParse(query["zoneId"]?.ToString(), out int zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            string mode = query.ContainsKey("mode") ? query["mode"]?.ToString() : null;

            List<string> categories = null;
            if (query.ContainsKey("categories"))
            {
                categories = ParseStringList(query["categories"]);
            }

            List<string> defs = null;
            if (query.ContainsKey("defs"))
            {
                defs = ParseStringList(query["defs"]);
            }

            bool allow = true;
            if (query.ContainsKey("allow"))
            {
                bool.TryParse(query["allow"]?.ToString(), out allow);
            }

            var result = GameStateProvider.SetStorageFilter(zoneId, mode, categories, defs, allow);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 设置储存区优先级
        /// 参数: zoneId (必需), priority (必需)
        /// </summary>
        private static string SetStoragePriority(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            if (!query.ContainsKey("priority"))
            {
                return ErrorResponse("Missing 'priority' field. Valid values: Low, Normal, Preferred, Important, Critical or 0-5");
            }

            if (!int.TryParse(query["zoneId"]?.ToString(), out int zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            string priority = query["priority"]?.ToString();

            var result = GameStateProvider.SetStoragePriority(zoneId, priority);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取物品类别树
        /// 参数: parentCategory (可选)
        /// </summary>
        private static string GetThingCategories(Dictionary<string, object> query)
        {
            string parentCategory = query.ContainsKey("parentCategory") ? query["parentCategory"]?.ToString() : null;

            var result = GameStateProvider.GetThingCategories(parentCategory);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取储存预设列表
        /// </summary>
        private static string GetStoragePresets()
        {
            var result = GameStateProvider.GetStoragePresets();
            return SuccessResponse(result);
        }

        /// <summary>
        /// 应用储存预设
        /// 参数: zoneId (必需), presetName (必需)
        /// </summary>
        private static string ApplyStoragePreset(Dictionary<string, object> query)
        {
            if (!query.ContainsKey("zoneId"))
            {
                return ErrorResponse("Missing 'zoneId' field");
            }

            if (!query.ContainsKey("presetName"))
            {
                return ErrorResponse("Missing 'presetName' field. Use get_storage_presets to see available presets");
            }

            if (!int.TryParse(query["zoneId"]?.ToString(), out int zoneId))
            {
                return ErrorResponse("Invalid 'zoneId' format");
            }

            string presetName = query["presetName"]?.ToString();

            var result = GameStateProvider.ApplyStoragePreset(zoneId, presetName);

            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }

            return SuccessResponse(result);
        }

        /// <summary>
        /// 解析字符串列表（处理 SimpleJson 返回的字符串格式数组）
        /// </summary>
        private static List<string> ParseStringList(object value)
        {
            var result = new List<string>();

            if (value == null) return result;

            // SimpleJson 可能返回带方括号的字符串
            string str = value.ToString();

            if (str.StartsWith("[") && str.EndsWith("]"))
            {
                // 移除方括号并分割
                str = str.Substring(1, str.Length - 2);
                if (!string.IsNullOrWhiteSpace(str))
                {
                    foreach (var item in str.Split(','))
                    {
                        var trimmed = item.Trim().Trim('"');
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            result.Add(trimmed);
                        }
                    }
                }
            }
            else
            {
                // 单个值
                result.Add(str.Trim('"'));
            }

            return result;
        }

        #endregion

        #region ESDF 和 Voronoi 地图分析

        /// <summary>
        /// 获取 ESDF 地图信息
        /// </summary>
        private static string GetESDFMap()
        {
            var result = GameStateProvider.GetESDFMap();
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取 Voronoi 骨架地图信息
        /// </summary>
        private static string GetVoronoiMap()
        {
            var result = GameStateProvider.GetVoronoiMap();
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 查找适合建造的位置
        /// </summary>
        private static string FindBuildLocations(Dictionary<string, object> query)
        {
            // 解析参数
            string buildingDefName = null;
            if (query.ContainsKey("buildingDef"))
            {
                buildingDefName = query["buildingDef"]?.ToString();
            }

            int minDistance = 1;
            if (query.ContainsKey("minDistance"))
            {
                int.TryParse(query["minDistance"]?.ToString(), out minDistance);
            }

            bool preferIndoor = true;
            if (query.ContainsKey("preferIndoor"))
            {
                bool.TryParse(query["preferIndoor"]?.ToString(), out preferIndoor);
            }

            int limit = 10;
            if (query.ContainsKey("limit"))
            {
                int.TryParse(query["limit"]?.ToString(), out limit);
                limit = Mathf.Min(limit, 100); // 最大100个
            }

            var result = GameStateProvider.FindBuildLocations(buildingDefName, minDistance, preferIndoor, limit);
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取房间边界建造状态（用于AI追踪建造进度）
        /// </summary>
        private static string GetRoomBoundary(Dictionary<string, object> query)
        {
            // 参数验证
            if (!query.ContainsKey("center_x") || !query.ContainsKey("center_z") ||
                !query.ContainsKey("width") || !query.ContainsKey("height"))
            {
                return ErrorResponse("Missing required fields: center_x, center_z, width, height");
            }

            int centerX, centerZ, width, height;
            if (!int.TryParse(query["center_x"]?.ToString(), out centerX) ||
                !int.TryParse(query["center_z"]?.ToString(), out centerZ) ||
                !int.TryParse(query["width"]?.ToString(), out width) ||
                !int.TryParse(query["height"]?.ToString(), out height))
            {
                return ErrorResponse("Invalid parameter format");
            }

            var result = GameStateProvider.GetRoomBoundary(centerX, centerZ, width, height);
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 批量扫描区域（替代多次 get_cell_info 调用）
        /// </summary>
        private static string ScanArea(Dictionary<string, object> query)
        {
            // 参数验证
            if (!query.ContainsKey("center_x") || !query.ContainsKey("center_z") ||
                !query.ContainsKey("width") || !query.ContainsKey("height"))
            {
                return ErrorResponse("Missing required fields: center_x, center_z, width, height");
            }

            int centerX, centerZ, width, height;
            if (!int.TryParse(query["center_x"]?.ToString(), out centerX) ||
                !int.TryParse(query["center_z"]?.ToString(), out centerZ) ||
                !int.TryParse(query["width"]?.ToString(), out width) ||
                !int.TryParse(query["height"]?.ToString(), out height))
            {
                return ErrorResponse("Invalid parameter format");
            }

            var result = GameStateProvider.ScanArea(centerX, centerZ, width, height);
            if (result.ContainsKey("error"))
            {
                return ErrorResponse(result["error"].ToString());
            }
            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取河流信息（BFS 识别连通区域）
        /// </summary>
        private static string GetRiverInfo()
        {
            var result = GameStateProvider.GetRiverInfo();
            return SuccessResponse(result);
        }

        /// <summary>
        /// 获取沼泽信息（BFS 识别连通区域）
        /// </summary>
        private static string GetMarshInfo()
        {
            var result = GameStateProvider.GetMarshInfo();
            return SuccessResponse(result);
        }

        #endregion
    }

    /// <summary>
    /// 简单的 JSON 序列化/反序列化工具
    /// 兼容 .NET 4.7.2，不依赖外部库
    /// </summary>
    public static class SimpleJson
    {
        public static string SerializeObject(object obj)
        {
            if (obj == null) return "null";

            if (obj is string s) return EscapeString(s);
            if (obj is bool b) return b ? "true" : "false";
            if (obj is int i) return i.ToString();
            if (obj is float f) return f.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            if (obj is double d) return d.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            if (obj is Dictionary<string, object> dict)
            {
                var pairs = dict.Select(kvp => $"\"{EscapeJsonString(kvp.Key)}\":{SerializeObject(kvp.Value)}");
                return "{" + string.Join(",", pairs) + "}";
            }

            if (obj is IEnumerable<object> list)
            {
                var items = list.Select(item => SerializeObject(item));
                return "[" + string.Join(",", items) + "]";
            }

            // 匿名对象处理
            var type = obj.GetType();
            if (type.IsAnonymousType() || type.IsClass)
            {
                var parts = new List<string>();

                // 处理公共字段
                var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var field in fields)
                {
                    try
                    {
                        var value = field.GetValue(obj);
                        parts.Add($"\"{EscapeJsonString(field.Name)}\":{SerializeObject(value)}");
                    }
                    catch
                    {
                        parts.Add($"\"{EscapeJsonString(field.Name)}\":null");
                    }
                }

                // 处理公共属性（过滤掉索引器）
                var props = type.GetProperties().Where(prop => prop.GetIndexParameters().Length == 0);
                foreach (var prop in props)
                {
                    try
                    {
                        var value = prop.GetValue(obj);
                        parts.Add($"\"{EscapeJsonString(prop.Name)}\":{SerializeObject(value)}");
                    }
                    catch
                    {
                        parts.Add($"\"{EscapeJsonString(prop.Name)}\":null");
                    }
                }

                return "{" + string.Join(",", parts) + "}";
            }

            return EscapeString(obj.ToString());
        }

        /// <summary>
        /// 解析 JSON 数组为对象列表
        /// </summary>
        public static List<Dictionary<string, object>> DeserializeArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            json = json.Trim();
            if (!json.StartsWith("[") || !json.EndsWith("]")) return null;

            var result = new List<Dictionary<string, object>>();
            json = json.Substring(1, json.Length - 2).Trim();

            int i = 0;
            while (i < json.Length)
            {
                // 跳过空白和逗号
                while (i < json.Length && (char.IsWhiteSpace(json[i]) || json[i] == ','))
                    i++;

                if (i >= json.Length) break;

                // 找到下一个对象
                if (json[i] == '{')
                {
                    int depth = 1;
                    int start = i;
                    i++;
                    while (i < json.Length && depth > 0)
                    {
                        if (json[i] == '{') depth++;
                        else if (json[i] == '}') depth--;
                        else if (json[i] == '"') { i++; while (i < json.Length && json[i] != '"') { if (json[i] == '\\') i++; i++; } }
                        i++;
                    }
                    string objJson = json.Substring(start, i - start);
                    var obj = DeserializeObject(objJson);
                    if (obj != null)
                        result.Add(obj);
                }
                else
                {
                    i++;
                }
            }

            return result;
        }

        public static Dictionary<string, object> DeserializeObject(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}")) return null;

            var result = new Dictionary<string, object>();
            json = json.Substring(1, json.Length - 2).Trim();

            int i = 0;
            while (i < json.Length)
            {
                // 跳过空白和逗号
                while (i < json.Length && (char.IsWhiteSpace(json[i]) || json[i] == ','))
                    i++;

                if (i >= json.Length) break;

                // 解析键
                if (json[i] != '"') { i++; continue; }
                i++;
                int keyStart = i;
                while (i < json.Length && json[i] != '"')
                {
                    if (json[i] == '\\') i++;
                    i++;
                }
                string key = UnescapeJsonString(json.Substring(keyStart, i - keyStart));
                i++;

                // 跳过冒号
                while (i < json.Length && (char.IsWhiteSpace(json[i]) || json[i] == ':'))
                    i++;

                // 解析值
                object value;
                int valueStart = i;
                if (json[i] == '"')
                {
                    // 字符串
                    i++;
                    int strStart = i;
                    while (i < json.Length && json[i] != '"')
                    {
                        if (json[i] == '\\') i++;
                        i++;
                    }
                    value = UnescapeJsonString(json.Substring(strStart, i - strStart));
                    i++;
                }
                else if (json[i] == '{')
                {
                    // 嵌套对象
                    int depth = 1;
                    i++;
                    while (i < json.Length && depth > 0)
                    {
                        if (json[i] == '{') depth++;
                        else if (json[i] == '}') depth--;
                        else if (json[i] == '"') { i++; while (i < json.Length && json[i] != '"') { if (json[i] == '\\') i++; i++; } }
                        i++;
                    }
                    value = DeserializeObject(json.Substring(valueStart, i - valueStart));
                }
                else if (json[i] == '[')
                {
                    // 数组 - 简化处理，返回原始字符串
                    int depth = 1;
                    i++;
                    while (i < json.Length && depth > 0)
                    {
                        if (json[i] == '[') depth++;
                        else if (json[i] == ']') depth--;
                        else if (json[i] == '"') { i++; while (i < json.Length && json[i] != '"') { if (json[i] == '\\') i++; i++; } }
                        i++;
                    }
                    value = json.Substring(valueStart, i - valueStart);
                }
                else if (char.IsDigit(json[i]) || json[i] == '-')
                {
                    // 数字
                    while (i < json.Length && (char.IsDigit(json[i]) || json[i] == '.' || json[i] == '-' || json[i] == 'e' || json[i] == 'E'))
                        i++;
                    string numStr = json.Substring(valueStart, i - valueStart);
                    if (int.TryParse(numStr, out int intVal))
                        value = intVal;
                    else if (double.TryParse(numStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double doubleVal))
                        value = doubleVal;
                    else
                        value = numStr;
                }
                else if (json.Substring(i, 4) == "true")
                {
                    value = true;
                    i += 4;
                }
                else if (json.Substring(i, 5) == "false")
                {
                    value = false;
                    i += 5;
                }
                else if (json.Substring(i, 4) == "null")
                {
                    value = null;
                    i += 4;
                }
                else
                {
                    // 未知类型，跳过
                    while (i < json.Length && json[i] != ',' && json[i] != '}')
                        i++;
                    continue;
                }

                result[key] = value;
            }

            return result;
        }

        private static string EscapeString(string s)
        {
            if (s == null) return "null";
            return "\"" + EscapeJsonString(s) + "\"";
        }

        private static string EscapeJsonString(string s)
        {
            if (s == null) return "";

            var sb = new System.Text.StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        // 转义非ASCII字符为 \uXXXX 格式
                        if (c < 32 || c > 127)
                        {
                            sb.Append("\\u");
                            sb.Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        private static string UnescapeJsonString(string s)
        {
            if (s == null) return "";

            var sb = new System.Text.StringBuilder();
            int i = 0;
            while (i < s.Length)
            {
                if (s[i] == '\\' && i + 1 < s.Length)
                {
                    char next = s[i + 1];
                    switch (next)
                    {
                        case '"': sb.Append('"'); i += 2; break;
                        case '\\': sb.Append('\\'); i += 2; break;
                        case 'n': sb.Append('\n'); i += 2; break;
                        case 'r': sb.Append('\r'); i += 2; break;
                        case 't': sb.Append('\t'); i += 2; break;
                        case 'b': sb.Append('\b'); i += 2; break;
                        case 'f': sb.Append('\f'); i += 2; break;
                        case 'u':
                            if (i + 5 < s.Length)
                            {
                                string hex = s.Substring(i + 2, 4);
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                                {
                                    sb.Append((char)code);
                                    i += 6;
                                    break;
                                }
                            }
                            sb.Append(s[i]);
                            i++;
                            break;
                        default:
                            sb.Append(s[i]);
                            i++;
                            break;
                    }
                }
                else
                {
                    sb.Append(s[i]);
                    i++;
                }
            }
            return sb.ToString();
        }
    }

    // 匿名类型检测扩展
    internal static class TypeExtensions
    {
        internal static bool IsAnonymousType(this Type type)
        {
            if (type == null) return false;
            return type.Name.Contains("AnonymousType") ||
                   (type.Name.StartsWith("<>") && type.Name.Contains("__"));
        }
    }
}
