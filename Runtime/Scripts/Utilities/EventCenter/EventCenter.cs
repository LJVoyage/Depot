using System;
using System.Collections.Generic;

namespace VoyageForge.Depot.Runtime.Utilities
{
    /// <summary>
    /// 泛型事件中心
    /// </summary>
    /// <typeparam name="TBase">所有事件的基类型（约束类型）</typeparam>
    public class EventCenter<TBase> where TBase : IEvent
    {
        // 储存所有事件监听器：Key = 具体事件类型，Value = 对应的多播委托
        private readonly Dictionary<Type, Delegate> _eventTable = new Dictionary<Type, Delegate>();

        // 提供一个全局静态单例实例，方便跨模块调用（也可以不用，按需 new）
        public static EventCenter<TBase> Instance { get; } = new EventCenter<TBase>();

        // 私有构造，确保单例或受控创建
        private EventCenter()
        {
        }

        /// <summary>
        /// 订阅事件（约束 E 必须是 TBase 的子类/实现类）
        /// </summary>
        public static void Subscribe<E>(Action<E> listener) where E : TBase
        {
            Type eventType = typeof(E);
            if (Instance._eventTable.TryGetValue(eventType, out var existingDelegate))
            {
                // 合并委托
                Instance._eventTable[eventType] = Delegate.Combine(existingDelegate, listener);
            }
            else
            {
                Instance._eventTable[eventType] = listener;
            }
        }

        /// <summary>
        /// 取消订阅（必须在销毁时调用）
        /// </summary>
        public static void Unsubscribe<E>(Action<E> listener) where E : TBase
        {
            Type eventType = typeof(E);
            if (Instance._eventTable.TryGetValue(eventType, out var existingDelegate))
            {
                var newDelegate = Delegate.Remove(existingDelegate, listener);
                if (newDelegate == null)
                    Instance._eventTable.Remove(eventType);
                else
                    Instance._eventTable[eventType] = newDelegate;
            }
        }

        /// <summary>
        /// 触发事件（约束 E 必须是 TBase 的子类/实现类）
        /// </summary>
        public static void Trigger<E>(E eventData) where E : TBase
        {
            Type eventType = typeof(E);
            if (Instance._eventTable.TryGetValue(eventType, out var existingDelegate))
            {
                // 将 Delegate 转换为具体的强类型委托并执行
                (existingDelegate as Action<E>)?.Invoke(eventData);
            }
        }

        /// <summary>
        /// 清空所有监听（用于场景卸载或重置）
        /// </summary>
        public static void Clear() => Instance._eventTable.Clear();
    }
}