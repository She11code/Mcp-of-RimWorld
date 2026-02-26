using System.Collections.Generic;

namespace RimWorldAI.Core.MCP
{
    #region JSON-RPC 2.0 基础类型

    /// <summary>
    /// JSON-RPC 2.0 请求
    /// </summary>
    public class JsonRpcRequest
    {
        public string jsonrpc = "2.0";
        public string id;
        public string method;
        public Dictionary<string, object> @params;
    }

    /// <summary>
    /// JSON-RPC 2.0 响应
    /// </summary>
    public class JsonRpcResponse
    {
        public string jsonrpc = "2.0";
        public string id;
        public object result;
        public JsonRpcError error;
    }

    /// <summary>
    /// JSON-RPC 错误
    /// </summary>
    public class JsonRpcError
    {
        public int code;
        public string message;
        public object data;
    }

    /// <summary>
    /// JSON-RPC 异常
    /// </summary>
    public class JsonRpcException : System.Exception
    {
        public int Code { get; }
        public new object Data { get; }

        public JsonRpcException(int code, string message, object data = null) : base(message)
        {
            Code = code;
            Data = data;
        }
    }

    #endregion

    #region MCP Initialize

    /// <summary>
    /// 初始化请求参数
    /// </summary>
    public class InitializeParams
    {
        public string protocolVersion;
        public ClientCapabilities capabilities;
        public Implementation clientInfo;
    }

    /// <summary>
    /// 初始化响应结果
    /// </summary>
    public class InitializeResult
    {
        public string protocolVersion = "2025-03-26";
        public ServerCapabilities capabilities;
        public Implementation serverInfo;
        public string instructions;
    }

    /// <summary>
    /// 客户端能力
    /// </summary>
    public class ClientCapabilities
    {
        public object roots;
        public object sampling;
        public Dictionary<string, object> experimental;
    }

    /// <summary>
    /// 服务端能力
    /// </summary>
    public class ServerCapabilities
    {
        public ToolCapabilities tools;
    }

    /// <summary>
    /// 工具能力
    /// </summary>
    public class ToolCapabilities
    {
        public bool listChanged = false;
    }

    /// <summary>
    /// 实现信息
    /// </summary>
    public class Implementation
    {
        public string name;
        public string version;
    }

    #endregion

    #region MCP Tools

    /// <summary>
    /// 工具定义
    /// </summary>
    public class ToolDefinition
    {
        public string name;
        public string description;
        public Dictionary<string, object> inputSchema;
    }

    /// <summary>
    /// 调用工具结果
    /// </summary>
    public class CallToolResult
    {
        public List<ContentItem> content;
        public bool isError = false;
    }

    /// <summary>
    /// 内容项
    /// </summary>
    public class ContentItem
    {
        public string type = "text";
        public string text;
    }

    /// <summary>
    /// 列出工具结果
    /// </summary>
    public class ListToolsResult
    {
        public List<ToolDefinition> tools;
        public string nextCursor;
    }

    #endregion

    #region JSON-RPC 标准错误码

    /// <summary>
    /// JSON-RPC 标准错误码
    /// </summary>
    public static class JsonRpcErrorCodes
    {
        public const int ParseError = -32700;
        public const int InvalidRequest = -32600;
        public const int MethodNotFound = -32601;
        public const int InvalidParams = -32602;
        public const int InternalError = -32603;

        // MCP 特定错误码
        public const int ServerNotInitialized = -32002;
        public const int UnknownError = -32001;
    }

    #endregion
}
