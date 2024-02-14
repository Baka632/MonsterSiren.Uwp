namespace MonsterSiren.Api.Models.News;

/// <summary>
/// 表示新闻类型的枚举
/// </summary>
public enum NewsCategory
{
    /// <summary>
    /// 新歌发布
    /// </summary>
    NewSongs = 1,
    /// <summary>
    /// 上线致辞
    /// </summary>
    LaunchSpeech = 5,
    /// <summary>
    /// 资讯速递
    /// </summary>
    NewsAlert = 7,
    /// <summary>
    /// 艺人近况
    /// </summary>
    ArtistState = 8,
    /// <summary>
    /// 特别电台
    /// </summary>
    SpecialRadio = 11,
}
