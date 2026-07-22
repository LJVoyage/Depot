using System.Collections;
using UnityEditor;
using UnityEngine;


namespace VoyageForge.Depot.Editor
{
    /// <summary>
    /// VoyageForge ProjectBrowser Alias 启动器
    /// Unity Editor 启动:
    /// Domain Reload
    ///      |
    ///      |
    ///      v
    /// InitializeOnLoad
    ///      |
    ///      |
    ///      v
    /// Bootstrap||+---- 初始化数据库|+---- 安装 Harmony Patch
    ///
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectBrowserAliasBootstrap
    {
        private static bool initialized = false;

        static ProjectBrowserAliasBootstrap()
        {
            EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI;
        }

        private static void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            Initialize();
            EditorApplication.projectWindowItemOnGUI -=  ProjectWindowItemOnGUI;
        }

        private static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            HarmonyInstaller.Install(); // 内部会自行处理 GUI 就绪问题
        }
    }
}