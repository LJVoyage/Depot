using HarmonyLib;
using UnityEditor;
using UnityEngine;
using System.Reflection;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    [InitializeOnLoad]
    public static class ProjectBrowserAliasPatch
    {
        static Harmony harmony;


        static ProjectBrowserAliasPatch()
        {
            EditorApplication.delayCall += Install;
        }


        static void Install()
        {
            harmony = new Harmony("voyageforge.projectbrowser.alias");


            MethodInfo target = typeof(GUIContent).GetMethod(
                "Temp",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic,
                null,
                new[]
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


            MethodInfo prefix = typeof(ProjectBrowserAliasPatch)
                .GetMethod(
                    nameof(Prefix),
                    BindingFlags.Static |
                    BindingFlags.NonPublic
                );


            harmony.Patch(target, new HarmonyMethod(prefix));
        }


        static void Prefix(ref string t)
        {
           
            
            if (string.IsNullOrEmpty(t))
                return;

           
            
            string[] guids = AssetDatabase.FindAssets(t);

            foreach (var guid in guids)
            {
                if (ProjectBrowserAliasService.TryGetAlias(guid, out var alias))
                {
                    t = alias;

                    return;
                }
            }
        }
    }
}