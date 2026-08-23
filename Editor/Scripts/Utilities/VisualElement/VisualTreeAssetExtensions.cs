using UnityEngine.UIElements;

namespace VoyageForge.Depot.Editor.Utilities
{
    public static class VisualTreeAssetExtensions
    {
        /// <summary>
        /// 实例化 VisualTreeAsset，并自动设置根元素的 flexGrow = 1，
        /// 使其在父容器中自动拉伸填充剩余空间。
        /// </summary>
        /// <param name="asset">要实例化的 VisualTreeAsset</param>
        /// <returns>实例化后的 VisualElement（根元素），其 style.flexGrow 已被设为 1</returns>
        public static  TemplateContainer InstantiateWithFill(this VisualTreeAsset asset)
        {
            var templateContainer = asset.Instantiate();
            return templateContainer;
        }

        /// <summary>
        /// 实例化并直接添加到父元素，同时设置 flexGrow = 1。
        /// </summary>
        /// <param name="asset">要实例化的 VisualTreeAsset</param>
        /// <param name="parent">父 VisualElement</param>
        /// <returns>实例化后的 VisualElement（已添加到父元素）</returns>
        public static  TemplateContainer InstantiateWithFillAndAddTo(this VisualTreeAsset asset, VisualElement parent)
        {
            var templateContainer = asset.InstantiateWithFill();
            parent.Add(templateContainer);
            return templateContainer;
        }
    }
}