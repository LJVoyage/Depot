using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    /// Alias 数据库
    ///
    /// 职责:
    ///
    /// 1.
    /// 加载 ProjectSettings 配置
    ///
    /// 2.
    /// GUID 查询 Alias
    ///
    /// 3.
    /// 提供给 Harmony Patch 使用
    ///
    ///
    /// 调用:
    ///
    /// ProjectBrowser
    ///
    ///     |
    ///     |
    ///     v
    ///
    /// GUIContent.Temp
    ///
    ///     |
    ///     |
    ///     v
    ///
    /// AliasDatabase.GetAlias()
    ///
    /// </summary>
    public static class AliasDatabase
    {
        private static Dictionary<string, string> map;
        
        private static bool initialized;

        private const string ConfigPath = "ProjectSettings/VoyageForge/ProjectBrowserAlias.json";
        
        /// <summary>
        /// 初始化数据库
        /// </summary>
        public static void Initialize()
        {
            if (initialized)
                return;
            
            initialized = true;

            map =  new Dictionary<string, string>();

            Load();
        }


        /// <summary>
        /// 加载配置文件
        ///
        /// 使用 Unity JsonUtility
        ///
        /// 原因:
        ///
        /// Editor 环境稳定
        /// 不依赖第三方 Json
        ///
        /// </summary>
        private static void Load()
        {
            string absolute =System.IO.Path.Combine(
                    Application.dataPath,"..",ConfigPath
                );


            if (!System.IO.File.Exists(absolute))
            {
                Debug.Log("[VoyageForge Alias] Config Not Found\n" + absolute);


                return;
            }


            string json = System.IO.File.ReadAllText(absolute);

            ProjectBrowserAliasConfig config = JsonUtility.FromJson<ProjectBrowserAliasConfig>(
                json
            );


            if (config == null || config.aliases == null)
            {
                return;
            }


            foreach (AliasData item in config.aliases)
            {
                if (string.IsNullOrEmpty(item.guid) || string.IsNullOrEmpty(item.alias))
                    continue;

                map[item.guid] = item.alias;
            }


            Debug.Log("[VoyageForge Alias] Loaded : " + map.Count);
        }


        /// <summary>
        /// 根据 GUID 获取别名
        ///
        /// </summary>
        public static bool TryGetAlias(string guid, out string alias)
        {
            Initialize();

            return map.TryGetValue(guid, out alias);
        }


        /// <summary>
        /// 添加或者修改 Alias
        ///
        /// Editor Window 会调用
        ///
        /// </summary>
        public static void SetAlias(string guid, string alias)
        {
            Initialize();

            map[guid] = alias;

            Save();
        }


        /// <summary>
        /// 保存配置
        ///
        /// </summary>
        private static void Save()
        {
            ProjectBrowserAliasConfig config = new ProjectBrowserAliasConfig();

            foreach (var pair in map)
            {
                config.aliases.Add(new AliasData(pair.Key, pair.Value));
            }

            string json = JsonUtility.ToJson(config, true);

            string absolute = System.IO.Path.Combine(
                Application.dataPath, "..", ConfigPath
            );

            string dir = System.IO.Path.GetDirectoryName(absolute);

            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }


            System.IO.File.WriteAllText(absolute, json);

            AssetDatabase.Refresh();
        }
    }
}