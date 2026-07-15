using System;

namespace VoyageForge.Depot.Runtime.Utilities
{
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

        public static T Instance
        {
            get
            {
                if (_isDestroying)
                {
                    return null;
                }

                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            var instance = new T();
                            instance.Initialize();
                            _isInitialized = true;
                            _instance = instance;
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>初始化回调。在单例创建后自动调用。派生类可重写以执行自定义初始化逻辑。</summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>销毁单例。清理实例引用并调用销毁回调。</summary>
        public static void Destroy()
        {
            lock (_lock)
            {
                if (_instance != null && !_isDestroying)
                {
                    _isDestroying = true;
                    _instance.OnDestroy();
                    _isInitialized = false;
                    _instance = null;
                }
            }
        }

        /// <summary>销毁回调。在单例销毁前调用。派生类可重写以执行自定义清理逻辑。</summary>
        protected virtual void OnDestroy()
        {
        }
    }
}