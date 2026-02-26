# RimWorld AI Mod API 参考文档

本文档为 AI Agent 设计，按功能目标组织，帮助你快速找到实现目标所需的 API。

---

## 1. 快速入门

### 连接信息
- **协议**: WebSocket
- **默认端口**: 8080
- **端点**: `ws://localhost:8080/ai`

### 通用格式

**请求**:
```json
{"action": "命令名称", "参数": "值"}
```

**成功响应**:
```json
{"success": true, "data": {...}}
```

**错误响应**:
```json
{"success": false, "error": "错误信息"}
```

### 最简单的使用

```python
import websocket, json

ws = websocket.create_connection("ws://localhost:8080/ai")

# 让殖民者去砍树 - 系统自动分配空闲人员
ws.send(json.dumps({"action": "trigger_work", "workType": "PlantCutting"}))

# 让殖民者去收获作物
ws.send(json.dumps({"action": "trigger_work", "workType": "Growing"}))

# 让殖民者去搬运物品
ws.send(json.dumps({"action": "trigger_work", "workType": "Hauling"}))

ws.close()
```

---

## 2. 核心概念

### RimWorld 游戏实体层次

```
Map (地图)
├── Pawns (角色)
│   ├── Colonists (殖民者) - 你控制的人
│   ├── Prisoners (囚犯) - 被俘虏的人
│   ├── Enemies (敌人) - 敌对势力
│   └── Animals (动物) - 野生动物/驯养动物
│
├── Things (物品)
│   ├── Items (散落物品) - 食物、材料、武器等
│   ├── Plants (植物) - 树木、作物
│   └── Buildings (建筑) - 已建造的建筑
│
├── Zones (区域)
│   ├── Stockpile (储存区) - 存放物品
│   └── Growing (种植区) - 种植作物
│
└── Blueprints (蓝图) - 待建造的建筑计划
```

### 关键 ID 说明

| ID 类型 | 获取方式 | 用途 |
|---------|----------|------|
| pawnId | `get_colonists` | 移动、攻击、装备 |
| thingId | `get_food`, `get_materials` 等 | 解锁、装备 |
| zoneId | `get_zones`, `create_zone` | 储存区管理 |
| blueprintId | `get_blueprints` | 取消蓝图 |

---

## 3. 功能模块

### 3.1 殖民者管理

> 目标：了解殖民者状态，分配工作

#### 查看殖民者

| 命令 | 说明 | 返回 |
|------|------|------|
| `get_pawn_info` | **推荐** 获取殖民者完整信息 | 基础信息、技能、特质、心情、健康、装备 |
| `get_colonists` | 所有殖民者简要列表 | id, name, position, healthPercent, curJob |
| `get_work_priorities` | 殖民者的工作优先级 | 各工作类型的优先级 |

**示例 - 获取所有殖民者完整信息（推荐）**:
```json
{"action": "get_pawn_info"}
```

**响应**:
```json
{
  "success": true,
  "data": {
    "count": 2,
    "colonists": [
      {
        "id": 101,
        "name": "John",
        "position": {"x": 100, "z": 120},
        "gender": "Male",
        "biologicalAge": 25,
        "healthPercent": 0.85,
        "isDowned": false,
        "curJob": "Growing",
        "skills": [
          {"defName": "Growing", "label": "种植", "level": 12, "passion": "Major"},
          {"defName": "Construction", "label": "建造", "level": 8, "passion": "Minor"}
        ],
        "traits": [
          {"defName": "Industrious", "label": "勤劳", "degree": 1}
        ],
        "needs": {
          "mood": {"curLevel": 0.75, "curLevelPercentage": 0.75},
          "food": {"curLevel": 0.6, "isStarving": false}
        },
        "hediffs": [],
        "equipment": {"apparelCount": 4}
      },
      {
        "id": 102,
        "name": "Mary",
        "position": {"x": 105, "z": 118},
        "gender": "Female",
        "biologicalAge": 22,
        "healthPercent": 1.0,
        "isDowned": false,
        "curJob": "Research",
        "skills": [...],
        "traits": [...],
        "needs": {...},
        "hediffs": [],
        "equipment": {...}
      }
    ]
  }
}
```

**示例 - 获取单个角色详情（可选）**:
```json
{"action": "get_pawn_info", "pawnId": 101}
```

**示例 - 获取简要列表（轻量）**:
```json
{"action": "get_colonists"}
```

