using System;
using System.Collections.Generic;
using TechC.Core.Log;

namespace TechC.Core.Events
{
    /// <summary>
    /// 型安全なイベントバスシステム
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _eventHandlers = new Dictionary<Type, List<Delegate>>();

        #region Subscribe

        /// <summary>
        /// イベントを購読する
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;

            Type eventType = typeof(T);

            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = new List<Delegate>();

            _eventHandlers[eventType].Add(handler);
        }

        #endregion

        #region Unsubscribe

        /// <summary>
        /// イベントの購読を解除する
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            if (handler == null) return;

            Type eventType = typeof(T);

            if (!_eventHandlers.ContainsKey(eventType)) return;

            _eventHandlers[eventType].Remove(handler);

            if (_eventHandlers[eventType].Count == 0)
                _eventHandlers.Remove(eventType);
        }

        #endregion

        #region Publish

        /// <summary>
        /// イベントを発行する
        /// </summary>
        public static void Publish<T>(T eventData) where T : IGameEvent
        {
            Type eventType = typeof(T);

            if (!_eventHandlers.ContainsKey(eventType)) return;

            var handlers = new List<Delegate>(_eventHandlers[eventType]);
            foreach (var handler in handlers)
            {
                try
                {
                    (handler as Action<T>)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    CusLog.Error("EventBus", $"イベントハンドラーでエラーが発生しました: {eventType.Name}\n{ex}");
                }
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// すべての購読を解除する
        /// </summary>
        public static void ClearAllSubscriptions() => _eventHandlers.Clear();

        #endregion
    }
}