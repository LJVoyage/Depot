using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace VoyageForge.Depot.Editor
{
    public class PathCodeGenerator : EditorWindow
    {
        private DefaultAsset rootFolderAsset;
        private string rootFolder = "Assets";

        private DefaultAsset outputFolderAsset;
        private string outputDir = "Assets/Scripts/Generated";

        private string className = "AssetPaths";

        [MenuItem( "VoyageForge/Depot/Generate Asset Paths")]
        public static void ShowWindow() => GetWindow<PathCodeGenerator>("路径代码生成器");

        private void OnGUI()
        {
            GUILayout.Label("配置", EditorStyles.boldLabel);

            // 根目录 ObjectField
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("根目录", GUILayout.Width(60));
            DefaultAsset newRoot =
                (DefaultAsset)EditorGUILayout.ObjectField(rootFolderAsset, typeof(DefaultAsset), false);
            if (newRoot != rootFolderAsset)
            {
                rootFolderAsset = newRoot;
                if (rootFolderAsset != null)
                {
                    string path = AssetDatabase.GetAssetPath(rootFolderAsset);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        rootFolder = path;
                    }
                    else
                    {
                        string dir = Path.GetDirectoryName(path).Replace('\\', '/');
                        if (AssetDatabase.IsValidFolder(dir))
                        {
                            rootFolder = dir;
                            rootFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(dir);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("提示", "请选择文件夹，不能选择文件。", "确定");
                            rootFolderAsset = null;
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("  路径", rootFolder, EditorStyles.miniLabel);

            EditorGUILayout.Space(4);

            // 输出目录 ObjectField
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出目录", GUILayout.Width(60));
            DefaultAsset newOutput =
                (DefaultAsset)EditorGUILayout.ObjectField(outputFolderAsset, typeof(DefaultAsset), false);
            if (newOutput != outputFolderAsset)
            {
                outputFolderAsset = newOutput;
                if (outputFolderAsset != null)
                {
                    string path = AssetDatabase.GetAssetPath(outputFolderAsset);
                    if (AssetDatabase.IsValidFolder(path))
                    {
                        outputDir = path;
                    }
                    else
                    {
                        string dir = Path.GetDirectoryName(path).Replace('\\', '/');
                        if (AssetDatabase.IsValidFolder(dir))
                        {
                            outputDir = dir;
                            outputFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(dir);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("提示", "请选择文件夹，不能选择文件。", "确定");
                            outputFolderAsset = null;
                        }
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("  路径", outputDir, EditorStyles.miniLabel);

            EditorGUILayout.Space(4);

            className = EditorGUILayout.TextField("类名", className);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("生成代码", GUILayout.Height(30)))
            {
                GenerateCode();
            }
        }

        private void GenerateCode()
        {
            if (!AssetDatabase.IsValidFolder(rootFolder))
            {
                EditorUtility.DisplayDialog("错误", $"根目录无效: {rootFolder}", "确定");
                return;
            }

            // 确保输出目录存在
            string fullOutputPath = Path.Combine(Application.dataPath, outputDir.Replace("Assets/", ""));
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
                AssetDatabase.Refresh();
            }

            // 收集所有子文件夹（完整路径）
            var allFullPaths = new List<string>();
            GetSubFoldersRecursive(rootFolder, allFullPaths);

            if (allFullPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", $"在 '{rootFolder}' 下没有找到任何子文件夹。", "确定");
                return;
            }

            // 转换为相对路径（去掉 rootFolder 前缀）
            string rootNormalized = rootFolder.TrimEnd('/');
            var relativeDirs = allFullPaths
                .Select(p => p.Replace(rootNormalized, "").TrimStart('/'))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            // 生成模板文件（仅首次）
            string templatePath = Path.Combine(fullOutputPath, $"{className}.Base.template.cs");
            if (!File.Exists(templatePath))
            {
                File.WriteAllText(templatePath, GenerateTemplate(className, rootFolder));
            }

            // 生成主代码文件（每次覆盖）
            string mainPath = Path.Combine(fullOutputPath, $"{className}.g.cs");
            string mainContent = GenerateMainContent(relativeDirs, className);
            File.WriteAllText(mainPath, mainContent);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("成功", $"代码已生成到 {outputDir}\n根路径: {rootFolder}", "确定");
        }

        private void GetSubFoldersRecursive(string path, List<string> result)
        {
            string[] subFolders = AssetDatabase.GetSubFolders(path);
            foreach (var folder in subFolders)
            {
                result.Add(folder);
                GetSubFoldersRecursive(folder, result);
            }
        }

        private string GetFolderAlias(string folderPath)
        {
            folderPath = folderPath.Replace('\\', '/').Trim().TrimEnd('/');
            if (string.IsNullOrEmpty(folderPath)) return null;

            string guid = AssetDatabase.AssetPathToGUID(folderPath);
            if (string.IsNullOrEmpty(guid)) return null;

            if (ForgeMetaDatabase.TryGetNestedField(guid, ProjectBrowserAlias.AliasKey, out string alias))
                return alias;
            return null;
        }

        private string GenerateTemplate(string className, string rootPath)
        {
            return $@"// 此文件是自动生成的模板，请根据项目实际情况修改 Base 值
// 建议重命名为 {className}.Base.cs
public static partial class {className}
{{
    // 请修改为实际的根路径
    // 当前选择的根路径: {rootPath}
    public const string Base = ""{rootPath}/"";
}}";
        }

        private string GenerateMainContent(List<string> relativeDirectories, string className)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// 自动生成，请勿手动修改");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"public static partial class {className}");
            sb.AppendLine("{");
            sb.AppendLine($"    // 根路径 Base 定义在 {className}.Base.cs 中，请自行维护");
            sb.AppendLine();

            var dirParts = relativeDirectories.Select(d => d.Split('/').ToList()).ToList();
            var rootDirs = dirParts.Select(p => p.First()).Distinct().OrderBy(s => s).ToList();

            foreach (var rootDir in rootDirs)
            {
                string fullPath = $"{rootFolder.TrimEnd('/')}/{rootDir}";
                string alias = GetFolderAlias(fullPath);
                string safeRoot = Sanitize(rootDir);

                // 生成 XML 文档注释（别名）
                if (!string.IsNullOrEmpty(alias))
                {
                    sb.AppendLine($"    /// <summary>");
                    sb.AppendLine($"    /// {alias}");
                    sb.AppendLine($"    /// </summary>");
                }

                sb.AppendLine($"    public static class {safeRoot}");
                sb.AppendLine("    {");
                sb.AppendLine($"        public const string Base = {className}.Base + \"{rootDir}/\";");

                var subParts = dirParts.Where(p => p.First() == rootDir && p.Count > 1)
                    .Select(p => p.Skip(1).ToList()).ToList();
                GenerateSubClasses(sb, subParts, $"{className}.{safeRoot}", 1, fullPath);

                sb.AppendLine("    }");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        private void GenerateSubClasses(StringBuilder sb, List<List<string>> subPaths, string parentAccess, int depth,
            string parentFullPath)
        {
            var currentDirs = subPaths.Select(p => p.FirstOrDefault()).Where(s => !string.IsNullOrEmpty(s)).Distinct()
                .OrderBy(s => s).ToList();
            foreach (var dir in currentDirs)
            {
                string fullPath = $"{parentFullPath.TrimEnd('/')}/{dir}";
                string alias = GetFolderAlias(fullPath);
                string safeDir = Sanitize(dir);
                string indent = new string(' ', depth * 4);

                // 生成 XML 文档注释（别名）并添加缩进
                if (!string.IsNullOrEmpty(alias))
                {
                    sb.AppendLine($"{indent}/// <summary>");
                    sb.AppendLine($"{indent}/// {alias}");
                    sb.AppendLine($"{indent}/// </summary>");
                }

                sb.AppendLine($"{indent}public static class {safeDir}");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    public const string Base = {parentAccess}.Base + \"{dir}/\";");

                var deeper = subPaths.Where(p => p.First() == dir && p.Count > 1)
                    .Select(p => p.Skip(1).ToList()).ToList();
                if (deeper.Any())
                {
                    GenerateSubClasses(sb, deeper, $"{parentAccess}.{safeDir}", depth + 1, fullPath);
                }

                sb.AppendLine($"{indent}}}");
            }
        }

        private string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "_";
            foreach (char c in Path.GetInvalidFileNameChars()) input = input.Replace(c, '_');
            if (char.IsDigit(input[0])) input = "_" + input;
            return input.Replace(" ", "_").Replace("-", "_").Replace(".", "_");
        }
    }
}