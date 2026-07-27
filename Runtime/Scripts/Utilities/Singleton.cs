using System;

namespace VoyageForge.Depot.Runtime.Utilities
{
    /// <summary>
    /// 非 MonoBehaviour 单例基类。
    /// </summary>
    /// <typeparam name="T">派生类型，必须具有无参构造函数。</typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isDestroying = false;
        private static bool _isInitialized = false;

        /// <summary>当前实例是否已完成初始化。</summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>单例的有效实例是否存在（已创建且未销毁）。</summary>
        public static bool HasInstance => _instance != null && !_isDestroying;

        /// <summary>单例是否正在被销毁。</summary>
        public static bool IsDestroying => _isDestroying;

        /// <summary>获取单例实例。</summary>
        /// <exception cref="InvalidOperationException">在实例销毁后尝试创建新实例时抛出（除非调用 ResetDestroyState）。</exception>
        public static T Instance
        {
            get
            {
                // 整个逻辑放在锁内，确保原子性
                lock (_lock)
                {
                    if (_isDestroying)
                    {
                        // 为了避免在销毁后意外重建，可选择抛出异常或返回 null
                        // 这里返回 null，与原来行为一致
                        return null;
                    }

                    if (_instance == null)
                    {
                        var instance = new T();
                        try
                        {
                            instance.Initialize();
                        }
                        catch (Exception ex)
                        {
                            // 初始化失败，确保不留下半成品实例
                            // 可选择重新抛出，让调用者处理
                            throw new InvalidOperationException($"单例 {typeof(T)} 初始化失败", ex);
                        }
                        _isInitialized = true;
                        _instance = instance;
                    }
                    return _instance;
                }
            }
        }

        /// <summary>初始化回调。在单例创建后自动调用。派生类可重写以执行自定义初始化逻辑。</summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>销毁单例。清理实例引用并调用销毁回调。</summary>
        /// <remarks>销毁后，再次访问 Instance 将返回 null，除非调用 <see cref="ResetDestroyState"/> 重置销毁标志。</remarks>
        public static void Destroy()
        {
            lock (_lock)
            {
                if (_instance == null || _isDestroying)
                    return;

                _isDestroying = true;
                try
                {
                    _instance.OnDestroy();
                }
                catch (Exception ex)
                {
                    // 记录异常但继续清理
                    UnityEngine.Debug.LogError($"单例 {typeof(T)} 销毁回调 OnDestroy 发生异常: {ex}");
                }
                finally
                {
                    _isInitialized = false;
                    _instance = null;
                    // 注意：不重置 _isDestroying，防止销毁后重建。如需重建请调用 ResetDestroyState。
                }
            }
        }

        /// <summary>销毁回调。在单例销毁前调用。派生类可重写以执行自定义清理逻辑。</summary>
        protected virtual void OnDestroy()
        {
        }

        /// <summary>重置销毁状态，允许重新创建单例（通常用于测试或特殊场景）。</summary>
        public static void ResetDestroyState()
        {
            lock (_lock)
            {
                if (_instance != null)
                {
                    // 如果实例仍在，先销毁
                    Destroy();
                }
                _isDestroying = false;
                _isInitialized = false;
            }
        }

        /// <summary>尝试获取实例，若不存在或正在销毁则返回 false，避免 null 检查。</summary>
        /// <param name="instance">输出实例，若返回 true 则为有效实例，否则为 default。</param>
        /// <returns>是否成功获取有效实例。</returns>
        public static bool TryGetInstance(out T instance)
        {
            instance = Instance;
            return instance != null;
        }
    }
}