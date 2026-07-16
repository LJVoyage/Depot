using UnityEditor;
using UnityEngine;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    ///
    /// Project Browser Alias 编辑窗口
    ///
    /// 功能:
    ///
    /// 1. 拖入资源
    /// 2. 输入 Alias
    /// 3. 保存到 JSON
    ///
    ///
    /// 使用:
    ///
    /// VoyageForge
    ///     >
    /// Project Browser Alias
    ///
    /// </summary>
    public class ProjectBrowserAliasWindow :
        EditorWindow
    {
        /// <summary>
        ///
        /// 当前选择资源
        ///
        /// </summary>
        private Object targetObject;


        /// <summary>
        ///
        /// Alias 文本
        ///
        /// </summary>
        private string alias;


        /// <summary>
        ///
        /// 打开窗口菜单
        ///
        /// </summary>
        [MenuItem(
            "VoyageForge/Project Browser Alias"
        )]
        private static void Open()
        {
            GetWindow<ProjectBrowserAliasWindow>(
                "Alias"
            );
        }


        /// <summary>
        ///
        /// GUI绘制
        ///
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label(
                "Project Browser Alias",
                EditorStyles.boldLabel
            );


            EditorGUILayout.Space();


            targetObject =
                EditorGUILayout.ObjectField(
                    "资源",
                    targetObject,
                    typeof(Object),
                    false
                );


            if (targetObject == null)
                return;


            string path =
                ProjectBrowserAliasUtility
                    .GetAssetPath(
                        targetObject
                    );


            string guid =
                ProjectBrowserAliasUtility
                    .GetGUID(
                        path
                    );


            EditorGUILayout.LabelField(
                "Path",
                path
            );


            EditorGUILayout.LabelField(
                "GUID",
                guid
            );


            alias =
                EditorGUILayout.TextField(
                    "Alias",
                    alias
                );


            GUILayout.Space(10);


            if (
                GUILayout.Button(
                    "保存 Alias"
                ))
            {
                ProjectBrowserAliasDatabase
                    .SetAlias(
                        guid,
                        path,
                        alias
                    );


                Debug.Log(
                    $"Alias Saved\n{path}\n{alias}"
                );
            }
        }
    }
}