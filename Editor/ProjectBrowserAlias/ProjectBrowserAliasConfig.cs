using System;
using System.Collections.Generic;


namespace VoyageForge.EditorTools
{
    /// <summary>
    /// ProjectBrowser Alias 配置文件
    ///
    /// 对应:
    ///
    /// ProjectSettings/VoyageForge/ProjectBrowserAliasSettings.json
    ///
    /// 保存所有资源别名
    ///
    /// Key:
    /// Unity GUID
    ///
    /// Value:
    /// Alias
    ///
    /// </summary>
    [Serializable]
    public class ProjectBrowserAliasConfig
    {
        /// <summary>
        /// 配置版本
        /// 用于以后升级格式
        /// </summary>
        public int version = 1;


        /// <summary>
        /// 所有 Alias 数据
        /// </summary>
        public List<ProjectBrowserAliasItem> items = new();
    }


    /// <summary>
    /// 单个资源 Alias 数据
    /// </summary>
    [Serializable]
    public class ProjectBrowserAliasItem
    {
        /// <summary>
        /// Unity GUID
        ///
        /// 例如:
        ///
        /// 1ab23cd456ef789
        ///
        /// GUID 永远不会因为移动文件改变
        ///
        /// 所以比路径更加适合作为 Key
        /// </summary>
        public string guid;

        /// <summary>
        /// 当前路径缓存
        ///
        /// 用于显示
        /// </summary>
        public string path;


        /// <summary>
        /// 显示名称
        /// </summary>
        public string alias;
    }
}