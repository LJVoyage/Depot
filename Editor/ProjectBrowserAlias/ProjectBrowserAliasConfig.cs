using System;
using System.Collections.Generic;


namespace VoyageForge.EditorTools.ProjectBrowserAlias
{


    /// <summary>
    /// ProjectBrowser Alias 配置
    ///
    /// 对应文件:
    ///
    /// ProjectSettings/VoyageForge/ProjectBrowserAlias.json
    ///
    /// 内容:
    ///
    /// {
    ///     "aliases":
    ///     [
    ///         {
    ///             "guid":"xxxx",
    ///             "alias":"登录界面"
    ///         }
    ///     ]
    /// }
    ///
    /// </summary>
    [Serializable]
    public class ProjectBrowserAliasConfig
    {
        /// <summary>
        /// 所有别名数据
        /// </summary>
        public List<AliasData> aliases = new List<AliasData>();
        
    }

}