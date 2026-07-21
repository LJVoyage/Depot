using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    /// VoyageForge ProjectBrowser Alias 启动器
    ///
    /// Unity Editor 启动:
    ///
    /// Domain Reload
    ///
    ///      |
    ///      |
    ///      v
    ///
    /// InitializeOnLoad
    ///
    ///      |
    ///      |
    ///      v
    ///
    /// Bootstrap
    ///
    ///      |
    ///      |
    ///      +---- 初始化数据库
    ///
    ///      |
    ///      +---- 安装 Harmony Patch
    ///
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectBrowserAliasBootstrap
    {
        private static double startTime;
        private static bool initialized = false;

        static ProjectBrowserAliasBootstrap()
        {
            // 如果已经初始化过，直接返回（避免重复）
            if (initialized) return;

            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            // 检查是否已过 2 秒
            if (EditorApplication.timeSinceStartup - startTime >= 1.0)
            {
                // 取消注册，防止重复执行
                EditorApplication.update -= OnUpdate;
                
                if (!initialized)
                {
                    initialized = true;
                    EditorApplication.delayCall += () => 
                    {
                        AliasDatabase.Initialize();
                        HarmonyInstaller.Install();
                    };
                    
                }
            }
        }
    }
}