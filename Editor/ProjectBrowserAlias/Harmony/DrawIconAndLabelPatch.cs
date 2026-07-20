using System;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    /// ObjectListArea.LocalGroup.DrawIconAndLabel Patch
    ///
    /// Unity 内部类型:UnityEditor.ObjectListArea+LocalGroup
    ///
    /// 方法:DrawIconAndLabel
    ///
    /// 当前 Unity 版本:
    ///
    /// void DrawIconAndLabel(
    ///     Rect rect,
    ///     FilteredHierarchy.FilterResult filterItem,
    ///     string label,
    ///     Texture2D icon,
    ///     bool selected,
    ///     bool focus
    /// )
    ///
    /// 参数:filterItem是关键。内部保存:instanceID
    /// 最终:instanceID=>GUID=>Alias
    ///
    /// 当文件是 列表形式时 使用此格式
    /// </summary>
    public static class DrawIconAndLabelPatch
    {
        public static void Install(Harmony harmony)
        {
            Type localGroup =
                typeof(EditorWindow)
                    .Assembly
                    .GetType(
                        "UnityEditor.ObjectListArea+LocalGroup"
                    );

            if (localGroup == null)
            {
                Debug.LogError("[VoyageForge Alias] LocalGroup Not Found");
                return;
            }

            MethodInfo target =
                localGroup.GetMethod(
                    "DrawIconAndLabel",
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.NonPublic
                );

            if (target == null)
            {
                Debug.LogError("[VoyageForge Alias] DrawIconAndLabel Not Found");
                return;
            }

            MethodInfo prefix =
                typeof(DrawIconAndLabelPatch)
                    .GetMethod(
                        nameof(Prefix),
                        BindingFlags.Static |
                        BindingFlags.NonPublic
                    );

            harmony.Patch(target, prefix: new HarmonyMethod(prefix));

            //Debug.Log("[VoyageForge Alias] DrawIconAndLabel Patch OK");
        }


        /// <summary>
        ///
        /// Prefix 参数必须和 Unity 方法参数匹配
        ///
        /// 这里不能写:
        ///
        /// string label
        ///
        /// 因为 Harmony 根据参数名绑定。
        ///
        /// Unity 内部参数可能:
        ///
        /// filterItem
        ///
        /// label
        ///
        ///
        /// 如果 Unity 改名:
        ///
        /// filterItem
        ///
        /// =>
        ///
        /// item
        ///
        ///
        /// 会失败。
        ///
        ///
        /// 生产环境建议使用:
        ///
        /// object[] __args
        ///
        /// 避免版本问题。
        ///
        /// </summary>
        private static void Prefix(object[] __args)
        {
            if (__args == null)
                return;

            string oldLabel = null;

            int instanceID = 0;

            foreach (object arg in __args)
            {
                if (arg == null)
                    continue;

                if (arg is string)
                {
                    oldLabel =
                        arg as string;
                }

                /*
                 *
                 * Unity FilterResult
                 *
                 * 不是公开类型
                 *
                 *
                 * 所以反射读取
                 *
                 */

                Type type = arg.GetType();

                FieldInfo field = type.GetField(
                    "instanceID",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic
                );

                if (field != null)
                {
                    object value = field.GetValue(arg);

                    if (value is int)
                    {
                        instanceID = (int)value;
                    }
                }
            }


            if (instanceID == 0)
                return;

            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj == null)
                return;

            string path = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(path))
                return;

            string guid = AssetDatabase.AssetPathToGUID(path);


            if (string.IsNullOrEmpty(guid))
                return;


            string alias;


            if (AliasDatabase.TryGetAlias(guid, out alias))
            {
                /*
                 *
                 * 修改真正绘制参数
                 *
                 *
                 * 注意:
                 *
                 * 这里修改 args
                 *
                 *
                 * 后续 Unity:
                 *
                 * DrawIconAndLabel
                 *
                 * 使用修改后的 label
                 *
                 *
                 */


                for (int i = 0; i < __args.Length; i++)
                {
                    if (__args[i] is string)
                    {
                        __args[i] = alias;

                        // Debug.Log("[VoyageForge Alias] " + oldLabel + " => " + alias);
                        break;
                    }
                }
            }
        }
    }
}