﻿using Microsoft.UI.Xaml.Controls;

namespace MonsterSiren.Uwp.ViewModels;

/// <summary>
/// 为 <see cref="MainPage"/> 提供视图模型
/// </summary>
public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private bool _InfoBarOpen;
    [ObservableProperty]
    private string _InfoBarTitle = string.Empty;
    [ObservableProperty]
    private string _InfoBarMessage = string.Empty;
    [ObservableProperty]
    private InfoBarSeverity _InfoBarSeverity;

    public MainViewModel()
    {
        IsActive = true;
    }

    ~MainViewModel()
    {
        IsActive = false;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        WeakReferenceMessenger.Default.Register<ErrorInfo, string>(this, CommonValues.ApplicationErrorMessageToken, OnErrorMessageReceived);
    }

    private void OnErrorMessageReceived(object recipient, ErrorInfo message)
    {
        SetInfoBar(message.Title, message.Message, InfoBarSeverity.Error);
    }

    private void SetInfoBar(string title, string message, InfoBarSeverity severity)
    {
        InfoBarTitle = title;
        InfoBarMessage = message;
        InfoBarSeverity = severity;
        InfoBarOpen = true;
    }
}