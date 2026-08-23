using System;
using UnityEngine;
using UnityEngine.Events;
using VoyageForge.Depot.Editor.Utilities;
using UnityEngine.UIElements;

namespace VoyageForge.Depot.Editor.ProjectBrowser
{
    public sealed class LabelItem : VFVisualElement
    {
        
        /// <summary>
        /// 激活时样式
        /// </summary>
        private const string _activeClassName = "label-item-active";
        
        /// <summary>
        /// 聚焦 时 样式
        /// </summary>
        private const string _hoverClassName = "label-item-hover";

        private readonly Label _label;

        public readonly Button _button;

        private VisualElement _labelActive;

        private readonly VisualElement _templateContainer;

        private readonly Action<VisualElement> _closeAction;

        public Action<VisualElement> ActiveAction;

        private bool _isActive = false;

        public LabelItem(string label, Action<VisualElement> CloseAction)
        {
            _templateContainer = TreeAsset.InstantiateWithFillAndAddTo(this);

            _closeAction = CloseAction;

            _templateContainer.AddToClassList("label-item");

            _label = this.Q<Label>("label");

            _label.text = label;

            _button = this.Q<Button>("button");

            RegisterCallback<ClickEvent>(OnClickActive);
            RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

            _button.RegisterCallback<ClickEvent>(OnClose);
        }

        private void OnClose(ClickEvent evt)
        {
            evt.StopPropagation();

            _closeAction?.Invoke(this);
        }

        /// <summary>
        /// 鼠标离开元素时调用
        /// </summary>
        /// <param name="evt"></param>
        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            Debug.Log("OnMouseLeave");
            _templateContainer.RemoveFromClassList(_hoverClassName);
        }

        /// <summary>
        /// 鼠标进入时调用
        /// </summary>
        /// <param name="evt"></param>
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            Debug.Log("OnMouseEnter");
            if (_isActive)
            {
                
            }
            else
            {
                _templateContainer.AddToClassList(_hoverClassName);
            }
        }


        /// <summary>
        /// 点击激活
        /// </summary>
        /// <param name="evt"></param>
        private void OnClickActive(ClickEvent evt)
        {
            _isActive  = !_isActive;
            if (_isActive)
            {
                _templateContainer.AddToClassList(_activeClassName);
                _templateContainer.RemoveFromClassList(_hoverClassName);
            }
            else
            {
                _templateContainer.RemoveFromClassList(_activeClassName);
            }
           
        }
    }
}