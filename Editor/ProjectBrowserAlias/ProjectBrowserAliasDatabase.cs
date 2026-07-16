using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;


namespace VoyageForge.EditorTools
{
    public static class ProjectBrowserAliasDatabase
    {
        private const string FolderPath = "ProjectSettings/VoyageForge";

        private const string FilePath = FolderPath + "/ProjectBrowserAliasSettings.json";

        private static ProjectBrowserAliasConfig cache;

        /// <summary>
        /// 获取配置
        /// </summary>
        public static ProjectBrowserAliasConfig Config
        {
            get
            {
                if (cache == null)
                    Load();

                return cache;
            }
        }


        /// <summary>
        /// 加载 JSON
        /// </summary>
        public static void Load()
        {
            EnsureFolder();

            if (!File.Exists(FilePath))
            {
                cache = new ProjectBrowserAliasConfig();

                Save();

                return;
            }


            string json = File.ReadAllText(FilePath);


            cache = JsonUtility.FromJson<ProjectBrowserAliasConfig>(json);


            if (cache == null)
                cache =
                    new ProjectBrowserAliasConfig();
        }


        /// <summary>
        /// 保存配置
        /// </summary>
        public static void Save()
        {
            EnsureFolder();

            string json = JsonUtility.ToJson(cache, true);

            File.WriteAllText(FilePath, json);

            AssetDatabase.Refresh();
        }


        private static void EnsureFolder()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }


        /// <summary>
        /// 根据 GUID 获取 Alias
        /// </summary>
        public static string GetAlias(string guid)
        {
            var item = Config.items
                .FirstOrDefault(x => x.guid == guid);
            return item?.alias;
        }


        /// <summary>
        /// 添加或者修改 Alias
        /// </summary>
        public static void SetAlias(string guid, string path, string alias)
        {
            var item = Config.items.FirstOrDefault(x => x.guid == guid);


            if (item == null)
            {
                item = new ProjectBrowserAliasItem();
                Config.items.Add(item);
            }

            item.guid = guid;
            item.path = path;

            item.alias = alias;

            Save();
        }
    }
}