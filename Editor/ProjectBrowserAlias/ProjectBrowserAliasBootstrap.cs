using UnityEditor;


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
        static ProjectBrowserAliasBootstrap()
        {
            EditorApplication.delayCall += () =>
            {
                AliasDatabase.Initialize();
    
                // 下一部分安装 Harmony
                HarmonyInstaller.Install();
            };
        }
    }
}