**响应**:
```json
{
  "success": true,
  "data": {
    "count": 3,
    "colonists": [
      {"id": 101, "name": "John", "position": {"x": 100, "z": 120}, "healthPercent": 1.0, "isDowned": false, "curJob": "None"},
      {"id": 102, "name": "Mary", "position": {"x": 105, "z": 118}, "healthPercent": 0.8, "isDowned": false, "curJob": "Growing"}
    ]
  }
}
```
}
```

#### 分配工作

| 命令 | 说明 | 参数 |
|------|------|------|
| `trigger_work` | **推荐** 触发工作类型，系统自动分配 | workType |
| `set_work_priority` | 设置单个殖民者的工作优先级 | pawnId, workType, priority |

**trigger_work 工作类型**:

| workType | 说明 | 何时使用 |
|----------|------|----------|
| `Firefighter` | 消防 | 发生火灾时 |
| `Doctor` | 医疗 | 有人受伤/生病时 |
| `Patient` | 就医 | 殖民者需要治疗时 |
| `Hauling` | 搬运 | 物品散落需要整理时 |
| `Cleaning` | 清洁 | 基地有污垢时 |
| `PlantCutting` | 砍树 | 需要木材时 |
| `Growing` | 种植/收获 | 农作物需要管理时 |
| `Mining` | 挖矿 | 需要矿石时 |
| `Construction` | 建造 | 有蓝图需要建造时 |
| `Repair` | 维修 | 建筑损坏时 |
| `Hunting` | 狩猎 | 需要肉类/皮革时 |
| `Cooking` | 烹饪 | 需要食物时 |
| `Research` | 研究 | 需要科技进度时 |

**示例 - 让殖民者去砍树**:
```json
{"action": "trigger_work", "workType": "PlantCutting"}
```

**响应**:
```json
{
  "success": true,
  "data": {
    "workType": "PlantCutting",
    "assignedCount": 2,
    "assignedColonists": [
      {"pawnId": 101, "name": "John", "jobAssigned": true},
      {"pawnId": 102, "name": "Mary", "jobAssigned": true}
    ]
  }
}
```

#### 控制殖民者行动

| 命令 | 说明 | 参数 |
|------|------|------|
| `move_pawn` | 移动到指定位置 | pawnId, x, z |
| `stop_pawn` | 停止当前任务 | pawnId |
| `attack_target` | 攻击目标 | pawnId, targetId |
| `equip_tool` | 装备武器/工具 | pawnId, thingId |

---

### 3.2 资源管理

> 目标：了解殖民地资源状况，决定生产/收集优先级

#### 资源查询

| 命令 | 说明 | 适用场景 |
|------|------|----------|
| `get_resources` | 资源总览 | 快速了解库存 |
| `get_critical_resources` | 关键资源 | 检查食物、药品等 |
| `get_wealth` | 财富概览 | 评估殖民地发展 |
| `get_food` | 食物统计 | 检查食物储备 |
| `get_materials` | 材料统计 | 检查建材储备 |
| `get_medicine` | 药品统计 | 检查医疗物资 |
| `get_weapons` | 武器统计 | 检查装备 |
| `get_apparel` | 衣物统计 | 检查服装 |

**示例 - 检查资源**:
```json
{"action": "get_resources", "summary": true}
```

**示例 - 按类型查物品**:
```json
{"action": "get_item_by_def", "defName": "Steel"}
```

**常用物品 defName**:

| 类别 | defName | 中文名 |
|------|---------|--------|
| 材料 | Steel | 钢铁 |
| 材料 | WoodLog | 木材 |
| 材料 | Plasteel | 塑钢 |
| 材料 | ComponentIndustrial | 工业组件 |
| 食物 | RawPotatoes | 生土豆 |
| 食物 | MealSimple | 简单餐 |
| 药品 | MedicineIndustrial | 工业药 |

#### 解锁物品

被禁止的物品（红色标记）无法被殖民者使用：

```json
{"action": "unlock_things", "thingId": 12345}
```

解锁所有被禁止的物品：
```json
{"action": "unlock_things", "all": true}
```

---

### 3.3 建造系统

> 目标：建造新建筑，扩展基地

#### 建造流程

```
1. get_buildable_defs → 查看可建造的建筑
2. place_blueprint → 放置蓝图
3. trigger_work(Construction) → 触发建造
4. get_blueprints → 查看建造进度（可选）
```

#### 建造相关命令

| 命令 | 说明 | 参数 |
|------|------|------|
| `get_buildable_defs` | 可建造的建筑定义 | category(可选) |
| `place_blueprint` | 放置蓝图 | defName, x, z, stuffDefName(可选), rotation(可选) |
| `get_blueprints` | 查看所有蓝图 | - |
| `cancel_blueprint` | 取消蓝图 | blueprintId |

**示例 - 放置太阳能发电机**:
```json
{
  "action": "place_blueprint",
  "defName": "SolarGenerator",
  "x": 100,
  "z": 100
}
```

**示例 - 放置木墙**:
```json
{
  "action": "place_blueprint",
  "defName": "Wall",
  "x": 100,
  "z": 100,
  "stuffDefName": "WoodLog",
  "rotation": "north"
}
```

**常用建筑 defName**:

| 类别 | defName | 中文名 |
|------|---------|--------|
| 生产 | TableButcher | 屠宰台 |
| 生产 | ElectricStove | 电动炉灶 |
| 生产 | TableStonecutter | 切石台 |
| 电力 | SolarGenerator | 太阳能发电机 |
| 电力 | Battery | 蓄电池 |
| 电力 | PowerConduit | 电缆 |
| 防御 | Sandbags | 沙袋 |
| 防御 | Turret_MiniTurret | 迷你炮塔 |
| 家具 | Bed | 床 |
| 家具 | Table2x2c | 桌子 |

**get_buildable_defs 类别筛选**:

| category | 说明 |
|----------|------|
| furniture | 家具 |
| production | 生产建筑 |
| power | 电力建筑 |
| defense | 防御建筑 |
| temperature | 温控建筑 |

---

### 3.4 农业系统

> 目标：管理种植区，确保食物供应

#### 农业相关命令

| 命令 | 说明 | 参数 |
|------|------|------|
| `get_crops` | 查看所有作物 | - |
| `get_trees` | 查看所有树木 | - |
| `get_wild_harvestable` | 野生可收获植物 | - |
| `create_zone` | 创建种植区 | type: "growing", cells |
| `set_growing_zone_plant` | 设置种植作物 | zoneId, plantDefName |
| `trigger_work` | 触发种植/收获 | workType: "Growing" |

**示例 - 创建种植区**:
```json
{
  "action": "create_zone",
  "type": "growing",
  "cells": [{"x": 100, "z": 100}, {"x": 101, "z": 100}, {"x": 102, "z": 100}]
}
```

**示例 - 设置种植水稻**:
```json
{
  "action": "set_growing_zone_plant",
  "zoneId": 2,
  "plantDefName": "Plant_Rice"
}
```

**常用作物 defName**:

| defName | 中文名 | 特点 |
|---------|--------|------|
| Plant_Rice | 水稻 | 生长快，产量中 |
| Plant_Potato | 土豆 | 适应性强 |
| Plant_Corn | 玉米 | 产量高，生长慢 |
| Plant_Strawberry | 草莓 | 可生食 |
| Plant_Cotton | 棉花 | 产布料 |
| Plant_Healroot | 草药 | 产药品 |
| Plant_Haygrass | 干草 | 动物饲料 |

---

### 3.5 储存系统

> 目标：管理储存区，保持物品有序

#### 储存管理命令

| 命令 | 说明 | 参数 |
|------|------|------|
| `get_zones` | 查看所有区域 | detailed(可选) |
| `get_zone_info` | 区域详情 | zoneId |
| `create_zone` | 创建储存区 | type: "stockpile", cells |
| `delete_zone` | 删除区域 | zoneId |
| `add_cells_to_zone` | 添加格子 | zoneId, cells |
| `remove_cells_from_zone` | 移除格子 | zoneId, cells |
| `get_storage_settings` | 储存区设置 | zoneId |
| `set_storage_filter` | 设置物品过滤 | zoneId, mode/categories/defs |
| `set_storage_priority` | 设置优先级 | zoneId, priority |
| `apply_storage_preset` | 应用预设 | zoneId, presetName |
| `get_thing_categories` | 物品类别树 | parentCategory(可选) |
| `get_storage_presets` | 储存预设列表 | - |

#### 储存预设（快速配置）

| presetName | 说明 | 用途 |
|------------|------|------|
| all | 允许所有物品 | 通用储存区 |
| food | 只允许食物 | 食物专用 |
| materials | 只允许原材料 | 材料专用 |
| weapons | 只允许武器 | 武器库 |
| apparel | 只允许衣物 | 衣柜 |
| medicine | 只允许药品 | 医疗站 |
| corpses | 只允许尸体 | 垃圾/停尸间 |
| chunks | 只允许碎石块 | 碎石堆 |

#### 储存优先级

| 优先级 | 说明 | 使用场景 |
|--------|------|----------|
| Critical | 关键 | 必须优先填充的储存区 |
| Important | 重要 | 重要物资储存区 |
| Preferred | 优先 | 一般优先储存区 |
| Normal | 普通 | 默认优先级 |
| Low | 低 | 临时/边缘储存区 |
| Unstored | 不储存 | 禁止存放 |

**示例 - 创建食物储存区**:
```json
// 1. 创建储存区
{"action": "create_zone", "type": "stockpile", "cells": [{"x": 100, "z": 100}]}
// 响应: {"success": true, "data": {"zoneId": 15, ...}}

