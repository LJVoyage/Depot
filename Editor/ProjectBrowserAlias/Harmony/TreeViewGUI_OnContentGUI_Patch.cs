using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    public static class TreeViewGUI_OnContentGUI_Patch
    {
        // 缓存类型和属性访问器，避免重复反射
        private static Type s_FolderTreeItemType;
        private static PropertyInfo s_GuidProperty;
        private static FieldInfo s_GuidField;
        private static bool s_Initialized;

        public static void Install(Harmony harmony)
        {
            // 1. 动态获取 internal 类型 TreeViewGUI
            Type treeViewGUIType = AccessTools.TypeByName("UnityEditor.IMGUI.Controls.TreeViewGUI");
            if (treeViewGUIType == null)
            {
                Debug.LogError("[VoyageForge] 未找到 TreeViewGUI 类型");
                return;
            }

            // 2. 获取目标方法 OnContentGUI（参数必须完全匹配）
            MethodInfo target = AccessTools.Method(treeViewGUIType, "OnContentGUI",
                new Type[]
                {
                    typeof(Rect), // rect
                    typeof(int), // row
                    typeof(TreeViewItem), // item
                    typeof(string), // label
                    typeof(bool), // selected
                    typeof(bool), // focused
                    typeof(bool), // useBoldFont
                    typeof(bool) // isPinging
                });

            if (target == null)
            {
                Debug.LogError("[VoyageForge] 未找到 OnContentGUI 方法");
                return;
            }

            // 3. 获取我们的 Prefix 方法
            MethodInfo prefix =
                typeof(TreeViewGUI_OnContentGUI_Patch).GetMethod(nameof(Prefix),
                    BindingFlags.Static | BindingFlags.NonPublic);

            // 4. 应用补丁
            harmony.Patch(target, prefix: new HarmonyMethod(prefix));
            //Debug.Log("[VoyageForge] TreeViewGUI.OnContentGUI 补丁安装成功");
        }

        // Prefix 方法：参数名与目标方法一致，label 要 ref 才能修改
        static void Prefix(
            Rect rect,
            int row,
            TreeViewItem item,
            ref string label,
            bool selected,
            bool focused,
            bool useBoldFont,
            bool isPinging)
        {
            // 高效过滤：只处理左侧文件夹树（基于类型检查）
            if (!IsLeftFolderTree(item))
                return;

            // 高效获取 Guid（使用缓存的 PropertyInfo/FieldInfo）
            string guid = GetGuidFromItem(item);

            // 现在你可以使用 item, label, guid
            //Debug.Log($"[FolderTree] Row:{row}, Label:{label}, Guid:{guid}, ID:{item?.id}, Depth:{item?.depth}");

            if (string.IsNullOrEmpty(guid))
                return;

            // 示例：替换 label（使用 guid 查询别名）
            if (!string.IsNullOrEmpty(label) && AliasDatabase.TryGetAlias(guid, out var alias))
            {
                label = alias;
                // Debug.Log($"[Alias Replace] {label} => {alias}");
            }
        }

        // --------------------------------------------
        // 高性能判断：不生成任何字符串，不产生 GC
        // --------------------------------------------
        private static bool IsLeftFolderTree(TreeViewItem item)
        {
            if (item == null)
                return false;

            // 延迟初始化（只执行一次反射）
            if (!s_Initialized)
            {
                s_FolderTreeItemType = AccessTools.TypeByName(
                    "UnityEditor.AssetsTreeViewDataSource+FolderTreeItem");
                s_Initialized = true;
            }

            // 如果连 FolderTreeItem 类型都找不到，降级为类型名包含判断（仍然比堆栈好得多）
            if (s_FolderTreeItemType == null)
                return item.GetType().FullName?.Contains("FolderTreeItem") == true;

            // 类型匹配（最快，相当于 is 操作）
            return s_FolderTreeItemType.IsInstanceOfType(item);
        }

        // 高效获取 Guid（缓存 PropertyInfo/FieldInfo）
        private static string GetGuidFromItem(TreeViewItem item)
        {
            if (item == null)
                return null;

            // 尝试从属性获取（优先，因为 Guid 在 FolderTreeItem 中是属性）
            if (s_GuidProperty == null && s_GuidField == null)
            {
                var type = item.GetType();
                s_GuidProperty = type.GetProperty("Guid",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (s_GuidProperty == null)
                {
                    s_GuidField = type.GetField("Guid",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }

            if (s_GuidProperty != null)
                return s_GuidProperty.GetValue(item) as string;

            if (s_GuidField != null)
                return s_GuidField.GetValue(item) as string;

            return null;
        }
    }
}