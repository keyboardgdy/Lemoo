# 配置安全指南

## 概述

为了保护数据库连接字符串中的敏感信息（用户名和密码），本应用实现了多层配置安全机制。

## 配置优先级

配置按以下优先级加载（高优先级覆盖低优先级）：

1. **环境变量**（最高优先级）
2. **appsettings.{Environment}.json**（开发环境）
3. **appsettings.json**（默认配置）

## 使用方法

### 方法一：使用环境变量（推荐 - 生产环境）

#### Windows PowerShell
```powershell
# 设置数据库用户名
$env:LEMOO_DB_USERID = "sa"

# 设置数据库密码
$env:LEMOO_DB_PASSWORD = "newu"

# 运行应用程序
.\Lemoo.App.exe
```

#### Windows CMD
```cmd
set LEMOO_DB_USERID=sa
set LEMOO_DB_PASSWORD=newu
Lemoo.App.exe
```

#### 永久设置（系统环境变量）
1. 打开"系统属性" → "高级" → "环境变量"
2. 在"用户变量"或"系统变量"中添加：
   - `LEMOO_DB_USERID` = `sa`
   - `LEMOO_DB_PASSWORD` = `newu`

### 方法二：使用配置文件（开发环境）

#### appsettings.json（默认配置）
```json
{
  "Database": {
    "Server": "localhost",
    "Database": "LemooDb",
    "TrustServerCertificate": true,
    "UserId": "",
    "Password": ""
  }
}
```

**注意**：生产环境不应在此文件中存储真实的用户名和密码。

#### appsettings.Development.json（开发环境配置）

**重要**：此文件已添加到 `.gitignore`，不会被提交到版本控制系统。

首次使用时，请复制示例文件：
```bash
# 复制示例文件
copy appsettings.Development.json.example appsettings.Development.json
```

然后编辑 `appsettings.Development.json`，填入你的数据库凭据：
```json
{
  "Database": {
    "Server": "localhost",
    "Database": "LemooDb",
    "TrustServerCertificate": true,
    "UserId": "sa",
    "Password": "newu"
  }
}
```

此文件仅在 DEBUG 模式下加载。

### 方法三：混合使用

- 非敏感信息（Server, Database）存储在配置文件中
- 敏感信息（UserId, Password）通过环境变量提供

## 配置文件说明

### appsettings.json
- **用途**：存储默认配置和非敏感信息
- **提交到 Git**：✅ 是
- **包含敏感信息**：❌ 否（UserId 和 Password 应为空）

### appsettings.Development.json
- **用途**：开发环境专用配置
- **提交到 Git**：⚠️ 可选（如果包含敏感信息，应添加到 .gitignore）
- **包含敏感信息**：⚠️ 可能（仅用于本地开发）

## 安全最佳实践

### ✅ 推荐做法

1. **生产环境使用环境变量**
   ```powershell
   $env:LEMOO_DB_USERID = "production_user"
   $env:LEMOO_DB_PASSWORD = "secure_password"
   ```

2. **开发环境使用 appsettings.Development.json**
   - 方便本地开发
   - 不提交到版本控制

3. **使用强密码**
   - 避免使用默认密码
   - 定期更换密码

4. **限制数据库用户权限**
   - 使用最小权限原则
   - 避免使用 sa 账户（生产环境）

### ❌ 避免做法

1. **不要在代码中硬编码密码**
   ```csharp
   // ❌ 错误示例
   var password = "newu";
   ```

2. **不要将敏感信息提交到 Git**
   - 确保 `.gitignore` 包含 `appsettings.Development.json`（如果包含敏感信息）

3. **不要在 appsettings.json 中存储生产环境密码**

## .gitignore 配置

`.gitignore` 已配置为忽略包含敏感信息的配置文件：

```
# Application configuration files with sensitive data
appsettings.Development.json
appsettings.*.json
!appsettings.json
!appsettings.*.example.json
```

这意味着：
- ✅ `appsettings.json` 会被提交（不包含敏感信息）
- ✅ `appsettings.*.example.json` 会被提交（作为模板）
- ❌ `appsettings.Development.json` 不会被提交（包含敏感信息）
- ❌ 其他环境特定的配置文件不会被提交

## 配置加载流程

```
1. 读取 appsettings.json
   ↓
2. 读取 appsettings.{Environment}.json（如果存在）
   ↓
3. 读取环境变量（覆盖配置文件中的值）
   ↓
4. 生成最终的 DatabaseConfiguration 对象
```

## 环境变量列表

| 变量名 | 说明 | 示例 |
|--------|------|------|
| `LEMOO_DB_USERID` | 数据库用户名 | `sa` |
| `LEMOO_DB_PASSWORD` | 数据库密码 | `newu` |
| `ASPNETCORE_ENVIRONMENT` | 环境名称 | `Development`, `Production` |

## 验证配置

启动应用程序时，检查 Debug 输出窗口，应该看到配置加载信息。如果配置不正确，应用程序会显示错误信息。

## 故障排除

### 问题：无法连接到数据库

**检查步骤**：
1. 确认环境变量已正确设置
2. 检查配置文件格式是否正确（JSON）
3. 验证数据库服务是否运行
4. 检查网络连接

### 问题：配置未生效

**解决方案**：
1. 确认环境变量名称正确（`LEMOO_DB_USERID`, `LEMOO_DB_PASSWORD`）
2. 重启应用程序（环境变量更改后需要重启）
3. 检查配置文件路径是否正确

## 示例：生产环境部署

### 方式一：通过环境变量（推荐）

```powershell
# 设置环境变量
$env:LEMOO_DB_USERID = "prod_user"
$env:LEMOO_DB_PASSWORD = "SecurePassword123!"

# 运行应用程序
.\Lemoo.App.exe
```

### 方式二：通过系统环境变量

1. 在服务器上设置系统环境变量
2. 重启应用程序
3. 应用程序自动读取环境变量

## 迁移指南

### 从硬编码配置迁移

**旧代码**：
```csharp
var dbConfig = new DatabaseConfiguration
{
    UserId = "sa",
    Password = "newu"
};
```

**新代码**：
```csharp
// 自动从配置和环境变量加载
var dbConfig = ConfigurationLoader.LoadDatabaseConfiguration(configuration);
```

无需修改其他代码，配置系统会自动处理。

