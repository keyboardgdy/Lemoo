# 故障排除指南

## 问题：程序启动失败，数据库中User表已存在

### 问题描述

当数据库已存在但表结构是旧版本时，程序启动可能会失败，错误信息可能包括：
- "数据库迁移失败"
- "表结构与迁移不匹配"
- "无法应用迁移"

### 原因分析

1. **数据库已存在但迁移历史表不存在**
   - 数据库是通过 `EnsureCreatedAsync()` 创建的
   - 没有迁移历史表（`__EFMigrationsHistory`）
   - EF Core 无法判断迁移状态

2. **表结构与迁移不匹配**
   - 数据库表缺少新字段（如 Name, PhoneNumber, Email, AvatarUrl）
   - 迁移系统无法自动更新表结构

### 解决方案

#### 方案一：手动应用迁移（推荐）

在项目根目录执行：

```bash
# 应用迁移到数据库
dotnet ef database update -p Lemoo.Infrastructure
```

这会：
- 创建迁移历史表（如果不存在）
- 应用所有待处理的迁移
- 更新表结构以匹配当前模型

#### 方案二：删除并重建数据库（开发环境）

**警告**：此操作会删除所有数据！

```sql
-- 在 SQL Server Management Studio 中执行
DROP DATABASE LemooDb;
```

然后重新运行应用程序，数据库会自动创建。

#### 方案三：手动添加缺失字段（如果数据重要）

如果数据库中有重要数据，可以手动添加缺失的字段：

```sql
-- 在 SQL Server Management Studio 中执行
USE LemooDb;

-- 添加 Name 字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Name')
BEGIN
    ALTER TABLE Users ADD Name NVARCHAR(100) NULL;
END

-- 添加 PhoneNumber 字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Users ADD PhoneNumber NVARCHAR(20) NULL;
    CREATE UNIQUE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber) WHERE PhoneNumber IS NOT NULL;
END

-- 添加 Email 字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Email')
BEGIN
    ALTER TABLE Users ADD Email NVARCHAR(200) NULL;
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email) WHERE Email IS NOT NULL;
END

-- 添加 AvatarUrl 字段
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'AvatarUrl')
BEGIN
    ALTER TABLE Users ADD AvatarUrl NVARCHAR(500) NULL;
END

-- 创建迁移历史表（如果不存在）
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] NVARCHAR(150) NOT NULL,
        [ProductVersion] NVARCHAR(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- 插入迁移记录（标记迁移已应用）
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251230025139_InitialCreate')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251230025139_InitialCreate', '10.0.1');
END
```

### 验证修复

修复后，运行应用程序应该能够正常启动。可以通过以下方式验证：

1. **检查迁移状态**
   ```bash
   dotnet ef migrations list -p Lemoo.Infrastructure
   ```
   应该显示所有迁移都已应用。

2. **检查表结构**
   ```sql
   -- 在 SQL Server Management Studio 中执行
   SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = 'Users'
   ORDER BY ORDINAL_POSITION;
   ```
   应该包含所有字段：Id, Username, PasswordHash, Name, PhoneNumber, Email, AvatarUrl, CreatedAt, LastLoginAt, IsEnabled

3. **检查索引**
   ```sql
   -- 在 SQL Server Management Studio 中执行
   SELECT i.name AS IndexName, c.name AS ColumnName
   FROM sys.indexes i
   INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
   INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
   WHERE i.object_id = OBJECT_ID('Users')
   ORDER BY i.name, ic.key_ordinal;
   ```
   应该包含索引：IX_Users_Username, IX_Users_Email, IX_Users_PhoneNumber

### 预防措施

为了避免将来出现类似问题：

1. **始终使用迁移**：不要使用 `EnsureCreatedAsync()` 在生产环境
2. **版本控制迁移文件**：将迁移文件提交到 Git
3. **测试迁移**：在开发/测试环境先测试迁移
4. **备份数据**：在生产环境应用迁移前备份数据库

### 常见错误信息

#### 错误：无法应用迁移，因为表已存在

**解决方案**：使用方案一（手动应用迁移）

#### 错误：列已存在

**解决方案**：使用方案三（手动添加字段）或方案二（删除重建）

#### 错误：迁移历史表不存在

**解决方案**：使用方案一（手动应用迁移），迁移系统会自动创建历史表

### 获取帮助

如果以上方案都无法解决问题，请：

1. 检查错误日志（Debug 输出窗口）
2. 查看详细的异常堆栈跟踪
3. 确认数据库连接字符串正确
4. 确认 SQL Server 服务正在运行

