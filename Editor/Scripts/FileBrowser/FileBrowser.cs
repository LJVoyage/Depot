using System;
using UnityEditor;
using VoyageForge.Depot.Editor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace VoyageForge.Depot.Editor.ProjectBrowser
{
    public class FileBrowser : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _visualTreeAsset;

        private Image previewImage;

        [MenuItem("Tools/文件浏览器器")]
        public static void ShowWindow()
        {
            var window = GetWindow<FileBrowser>("文件浏览器");
            window.minSize = new Vector2(350, 250);
            window.Show();
        }

        private void CreateGUI()
        {
            _visualTreeAsset.InstantiateWithFillAndAddTo(rootVisualElement);
        }
    }
}