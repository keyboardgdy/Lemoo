using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lemoo.App.Models.Enums;
using Lemoo.App.Services;
using Lemoo.Application.Services;

namespace Lemoo.App.ViewModels;

/// <summary>
/// 登录视图模型
/// </summary>
public partial class LoginViewModel : BaseViewModel
{
    private readonly PasswordStorageService _passwordStorageService;
    private readonly IAuthenticationAppService _authenticationAppService;
    private int _loginFailureCount = 0;
    private const int MaxLoginAttempts = 5;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    /// <summary>
    /// 显示 Toast 消息事件
    /// </summary>
    public event EventHandler<(string Message, ToastType Type)>? ShowToast;

    [ObservableProperty]
    private bool _rememberPassword;

    [ObservableProperty]
    private bool _isPasswordVisible;

    /// <summary>
    /// 登录成功事件
    /// </summary>
    public event EventHandler? LoginSuccessful;

    /// <summary>
    /// 密码属性变化时的处理（用于同步到 PasswordBox）
    /// </summary>
    partial void OnPasswordChanged(string value)
    {
        // 当密码从外部设置时（如加载保存的凭据），触发事件通知 UI 更新
        PasswordChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 记住密码属性变化时的处理
    /// </summary>
    partial void OnRememberPasswordChanged(bool value)
    {
        // 如果取消记住密码，清除保存的凭据
        if (!value)
        {
            _passwordStorageService.ClearCredentials();
        }
    }

    /// <summary>
    /// 密码变化事件（用于通知 UI 更新 PasswordBox）
    /// </summary>
    public event EventHandler? PasswordChanged;

    public LoginViewModel(
        PasswordStorageService? passwordStorageService = null,
        IAuthenticationAppService? authenticationAppService = null)
    {
        _passwordStorageService = passwordStorageService ?? new PasswordStorageService();
        _authenticationAppService = authenticationAppService ?? throw new ArgumentNullException(nameof(authenticationAppService));
        LoadSavedCredentials();
    }

    /// <summary>
    /// 登录命令
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        // 清除之前的错误信息
        ErrorMessage = string.Empty;
        HasError = false;

        // 检查是否超过最大尝试次数
        if (_loginFailureCount >= MaxLoginAttempts)
        {
            ShowError($"登录失败次数过多，请稍后再试。");
            return;
        }

        // 简单的验证（实际应用中应该调用认证服务）
        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("请输入用户名");
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ShowError("请输入密码");
            return;
        }

        // 调用认证应用服务验证登录
        try
        {
            // 异步验证登录凭据
            var isValid = await _authenticationAppService.ValidateCredentialsAsync(Username, Password);
            
            if (isValid)
            {
                // 登录成功，重置失败计数
                _loginFailureCount = 0;

                // 更新最后登录时间
                await _authenticationAppService.UpdateLastLoginTimeAsync(Username);

                // 保存登录凭据（如果勾选了记住密码）
                _passwordStorageService.SaveCredentials(Username, Password, RememberPassword);

                // 登录成功，触发事件
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // 登录失败，增加失败计数
                _loginFailureCount++;
                var remainingAttempts = MaxLoginAttempts - _loginFailureCount;
                
                if (remainingAttempts > 0)
                {
                    ShowError($"用户名或密码错误，还有 {remainingAttempts} 次尝试机会。");
                }
                else
                {
                    ShowError("登录失败次数过多，请稍后再试。");
                }
            }
        }
        catch (Exception ex)
        {
            _loginFailureCount++;
            ShowError($"登录失败：{ex.Message}");
        }
    }


    /// <summary>
    /// 显示错误信息（使用 Toast）
    /// </summary>
    private void ShowError(string message)
    {
        // 触发 Toast 显示事件
        ShowToast?.Invoke(this, (message, ToastType.Error));
    }

    /// <summary>
    /// 加载保存的登录凭据
    /// </summary>
    private void LoadSavedCredentials()
    {
        try
        {
            var (savedUsername, savedPassword) = _passwordStorageService.LoadCredentials();
            if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
            {
                // 先设置用户名
                Username = savedUsername;
                // 再设置密码（这会触发 OnPasswordChanged，从而触发 PasswordChanged 事件）
                Password = savedPassword;
                // 最后设置记住密码标志
                RememberPassword = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载保存的凭据失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 切换密码可见性
    /// </summary>
    [RelayCommand]
    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    /// <summary>
    /// 清除保存的凭据
    /// </summary>
    public void ClearSavedCredentials()
    {
        _passwordStorageService.ClearCredentials();
    }
}

