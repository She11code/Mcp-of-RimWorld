#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
RimWorld AI Mod API 测试脚本
用于批量测试 WebSocket API 接口功能

使用方法:
    python test_api.py                    # 运行所有测试
    python test_api.py --quick            # 快速测试（仅基础命令）
    python test_api.py --control          # 测试控制命令
    python test_api.py --host 192.168.1.100 --port 8080
"""

import websocket
import json
import time
import argparse
from typing import Optional, Dict, Any, List, Callable
from dataclasses import dataclass
from enum import Enum


class TestStatus(Enum):
    PENDING = "pending"
    RUNNING = "running"
    PASSED = "passed"
    FAILED = "failed"
    SKIPPED = "skipped"


@dataclass
class TestResult:
    name: str
    status: TestStatus
    message: str = ""
    response: Optional[Dict] = None
    duration: float = 0.0


class RimWorldAITester:
    def __init__(self, host: str = "localhost", port: int = 8080, verbose: bool = False):
        self.host = host
        self.port = port
        self.verbose = verbose
        self.ws: Optional[websocket.WebSocket] = None
        self.results: List[TestResult] = []
        self.colonist_id: Optional[int] = None
        self.thing_id: Optional[int] = None
        self.plant_id: Optional[int] = None
        self.building_id: Optional[int] = None
        self.zone_id: Optional[int] = None

    def connect(self) -> bool:
        """连接到 WebSocket 服务器"""
        url = f"ws://{self.host}:{self.port}/ai"
        print(f"\n{'='*60}")
        print(f"连接到 {url}...")

        try:
            self.ws = websocket.create_connection(url, timeout=10)
            # 接收欢迎消息
            welcome = self.ws.recv()
            welcome_data = json.loads(welcome)
            print(f"连接成功!")
            print(f"服务器消息: {welcome_data.get('message', 'N/A')}")
            print(f"可用命令数量: {welcome_data.get('commandCount', 'N/A')}")
            if self.verbose:
                print(f"\n欢迎消息完整响应:")
                print(self._format_json(welcome_data))
            return True
        except Exception as e:
            print(f"连接失败: {e}")
            return False

    def disconnect(self):
        """断开连接"""
        if self.ws:
            self.ws.close()
            self.ws = None

    def send_command(self, action: str, **params) -> Dict[str, Any]:
        """发送命令并接收响应"""
        if not self.ws:
            raise RuntimeError("未连接到服务器")

        command = {"action": action, **params}
        self.ws.send(json.dumps(command))
        response = self.ws.recv()
        return json.loads(response)

    def _format_json(self, data: Dict, indent: int = 4) -> str:
        """格式化JSON数据用于打印"""
        return json.dumps(data, ensure_ascii=False, indent=indent)

    def run_test(self, name: str, test_func: Callable, skip: bool = False) -> TestResult:
        """运行单个测试"""
        result = TestResult(name=name, status=TestStatus.PENDING)

        if skip:
            result.status = TestStatus.SKIPPED
            result.message = "测试被跳过"
            self.results.append(result)
            print(f"  [SKIP] {name}")
            return result

        print(f"\n  测试: {name}")
        start_time = time.time()

        try:
            result.status = TestStatus.RUNNING
            response = test_func()
            result.response = response
            result.duration = time.time() - start_time

            if response.get("success", False):
                result.status = TestStatus.PASSED
                result.message = "测试通过"
                print(f"  [PASS] {name} ({result.duration:.2f}s)")
                if self.verbose:
                    print(f"  响应数据:")
                    print(self._format_json(response))
            else:
                result.status = TestStatus.FAILED
                result.message = response.get("error", "未知错误")
                print(f"  [FAIL] {name}: {result.message}")
                if self.verbose:
                    print(f"  错误响应:")
                    print(self._format_json(response))

        except Exception as e:
            result.status = TestStatus.FAILED
            result.message = str(e)
            result.duration = time.time() - start_time
            print(f"  [ERROR] {name}: {e}")

        self.results.append(result)
        return result

    # ==================== 基础命令测试 ====================

    def test_ping(self) -> Dict:
        """测试连接"""
        return self.send_command("ping")

    # ==================== 地图和游戏状态测试 ====================

    def test_get_game_state(self) -> Dict:
        """获取游戏状态"""
        return self.send_command("get_game_state")

    def test_get_time_info(self) -> Dict:
        """获取时间和昼夜信息"""
        return self.send_command("get_time_info")

    def test_get_weather_info(self) -> Dict:
        """获取天气信息"""
        return self.send_command("get_weather_info")

    # ==================== 角色查询测试 ====================

    def test_get_all_pawns(self) -> Dict:
        """获取所有角色"""
        return self.send_command("get_all_pawns")

    def test_get_colonists(self) -> Dict:
        """获取殖民者"""
        response = self.send_command("get_colonists")

        # 保存第一个殖民者ID供后续测试使用
        if response.get("success") and response.get("data", {}).get("colonists"):
            self.colonist_id = response["data"]["colonists"][0].get("id")
            print(f"    保存殖民者ID: {self.colonist_id}")

        return response

    def test_get_prisoners(self) -> Dict:
        """获取囚犯"""
        return self.send_command("get_prisoners")

    def test_get_enemies(self) -> Dict:
        """获取敌人"""
        return self.send_command("get_enemies")

    def test_get_animals(self) -> Dict:
        """获取动物"""
        return self.send_command("get_animals")

    def test_get_pawn_info(self) -> Dict:
        """获取角色详情"""
        if not self.colonist_id:
            return {"success": False, "error": "没有可用的殖民者ID"}
        return self.send_command("get_pawn_info", pawnId=self.colonist_id)

    # ==================== 植物分类查询测试 ====================

    def test_get_trees(self) -> Dict:
        """获取所有树木"""
        return self.send_command("get_trees")

    def test_get_crops(self) -> Dict:
        """获取所有农作物"""
        return self.send_command("get_crops")

    def test_get_wild_harvestable(self) -> Dict:
        """获取野生可收获植物"""
        return self.send_command("get_wild_harvestable")

    def test_get_plant_by_def(self) -> Dict:
        """按defName获取特定植物"""
        return self.send_command("get_plant_by_def", defName="Plant_Corn")

    # ==================== 物品分类查询测试 ====================

    def test_get_food(self) -> Dict:
        """获取所有食物"""
        return self.send_command("get_food")

    def test_get_weapons(self) -> Dict:
        """获取所有武器"""
        return self.send_command("get_weapons")

    def test_get_apparel(self) -> Dict:
        """获取所有衣物"""
        return self.send_command("get_apparel")

    def test_get_medicine(self) -> Dict:
        """获取所有药品"""
        return self.send_command("get_medicine")

    def test_get_materials(self) -> Dict:
        """获取所有材料"""
        return self.send_command("get_materials")

    def test_get_item_by_def(self) -> Dict:
        """按defName获取特定物品"""
        return self.send_command("get_item_by_def", defName="Steel")

    # ==================== 建筑分类查询测试 ====================

    def test_get_production_buildings(self) -> Dict:
        """获取所有生产建筑"""
        response = self.send_command("get_production_buildings")

        # 保存建筑ID
        if response.get("success") and response.get("data", {}).get("types"):
            for type_name, type_data in response["data"]["types"].items():
                buildings = type_data.get("buildings", [])
                if buildings and not self.building_id:
                    self.building_id = buildings[0].get("id")
                    print(f"    保存建筑ID: {self.building_id} ({type_name})")
                    break

        return response

    def test_get_power_buildings(self) -> Dict:
        """获取所有电力建筑"""
        return self.send_command("get_power_buildings")

    def test_get_defense_buildings(self) -> Dict:
        """获取所有防御建筑"""
        return self.send_command("get_defense_buildings")

    def test_get_storage_buildings(self) -> Dict:
        """获取所有储存建筑"""
        return self.send_command("get_storage_buildings")

    def test_get_furniture(self) -> Dict:
        """获取所有家具"""
        return self.send_command("get_furniture")

    def test_get_building_by_def(self) -> Dict:
        """按defName获取特定建筑"""
        return self.send_command("get_building_by_def", defName="TableButcher")

    # ==================== 物品详情测试 ====================

    def test_get_thing_info(self) -> Dict:
        """获取物品详情"""
        if not self.thing_id:
            return {"success": False, "error": "没有可用的物品ID"}
        return self.send_command("get_thing_info", thingId=self.thing_id)

    # ==================== 区域和资源测试 ====================

    def test_get_zones(self) -> Dict:
        """获取区域"""
        response = self.send_command("get_zones", detailed=True)

        # 保存区域ID
        if response.get("success") and response.get("data", {}).get("zones"):
            self.zone_id = response["data"]["zones"][0].get("id")

        return response

    def test_get_zone_info(self) -> Dict:
        """获取区域详情"""
        if self.zone_id is None:  # 修复：使用 is None 而不是 if not（因为 id 可能是 0）
            return {"success": False, "error": "没有可用的区域ID"}
        return self.send_command("get_zone_info", zoneId=self.zone_id)

    def test_get_areas(self) -> Dict:
        """获取活动区域"""
        return self.send_command("get_areas")

    def test_get_room_info(self) -> Dict:
        """获取房间信息"""
        return self.send_command("get_room_info", x=125, z=125)

    def test_get_resources(self) -> Dict:
        """获取资源统计"""
        return self.send_command("get_resources", summary=True)

    def test_get_critical_resources(self) -> Dict:
        """获取关键资源"""
        return self.send_command("get_critical_resources")

    def test_get_wealth(self) -> Dict:
        """获取财富"""
        return self.send_command("get_wealth")

    # ==================== 工作系统测试 ====================

    def test_get_work_priorities(self) -> Dict:
        """获取工作优先级"""
        if not self.colonist_id:
            return {"success": False, "error": "没有可用的殖民者ID"}
        return self.send_command("get_work_priorities", pawnId=self.colonist_id)

    def test_get_work_types(self) -> Dict:
        """获取工作类型"""
        return self.send_command("get_work_types")

    def test_get_available_work(self) -> Dict:
        """获取可分配工作"""
        if not self.colonist_id:
            return {"success": False, "error": "没有可用的殖民者ID"}
        return self.send_command("get_available_work", pawnId=self.colonist_id)

    # ==================== 搬运系统测试 ====================

    def test_get_haulables(self) -> Dict:
        """获取可搬运物品"""
        return self.send_command("get_haulables")

    # ==================== 控制命令测试（需要谨慎） ====================

    def test_move_pawn(self) -> Dict:
        """移动角色"""
        if not self.colonist_id:
            return {"success": False, "error": "没有可用的殖民者ID"}
        return self.send_command(
            "move_pawn",
            pawnId=self.colonist_id,
            x=125,
            z=125
        )

    def test_stop_pawn(self) -> Dict:
        """停止角色"""
        if not self.colonist_id:
            return {"success": False, "error": "没有可用的殖民者ID"}
        return self.send_command("stop_pawn", pawnId=self.colonist_id)

    def test_set_work_priority(self) -> Dict:
        """设置工作优先级"""
        if not self.colonist_id:
            return {"success": False, "error": "没有可用的殖民者ID"}
        return self.send_command(
            "set_work_priority",
            pawnId=self.colonist_id,
            workType="Hauling",
            priority=2
        )

    def test_unlock_things(self) -> Dict:
        """解锁物品"""
        return self.send_command("unlock_things", all=False)

    def test_place_blueprint(self) -> Dict:
        """放置蓝图"""
        return self.send_command(
            "place_blueprint",
            defName="Wall",
            x=120,
            z=120,
            stuffDefName="WoodLog",
            rotation="north"
        )

    # ==================== 批量工作触发测试 ====================

    def test_trigger_work(self) -> Dict:
        """触发砍树工作"""
        return self.send_command("trigger_work", workType="PlantCutting")

    def test_get_supported_work_types(self) -> Dict:
        """获取支持的工作类型"""
        return self.send_command("get_supported_work_types")

    # ==================== 建筑位置推荐测试 ====================

    def test_get_recommended_build_positions_solar(self) -> Dict:
        """获取太阳能板推荐位置"""
        return self.send_command("get_recommended_build_positions",
            defName="SolarGenerator",
            count=3
        )

    def test_get_recommended_build_positions_wall(self) -> Dict:
        """获取墙壁推荐位置"""
        return self.send_command("get_recommended_build_positions",
            defName="Wall",
            count=5
        )

    # ==================== 区域管理测试 ====================

    def test_create_zone_stockpile(self) -> Dict:
        """创建储存区"""
        # 在地图中心附近创建一个小的储存区
        return self.send_command("create_zone",
            type="stockpile",
            cells=[{"x": 125, "z": 125}, {"x": 126, "z": 125}, {"x": 125, "z": 126}]
        )

    def test_create_zone_growing(self) -> Dict:
        """创建种植区"""
        return self.send_command("create_zone",
            type="growing",
            cells=[{"x": 130, "z": 130}, {"x": 131, "z": 130}, {"x": 130, "z": 131}]
        )

    def test_delete_zone(self) -> Dict:
        """删除区域（先获取区域列表，删除最后一个）"""
        zones_result = self.send_command("get_zones")
        if zones_result.get("success") and zones_result.get("data", {}).get("count", 0) > 0:
            zones = zones_result["data"]["zones"]
            last_zone_id = zones[-1]["id"]
            return self.send_command("delete_zone", zoneId=last_zone_id)
        return {"success": False, "error": "No zones to delete"}

    def test_add_cells_to_zone(self) -> Dict:
        """向区域添加格子"""
        zones_result = self.send_command("get_zones")
        if zones_result.get("success") and zones_result.get("data", {}).get("count", 0) > 0:
            zones = zones_result["data"]["zones"]
            first_zone_id = zones[0]["id"]
            return self.send_command("add_cells_to_zone",
                zoneId=first_zone_id,
                cells=[{"x": 135, "z": 135}]
            )
        return {"success": False, "error": "No zones available"}

    def test_remove_cells_from_zone(self) -> Dict:
        """从区域移除格子"""
        zones_result = self.send_command("get_zones")
        if zones_result.get("success") and zones_result.get("data", {}).get("count", 0) > 0:
            zones = zones_result["data"]["zones"]
            # 找一个有多格子的区域
            for zone in zones:
                if zone.get("cellCount", 0) > 1:
                    bounds = zone.get("bounds", {})
                    if bounds:
                        x = bounds.get("maxX", 0)
                        z = bounds.get("maxZ", 0)
                        return self.send_command("remove_cells_from_zone",
                            zoneId=zone["id"],
                            cells=[{"x": x, "z": z}]
                        )
        return {"success": False, "error": "No suitable zones available"}

    def test_set_growing_zone_plant(self) -> Dict:
        """设置种植区作物"""
        zones_result = self.send_command("get_zones")
        if zones_result.get("success"):
            zones = zones_result.get("data", {}).get("zones", [])
            for zone in zones:
                if zone.get("type") == "Zone_Growing":
                    return self.send_command("set_growing_zone_plant",
                        zoneId=zone["id"],
                        plantDefName="Plant_Rice"
                    )
        return {"success": False, "error": "No growing zones available"}

    # ==================== 批量测试 ====================

    def run_query_tests(self):
        """运行所有查询测试"""
        print("\n" + "="*60)
        print("查询命令测试")
        print("="*60)

        # 基础
        self.run_test("ping - 连接测试", self.test_ping)

        # 地图和游戏状态
        self.run_test("get_game_state - 游戏状态", self.test_get_game_state)

        # 时间和天气
        self.run_test("get_time_info - 时间和昼夜信息", self.test_get_time_info)
        self.run_test("get_weather_info - 天气信息", self.test_get_weather_info)

        # 角色查询
        self.run_test("get_all_pawns - 所有角色", self.test_get_all_pawns)
        self.run_test("get_colonists - 殖民者", self.test_get_colonists)
        self.run_test("get_prisoners - 囚犯", self.test_get_prisoners)
        self.run_test("get_enemies - 敌人", self.test_get_enemies)
        self.run_test("get_animals - 动物", self.test_get_animals)
        self.run_test("get_pawn_info - 角色详情", self.test_get_pawn_info)

        # 植物分类查询
        print("\n  --- 植物分类查询 ---")
        self.run_test("get_trees - 所有树木", self.test_get_trees)
        self.run_test("get_crops - 所有农作物", self.test_get_crops)
        self.run_test("get_wild_harvestable - 野生可收获植物", self.test_get_wild_harvestable)
        self.run_test("get_plant_by_def - 按defName获取植物", self.test_get_plant_by_def)

        # 物品分类查询
        print("\n  --- 物品分类查询 ---")
        self.run_test("get_food - 所有食物", self.test_get_food)
        self.run_test("get_weapons - 所有武器", self.test_get_weapons)
        self.run_test("get_apparel - 所有衣物", self.test_get_apparel)
        self.run_test("get_medicine - 所有药品", self.test_get_medicine)
        self.run_test("get_materials - 所有材料", self.test_get_materials)
        self.run_test("get_item_by_def - 按defName获取物品", self.test_get_item_by_def)

        # 建筑分类查询
        print("\n  --- 建筑分类查询 ---")
        self.run_test("get_production_buildings - 生产建筑", self.test_get_production_buildings)
        self.run_test("get_power_buildings - 电力建筑", self.test_get_power_buildings)
        self.run_test("get_defense_buildings - 防御建筑", self.test_get_defense_buildings)
        self.run_test("get_storage_buildings - 储存建筑", self.test_get_storage_buildings)
        self.run_test("get_furniture - 家具", self.test_get_furniture)
        self.run_test("get_building_by_def - 按defName获取建筑", self.test_get_building_by_def)

        # 物品详情
        self.run_test("get_thing_info - 物品详情", self.test_get_thing_info)

        # 区域和资源
        self.run_test("get_zones - 区域", self.test_get_zones)
        self.run_test("get_zone_info - 区域详情", self.test_get_zone_info)
        self.run_test("get_areas - 活动区域", self.test_get_areas)
        self.run_test("get_room_info - 房间信息", self.test_get_room_info)
        self.run_test("get_resources - 资源统计", self.test_get_resources)
        self.run_test("get_critical_resources - 关键资源", self.test_get_critical_resources)
        self.run_test("get_wealth - 财富", self.test_get_wealth)

        # 工作系统
        self.run_test("get_work_priorities - 工作优先级", self.test_get_work_priorities)
        self.run_test("get_work_types - 工作类型", self.test_get_work_types)
        self.run_test("get_available_work - 可分配工作", self.test_get_available_work)
        self.run_test("get_supported_work_types - 支持的工作类型", self.test_get_supported_work_types)

        # 建筑位置推荐
        self.run_test("get_recommended_build_positions - 太阳能板位置", self.test_get_recommended_build_positions_solar)
        self.run_test("get_recommended_build_positions - 墙壁位置", self.test_get_recommended_build_positions_wall)

        # 搬运和建造
        self.run_test("get_haulables - 可搬运物品", self.test_get_haulables)

    def run_control_tests(self):
        """运行控制命令测试（会影响游戏状态）"""
        print("\n" + "="*60)
        print("控制命令测试 (会影响游戏状态)")
        print("="*60)

        self.run_test("move_pawn - 移动角色", self.test_move_pawn)
        self.run_test("stop_pawn - 停止角色", self.test_stop_pawn)
        self.run_test("set_work_priority - 设置优先级", self.test_set_work_priority)
        self.run_test("unlock_things - 解锁物品", self.test_unlock_things)
        self.run_test("place_blueprint - 放置蓝图", self.test_place_blueprint)
        self.run_test("trigger_work - 触发砍树工作", self.test_trigger_work)

        # 区域管理测试
        print("\n  --- 区域管理测试 ---")
        self.run_test("create_zone - 创建储存区", self.test_create_zone_stockpile)
        self.run_test("create_zone - 创建种植区", self.test_create_zone_growing)
        self.run_test("add_cells_to_zone - 添加格子", self.test_add_cells_to_zone)
        self.run_test("set_growing_zone_plant - 设置作物", self.test_set_growing_zone_plant)
        self.run_test("remove_cells_from_zone - 移除格子", self.test_remove_cells_from_zone)
        self.run_test("delete_zone - 删除区域", self.test_delete_zone)

    def run_quick_tests(self):
        """快速测试（仅基础命令）"""
        print("\n" + "="*60)
        print("快速测试 (仅基础命令)")
        print("="*60)

        self.run_test("ping - 连接测试", self.test_ping)
        self.run_test("get_game_state - 游戏状态", self.test_get_game_state)
        self.run_test("get_time_info - 时间和昼夜信息", self.test_get_time_info)
        self.run_test("get_weather_info - 天气信息", self.test_get_weather_info)
        self.run_test("get_colonists - 殖民者", self.test_get_colonists)
        self.run_test("get_pawn_info - 角色详情", self.test_get_pawn_info)
        self.run_test("get_crops - 农作物", self.test_get_crops)
        self.run_test("get_food - 食物", self.test_get_food)
        self.run_test("get_resources - 资源统计", self.test_get_resources)

    def run_all_tests(self, include_control: bool = False):
        """运行所有测试"""
        self.run_query_tests()

        if include_control:
            print("\n" + "!"*60)
            print("警告: 控制命令测试会修改游戏状态!")
            print("!"*60)
            self.run_control_tests()

    def print_summary(self):
        """打印测试摘要"""
        print("\n" + "="*60)
        print("测试摘要")
        print("="*60)

        passed = sum(1 for r in self.results if r.status == TestStatus.PASSED)
        failed = sum(1 for r in self.results if r.status == TestStatus.FAILED)
        skipped = sum(1 for r in self.results if r.status == TestStatus.SKIPPED)
        total = len(self.results)

        print(f"\n总计: {total} 个测试")
        print(f"  通过: {passed}")
        print(f"  失败: {failed}")
        print(f"  跳过: {skipped}")

        if failed > 0:
            print("\n失败的测试:")
            for r in self.results:
                if r.status == TestStatus.FAILED:
                    print(f"  - {r.name}: {r.message}")

        # 计算总耗时
        total_time = sum(r.duration for r in self.results)
        print(f"\n总耗时: {total_time:.2f}s")

        return failed == 0


def main():
    parser = argparse.ArgumentParser(
        description="RimWorld AI Mod API 测试脚本",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
示例:
  python test_api.py                    # 运行所有查询测试
  python test_api.py --quick            # 快速测试
  python test_api.py --control          # 包含控制命令测试
  python test_api.py -v                 # 详细输出模式（打印JSON响应）
  python test_api.py --quick -v         # 快速测试 + 详细输出
  python test_api.py --host 192.168.1.100
        """
    )

    parser.add_argument("--host", default="localhost", help="服务器地址 (默认: localhost)")
    parser.add_argument("--port", type=int, default=8080, help="服务器端口 (默认: 8080)")
    parser.add_argument("--quick", action="store_true", help="快速测试模式")
    parser.add_argument("--control", action="store_true", help="包含控制命令测试")
    parser.add_argument("-v", "--verbose", action="store_true", help="详细输出模式，打印完整JSON响应")

    args = parser.parse_args()

    tester = RimWorldAITester(host=args.host, port=args.port, verbose=args.verbose)

    try:
        if not tester.connect():
            return 1

        if args.quick:
            tester.run_quick_tests()
        else:
            tester.run_all_tests(include_control=args.control)

        success = tester.print_summary()
        return 0 if success else 1

    except KeyboardInterrupt:
        print("\n\n测试被用户中断")
        return 130
    finally:
        tester.disconnect()


if __name__ == "__main__":
    exit(main())
