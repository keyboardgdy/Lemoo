# 配置文件说明

## 首次设置

### 1. 创建开发环境配置文件

复制示例文件并重命名：

```bash
# Windows PowerShell
Copy-Item appsettings.Development.json.example appsettings.Development.json

# Windows CMD
copy appsettings.Development.json.example appsettings.Development.json
```

### 2. 编辑配置文件

打开 `appsettings.Development.json`，填入你的数据库连接信息：

```json
{
  "Database": {
    "Server": "localhost",
    "Database": "LemooDb",
    "TrustServerCertificate": true,
    "UserId": "你的数据库用户名",
    "Password": "你的数据库密码"
  }
}
```

### 3. 验证配置

运行应用程序，检查是否能正常连接到数据库。

## 文件说明

| 文件 | 说明 | 是否提交到 Git |
|------|------|---------------|
| `appsettings.json` | 默认配置（不包含敏感信息） | ✅ 是 |
| `appsettings.Development.json.example` | 开发环境配置模板 | ✅ 是 |
| `appsettings.Development.json` | 开发环境配置（包含敏感信息） | ❌ 否 |

## 安全提示

⚠️ **重要**：`appsettings.Development.json` 已添加到 `.gitignore`，不会被提交到版本控制系统。

如果你已经将 `appsettings.Development.json` 提交到了 Git，需要：

1. 从 Git 历史中移除：
   ```bash
   git rm --cached Lemoo.App/appsettings.Development.json
   git commit -m "Remove appsettings.Development.json from version control"
   ```

2. 确保文件已添加到 `.gitignore`

3. 重新创建本地配置文件（参考上面的步骤）

## 使用环境变量（推荐）

生产环境建议使用环境变量而不是配置文件：

```powershell
$env:LEMOO_DB_USERID = "sa"
$env:LEMOO_DB_PASSWORD = "newu"
```

详细说明请参考 `docs/CONFIGURATION_SECURITY.md`。

