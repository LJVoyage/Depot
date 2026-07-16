using UnityEditor;
using UnityEngine;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    ///
    /// Project Browser Alias 工具类
    ///
    /// 提供：
    ///
    /// Object
    ///     ↓
    /// AssetPath
    ///     ↓
    /// GUID
    ///
    /// 的转换
    ///
    ///
    /// 为什么单独封装？
    ///
    /// 因为 Harmony Patch 运行在 Unity 内部 GUI 绘制流程
    ///
    /// 不应该把 AssetDatabase 逻辑散落在 Patch 内
    ///
    /// </summary>
    public static class ProjectBrowserAliasUtility
    {
        /// <summary>
        ///
        /// Unity Object 获取 AssetPath
        ///
        /// 示例:
        ///
        /// LoginPanel.prefab
        ///
        /// 返回:
        ///
        /// Assets/UI/LoginPanel.prefab
        ///
        /// </summary>
        public static string GetAssetPath(
            Object obj
        )
        {
            if (obj == null)
                return null;


            return AssetDatabase.GetAssetPath(
                obj
            );
        }


        /// <summary>
        ///
        /// Object 获取 GUID
        ///
        ///
        /// 流程:
        ///
        /// Object
        /// ↓
        /// AssetPath
        /// ↓
        /// AssetPathToGUID
        ///
        /// </summary>
        public static string GetGUID(
            Object obj
        )
        {
            string path =
                GetAssetPath(
                    obj
                );


            return GetGUID(
                path
            );
        }


        /// <summary>
        ///
        /// AssetPath 获取 GUID
        ///
        /// </summary>
        public static string GetGUID(
            string path
        )
        {
            if (string.IsNullOrEmpty(path))
                return null;


            return AssetDatabase
                .AssetPathToGUID(
                    path
                );
        }


        /// <summary>
        ///
        /// GUID 获取 AssetPath
        ///
        /// </summary>
        public static string GetAssetPathByGUID(
            string guid
        )
        {
            if (string.IsNullOrEmpty(guid))
                return null;


            return AssetDatabase
                .GUIDToAssetPath(
                    guid
                );
        }


        /// <summary>
        ///
        /// 判断资源是否有效
        ///
        /// 
        /// Unity 删除资源后：
        ///
        /// GUID 可能还存在于 JSON
        ///
        /// 需要过滤
        ///
        /// </summary>
        public static bool IsAssetValid(
            string guid
        )
        {
            string path =
                GetAssetPathByGUID(
                    guid
                );


            return
                !string.IsNullOrEmpty(path)
                &&
                AssetDatabase.LoadAssetAtPath<Object>(
                    path
                ) != null;
        }


        /// <summary>
        ///
        /// 清理无效 Alias
        ///
        ///
        /// 例如:
        ///
        /// LoginPanel.prefab
        ///
        /// 删除
        ///
        /// JSON:
        ///
        /// guid xxxx
        ///
        /// 
        /// 此函数删除残留数据
        ///
        /// </summary>
        public static void CleanupInvalid()
        {
            var config =
                ProjectBrowserAliasDatabase.Config;


            config.items.RemoveAll(x =>
                !IsAssetValid(
                    x.guid
                )
            );


            ProjectBrowserAliasDatabase.Save();
        }
    }
}