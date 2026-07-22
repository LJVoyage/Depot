# VoyageForge ProjectBrowser Alias

Unity Editor Project 窗口资源别名系统。

---

# 1. 功能介绍


VoyageForge ProjectBrowser Alias 用于修改 Unity Project Browser 中显示的资源名称。


例如：

原始：

```
Assets/UI/Login/LoginPanel.prefab
```


Project 窗口显示：

```
LoginPanel
```


添加 Alias 后：

```
登录界面
```


实际文件：

```
Assets/UI/Login/LoginPanel.prefab
```


不会发生任何改变。



---

# 2. 设计目标


## 2.1 不修改 Asset


本工具不会：

- 修改文件名
- 修改路径
- 修改 meta
- 修改 GUID


只影响 Editor 显示。


---

## 2.2 使用 GUID 作为 Key


为什么不用路径？


错误方案：


```
Assets/UI/Login/LoginPanel.prefab
```


问题：

移动：

```
Assets/UI/Login/LoginPanel.prefab


移动到:


Assets/Game/UI/Login.prefab
```


路径改变。


Alias 失效。


---

正确：

Unity meta:

```
guid: a83d91xxxx
```


移动文件：

```
Assets/UI/Login/LoginPanel.prefab


↓

Assets/Game/UI/Login.prefab
```


GUID:

```
a83d91xxxx
```


保持不变。



所以：

```
GUID -> Alias
```


---

# 3. 系统结构


```
ProjectBrowserAliasWindow


        |
        |
        v


AssetDatabase


        |
        |
        v


GUID


        |
        |
        v


ProjectBrowserAlias.json



```



---

# 4. 配置文件


位置：

```
ProjectSettings/VoyageForge/ProjectBrowserAlias.json
```



示例：

```json
{
    "aliases":
    [
        {
            "guid":"a832xxxx",
            "alias":"登录界面"
        }
    ]
}
```


---

# 5. Harmony Patch 原理


Unity Project Browser 并不是直接读取 Asset 名称。


内部流程：


```
ProjectBrowser.OnGUI


        ↓


ObjectListArea.OnGUI


        ↓


ObjectListArea.HandleListArea


        ↓


ObjectListArea.Group.Draw


        ↓


ObjectListArea.LocalGroup.DrawInternal


        ↓


ObjectListArea.LocalGroup.DrawItem


        ↓


ObjectListArea.LocalGroup.DrawIconAndLabel


        ↓


GUIStyle.Draw


        ↓


GUIContent.Temp(string)


```


最终显示文字的位置：

```
GUIContent
```


---

# 6. 为什么修改 m_Content 无效


最初尝试：


```
ObjectListArea.SetupData


        ↓


m_Content.text = "xxx"

```


但是：


Unity 后续绘制：


```
DrawIconAndLabel


        ↓


GUIStyle.Draw


        ↓


GUIContent.Temp(label)

```


重新生成 GUIContent。


所以：

```
m_Content
```

只是缓存。


最终显示使用的是新的 GUIContent。


---

# 7. 为什么 Patch GUIContent.Temp


因为：

```
GUIContent.Temp(string)
```

是最终入口。



优点：

- 稳定
- 不依赖 ObjectListArea 内部字段
- Unity 小版本变化影响较小


缺点：

只有：

```
string
```


没有：

```
GUID
```


所以无法区分：

```
Assets/A/LoginPanel.prefab

Assets/B/LoginPanel.prefab

```


---

# 8. 最终方案 DrawIconAndLabel


最终 Hook：


```
ObjectListArea.LocalGroup.DrawIconAndLabel

```


原因：


这里拥有：

```
FilteredHierarchy.FilterResult
```


其中包含：

```
instanceID
```


转换：


```
instanceID


↓

UnityEngine.Object


↓

AssetDatabase.GetAssetPath


↓

AssetDatabase.AssetPathToGUID


↓

AliasDatabase


↓

显示 Alias

```



---

# 9. 如何推导 Unity 调用链


当 Unity 内部 API 不确定时。


## 第一步：找到目标结果


例如：

想修改 Project Browser 名称。


目标：

```
LoginPanel
```



---

## 第二步：搜索字符串来源


使用 Harmony Patch：


```
GUIContent.Temp(string)

```


打印：


```csharp
Environment.StackTrace
```



得到：


```
GUIContent.Temp


↓

GUIStyle.Draw


↓

ObjectListArea.LocalGroup.DrawIconAndLabel


↓

ObjectListArea.LocalGroup.DrawItem


↓

ObjectListArea.OnGUI

```


---

## 第三步：选择最靠近数据源的位置


比较：

|位置|数据|
|-|-|
|GUIContent.Temp|string|
|DrawIconAndLabel|instanceID|
|SetupData|缓存|


选择：

```
DrawIconAndLabel
```


---

# 10. Unity 升级后 Patch 失效怎么办？


Unity Editor API 是内部 API。


例如：

Unity 2022:

```
DrawIconAndLabel
```


Unity 2024:

可能：

```
DrawLabel
```


或者：

参数改变。


---

重新分析流程：

---

## 10.1 Dump 方法


创建：


```csharp
typeof(EditorWindow)
    .Assembly
    .GetType(
        "UnityEditor.ObjectListArea+LocalGroup"
    );
```


打印：

```
GetMethods()
```


寻找：

```
Draw
Label
Icon
Content
```



---

## 10.2 使用 StackTrace


重新 Hook：


```csharp
GUIContent.Temp
```


Prefix:

```csharp
Debug.Log(
    Environment.StackTrace
);
```



观察调用链。


---

## 10.3 使用 ILSpy


打开：

```
UnityEditor.dll
```


搜索：

```
DrawIconAndLabel
```


查看：


```csharp
void DrawIconAndLabel(...)
{
    GUIStyle.Draw(
        ...
    );
}

```


确认参数。


---

# 11. Harmony 参数变化


错误：


Unity:

```csharp
Temp(string t)
```


Patch:

```csharp
Prefix(ref string text)
```


结果：

```
Parameter "text" not found
```


原因：

Harmony 根据名字绑定。


正确：

```csharp
Prefix(ref string t)
```



或者：

```csharp
Prefix(object[] __args)
```


推荐：

内部 Unity API：

使用：

```
object[] __args
```


兼容性更高。



---

# 12. 常见错误


## GUIStyle 初始化错误


错误：

```
Unable to use named GUIStyle without current skin
```


原因：

Editor GUIStyle 必须在：

```
OnGUI
```


期间创建。



---

## Harmony 找不到方法


错误：

```
Method not found
```


检查：

```
BindingFlags
```


内部方法：

需要：

```csharp
BindingFlags.NonPublic
```


---

## Patch 参数错误


错误：

```
Parameter "xxx" not found
```


原因：

Unity 参数名字变化。


解决：

使用：

```
object[] __args
```


---

# 13. 后续扩展


可以继续增加：

## 分类


例如：

```
UI
 ├── 登录界面
 ├── 背包界面


Skill
 ├── 火球术

```



## 图标


AliasData:

增加：

```csharp
string iconGUID;
```



## 搜索增强


Project Browser 搜索：

```
登录
```


匹配：

```
LoginPanel
```



---

# 14. VoyageForge EditorTools


项目：

```
VoyageForge.EditorTools

```


包含：

- ProjectBrowserAlias
- AssetRegistry
- PrefabCollector
- UI Generator
- Code Generator


目标：

打造 Unity 工程生产工具链。

