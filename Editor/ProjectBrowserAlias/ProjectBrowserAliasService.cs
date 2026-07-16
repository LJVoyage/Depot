using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace VoyageForge.EditorTools
{
    public static class ProjectBrowserAliasService
    {
        static Dictionary<string, string> map;


        static ProjectBrowserAliasService()
        {
            Load();
        }


        public static void Load()
        {
            map =new Dictionary<string, string>();


            string[] guids =AssetDatabase.FindAssets("t:ProjectBrowserAliasDatabase");

          var path = AssetDatabase.GUIDToAssetPath(guids.First());
            
            var db =AssetDatabase.LoadAssetAtPath<ProjectBrowserAliasDatabase>(path);


            if (db == null)
                return;


            foreach (var item in db.items)
            {
                if (!map.ContainsKey(item.guid))
                {
                    map.Add(
                        item.guid,
                        item.alias
                    );
                }
            }
        }


        public static bool TryGetAlias(
            string guid,
            out string alias)
        {
            if (map == null)
                Load();


            return map.TryGetValue(
                guid,
                out alias
            );
        }
    }
}