using System.Windows.Input;
using MonsterSiren.Uwp.Models.Playlists;

namespace MonsterSiren.Uwp.Controls.MenuFlyouts;

/// <summary>
/// 表示为创建“添加到”菜单项目提供帮助方法的 <see cref="MenuFlyout"/>。
/// </summary>
public abstract class AddToMenuFlyoutBase : MsrMenuFlyoutBase
{
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
                                                                          playlist => new CommandParameter((playlist, sourceData), playlistCommandCallback),
                                                                          optionalModel);
        subItem.Tag = "Placeholder_For_AddTo";
        Items.Insert(targetIndex, subItem);
    }
}
