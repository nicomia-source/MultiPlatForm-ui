# 平台切换系统使用说明

## 概述
这个系统使用观察者模式实现了一个灵活的平台切换功能，支持PC、PS5、Android、iOS四个平台。

## 核心组件

### 1. Platform 枚举
定义了四个支持的平台：
- PC
- PS5
- Android
- iOS

### 2. IPlatformObserver 接口
观察者接口，任何需要响应平台切换的类都应该实现这个接口。

### 3. PlatformManager 类
- 单例模式的平台管理器
- 管理当前平台状态
- 维护观察者列表
- 负责通知所有观察者平台变化

### 4. ChangePlatform 类
- 主要的UI控制器
- 处理Dropdown的选择事件
- 将用户选择传递给PlatformManager

### 5. PlatformDisplay 类
- 观察者示例实现
- 显示当前选择的平台

### 6. PlatformSpecificBehavior 类
- 高级观察者实现
- 根据平台切换UI元素和游戏对象的显示/隐藏
- 支持平台特定的按钮组和游戏对象数组
- 提供详细的平台信息显示

### 7. PlatformPerformanceSettings 类
- 性能优化观察者
- 根据不同平台自动调整画质设置和帧率
- PC：最高画质，60FPS
- 主机：高画质，60FPS
- 移动端：中等画质，30FPS

## Unity中的设置步骤

### 1. 创建UI
1. 在Scene中创建一个Canvas
2. 添加一个Dropdown组件
3. 可选：添加一个Text组件用于显示当前平台

### 2. 设置脚本
1. 将ChangePlatform脚本添加到一个GameObject上
2. 在Inspector中将Dropdown组件拖拽到platformDropdown字段
3. 如果要显示当前平台，将PlatformDisplay脚本添加到另一个GameObject上
4. 将Text组件拖拽到PlatformDisplay的displayText字段
5. 可选：添加PlatformSpecificBehavior脚本来控制平台特定的UI和对象
6. 可选：添加PlatformPerformanceSettings脚本来自动优化性能设置

### 3. 运行测试
- 运行场景
- 使用Dropdown选择不同平台
- 观察Console中的日志和Text显示的变化

## 扩展功能

### 添加新的观察者
```csharp
public class MyPlatformObserver : MonoBehaviour, IPlatformObserver
{
    private void Start()
    {
        PlatformManager.Instance.AddObserver(this);
    }
    
    private void OnDestroy()
    {
        if (PlatformManager.Instance != null)
        {
            PlatformManager.Instance.RemoveObserver(this);
        }
    }
    
    public void OnPlatformChanged(Platform newPlatform)
    {
        // 在这里实现你的平台切换逻辑
        switch(newPlatform)
        {
            case Platform.PC:
                // PC特定逻辑
                break;
            case Platform.PS5:
                // PS5特定逻辑
                break;
            // 其他平台...
        }
    }
}
```

## 设计模式优势

1. **松耦合**：观察者和被观察者之间松耦合，易于维护
2. **可扩展**：可以轻松添加新的观察者而不修改现有代码
3. **一致性**：所有观察者都会同时收到平台变化通知
4. **单一职责**：每个类都有明确的职责

## 高级功能

### PlatformSpecificBehavior 使用方法
1. 将脚本添加到GameObject上
2. 在Inspector中设置以下数组：
   - PC Only Buttons：仅在PC平台显示的按钮
   - Console Buttons：仅在主机平台显示的按钮
   - Mobile Buttons：仅在移动平台显示的按钮
   - Platform Info Text：显示平台详细信息的Text组件
   - 对应的游戏对象数组

### PlatformPerformanceSettings 使用方法
1. 将脚本添加到GameObject上
2. 在Inspector中可以调整各平台的目标帧率
3. 脚本会自动根据平台设置相应的画质等级

## 注意事项

1. 确保在OnDestroy中移除观察者，避免内存泄漏
2. PlatformManager使用单例模式，会在场景切换时保持存在
3. 观察者注册应该在Start()中进行，确保PlatformManager已经初始化
4. PlatformSpecificBehavior和PlatformPerformanceSettings位于PlatformSpecificBehavior.cs文件中
5. 所有观察者类都会在平台切换时自动收到通知并执行相应逻辑