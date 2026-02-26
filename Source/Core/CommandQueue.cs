using System;
using System.Collections.Concurrent;
using Verse;

namespace RimWorldAI.Core
{
    /// <summary>
    /// 待处理的命令请求
    /// </summary>
    public class PendingCommand
    {
        public string Message { get; set; }
        public Action<string> ResponseCallback { get; set; }
    }

    /// <summary>
    /// 线程安全的命令队列
    /// WebSocket 在后台线程接收消息，通过此队列传递到主线程处理
    /// </summary>
    public static class CommandQueue
    {
        private static readonly ConcurrentQueue<PendingCommand> _queue = new ConcurrentQueue<PendingCommand>();

        /// <summary>
        /// 入队命令（从 WebSocket 后台线程调用）
        /// </summary>
        public static void Enqueue(string message, Action<string> responseCallback)
        {
            _queue.Enqueue(new PendingCommand
            {
                Message = message,
                ResponseCallback = responseCallback
            });
        }

        /// <summary>
        /// 处理所有待处理的命令（从主线程调用）
        /// </summary>
        public static void ProcessAll()
        {
            int processed = 0;
            while (_queue.TryDequeue(out var command) && processed < 100)
            {
                processed++;
                try
                {
                    string response = GameStateQuery.HandleQuery(command.Message);
                    command.ResponseCallback?.Invoke(response);
                }
                catch (Exception ex)
                {
                    Verse.Log.Error($"[RimWorldAI] Error processing command: {ex.Message}");
                    command.ResponseCallback?.Invoke($"{{\"success\":false,\"error\":\"{ex.Message.Replace("\"", "\\\"")}\"}}");
                }
            }
        }

        /// <summary>
        /// 获取待处理命令数量
        /// </summary>
        public static int PendingCount => _queue.Count;
    }
}
