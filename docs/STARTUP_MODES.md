# 启动模式说明

Lemoo应用程序支持两种启动模式，以满足不同的使用场景。

## 启动模式类型

### 1. 快速启动模式（FastStart）

**特点：**
- ✅ 不初始化数据库种子数据
- ✅ 启动速度快
- ✅ 保证稳定运行
- ✅ 适合日常使用和生产环境

**行为：**
- 仅确保数据库已创建（如果不存在）
- 不执行种子数据初始化
- 跳过用户数据检查

**默认使用场景：**
- RELEASE模式编译（生产环境）

### 2. 初始化模式（Initialize）

**特点：**
- ✅ 初始化数据库并更新种子数据
- ✅ 创建默认管理员用户（如果需要）
- ✅ 适合首次部署或需要更新种子数据时使用

**行为：**
- 确保数据库已创建
- 检查并初始化种子数据
- 如果用户表为空，创建默认管理员用户（用户名：sa，密码：newu）

**默认使用场景：**
- DEBUG模式编译（Visual Studio设计阶段和调试时）
- 开发环境

## 默认行为

### DEBUG模式（开发环境）
- **默认模式**：`Initialize`（初始化模式）
- **原因**：方便开发时自动更新种子数据，无需手动指定参数
- **适用场景**：Visual Studio设计阶段、调试运行、开发测试

### RELEASE模式（生产环境）
- **默认模式**：`FastStart`（快速启动模式）
- **原因**：保证启动速度和稳定运行
- **适用场景**：生产部署、日常使用

> **注意**：即使有默认模式，仍然可以通过命令行参数或环境变量覆盖默认行为。

## 使用方法

### 方法一：使用默认模式（推荐）

#### DEBUG模式（自动使用初始化模式）
```bash
# 在Visual Studio中按F5运行，自动使用Initialize模式
# 无需任何参数，自动初始化数据库和种子数据
```

#### RELEASE模式（自动使用快速启动模式）
```bash
# 发布版本自动使用FastStart模式
Lemoo.App.exe
```

### 方法二：命令行参数覆盖

#### 强制使用快速启动模式
```bash
# Windows
Lemoo.App.exe --fast
Lemoo.App.exe /fast
```

#### 强制使用初始化模式
```bash
# Windows
Lemoo.App.exe --init
Lemoo.App.exe --initialize
Lemoo.App.exe /init
```

### 方法二：环境变量

设置环境变量 `LEMOO_STARTUP_MODE`：

```bash
# Windows PowerShell
$env:LEMOO_STARTUP_MODE = "FastStart"
Lemoo.App.exe

# 或
$env:LEMOO_STARTUP_MODE = "Initialize"
Lemoo.App.exe
```

```bash
# Windows CMD
set LEMOO_STARTUP_MODE=FastStart
Lemoo.App.exe

# 或
set LEMOO_STARTUP_MODE=Initialize
Lemoo.App.exe
```

## 使用场景

### 快速启动模式适用于：
- 日常使用
- 生产环境
- 需要快速启动的场景
- 数据库已存在且数据完整的情况

### 初始化模式适用于：
- 首次部署应用
- 需要更新种子数据
- 数据库迁移后需要初始化数据
- 开发环境重置数据

## 技术实现

### 启动配置类
位置：`Lemoo.Infrastructure/Configuration/StartupConfiguration.cs`

```csharp
public enum StartupMode
{
    FastStart,      // 快速启动模式
    Initialize      // 初始化模式
}
```

### 数据库初始化服务
位置：`Lemoo.Infrastructure/Services/DatabaseInitializer.cs`

- `EnsureDatabaseCreatedAsync()`: 仅确保数据库已创建
- `InitializeAsync()`: 初始化数据库并更新种子数据
- `SeedDataAsync()`: 更新种子数据

### 启动流程

1. 解析命令行参数或环境变量
2. 创建启动配置对象
3. 注册服务时传入启动配置
4. 根据启动模式决定是否初始化数据库：
   - **快速启动模式**：调用 `EnsureDatabaseCreatedAsync()`
   - **初始化模式**：调用 `InitializeAsync()`

## 注意事项

1. **默认模式**：如果不指定任何参数，默认使用快速启动模式
2. **数据库连接**：确保SQL Server已启动并可访问
3. **种子数据**：初始化模式只在用户表为空时创建默认用户
4. **性能影响**：初始化模式会增加启动时间，但只在需要时使用

## 示例

### 开发环境首次运行
```bash
# 初始化数据库和种子数据
Lemoo.App.exe --init
```

### 生产环境日常使用
```bash
# 快速启动（默认）
Lemoo.App.exe
```

### 通过环境变量设置
```bash
# 设置环境变量后启动
set LEMOO_STARTUP_MODE=Initialize
Lemoo.App.exe
```

