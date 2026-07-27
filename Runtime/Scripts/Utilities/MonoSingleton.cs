using UnityEngine;

namespace VoyageForge.Depot.Runtime.Utilities
{
    /// <summary>
    /// 可继承的 MonoBehaviour 单例基类。
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        private static readonly object _lock = new object();

        private static bool _applicationIsQuitting = false;
        
        private static bool _isInitialized = false;

        /// <summary>当前实例是否已完成初始化。</summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>单例的有效实例是否存在（已创建且未标记销毁）。</summary>
        public static bool HasInstance => _instance != null && !_applicationIsQuitting;

        /// <summary>单例是否正在被销毁（或应用正在退出）。</summary>
        public static bool IsDestroying => _applicationIsQuitting;

        /// <summary>单例 GameObject 名称，派生类可重写以自定义。</summary>
        protected virtual string _name => $"[Singleton] {typeof(T)}";

        /// <summary>获取单例实例。</summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] 已退出应用，不再创建 {typeof(T)} 单例。");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // 尝试在场景中查找现有实例
                        _instance = (T)FindObjectOfType(typeof(T));

                        // 如果没有，则创建新 GameObject
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = _instance._name;   // 使用虚拟属性

                            // 默认设置为跨场景持久化
                            DontDestroyOnLoad(singletonObject);

                            Debug.Log($"[MonoSingleton] 创建 {typeof(T)} 单例。");
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>在 Awake 时检查重复实例</summary>
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);

                // 调用抽象初始化方法（必须由派生类实现）
                OnInitialize();
                _isInitialized = true;

                // 可选扩展钩子
                OnAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] 场景中已有 {typeof(T)} 实例，销毁重复实例。");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化回调。在 Awake 完成实例注册后自动调用。
        /// 派生类必须实现此方法以完成自定义初始化逻辑。
        /// </summary>
        protected abstract void OnInitialize();

        /// <summary>
        /// Awake 扩展钩子，派生类可重写以执行额外初始化（可选）。
        /// </summary>
        protected virtual void OnAwake() { }

        /// <summary>应用退出时标记单例销毁</summary>
        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
            OnApplicationQuitting();   // 钩子供派生类清理
        }

        /// <summary>
        /// OnApplicationQuit 扩展钩子，派生类可重写以执行清理（可选）。
        /// </summary>
        protected virtual void OnApplicationQuitting() { }

        /// <summary>销毁时清理。Unity 会在对象销毁时自动调用。</summary>
        private void OnDestroy()
        {
            if (_instance == this)
            {
                // 注意：不修改 _applicationIsQuitting，以支持非退出场景下的重新创建
                _isInitialized = false;
                _instance = null;
                OnDestroying();        // 钩子供派生类清理
            }
        }

        /// <summary>
        /// OnDestroy 扩展钩子，派生类可重写以执行清理（可选）。
        /// </summary>
        protected virtual void OnDestroying() { }
    }
}