// 2. 应用食物预设
{"action": "apply_storage_preset", "zoneId": 15, "presetName": "food"}

// 3. 设置高优先级
{"action": "set_storage_priority", "zoneId": 15, "priority": "Important"}
```

**示例 - 精细控制物品过滤**:
```json
// 允许所有物品
{"action": "set_storage_filter", "zoneId": 15, "mode": "allowAll"}

// 禁止所有物品
{"action": "set_storage_filter", "zoneId": 15, "mode": "disallowAll"}

// 允许特定类别
{"action": "set_storage_filter", "zoneId": 15, "categories": ["Foods", "Medicine"], "allow": true}

// 允许特定物品
{"action": "set_storage_filter", "zoneId": 15, "defs": ["Steel", "WoodLog"], "allow": true}
```

---

### 3.6 区域管理

> 目标：创建和管理各种区域

#### 区域命令总览

| 命令 | 说明 | 参数 |
|------|------|------|
| `get_zones` | 所有区域列表 | detailed |
| `get_zone_info` | 区域详情 | zoneId |
| `create_zone` | 创建区域 | type, cells |
| `delete_zone` | 删除区域 | zoneId |
| `add_cells_to_zone` | 添加格子 | zoneId, cells |
| `remove_cells_from_zone` | 移除格子 | zoneId, cells |
| `get_areas` | 活动区域 | - |
| `get_room_info` | 房间信息 | x, z |

#### 区域类型

| type | 类名 | 用途 |
|------|------|------|
| stockpile | Zone_Stockpile | 储存物品 |
| growing | Zone_Growing | 种植作物 |

---

### 3.7 环境感知

> 目标：了解游戏世界状态

#### 时间和天气

| 命令 | 说明 | 返回信息 |
|------|------|----------|
| `get_time_info` | 时间信息 | tick, hour, season, isDaytime, sunGlow |
| `get_weather_info` | 天气信息 | current, rainRate, snowRate, windSpeedFactor |

**时间信息示例**:
```json
{
  "action": "get_time_info"
}
// 响应:
{
  "tick": 1234567,
  "hour": 14,
  "isDaytime": true,
  "sunGlow": 0.85,
  "season": "Summer",
  "daysPassed": 20
}
```

#### 地图感知

| 命令 | 说明 | 用途 |
|------|------|------|
| `get_game_state` | 游戏状态总览 | 快速了解全局 |
| `get_esdf_map` | ESDF 距离场地图 | 障碍物分析 |
| `get_voronoi_map` | Voronoi 骨架 | 拓扑分析 |
| `get_cell_info` | 单元格详情 | 查看特定格子 |

---

### 3.8 其他实体查询

#### 植物

| 命令 | 说明 |
|------|------|
| `get_trees` | 树木统计（按类型细分） |
| `get_crops` | 农作物统计 |
| `get_wild_harvestable` | 野生可收获植物 |
| `get_plant_by_def` | 按定义名查植物 |

#### 建筑

| 命令 | 说明 |
|------|------|
| `get_production_buildings` | 生产建筑 |
| `get_power_buildings` | 电力建筑 |
| `get_defense_buildings` | 防御建筑 |
| `get_storage_buildings` | 储存建筑 |
| `get_furniture` | 家具 |
| `get_building_by_def` | 按定义名查建筑 |

#### 角色

| 命令 | 说明 |
|------|------|
| `get_all_pawns` | 所有角色 |
| `get_colonists` | 殖民者 |
| `get_prisoners` | 囚犯 |
| `get_enemies` | 敌人 |
| `get_animals` | 动物 |

---

## 4. 决策指南

### 场景：殖民地刚开局

```
1. get_game_state → 了解游戏概况
2. get_colonists → 了解殖民者能力和状态
3. get_resources → 了解初始资源
4. create_zone(type: stockpile) → 创建储存区
5. apply_storage_preset(preset: all) → 允许所有物品
6. trigger_work(Hauling) → 搬运初始物品
```

### 场景：食物不足

```
1. get_food → 检查食物储备
2. get_crops → 检查作物状态
3. 如果有成熟作物: trigger_work(Growing)
4. 如果没有种植区: create_zone(type: growing)
5. set_growing_zone_plant → 设置种植水稻（快速收获）
6. trigger_work(Growing) → 开始种植
```

### 场景：需要建造

```
1. get_buildable_defs(category) → 查看可建造建筑
2. get_materials → 检查建材储备
3. place_blueprint → 放置蓝图
4. trigger_work(Construction) → 触发建造
5. 如果缺材料: trigger_work(Mining/PlantCutting)
```

### 场景：基地杂乱

```
1. get_haulables → 查看待搬运物品
2. create_zone(type: stockpile) → 确保有储存区
3. trigger_work(Hauling) → 搬运物品
4. trigger_work(Cleaning) → 清理污垢
```

### 场景：发生袭击

```
1. get_enemies → 查看敌人数量和位置
2. get_defense_buildings → 检查防御设施
3. move_pawn → 移动殖民者到防御位置
4. equip_tool → 装备武器
5. attack_target → 攻击敌人
```

---

## 5. 命令速查表

### 按功能分类

| 功能 | 命令 |
|------|------|
| **连接测试** | ping |
| **地图/环境** | get_game_state, get_time_info, get_weather_info, get_esdf_map, get_voronoi_map, get_cell_info, get_river, get_marsh |
| **殖民者** | get_colonists, get_pawn_info, get_all_pawns, get_prisoners, get_enemies, get_animals |
| **工作** | trigger_work, get_work_priorities, set_work_priority, get_work_types, get_supported_work_types |
| **控制行动** | move_pawn, stop_pawn, attack_target, equip_tool |
| **资源** | get_resources, get_critical_resources, get_wealth |
| **物品查询** | get_food, get_materials, get_medicine, get_weapons, get_apparel, get_item_by_def, get_thing_info |
| **物品操作** | unlock_things, get_haulables |
| **植物** | get_trees, get_crops, get_wild_harvestable, get_plant_by_def |
| **建筑查询** | get_production_buildings, get_power_buildings, get_defense_buildings, get_storage_buildings, get_furniture, get_building_by_def |
| **建造** | get_buildable_defs, place_blueprint, get_blueprints, cancel_blueprint |
| **区域/储存** | get_zones, get_zone_info, create_zone, delete_zone, add_cells_to_zone, remove_cells_from_zone |
| **种植区** | set_growing_zone_plant |
| **储存设置** | get_storage_settings, set_storage_filter, set_storage_priority, get_thing_categories, get_storage_presets, apply_storage_preset |
| **其他** | get_areas, get_room_info |

### 按操作类型分类

**纯查询（不修改游戏状态）**:
- ping, get_game_state, get_time_info, get_weather_info
- get_colonists, get_pawn_info, get_all_pawns, get_prisoners, get_enemies, get_animals
- get_work_priorities, get_work_types, get_supported_work_types, get_available_work
- get_resources, get_critical_resources, get_wealth
- get_food, get_materials, get_medicine, get_weapons, get_apparel, get_item_by_def, get_thing_info
- get_trees, get_crops, get_wild_harvestable, get_plant_by_def
- get_production_buildings, get_power_buildings, get_defense_buildings, get_storage_buildings, get_furniture, get_building_by_def
- get_buildable_defs, get_blueprints
- get_zones, get_zone_info, get_areas, get_room_info, get_haulables
- get_esdf_map, get_voronoi_map, get_cell_info, get_river, get_marsh
- get_storage_settings, get_thing_categories, get_storage_presets

**控制命令（修改游戏状态）**:
- trigger_work, set_work_priority
- move_pawn, stop_pawn, attack_target, equip_tool
- unlock_things
- place_blueprint, cancel_blueprint
- create_zone, delete_zone, add_cells_to_zone, remove_cells_from_zone
- set_growing_zone_plant
- set_storage_filter, set_storage_priority, apply_storage_preset

---

## 6. 错误处理

### 通用错误格式
```json
{"success": false, "error": "错误描述"}
```

### 常见错误

| 错误信息 | 原因 | 解决方案 |
|----------|------|----------|
| `Missing required field: xxx` | 缺少必需参数 | 检查请求参数 |
| `Pawn with id xxx not found` | 角色ID不存在 | 重新查询有效ID |
| `Thing with id xxx not found` | 物品ID不存在 | 重新查询有效ID |
| `Zone with id xxx not found` | 区域ID不存在 | 重新查询有效ID |
| `No current map available` | 没有加载地图 | 确保游戏已加载存档 |
| `没有空闲殖民者可分配` | 所有殖民者都在忙 | 等待或停止其他任务 |


