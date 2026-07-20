using System;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{
    /// <summary>
    /// 单个资源别名数据
    ///
    /// 设计：
    /// Unity Asset 的唯一身份不是路径，而是 GUID。
    ///
    /// 例如：
    ///
    /// Assets/UI/Login/LoginPanel.prefab
    ///
    /// Unity:
    ///
    /// guid: a123456789xxxx
    ///
    /// 当文件移动：
    ///
    /// Assets/UI/Login/LoginPanel.prefab
    ///     |
    ///     v
    /// Assets/Game/UI/Login.prefab
    ///
    /// GUID 不变。
    ///
    /// 所以 Alias 使用 GUID 作为 Key。
    /// </summary>
    [Serializable]
    public class AliasData
    {
        /// <summary>
        /// Unity Asset GUID
        ///
        /// 例如:
        ///
        /// "a83d9e8xxxx"
        ///
        /// </summary>
        public string guid;


        /// <summary>
        /// 显示名称
        ///
        /// 例如:
        ///
        /// LoginPanel
        ///
        /// 修改为:
        ///
        /// 登录界面
        ///
        /// </summary>
        public string alias;


        public AliasData()
        {
        }


        public AliasData(string guid, string alias)
        {
            this.guid = guid;
            this.alias = alias;
        }
    }
}