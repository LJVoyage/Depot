using HarmonyLib;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    ///
    /// Project Browser Alias 核心 Patch
    ///
    ///
    /// ==========================================================
    /// Unity Project Browser 显示流程分析
    /// ==========================================================
    ///
    /// 通过调用栈分析得到：
    ///
    ///
    /// ProjectBrowser.OnGUI()
    ///
    ///     ↓
    ///
    /// ObjectListArea.OnGUI()
    ///
    ///     ↓
    ///
    /// ObjectListArea.HandleListArea()
    ///
    ///     ↓
    ///
    /// ObjectListArea.Group.Draw()
    ///
    ///     ↓
    ///
    /// ObjectListArea.LocalGroup.DrawInternal()
    ///
    ///     ↓
    ///
    /// ObjectListArea.LocalGroup.DrawItem()
    ///
    ///     ↓
    ///
    /// ObjectListArea.LocalGroup.DrawIconAndLabel()
    ///
    ///     ↓
    ///
    /// GUIStyle.Draw()
    ///
    ///     ↓
    ///
    /// GUIContent.Temp(string)
    ///
    ///
    /// 最终发现：
    ///
    /// Unity 并不是从 ObjectListArea.m_Content
    /// 读取最终文字。
    ///
    /// m_Content:
    ///
    /// 是内部缓存数据
    ///
    /// 在绘制之前还会重新生成 GUIContent
    ///
    ///
    /// 真正进入 IMGUI 绘制流程的位置：
    ///
    /// GUIContent.Temp()
    ///
    ///
    /// 所以 Hook:
    ///
    /// GUIContent.Temp(string)
    ///
    ///
    /// ==========================================================
    ///
    /// </summary>
    [InitializeOnLoad]
    public static class GUIContentTempPatch
    {
        private static Harmony harmony;


        /// <summary>
        ///
        /// 防止 Unity 重载导致重复 Patch
        ///
        /// </summary>
        private static bool installed;


        static GUIContentTempPatch()
        {
            EditorApplication.delayCall += Install;
        }


        /// <summary>
        ///
        /// 安装 Harmony Patch
        ///
        ///
        /// 注意:
        ///
        /// UnityEditor 内部 API 全部没有公开保证
        ///
        /// Unity 升级可能改变：
        ///
        /// GUIContent.Temp
        ///
        /// 参数名称
        ///
        /// 调用位置
        ///
        ///
        /// 所以这里必须：
        ///
        /// 反射寻找
        ///
        /// </summary>
        private static void Install()
        {
            if (installed)
                return;


            installed = true;


            harmony =
                new Harmony(
                    "VoyageForge.ProjectBrowserAlias.GUIContent"
                );


            /*
             *
             * 查找:
             *
             * UnityEngine.GUIContent.Temp(string)
             *
             *
             * Unity 2022.3:
             *
             * 方法签名:
             *
             * static GUIContent Temp(string t)
             *
             *
             * 注意:
             *
             * 参数名字是 t
             *
             * 不是 text
             *
             *
             * Harmony Prefix:
             *
             * ref string t
             *
             * 必须匹配参数名称
             *
             *
             * 否则:
             *
             * Exception:
             *
             * Parameter "text" not found
             *
             *
             */


            MethodInfo target =
                typeof(GUIContent)
                    .GetMethod(
                        "Temp",
                        BindingFlags.Static |
                        BindingFlags.NonPublic |
                        BindingFlags.Public,
                        null,
                        new Type[]
                        {
                            typeof(string)
                        },
                        null
                    );


            if (target == null)
            {
                Debug.LogError(
                    "GUIContent.Temp(string) not found"
                );


                return;
            }


            MethodInfo prefix =
                typeof(GUIContentTempPatch)
                    .GetMethod(
                        nameof(Prefix),
                        BindingFlags.Static |
                        BindingFlags.NonPublic
                    );


            harmony.Patch(
                target,
                prefix:
                new HarmonyMethod(
                    prefix
                )
            );


            Debug.Log(
                "VoyageForge ProjectBrowserAlias GUIContent.Temp Patch OK"
            );
        }


        /// <summary>
        ///
        /// Harmony Prefix
        ///
        ///
        /// 执行时间:
        ///
        /// Unity 创建 GUIContent 之前
        ///
        ///
        /// 参数:
        ///
        /// ref string t
        ///
        /// 对应:
        ///
        /// GUIContent.Temp(string t)
        ///
        ///
        /// 修改这里:
        ///
        /// 会影响后续绘制
        ///
        /// </summary>
        private static void Prefix(ref string t)
        {
            if (string.IsNullOrEmpty(t))
                return;


            /*
             *
             * 防止修改 Unity 内部字符串
             *
             * 例如:
             *
             * Mesh
             * Material
             * Shader
             *
             *
             * 我们只处理资源 Alias
             *
             */


            string path = FindAssetPathByCurrentLabel(t);

            if (string.IsNullOrEmpty(path))
                return;

            string guid = AssetDatabase.AssetPathToGUID(path);


            string alias = ProjectBrowserAliasDatabase.GetAlias(guid);


            if (!string.IsNullOrEmpty(alias))
            {
                t = alias;
            }
        }


        /// <summary>
        ///
        /// 根据 Unity 当前绘制名称寻找资源
        ///
        ///
        /// 注意:
        ///
        /// GUIContent.Temp
        ///
        /// 只提供字符串
        ///
        /// 没有 AssetReference
        ///
        ///
        /// 所以不能直接:
        ///
        /// GUID
        ///
        ///
        /// 需要通过:
        ///
        /// AssetDatabase
        ///
        /// 搜索匹配
        ///
        ///
        /// </summary>
        private static string FindAssetPathByCurrentLabel(string label)
        {
            string[] ids = AssetDatabase.FindAssets(label);

            foreach (string id in ids)
            {
                string path = AssetDatabase.GUIDToAssetPath(id);

                string name = System.IO.Path.GetFileNameWithoutExtension(path);

                if (name == label)
                {
                    return path;
                }
            }

            return null;
        }
    }
}