# ProjectBrowserAlias

Unity Editor ProjectBrowser 中文显示别名工具。

## 简介

ProjectBrowserAlias 用于修改 Unity Project 窗口中的资源显示名称。

它不会修改：

* 文件名称
* 文件路径
* GUID
* `.meta` 文件
* AssetDatabase 数据

只修改 Unity Editor 绘制阶段的显示文本。

## 特性

* 保留英文资源名称
* 支持中文显示
* 不影响打包
* 不影响引用关系
* 不影响版本管理
* 不需要修改 Unity 源代码

## 原理

Unity ProjectBrowser 显示资源名称流程：

```
Asset
 |
 |
ObjectListArea
 |
 |
GUIContent.Temp(string)
 |
 |
GUIStyle.Draw
 |
 |
Editor GUI
```

本工具通过 Harmony Patch：

```
UnityEngine.GUIContent.Temp(string)
```

在 GUIContent 创建前修改文本。

例如：

真实文件：

```
Assets/Player/LoginPanel.prefab
```

Unity 内部名称：

```
LoginPanel
```

显示：

```
登录界面
```

但是实际路径保持：

```
Assets/Player/LoginPanel.prefab
```

## 使用方式

添加别名：

```csharp
ProjectBrowserAlias.Add(
    "LoginPanel",
    "登录界面"
);
```

多个资源：

```csharp
ProjectBrowserAlias.Add(
    "Player",
    "玩家系统"
);


ProjectBrowserAlias.Add(
    "Weapon",
    "武器系统"
);
```

删除：

```csharp
ProjectBrowserAlias.Remove(
    "LoginPanel"
);
```

清空：

```csharp
ProjectBrowserAlias.Clear();
```

## 数据建议

实际项目中建议使用 JSON：

例如：

```json
{
    "LoginPanel":"登录界面",
    "Player":"玩家系统",
    "Weapon":"武器系统"
}
```

Editor 启动时加载：

```
AliasDatabase.json
        |
        |
ProjectBrowserAlias.Add()
```

## 注意事项

### 1. 仅影响 Editor

不会影响：

* Player Build
* Runtime
* AssetBundle
* YooAsset
* Addressables

### 2. 不修改资源

不会执行：

```
AssetDatabase.RenameAsset()
```

所以不会产生：

* GUID变化
* 引用丢失

### 3. Unity版本

目前测试：

```
Unity 2022.3.x
```

不同 Unity 版本内部 GUI 调用可能变化。

## 未来扩展

计划支持：

* 文件夹别名
* 多语言切换
* 项目级配置
* ScriptableObject 配置
* EditorWindow 中文化
* Hierarchy 界面别名

## License

MIT
