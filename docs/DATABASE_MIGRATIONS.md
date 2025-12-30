# 数据库迁移指南

## 问题说明

当修改实体类或数据模型配置后，使用 `EnsureCreatedAsync()` 方法不会更新已存在的数据库表结构。这是因为 `EnsureCreatedAsync()` 只在数据库不存在时创建数据库，如果数据库已存在，它不会更新表结构。

## 解决方案

使用 EF Core Migrations（迁移）来管理数据库架构变更。迁移会自动检测模型变更并生成相应的 SQL 脚本来更新数据库结构。

## 使用步骤

### 1. 安装 EF Core 工具（首次使用）

```bash
dotnet tool install --global dotnet-ef
```

### 2. 创建迁移

在项目根目录（包含 `.sln` 文件的目录）执行以下命令：

```bash
# 创建迁移（迁移名称：AddUserFields）
dotnet ef migrations add AddUserFields --project Lemoo.Infrastructure

# 或者使用简短名称
dotnet ef migrations add AddUserFields -p Lemoo.Infrastructure
```

**注意**：由于已创建 `LemooDbContextFactory`，可以直接使用 `--project` 参数，无需指定 `--startup-project`。

**迁移名称建议：**
- `AddUserFields` - 添加用户字段
- `UpdateUserTable` - 更新用户表
- `InitialCreate` - 初始创建（首次迁移）

### 2. 查看迁移文件

迁移文件会创建在 `Lemoo.Infrastructure/Migrations/` 目录下，包含：
- `[时间戳]_[迁移名称].cs` - 迁移类文件
- `[时间戳]_[迁移名称].Designer.cs` - 设计器文件

### 3. 应用迁移

#### 方式一：自动应用（推荐）

应用程序启动时会自动检测并应用待处理的迁移。在初始化模式下（DEBUG模式或使用 `--init` 参数），会自动应用迁移。

#### 方式二：手动应用

```bash
# 应用迁移到数据库
dotnet ef database update --project Lemoo.Infrastructure

# 或者使用简短参数
dotnet ef database update -p Lemoo.Infrastructure
```

### 4. 查看迁移状态

```bash
# 列出所有迁移
dotnet ef migrations list --project Lemoo.Infrastructure

# 查看待应用的迁移
dotnet ef migrations list --project Lemoo.Infrastructure --pending
```

## 迁移工作流程

### 开发环境（推荐流程）

1. **修改实体或配置**
   - 修改 `User.cs` 实体类
   - 修改 `LemooDbContext.cs` 中的模型配置

2. **创建迁移**
   ```bash
   dotnet ef migrations add AddUserFields -p Lemoo.Infrastructure
   ```

3. **检查迁移文件**
   - 查看生成的迁移文件，确认变更正确
   - 如有必要，手动编辑迁移文件

4. **应用迁移**
   - 在 DEBUG 模式下运行应用，会自动应用迁移
   - 或手动执行：`dotnet ef database update -p Lemoo.Infrastructure`

### 生产环境

1. **生成 SQL 脚本**（可选，用于审查）
   ```bash
   dotnet ef migrations script --project Lemoo.Infrastructure --startup-project Lemoo.App --output migration.sql
   ```

2. **应用迁移**
   - 应用程序启动时会自动应用迁移
   - 或手动执行：`dotnet ef database update -p Lemoo.Infrastructure`

## 常见问题

### Q: 迁移失败怎么办？

A: 如果迁移失败，可以：
1. 检查错误信息，修复问题
2. 删除失败的迁移文件
3. 重新创建迁移

### Q: 如何回滚迁移？

A: 回滚到上一个迁移：
```bash
dotnet ef database update [上一个迁移名称] -p Lemoo.Infrastructure
```

### Q: 如何删除迁移？

A: 如果迁移还未应用到数据库：
```bash
dotnet ef migrations remove --project Lemoo.Infrastructure
```

### Q: 迁移文件需要提交到版本控制吗？

A: **是的**，迁移文件应该提交到版本控制（Git），这样团队成员可以同步数据库结构变更。

## 注意事项

1. **备份数据**：在生产环境应用迁移前，务必备份数据库
2. **测试迁移**：在开发/测试环境先测试迁移
3. **迁移顺序**：迁移按时间戳顺序应用，不要手动修改迁移文件的时间戳
4. **数据丢失**：某些迁移操作可能导致数据丢失（如删除列），请谨慎操作

## 当前实现

应用程序已配置为：
- **自动检测迁移**：启动时检查是否有待应用的迁移
- **自动应用迁移**：在初始化模式下自动应用迁移
- **兼容性处理**：如果没有迁移，使用 `EnsureCreatedAsync()` 创建数据库（首次运行）

## 快速命令参考

```bash
# 创建迁移
dotnet ef migrations add [迁移名称] -p Lemoo.Infrastructure

# 应用迁移
dotnet ef database update -p Lemoo.Infrastructure

# 查看迁移列表
dotnet ef migrations list -p Lemoo.Infrastructure

# 删除迁移（未应用）
dotnet ef migrations remove -p Lemoo.Infrastructure

# 生成 SQL 脚本
dotnet ef migrations script -p Lemoo.Infrastructure -o migration.sql
```

## 已创建的迁移

✅ **InitialCreate** - 初始数据库创建，包含完整的用户表结构（包括 Name, PhoneNumber, Email, AvatarUrl 字段）

下次修改实体或配置时，只需创建新的迁移即可。

