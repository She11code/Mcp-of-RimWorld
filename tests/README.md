# RimWorld AI Mod API 测试

## 安装依赖

```bash
pip install -r requirements.txt
```

## 使用方法

### 基本用法

```bash
# 运行所有查询测试
python test_api.py

# 快速测试（仅基础命令）
python test_api.py --quick

# 包含控制命令测试（会影响游戏状态！）
python test_api.py --control

# 详细输出模式（打印完整JSON响应）
python test_api.py -v

# 详细输出 + 快速测试
python test_api.py --quick -v

# 连接到其他地址
python test_api.py --host 192.168.1.100 --port 8080
```

### 测试模式

| 模式 | 命令 | 说明 |
|------|------|------|
| 完整查询测试 | `python test_api.py` | 测试所有查询命令，不修改游戏状态 |
| 快速测试 | `python test_api.py --quick` | 仅测试6个基础命令 |
| 包含控制命令 | `python test_api.py --control` | 测试所有命令，包括会修改游戏状态的控制命令 |
| 详细输出模式 | `python test_api.py -v` | 打印每个测试的完整JSON响应数据 |

### 命令行参数

| 参数 | 简写 | 说明 |
|------|------|------|
| `--host` | | 服务器地址（默认: localhost） |
| `--port` | | 服务器端口（默认: 8080） |
| `--quick` | | 快速测试模式 |
| `--control` | | 包含控制命令测试 |
| `--verbose` | `-v` | 详细输出模式，打印完整JSON响应 |

### 测试输出示例

**普通模式:**
```
============================================================
连接到 ws://localhost:8080/ai...
连接成功!
服务器消息: RimWorldAI ready
可用命令数量: 45

============================================================
查询命令测试
============================================================

  测试: ping - 连接测试
  [PASS] ping - 连接测试 (0.01s)

  测试: get_map_info - 地图信息
  [PASS] get_map_info - 地图信息 (0.05s)

  ...

============================================================
测试摘要
============================================================

总计: 25 个测试
  通过: 24
  失败: 1
  跳过: 0

失败的测试:
  - get_pawn_info - 角色详情: Pawn with id 12345 not found

总耗时: 1.23s
```

**详细模式 (`-v`):**
```
  测试: get_map_info - 地图信息
  [PASS] get_map_info - 地图信息 (0.05s)
  响应数据:
  {
      "success": true,
      "data": {
          "size": 250,
          "seed": 12345,
          "temperature": 22.5,
          "rainfall": 0.8,
          "biome": "TemperateForest"
      }
  }
```

## 测试命令列表

### 查询命令（不影响游戏状态）

**基础命令：**
- `ping` - 连接测试
- `get_game_state` - 游戏状态

**地图分析：**
- `get_esdf_map` - ESDF 距离场地图
- `get_voronoi_map` - Voronoi 骨架
- `get_cell_info` - 单元格详情
- `get_river` - 河流信息
- `get_marsh` - 沼泽信息

**时间和天气：**
- `get_time_info` - 时间和昼夜信息（tick、小时、季节、太阳光照）
- `get_weather_info` - 天气信息（当前天气、降雨/雪率、风速）

**角色查询：**
- `get_all_pawns` - 所有角色
- `get_colonists` - 殖民者列表
- `get_prisoners` - 囚犯列表
- `get_enemies` - 敌人列表
- `get_animals` - 动物列表
- `get_pawn_info` - 角色详情

**植物分类查询：**
- `get_trees` - 树木统计（按类型细分）
- `get_crops` - 农作物统计
- `get_wild_harvestable` - 野生可收获植物
- `get_plant_by_def` - 按 defName 获取特定植物

**物品分类查询：**
- `get_food` - 食物统计
- `get_weapons` - 武器统计
- `get_apparel` - 衣物统计
- `get_medicine` - 药品统计
- `get_materials` - 材料统计
- `get_item_by_def` - 按 defName 获取特定物品
- `get_thing_info` - 物品详情

**建筑分类查询：**
- `get_production_buildings` - 生产建筑
- `get_power_buildings` - 电力建筑
- `get_defense_buildings` - 防御建筑
- `get_storage_buildings` - 储存建筑
- `get_furniture` - 家具
- `get_building_by_def` - 按 defName 获取特定建筑

**区域和资源：**
- `get_zones` - 区域列表
- `get_zone_info` - 区域详情
- `get_areas` - 活动区域
- `get_room_info` - 房间信息
- `get_resources` - 资源统计
- `get_critical_resources` - 关键资源
- `get_wealth` - 财富概览

**工作系统：**
- `get_work_priorities` - 工作优先级
- `get_work_types` - 工作类型
- `get_available_work` - 可分配工作
- `get_supported_work_types` - 支持 trigger_work 的工作类型

**其他：**
- `get_haulables` - 可搬运物品
- `find_build_locations` - 建筑位置推荐

### 控制命令（会修改游戏状态）

使用 `--control` 参数才会执行：

- `move_pawn` - 移动角色
- `stop_pawn` - 停止角色
- `set_work_priority` - 设置工作优先级
- `unlock_things` - 解锁物品
- `place_blueprint` - 放置蓝图
- `trigger_work` - 触发工作（核心功能）

**区域管理：**
- `create_zone` - 创建区域（储存区/种植区）
- `delete_zone` - 删除区域
- `add_cells_to_zone` - 向区域添加格子
- `remove_cells_from_zone` - 从区域移除格子
- `set_growing_zone_plant` - 设置种植区作物

## 注意事项

1. **确保游戏已启动并加载存档** - 测试需要连接到运行中的 RimWorld 游戏
2. **控制命令测试会影响游戏** - 使用 `--control` 参数时要小心，会修改游戏状态
3. **部分测试依赖前置数据** - 比如角色详情测试需要一个殖民者ID
