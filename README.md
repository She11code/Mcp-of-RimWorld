# RimWorld MCP

MCP (Model Context Protocol) 接口，允许外部 AI 控制 RimWorld 游戏。

## 功能

### 游戏状态查询
- `get_game_state` - 获取完整游戏状态
- `get_time_info` - 时间和季节信息
- `get_weather_info` - 天气状况

### 角色管理
- `get_colonists` - 殖民者列表及状态
- `get_enemies` - 敌人检测
- `get_animals` - 动物列表
- `get_pawn_info` - 角色详细信息

### 建造系统
- `get_buildings` - 建筑列表
- `get_room_boundary` - 房间边界检测
- `place_blueprint` - 放置建造蓝图
- `cancel_blueprint` - 取消蓝图

### 区域管理
- `get_zones` - 区域列表
- `create_zone` - 创建区域（种植区/储存区等）
- `delete_zone` - 删除区域

### 工作系统
- `get_work_types` - 工作类型列表
- `get_work_priority` - 查询工作优先级
- `set_work_priority` - 设置工作优先级
- `trigger_work` - 触发工作执行

### 物品管理
- `get_food` - 食物库存
- `get_materials` - 材料库存
- `get_weapons` - 武器库存

### 地图分析
- `scan_area` - 扫描区域
- `get_esdf_map` - 距离场分析
- `get_voronoi_map` - 拓扑分析
- `find_build_locations` - 寻找建造位置

## 安装

1. 编译项目
```bash
cd Source
dotnet build -c Release
```

2. 将 `Assemblies/RimWorldAI.dll` 和 `About/` 文件夹复制到 RimWorld Mods 目录：
```
Windows: C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\RimWorldAI\
Linux: ~/.steam/steam/steamapps/common/RimWorld/Mods/RimWorldAI/
Mac: ~/Library/Application Support/Steam/steamapps/common/RimWorld/Mods/RimWorldAI/
```

3. 启动游戏，在模组管理器中启用 "RimWorld AI"

## 使用

MOD 加载后，MCP 服务器会在 `http://localhost:8080/mcp` 启动。

使用 MCP 客户端连接即可调用上述工具。

### 示例请求

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "get_colonists",
    "arguments": {}
  }
}
```

### 示例响应

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [{
      "type": "text",
      "text": "[{\"pawnId\": 1, \"name\": \"John\", \"skills\": {...}}]"
    }]
  }
}
```

## 配合 Claude Desktop 使用

在 Claude Desktop 配置文件中添加：

```json
{
  "mcpServers": {
    "rimworld": {
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

## 支持版本

- RimWorld 1.6

## 作者

She11code

## 许可证

MIT
