using System;
using System.Reflection;
using HarmonyLib;
using UnityEditor;
using UnityEngine;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    /// Hook:
    ///
    /// UnityEngine.GUIContent.Temp(string)
    ///
    ///
    /// Unity 内部绘制:
    ///
    /// GUIStyle.Draw
    ///
    /// 会调用:
    ///
    /// GUIContent.Temp(label)
    ///
    ///
    /// 我们在这里替换文字。
    ///
    ///
    /// 注意：
    ///
    /// 参数名字不能写:
    ///
    /// ref string text
    ///
    ///
    /// 因为 Unity:
    ///
    /// static GUIContent Temp(string t)
    ///
    /// 参数名称:
    ///
    /// t
    ///
    ///
    /// Harmony 默认根据名字绑定。
    ///
    /// 所以必须:
    ///
    /// string t
    ///
    /// 或者使用 Harmony Argument
    ///
    /// </summary>
    public static class GUIContentTempPatch
    {
        public static void Install(Harmony harmony)
        {
            
            // 1. 检查 GUI 是否就绪
            if (!HarmonyInstaller.IsGUIAvailable())
            {
                EditorApplication.delayCall += () => Install(harmony);
                return;
            }
            
            MethodInfo target = typeof(GUIContent).GetMethod(
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
                Debug.LogError("[VoyageForge Alias] GUIContent.Temp(string) Not Found");
                return;
            }


            MethodInfo prefix = typeof(GUIContentTempPatch)
                    .GetMethod(
                        nameof(Prefix),
                        BindingFlags.Static |
                        BindingFlags.NonPublic
                    );


            harmony.Patch(target, prefix: new HarmonyMethod(prefix));

            //Debug.Log("[VoyageForge Alias] GUIContent.Temp Patch OK");
        }


        /// <summary>
        /// Prefix
        /// 执行顺序:
        /// Unity: GUIContent.Temp("LoginPanel")进入这里
        /// 修改参数:LoginPanel =>登录界面 返回 Unity创建 GUIContent
        ///
        /// </summary>
        static void Prefix(ref string t)
        {
            if (string.IsNullOrEmpty(t))
                return;

          
            
            // 判断是否来自 ProjectBrowser
            if (!IsProjectBrowserCall())
                return;

            // Debug.Log("[Alias Check] " + t);


            if (AliasDatabase.TryGetAlias(t, out var alias))
            {
                t = alias;

              //  Debug.Log("[Alias Replace] " + t + " => " + alias);
            }
        }


        static bool IsProjectBrowserCall()
        {
            var stack = Environment.StackTrace;

            return stack.Contains("ObjectListArea") && stack.Contains("ProjectBrowser");
        }
        
        
    }
}