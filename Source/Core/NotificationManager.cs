using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;

namespace RimWorldAI.Core
{
    /// <summary>
    /// 通知事件数据
    /// </summary>
    public class NotificationEvent
    {
        public string Id { get; set; }
        public string Type { get; set; }        // letter, message
        public string Title { get; set; }
        public string Content { get; set; }
        public long Timestamp { get; set; }
        public long RealTimestamp { get; set; }
        public string DefName { get; set; }
        public bool IsNew { get; set; } = true;

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["id"] = Id,
                ["type"] = Type,
                ["title"] = Title ?? "",
                ["content"] = Content ?? "",
                ["timestamp"] = Timestamp,
                ["realTimestamp"] = RealTimestamp,
                ["defName"] = DefName ?? "",
                ["isNew"] = IsNew
            };
        }
    }

    /// <summary>
    /// 通知管理器 - 收集游戏中的信件和消息
    /// </summary>
    public class NotificationManager
    {
        private static NotificationManager _instance;
        public static NotificationManager Instance => _instance ?? (_instance = new NotificationManager());

        private readonly ConcurrentQueue<NotificationEvent> _notifications = new ConcurrentQueue<NotificationEvent>();
        private readonly HashSet<int> _processedLetterIds = new HashSet<int>();
        private long _idCounter = 0;

        private const int MaxNotifications = 500;
        private const int MaxProcessedIds = 1000;

        private long CurrentTick => Find.TickManager?.TicksGame ?? 0;

        private string GenerateId()
        {
            return $"notif_{CurrentTick}_{System.Threading.Interlocked.Increment(ref _idCounter)}";
        }

        /// <summary>
        /// 添加信件通知
        /// </summary>
        public void AddLetter(Letter letter)
        {
            if (letter == null) return;

            int letterId = letter.ID;
            lock (_processedLetterIds)
            {
                if (_processedLetterIds.Contains(letterId))
                    return;

                _processedLetterIds.Add(letterId);
                if (_processedLetterIds.Count > MaxProcessedIds)
                    _processedLetterIds.Clear();
            }

            var notification = new NotificationEvent
            {
                Id = GenerateId(),
                Type = "letter",
                Title = letter.Label.RawText ?? "",
                Content = "",  // Letter内容需要通过其他方式获取
                Timestamp = CurrentTick,
                RealTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DefName = letter.def?.defName ?? "",
                IsNew = true
            };

            _notifications.Enqueue(notification);
            TrimQueue();
            Verse.Log.Message($"[RimWorldAI] Letter captured: {notification.Title}");
        }

        /// <summary>
        /// 添加消息通知
        /// </summary>
        public void AddMessage(string text, MessageTypeDef def)
        {
            if (string.IsNullOrEmpty(text)) return;

            var notification = new NotificationEvent
            {
                Id = GenerateId(),
                Type = "message",
                Title = text,
                Content = "",
                Timestamp = CurrentTick,
                RealTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DefName = def?.defName ?? "",
                IsNew = true
            };

            _notifications.Enqueue(notification);
            TrimQueue();
            Verse.Log.Message($"[RimWorldAI] Message captured: {notification.Title}");
        }

        private void TrimQueue()
        {
            while (_notifications.Count > MaxNotifications)
            {
                _notifications.TryDequeue(out _);
            }
        }

        /// <summary>
        /// 获取最近的通知
        /// </summary>
        public List<NotificationEvent> GetRecentNotifications(int limit = 50)
        {
            return _notifications.ToList()
                .OrderByDescending(n => n.Timestamp)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// 获取指定ID之后的通知
        /// </summary>
        public List<NotificationEvent> GetNotificationsSince(string sinceId, int limit = 50)
        {
            var all = _notifications.ToList();
            int idx = all.FindIndex(n => n.Id == sinceId);
            if (idx >= 0)
            {
                return all.Skip(idx + 1).Take(limit).ToList();
            }
            return all.OrderByDescending(n => n.Timestamp).Take(limit).ToList();
        }

        /// <summary>
        /// 获取未读通知
        /// </summary>
        public List<NotificationEvent> GetUnreadNotifications(int limit = 50)
        {
            return _notifications.ToList()
                .Where(n => n.IsNew)
                .OrderByDescending(n => n.Timestamp)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// 标记所有为已读
        /// </summary>
        public void MarkAllAsRead()
        {
            foreach (var n in _notifications)
                n.IsNew = false;
        }

        /// <summary>
        /// 清除所有通知
        /// </summary>
        public void ClearAll()
        {
            while (_notifications.TryDequeue(out _)) { }
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, object> GetStats()
        {
            var all = _notifications.ToList();
            return new Dictionary<string, object>
            {
                ["total"] = all.Count,
                ["unread"] = all.Count(n => n.IsNew),
                ["letters"] = all.Count(n => n.Type == "letter"),
                ["messages"] = all.Count(n => n.Type == "message")
            };
        }
    }
}
