using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorldAI.Core.MapAnalysis
{
    /// <summary>
    /// 骨架节点周围的障碍物信息
    /// </summary>
    public class NodeObstacleInfo
    {
        public string type;         // "terrain", "building", "thing"
        public string defName;      // 定义名称
        public string label;        // 显示名称
        public string direction;    // 方向 "north", "south", "east", "west" 等
        public int distance;        // 距离
        public int x, z;            // 障碍物坐标
        public string category;     // 类别

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["type"] = type,
                ["defName"] = defName,
                ["label"] = label,
                ["direction"] = direction,
                ["distance"] = distance,
                ["x"] = x,
                ["z"] = z,
                ["category"] = category
            };
        }
    }

    /// <summary>
    /// 骨架区域的房间信息
    /// </summary>
    public class RegionRoomInfo
    {
        public bool isIndoor;           // 是否室内
        public string roomRole;         // 房间角色
        public string roomLabel;        // 房间显示名称
        public int roomId;              // 房间 ID
        public List<string> buildingTypes;  // 区域内的建筑类型

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["isIndoor"] = isIndoor,
                ["roomRole"] = roomRole,
                ["roomLabel"] = roomLabel,
                ["roomId"] = roomId,
                ["buildingTypes"] = buildingTypes
            };
        }
    }

    /// <summary>
    /// Voronoi 骨架节点
    /// </summary>
    public class SkeletonNode
    {
        public int id;
        public IntVec3 position;
        public int distance;  // ESDF 值（安全程度）
        public List<NodeObstacleInfo> nearbyObstacles;  // 周围障碍物信息

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = id,
                ["x"] = position.x,
                ["z"] = position.z,
                ["distance"] = distance,
                ["nearbyObstacles"] = nearbyObstacles?.Select(o => o.ToDictionary()).ToList()
            };
        }
    }

    /// <summary>
    /// Voronoi 骨架边
    /// </summary>
    public struct SkeletonEdge
    {
        public int fromNode;
        public int toNode;
        public int length;
    }

    /// <summary>
    /// Voronoi 骨架区域
    /// </summary>
    public class SkeletonRegion
    {
        public int id;
        public int centerNodeId;
        public int area;
        public CellRect bounds;
        public RegionRoomInfo roomInfo;  // 房间信息

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = id,
                ["centerNodeId"] = centerNodeId,
                ["area"] = area,
                ["bounds"] = new
                {
                    minX = bounds.minX,
                    minZ = bounds.minZ,
                    maxX = bounds.maxX,
                    maxZ = bounds.maxZ
                },
                ["roomInfo"] = roomInfo?.ToDictionary()
            };
        }
    }

    /// <summary>
    /// Voronoi 骨架提取器
    /// 基于 ESDF 局部最大值提取地图骨架
    /// </summary>
    public class VoronoiSkeleton
    {
        private List<SkeletonNode> nodes = new List<SkeletonNode>();
        private List<SkeletonEdge> edges = new List<SkeletonEdge>();
        private List<SkeletonRegion> regions = new List<SkeletonRegion>();
        private Map map;

        // 8方向邻居偏移
        private static readonly IntVec3[] NeighborOffsets8 = new[]
        {
            new IntVec3(1, 0, 0),
            new IntVec3(-1, 0, 0),
            new IntVec3(0, 0, 1),
            new IntVec3(0, 0, -1),
            new IntVec3(1, 0, 1),
            new IntVec3(-1, 0, -1),
            new IntVec3(1, 0, -1),
            new IntVec3(-1, 0, 1)
        };

        public List<SkeletonNode> Nodes => nodes;
        public List<SkeletonEdge> Edges => edges;
        public List<SkeletonRegion> Regions => regions;

        public VoronoiSkeleton(Map map)
        {
            this.map = map;
        }

        /// <summary>
        /// 计算骨架
        /// </summary>
        public void Calculate(ESDFGrid esdf)
        {
            nodes.Clear();
            edges.Clear();
            regions.Clear();

            // 1. 找到 ESDF 局部最大值作为骨架节点
            FindSkeletonNodes(esdf);

            // 2. 连接相邻节点形成骨架边
            ConnectNodes(esdf);

            // 3. 识别骨架划分的区域
            IdentifyRegions(esdf);
        }

        /// <summary>
        /// 查找骨架节点（ESDF 局部最大值）
        /// </summary>
        private void FindSkeletonNodes(ESDFGrid esdf)
        {
            int mapSizeX = map.Size.x;
            int mapSizeZ = map.Size.z;

            // 采样步长（减少节点数量）
            int step = 3;
            int minDistance = 2;  // 最小距离阈值，过滤掉太靠近障碍物的点

            for (int x = step; x < mapSizeX - step; x += step)
            {
                for (int z = step; z < mapSizeZ - step; z += step)
                {
                    var cell = new IntVec3(x, 0, z);
                    int dist = esdf.GetDistance(cell);

                    // 跳过太靠近障碍物的点
                    if (dist < minDistance)
                    {
                        continue;
                    }

                    // 检查是否是局部最大值
                    if (IsLocalMaximum(esdf, cell, step))
                    {
                        nodes.Add(new SkeletonNode
                        {
                            id = nodes.Count,
                            position = cell,
                            distance = dist,
                            nearbyObstacles = CollectNearbyObstacles(esdf, cell)
                        });
                    }
                }
            }

            // 过滤：保留每个区域内距离最大的节点
            FilterNodesByProximity(15);
        }

        /// <summary>
        /// 收集节点周围的障碍物信息（8方向搜索）
        /// </summary>
        private List<NodeObstacleInfo> CollectNearbyObstacles(ESDFGrid esdf, IntVec3 center)
        {
            var result = new List<NodeObstacleInfo>();
            var seenDefNames = new HashSet<string>();

            // 8方向搜索
            var directions = new[]
            {
                (new IntVec3(1, 0, 0), "east"),
                (new IntVec3(-1, 0, 0), "west"),
                (new IntVec3(0, 0, 1), "north"),
                (new IntVec3(0, 0, -1), "south"),
                (new IntVec3(1, 0, 1), "northeast"),
                (new IntVec3(-1, 0, 1), "northwest"),
                (new IntVec3(1, 0, -1), "southeast"),
                (new IntVec3(-1, 0, -1), "southwest")
            };

            int maxSearchDistance = 30;  // 最大搜索距离

            foreach (var (dir, dirName) in directions)
            {
                for (int dist = 1; dist <= maxSearchDistance; dist++)
                {
                    var checkCell = center + dir * dist;
                    if (!checkCell.InBounds(map))
                    {
                        break;
                    }

                    var obstacle = esdf.GetNearestObstacle(checkCell);
                    if (obstacle != null && obstacle.x == checkCell.x && obstacle.z == checkCell.z)
                    {
                        // 找到障碍物
                        if (!seenDefNames.Contains(obstacle.defName) || result.Count < 8)
                        {
                            seenDefNames.Add(obstacle.defName);
                            result.Add(new NodeObstacleInfo
                            {
                                type = obstacle.type,
                                defName = obstacle.defName,
                                label = obstacle.label,
                                direction = dirName,
                                distance = dist,
                                x = obstacle.x,
                                z = obstacle.z,
                                category = obstacle.category
                            });
                        }
                        break;  // 这个方向找到障碍物后停止
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 检查是否是局部最大值
        /// </summary>
        private bool IsLocalMaximum(ESDFGrid esdf, IntVec3 cell, int radius)
        {
            int dist = esdf.GetDistance(cell);

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    if (dx == 0 && dz == 0)
                    {
                        continue;
                    }

                    var neighbor = new IntVec3(cell.x + dx, 0, cell.z + dz);
                    if (neighbor.InBounds(map))
                    {
                        if (esdf.GetDistance(neighbor) > dist)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 过滤距离过近的节点，保留距离更大的
        /// </summary>
        private void FilterNodesByProximity(int minSeparation)
        {
            var filtered = new List<SkeletonNode>();
            var used = new bool[nodes.Count];

            // 按距离降序排序
            var sortedNodes = nodes.OrderByDescending(n => n.distance).ToList();

            for (int i = 0; i < sortedNodes.Count; i++)
            {
                if (used[i])
                {
                    continue;
                }

                var node = sortedNodes[i];
                node.id = filtered.Count;  // 更新 ID
                filtered.Add(node);  // 保留原有节点（包含 nearbyObstacles）

                // 标记附近的节点为已使用
                for (int j = i + 1; j < sortedNodes.Count; j++)
                {
                    if (used[j])
                    {
                        continue;
                    }

                    var other = sortedNodes[j];
                    float dist = Mathf.Sqrt(
                        Mathf.Pow(node.position.x - other.position.x, 2) +
                        Mathf.Pow(node.position.z - other.position.z, 2)
                    );

                    if (dist < minSeparation)
                    {
                        used[j] = true;
                    }
                }
            }

            nodes = filtered;
        }

        /// <summary>
        /// 连接相邻节点形成骨架边
        /// </summary>
        private void ConnectNodes(ESDFGrid esdf)
        {
            float maxEdgeLength = 50f;  // 最大边长度

            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    var nodeA = nodes[i];
                    var nodeB = nodes[j];

                    float dist = Mathf.Sqrt(
                        Mathf.Pow(nodeA.position.x - nodeB.position.x, 2) +
                        Mathf.Pow(nodeA.position.z - nodeB.position.z, 2)
                    );

                    if (dist <= maxEdgeLength)
                    {
                        // 检查路径是否可行（沿途 ESDF 值不能太低）
                        if (IsPathValid(esdf, nodeA.position, nodeB.position))
                        {
                            edges.Add(new SkeletonEdge
                            {
                                fromNode = i,
                                toNode = j,
                                length = Mathf.RoundToInt(dist)
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查两点之间的路径是否有效
        /// </summary>
        private bool IsPathValid(ESDFGrid esdf, IntVec3 from, IntVec3 to, int minDistance = 1)
        {
            int dx = to.x - from.x;
            int dz = to.z - from.z;
            int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz));

            if (steps == 0)
            {
                return true;
            }

            for (int i = 0; i <= steps; i++)
            {
                int x = from.x + dx * i / steps;
                int z = from.z + dz * i / steps;

                if (esdf.GetDistanceSafe(new IntVec3(x, 0, z)) < minDistance)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 识别骨架划分的区域
        /// </summary>
        private void IdentifyRegions(ESDFGrid esdf)
        {
            if (nodes.Count == 0)
            {
                return;
            }

            // 简单实现：每个节点周围的区域作为一个区域
            // 使用 BFS 扩展，根据到节点的距离分配区域

            int mapSizeX = map.Size.x;
            int mapSizeZ = map.Size.z;
            var regionMap = new int[mapSizeX, mapSizeZ];
            var distanceMap = new float[mapSizeX, mapSizeZ];

            // 初始化
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    regionMap[x, z] = -1;
                    distanceMap[x, z] = float.MaxValue;
                }
            }

            // 计算每个格子到最近节点的距离
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                int radius = node.distance * 2 + 10;  // 扩展半径

                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        int x = node.position.x + dx;
                        int z = node.position.z + dz;

                        if (x < 0 || x >= mapSizeX || z < 0 || z >= mapSizeZ)
                        {
                            continue;
                        }

                        float dist = Mathf.Sqrt(dx * dx + dz * dz);
                        if (dist < distanceMap[x, z])
                        {
                            distanceMap[x, z] = dist;
                            regionMap[x, z] = i;
                        }
                    }
                }
            }

            // 统计每个区域的格子数和边界
            var regionAreas = new Dictionary<int, int>();
            var regionBounds = new Dictionary<int, CellRect>();

            for (int x = 0; x < mapSizeX; x++)
            {
                for (int z = 0; z < mapSizeZ; z++)
                {
                    int regionId = regionMap[x, z];
                    if (regionId < 0)
                    {
                        continue;
                    }

                    if (!regionAreas.ContainsKey(regionId))
                    {
                        regionAreas[regionId] = 0;
                        regionBounds[regionId] = CellRect.SingleCell(new IntVec3(x, 0, z));
                    }

                    regionAreas[regionId]++;

                    var bounds = regionBounds[regionId];
                    if (x < bounds.minX || x > bounds.maxX || z < bounds.minZ || z > bounds.maxZ)
                    {
                        regionBounds[regionId] = CellRect.FromLimits(
                            Mathf.Min(bounds.minX, x),
                            Mathf.Min(bounds.minZ, z),
                            Mathf.Max(bounds.maxX, x),
                            Mathf.Max(bounds.maxZ, z)
                        );
                    }
                }
            }

            // 创建区域列表（过滤太小的区域）
            int minRegionArea = 20;
            foreach (var kvp in regionAreas)
            {
                if (kvp.Value >= minRegionArea)
                {
                    var region = new SkeletonRegion
                    {
                        id = regions.Count,
                        centerNodeId = kvp.Key,
                        area = kvp.Value,
                        bounds = regionBounds[kvp.Key],
                        roomInfo = CollectRegionRoomInfo(kvp.Key, regionBounds[kvp.Key])
                    };
                    regions.Add(region);
                }
            }
        }

        /// <summary>
        /// 收集区域的房间信息
        /// </summary>
        private RegionRoomInfo CollectRegionRoomInfo(int centerNodeId, CellRect bounds)
        {
            var result = new RegionRoomInfo
            {
                buildingTypes = new List<string>()
            };

            // 从区域中心节点获取房间信息
            if (centerNodeId >= 0 && centerNodeId < nodes.Count)
            {
                var centerNode = nodes[centerNodeId];
                var room = centerNode.position.GetRoom(map);

                if (room != null)
                {
                    result.isIndoor = !room.PsychologicallyOutdoors && !room.IsDoorway;
                    result.roomRole = room.Role?.defName ?? "None";
                    result.roomLabel = room.Role?.LabelCap ?? "无";
                    result.roomId = room.ID;

                    // 收集区域内的建筑类型
                    var buildingTypesSet = new HashSet<string>();
                    int sampleStep = 3;  // 采样步长

                    for (int x = bounds.minX; x <= bounds.maxX; x += sampleStep)
                    {
                        for (int z = bounds.minZ; z <= bounds.maxZ; z += sampleStep)
                        {
                            var cell = new IntVec3(x, 0, z);
                            if (!cell.InBounds(map)) continue;

                            var building = map.edificeGrid[cell];
                            if (building != null)
                            {
                                buildingTypesSet.Add(building.def.defName);
                                if (buildingTypesSet.Count >= 10)
                                {
                                    break;  // 最多收集10种建筑类型
                                }
                            }
                        }
                        if (buildingTypesSet.Count >= 10) break;
                    }

                    result.buildingTypes = buildingTypesSet.ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// 获取骨架统计信息
        /// </summary>
        public VoronoiStatistics GetStatistics()
        {
            var stats = new VoronoiStatistics
            {
                totalNodes = nodes.Count,
                totalEdges = edges.Count,
                totalRegions = regions.Count
            };

            if (nodes.Count > 0)
            {
                stats.avgNodeDistance = (float)nodes.Average(n => n.distance);
                stats.maxNodeDistance = nodes.Max(n => n.distance);
            }

            if (edges.Count > 0)
            {
                stats.avgEdgeLength = (float)edges.Average(e => e.length);
            }

            if (regions.Count > 0)
            {
                stats.avgRegionArea = (float)regions.Average(r => r.area);
            }

            return stats;
        }

        /// <summary>
        /// 获取两点之间沿骨架的最优路径
        /// </summary>
        public List<IntVec3> GetBackbonePath(IntVec3 from, IntVec3 to)
        {
            // 简单实现：返回直线路径
            // TODO: 实现沿骨架节点的路径
            var path = new List<IntVec3>();

            int dx = to.x - from.x;
            int dz = to.z - from.z;
            int steps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz));

            if (steps == 0)
            {
                path.Add(from);
                return path;
            }

            for (int i = 0; i <= steps; i++)
            {
                int x = from.x + dx * i / steps;
                int z = from.z + dz * i / steps;
                path.Add(new IntVec3(x, 0, z));
            }

            return path;
        }
    }

    /// <summary>
    /// Voronoi 统计信息
    /// </summary>
    public class VoronoiStatistics
    {
        public int totalNodes;
        public int totalEdges;
        public int totalRegions;
        public float avgNodeDistance;
        public int maxNodeDistance;
        public float avgEdgeLength;
        public float avgRegionArea;
    }
}
