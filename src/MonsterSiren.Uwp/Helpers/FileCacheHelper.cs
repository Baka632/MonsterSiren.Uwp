using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MonsterSiren.Uwp.Helpers;

/// <summary>
/// 为应用程序数据提供文件中缓存的类
/// </summary>
internal sealed class FileCacheHelper
{
    private const string DefaultAlbumCoverCacheFolderName = "AlbumCover";
    private static readonly StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;

    /// <summary>
    /// 获取 <see cref="FileCacheHelper"/> 的默认实例
    /// </summary>
    public static FileCacheHelper Default { get; } = new FileCacheHelper();

    /// <summary>
    /// 使用指定的文件名与随机访问流，在专辑封面缓存文件夹创建文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="stream">专辑封面的随机访问流</param>
    public async Task StoreAlbumCoverAsync(string fileName, IRandomAccessStream stream)
    {
        StorageFolder coverFolder = await tempFolder.CreateFolderAsync(DefaultAlbumCoverCacheFolderName, CreationCollisionOption.OpenIfExists);

        StorageFile file = await coverFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        using IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
        await RandomAccessStream.CopyAsync(stream, fileStream);
        await fileStream.FlushAsync();
    }

    /// <summary>
    /// 使用指定的 <see cref="AlbumDetail"/> 实例，在专辑封面缓存文件夹创建专辑封面文件
    /// </summary>
    /// <param name="albumDetail">一个 <see cref="AlbumDetail"/> 实例</param>
    public async Task StoreAlbumCoverAsync(AlbumDetail albumDetail)
    {
        Uri coverUri = new(albumDetail.CoverUrl, UriKind.Absolute);

        RandomAccessStreamReference coverFile = RandomAccessStreamReference.CreateFromUri(coverUri);
        IRandomAccessStreamWithContentType stream = await coverFile.OpenReadAsync();
        string fileName = $"{albumDetail.Cid}.jpg";

        await StoreAlbumCoverAsync(fileName, stream);
    }

    /// <summary>
    /// 使用指定的 <see cref="AlbumInfo"/> 实例，在专辑封面缓存文件夹创建专辑封面文件
    /// </summary>
    /// <param name="albumInfo">一个 <see cref="AlbumInfo"/> 实例</param>
    public async Task StoreAlbumCoverAsync(AlbumInfo albumInfo)
    {
        Uri coverUri = new(albumInfo.CoverUrl, UriKind.Absolute);

        RandomAccessStreamReference coverFile = RandomAccessStreamReference.CreateFromUri(coverUri);
        IRandomAccessStreamWithContentType stream = await coverFile.OpenReadAsync();
        string fileName = $"{albumInfo.Cid}.jpg";

        await StoreAlbumCoverAsync(fileName, stream);
    }

    /// <summary>
    /// 通过指定的文件名获取专辑封面的随机访问流
    /// </summary>
    /// <param name="fileName">专辑封面的文件名</param>
    /// <returns>包含专辑封面数据的 <see cref="IRandomAccessStream"/></returns>
    public async Task<IRandomAccessStream> GetAlbumCoverStreamAsync(string fileName)
    {
        StorageFolder coverFolder = await tempFolder.CreateFolderAsync(DefaultAlbumCoverCacheFolderName, CreationCollisionOption.OpenIfExists);

        if (coverFolder != null)
        {
            StorageFile file = await coverFolder.GetFileAsync(fileName);
            return await file?.OpenReadAsync();
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumDetail"/> 实例获取专辑封面的随机访问流
    /// </summary>
    /// <param name="albumDetail"><see cref="AlbumDetail"/> 的实例</param>
    /// <returns>包含专辑封面数据的 <see cref="IRandomAccessStream"/></returns>
    public async Task<IRandomAccessStream> GetAlbumCoverStreamAsync(AlbumDetail albumDetail)
    {
        string fileName = $"{albumDetail.Cid}.jpg";
        return await GetAlbumCoverStreamAsync(fileName);
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumInfo"/> 实例获取专辑封面的随机访问流
    /// </summary>
    /// <param name="albumInfo"><see cref="AlbumInfo"/> 的实例</param>
    /// <returns>包含专辑封面数据的 <see cref="IRandomAccessStream"/></returns>
    public async Task<IRandomAccessStream> GetAlbumCoverStreamAsync(AlbumInfo albumInfo)
    {
        string fileName = $"{albumInfo.Cid}.jpg";
        return await GetAlbumCoverStreamAsync(fileName);
    }

    /// <summary>
    /// 通过指定的文件名获取指向专辑封面的 Uri
    /// </summary>
    /// <param name="fileName">专辑封面的文件名</param>
    /// <returns>指向专辑封面的 <see cref="Uri"/></returns>
    public async Task<Uri> GetAlbumCoverUriAsync(string fileName)
    {
        StorageFolder coverFolder = await tempFolder.CreateFolderAsync(DefaultAlbumCoverCacheFolderName, CreationCollisionOption.OpenIfExists);

        if (coverFolder != null && await coverFolder.FileExistsAsync(fileName))
        {
            return new Uri($"ms-appdata:///temp/{DefaultAlbumCoverCacheFolderName}/{fileName}", UriKind.Absolute);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumDetail"/> 实例获取指向专辑封面的 Uri
    /// </summary>
    /// <param name="albumDetail"><see cref="AlbumDetail"/> 的实例</param>
    /// <returns>指向专辑封面的 <see cref="Uri"/></returns>
    public async Task<Uri> GetAlbumCoverUriAsync(AlbumDetail albumDetail)
    {
        string fileName = $"{albumDetail.Cid}.jpg";
        return await GetAlbumCoverUriAsync(fileName);
    }

    /// <summary>
    /// 通过指定的 <see cref="AlbumInfo"/> 实例获取指向专辑封面的 Uri
    /// </summary>
    /// <param name="albumInfo"><see cref="AlbumInfo"/> 的实例</param>
    /// <returns>指向专辑封面的 <see cref="Uri"/></returns>
    public async Task<Uri> GetAlbumCoverUriAsync(AlbumInfo albumInfo)
    {
        string fileName = $"{albumInfo.Cid}.jpg";
        return await GetAlbumCoverUriAsync(fileName);
    }
}
