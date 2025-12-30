# Interfaces 文件夹

此文件夹用于存放接口定义。

## 使用场景

- 服务接口（IService）
- 数据访问接口（IRepository）
- 业务逻辑接口（IUseCase）
- 其他通用接口定义

## 示例

```csharp
namespace Lemoo.App.Interfaces;

public interface INavigationService
{
    void NavigateTo(string pageKey);
    bool CanNavigateTo(string pageKey);
}
```

