using System.Net.Http;
using MonsterSiren.Uwp.Models.Abstracts;

namespace MonsterSiren.Uwp;

partial class CommonValues
{
    /// <summary>
    /// 将一个 <see cref="IFavoriteAddable"/> 添加到收藏夹中。
    /// </summary>
    /// <param name="favoriteAddable">一个 <see cref="IFavoriteAddable"/> 实例。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> AddToFavorite(IFavoriteAddable favoriteAddable)
    {
        try
        {
            ExceptionBox box = new();
            await favoriteAddable.AddToFavoriteAsync(box);
            box.Unbox();
            return true;
        }
        catch (HttpRequestException)
        {
            await DisplayInternetErrorDialog();
        }
        catch (AggregateException ex)
        {
            await DisplayAggregateExceptionErrorDialog(ex);
        }

        return false;
    }

    /// <summary>
    /// 从收藏夹中移除指定的 <see cref="IFavoriteAddable"/>。
    /// </summary>
    /// <param name="favoriteAddable">指定的 <see cref="IFavoriteAddable"/>。</param>
    /// <returns>指示操作是否成功的布尔值。</returns>
    public static async Task<bool> RemoveFromFavorite(IFavoriteAddable favoriteAddable)
    {
        await favoriteAddable.RemoveFromFavoriteAsync();
        return true;
    }
}
