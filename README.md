## Lemoo WPF 基础框架（CommunityToolkit.Mvvm）

项目结构示例：

- `Lemoo.App`
  - `App.xaml` / `App.xaml.cs`：应用入口
  - `Views`：界面（如 `MainWindow.xaml`）
  - `ViewModels`：视图模型（`BaseViewModel`、`MainViewModel` 等）
  - `Models`：领域模型
  - `Services`：应用服务（导航、日志、API 等）
  - `Core`：通用基础设施（常量、接口、扩展方法等）

已集成 `CommunityToolkit.Mvvm`，后续可以通过：
- `[ObservableProperty]` 快速声明可通知属性
- `[RelayCommand]` 快速声明命令

## 界面展示
<img width="1440" height="802" alt="image" src="https://github.com/user-attachments/assets/8a531f63-6ce8-4633-81f2-8dc9198a8d3b" />

