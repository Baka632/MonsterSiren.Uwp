using System.Windows.Input;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

/// <summary>
/// 表示为创建“添加到”菜单项目提供帮助方法的 <see cref="MenuFlyout"/>。
/// </summary>
public abstract class WithAddToMenuFlyout : MenuFlyout
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
    /// 构造 <see cref="WithAddToMenuFlyoutBase"/> 的新实例。
    /// </summary>
    public WithAddToMenuFlyout()
    {
        Opening += OnOpening;
    }

    /// <summary>
    /// 提供通用命令的 <see cref="CommonResourcesViewModel"/>。
    /// </summary>
    public CommonResourcesViewModel ViewModel { get; } = CommonResourcesViewModel.Shared;

    /// <summary>
    /// 初始化“添加到”菜单项目。
    /// </summary>
    /// <param name="sourceData">用于创建菜单的数据源。</param>
    /// <param name="optionalModel">一个可选的 <see cref="Playlist"/>，用于防止播放列表添加自身。</param>
    /// <param name="addToNowPlayingCommandCallback">添加到正在播放命令结束后的回调命令。</param>
    /// <param name="playlistCommandCallback">添加到播放列表命令结束后的回调命令。</param>
    protected void InitializeAddToMenuFlyoutItem(object sourceData, Playlist optionalModel = null, ICommand addToNowPlayingCommandCallback = null, ICommand playlistCommandCallback = null)
    {
        MenuFlyoutItemBase target = Items.Single(static item => (string)item.Tag == "Placeholder_For_AddTo");

        int targetIndex = Items.IndexOf(target);
        Items.RemoveAt(targetIndex);

        MenuFlyoutSubItem subItem = CommonValues.CreateAddToFlyoutSubItem(ViewModel.AddToNowPlayingCommand,
                                                                          new CommandParameter(sourceData, addToNowPlayingCommandCallback),
                                                                          ViewModel.AddItemToPlaylistCommand,
                                                                          (Playlist playlist) => new CommandParameter((playlist, sourceData), playlistCommandCallback),
                                                                          optionalModel);
        subItem.Tag = "Placeholder_For_AddTo";
        Items.Insert(targetIndex, subItem);
    }

    private void OnOpening(object sender, object e)
    {
        // 模拟 FrameworkElement 的 Loading 事件。
        // 用于 x:Bind，毕竟生成的 x:Bind 也不会用这个 sender 参数。
        Loading?.Invoke(null, e);
    }
}
