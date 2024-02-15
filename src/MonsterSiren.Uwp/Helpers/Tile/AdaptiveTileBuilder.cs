using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;

namespace MonsterSiren.Uwp.Helpers.Tile;

/// <summary>
/// 自适应型磁贴生成器
/// </summary>
public sealed class AdaptiveTileBuilder
{
    private readonly TileContent tile;

    /// <summary>
    /// 构造 <see cref="AdaptiveTileBuilder"/> 的新实例
    /// </summary>
    public AdaptiveTileBuilder()
    {
        tile = new TileContent
        {
            Visual = new TileVisual()
            {
                TileSmall = new TileBinding() { Content = new TileBindingContentAdaptive() },
                TileMedium = new TileBinding() { Content = new TileBindingContentAdaptive() },
                TileLarge = new TileBinding() { Content = new TileBindingContentAdaptive() },
                TileWide = new TileBinding() { Content = new TileBindingContentAdaptive() },
            }
        };
    }

    /// <summary>
    /// 获取表示大型磁贴的 <see cref="TileBinding"/>
    /// </summary>
    public TileBinding TileLarge { get => tile.Visual.TileLarge; }
    /// <summary>
    /// 获取表示宽型磁贴的 <see cref="TileBinding"/>
    /// </summary>
    public TileBinding TileWide { get => tile.Visual.TileWide; }
    /// <summary>
    /// 获取表示中型磁贴的 <see cref="TileBinding"/>
    /// </summary>
    public TileBinding TileMedium { get => tile.Visual.TileMedium; }
    /// <summary>
    /// 获取表示小型磁贴的 <see cref="TileBinding"/>
    /// </summary>
    public TileBinding TileSmall { get => tile.Visual.TileSmall; }

    /// <summary>
    /// 配置所有磁贴的显示名称。注意：不同大小的磁贴可能会忽略这里的配置，设置自己的显示名称
    /// </summary>
    /// <param name="displayName">磁贴的显示名称</param>
    public AdaptiveTileBuilder ConfigureDisplayName(string displayName)
    {
        tile.Visual.DisplayName = displayName;
        return this;
    }

    /// <summary>
    /// 生成表示磁贴的 <see cref="TileContent"/>
    /// </summary>
    /// <returns>已配置好的 <see cref="TileContent"/></returns>
    public TileContent Build()
    {
        return tile;
    }

    /// <summary>
    /// 生成表示磁贴的 <see cref="XmlDocument"/>
    /// </summary>
    /// <returns>表示磁贴内容的 <see cref="XmlDocument"/></returns>
    public XmlDocument BuildXml()
    {
        return tile.GetXml();
    }
}
