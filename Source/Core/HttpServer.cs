using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using RimWorldAI.Core.MCP;

namespace RimWorldAI.Core
{
    /// <summary>
    /// HTTP 服务器，支持 MCP Streamable HTTP 协议和原有 JSON API
    /// </summary>
    public class HttpServer : IDisposable
    {
        private static HttpServer _instance;
        public static HttpServer Instance => _instance ?? (_instance = new HttpServer());

        private HttpListener _listener;
        private readonly int _port;
        private bool _isRunning;
        private Thread _listenerThread;
        private readonly MCPHandler _mcpHandler;

        // 会话管理
        private readonly ConcurrentDictionary<string, MCPSession> _sessions = new ConcurrentDictionary<string, MCPSession>();

        public bool IsRunning => _isRunning;
        public int Port => _port;

        private HttpServer(int port = 8080)
        {
            _port = port;
            _mcpHandler = new MCPHandler();
        }

        /// <summary>
        /// 启动 HTTP 服务器
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{_port}/");
                _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
                _listener.Start();

                _isRunning = true;

                _listenerThread = new Thread(ListenLoop) { IsBackground = true };
                _listenerThread.Start();

                Verse.Log.Message($"[RimWorldAI] HTTP server started on port {_port}");
                Verse.Log.Message($"[RimWorldAI] MCP endpoint: http://localhost:{_port}/mcp");
                Verse.Log.Message($"[RimWorldAI] Legacy API: http://localhost:{_port}/api");
            }
            catch (Exception ex)
            {
                Verse.Log.Error($"[RimWorldAI] Failed to start HTTP server: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止 HTTP 服务器
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;

            // 清理所有会话
            foreach (var session in _sessions.Values)
            {
                session.CancellationTokenSource?.Cancel();
            }
            _sessions.Clear();

            _listener?.Stop();
            _listener?.Close();

            Verse.Log.Message("[RimWorldAI] HTTP server stopped");
        }

        /// <summary>
        /// 监听循环
        /// </summary>
        private void ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                }
                catch (HttpListenerException)
                {
                    // 服务器停止时会抛出异常，忽略
                    break;
                }
                catch (Exception ex)
                {
                    Verse.Log.Error($"[RimWorldAI] HTTP listener error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 处理 HTTP 请求
        /// </summary>
        private void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                // 设置 CORS 头 - 暴露 MCP 相关 headers
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept, Mcp-Session-Id, Last-Event-Id, Mcp-Protocol-Version");
                response.Headers.Add("Access-Control-Expose-Headers", "Mcp-Session-Id, Last-Event-Id, Mcp-Protocol-Version");

                // 处理 OPTIONS 预检请求
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 204;
                    response.Close();
                    return;
                }

                // 路由请求
                string path = request.Url.AbsolutePath.ToLower();

                switch (path)
                {
                    case "/mcp":
                        HandleMCPRequest(context);
                        break;
                    case "/api":
                        HandleLegacyAPIRequest(context);
                        break;
                    case "/health":
                        HandleHealthCheck(context);
                        break;
                    default:
                        SendError(response, 404, "Not Found");
                        break;
                }
            }
            catch (Exception ex)
            {
                Verse.Log.Error($"[RimWorldAI] Request handling error: {ex.Message}");
                SendError(response, 500, "Internal Server Error");
            }
        }

        /// <summary>
        /// 处理 MCP Streamable HTTP 请求
        /// </summary>
        private void HandleMCPRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // 获取 session ID
            string sessionId = request.Headers["Mcp-Session-Id"];

            switch (request.HttpMethod)
            {
                case "POST":
                    HandleMCPPost(context, sessionId);
                    break;
                case "GET":
                    HandleMCPGet(context, sessionId);
                    break;
                case "DELETE":
                    HandleMCPDelete(context, sessionId);
                    break;
                default:
                    response.StatusCode = 405;
                    response.Headers.Add("Allow", "GET, POST, DELETE");
                    SendJsonResponse(response, new Dictionary<string, object>
                    {
                        ["jsonrpc"] = "2.0",
                        ["error"] = new Dictionary<string, object>
                        {
                            ["code"] = -32000,
                            ["message"] = "Method not allowed"
                        }
                    });
                    break;
            }
        }

        /// <summary>
        /// 处理 MCP POST 请求
        /// </summary>
        private void HandleMCPPost(HttpListenerContext context, string sessionId)
        {
            var request = context.Request;
            var response = context.Response;

            response.ContentType = "application/json";

            // 添加 MCP 协议版本 header (帮助客户端识别服务器版本)
            response.Headers.Add("Mcp-Protocol-Version", "2025-03-26");

            MCPSession currentSession = null;

            try
            {
                // 读取请求体
                string body;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    body = reader.ReadToEnd();
                }

                // 检查是否为 batch 请求（数组形式）
                body = body.Trim();
                if (body.StartsWith("["))
                {
                    // Batch 请求处理
                    HandleBatchRequest(context, body, sessionId);
                    return;
                }

                // 单个请求处理
                // 解析 JSON-RPC 请求
                var rpcRequest = ParseJsonRpcRequest(body);

                // 调试日志
                Verse.Log.Message($"[RimWorldAI] MCP Request: method={rpcRequest.method}, id={rpcRequest.id ?? "null"}, sessionId={sessionId ?? "null"}");

                // 如果是 initialize 请求，创建新会话
                if (rpcRequest.method == "initialize")
                {
                    sessionId = Guid.NewGuid().ToString("N");
                    currentSession = new MCPSession
                    {
                        SessionId = sessionId,
                        CreatedAt = DateTime.UtcNow,
                        CancellationTokenSource = new CancellationTokenSource(),
                        Initialized = false
                    };
                    _sessions[sessionId] = currentSession;

                    // 返回 session ID header
                    response.Headers.Add("Mcp-Session-Id", sessionId);

                    Verse.Log.Message($"[RimWorldAI] MCP session created: {sessionId}");
                }
                else if (!string.IsNullOrEmpty(sessionId) && _sessions.TryGetValue(sessionId, out currentSession))
                {
                    // 已有会话，返回 session ID header
                    response.Headers.Add("Mcp-Session-Id", sessionId);
                }
                else if (string.IsNullOrEmpty(sessionId))
                {
                    // 无 session ID 的请求处理
                    // 根据 MCP 规范，只有 initialize 和 ping 可以在没有 session 的情况下调用
                    if (rpcRequest.method != "ping")
                    {
                        Verse.Log.Warning($"[RimWorldAI] MCP request without session ID: {rpcRequest.method}");

                        // 对于需要 session 的请求，返回错误
                        if (!string.IsNullOrEmpty(rpcRequest.id))
                        {
                            SendJsonResponse(response, new Dictionary<string, object>
                            {
                                ["jsonrpc"] = "2.0",
                                ["id"] = rpcRequest.id,
                                ["error"] = new Dictionary<string, object>
                                {
                                    ["code"] = -32000,
                                    ["message"] = "Missing Mcp-Session-Id header. Call 'initialize' first to create a session."
                                }
                            });
                        }
                        else
                        {
                            response.StatusCode = 400;
                            response.Close();
                        }
                        return;
                    }
                    Verse.Log.Message($"[RimWorldAI] MCP stateless ping request");
                }
                else
                {
                    // 无效的 session ID
                    Verse.Log.Warning($"[RimWorldAI] MCP request with invalid session ID: {sessionId}");

                    if (!string.IsNullOrEmpty(rpcRequest.id))
                    {
                        response.StatusCode = 404;
                        SendJsonResponse(response, new Dictionary<string, object>
                        {
                            ["jsonrpc"] = "2.0",
                            ["id"] = rpcRequest.id,
                            ["error"] = new Dictionary<string, object>
                            {
                                ["code"] = -32001,
                                ["message"] = "Session not found. The session may have expired."
                            }
                        });
                    }
                    else
                    {
                        response.StatusCode = 404;
                        response.Close();
                    }
                    return;
                }

                // 处理请求，传入当前会话
                var rpcResponse = _mcpHandler.HandleRequest(rpcRequest, currentSession);

                // 如果是通知 (id 为空)，不返回响应
                // 根据 MCP 规范，对于 notifications/responses 应返回 202 Accepted
                if (string.IsNullOrEmpty(rpcRequest.id))
                {
                    response.StatusCode = 202; // 202 Accepted (MCP 规范要求)
                    response.Close();
                    return;
                }

                // 返回响应
                SendJsonResponse(response, rpcResponse);
            }
            catch (Exception ex)
            {
                Verse.Log.Error($"[RimWorldAI] MCP POST request error: {ex.Message}\n{ex.StackTrace}");
                SendJsonResponse(response, new Dictionary<string, object>
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = (string)null,
                    ["error"] = new Dictionary<string, object>
                    {
                        ["code"] = -32700,
                        ["message"] = "Parse error",
                        ["data"] = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// 处理 MCP Batch 请求（JSON-RPC 数组）
        /// </summary>
        private void HandleBatchRequest(HttpListenerContext context, string body, string sessionId)
        {
            var response = context.Response;
            response.ContentType = "application/json";
            response.Headers.Add("Mcp-Protocol-Version", "2025-03-26");

            try
            {
                // 解析 JSON 数组
                var requestList = SimpleJson.DeserializeArray(body);
                if (requestList == null || requestList.Count == 0)
                {
                    var errorResponse = new JsonRpcResponse
                    {
                        jsonrpc = "2.0",
                        id = null,
                        error = new JsonRpcError
                        {
                            code = -32600,
                            message = "Invalid Request: Expected non-empty array"
                        }
                    };
                    SendJsonResponse(response, errorResponse);
                    return;
                }

                Verse.Log.Message($"[RimWorldAI] MCP Batch request: {requestList.Count} items, sessionId={sessionId ?? "null"}");

                var responses = new List<object>();
                bool hasRequests = false;
                MCPSession currentSession = null;

                // 获取或创建会话
                if (!string.IsNullOrEmpty(sessionId) && _sessions.TryGetValue(sessionId, out currentSession))
                {
                    response.Headers.Add("Mcp-Session-Id", sessionId);
                }

                foreach (var requestDict in requestList)
                {
                    var rpcRequest = new JsonRpcRequest();
                    if (requestDict.TryGetValue("jsonrpc", out var jsonrpc))
                        rpcRequest.jsonrpc = jsonrpc?.ToString();
                    if (requestDict.TryGetValue("id", out var id))
                        rpcRequest.id = id?.ToString();
                    if (requestDict.TryGetValue("method", out var method))
                        rpcRequest.method = method?.ToString();
                    if (requestDict.TryGetValue("params", out var @params) && @params is Dictionary<string, object> paramsDict)
                        rpcRequest.@params = paramsDict;

                    // 检查是否为 initialize
                    if (rpcRequest.method == "initialize")
                    {
                        sessionId = Guid.NewGuid().ToString("N");
                        currentSession = new MCPSession
                        {
                            SessionId = sessionId,
                            CreatedAt = DateTime.UtcNow,
                            CancellationTokenSource = new CancellationTokenSource(),
                            Initialized = false
                        };
                        _sessions[sessionId] = currentSession;
                        response.Headers.Add("Mcp-Session-Id", sessionId);
                        Verse.Log.Message($"[RimWorldAI] MCP session created in batch: {sessionId}");
                    }

                    // 如果有 id，则为请求（需要响应）
                    if (!string.IsNullOrEmpty(rpcRequest.id))
                    {
                        hasRequests = true;
                        var rpcResponse = _mcpHandler.HandleRequest(rpcRequest, currentSession);
                        responses.Add(rpcResponse);
                    }
                    // 如果没有 id，则为通知（不需要响应）
                }

                // 如果只有通知/响应，返回 202 Accepted
                if (!hasRequests)
                {
                    response.StatusCode = 202;
                    response.Close();
                    return;
                }

                // 返回 batch 响应
                SendBatchJsonResponse(response, responses);
            }
            catch (Exception ex)
            {
                Verse.Log.Error($"[RimWorldAI] MCP Batch request error: {ex.Message}\n{ex.StackTrace}");
                var errorResponse = new JsonRpcResponse
                {
                    jsonrpc = "2.0",
                    id = null,
                    error = new JsonRpcError
                    {
                        code = -32700,
                        message = "Parse error",
                        data = ex.Message
                    }
                };
                SendJsonResponse(response, errorResponse);
            }
        }

        /// <summary>
        /// 发送 batch JSON 响应
        /// </summary>
        private void SendBatchJsonResponse(HttpListenerResponse response, List<object> responses)
        {
            var jsonParts = new List<string>();
            foreach (var resp in responses)
            {
                if (resp is JsonRpcResponse rpcResp)
                {
                    jsonParts.Add(SerializeJsonRpcResponse(rpcResp));
                }
                else
                {
                    jsonParts.Add(SimpleJson.SerializeObject(resp));
                }
            }

            string json = "[" + string.Join(",", jsonParts) + "]";
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        /// <summary>
        /// 处理 MCP GET 请求 (SSE 流)
        /// </summary>
        private void HandleMCPGet(HttpListenerContext context, string sessionId)
        {
            var response = context.Response;

            if (string.IsNullOrEmpty(sessionId) || !_sessions.ContainsKey(sessionId))
            {
                response.StatusCode = 400;
                response.ContentType = "application/json";
                SendJsonResponse(response, new Dictionary<string, object>
                {
                    ["jsonrpc"] = "2.0",
                    ["error"] = new Dictionary<string, object>
                    {
                        ["code"] = -32000,
                        ["message"] = "Bad Request: No valid session ID provided"
                    }
                });
                return;
            }

            // 检查 Last-Event-ID (用于重连)
            string lastEventId = context.Request.Headers["Last-Event-Id"];
            if (!string.IsNullOrEmpty(lastEventId))
            {
                Verse.Log.Message($"[RimWorldAI] MCP client reconnecting with Last-Event-ID: {lastEventId}");
            }
            else
            {
                Verse.Log.Message($"[RimWorldAI] Establishing SSE stream for session {sessionId}");
            }

            // 返回 SSE 流
            response.ContentType = "text/event-stream";
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");
            response.Headers.Add("Mcp-Session-Id", sessionId);

            try
            {
                var session = _sessions[sessionId];
                var stream = response.OutputStream;
                var writer = new StreamWriter(stream, Encoding.UTF8);

                // 发送初始事件
                SendSSEEvent(writer, "endpoint", $"{{\"session\":\"{sessionId}\"}}");

                // 保持连接，发送心跳
                while (_isRunning && !session.CancellationTokenSource.Token.IsCancellationRequested)
                {
                    Thread.Sleep(30000); // 30秒心跳间隔
                    SendSSEEvent(writer, "ping", "{}");
                }
            }
            catch (Exception ex)
            {
                Verse.Log.Error($"[RimWorldAI] SSE stream error: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理 MCP DELETE 请求 (终止会话)
        /// </summary>
        private void HandleMCPDelete(HttpListenerContext context, string sessionId)
        {
            var response = context.Response;

            if (string.IsNullOrEmpty(sessionId) || !_sessions.ContainsKey(sessionId))
            {
                response.StatusCode = 400;
                response.ContentType = "application/json";
                SendJsonResponse(response, new Dictionary<string, object>
                {
                    ["jsonrpc"] = "2.0",
                    ["error"] = new Dictionary<string, object>
                    {
                        ["code"] = -32000,
                        ["message"] = "Bad Request: No valid session ID provided"
                    }
                });
                return;
            }

            Verse.Log.Message($"[RimWorldAI] MCP session terminated: {sessionId}");

            // 移除会话
            if (_sessions.TryRemove(sessionId, out var session))
            {
                session.CancellationTokenSource?.Cancel();
            }

            response.StatusCode = 204;
            response.Close();
        }

        /// <summary>
        /// 发送 SSE 事件
        /// </summary>
        private void SendSSEEvent(StreamWriter writer, string eventType, string data)
        {
            writer.WriteLine($"event: {eventType}");
            writer.WriteLine($"data: {data}");
            writer.WriteLine();
            writer.Flush();
        }

        /// <summary>
        /// 处理原有 JSON API 请求 (向后兼容)
        /// </summary>
        private void HandleLegacyAPIRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            response.ContentType = "application/json";

            if (request.HttpMethod == "POST")
            {
                try
                {
                    string body;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        body = reader.ReadToEnd();
                    }

                    // 使用原有的 CommandQueue 处理
                    string result = ExecuteSynchronously(body);
                    SendJsonResponse(response, result);
                }
                catch (Exception ex)
                {
                    Verse.Log.Error($"[RimWorldAI] Legacy API error: {ex.Message}");
                    SendJsonResponse(response, new Dictionary<string, object>
                    {
                        ["success"] = false,
                        ["error"] = ex.Message
                    });
                }
            }
            else
            {
                response.StatusCode = 405;
                SendJsonResponse(response, new Dictionary<string, object>
                {
                    ["error"] = "Method not allowed, use POST"
                });
            }
        }

        /// <summary>
        /// 处理健康检查请求
        /// </summary>
        private void HandleHealthCheck(HttpListenerContext context)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            SendJsonResponse(response, new Dictionary<string, object>
            {
                ["status"] = "ok",
                ["server"] = "RimWorldAI",
                ["version"] = "1.0.0",
                ["mcp"] = true,
                ["mcpProtocolVersion"] = "2025-03-26",
                ["activeSessions"] = _sessions.Count,
                ["gameLoaded"] = Verse.Find.CurrentMap != null,
                ["timestamp"] = DateTime.UtcNow.ToString("o")
            });
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

            return result;
        }

        /// <summary>
        /// 解析 JSON-RPC 请求
        /// </summary>
        private JsonRpcRequest ParseJsonRpcRequest(string body)
        {
            var parsed = SimpleJson.DeserializeObject(body);

            var request = new JsonRpcRequest();

            if (parsed.TryGetValue("jsonrpc", out var jsonrpc))
            {
                request.jsonrpc = jsonrpc?.ToString();
            }

            if (parsed.TryGetValue("id", out var id))
            {
                request.id = id?.ToString();
            }

            if (parsed.TryGetValue("method", out var method))
            {
                request.method = method?.ToString();
            }

            if (parsed.TryGetValue("params", out var @params) && @params is Dictionary<string, object> paramsDict)
            {
                request.@params = paramsDict;
            }

            return request;
        }

        /// <summary>
        /// 发送 JSON 响应
        /// </summary>
        private void SendJsonResponse(HttpListenerResponse response, object data)
        {
            string json;
            if (data is string str)
            {
                json = str;
            }
            else if (data is JsonRpcResponse rpcResponse)
            {
                // JSON-RPC 2.0 规范：成功时只有 result，失败时只有 error
                // 不能同时包含 result 和 error 字段
                json = SerializeJsonRpcResponse(rpcResponse);
            }
            else
            {
                json = SimpleJson.SerializeObject(data);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        /// <summary>
        /// 序列化 JSON-RPC 响应，符合 JSON-RPC 2.0 规范
        /// 成功时不包含 error 字段，失败时不包含 result 字段
        /// </summary>
        private string SerializeJsonRpcResponse(JsonRpcResponse rpcResponse)
        {
            var parts = new List<string>();
            parts.Add($"\"jsonrpc\":\"{rpcResponse.jsonrpc}\"");

            // id 可以为 null（对于没有 id 的请求的响应）
            if (rpcResponse.id != null)
            {
                parts.Add($"\"id\":{SimpleJson.SerializeObject(rpcResponse.id)}");
            }

            // 根据是否有错误决定包含哪些字段
            if (rpcResponse.error != null)
            {
                // 有错误时，只包含 error，不包含 result
                // 手动序列化 error，跳过 null 的 data 字段
                var errorParts = new List<string>();
                errorParts.Add($"\"code\":{rpcResponse.error.code}");
                errorParts.Add($"\"message\":{SimpleJson.SerializeObject(rpcResponse.error.message)}");
                if (rpcResponse.error.data != null)
                {
                    errorParts.Add($"\"data\":{SimpleJson.SerializeObject(rpcResponse.error.data)}");
                }
                parts.Add($"\"error\":{{{string.Join(",", errorParts)}}}");
            }
            else
            {
                // 成功时，只包含 result，不包含 error
                parts.Add($"\"result\":{SimpleJson.SerializeObject(rpcResponse.result)}");
            }

            return "{" + string.Join(",", parts) + "}";
        }

        /// <summary>
        /// 发送错误响应
        /// </summary>
        private void SendError(HttpListenerResponse response, int statusCode, string message)
        {
            response.StatusCode = statusCode;
            response.ContentType = "application/json";

            SendJsonResponse(response, new Dictionary<string, object>
            {
                ["error"] = message,
                ["status"] = statusCode
            });
        }

        public void Dispose()
        {
            Stop();
        }
    }

    /// <summary>
    /// MCP 会话信息
    /// </summary>
    public class MCPSession
    {
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        /// <summary>
        /// 会话是否已完成初始化（收到 initialized 通知）
        /// </summary>
        public bool Initialized { get; set; }
    }
}
