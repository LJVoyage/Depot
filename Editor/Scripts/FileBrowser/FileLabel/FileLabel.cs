using System.Collections.Generic;
using VoyageForge.Depot.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace VoyageForge.Depot.Editor.ProjectBrowser
{
    public sealed class FileLabel : VFVisualElement
    {
        private const string _fileLabelItemClassName = "file-label-item";

        private VisualElement _container;


        // ---------- UxmlFactory 和 UxmlTraits（支持 UI Builder 和 UXML 序列化） ----------
        public new class UxmlFactory : UxmlFactory<FileLabel, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        private readonly List<string> _labels = new()
        {
            "File",
            "Label",
        };

      


        public FileLabel()
        {
            style.alignItems = Align.FlexEnd;

            _container = TreeAsset.InstantiateWithFillAndAddTo(this);

            foreach (var label in _labels)
            {
                _container.Add(new LabelItem(label,OnItemClose));
            }
        }

        private void OnItemClose(VisualElement visualElement)
        {
            
        }
    }
}