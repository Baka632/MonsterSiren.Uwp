namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

public abstract class MsrMenuFlyoutBase : MenuFlyout
{
    /// <summary>
    /// 此事件等效于 <see cref="FlyoutBase.Opening"/> 事件。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 此事件模拟了 <see cref="FrameworkElement.Loading"/> 事件，用于支持 <c>x:Bind</c> 编译时绑定。
    /// </para>
    /// <para>
    /// 不同点在于引发事件时，事件参数 sender 始终为 <see langword="null"/>，因为本类不是 <see cref="FrameworkElement"/> 的派生类。
    /// </para>
    /// </remarks>
    public event TypedEventHandler<FrameworkElement, object> Loading;

    /// <summary>
    /// 构造 <see cref="MsrMenuFlyoutBase"/> 的新实例。
    /// </summary>
    public MsrMenuFlyoutBase()
    {
        Opening += OnOpening;
    }

    /// <summary>
    /// 提供通用命令的 <see cref="CommonResourcesViewModel"/>。
    /// </summary>
    public CommonResourcesViewModel ViewModel { get; } = CommonResourcesViewModel.Shared;

    public Visibility FavoriteVisibility
    {
        get => (Visibility)GetValue(FavoriteVisibilityProperty);
        set => SetValue(FavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty FavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(FavoriteVisibility), typeof(Visibility), typeof(MsrMenuFlyoutBase), new PropertyMetadata(Visibility.Collapsed));

    public Visibility UnfavoriteVisibility
    {
        get => (Visibility)GetValue(UnfavoriteVisibilityProperty);
        set => SetValue(UnfavoriteVisibilityProperty, value);
    }

    public static readonly DependencyProperty UnfavoriteVisibilityProperty =
        DependencyProperty.Register(nameof(UnfavoriteVisibility), typeof(Visibility), typeof(MsrMenuFlyoutBase), new PropertyMetadata(Visibility.Collapsed));

    public object SourceData
    {
        get => GetValue(SourceDataProperty);
        set => SetValue(SourceDataProperty, value);
    }

    public static readonly DependencyProperty SourceDataProperty =
        DependencyProperty.Register(nameof(SourceData), typeof(object), typeof(MsrMenuFlyoutBase), new PropertyMetadata(null, OnSourceDataPropertyChangedHandler));

    private static void OnSourceDataPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MsrMenuFlyoutBase flyout = (MsrMenuFlyoutBase)d;
        flyout.OnSourceDataPropertyChanged(d, e);
    }

    protected virtual void OnSourceDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    private void OnOpening(object sender, object e)
    {
        // 模拟 FrameworkElement 的 Loading 事件。
        // 用于 x:Bind，毕竟生成的 x:Bind 也不会用这个 sender 参数。
        Loading?.Invoke(null, e);
    }
}
