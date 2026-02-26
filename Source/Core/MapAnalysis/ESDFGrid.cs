using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorldAI.Core.MapAnalysis
{
    /// <summary>
    /// 障碍物信息
    /// 记录障碍物的类型、名称、位置等详细信息
    /// </summary>
    public class ObstacleInfo
    {
        public string type;           // "terrain", "building", "thing"
        public string defName;        // 定义名称 (如 "Wall", "WaterDeep")
        public string label;          // 显示名称 (如 "墙", "深水")
        public int x;                 // 障碍物 X 坐标
        public int z;                 // 障碍物 Z 坐标
        public string category;       // 细分类别 (如 "water", "rock", "structure")

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["type"] = type,
                ["defName"] = defName,
                ["label"] = label,
                ["x"] = x,
                ["z"] = z,
                ["category"] = category
            };
        }
    }

    /// <summary>
    /// ESDF (Euclidean Signed Distance Function) 距离场
    /// 计算每个格子到最近障碍物的距离
    /// 基于 FIESTA 算法：BFS 从障碍物向外扩散
    /// </summary>
    public class ESDFGrid
    {
        private int[,] distances;
        private ObstacleInfo[,] nearestObstacle;  // 每个格子的最近障碍物信息
        private Dictionary<string, Dictionary<string, int>> obstacleTypeCount;  // 障碍物类型统计
        private List<ObstacleInfo> obstacleSamples;  // 障碍物样本列表
        private Map map;
        private bool dirty = true;
        private int mapSizeX;
        private int mapSizeZ;

        // 4方向邻居偏移（曼哈顿距离）
        private static readonly IntVec3[] NeighborOffsets = new[]
        {
            new IntVec3(1, 0, 0),
            new IntVec3(-1, 0, 0),
            new IntVec3(0, 0, 1),
            new IntVec3(0, 0, -1)
        };

        public ESDFGrid(Map map)
        {
            this.map = map;
            this.mapSizeX = map.Size.x;
            this.mapSizeZ = map.Size.z;
            distances = new int[mapSizeX, mapSizeZ];
            nearestObstacle = new ObstacleInfo[mapSizeX, mapSizeZ];
            obstacleTypeCount = new Dictionary<string, Dictionary<string, int>>();
            obstacleSamples = new List<ObstacleInfo>();
        }

        /// <summary>
        /// 标记需要重新计算
        /// </summary>
        public void MarkDirty()
        {
            dirty = true;
        }

        /// <summary>
        /// 获取某格到最近障碍物的距离
        /// </summary>
        public int GetDistance(IntVec3 cell)
        {
            if (dirty)
            {
                Recalculate();
            }
            return distances[cell.x, cell.z];
        }

        /// <summary>
        /// 获取某格到最近障碍物的距离（安全版本，带边界检查）
        /// </summary>
        public int GetDistanceSafe(IntVec3 cell)
        {
            if (!cell.InBounds(map))
            {
                return 0;
            }
            return GetDistance(cell);
        }

        /// <summary>
        /// 判断是否是障碍物（不可通行的格子）
        /// </summary>
        private bool IsObstacle(IntVec3 cell)
        {
            return !cell.Walkable(map);
        }

        /// <summary>
        /// 获取障碍物详细信息
        /// </summary>
        private ObstacleInfo GetObstacleInfo(IntVec3 cell)
        {
            // 1. 检查地形（水、岩石等）
            var terrain = map.terrainGrid.TerrainAt(cell);
            if (terrain != null && terrain.passability == Traversability.Impassable)
            {
                return new ObstacleInfo
                {
                    type = "terrain",
                    defName = terrain.defName,
                    label = terrain.LabelCap,
                    x = cell.x,
                    z = cell.z,
                    category = GetTerrainCategory(terrain)
                };
            }

            // 2. 检查建筑（墙、门等）
            var building = map.edificeGrid[cell];
            if (building != null && building.def.passability == Traversability.Impassable)
            {
                return new ObstacleInfo
                {
                    type = "building",
                    defName = building.def.defName,
                    label = building.def.LabelCap,
                    x = cell.x,
                    z = cell.z,
                    category = GetBuildingCategory(building)
                };
            }

            // 3. 检查物品（倒下的树、石头等）
            var things = map.thingGrid.ThingsListAt(cell);
            foreach (var thing in things)
            {
                if (thing.def.passability == Traversability.Impassable)
                {
                    return new ObstacleInfo
                    {
                        type = "thing",
                        defName = thing.def.defName,
                        label = thing.def.LabelCap,
                        x = cell.x,
                        z = cell.z,
                        category = thing.def.category.ToString()
                    };
                }
            }

            return null;
        }

        /// <summary>
        /// 获取地形类别
        /// 增强分类：区分水体类型、沼泽、冻土等
        /// </summary>
        private string GetTerrainCategory(TerrainDef terrain)
        {
            var defName = terrain.defName;

            // 水体类型细分
            if (terrain.IsWater)
            {
                if (defName.Contains("Deep") || defName == "WaterDeep") return "water_deep";
                if (defName.Contains("Shallow") || defName == "WaterShallow") return "water_shallow";
                if (defName.Contains("Ocean")) return "water_ocean";
                return "water";
            }

            // 河流
            if (terrain.IsRiver) return "river";

            // 沼泽
            if (defName.Contains("Marsh") || defName.Contains("Swamp")) return "marsh";

            // 冻土/冰
            if (defName.Contains("Ice") || defName.Contains("Frozen")) return "ice";

            // 天然岩石地形
            if (terrain.tags != null && terrain.tags.Contains("NaturalRock")) return "rock";

            // 沙漠
            if (defName.Contains("Sand") || defName.Contains("Desert")) return "sand";

            // 腐蚀地形
            if (defName.Contains("Polluted")) return "polluted";

            return "impassable";
        }

        /// <summary>
        /// 获取建筑类别
        /// 增强分类：区分矿脉、岩石、建筑等
        /// </summary>
        private string GetBuildingCategory(Building building)
        {
            var defName = building.def.defName;

            // 1. 矿脉类型 (Mineable 前缀)
            if (defName.StartsWith("Mineable"))
            {
                return GetOreCategory(defName);
            }

            // 2. 天然岩石 (山体/矿石)
            if (IsNaturalRock(defName))
            {
                return "rock";
            }

            // 3. 围栏
            if (building.def.IsFence) return "fence";

            // 4. 危险建筑
            if (building.def.building?.ai_combatDangerous == true) return "dangerous";

            // 5. 门
            if (building.def.IsDoor) return "door";

            // 6. 墙体
            if (IsWall(building)) return "wall";

            return "structure";
        }

        /// <summary>
        /// 根据矿脉 defName 返回具体矿石类别
        /// </summary>
        private string GetOreCategory(string defName)
        {
            // 矿石类型映射
            if (defName.Contains("Steel")) return "ore_steel";
            if (defName.Contains("Gold")) return "ore_gold";
            if (defName.Contains("Silver")) return "ore_silver";
            if (defName.Contains("Uranium")) return "ore_uranium";
            if (defName.Contains("Plasteel")) return "ore_plasteel";
            if (defName.Contains("Jade")) return "ore_jade";
            if (defName.Contains("Components")) return "ore_components";
            if (defName.Contains("Chemfuel")) return "ore_chemfuel";
            return "ore";  // 通用矿石类别
        }

        /// <summary>
        /// 判断是否是天然岩石（山体）
        /// </summary>
        private bool IsNaturalRock(string defName)
        {
            // RimWorld 中的天然岩石类型
            var naturalRocks = new HashSet<string>
            {
                "Slate", "Sandstone", "Granite", "Limestone", "Marble",
                "CollapsedRocks", "ChunkGranite", "ChunkSlate", "ChunkSandstone",
                "ChunkLimestone", "ChunkMarble"
            };
            return naturalRocks.Contains(defName);
        }

        /// <summary>
        /// 判断是否是墙体
        /// </summary>
        private bool IsWall(Building building)
        {
            var defName = building.def.defName;

            // 常见墙体定义
            if (defName == "Wall") return true;
            if (defName.Contains("Wall_")) return true;
            if (defName.EndsWith("_Wall")) return true;

            // 检查是否是建筑框架且不可通行
            if (building.def.building != null &&
                building.def.passability == Traversability.Impassable &&
                !building.def.IsDoor &&
                !building.def.IsFence)
            {
                // 通过 Stuff 判断是否是建造的墙
                var stuff = building.Stuff;
                if (stuff != null)
                {
                    // 玩家建造的墙会有材料信息
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取某格的最近障碍物信息
        /// </summary>
        public ObstacleInfo GetNearestObstacle(IntVec3 cell)
        {
            if (dirty)
            {
                Recalculate();
            }
            if (!cell.InBounds(map))
            {
                return null;
            }
            return nearestObstacle[cell.x, cell.z];
        }

        /// <summary>
        /// 获取障碍物类型分布统计
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> GetObstacleTypeDistribution()
        {
            if (dirty)
            {
                Recalculate();
            }
            return obstacleTypeCount;
        }

        /// <summary>
        /// 获取障碍物样本列表
        /// </summary>
        public List<ObstacleInfo> GetObstacleSamples(int maxSamples = 20)
        {
            if (dirty)
            {
                Recalculate();
            }
            return obstacleSamples.Take(maxSamples).ToList();
        }

        /// <summary>
        /// 重新计算距离场
        /// 使用 BFS 从所有障碍物向外扩散
        /// </summary>
        public void Recalculate()
        {
            var queue = new Queue<IntVec3>();

            // 重置障碍物统计
            obstacleTypeCount = new Dictionary<string, Dictionary<string, int>>
            {
                ["terrain"] = new Dictionary<string, int>(),
                ["building"] = new Dictionary<string, int>(),
                ["thing"] = new Dictionary<string, int>()
            };
            obstacleSamples = new List<ObstacleInfo>();
            var seenDefNames = new HashSet<string>();

            // 1. 初始化：所有障碍物距离=0，记录障碍物信息
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    var cell = new IntVec3(x, 0, z);
                    var obstacleInfo = GetObstacleInfo(cell);

                    if (obstacleInfo != null)
                    {
                        distances[x, z] = 0;
                        nearestObstacle[x, z] = obstacleInfo;  // 记录自己
                        queue.Enqueue(cell);

                        // 统计障碍物类型
                        if (!obstacleTypeCount[obstacleInfo.type].ContainsKey(obstacleInfo.defName))
                        {
                            obstacleTypeCount[obstacleInfo.type][obstacleInfo.defName] = 0;
                        }
                        obstacleTypeCount[obstacleInfo.type][obstacleInfo.defName]++;

                        // 收集样本（每种类型只收集一个）
                        if (!seenDefNames.Contains(obstacleInfo.defName))
                        {
                            seenDefNames.Add(obstacleInfo.defName);
                            obstacleSamples.Add(obstacleInfo);
                        }
                    }
                    else
                    {
                        distances[x, z] = int.MaxValue;
                        nearestObstacle[x, z] = null;
                    }
                }
            }

            // 2. BFS 扩散：传递最近障碍物信息
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int currentDist = distances[current.x, current.z];
                var currentObstacle = nearestObstacle[current.x, current.z];

                foreach (var offset in NeighborOffsets)
                {
                    var neighbor = current + offset;

                    // 边界检查
                    if (neighbor.x < 0 || neighbor.x >= mapSizeX ||
                        neighbor.z < 0 || neighbor.z >= mapSizeZ)
                    {
                        continue;
                    }

                    int newDist = currentDist + 1;
                    if (newDist < distances[neighbor.x, neighbor.z])
                    {
                        distances[neighbor.x, neighbor.z] = newDist;
                        nearestObstacle[neighbor.x, neighbor.z] = currentObstacle;  // 传递障碍物信息
                        queue.Enqueue(neighbor);
                    }
                }
            }

            dirty = false;
        }

        /// <summary>
        /// 获取距离场统计信息
        /// </summary>
        public ESDFStatistics GetStatistics()
        {
            if (dirty)
            {
                Recalculate();
            }

            var stats = new ESDFStatistics
            {
                totalCells = mapSizeX * mapSizeZ
            };

            int maxDist = 0;
            long totalDist = 0;
            int obstacleCount = 0;

            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    int dist = distances[x, z];

                    if (dist == 0)
                    {
                        obstacleCount++;
                        stats.distanceDistribution["0"]++;
                    }
                    else if (dist == 1)
                    {
                        stats.distanceDistribution["1"]++;
                    }
                    else if (dist == 2)
                    {
                        stats.distanceDistribution["2"]++;
                    }
                    else if (dist >= 3)
                    {
                        stats.distanceDistribution["3+"]++;
                    }

                    if (dist > maxDist)
                    {
                        maxDist = dist;
                    }

                    totalDist += dist;
                }
            }

            stats.maxDistance = maxDist;
            stats.obstacleCount = obstacleCount;
            stats.walkableCount = stats.totalCells - obstacleCount;

            if (stats.walkableCount > 0)
            {
                stats.avgDistance = (float)totalDist / stats.walkableCount;
            }

            stats.safeAreaPercent = stats.totalCells > 0
                ? (float)stats.distanceDistribution["3+"] / stats.totalCells * 100
                : 0;

            return stats;
        }

        /// <summary>
        /// 查找满足最小距离要求的安全区域
        /// </summary>
        public List<SafeArea> FindSafeAreas(int minDistance, int minAreaSize = 10)
        {
            if (dirty)
            {
                Recalculate();
            }

            var safeAreas = new List<SafeArea>();
            var visited = new bool[mapSizeX, mapSizeZ];

            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    if (visited[x, z])
                    {
                        continue;
                    }

                    int dist = distances[x, z];
                    if (dist < minDistance)
                    {
                        visited[x, z] = true;
                        continue;
                    }

                    // BFS 找到连续的安全区域
                    var area = new List<IntVec3>();
                    var queue = new Queue<IntVec3>();
                    queue.Enqueue(new IntVec3(x, 0, z));
                    visited[x, z] = true;

                    int minX = x, maxX = x, minZ = z, maxZ = z;
                    int minDistInArea = dist;

                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        area.Add(current);

                        foreach (var offset in NeighborOffsets)
                        {
                            var neighbor = current + offset;

                            if (neighbor.x < 0 || neighbor.x >= mapSizeX ||
                                neighbor.z < 0 || neighbor.z >= mapSizeZ)
                            {
                                continue;
                            }

                            if (visited[neighbor.x, neighbor.z])
                            {
                                continue;
                            }

                            int neighborDist = distances[neighbor.x, neighbor.z];
                            if (neighborDist >= minDistance)
                            {
                                visited[neighbor.x, neighbor.z] = true;
                                queue.Enqueue(neighbor);

                                if (neighbor.x < minX) minX = neighbor.x;
                                if (neighbor.x > maxX) maxX = neighbor.x;
                                if (neighbor.z < minZ) minZ = neighbor.z;
                                if (neighbor.z > maxZ) maxZ = neighbor.z;
                                if (neighborDist < minDistInArea) minDistInArea = neighborDist;
                            }
                        }
                    }

                    if (area.Count >= minAreaSize)
                    {
                        safeAreas.Add(new SafeArea
                        {
                            minX = minX,
                            minZ = minZ,
                            maxX = maxX,
                            maxZ = maxZ,
                            area = area.Count,
                            minDistance = minDistInArea
                        });
                    }
                }
            }

            // 按面积排序，返回最大的区域
            return safeAreas.OrderByDescending(a => a.area).ToList();
        }

        /// <summary>
        /// 检查某格是否是 ESDF 局部最大值（用于 Voronoi 骨架提取）
        /// </summary>
        public bool IsLocalMaximum(IntVec3 cell)
        {
            if (dirty)
            {
                Recalculate();
            }

            int dist = distances[cell.x, cell.z];
            if (dist == 0)
            {
                return false;
            }

            foreach (var offset in NeighborOffsets)
            {
                var neighbor = cell + offset;
                if (neighbor.InBounds(map))
                {
                    if (distances[neighbor.x, neighbor.z] > dist)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取指定区域内的距离场数据
        /// </summary>
        public int[,] GetDistanceRect(CellRect rect)
        {
            if (dirty)
            {
                Recalculate();
            }

            int width = rect.Width;
            int height = rect.Height;
            var result = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    int mapX = rect.minX + x;
                    int mapZ = rect.minZ + z;

                    if (mapX >= 0 && mapX < mapSizeX && mapZ >= 0 && mapZ < mapSizeZ)
                    {
                        result[x, z] = distances[mapX, mapZ];
                    }
                    else
                    {
                        result[x, z] = 0;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取地图地理分析摘要
        /// 帮助 AI 理解地图的整体地理分布
        /// </summary>
        public GeographicAnalysis GetGeographicAnalysis()
        {
            if (dirty)
            {
                Recalculate();
            }

            var analysis = new GeographicAnalysis
            {
                mapSize = new { x = mapSizeX, z = mapSizeZ }
            };

            // 统计各类障碍物
            var categoryCounts = new Dictionary<string, int>();
            var oreDeposits = new List<Dictionary<string, object>>();
            var rockFormations = new List<Dictionary<string, object>>();
            var waterBodies = new List<Dictionary<string, object>>();
            var structures = new List<Dictionary<string, object>>();

            var seenPositions = new HashSet<string>();

            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    var obstacle = nearestObstacle[x, z];
                    if (obstacle == null) continue;

                    string posKey = $"{obstacle.x},{obstacle.z}";
                    if (seenPositions.Contains(posKey)) continue;
                    seenPositions.Add(posKey);

                    // 统计类别
                    string category = obstacle.category;
                    if (!categoryCounts.ContainsKey(category))
                    {
                        categoryCounts[category] = 0;
                    }
                    categoryCounts[category]++;

                    // 分类收集
                    var obstacleInfo = new Dictionary<string, object>
                    {
                        ["defName"] = obstacle.defName,
                        ["label"] = obstacle.label,
                        ["x"] = obstacle.x,
                        ["z"] = obstacle.z,
                        ["category"] = obstacle.category
                    };

                    if (category.StartsWith("ore"))
                    {
                        oreDeposits.Add(obstacleInfo);
                    }
                    else if (category == "rock")
                    {
                        rockFormations.Add(obstacleInfo);
                    }
                    else if (category.StartsWith("water"))
                    {
                        waterBodies.Add(obstacleInfo);
                    }
                    else if (category == "wall" || category == "structure" || category == "door")
                    {
                        structures.Add(obstacleInfo);
                    }
                }
            }

            analysis.categoryCounts = categoryCounts;
            analysis.oreDeposits = oreDeposits.Take(20).ToList();  // 最多返回20个
            analysis.rockFormations = rockFormations.Take(20).ToList();
            analysis.waterBodies = waterBodies.Take(10).ToList();
            analysis.structures = structures.Take(20).ToList();

            // 生成地理描述
            analysis.geographicSummary = GenerateGeographicSummary(categoryCounts, oreDeposits.Count, rockFormations.Count);

            return analysis;
        }

        /// <summary>
        /// 生成地理描述文本
        /// </summary>
        private string GenerateGeographicSummary(Dictionary<string, int> categoryCounts, int oreCount, int rockCount)
        {
            var descriptions = new List<string>();

            // 矿物资源
            if (oreCount > 0)
            {
                var oreTypes = categoryCounts.Where(kvp => kvp.Key.StartsWith("ore"))
                    .Select(kvp => $"{kvp.Key.Replace("ore_", "")}({kvp.Value})")
                    .ToList();
                if (oreTypes.Count > 0)
                {
                    descriptions.Add($"发现 {oreCount} 处矿脉，包括: {string.Join(", ", oreTypes)}");
                }
            }

            // 岩石
            if (categoryCounts.ContainsKey("rock") && categoryCounts["rock"] > 0)
            {
                descriptions.Add($"存在 {categoryCounts["rock"]} 处天然岩石/山体");
            }

            // 水体（不可通行的）
            var waterCount = categoryCounts.Where(kvp => kvp.Key.StartsWith("water") || kvp.Key == "marsh")
                .Sum(kvp => kvp.Value);
            if (waterCount > 0)
            {
                descriptions.Add($"存在 {waterCount} 处不可通行水体/沼泽");
            }

            // 建筑结构
            var structCount = categoryCounts.Where(kvp => kvp.Key == "wall" || kvp.Key == "structure" || kvp.Key == "door")
                .Sum(kvp => kvp.Value);
            if (structCount > 0)
            {
                descriptions.Add($"存在 {structCount} 处人造建筑");
            }

            if (descriptions.Count == 0)
            {
                descriptions.Add("地图开阔，无明显障碍物");
            }

            return string.Join("。", descriptions);
        }
    }

    /// <summary>
    /// ESDF 统计信息
    /// </summary>
    public class ESDFStatistics
    {
        public int totalCells;
        public int obstacleCount;
        public int walkableCount;
        public int maxDistance;
        public float avgDistance;
        public float safeAreaPercent;

        public Dictionary<string, int> distanceDistribution = new Dictionary<string, int>
        {
            ["0"] = 0,
            ["1"] = 0,
            ["2"] = 0,
            ["3+"] = 0
        };
    }

    /// <summary>
    /// 安全区域
    /// </summary>
    public class SafeArea
    {
        public int minX;
        public int minZ;
        public int maxX;
        public int maxZ;
        public int area;
        public int minDistance;
    }

    /// <summary>
    /// 地理分析结果
    /// </summary>
    public class GeographicAnalysis
    {
        public object mapSize;
        public Dictionary<string, int> categoryCounts;
        public List<Dictionary<string, object>> oreDeposits;
        public List<Dictionary<string, object>> rockFormations;
        public List<Dictionary<string, object>> waterBodies;
        public List<Dictionary<string, object>> structures;
        public string geographicSummary;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["mapSize"] = mapSize,
                ["categoryCounts"] = categoryCounts,
                ["oreDeposits"] = oreDeposits,
                ["rockFormations"] = rockFormations,
                ["waterBodies"] = waterBodies,
                ["structures"] = structures,
                ["geographicSummary"] = geographicSummary
            };
        }
    }
}
