using System.Collections.Generic;
using System.Text;

namespace RimWorldAI.Core.MCP
{
    /// <summary>
    /// MCP Tool 定义生成器 - 从 CommandRegistry 生成 MCP Tool 定义
    /// </summary>
    public static class MCPToolGenerator
    {
        /// <summary>
        /// 生成所有 MCP Tool 定义
        /// </summary>
        public static List<ToolDefinition> GenerateTools()
        {
            var tools = new List<ToolDefinition>();

            foreach (var kvp in CommandRegistry.Commands)
            {
                var cmd = kvp.Value;
                tools.Add(CreateToolFromCommand(cmd));
            }

            return tools;
        }

        /// <summary>
        /// 从命令定义创建 MCP Tool
        /// </summary>
        private static ToolDefinition CreateToolFromCommand(CommandDefinition cmd)
        {
            return new ToolDefinition
            {
                name = cmd.Name,
                description = BuildDescription(cmd),
                inputSchema = GenerateInputSchema(cmd)
            };
        }

        /// <summary>
        /// 构建完整的工具描述（包含游戏背景知识和操作提示）
        /// </summary>
        private static string BuildDescription(CommandDefinition cmd)
        {
            var sb = new StringBuilder();

            // 基础描述
            sb.Append(cmd.Description ?? $"Execute {cmd.Name} action");

            // 添加游戏背景知识和操作提示
            if (cmd.Hints != null && cmd.Hints.Length > 0)
            {
                sb.Append("\n\n[游戏知识与提示]\n");
                foreach (var hint in cmd.Hints)
                {
                    sb.Append("• ").Append(hint).Append("\n");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成输入参数 Schema (JSON Schema 格式)
        /// </summary>
        private static Dictionary<string, object> GenerateInputSchema(CommandDefinition cmd)
        {
            var properties = new Dictionary<string, object>();
            var required = new List<string>();

            // 添加必需参数
            if (cmd.RequiredParams != null)
            {
                foreach (var param in cmd.RequiredParams)
                {
                    properties[param] = InferParamSchema(param);
                    required.Add(param);
                }
            }

            // 添加可选参数
            if (cmd.OptionalParams != null)
            {
                foreach (var param in cmd.OptionalParams)
                {
                    properties[param] = InferParamSchema(param);
                }
            }

            var schema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = properties
            };

            if (required.Count > 0)
            {
                schema["required"] = required;
            }

            return schema;
        }

        /// <summary>
        /// 根据参数名推断参数类型和详细描述
        /// </summary>
        private static Dictionary<string, object> InferParamSchema(string paramName)
        {
            string lowerName = paramName.ToLower();

            // ID 类型参数 (整数)
            if (lowerName.EndsWith("id") || lowerName == "id")
            {
                string desc = GetIdDescription(paramName);
                return new Dictionary<string, object>
                {
                    ["type"] = "integer",
                    ["description"] = desc
                };
            }

            // 坐标参数 (整数)
            if (lowerName == "x" || lowerName == "z")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "integer",
                    ["description"] = $"地图坐标 {paramName.ToUpper()} (RimWorld地图坐标系，范围约0-250)"
                };
            }

            // Cells 参数 (JSON 字符串)
            if (lowerName == "cells")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "坐标数组JSON字符串，格式：[{\"x\":100,\"z\":100},{\"x\":101,\"z\":101}]。用于定义区域范围，每个对象包含x和z坐标"
                };
            }

