using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace MonsterSiren.Uwp.Helpers.Tile;

/// <summary>
/// 为磁贴相关操作提供帮助的类。
/// </summary>
public static class TileHelper
{
    /// <summary>
    /// 使用指定的 <see cref="TileContent"/> 来显示磁贴。
    /// </summary>
    /// <param name="tileContent">表示磁贴的 <see cref="TileContent"/>。</param>
    public static void ShowTitle(TileContent tileContent)
    {
        TileNotification tileNotif = new(tileContent.GetXml());
        TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
    }

    /// <summary>
    /// 使用指定的 <see cref="TileNotification"/> 来显示磁贴。
    /// </summary>
    /// <param name="tileNotification">表示磁贴的 <see cref="TileNotification"/>。</param>
    public static void ShowTitle(TileNotification tileNotification)
    {
        TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
    }
    
    /// <summary>
    /// 使用指定的 <see cref="XmlDocument"/> 来显示磁贴。
    /// </summary>
    /// <param name="xmlDoc">表示磁贴的 <see cref="XmlDocument"/>。</param>
    public static void ShowTitle(XmlDocument xmlDoc)
    {
        TileNotification tileNotif = new(xmlDoc);
        TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
    }

    /// <summary>
    /// 删除磁贴。
    /// </summary>
    public static void DeleteTile()
    {
        TileUpdateManager.CreateTileUpdaterForApplication().Clear();
    }
}
