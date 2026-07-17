using HarmonyLib;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor.IMGUI.Controls;

[HarmonyPatch]
public static class AssetsTreeViewAliasPatch
{
    private static readonly Dictionary<string, string> TestAliases = new Dictionary<string, string>
    {
        { "9fc0d4010bbf28b4594072e72b8655ab", "⭐ 文档" },
        { "你的第二个GUID", "📁 美术" }
    };

    static MethodBase TargetMethod()
    {
        // 获取 AssetsTreeViewDataSource 类型
        var dataSourceType = Type.GetType("UnityEditor.AssetsTreeViewDataSource, UnityEditor.CoreModule");
        if (dataSourceType == null)
        {
            Debug.LogError("[Alias] AssetsTreeViewDataSource type not found.");
            return null;
        }
        // 获取 FetchData 方法
        var method = dataSourceType.GetMethod("FetchData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            Debug.LogError("[Alias] FetchData method not found.");
        }
        return method;
    }

    static void Postfix(object __instance) // __instance 是 AssetsTreeViewDataSource
    {
        Debug.Log(__instance);
        
        try
        {
            // 获取基类 LazyTreeViewDataSource 的 m_Rows 字段
            var baseType = __instance.GetType().BaseType; // LazyTreeViewDataSource
            if (baseType == null) return;
            var rowsField = baseType.GetField("m_Rows", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rowsField == null)
            {
                // 备选：尝试从当前类型查找
                rowsField = __instance.GetType().GetField("m_Rows", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rowsField == null) return;
            }
            var rows = rowsField.GetValue(__instance) as System.Collections.IList;
            if (rows == null) return;

            // 获取 FolderTreeItem 类型
            var folderTreeItemType = Type.GetType("UnityEditor.AssetsTreeViewDataSource+FolderTreeItem, UnityEditor.CoreModule");
            if (folderTreeItemType == null)
            {
                Debug.LogError("[Alias] FolderTreeItem type not found.");
                return;
            }

            // 获取 Guid 属性（或字段）
            var guidProp = folderTreeItemType.GetProperty("Guid", BindingFlags.Public | BindingFlags.Instance);
            if (guidProp == null)
            {
                // 尝试字段
                guidProp = null;
                var guidField = folderTreeItemType.GetField("Guid", BindingFlags.Public | BindingFlags.Instance);
                if (guidField != null)
                {
                    // 使用字段读取
                    foreach (var item in rows)
                    {
                        if (item != null && folderTreeItemType.IsAssignableFrom(item.GetType()))
                        {
                            string guid = guidField.GetValue(item) as string;
                            if (!string.IsNullOrEmpty(guid) && TestAliases.TryGetValue(guid, out string alias))
                            {
                                // 修改 displayName 字段（TreeViewItem 的公共字段）
                                var displayNameField = typeof(TreeViewItem).GetField("displayName", BindingFlags.Public | BindingFlags.Instance);
                                if (displayNameField != null)
                                    displayNameField.SetValue(item, alias);
                            }
                        }
                    }
                    return;
                }
                else
                {
                    Debug.LogError("[Alias] Guid property or field not found on FolderTreeItem.");
                    return;
                }
            }

            // 通过属性读取
            foreach (var item in rows)
            {
                if (item != null && folderTreeItemType.IsAssignableFrom(item.GetType()))
                {
                    string guid = guidProp.GetValue(item) as string;
                    if (!string.IsNullOrEmpty(guid) && TestAliases.TryGetValue(guid, out string alias))
                    {
                        // 修改 displayName
                        var displayNameField = typeof(TreeViewItem).GetField("displayName", BindingFlags.Public | BindingFlags.Instance);
                        if (displayNameField != null)
                            displayNameField.SetValue(item, alias);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Alias] Error in Postfix: {ex}");
        }
    }
}

[InitializeOnLoad]
public static class AliasLoader
{
    static AliasLoader()
    {
        // EditorApplication.delayCall += () =>
        // {
        //     var harmony = new Harmony("com.yourcompany.alias");
        //     harmony.PatchAll();
        //     Debug.Log("[Alias] FetchData patch loaded (reflection-based).");
        // };
    }
}