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
                    "[响应字段]pawnId(唯一标识)、name、skills(技能列表)、traits(特质)、needs(hunger/rest/joy)、moodLevel、healthState",
                    // === 新增：室内需求 ===
                    "[室内需求-重要]殖民者需要在室内睡觉、吃饭、工作！室外会降低心情",
                    "[睡眠环境]床必须放在封闭房间内，有屋顶，否则会冻死/热死/心情差",
                    "[工作环境]工作台、研究台必须在室内，室外工作会受天气影响",
                    "[温度控制]室内可以安装Cooler(冷却器)和Heater(加热器)控制温度",
                    "[舒适度]室内地板、照明、家具提升殖民者心情和工作效率",
                    "[基地规划]确保每个殖民者有独立卧室或至少有床在室内房间"
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
                Description = "获取角色详情（技能、特质、健康、装备、心情、睡眠环境等）",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数说明]pawnId: 从get_colonists/get_pawns获取的角色唯一标识",
                    "[技能系统]skillLevel(0-20级)，passion(None/Minor/Major影响学习速度)",
                    "[特质系统]traits数组，如'FastLearner'(学习快)、'Cannibal'(食人癖)",
                    "[健康系统]healthPercent(总体健康百分比)、hediffs(伤害/疾病列表)",
                    "[健康扩展]canCrawl(能否爬行)、painLevel(疼痛等级0.0+)、bleedRate(出血率)、canBleed(能否出血)",
                    "[心情系统]moodLevel(0-100)，低于30%可能精神崩溃",
                    "[需求状态]needs包含hunger(饥饿)、rest(疲劳)、joy(娱乐)、comfort(舒适度)、outdoors(户外)、chemical(化学需求/成瘾)",
                    "[能力统计]ticksPerMoveCardinal/Diagonal(移动速度tick)、psychicSensitivity(精神敏感度)、disabledWorkTags(禁止工作)",
                    "[睡眠环境]sleepEnvironment包含：hasOwnedBed(是否有床)、isRoofed(是否有屋顶)、isOutdoors(是否户外)、temperature(温度)",
                    "[睡眠威胁]sleepEnvironment.warnings数组，包含生存威胁警告如'露天低温-会冻死'、'高温中暑-会热死'",
                    "[睡眠危险]sleepEnvironment.isDangerous=true表示睡眠环境有生命危险，需要立即处理！",
                    "[精神状态]mentalState(精神崩溃类型)、inspiration(灵感类型)",
                    "[装备详情]equipment列出当前装备的武器和衣物",
                    "[当前工作]curJob显示正在执行的任务",
                    "[响应字段]skills、traits、hediffs、needs、equipment、sleepEnvironment、mentalState、inspiration等"
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
            ["get_exposed_items"] = new CommandDefinition
            {
                Name = "get_exposed_items",
                Description = "检测露天/暴露物资（生存关键：检测正在恶化和腐烂的物资）",
                Hints = new[]
                {
                    "[核心用途]检测暴露在室外会恶化/腐烂的物资，避免资源损失",
                    "[露天定义]无屋顶、心理户外、接触地图边缘的区域",
                    "[恶化机制]部分物品在室外会持续损失耐久度，如武器/衣物/食物",
                    "[腐烂机制]食物类物品会随时间腐烂，露天会加速腐烂",
                    "[返回内容]items数组列出所有露天物资及其恶化状态",
                    "[关键字段]isDangerous=true表示有威胁，warnings数组列出具体威胁",
                    "[恶化率]deteriorationRate>0表示正在恶化，单位：/天",
                    "[腐烂时间]ticksUntilRot表示剩余腐烂时间，2500ticks=1游戏小时",
                    "[紧急处理]isDangerous=true的物资需要立即搬运到室内或有屋顶的区域",
                    "[响应字段]totalCount(总物资数)、exposedCount(露天数)、deterioratingCount(恶化中)、items(详细列表)"
                },
                Category = "query"
            },
            // ==================== 生产系统 ====================
            ["get_workbench_recipes"] = new CommandDefinition
            {
                Name = "get_workbench_recipes",
                Description = "获取工作台可用配方列表",
                RequiredParams = new[] { "thingId" },
                Hints = new[]
                {
                    "[参数thingId]工作台建筑ID（从get_production_buildings获取）",
                    "[返回内容]配方列表，包含产品、原料、技能需求、研究前置等",
                    "[用途]了解工作台能生产什么物品",
                    "[配方字段]defName(ID)、label(名称)、workAmount(工作量)、products(产品)、ingredients(原料)",
                    "[技能需求]skillRequirements显示需要的技能等级",
                    "[可用性]availableNow表示配方是否已解锁",
                    "[响应字段]recipeCount(配方数)、recipes(配方列表)"
                },
                Category = "production"
            },
            ["get_workbench_bills"] = new CommandDefinition
            {
                Name = "get_workbench_bills",
                Description = "获取工作台当前生产账单（任务）",
                RequiredParams = new[] { "thingId" },
                Hints = new[]
                {
                    "[参数thingId]工作台建筑ID",
                    "[返回内容]当前账单列表，包含重复模式、暂停状态、进度等",
                    "[用途]查看工作台正在生产什么",
                    "[重复模式]Forever(永远)、RepeatCount(重复N次)、TargetCount(目标数量)",
                    "[存储模式]storeMode: BestStockpile(最佳储存)、DropOnFloor(放地上)",
                    "[暂停状态]paused=true时账单不会被执行",
                    "[进度]currentCount/targetCount显示目标完成进度",
                    "[响应字段]billCount(账单数)、bills(账单详情列表)"
                },
                Category = "production"
            },

            // ==================== 医疗系统 ====================
            ["get_medical_care"] = new CommandDefinition
            {
                Name = "get_medical_care",
                Description = "获取殖民者医疗护理设置（护理级别、自疗、药物政策）",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[医疗级别]medCareLevel 0-4: 0=无护理, 1=无药物, 2=草药, 3=普通药物, 4=最佳",
                    "[自疗]selfTend=true允许殖民者自己处理伤口",
                    "[药物政策]drugPolicy包含当前分配的药物政策信息",
                    "[生存关键]低医疗级别可能导致重伤殖民者得不到及时治疗",
                    "[响应字段]medCareLevel、medCareLabel、selfTend、drugPolicy"
                },
                Category = "medical"
            },
            ["set_medical_care"] = new CommandDefinition
            {
                Name = "set_medical_care",
                Description = "设置殖民者医疗护理级别",
                RequiredParams = new[] { "pawnId", "medCareLevel" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[参数medCareLevel]0=无护理, 1=无药物(接受治疗), 2=草药或更差, 3=普通药物或更差, 4=最佳医疗",
                    "[建议]重要殖民者设置为4(最佳)，普通殖民者设置2-3",
                    "[紧急情况]战斗前检查并提升医疗级别",
                    "[响应字段]success、oldLevel、newLevel、newLabel"
                },
                Category = "medical"
            },
            ["set_self_tend"] = new CommandDefinition
            {
                Name = "set_self_tend",
                Description = "设置殖民者是否允许自疗",
                RequiredParams = new[] { "pawnId", "allowed" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[参数allowed]true=允许自疗, false=禁止自疗",
                    "[注意事项]医生殖民者应该允许自疗以便治疗自己",
                    "[风险]非医生自疗效果差，可能留下疤痕",
                    "[响应字段]success、oldSelfTend、newSelfTend"
                },
                Category = "medical"
            },
            ["get_drug_policy"] = new CommandDefinition
            {
                Name = "get_drug_policy",
                Description = "获取殖民者药物政策详情",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[返回内容]药物政策名称、药物列表、服用条件",
                    "[药物信息]每种药物的defName、label、服用频率、触发条件",
                    "[触发条件]moodThreshold(心情阈值)、expectationThreshold(期望阈值)",
                    "[成瘾风险]某些药物有成瘾风险，需要监控使用",
                    "[响应字段]policyId、label、drugCount、drugs列表"
                },
                Category = "medical"
            },

            // ==================== 通知系统 ====================
            ["get_notifications"] = new CommandDefinition
            {
                Name = "get_notifications",
                Description = "获取游戏通知列表(Letter信件/Message消息)，支持增量查询",
                OptionalParams = new[] { "sinceId", "onlyNew", "limit" },
                Hints = new[]
                {
                    "[参数sinceId]获取此ID之后的通知，用于增量查询",
                    "[参数onlyNew]true=仅返回未读通知",
                    "[参数limit]最大返回数量(1-200，默认50)",
                    "[通知类型]letter-右下角可点击通知(袭击/交易/任务)",
                    "[通知类型]message-左上角临时提示(殖民者状态)",
                    "[响应字段]notifications数组、count、lastId、stats统计"
                },
                Category = "notification"
            },
            ["mark_all_notifications_read"] = new CommandDefinition
            {
                Name = "mark_all_notifications_read",
                Description = "标记所有通知为已读",
                Hints = new[]
                {
                    "[用途]批量清除未读状态",
                    "[响应字段]success"
                },
                Category = "notification"
            },
            ["clear_notifications"] = new CommandDefinition
            {
                Name = "clear_notifications",
                Description = "清除所有已收集的通知",
                Hints = new[]
                {
                    "[用途]清空通知队列，释放内存",
                    "[注意]清除后无法恢复历史通知",
                    "[响应字段]success"
                },
                Category = "notification"
            },
            ["get_alerts"] = new CommandDefinition
            {
                Name = "get_alerts",
                Description = "获取当前活动的警报列表",
                Hints = new[]
                {
                    "[警报类型]饥饿、健康、电力不足、温度异常、需要研究等",
                    "[优先级]Critical(紧急)>High(高)>Medium(中)>Low(低)",
                    "[处理建议]Critical和High警报需要优先处理",
                    "[culprits]相关目标列表，如受影响的殖民者",
                    "[响应字段]alerts数组、count"
                },
                Category = "notification"
            },

            // ==================== 殖民者控制 ====================
            ["draft_pawn"] = new CommandDefinition
            {
                Name = "draft_pawn",
                Description = "征召殖民者进入战斗状态",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[效果]殖民者将进入征召状态，可以接受攻击命令",
                    "[自动开火]默认启用FireAtWill",
                    "[限制]昏迷、休眠中的殖民者无法征召",
                    "[响应字段]success、wasDrafted、nowDrafted、fireAtWill"
                },
                Category = "pawn_control"
            },
            ["undraft_pawn"] = new CommandDefinition
            {
                Name = "undraft_pawn",
                Description = "解除殖民者的征召状态",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[效果]殖民者退出战斗状态，恢复日常工作",
                    "[响应字段]success、wasDrafted、nowDrafted"
                },
                Category = "pawn_control"
            },
            ["set_fire_at_will"] = new CommandDefinition
            {
                Name = "set_fire_at_will",
                Description = "设置殖民者是否自动开火",
                RequiredParams = new[] { "pawnId", "fireAtWill" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[参数fireAtWill]true=自动开火, false=不开火(需要手动指定目标)",
                    "[限制]仅对已征召的殖民者有效",
                    "[响应字段]success、wasFireAtWill、nowFireAtWill"
                },
                Category = "pawn_control"
            },
            ["get_timetable"] = new CommandDefinition
            {
                Name = "get_timetable",
                Description = "获取殖民者的24小时日程安排",
                RequiredParams = new[] { "pawnId" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[返回内容]24小时日程表，每小时对应一种安排",
                    "[安排类型]Anything(任意)、Sleep(睡眠)、Meditate(冥想)",
                    "[响应字段]timetable数组、currentHour、currentAssignment"
                },
                Category = "pawn_control"
            },
            ["set_timetable"] = new CommandDefinition
            {
                Name = "set_timetable",
                Description = "设置殖民者单个小时的日程安排",
                RequiredParams = new[] { "pawnId", "hour", "assignment" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[参数hour]小时(0-23)",
                    "[参数assignment]安排类型: Anything/Sleep/Meditate",
                    "[示例]设置22点睡眠: hour=22, assignment=Sleep",
                    "[响应字段]success、oldAssignment、newAssignment"
                },
                Category = "pawn_control"
            },
            ["set_timetable_range"] = new CommandDefinition
            {
                Name = "set_timetable_range",
                Description = "批量设置殖民者的日程安排",
                RequiredParams = new[] { "pawnId", "startHour", "endHour", "assignment" },
                Hints = new[]
                {
                    "[参数pawnId]殖民者ID",
                    "[参数startHour]开始小时(0-23)",
                    "[参数endHour]结束小时(0-23)",
                    "[参数assignment]安排类型: Anything/Sleep/Meditate",
                    "[示例]设置22:00-06:00为睡眠: startHour=22, endHour=6, assignment=Sleep",
                    "[响应字段]success、hoursChanged"
                },
                Category = "pawn_control"
            },
            ["get_time_assignments"] = new CommandDefinition
            {
                Name = "get_time_assignments",
                Description = "获取所有可用的时间安排类型",
                Hints = new[]
                {
                    "[返回内容]所有TimeAssignmentDef列表",
                    "[常见类型]Anything(任意工作)、Sleep(强制睡眠)、Meditate(冥想)",
                    "[DLC]Meditate需要Royalty DLC",
                    "[响应字段]assignments数组、count"
                },
                Category = "pawn_control"
            },
            ["get_use_work_priorities"] = new CommandDefinition
            {
                Name = "get_use_work_priorities",
                Description = "查询是否启用了手动工作优先级模式",
                Hints = new[]
                {
                    "[重要性]必须启用此模式后，set_work_priority 命令才会生效",
                    "[默认值]游戏默认为false，所有殖民者使用默认优先级3",
                    "[响应字段]useWorkPriorities(true/false)、description"
                },
                Category = "work_control"
            },
            ["set_use_work_priorities"] = new CommandDefinition
            {
                Name = "set_use_work_priorities",
                Description = "设置是否启用手动工作优先级模式",
                RequiredParams = new[] { "enabled" },
                Hints = new[]
                {
                    "[参数enabled]true启用/false禁用",
                    "[使用场景]首次使用 set_work_priority 前必须调用此命令启用",
                    "[效果]启用后可以设置每个殖民者的工作优先级(1-4)",
                    "[响应字段]useWorkPriorities、previousValue、changed、message"
                },
                Category = "work_control"
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
            ["get_buildings"] = new CommandDefinition
            {
                Name = "get_buildings",
                Description = "获取建筑列表（按类别筛选）",
                OptionalParams = new[] { "category" },
                Hints = new[]
                {
                    "[参数category]类别: production(生产)、power(电力)、defense(防御)、storage(储存)、furniture(家具)、all(全部-默认)",
                    "[生产建筑]TableButcher(屠宰台)、TableStove(灶台)、CraftingSpot(手工点)、Stonecutter(切石机)",
                    "[电力建筑]SolarGenerator(太阳能)、WindTurbine(风力)、Battery(电池)、Geothermal(地热)",
                    "[防御建筑]MiniTurret(炮塔)、Sandbag(沙袋)、SpikeTrap(尖刺陷阱)",
                    "[储存建筑]Shelf(货架)、EquipmentRack(武器架)",
                    "[家具]Bed(床)、Table(桌子)、Chair(椅子)",
                    "[响应字段]category、totalCount、types(按defName分组的建筑列表)"
                },
                Category = "query"
            },
            // 兼容旧命令（重定向到 get_buildings）
            ["get_production_buildings"] = new CommandDefinition
            {
                Name = "get_production_buildings",
                Description = "获取所有生产建筑(兼容命令，建议使用get_buildings?category=production)",
                Hints = new[]
                {
                    "[建议]使用 get_buildings?category=production 替代",
                    "[建筑类型]TableButcher(屠宰台)、TableStove(灶台)、CraftingSpot(手工点)、Stonecutter(切石机)"
                },
                Category = "query"
            },
            ["get_power_buildings"] = new CommandDefinition
            {
                Name = "get_power_buildings",
                Description = "获取所有电力建筑(兼容命令，建议使用get_buildings?category=power)",
                Hints = new[]
                {
                    "[建议]使用 get_buildings?category=power 替代",
                    "[发电类型]SolarGenerator(太阳能)、WindTurbine(风力)、Geothermal(地热)"
                },
                Category = "query"
            },
            ["get_defense_buildings"] = new CommandDefinition
            {
                Name = "get_defense_buildings",
                Description = "获取所有防御建筑(兼容命令，建议使用get_buildings?category=defense)",
                Hints = new[]
                {
                    "[建议]使用 get_buildings?category=defense 替代",
                    "[防御类型]MiniTurret(炮塔)、Sandbag(沙袋)、SpikeTrap(陷阱)"
                },
                Category = "query"
            },
            ["get_storage_buildings"] = new CommandDefinition
            {
                Name = "get_storage_buildings",
                Description = "获取所有储存建筑(兼容命令，建议使用get_buildings?category=storage)",
                Hints = new[]
                {
                    "[建议]使用 get_buildings?category=storage 替代",
                    "[储存类型]Shelf(货架)、EquipmentRack(武器架)"
                },
                Category = "query"
            },
            ["get_furniture"] = new CommandDefinition
            {
                Name = "get_furniture",
                Description = "获取所有家具(兼容命令，建议使用get_buildings?category=furniture)",
                Hints = new[]
                {
                    "[建议]使用 get_buildings?category=furniture 替代",
                    "[家具类型]Bed(床)、Table(桌子)、Chair(椅子)"
                },
                Category = "query"
            },
            ["get_research_status"] = new CommandDefinition
            {
                Name = "get_research_status",
                Description = "获取研究系统状态(当前项目/可用项目/已完成项目)",
                Hints = new[]
                {
                    "[研究台]需要建造研究台(TableResearch)才能进行研究",
                    "[当前项目]currentProject字段显示正在研究的项目，null表示未选择",
                    "[进度显示]progress为0-1之间的浮点数，表示完成百分比",
                    "[可用项目]availableProjects列出所有前置条件已满足的项目",
                    "[已完成]completedProjects列出所有已完成的研究",
                    "[设置项目]使用set_research_project命令设置研究项目"
                },
                Category = "query"
            },
            ["set_research_project"] = new CommandDefinition
            {
                Name = "set_research_project",
                Description = "设置当前研究项目",
                RequiredParams = new[] { "projectDefName" },
                Hints = new[]
                {
                    "[参数]projectDefName: 研究项目的defName，如Electricity(电力)、Batteries(电池)、SolarPanels(太阳能板)",
                    "[前置条件]研究项目的前置条件必须已完成",
                    "[查看可用]使用get_research_status查看availableProjects列表",
                    "[常见项目]Electricity(电力基础)、Batteries(电池)、SolarPanels(太阳能板)、GunTurrets(炮塔)",
                    "[切换研究]可以随时切换研究项目，已研究的进度会保留",
                    "[错误情况]如果项目不存在、已完成或前置条件未满足，会返回错误信息"
                },
                Category = "control"
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

            // ==================== 区域管理控制命令 ====================
            ["create_area"] = new CommandDefinition
            {
                Name = "create_area",
                Description = "创建允许区域（用于限制角色活动范围）",
                RequiredParams = new[] { "name" },
                OptionalParams = new[] { "cells" },
                Hints = new[]
                {
                    "[参数name]区域名称",
                    "[参数cells]坐标数组JSON，格式：[{\"x\":100,\"z\":100},{\"x\":101,\"z\":100}]",
                    "[用途]创建区域后可用于限制殖民者/动物的活动范围",
                    "[工作流]create_area→set_pawn_area_restriction",
                    "[响应字段]success、areaId(新区域ID)、name、cellCount"
                },
                Category = "control"
            },
            ["delete_area"] = new CommandDefinition
            {
                Name = "delete_area",
                Description = "删除允许区域",
                RequiredParams = new[] { "areaId" },
                Hints = new[]
                {
                    "[参数areaId]要删除的区域ID，从get_areas获取",
                    "[注意]删除后角色将不受该区域限制",
                    "[响应字段]success、message"
                },
                Category = "control"
            },
            ["rename_area"] = new CommandDefinition
            {
                Name = "rename_area",
                Description = "重命名允许区域",
                RequiredParams = new[] { "areaId", "name" },
                Hints = new[]
                {
                    "[参数areaId]区域ID",
                    "[参数name]新名称",
                    "[响应字段]success、newName"
                },
                Category = "control"
            },
            ["add_cells_to_area"] = new CommandDefinition
            {
                Name = "add_cells_to_area",
                Description = "向区域添加格子",
                RequiredParams = new[] { "areaId", "cells" },
                Hints = new[]
                {
                    "[参数areaId]目标区域ID",
                    "[参数cells]坐标数组JSON，格式：[{\"x\":100,\"z\":100}]",
                    "[限制]只能添加可行走的格子",
                    "[响应字段]success、addedCount、totalCells"
                },
                Category = "control"
            },
            ["remove_cells_from_area"] = new CommandDefinition
            {
                Name = "remove_cells_from_area",
                Description = "从区域移除格子",
                RequiredParams = new[] { "areaId", "cells" },
                Hints = new[]
                {
                    "[参数areaId]目标区域ID",
                    "[参数cells]要移除的坐标数组JSON",
                    "[响应字段]success、removedCount、totalCells"
                },
                Category = "control"
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
                Description = "获取工作类型列表",
                OptionalParams = new[] { "filter" },
                Hints = new[]
                {
                    "[参数filter]筛选模式: all(所有类型-默认) 或 supported(仅trigger_work支持的类型)",
                    "[工作类型列表]Firefighter、Patient、Doctor、BasicWorker、Warden、Handling、Cooking、Hunting、Construction、Repair、Growing、Mining、PlantCutting、Smithing、Tailoring、Art、Hauling、Cleaning、Research",
                    "[工作说明]Firefighter(灭火)、Patient(就医)、Doctor(治疗病人)、Warden(管理囚犯)、Handling(驯兽)",
                    "[依赖关系]Cooking需要灶台，Research需要研究台，Smithing需要锻造台",
                    "[紧急程度]Firefighter/Doctor/Patient应始终有人可做",
                    "[响应字段]filter、count、workTypes(工作类型列表)"
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
                    "[响应字段]buildings(建筑列表)、category、defName、cost(建造成本)",
                    // === 新增：室内外分类 ===
                    "[必须室内]Bed(床)、TableBase(餐桌)、Chair(椅子)、工作台、研究台、储存区",
                    "[必须室外]SolarGenerator(太阳能)、WindTurbine(风车)、种植区、陷阱",
                    "[墙内设施]Cooler(冷却器)、Heater(加热器)、Vent(通风口)需要墙或室内",
                    "[门的位置]Door必须放在墙上，连通室内外或房间之间",
                    "[建造原则]先建墙围出房间，再放室内设施，最后放室外设施"
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
                Description = "放置单个建造蓝图（配合get_room_boundary批量建造房间）",
                RequiredParams = new[] { "defName", "x", "z" },
                OptionalParams = new[] { "stuffDefName", "rotation" },
                Hints = new[]
                {
                    "[参数defName]建筑定义名，如Wall(墙)、Door(门)、Bed(床)，用get_buildable_defs查看全部",
                    "[参数x/z]放置位置的地图坐标",
                    "[参数stuffDefName]建造材料，如WoodLog(木材)、Steel(钢铁)、Plasteel(塑钢)",
                    "[参数rotation]旋转方向：North/East/South/West，默认North",
                    "[新工作流]建造房间时：1.get_room_boundary获取边界 2.对missingCells逐个调用place_blueprint",
                    "[旧工作流-废弃]不再需要自己计算16面墙的坐标，使用get_room_boundary一次性获取",
                    "[建造流程]place_blueprint → 确保有材料 → trigger_work(Construction)",
                    "[材料选择]木材(快速建造但易燃)、钢铁(均衡)、塑钢(坚固但稀有)",
                    "[常见错误]材料不足时蓝图不会建造，用get_materials检查库存",
                    "[响应字段]success、blueprintId、position、materialsNeeded",
                    // === 新增：室内优先原则 ===
                    "[室内优先-关键]所有设施必须放在室内！床/工作台/研究台在室外是严重错误！",
                    "[建造顺序-重要]先建封闭房间，再放家具！不要先放床再考虑建墙！",
                    "[正确流程]1.选址 → 2.建造封闭房间(get_room_boundary+build=true) → 3.等建成 → 4.放家具",
                    "[错误流程]❌ 先放床在室外 → 再考虑建墙 → 结果：殖民者睡在露天",
                    "[设施分类]床(Bed)/餐桌(Table)/工作台必须在室内，只有太阳能/风车在室外",
                    "[室内好处]室内有温度控制(不冻死/热死)、不受天气影响、提升心情",
                    "[建造优先级]卧室(5x5+) → 餐厅(7x7+) → 厨房(5x5+) → 工作室(7x7+) → 防御",
                    "[不要省空间]房间宁可建大也不要太小，太小无法使用且殖民者会不高兴"
                },
                Category = "control"
            },

            // ==================== 地图分析与选址工具 ====================
            // 选址工作流: get_esdf_map(了解地形) → get_voronoi_map(骨架规划) → find_build_locations(找位置) → get_room_boundary(建造)

            ["get_esdf_map"] = new CommandDefinition
            {
                Name = "get_esdf_map",
                Description = "ESDF距离场地图 - 分析地图开阔程度，找适合建造的安全区域",
                Hints = new[]
                {
                    // === 核心用途 ===
                    "[核心用途]分析地图每个格子到最近障碍物的距离，找开阔安全的建造区域",
                    "[选址第一步]新建基地时先调用此命令，了解哪里有足够空间",
                    // === 概念解释 ===
                    "[ESDF]Euclidean Signed Distance Field，距离值越大越开阔安全",
                    "[距离含义]0=障碍物本身，1=紧贴障碍，2=窄通道，3+=可建造，5+=宽阔",
                    // === 返回数据 ===
                    "[safeAreas]返回距离>=3的连通区域列表，每个区域含边界、中心、格子数",
                    "[obstacleTypes]返回障碍物类型分布：terrain(地形)、building(建筑)、thing(物品)",
                    "[distanceDistribution]返回各距离值的格子数量统计",
                    // === 使用场景 ===
                    "[新基地选址]找distanceDistribution中3+值多的区域",
                    "[室内规划]检查safeAreas找到能放工作台的开阔位置",
                    "[走廊规划]距离2-3的区域适合做走廊，避免浪费宽阔空间"
                },
                Category = "map"
            },
            ["get_voronoi_map"] = new CommandDefinition
            {
                Name = "get_voronoi_map",
                Description = "Voronoi骨架地图 - 提取地图主干道和区域划分，规划基地布局",
                Hints = new[]
                {
                    // === 核心用途 ===
                    "[核心用途]提取地图骨架结构，规划主走廊位置和功能分区",
                    "[选址第二步]了解地形后，用此命令规划基地主干道",
                    // === 概念解释 ===
                    "[骨架概念]Voronoi骨架是离所有障碍物最远的点连成的线，即最佳主走廊",
                    "[节点]nodes是ESDF局部最大值点，骨架的关键交叉点，最安全的位置",
                    "[边]edges连接节点形成骨架网络，沿边建造走廊最安全",
                    // === 返回数据 ===
                    "[nodes]骨架节点列表，每个节点含坐标、ESDF距离、8方向障碍物信息",
                    "[edges]骨架边列表，连接两个节点，表示可通行的安全路径",
                    "[regions]区域划分，骨架将地图分成多个区域，每个区域含房间信息",
                    // === 使用场景 ===
                    "[主走廊]沿edges放置地板，形成基地主通道",
                    "[功能分区]不同regions可规划为不同功能区(卧室区/工作区/储存区)",
                    "[节点利用]nodes位置适合放重要建筑(如研究台、灶台)"
                },
                Category = "map"
            },
            ["find_build_locations"] = new CommandDefinition
            {
                Name = "find_build_locations",
                Description = "选址工具 - 找到适合建造的具体位置坐标",
                OptionalParams = new[] { "buildingDef", "minDistance", "preferIndoor", "limit" },
                Hints = new[]
                {
                    // === 核心用途 ===
                    "[核心用途]基于ESDF安全距离，返回N个最佳建造位置的坐标",
                    "[选址第三步]规划好布局后，用此命令找具体建造点",
                    // === 参数说明 ===
                    "[minDistance]离障碍物最小距离，默认1。工作台/床用1，走廊用2，广场用3",
                    "[preferIndoor]优先室内，默认true。室内建筑(床/工作台)保持true，室外建筑(太阳能)设false",
                    "[limit]返回位置数量，默认10。需要更多选项时增大",
                    // === 返回数据 ===
                    "[locations]位置列表，每个位置含x/z坐标、ESDF距离、室内/室外标记",
                    "[nearestObstacle]每个位置最近的障碍物信息(类型/名称/坐标)",
                    // === 使用场景 ===
                    "[房间中心]返回的坐标直接用作get_room_boundary的center_x/center_z",
                    "[工作台位置]minDistance=1,preferIndoor=true，找靠墙但不贴墙的位置",
                    "[室外建筑]preferIndoor=false，找室外开阔位置放太阳能/风车",
                    // === 工作流 ===
                    "[工作流]find_build_locations → 返回坐标 → get_room_boundary(坐标,宽,高,build=true)"
                },
                Category = "map"
            },
            ["get_room_boundary"] = new CommandDefinition
            {
                Name = "get_room_boundary",
                Description = "房间建造核心命令 - 获取边界状态或自动建造房间",
                RequiredParams = new[] { "center_x", "center_z", "width", "height" },
                OptionalParams = new[] { "build", "wallDef", "stuffDef" },
                Hints = new[]
                {
                    // === 核心用途 ===
                    "[核心用途]建造房间的唯一入口命令，自动计算边界位置，避免漏墙",
                    "[选址最后一步]find_build_locations返回坐标 → 此命令建造房间",
                    // === 参数说明 ===
                    "[center_x/center_z]房间中心坐标，从find_build_locations获取",
                    "[width/height]房间内部尺寸(不含墙)，最小5x5，太小无法使用！",
                    "[build]true=自动放置蓝图建造，false=只返回边界状态",
                    "[stuffDef]材料: WoodLog(木材)、Steel(钢铁)、Plasteel(塑钢)",
                    // === 返回数据 ===
                    "[completionPercent]完成百分比，100%表示房间已建成",
                    "[missingCells]需要建造的位置列表，build=true时自动处理",
                    "[edgeCells]所有边界格子的状态: empty/built/blueprint/frame",
                    // === 房间尺寸指南 ===
                    "[卧室]5x5单人间，7x7双人间",
                    "[餐厅]7x7起步，9x9更舒适",
                    "[厨房]5x5起步，放灶台+备料台",
                    "[工作室]7x7起步，放多个工作台",
                    "[冷库]7x7起步，储存大量食物",
                    "[走廊]宽度3格，方便双向通行"
                },
                Category = "map"
            },
            ["scan_area"] = new CommandDefinition
            {
                Name = "scan_area",
                Description = "区域扫描 - 一次性获取矩形区域所有格子状态",
                RequiredParams = new[] { "center_x", "center_z", "width", "height" },
                Hints = new[]
                {
                    // === 核心用途 ===
                    "[核心用途]批量扫描区域，一次返回所有格子状态，避免多次API调用",
                    "[选址辅助]建造前扫描目标区域，确认没有障碍物",
                    // === 参数说明 ===
                    "[center_x/center_z]扫描区域中心坐标",
                    "[width/height]扫描范围，建议不超过20x20(400格)",
                    // === 返回数据 ===
                    "[cells]所有格子状态列表，含坐标、地形、是否可通行",
                    "[buildings]区域内的建筑列表",
                    "[blueprints]区域内的蓝图列表",
                    "[items]区域内的物品列表",
                    "[passableCount/impassableCount]可通行/不可通行格子统计",
                    // === 使用场景 ===
                    "[建造前检查]扫描后检查impassableCount，>0说明有障碍",
                    "[房间规划]扫描计划位置，确认空间足够"
                },
                Category = "map"
            },
            ["get_river"] = new CommandDefinition
            {
                Name = "get_river",
                Description = "河流信息 - 识别河流位置和走向，规划时避开或利用",
                RequiredParams = new string[] { },
                Hints = new[]
                {
                    // === 核心用途 ===
                    "[核心用途]识别地图上的河流，帮助避开水源或利用浅滩",
                    "[选址参考]河流边适合种田(土壤湿润)，但不能在河上建墙",
                    // === 返回数据 ===
                    "[hasRiver]是否有河流，true/false",
                    "[direction]河流走向: north-south(南北) 或 east-west(东西)",
                    "[segments]河段列表，每个河段含边界、中心、格子数",
                    "[totalFordCells]浅滩数量，浅滩是可以步行穿越河流的点",
                    // === 使用场景 ===
                    "[避开河流]规划房间时检查坐标是否在河流segments内",
                    "[利用浅滩]过河路径规划时优先选择浅滩位置",
                    "[农田规划]河流附近的土壤通常更肥沃，适合放种植区"
                },
                Category = "map"
            },
            ["get_marsh"] = new CommandDefinition
            {
                Name = "get_marsh",
                Description = "沼泽信息 - 识别沼泽位置，规划时避开",
                RequiredParams = new string[] { },
                Hints = new[]
                {
                    // === 核心用途 ===
                    "[核心用途]识别沼泽区域，沼泽移动慢且不能建墙",
                    "[选址参考]选址时避开沼泽，除非故意用作防御屏障",
                    // === 返回数据 ===
                    "[hasMarsh]是否有沼泽，true/false",
                    "[segments]沼泽区域列表，每个区域含边界和格子数",
                    "[totalMarshCells]沼泽总格数",
                    // === 使用场景 ===
                    "[避开沼泽]get_room_boundary前检查坐标是否在沼泽segments内",
                    "[防御利用]沼泽会减慢敌人移动，可在沼泽边缘布置防御"
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
