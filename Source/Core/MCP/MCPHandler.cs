using System;
using System.Collections.Generic;

namespace RimWorldAI.Core.MCP
{
    /// <summary>
    /// MCP 协议处理器 - 处理 JSON-RPC 2.0 请求
    /// </summary>
    public class MCPHandler
    {
        private readonly MCPToolExecutor _toolExecutor;

        public MCPHandler()
        {
            _toolExecutor = new MCPToolExecutor();
        }

        /// <summary>
        /// 处理 JSON-RPC 请求
        /// </summary>
        /// <param name="request">JSON-RPC 请求</param>
        /// <param name="session">当前会话（可为 null，仅用于 ping 和 initialize）</param>
        public JsonRpcResponse HandleRequest(JsonRpcRequest request, MCPSession session = null)
        {
            var response = new JsonRpcResponse
            {
                jsonrpc = "2.0",
                id = request.id
            };

            try
            {
                switch (request.method)
                {
                    case "initialize":
                        response.result = HandleInitialize(request, session);
                        break;
                    case "notifications/initialized":
                        response.result = HandleInitializedNotification(request, session);
                        break;
                    case "tools/list":
                        // 检查会话是否已初始化
                        if (session == null || !session.Initialized)
                        {
                            throw new JsonRpcException(JsonRpcErrorCodes.ServerNotInitialized, "Server not initialized. Call 'initialize' first.");
                        }
                        response.result = HandleToolsList(request);
                        break;
                    case "tools/call":
                        // 检查会话是否已初始化
                        if (session == null || !session.Initialized)
                        {
                            throw new JsonRpcException(JsonRpcErrorCodes.ServerNotInitialized, "Server not initialized. Call 'initialize' first.");
                        }
                        response.result = HandleToolsCall(request);
                        break;
                    case "ping":
                        response.result = HandlePing(request);
                        break;
                    default:
                        throw new JsonRpcException(JsonRpcErrorCodes.MethodNotFound, $"Method not found: {request.method}");
                }
            }
            catch (JsonRpcException ex)
            {
                response.result = null; // 清除 result，确保只有 error
                response.error = new JsonRpcError
                {
                    code = ex.Code,
                    message = ex.Message,
                    data = ex.Data
                };
            }
            catch (Exception ex)
            {
                Verse.Log.Error($"[RimWorldAI] MCP handler error: {ex.Message}\n{ex.StackTrace}");
                response.result = null; // 清除 result，确保只有 error
                response.error = new JsonRpcError
                {
                    code = JsonRpcErrorCodes.InternalError,
                    message = "Internal error",
                    data = ex.Message
                };
            }

            return response;
        }

        /// <summary>
        /// 处理 initialize 请求
        /// </summary>
        private InitializeResult HandleInitialize(JsonRpcRequest request, MCPSession session)
        {
            // 标记会话为已初始化（注意：客户端还需要发送 notifications/initialized）
            if (session != null)
            {
                session.Initialized = true;
            }

            Verse.Log.Message($"[RimWorldAI] MCP client initialized, session: {session?.SessionId ?? "null"}");

            return new InitializeResult
            {
                protocolVersion = "2025-03-26",
                capabilities = new ServerCapabilities
                {
                    tools = new ToolCapabilities { listChanged = false }
                },
                serverInfo = new Implementation
                {
                    name = "RimWorldAI",
                    version = "1.0.0"
                },
                instructions = "RimWorld AI Control Server - Control your colony through AI tools. Use tools/list to see available commands and tools/call to execute them."
            };
        }

        /// <summary>
        /// 处理 initialized 通知
        /// </summary>
        private object HandleInitializedNotification(JsonRpcRequest request, MCPSession session)
        {
            // 这是一个通知，不需要返回结果
            // 客户端表示已准备好开始正常操作
            Verse.Log.Message($"[RimWorldAI] MCP client sent initialized notification, session: {session?.SessionId ?? "null"}");
            return null;
        }

        /// <summary>
        /// 处理 tools/list 请求
        /// </summary>
        private object HandleToolsList(JsonRpcRequest request)
        {
            // 返回匿名对象，不包含 nextCursor 字段（如果没有更多数据）
            return new
            {
                tools = MCPToolGenerator.GenerateTools()
            };
        }

        /// <summary>
        /// 处理 tools/call 请求
        /// </summary>
        private CallToolResult HandleToolsCall(JsonRpcRequest request)
        {
            // 解析参数
            var @params = request.@params;
            if (@params == null)
            {
                throw new JsonRpcException(JsonRpcErrorCodes.InvalidParams, "Missing params for tools/call");
            }

            // 获取工具名称
            if (!@params.TryGetValue("name", out var nameObj) || nameObj == null)
            {
                throw new JsonRpcException(JsonRpcErrorCodes.InvalidParams, "Missing 'name' parameter");
            }

            string toolName = nameObj.ToString();

            // 获取参数
            Dictionary<string, object> arguments = null;
            if (@params.TryGetValue("arguments", out var argsObj) && argsObj is Dictionary<string, object> args)
            {
                arguments = args;
            }

            // 验证工具是否存在
            if (!CommandRegistry.IsValidCommand(toolName))
            {
                throw new JsonRpcException(JsonRpcErrorCodes.InvalidParams, $"Unknown tool: {toolName}");
            }

            // 执行工具
            return _toolExecutor.Execute(toolName, arguments);
        }

        /// <summary>
        /// 处理 ping 请求
        /// </summary>
        private object HandlePing(JsonRpcRequest request)
        {
            return new Dictionary<string, object>
            {
                ["status"] = "ok",
                ["timestamp"] = DateTime.UtcNow.ToString("o")
            };
        }
    }
}