            // 工作类型
            if (lowerName == "worktype")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "工作类型名称。可选值：Firefighter(消防)、Patient(就医)、Doctor(医疗)、BasicWorker(基础)、Warden(狱卒)、Handling(驯兽)、Cooking(烹饪)、Hunting(狩猎)、Construction(建造)、Repair(维修)、Growing(种植)、Mining(采矿)、PlantCutting(砍伐)、Smithing(锻造)、Tailoring(裁缝)、Art(艺术)、Hauling(搬运)、Cleaning(清洁)、Research(研究)"
                };
            }

            // 工作优先级 (整数0-4)
            if (lowerName.Contains("priority") && !lowerName.Contains("storage"))
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "integer",
                    ["description"] = "工作优先级(0-4)：0=不从事该工作，1=低优先级，2=普通优先级，3=优先执行，4=重要/最优先。建议根据殖民者技能设置"
                };
            }

            // 储存优先级 (字符串枚举)
            if (lowerName == "priority")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["enum"] = new[] { "Low", "Normal", "Preferred", "Important", "Critical" },
                    ["description"] = "储存区优先级。Critical(关键-食物/药品)、Important(重要)、Preferred(优先)、Normal(普通)、Low(低)。殖民者优先填充高优先级储存区"
                };
            }

            // 区域类型
            if (lowerName == "type" && paramName == "type")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["enum"] = new[] { "stockpile", "growing" },
                    ["description"] = "区域类型：'stockpile'创建储存区(殖民者自动搬运物品)，'growing'创建种植区(殖民者自动种植/收获作物)"
                };
            }

            // 旋转方向
            if (lowerName == "rotation")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["enum"] = new[] { "North", "East", "South", "West" },
                    ["description"] = "建筑旋转方向。North(北/默认)、East(东)、South(南)、West(西)。影响建筑朝向和占用空间"
                };
            }

            // 布尔类型参数
            if (lowerName == "all" || lowerName == "detailed" || lowerName == "summary" || lowerName == "allow")
            {
                string boolDesc = GetBoolDescription(paramName);
                return new Dictionary<string, object>
                {
                    ["type"] = "boolean",
                    ["description"] = boolDesc
                };
            }

            // 模式参数
            if (lowerName == "mode")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "过滤模式：'allowAll'(允许所有物品)、'disallowAll'(禁止所有物品)。配合categories/defs和allow参数精确控制"
                };
            }

            // 类别参数
            if (lowerName == "category" || lowerName == "categories")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "物品类别名称。常用类别：Foods(食物)、Weapons(武器)、Apparel(衣物)、Resources(资源)、Medicine(药品)、Materials(材料)。使用get_thing_categories查看完整类别树"
                };
            }

            // Def 名称参数
            if (lowerName.Contains("defname") || lowerName.Contains("stuff"))
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = $"{FormatDescription(paramName)}。defName是游戏中物品/建筑/植物的唯一标识符。常用值：Steel(钢铁)、WoodLog(木材)、Bed(床)、TableStove(灶台)。使用get_buildable_defs/get_item_by_def查询有效值"
                };
            }

            // def参数 (单独处理)
            if (lowerName == "defs")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "物品defName数组字符串，如'[\"Steel\",\"WoodLog\"]'。用于精确指定允许/禁止的物品"
                };
            }

            // 预设名称
            if (lowerName == "presetname")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["enum"] = new[] { "food", "meals", "raw_food", "materials", "weapons", "apparel", "medicine", "items", "corpses", "chunks" },
                    ["description"] = "储存预设名称，快速配置储存区过滤规则：food(食物)、meals(餐食)、raw_food(生食)、materials(材料)、weapons(武器)、apparel(衣物)、medicine(药品)、items(物品)、corpses(尸体)、chunks(石块)"
                };
            }

            // 父类别
            if (lowerName == "parentcategory")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "父类别名称，用于查询嵌套类别的子项。如传入'Foods'可查看食物下的子类别"
                };
            }

            // plantDefName 特殊处理
            if (lowerName == "plantdefname")
            {
                return new Dictionary<string, object>
                {
                    ["type"] = "string",
                    ["description"] = "作物defName。常用值：Plant_Rice(水稻-生长快)、Plant_Potato(土豆-适应强)、Plant_Corn(玉米-产量高)、Plant_Cotton(棉花)、Plant_Healroot(药草)、Plant_Devilstrand(魔鬼丝)"
                };
            }

            // 默认字符串类型
            return new Dictionary<string, object>
            {
                ["type"] = "string",
                ["description"] = FormatDescription(paramName)
            };
        }

        /// <summary>
        /// 获取ID类型参数的详细描述
        /// </summary>
        private static string GetIdDescription(string paramName)
        {
            string lowerName = paramName.ToLower();

            if (lowerName == "pawnid")
                return "角色唯一标识符(整数)。从get_colonists/get_pawns/get_prisoners等命令获取。用于指定殖民者、囚犯或动物";
            if (lowerName == "thingid")
                return "物品唯一标识符(整数)。从get_food/get_weapons/get_materials等命令获取。用于指定地上的物品";
            if (lowerName == "zoneid")
                return "区域唯一标识符(整数)。从get_zones命令获取。用于指定储存区或种植区";
            if (lowerName == "blueprintid")
                return "蓝图唯一标识符(整数)。从get_blueprints命令获取。用于指定待建造的建筑蓝图";
            if (lowerName == "targetid")
                return "目标唯一标识符(整数)。从get_enemies/get_animals命令获取。用于指定攻击目标";

            return $"{FormatDescription(paramName)} (整数ID，从相应查询命令获取)";
        }

        /// <summary>
        /// 获取布尔类型参数的详细描述
        /// </summary>
        private static string GetBoolDescription(string paramName)
        {
            string lowerName = paramName.ToLower();

            if (lowerName == "all")
                return "是否应用于所有对象。true=应用于所有(如解禁所有物品)，false=仅应用于指定对象";
            if (lowerName == "detailed")
                return "是否返回详细信息。true=返回完整数据包括格子列表，false=返回简要信息";
            if (lowerName == "summary")
                return "是否返回汇总信息。true=返回统计汇总，false=返回详细列表";
            if (lowerName == "allow")
                return "允许或禁止。true=允许该类别/物品进入储存区，false=禁止该类别/物品";

            return FormatDescription(paramName);
        }

        /// <summary>
        /// 将 camelCase 参数名转换为可读描述
        /// </summary>
        private static string FormatDescription(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
                return "";

            var result = new StringBuilder();
            result.Append(char.ToUpper(paramName[0]));

            for (int i = 1; i < paramName.Length; i++)
            {
                char c = paramName[i];
                if (char.IsUpper(c))
                {
                    result.Append(' ');
                    result.Append(c);
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}
