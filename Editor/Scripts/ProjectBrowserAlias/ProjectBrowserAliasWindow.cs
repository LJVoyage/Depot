using UnityEditor;
using UnityEngine;


namespace VoyageForge.Depot.Editor
{
    /// <summary>
    /// VoyageForge ProjectBrowser Alias 编辑器窗口
    ///
    /// 使用:Unity Menu: VoyageForge|| Project Browser Alias
    ///
    /// 工作流程:
    ///
    /// 1.从 Project 窗口拖 Asset
    ///
    /// 2.AssetDatabase 获取:Assets/xxx.prefab
    ///
    /// 3.转换:Path=>GUID
    ///
    /// 4.保存:GUID=>Alias
    ///
    /// </summary>
    public class ProjectBrowserAliasWindow : EditorWindow
    {
        private Object targetAsset;
        
        private string alias;
        
        private string guid;
        
        private string path;

        [MenuItem( "VoyageForge/Project Browser Alias")]
        public static void Open()
        {
            ProjectBrowserAliasWindow window = GetWindow<ProjectBrowserAliasWindow>();

            window.titleContent =new GUIContent("Alias"  );
            
            window.Show();
        }


        private void OnGUI()
        {
            GUILayout.Space(10);

            GUILayout.Label(
                "VoyageForge Project Browser Alias",
                EditorStyles.boldLabel
            );

            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "拖入 Project 资源，然后设置显示别名。",
                MessageType.Info
            );

            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();

            targetAsset = EditorGUILayout.ObjectField(
                "Asset", 
                targetAsset, 
                typeof(Object),
                false
            );

            if (EditorGUI.EndChangeCheck())
            {
                RefreshAsset();
            }
            
            GUILayout.Space(10);

            EditorGUILayout.LabelField("Path", path);

            EditorGUILayout.LabelField("GUID", guid);

            GUILayout.Space(10);

            alias = EditorGUILayout.TextField("Alias", alias);

            GUILayout.Space(20);

            GUI.enabled = !string.IsNullOrEmpty(guid);

            if (GUILayout.Button("保存 Alias"))
            {
                Save();
            }

            GUI.enabled = true;
        }


        /// <summary>
        /// Asset 变化自动解析:Object=>Path =>GUID
        /// </summary>
        private void RefreshAsset()
        {
            guid = "";
            path = "";
            alias = "";

            if (targetAsset == null)
                return;

            path = AssetDatabase.GetAssetPath(targetAsset);

            if (string.IsNullOrEmpty(path))
                return;

            guid = AssetDatabase.AssetPathToGUID(path);

            if (ForgeMetaDatabase.TryGetNestedField(guid,ProjectBrowserAlias.AliasKey, out string oldAlias))
            {
                Debug.Log(oldAlias);
                alias = oldAlias;
            }
            else
            {
                alias = targetAsset.name;
            }
        }


        /// <summary>
        /// 保存配置
        /// </summary>
        private void Save()
        {
            if (string.IsNullOrEmpty(guid))
                return;


            if (string.IsNullOrEmpty(alias))
            {
                Debug.LogWarning("Alias 不能为空");
                return;
            }

            ForgeMetaDatabase.SetNestedField(guid, ProjectBrowserAlias.AliasKey, alias);

            Debug.Log("[VoyageForge Alias] Saved\n" + guid + "\n" + alias);

            Repaint();
        }
    }


    public static class ProjectBrowserAlias
    {
        public const string PackageName = "com.voyageforge.depot";
        
        public const string AliasKey = PackageName + ".Alias";
    }
}