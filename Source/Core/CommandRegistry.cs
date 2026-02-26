using System.Collections.Generic;
using System.Linq;

namespace RimWorldAI.Core
{
    /// <summary>
    /// 命令定义
    /// </summary>
    public class CommandDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] RequiredParams { get; set; }
        public string[] OptionalParams { get; set; }
        public string Category { get; set; }
        /// <summary>
        /// 游戏背景知识和操作提示（用于 AI Agent 理解上下文）
        /// </summary>
        public string[] Hints { get; set; }

        public string ToHelpString()
        {
            var parts = new List<string> { Name };

            if (RequiredParams != null && RequiredParams.Length > 0)
            {
                parts.Add($"(需要: {string.Join(", ", RequiredParams)})");
            }

            if (OptionalParams != null && OptionalParams.Length > 0)
            {
                parts.Add($"(可选: {string.Join(", ", OptionalParams)})");
            }

            if (!string.IsNullOrEmpty(Description))
            {
                parts.Add($"- {Description}");
            }

            return string.Join(" ", parts);
        }
    }

    /// <summary>
    /// 命令注册表 - 统一管理所有 API 命令定义
    /// </summary>
    public static class CommandRegistry
    {
        /// <summary>
        /// 所有命令定义
        /// </summary>
        public static readonly Dictionary<string, CommandDefinition> Commands = new Dictionary<string, CommandDefinition>
        {
            // ==================== 基础命令 ====================
            ["ping"] = new CommandDefinition
            {
                Name = "ping",
                Description = "测试与游戏服务器的连接状态",
                Hints = new[]
                {
                    "[用途]验证MCP服务器是否正常运行并与游戏通信",
                    "[响应]返回 {\"success\":true,\"pong\":true} 表示连接正常",
                    "[时机]建议在执行任何操作前先ping确认连接"
                },
                Category = "basic"
            },

            // ==================== 地图和游戏状态 ====================
            ["get_game_state"] = new CommandDefinition
            {
                Name = "get_game_state",
                Description = "获取完整游戏状态（殖民者、资源、建筑、威胁等综合信息）",
                Hints = new[]
                {
                    "[RimWorld背景]RimWorld是一款殖民地生存模拟游戏，你需要管理一群坠机幸存者建立新家园",
                    "[核心资源]食物（Food）维持生存、钢铁（Steel）用于建造、木材（WoodLog）初期必备",
                    "[殖民者需求]需要食物、休息、娱乐、社交，心情过低会精神崩溃",
                    "[威胁类型]野生动物、袭击者（Raiders）、机械族（Mechanoids）、疾病、极端天气",
                    "[响应内容]包含colonists(殖民者列表)、resources(资源统计)、threats(当前威胁)、time(时间信息)"
                },
                Category = "query"
            },
            ["get_time_info"] = new CommandDefinition
            {
                Name = "get_time_info",
                Description = "获取时间和昼夜信息（tick、小时、季节、年份、太阳光照度）",
                Hints = new[]
                {
                    "[时间系统]RimWorld中1小时=2500 ticks，1天=24小时=60000 ticks",
                    "[昼夜循环]白天（6:00-18:00）太阳能发电，夜间需要电池供电",
                    "[季节影响]春季适合种植，夏季温度高，秋季收获季，冬季极寒且作物停止生长",
                    "[殖民者作息]夜间工作效率降低，需要分配休息时间",
                    "[响应字段]tick(游戏刻)、hourOfDay(0-23)、season(季节)、year(年份)、daylightFactor(光照系数0-1)"
                },
                Category = "query"
            },
            ["get_weather_info"] = new CommandDefinition
            {
                Name = "get_weather_info",
                Description = "获取天气信息（当前天气、降雨/雪率、风速、移动速度影响）",
                Hints = new[]
                {
                    "[天气类型]Clear(晴朗)、Rain(雨)、Fog(雾)、Snow(雪)、Storm(风暴)、Flashstorm(雷暴)",
                    "[太阳能影响]雨天/雪天太阳能发电效率降低，需要备用电源",
                    "[移动影响]恶劣天气降低移动速度，影响工作效率",
                    "[温度影响]极端天气可能导致殖民者体温过高/过低",
                    "[火灾风险]雷暴可能引发山火，需提前清理周围植被",
                    "[响应字段]weatherDef(天气类型)、rainRate(降雨率0-1)、snowRate(降雪率0-1)、windSpeed(风速)、moveSpeedFactor(移动速度系数)"
                },
                Category = "query"
            },

            // ==================== 角色查询 ====================
            ["get_all_pawns"] = new CommandDefinition
            {
                Name = "get_all_pawns",
                Description = "获取所有角色（殖民者、囚犯、敌人、动物）",
                Hints = new[]
                {
                    "[Pawn概念]Pawn是RimWorld中所有活体生物的统称",
                    "[角色类型]Colonist(殖民者-玩家控制)、Prisoner(囚犯)、Enemy(敌人)、Animal(动物)",
                    "[ID用途]返回的pawnId用于其他命令指定目标角色",
                    "[响应字段]返回数组，每项包含pawnId、name、kind(类型)、position(x,z)、healthState"
                },
                Category = "query"
            },
            ["get_colonists"] = new CommandDefinition
            {
                Name = "get_colonists",
                Description = "获取殖民者列表（你控制的角色）",
                Hints = new[]
                {
                    "[殖民者简介]殖民者是坠机幸存者，是你的主要资源和控制对象",
                    "[技能系统]每个殖民者有不同技能等级（0-20）和激情（无/感兴趣/热爱）",
                    "[特质系统]特质影响殖民者能力，如'勤奋'(工作快)或'懒惰'(工作慢)",
                    "[需求管理]殖民者需要：食物(饥饿)、休息(疲劳)、娱乐(心情)、社交",
                    "[心情系统]心情过低会触发精神崩溃，导致无法控制",
                    "[心情系统]moodLevel 0-100，<30%可能精神崩溃，<10%极高风险",
                    "[崩溃类型]极度悲伤(游荡)、疯狂流浪(离开)、纵火狂(放火)",
                    "[心情管理]高质量食物、舒适房间、娱乐设施可提升心情",
                    "[技能激情]Passion影响学习速度：None(无加成)、Minor(1.5倍)、Major(2倍)",
                    "[特质影响]FastLearner(学习快)、Cannibal(食人癖-吃人不减心情)",
                    "[响应字段]pawnId(唯一标识)、name、skills(技能列表)、traits(特质)、needs(hunger/rest/joy)、moodLevel、healthState"
                },
                Category = "query"
            },
            ["get_prisoners"] = new CommandDefinition
            {
                Name = "get_prisoners",
                Description = "获取囚犯列表",
                Hints = new[]
                {
                    "[囚犯来源]战斗中击倒敌人后可以俘虏，或救援受伤的访客",
                    "[招募系统]囚犯可以通过'狱卒聊天'逐步转化为殖民者",
                    "[招募条件]需要社交技能高的殖民者进行招募，囚犯心情和健康影响成功率",
                    "[牢房要求]需要封闭房间、床、食物来源，门需要保持关闭",
                    "[响应字段]pawnId、name、prisonState(囚犯状态)、recruitProgress(招募进度0-1)"
                },
                Category = "query"
            },
            ["get_enemies"] = new CommandDefinition
            {
                Name = "get_enemies",
                Description = "获取敌人列表",
                Hints = new[]
                {
                    "[敌人类型]Raider(袭击者-人类)、Mechanoid(机械族-高护甲)、Manhunter(疯狂动物)",
                    "[威胁等级]根据殖民地财富计算，财富越高袭击越强",
                    "[战斗策略]利用掩体(沙袋/墙壁)获得75%掩护加成，优先消灭近战单位",
                    "[装备差异]袭击者装备从原始武器到高级枪械不等",
                    "[响应字段]pawnId、name、kind(敌人类型)、equipment(装备)、threatLevel(威胁等级)"
                },
                Category = "query"
            },
            ["get_animals"] = new CommandDefinition
            {
                Name = "get_animals",
                Description = "获取殖民地动物列表",
                Hints = new[]
                {
                    "[动物分类]Wild(野生-可狩猎)、Tame(驯养-属于殖民地)、Farm(农场动物)",
                    "[驯养动物用途]搬运(Hauling)、战斗(Guard)、放牧(Grazing)、产奶/产蛋/产毛",
                    "[狩猎价值]野生动物可获取肉(Meat)和皮革(Leather)",
                    "[冬季注意]草食动物冬季需要干草(Hay)或青贮饲料(Silage)喂养",
                    "[响应字段]pawnId、name、animalType(种类)、isTame(是否驯养)、trainingLevel(训练等级)"
                },
                Category = "query"
            },
            ["get_pawn_info"] = new CommandDefinition
            {
                Name = "get_pawn_info",
                Description = "获取角色详情（技能、特质、健康、装备、心情等）",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数说明]pawnId: 从get_colonists/get_pawns获取的角色唯一标识",
                    "[技能系统]skillLevel(0-20级)，passion(None/Minor/Major影响学习速度)",
                    "[特质系统]traits数组，如'FastLearner'(学习快)、'Cannibal'(食人癖)",
                    "[健康系统]healthConditions(健康状况)、injuries(伤势)、diseases(疾病)",
                    "[心情系统]moodLevel(0-100)，低于30%可能精神崩溃",
                    "[详细技能]返回技能等级(0-20)和激情(None/Minor/Major)",
                    "[健康详情]healthConditions含伤口、疾病、义体、成瘾",
                    "[需求状态]needs包含hunger(饥饿)、rest(疲劳)、joy(娱乐)",
                    "[心情因素]moodFactors列出影响心情的具体因素(+/-值)",
                    "[装备详情]equipment列出当前装备的武器和衣物",
                    "[当前工作]currentJob显示正在执行的任务和目标",
                    "[响应字段]skills、traits、health、needs(需求)、equipment(装备)、currentJob(当前工作)"
                },
                Category = "query"
            },

            // ==================== 物品查询 ====================
            ["get_thing_info"] = new CommandDefinition
            {
                Name = "get_thing_info",
                Description = "获取物品详情",
                RequiredParams = new[] { "thingId" },
                Hints = new[]
                {
                    "[参数说明]thingId: 物品唯一标识，从get_food/get_weapons等命令获取",
                    "[物品状态]hitPoints(耐久度)、stackCount(堆叠数量)、rottable(是否可腐烂)",
                    "[腐烂系统]食物等物品有保质期，需要冷藏延长保存时间",
                    "[响应字段]thingId、defName(物品定义)、position、stackCount、hitPoints、quality(品质)"
                },
                Category = "query"
            },

            // ==================== 植物查询 ====================
            ["get_trees"] = new CommandDefinition
            {
                Name = "get_trees",
                Description = "获取所有树木(按橡树/松树/白杨等细分，包含每棵树的位置和状态)",
                Hints = new[]
                {
                    "[树木类型]Oak(橡树)、Pine(松树)、Poplar(白杨)、Birch(桦树)、Cecropia(号角树)",
                    "[砍伐收益]砍伐树木获得木材(WoodLog)，数量取决于树木大小",
                    "[砍伐方法]使用 trigger_work(PlantCutting) 让殖民者执行砍伐工作",
                    "[生长阶段]Growth(0-1)表示生长程度，未长成的小树木材产出少",
                    "[战略考虑]保留部分树木作为防风林和美观，也用于遮挡敌人视线",
                    "[响应字段]treeId、defName(树种)、position(x,z)、growth(生长度0-1)、woodYield(预计木材产出)"
                },
                Category = "query"
            },
            ["get_crops"] = new CommandDefinition
            {
                Name = "get_crops",
                Description = "获取所有农作物(按玉米/土豆/水稻等细分，包含每棵作物的位置和状态)",
                Hints = new[]
                {
                    "[作物类型]Rice(水稻-生长快)、Potato(土豆-适应性强)、Corn(玉米-产量高)、Cotton(棉花)、Healroot(药草)",
                    "[收获时机]Growth=1表示成熟，需要及时收获否则会腐烂",
                    "[收获方法]使用 trigger_work(Growing) 让殖民者种植/收获",
                    "[季节限制]冬季作物停止生长，需要提前规划种植周期",
                    "[响应字段]cropId、defName(作物类型)、position、growth(成熟度0-1)、harvestable(是否可收获)"
                },
                Category = "query"
            },
            ["get_wild_harvestable"] = new CommandDefinition
            {
                Name = "get_wild_harvestable",
                Description = "获取野生可收获植物(按野生浆果/仙人掌等细分)",
                Hints = new[]
                {
                    "[野生植物类型]Berries(浆果-食物)、Agave(龙舌兰-食物/纤维)、Saguaro(仙人掌)",
                    "[应急食物]野生浆果可直接食用，是开局应急食物来源",
                    "[收获方法]使用 trigger_work(Growing) 让殖民者收获野生植物",
                    "[再生特性]野生植物收获后会重新生长",
                    "[响应字段]plantId、defName、position、growth、nutrition(营养价值)"
                },
                Category = "query"
            },
            ["get_plant_by_def"] = new CommandDefinition
            {
                Name = "get_plant_by_def",
                Description = "按defName获取特定类型植物(如Plant_Corn)",
                RequiredParams = new[] { "defName" },
                Hints = new[]
                {
                    "[参数说明]defName: 植物定义名，如Plant_Corn、Plant_Rice、Plant_TreeOak",
                    "[用途]精确查询特定类型植物的数量和分布",
                    "[defName格式]通常为Plant_前缀+具体名称，如Plant_Potato",
                    "[响应字段]返回该类型所有植物的列表"
                },
                Category = "query"
            },

            // ==================== 物品分类查询 ====================
            ["get_food"] = new CommandDefinition
            {
                Name = "get_food",
                Description = "获取所有食物(按类型细分，包含位置和数量)",
                Hints = new[]
                {
                    "[食物类型]Raw(生食-需烹饪)、Meals(餐食-可直接吃)、Corpse(尸体-需屠宰)",
                    "[餐食品质]SimpleMeal(简单餐食)、FineMeal(精致餐食)、LavishMeal(奢华餐食)提供不同心情加成",
                    "[营养值]Nutrition值表示食物饱腹程度，成人每餐需要约0.8营养",
                    "[腐烂系统]食物有保质期，需要冷藏室延长保存，腐烂食物不能食用",
                    "[优先级]优先消耗即将腐烂的食物",
                    "[响应字段]thingId、defName、nutrition(营养值)、position、rotTime(腐烂剩余时间)"
                },
                Category = "query"
            },
            ["get_weapons"] = new CommandDefinition
            {
                Name = "get_weapons",
                Description = "获取所有武器(按类型细分，包含位置和数量)",
                Hints = new[]
                {
                    "[武器分类]Melee(近战)、Ranged(远程)、Grenade(手雷)",
                    "[近战武器]Knife(刀)、Spear(矛)、LongSword(长剑)、Mace(锤)",
                    "[远程武器]Bow(弓)、Pistol(手枪)、Rifle(步枪)、SniperRifle(狙击枪)",
                    "[伤害属性]DPS(每秒伤害)、Range(射程)、Accuracy(精准度)",
                    "[装备方法]使用 equip_tool(pawnId, thingId) 为殖民者装备武器",
                    "[响应字段]thingId、defName、damageType、dps、range、position"
                },
                Category = "query"
            },
            ["get_apparel"] = new CommandDefinition
            {
                Name = "get_apparel",
                Description = "获取所有衣物(按类型细分，包含位置和耐久)",
                Hints = new[]
                {
                    "[衣物分类]Headgear(头饰)、UpperBody(上衣)、LowerBody(下装)、FullBody(全身)",
                    "[温度保护]InsulationCold(保暖)、InsulationHeat(隔热)值",
                    "[防护属性]ArmorRating(护甲值)影响伤害减免",
                    "[季节需求]冬季需要保暖衣物(Parka大衣)，夏季需要轻薄衣物",
                    "[耐久系统]衣物耐久会下降，破损后保护效果降低",
                    "[响应字段]thingId、defName、layer(穿着层)、hitPoints(耐久)、insulationValues"
                },
                Category = "query"
            },
            ["get_medicine"] = new CommandDefinition
            {
                Name = "get_medicine",
                Description = "获取所有药品(按类型细分，包含位置和数量)",
                Hints = new[]
                {
                    "[药品类型]HerbalMedicine(草药-效果低)、Medicine(普通药品)、GlitterworldMedicine(高级药品)",
                    "[治疗质量]药品质量影响治疗效果和恢复速度",
                    "[获取方式]草药可种植Healroot获得，高级药品需要交易或制造",
                    "[医疗需求]战斗后检查殖民者伤势，及时治疗防止感染",
                    "[响应字段]thingId、defName、medicalPotency(药效0-1)、stackCount、position"
                },
                Category = "query"
            },
            ["get_materials"] = new CommandDefinition
            {
                Name = "get_materials",
                Description = "获取所有材料(木材/钢铁/布料等，按类型细分)",
                Hints = new[]
                {
                    "[基础材料]Steel(钢铁-建造主力)、WoodLog(木材-初期必备)、Cloth(布料)",
                    "[高级材料]Plasteel(塑钢-高级建筑)、Uranium(铀-高伤害武器)、Gold(黄金)",
                    "[获取方式]钢铁/铀需要挖矿，木材砍树获得，布料种植棉花或交易",
                    "[建造消耗]不同建筑消耗不同材料，查看蓝图了解需求",
                    "[响应字段]thingId、defName、stackCount(数量)、position"
                },
                Category = "query"
            },
            ["get_item_by_def"] = new CommandDefinition
            {
                Name = "get_item_by_def",
                Description = "按defName获取特定物品(如Steel, WoodLog)",
                RequiredParams = new[] { "defName" },
                Hints = new[]
                {
                    "[参数说明]defName: 物品定义名，如Steel、WoodLog、Cloth、Plasteel",
                    "[常用defName]Steel(钢铁)、WoodLog(木材)、Cloth(布料)、Chemfuel(化学燃料)",
                    "[用途]精确统计特定物品的总量和分布位置",
                    "[响应字段]返回该类型所有物品的列表及总数量"
                },
                Category = "query"
            },

            // ==================== 建筑分类查询 ====================
            ["get_production_buildings"] = new CommandDefinition
            {
                Name = "get_production_buildings",
                Description = "获取所有生产建筑(工作台等，按类型细分)",
                Hints = new[]
                {
                    "[建筑类型]TableButcher(屠宰台)、TableStove(灶台)、CraftingSpot(手工点)、Stonecutter(切石机)",
                    "[电力需求]高级工作台需要电力连接才能运行",
                    "[工作分配]殖民者需要被分配相关工作才能使用工作台",
                    "[效率因素]工作台速度受操作者技能影响",
                    "[响应字段]buildingId、defName、powerConsumption(耗电)、position、currentBill(当前任务)"
                },
                Category = "query"
            },
            ["get_power_buildings"] = new CommandDefinition
            {
                Name = "get_power_buildings",
                Description = "获取所有电力建筑(发电机/电池等，按类型细分)",
                Hints = new[]
                {
                    "[发电类型]SolarGenerator(太阳能-白天)、WindTurbine(风力-全天)、FueledGenerator(燃料-稳定)、Geothermal(地热-稳定高功率)",
                    "[储能设备]Battery(电池)储存多余电力，夜间使用",
                    "[电网系统]建筑通过电缆连接，电缆需要金属建造",
                    "[功率平衡]发电量>耗电量才能稳定运行，否则会停电",
                    "[响应字段]buildingId、defName、powerOutput(发电量)、storedEnergy(储电量)、position"
                },
                Category = "query"
            },
            ["get_defense_buildings"] = new CommandDefinition
            {
                Name = "get_defense_buildings",
                Description = "获取所有防御建筑(炮塔/陷阱/沙袋等，按类型细分)",
                Hints = new[]
                {
                    "[炮塔类型]MiniTurret(小型炮塔)、AutoTurret(自动炮塔)、Mortar(迫击炮)",
                    "[掩体系统]Sandbag(沙袋)、Barricade(路障)提供75%掩护加成",
                    "[陷阱系统]SpikeTrap(尖刺陷阱)、IEDTrap(爆炸陷阱)对敌人造成伤害",
                    "[电力需求]炮塔需要电力和弹药才能运作",
                    "[响应字段]buildingId、defName、powerConsumption、ammoCount(弹药)、position"
                },
                Category = "query"
            },
            ["get_storage_buildings"] = new CommandDefinition
            {
                Name = "get_storage_buildings",
                Description = "获取所有储存建筑(货架等，按类型细分)",
                Hints = new[]
                {
                    "[储存类型]Shelf(货架)、EquipmentRack(武器架)",
                    "[容量优势]货架比地面堆放更整洁，容量更大",
                    "[分类储存]可以设置只存放特定类型物品",
                    "[响应字段]buildingId、defName、storageCapacity(容量)、currentItems(当前物品)、position"
                },
                Category = "query"
            },
            ["get_furniture"] = new CommandDefinition
            {
                Name = "get_furniture",
                Description = "获取所有家具(床/椅子/桌子等，按类型细分)",
                Hints = new[]
                {
                    "[必需家具]Bed(床-睡眠)、Table(桌子-进餐)、Stool/Chair(椅子-坐着进餐)",
                    "[舒适度]家具舒适度(Comfort)影响殖民者休息质量",
                    "[美观度]装饰品增加房间美观度，提高殖民者心情",
                    "[品质系统]家具品质(Awful到Legendary)影响效果",
                    "[响应字段]buildingId、defName、quality(品质)、comfort(舒适度)、position"
                },
                Category = "query"
            },
            ["get_building_by_def"] = new CommandDefinition
            {
                Name = "get_building_by_def",
                Description = "按defName获取特定建筑(如TableButcher, Campfire)",
                RequiredParams = new[] { "defName" },
                Hints = new[]
                {
                    "[参数说明]defName: 建筑定义名，如Bed、TableStove、SolarGenerator",
                    "[用途]精确查询特定类型建筑的数量和状态",
                    "[常用defName]Bed(床)、TableStove(灶台)、Campfire(篝火)、Cooler(制冷机)",
                    "[响应字段]返回该类型所有建筑的列表"
                },
                Category = "query"
            },

            // ==================== 区域和资源 ====================
            ["get_zones"] = new CommandDefinition
            {
                Name = "get_zones",
                Description = "获取所有区域（储存区、种植区等）",
                OptionalParams = new[] { "detailed" },
                Hints = new[]
                {
                    "[区域类型]Stockpile(储存区-存放物品)、GrowingZone(种植区-种植作物)",
                    "[储存区用途]殖民者会自动将物品搬运到储存区",
                    "[种植区用途]殖民者会自动在种植区种植和收获作物",
                    "[参数detailed]设为true返回详细信息，包括格子列表和设置",
                    "[响应字段]zoneId、type、cellCount(格子数)、position、settings(储存设置)"
                },
                Category = "query"
            },
            ["get_zone_info"] = new CommandDefinition
            {
                Name = "get_zone_info",
                Description = "获取区域详情",
                RequiredParams = new[] { "zoneId" },
                Hints = new[]
                {
                    "[参数说明]zoneId: 区域唯一标识，从get_zones获取",
                    "[储存区信息]包含物品过滤设置、优先级、当前存放物品",
                    "[种植区信息]包含种植作物类型、生长状态",
                    "[响应字段]zoneId、type、cells(格子坐标)、settings、items(存放物品)"
                },
                Category = "query"
            },

            // 区域管理控制命令
            ["create_zone"] = new CommandDefinition
            {
                Name = "create_zone",
                Description = "创建新区域（储存区或种植区）",
                RequiredParams = new[] { "type" },
                OptionalParams = new[] { "cells" },
                Hints = new[]
                {
                    "[参数type]'stockpile'创建储存区，'growing'创建种植区",
                    "[参数cells]坐标数组，格式：[{\"x\":100,\"z\":100},{\"x\":101,\"z\":100}]",
                    "[储存区用途]殖民者会自动将解禁后的物品搬运到储存区",
                    "[种植区用途]殖民者会自动在种植区种植和收获作物",
                    "[操作流程]创建区域→设置过滤/作物→触发搬运/种植工作",
                    "[储存区规划]食物/药品需要冷藏室，用Cooler(制冷机)降温",
                    "[冷藏室设计]封闭房间+Cooler，目标温度-1°C(冷冻)或4°C(冷藏)",
                    "[种植区规划]靠近水源(土壤湿度高)，避开污染区域",
                    "[作物选择]Rice(水稻-快速)、Corn(玉米-高产)、Potato(土豆-耐寒)",
                    "[区域大小]储存区建议不超过100格(太大难管理)，种植区根据人口规划",
                    "[工作流]create_zone→set_storage_filter/apply_storage_preset→trigger_work(Hauling/Growing)",
                    "[响应字段]zoneId(新区域ID)、success、cellCount(格子数量)"
                },
                Category = "control"
            },
            ["delete_zone"] = new CommandDefinition
            {
                Name = "delete_zone",
                Description = "删除区域",
                RequiredParams = new[] { "zoneId" },
                Hints = new[]
                {
                    "[参数说明]zoneId: 要删除的区域ID",
                    "[注意]删除后区域内物品不会消失，但不再被管理",
                    "[响应字段]success、message"
                },
                Category = "control"
            },
            ["add_cells_to_zone"] = new CommandDefinition
            {
                Name = "add_cells_to_zone",
                Description = "向区域添加格子",
                RequiredParams = new[] { "zoneId", "cells" },
                Hints = new[]
                {
                    "[参数zoneId]目标区域ID",
                    "[参数cells]要添加的坐标数组，格式：[{\"x\":100,\"z\":100}]",
                    "[限制]只能添加可行走的格子，不能跨越墙壁/水域",
                    "[响应字段]success、addedCount(添加数量)、totalCells(总格子数)"
                },
                Category = "control"
            },
            ["remove_cells_from_zone"] = new CommandDefinition
            {
                Name = "remove_cells_from_zone",
                Description = "从区域移除格子",
                RequiredParams = new[] { "zoneId", "cells" },
                Hints = new[]
                {
                    "[参数zoneId]目标区域ID",
                    "[参数cells]要移除的坐标数组",
                    "[注意]移除后该位置的物品不再受区域管理",
                    "[响应字段]success、removedCount、totalCells"
                },
                Category = "control"
            },
            ["set_growing_zone_plant"] = new CommandDefinition
            {
                Name = "set_growing_zone_plant",
                Description = "设置种植区的作物类型",
                RequiredParams = new[] { "zoneId", "plantDefName" },
                Hints = new[]
                {
                    "[参数zoneId]种植区ID",
                    "[参数plantDefName]作物定义名",
                    "[作物选择]Plant_Rice(水稻-生长快)、Plant_Potato(土豆-适应强)、Plant_Corn(玉米-产量高)",
                    "[经济作物]Plant_Cotton(棉花)、Plant_Healroot(药草)、Plant_Devilstrand(魔鬼丝)",
                    "[季节考虑]冬季作物停止生长，选择生长周期短的作物",
                    "[响应字段]success、plantDefName、zoneId"
                },
                Category = "control"
            },

            // ==================== 储存系统管理 ====================
            ["get_storage_settings"] = new CommandDefinition
            {
                Name = "get_storage_settings",
                Description = "获取储存区的详细设置（优先级、物品过滤）",
                RequiredParams = new[] { "zoneId" },
                Hints = new[]
                {
                    "[参数zoneId]储存区ID",
                    "[优先级系统]决定殖民者搬运物品的顺序",
                    "[过滤系统]控制哪些物品可以存放",
                    "[查看内容]返回储存区的优先级、允许的物品类别、当前物品列表",
                    "[诊断用途]储存区不接收物品？查看filter确认是否正确设置",
                    "[优先级检查]返回的priority字段显示当前优先级",
                    "[物品统计]返回当前区域内物品数量和类型分布",
                    "[响应字段]priority(优先级)、filter(过滤设置)、allowedItems(允许的物品类别)"
                },
                Category = "query"
            },
            ["set_storage_filter"] = new CommandDefinition
            {
                Name = "set_storage_filter",
                Description = "设置储存区的物品过滤规则（允许/禁止特定类别或物品）",
                RequiredParams = new[] { "zoneId" },
                OptionalParams = new[] { "mode", "categories", "defs", "allow" },
                Hints = new[]
                {
                    "[参数zoneId]储存区ID",
                    "[参数mode]'allowAll'(允许所有) 或 'disallowAll'(禁止所有)",
                    "[参数categories]类别名，用get_thing_categories查看可用类别",
                    "[参数defs]具体物品defName数组",
                    "[参数allow]true=允许，false=禁止",
                    "[使用示例]mode='disallowAll'→categories=['Food']→allow=true 表示只允许食物",
                    "[过滤逻辑]先设mode(disallowAll禁止所有)，再用categories+allow精确开启",
                    "[食物储存]mode='disallowAll'→categories=['Foods']→allow=true",
                    "[材料储存]mode='disallowAll'→categories=['Resources']→allow=true",
                    "[精细控制]用defs参数精确指定物品defName，如defs='[\"Steel\",\"WoodLog\"]'",
                    "[腐烂防护]食物区域应放在冷藏室，配合Cooler使用",
                    "[响应字段]success、updatedFilter(更新后的过滤设置)"
                },
                Category = "control"
            },
            ["set_storage_priority"] = new CommandDefinition
            {
                Name = "set_storage_priority",
                Description = "设置储存区的优先级（Low/Normal/Preferred/Important/Critical）",
                RequiredParams = new[] { "zoneId", "priority" },
                Hints = new[]
                {
                    "[参数zoneId]储存区ID",
                    "[参数priority]优先级字符串：Low/Normal/Preferred/Important/Critical",
                    "[优先级含义]Critical(关键)→Important(重要)→Preferred(优先)→Normal(普通)→Low(低)",
                    "[搬运逻辑]殖民者优先填充高优先级储存区",
                    "[建议设置]食物/药品设Critical，普通物资设Normal",
                    "[优先级排序]Critical > Important > Preferred > Normal > Low",
                    "[搬运逻辑]殖民者优先填充高优先级储存区，满后再填低优先级",
                    "[推荐设置]食物/药品=Critical(关键)，常用材料=Important(重要)",
                    "[分区管理]可创建多个储存区按优先级分层，如食物分Critical和Normal",
                    "[常见错误]所有区域同优先级会导致物品分散存放",
                    "[响应字段]success、newPriority"
                },
                Category = "control"
            },
            ["get_thing_categories"] = new CommandDefinition
            {
                Name = "get_thing_categories",
                Description = "获取物品类别树（用于设置储存区过滤时了解可用类别）",
                OptionalParams = new[] { "parentCategory" },
                Hints = new[]
                {
                    "[参数parentCategory]可选，查看指定父类别下的子类别",
                    "[根类别]Foods(食物)、Weapons(武器)、Apparel(衣物)、Resources(资源)等",
                    "[子类别]如Foods下有Meals、RawFood等",
                    "[响应字段]categories(类别列表)、parent(父类别)"
                },
                Category = "query"
            },
            ["get_storage_presets"] = new CommandDefinition
            {
                Name = "get_storage_presets",
                Description = "获取常用储存预设列表（food/materials/weapons等）",
                Hints = new[]
                {
                    "[预设用途]快速设置专用储存区，无需手动配置过滤",
                    "[可用预设]food(食物)、meals(餐食)、raw_food(生食)、materials(材料)、weapons(武器)",
                    "[扩展预设]apparel(衣物)、medicine(药品)、items(物品)、corpses(尸体)、chunks(石块)",
                    "[响应字段]presets(预设列表及包含的物品类型)"
                },
                Category = "query"
            },
            ["apply_storage_preset"] = new CommandDefinition
            {
                Name = "apply_storage_preset",
                Description = "应用储存预设到指定储存区（快速设置专用储存区）",
                RequiredParams = new[] { "zoneId", "presetName" },
                Hints = new[]
                {
                    "[参数zoneId]储存区ID",
                    "[参数presetName]预设名称，用get_storage_presets查看全部",
                    "[常用预设]food、materials、weapons、apparel、medicine",
                    "[效果]自动配置该预设对应的物品过滤规则",
                    "[预设列表]food(食物)、meals(餐食)、raw_food(生食)、materials(材料)、weapons(武器)、apparel(衣物)、medicine(药品)",
                    "[快速配置]比手动set_storage_filter更快，一键设置专用储存区",
                    "[使用流程]create_zone→apply_storage_preset→(可选)set_storage_priority",
                    "[组合建议]食物区用food预设+Critical优先级，材料区用materials+Important",
                    "[扩展预设]corpses(尸体-远离居住区)、chunks(石块-建材区)",
                    "[响应字段]success、appliedPreset、filterSummary"
                },
                Category = "control"
            },

            ["get_areas"] = new CommandDefinition
            {
                Name = "get_areas",
                Description = "获取活动区域",
                Hints = new[]
                {
                    "[活动区域]限制殖民者/动物可移动的范围",
                    "[用途]防止殖民者进入危险区域或保护特定区域",
                    "[响应字段]areas(区域列表)、name、color、cellCount"
                },
                Category = "query"
            },
            ["get_room_info"] = new CommandDefinition
            {
                Name = "get_room_info",
                Description = "获取房间信息",
                RequiredParams = new[] { "x", "z" },
                Hints = new[]
                {
                    "[参数x/z]地图坐标，查询该位置所在房间",
                    "[房间属性]温度、美观度、空间大小、污染程度",
                    "[房间类型]卧室、餐厅、厨房、监狱等",
                    "[响应字段]roomId、roomType、temperature、beauty、space"
                },
                Category = "query"
            },
            ["get_resources"] = new CommandDefinition
            {
                Name = "get_resources",
                Description = "获取资源统计",
                OptionalParams = new[] { "summary" },
                Hints = new[]
                {
                    "[参数summary]true返回简要统计，false返回详细列表",
                    "[资源类型]包括所有可用物品：食物、材料、武器等",
                    "[响应字段]resources(按类型分组的资源)、totalCount、totalValue"
                },
                Category = "query"
            },
            ["get_critical_resources"] = new CommandDefinition
            {
                Name = "get_critical_resources",
                Description = "获取关键资源概览（食物、药品等生存必需品）",
                Hints = new[]
                {
                    "[关键资源]食物(够吃几天)、药品(治疗能力)、燃料(发电/取暖)",
                    "[预警系统]数量低于阈值会标记为警告",
                    "[生存建议]食物<5天需紧急补充，药品不足需种植/交易",
                    "[响应字段]food(食物统计)、medicine(药品统计)、fuel(燃料统计)、warnings(警告列表)"
                },
                Category = "query"
            },
            ["get_wealth"] = new CommandDefinition
            {
                Name = "get_wealth",
                Description = "获取财富概览",
                Hints = new[]
                {
                    "[财富计算]包括所有物品、建筑、殖民者的总价值",
                    "[威胁关联]财富越高，袭击者数量和装备越强",
                    "[财富分类]Items(物品)、Buildings(建筑)、Pawns(角色)",
                    "[策略建议]平衡发展与防御，财富增长同时加强防御",
                    "[响应字段]totalWealth(总财富)、breakdown(分类明细)"
                },
                Category = "query"
            },

            // ==================== 工作系统 ====================
            ["get_work_priorities"] = new CommandDefinition
            {
                Name = "get_work_priorities",
                Description = "获取工作优先级",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[优先级系统]0=不从事，1=低优先，2=普通，3=优先，4=重要",
                    "[工作类型]Doctor、Patient、Firefighter、Construction、Hauling等",
                    "[查看用途]查看殖民者当前工作分配，用于优化分工",
                    "[响应格式]返回各工作类型及对应优先级(0-4)",
                    "[分工建议]确保每项关键工作至少有1-2人优先级>=2",
                    "[常见问题]某工作无人做？检查是否有人该工作优先级>0",
                    "[响应字段]pawnId、priorities(各工作类型的优先级)"
                },
                Category = "work"
            },
            ["set_work_priority"] = new CommandDefinition
            {
                Name = "set_work_priority",
                Description = "设置工作优先级（0=不从事，1-4=优先级从低到高）",
                RequiredParams = new[] { "pawnId", "workType", "priority" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[参数workType]工作类型，如Doctor、Hauling、Construction等",
                    "[参数priority]0=不从事，1=低优先，2=普通，3=优先，4=重要",
                    "[技能匹配]根据殖民者技能设置优先级，让擅长的人做擅长的事",
                    "[优先级系统]0=不从事，1=低，2=普通，3=优先，4=重要/最优先",
                    "[分配原则]让擅长的人做擅长的事，技能高+有激情者优先分配",
                    "[紧急设置]Doctor/Patient/Firefighter在紧急时应设为3-4",
                    "[避免冲突]同一优先级时殖民者按距离选择工作",
                    "[团队分工]建议：高技能者设核心工作优先级4，辅助工作设2-3",
                    "[示例]高Construction技能者：Construction=4, Mining=3, Hauling=2",
                    "[响应字段]success、newPriority"
                },
                Category = "control"
            },
            ["get_work_types"] = new CommandDefinition
            {
                Name = "get_work_types",
                Description = "获取所有工作类型",
                Hints = new[]
                {
                    "[工作类型列表]Firefighter、Patient、Doctor、BasicWorker、Warden、Handling、Cooking、Hunting、Construction、Repair、Growing、Mining、PlantCutting、Smithing、Tailoring、Art、Hauling、Cleaning、Research",
                    "[完整列表]Firefighter/Patient/Doctor/BasicWorker/Warden/Handling/Cooking/Hunting/Construction/Repair/Growing/Mining/PlantCutting/Smithing/Tailoring/Art/Hauling/Cleaning/Research",
                    "[工作说明]Firefighter(灭火)、Patient(就医)、Doctor(治疗病人)、Warden(管理囚犯)、Handling(驯兽)",
                    "[依赖关系]Cooking需要灶台，Research需要研究台，Smithing需要锻造台",
                    "[紧急程度]Firefighter/Doctor/Patient应始终有人可做",
                    "[响应字段]workTypes(所有工作类型及描述)"
                },
                Category = "work"
            },
            ["get_available_work"] = new CommandDefinition
            {
                Name = "get_available_work",
                Description = "获取可分配工作",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[返回内容]该殖民者可以从事的工作类型及其当前优先级",
                    "[响应字段]pawnId、availableWork(可分配工作列表)"
                },
                Category = "work"
            },

            // ==================== 搬运系统 ====================
            ["get_haulables"] = new CommandDefinition
            {
                Name = "get_haulables",
                Description = "获取需要搬运的物品（散落在地上的物品）",
                Hints = new[]
                {
                    "[开局场景]游戏开局时，物资会散落在地并带有禁止(Forbidden)属性",
                    "[搬运前提]1.使用unlock_things解禁物品 2.创建储存区(create_zone)",
                    "[自动搬运]解禁+储存区后，殖民者会自动搬运，无需手动分配",
                    "[加速方法]使用trigger_work(Hauling)让殖民者优先搬运",
                    "[响应字段]thingId、defName、position、stackCount、forbidden(是否被禁止)"
                },
                Category = "query"
            },

            // ==================== 建造管理 ====================
            ["get_blueprints"] = new CommandDefinition
            {
                Name = "get_blueprints",
                Description = "获取所有待建造的蓝图列表（含位置、材料需求、进度）",
                Hints = new[]
                {
                    "[蓝图概念]蓝图是待建造的建筑计划，放置后需要殖民者建造",
                    "[建造条件]需要足够材料和有Construction工作的殖民者",
                    "[材料需求]每个蓝图显示所需材料类型和数量",
                    "[加速建造]使用trigger_work(Construction)加快建造速度",
                    "[响应字段]blueprintId、defName、position、materialsNeeded(材料需求)、buildProgress(进度)"
                },
                Category = "query"
            },
            ["cancel_blueprint"] = new CommandDefinition
            {
                Name = "cancel_blueprint",
                Description = "取消指定的建造蓝图",
                RequiredParams = new[] { "blueprintId" },
                Hints = new[]
                {
                    "[参数blueprintId]蓝图ID，从get_blueprints获取",
                    "[效果]取消蓝图，已消耗的材料不会返还",
                    "[响应字段]success、message"
                },
                Category = "control"
            },
            ["get_buildable_defs"] = new CommandDefinition
            {
                Name = "get_buildable_defs",
                Description = "获取可建造的建筑定义列表（按类别筛选）",
                OptionalParams = new[] { "category" },
                Hints = new[]
                {
                    "[参数category]类别过滤：furniture(家具)、production(生产)、power(电力)、defense(防御)、temperature(温控)",
                    "[返回内容]可建造的建筑及其defName、成本、描述",
                    "[defName用途]用于place_blueprint命令放置蓝图",
                    "[响应字段]buildings(建筑列表)、category、defName、cost(建造成本)"
                },
                Category = "query"
            },

            // ==================== 批量工作触发 ====================
            ["trigger_work"] = new CommandDefinition
            {
                Name = "trigger_work",
                Description = "触发指定类型的工作，自动分配空闲殖民者执行",
                RequiredParams = new[] { "workType" },
                Hints = new[]
                {
                    "[参数workType]工作类型，见下方说明",
                    "[Firefighter]消防-发生火灾时使用，殖民者会扑灭火焰",
                    "[Doctor]医疗-有人受伤/生病时使用，殖民者会治疗病人",
                    "[Patient]就医-殖民者需要治疗时使用，让受伤殖民者去病床",
                    "[Hauling]搬运-物品散落时使用，需先unlock_things+创建储存区",
                    "[Cleaning]清洁-基地有污垢时使用，殖民者会清理地面",
                    "[PlantCutting]砍树-需要木材时使用，会自动标记殖民地附近的树木供砍伐",
                    "[Growing]种植/收获-农作物需要管理时使用",
                    "[Mining]挖矿-需要矿石时使用，需要先设计矿脉蓝图",
                    "[Construction]建造-有蓝图需要建造时使用",
                    "[Repair]维修-建筑损坏时使用",
                    "[Hunting]狩猎-需要肉类/皮革时使用",
                    "[Cooking]烹饪-需要食物时使用",
                    "[Research]研究-需要科技进度时使用",
                    "[自动标记]PlantCutting会自动在半径40格内标记最多30棵树",
                    "[工作分配逻辑]殖民者按优先级顺序执行工作，优先级高的工作先做",
                    "[多工作协调]可同时触发多种工作，殖民者根据优先级自动选择",
                    "[紧急情况]Firefighter(火灾)、Doctor(伤员)会自动提升优先级",
                    "[工作效率]殖民者技能影响工作速度，高技能殖民者优先分配",
                    "[注意事项]Hauling需要先unlock_things+创建储存区才能生效",
                    "[响应字段]success、assignedColonists(被分配的殖民者)、designationResult(标记结果)"
                },
                Category = "control"
            },
            ["get_supported_work_types"] = new CommandDefinition
            {
                Name = "get_supported_work_types",
                Description = "获取所有支持的工作类型列表（用于trigger_work命令）",
                Hints = new[]
                {
                    "[用途]查看trigger_work命令支持的所有工作类型",
                    "[响应字段]workTypes(工作类型列表及详细说明)"
                },
                Category = "work"
            },

            // ==================== 控制命令 ====================
            ["move_pawn"] = new CommandDefinition
            {
                Name = "move_pawn",
                Description = "移动角色到指定位置",
                RequiredParams = new[] { "pawnId", "x", "z" },
                Hints = new[]
                {
                    "[参数pawnId]角色ID（殖民者/动物）",
                    "[参数x/z]目标地图坐标",
                    "[寻路系统]殖民者会自动寻路，避开障碍和危险区域",
                    "[危险警告]移动到敌人/火源附近可能导致受伤",
                    "[移动限制]角色只能移动到可行走区域，不能穿越墙壁/水域",
                    "[危险规避]移动时自动避开火源，但不会自动避开敌人",
                    "[速度因素]移动速度受地形、负重、天气、健康状态影响",
                    "[战斗移动]战斗中移动会触发近战攻击，远程单位应保持距离",
                    "[停止移动]使用stop_pawn可中断移动命令",
                    "[响应字段]success、targetPosition"
                },
                Category = "control"
            },
            ["stop_pawn"] = new CommandDefinition
            {
                Name = "stop_pawn",
                Description = "停止角色当前任务",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]角色ID",
                    "[效果]立即停止当前工作/移动，角色进入空闲状态",
                    "[响应字段]success、stoppedJob(被停止的工作)"
                },
                Category = "control"
            },
            ["attack_target"] = new CommandDefinition
            {
                Name = "attack_target",
                Description = "攻击目标",
                RequiredParams = new[] { "pawnId", "targetId" },
                Hints = new[]
                {
                    "[参数pawnId]攻击者ID（殖民者）",
                    "[参数targetId]目标ID（敌人/动物，从get_enemies/get_animals获取）",
                    "[战斗系统]殖民者会使用装备的武器进行攻击",
                    "[战斗建议]利用掩体、集中火力、优先消灭近战单位",
                    "[战斗基础]殖民者使用装备的武器攻击，未装备时使用徒手(伤害低)",
                    "[武器射程]近战武器需贴近目标，远程武器可保持距离射击",
                    "[命中率]受射击技能、武器精度、掩体、距离影响",
                    "[掩护系统]站在沙袋/墙角后获得75%掩护加成，减少被命中率",
                    "[目标选择]优先攻击近战单位和低护甲目标，集火消灭威胁",
                    "[撤退时机]殖民者生命值低于30%应撤退治疗",
                    "[响应字段]success、targetId、combatStarted"
                },
                Category = "control"
            },
            ["equip_tool"] = new CommandDefinition
            {
                Name = "equip_tool",
                Description = "装备武器/工具",
                RequiredParams = new[] { "pawnId", "thingId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[参数thingId]物品ID（武器/工具，从get_weapons获取）",
                    "[武器效果]提高战斗力，不同武器有不同伤害和射程",
                    "[工具效果]部分工作使用特定工具效率更高",
                    "[装备限制]每个殖民者只能装备一件武器，装备新武器会卸下旧武器",
                    "[武器类型]近战(刀/剑/锤)适合高近战技能者，远程(枪/弓)适合高射击技能者",
                    "[装备来源]get_weapons获取地上武器ID，殖民者装备栏武器不在此列",
                    "[换装流程]move_pawn移动到武器附近→equip_tool装备→attack_target攻击",
                    "[卸下装备]殖民者会自动将旧武器放到地上，可在原位置找到",
                    "[响应字段]success、equippedItem(装备的物品)"
                },
                Category = "control"
            },
            ["unlock_things"] = new CommandDefinition
            {
                Name = "unlock_things",
                Description = "解锁被禁止的物品（使其可被搬运和使用）",
                OptionalParams = new[] { "thingId", "all" },
                Hints = new[]
                {
                    "[参数thingId]指定物品ID，解禁单个物品",
                    "[参数all]设为true解禁所有被禁止的物品",
                    "[开局必做]游戏开局时物资带有禁止(Forbidden)属性，必须解禁才能搬运",
                    "[搬运流程]unlock_things(all=true) → create_zone(type='stockpile') → trigger_work(Hauling)",
                    "[开局流程]坠机开局时，所有物资带有禁止(Forbidden)属性，必须解禁",
                    "[搬运前置]不解禁物品，殖民者不会搬运，储存区无法填充",
                    "[批量操作]使用all=true一键解禁所有物品，节省时间",
                    "[精细控制]使用thingId解禁单个物品，适合保护特定资源",
                    "[常见问题]解禁后物品仍不搬运？检查：1.是否有储存区 2.是否有人分配Hauling工作",
                    "[响应字段]success、unlockedCount(解禁数量)"
                },
                Category = "control"
            },
            ["place_blueprint"] = new CommandDefinition
            {
                Name = "place_blueprint",
                Description = "放置建造蓝图",
                RequiredParams = new[] { "defName", "x", "z" },
                OptionalParams = new[] { "stuffDefName", "rotation" },
                Hints = new[]
                {
                    "[参数defName]建筑定义名，如Bed、TableStove、SolarGenerator，用get_buildable_defs查看全部",
                    "[参数x/z]放置位置的地图坐标",
                    "[参数stuffDefName]建造材料，如WoodLog(木材)、Steel(钢铁)、Plasteel(塑钢)",
                    "[参数rotation]旋转方向：North/East/South/West，默认North",
                    "[建造流程]place_blueprint → 确保有材料 → trigger_work(Construction)",
                    "[建筑规划]优先建造顺序：卧室(休息)→厨房(食物)→餐厅(进餐)→防御工事",
                    "[房间尺寸]卧室建议4x4或更大，餐厅6x10以上，厨房靠近储存区",
                    "[材料选择]木材(快速建造但易燃)、钢铁(均衡)、塑钢(坚固但稀有)",
                    "[电力规划]建筑需要电力时，确保附近有电缆连接，用get_power_buildings查看电网",
                    "[常见错误]材料不足时蓝图不会建造，用get_materials检查库存",
                    "[响应字段]success、blueprintId、position、materialsNeeded"
                },
                Category = "control"
            },

            // ==================== ESDF 和 Voronoi 地图分析 ====================
            ["get_esdf_map"] = new CommandDefinition
            {
                Name = "get_esdf_map",
                Description = "获取ESDF距离场地图（每个格子到最近障碍物的距离和障碍物类型）",
                Hints = new[]
                {
                    "[ESDF概念]Euclidean Signed Distance Function，计算每个格子到最近障碍物的距离",
                    "[AI用途]建造时保持安全间距，工作台离墙1格，主走廊保持3格宽",
                    "[距离值]0=障碍物，1=靠墙，2=窄通道，3+=宽阔区域",
                    "[安全区域]safeAreas返回距离>=3的大块区域，适合放置重要建筑",
                    "[障碍物分布]obstacleTypeDistribution返回各类型障碍物数量（terrain/building/thing）",
                    "[障碍物样本]sampleObstacles返回各类型障碍物的代表样本",
                    "[响应字段]summary(统计)、distanceDistribution(距离分布)、obstacleTypeDistribution(障碍物类型分布)、sampleObstacles(障碍物样本)、safeAreas(安全区域列表)"
                },
                Category = "map"
            },
            ["get_voronoi_map"] = new CommandDefinition
            {
                Name = "get_voronoi_map",
                Description = "获取Voronoi骨架地图（基地主干道和区域划分，含障碍物和房间信息）",
                Hints = new[]
                {
                    "[Voronoi概念]从障碍物分布提取地图骨架，找到离障碍物最远的路径",
                    "[骨架节点]nodes是ESDF局部最大值点，代表最安全的位置",
                    "[节点障碍物]每个节点包含nearbyObstacles，显示8方向搜索到的障碍物（类型/方向/距离）",
                    "[骨架边]edges连接相邻节点，形成主干道",
                    "[区域房间]regions包含roomInfo，显示是否室内/房间角色/区域内建筑类型",
                    "[AI用途]沿骨架建造主走廊，骨架划分的区域适合规划功能区",
                    "[响应字段]nodes(含nearbyObstacles)、edges、regions(含roomInfo)、summary(统计)"
                },
                Category = "map"
            },
            ["find_build_locations"] = new CommandDefinition
            {
                Name = "find_build_locations",
                Description = "查找适合建造的位置（基于ESDF安全距离，返回附近障碍物信息）",
                OptionalParams = new[] { "buildingDef", "minDistance", "preferIndoor", "limit" },
                Hints = new[]
                {
                    "[参数buildingDef]建筑定义名（可选），用于未来扩展",
                    "[参数minDistance]离墙最小距离，默认1（工作台建议1，床可0）",
                    "[参数preferIndoor]是否优先室内，默认true",
                    "[参数limit]返回数量限制，默认10，最大100",
                    "[评分规则]距离越远越好，室内加分",
                    "[最近障碍物]nearestObstacle返回距离最近的障碍物详情（类型/名称/坐标）",
                    "[附近障碍物]nearbyObstacles返回8方向搜索到的障碍物列表（含方向/距离）",
                    "[响应字段]locations(位置列表)、totalCandidates(总候选数)"
                },
                Category = "map"
            },
            ["get_cell_info"] = new CommandDefinition
            {
                Name = "get_cell_info",
                Description = "获取指定位置的详细信息（ESDF、地形、建筑、房间）",
                RequiredParams = new[] { "x", "z" },
                Hints = new[]
                {
                    "[参数x]X坐标（必填）",
                    "[参数z]Z坐标（必填）",
                    "[AI视觉]返回该位置的完整信息，让AI能\"看见\"该格子",
                    "[ESDF信息]distance到最近障碍物距离，nearestObstacle障碍物详情（类型/名称/坐标）",
                    "[地形信息]terrain包含地形类型、是否是水/河",
                    "[建筑信息]building如果有建筑则返回建筑类型",
                    "[房间信息]room包含房间ID、角色、是否室内、大小",
                    "[响应字段]position、esdf(含nearestObstacle)、terrain、building、room"
                },
                Category = "map"
            },
            ["get_river"] = new CommandDefinition
            {
                Name = "get_river",
                Description = "获取河流信息（使用BFS识别连通区域和走向，包含浅水）",
                RequiredParams = new string[] { },
                Hints = new[]
                {
                    "[BFS算法]使用广度优先搜索识别连通的河流区域（包括河流和浅水）",
                    "[返回信息]hasRiver是否有河流、totalRiverCells总格数、direction走向(north-south/east-west)",
                    "[河段信息]segments包含每个河段的边界、中心点、格子数",
                    "[穿越点]totalFordCells浅滩数量（可穿越河流的点）",
                    "[AI用途]帮助AI了解地图上的水源分布和河流走向，规划基地时避开河流或利用浅滩"
                },
                Category = "map"
            },
            ["get_marsh"] = new CommandDefinition
            {
                Name = "get_marsh",
                Description = "获取沼泽信息（使用BFS识别连通区域）",
                RequiredParams = new string[] { },
                Hints = new[]
                {
                    "[BFS算法]使用广度优先搜索识别连通的沼泽区域",
                    "[检测条件]地形defName包含Marsh或Swamp",
                    "[返回信息]hasMarsh是否有沼泽、totalMarshCells总格数、segmentCount区域数量",
                    "[区域信息]segments包含每个沼泽区域的边界和格子数",
                    "[AI用途]帮助AI识别沼泽区域，规划基地时避开或利用沼泽"
                },
                Category = "map"
            }
        };

        /// <summary>
        /// 获取查询命令列表
        /// </summary>
        public static string[] GetQueryCommands()
        {
            return Commands
                .Where(kvp => kvp.Value.Category == "query" || kvp.Value.Category == "basic" || kvp.Value.Category == "work")
                .Select(kvp => kvp.Value.ToHelpString())
                .ToArray();
        }

        /// <summary>
        /// 获取控制命令列表
        /// </summary>
        public static string[] GetControlCommands()
        {
            return Commands
                .Where(kvp => kvp.Value.Category == "control")
                .Select(kvp => kvp.Value.ToHelpString())
                .ToArray();
        }

        /// <summary>
        /// 获取所有命令列表
        /// </summary>
        public static string[] GetAllCommands()
        {
            return Commands.Values
                .Select(cmd => cmd.ToHelpString())
                .ToArray();
        }

        /// <summary>
        /// 按分类获取命令
        /// </summary>
        public static Dictionary<string, string[]> GetCommandsByCategory()
        {
            return Commands.Values
                .GroupBy(cmd => cmd.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(cmd => cmd.ToHelpString()).ToArray()
                );
        }

        /// <summary>
        /// 检查命令是否存在
        /// </summary>
        public static bool IsValidCommand(string action)
        {
            return Commands.ContainsKey(action);
        }

        /// <summary>
        /// 获取命令定义
        /// </summary>
        public static CommandDefinition GetCommand(string action)
        {
            return Commands.TryGetValue(action, out var cmd) ? cmd : null;
        }
    }
}
