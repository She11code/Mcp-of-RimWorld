using System.Collections.Generic;
using System.Threading;

namespace RimWorldAI.Core.MCP
{
    /// <summary>
    /// MCP Tool 执行器 - 将 MCP Tool 调用转换为现有命令格式并执行
    /// </summary>
    public class MCPToolExecutor
    {
        /// <summary>
        /// 执行 MCP Tool 调用
        /// </summary>
        public CallToolResult Execute(string toolName, Dictionary<string, object> arguments)
        {
            // 1. 构造现有格式的命令
            var command = new Dictionary<string, object>
            {
                ["action"] = toolName
            };

            // 2. 合并参数
            if (arguments != null)
            {
                foreach (var arg in arguments)
                {
                    command[arg.Key] = arg.Value;
                }
            }

            // 3. 序列化命令
            string jsonCommand = SimpleJson.SerializeObject(command);

            // 4. 同步执行 (通过 CommandQueue 在主线程执行)
            string response = ExecuteSynchronously(jsonCommand);

            // 5. 转换为 MCP 结果格式
            return ConvertToMCPResult(response);
        }

        /// <summary>
        /// 同步执行命令 (跨线程)
        /// </summary>
        private string ExecuteSynchronously(string jsonCommand)
        {
            string result = null;
            var resetEvent = new ManualResetEvent(false);

            CommandQueue.Enqueue(jsonCommand, response =>
            {
                result = response;
                resetEvent.Set();
            });

            // 等待执行完成 (最多 10 秒)
            if (!resetEvent.WaitOne(10000))
            {
                return SimpleJson.SerializeObject(new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "Timeout waiting for game thread (10s)"
                });
            }

            // 检查结果是否为空
            if (string.IsNullOrEmpty(result))
            {
                return SimpleJson.SerializeObject(new Dictionary<string, object>
                {
                    ["success"] = false,
                    ["error"] = "Empty response from game thread - command may not have been processed"
                });
            }

            return result;
        }

        /// <summary>
        /// 将现有 JSON 响应转换为 MCP CallToolResult 格式
        /// </summary>
        private CallToolResult ConvertToMCPResult(string jsonResponse)
        {
            bool isError = false;

            try
            {
                var parsed = SimpleJson.DeserializeObject(jsonResponse);

                // 检查 parsed 是否为 null（JSON 解析失败）
                if (parsed == null)
                {
                    isError = true;
                }
                else if (parsed.ContainsKey("error"))
                {
                    isError = true;
                }
                else if (parsed.ContainsKey("success"))
                {
                    var successValue = parsed["success"];
                    if (successValue is bool b && !b)
                    {
                        isError = true;
                    }
                    else if (successValue != null && successValue.ToString() == "False")
                    {
                        isError = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                // JSON 解析异常
                Verse.Log.Error($"[RimWorldAI] JSON parse error: {ex.Message}");
                isError = true;
            }

            return new CallToolResult
            {
                content = new List<ContentItem>
                {
                    new ContentItem
                    {
                        type = "text",
                        text = jsonResponse
                    }
                },
                isError = isError
            };
        }
    }
}
