using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;


namespace VoyageForge.EditorTools
{
    public class ProjectBrowserAliasWindow : EditorWindow
    {
        Object targetObject;

        string alias;

        ProjectBrowserAliasDatabase database;

        string DB_PATH =
            "Assets/Editor/VoyageForge/ProjectBrowserAlias/ProjectBrowserAliasConfig.asset";


        [MenuItem("VoyageForge/Project Browser Alias")]
        static void Open()
        {
           
            
            GetWindow<ProjectBrowserAliasWindow>(
                "Alias"
            );
        }


        void OnEnable()
        {
            
            DB_PATH = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:ProjectBrowserAliasDatabase").First()) ;
            LoadDatabase();
        }


        void LoadDatabase()
        {
            database =AssetDatabase.LoadAssetAtPath <ProjectBrowserAliasDatabase>(DB_PATH);


            if (database == null)
            {
                database =CreateInstance<ProjectBrowserAliasDatabase>();


                string dir =Path.GetDirectoryName(DB_PATH);


                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }


                AssetDatabase.CreateAsset(
                    database,
                    DB_PATH
                );


                AssetDatabase.SaveAssets();
            }
        }


        void OnGUI()
        {
            GUILayout.Space(10);


            GUILayout.Label(
                "拖入资源添加 Alias",
                EditorStyles.boldLabel
            );


            GUILayout.Space(10);


            targetObject =
                EditorGUILayout.ObjectField(
                    "Asset",
                    targetObject,
                    typeof(Object),
                    false
                );


            alias =
                EditorGUILayout.TextField(
                    "Alias",
                    alias
                );


            GUILayout.Space(10);


            if (
                GUILayout.Button(
                    "Add Alias"
                )
            )
            {
                AddAlias();
            }


            GUILayout.Space(20);


            DrawList();
        }


        void AddAlias()
        {
            if (targetObject == null)
                return;


            string path =
                AssetDatabase.GetAssetPath(
                    targetObject
                );


            string guid =
                AssetDatabase.AssetPathToGUID(
                    path
                );


            var old =
                database.items.Find(x => x.guid == guid
                );


            if (old != null)
            {
                old.alias = alias;
            }
            else
            {
                database.items.Add(
                    new AliasItem()
                    {
                        guid = guid,
                        path = path,
                        alias = alias
                    }
                );
            }


            EditorUtility.SetDirty(
                database
            );


            AssetDatabase.SaveAssets();


            ProjectBrowserAliasService.Load();


            Debug.Log(
                $"Alias Added : {path} => {alias}"
            );
        }


        void DrawList()
        {
            GUILayout.Label(
                "Alias List",
                EditorStyles.boldLabel
            );


            foreach (var item in database.items)
            {
                GUILayout.BeginHorizontal();


                GUILayout.Label(
                    item.path
                );


                GUILayout.Label(
                    item.alias,
                    GUILayout.Width(120)
                );


                GUILayout.EndHorizontal();
            }
        }
    }
}