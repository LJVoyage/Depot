using System.Collections.Generic;

namespace VoyageForge.Depot.Editor
{
    /// <summary>
    /// 表示一个资产的元数据模型，包含版本号、资产 GUID 和自定义字段（支持嵌套结构）。
    /// </summary>
    [System.Serializable]
    public class ForgeMetadata
    {
        /// <summary>元数据格式版本号，便于未来升级</summary>
        public int version = 1;

        /// <summary>对应资产的 Unity GUID，用于校验和追溯</summary>
        public string guid;

        /// <summary>自定义数据字段，值可以是字符串或嵌套字典，支持任意层级</summary>
        public Dictionary<string, object> fields = new Dictionary<string, object>();

        public ForgeMetadata() { }

        /// <param name="assetGuid">资产的 Unity GUID</param>
        /// <param name="initialFields">初始字段字典（可为空）</param>
        public ForgeMetadata(string assetGuid, Dictionary<string, object> initialFields = null)
        {
            guid = assetGuid;
            fields = initialFields ?? new Dictionary<string, object>();
        }
    }
}