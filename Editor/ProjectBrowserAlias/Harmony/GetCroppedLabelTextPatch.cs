using System;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VoyageForge.EditorTools.ProjectBrowserAlias;


namespace VoyageForge.EditorTools
{
    /// <summary>
    /// ProjectBrowser资源名称替换核心Patch
    ///
    /// Unity调用链:
    ///
    /// ProjectBrowser.OnGUI
    ///        |
    ///        v
    /// ObjectListArea.OnGUI
    ///        |
    ///        v
    /// LocalGroup.DrawItem
    ///        |
    ///        v
    /// ObjectListArea.GetCroppedLabelText
    ///        |
    ///        v
    /// GUIStyle.Draw
    ///
    ///
    /// GetCroppedLabelText 是 Unity 最终裁剪显示名称的位置。
    /// 优点:
    /// 1. 有 AssetReference
    /// 2. 可以获取 GUID
    /// 3. 不影响其它 IMGUI
    /// 4. 不污染 GUIContent
    ///
    /// 图表形式时调用的函数
    /// </summary>
    public static class GetCroppedLabelTextPatch
    {
        public static void Install(Harmony harmony)
        {
            MethodInfo target = FindTarget();
            
            if (target == null)
            {
                Debug.LogError("[VoyageForge Alias] GetCroppedLabelText not found");
                return;
            }


            MethodInfo prefix =typeof(GetCroppedLabelTextPatch)
                    .GetMethod(
                        nameof(Prefix),
                        BindingFlags.Static |
                        BindingFlags.NonPublic
                    );


            harmony.Patch(target, prefix: new HarmonyMethod(prefix));

            //Debug.Log("[VoyageForge Alias] GetCroppedLabelText Patch OK\n" + target);
        }


        private static MethodInfo FindTarget()
        {
            Type objectListArea = typeof(EditorWindow)
                .Assembly
                .GetType(
                    "UnityEditor.ObjectListArea"
                );


            if (objectListArea == null)
            {
                Debug.LogError(
                    "ObjectListArea not found"
                );

                return null;
            }


            Type assetReferenceType = typeof(InternalEditorUtility)
                .GetNestedType(
                    "AssetReference",
                    BindingFlags.NonPublic
                );


            if (assetReferenceType == null)
            {
                Debug.LogError(
                    "AssetReference not found"
                );

                return null;
            }


            MethodInfo method = objectListArea.GetMethod(
                "GetCroppedLabelText",
                BindingFlags.Instance |
                BindingFlags.NonPublic,
                null,
                new Type[]
                {
                    assetReferenceType,
                    typeof(string),
                    typeof(float)
                },
                null
            );


            return method;
        }


        /// <summary>
        ///
        /// 原函数:
        ///
        /// string GetCroppedLabelText(
        ///     AssetReference assetReference,
        ///     string label,
        ///     float width
        /// )
        ///
        ///
        /// Prefix:
        ///
        /// 可以修改返回值
        ///
        /// </summary>
        private static bool Prefix(object assetReference, string fullText, float cropWidth, ref string __result)
        {
            if (string.IsNullOrEmpty(fullText))
                return true;

            string guid = GetGUID(assetReference);

            if (string.IsNullOrEmpty(guid))
                return true;

            if (AliasDatabase.TryGetAlias(guid, out var alias))
            {
                __result = alias;
            }
            else
            {
                return true;
            }

            if (string.IsNullOrEmpty(alias))
                return true;

            Debug.Log($"[VoyageForge Alias]\n" + $"GUID:{guid}\n" + $"OLD:{fullText}\n" + $"NEW:{alias}");

            __result = alias;

            return false;
        }


        private static string GetGUID(object assetReference)
        {
            if (assetReference == null)
                return null;

            Type type = assetReference.GetType();

            //
            // Unity InternalEditorUtility.AssetReference
            //
            // 内部类
            //
            // 不公开
            //
            // 通过反射获取:
            //
            // instanceID
            //

            FieldInfo field = type.GetField(
                "instanceID",
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public
            );

            if (field == null)
            {
                field = type.GetField(
                    "m_InstanceID",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic
                );
            }

            if (field == null)
                return null;

            int id = (int)field.GetValue(assetReference);

            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(id);

            if (obj == null)
                return null;


            string path = AssetDatabase.GetAssetPath(obj);

            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.AssetPathToGUID(path);
        }
    }
}