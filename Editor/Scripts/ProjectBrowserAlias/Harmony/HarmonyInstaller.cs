using HarmonyLib;
using UnityEngine;


namespace VoyageForge.Depot.Editor
{
    /// <summary>
    /// Harmony 总安装入口
    ///
    /// 所有 Patch 从这里统一管理
    ///
    /// 好处：
    ///
    /// 1.
    /// 避免多个 Harmony 实例
    ///
    /// 2.
    /// 方便卸载
    ///
    /// 3.
    /// Unity 版本变化时只需要检查这里
    ///
    /// </summary>
    public static class HarmonyInstaller
    {
        private static Harmony harmony;
        
        private const string ID = "com.voyageforge.projectbrowseralias";

        public static void Install()
        {
            if (harmony != null)
                return;


            harmony = new Harmony(ID);

            GUIContentTempPatch.Install(harmony);

            GetCroppedLabelTextPatch.Install(harmony);

            DrawIconAndLabelPatch.Install(harmony);
            
            TreeViewGUI_OnContentGUI_Patch.Install(harmony);

            //Debug.Log("[VoyageForge Alias] Harmony Installed");
        }
        
        public static bool IsGUIAvailable()
        {
            try
            {
                var skin = GUI.skin; // 若不在 OnGUI 中会抛出异常
                return true;
            }
            catch
            {
                Debug.Log("Harmony install failed");
                return false;
            }
        }
    }
}