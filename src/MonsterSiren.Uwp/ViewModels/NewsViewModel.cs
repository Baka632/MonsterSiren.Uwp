using System.Net.Http;

namespace MonsterSiren.Uwp.ViewModels;

public sealed partial class NewsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isLoading = false;
    [ObservableProperty]
    private Visibility errorVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private ErrorInfo errorInfo;
    [ObservableProperty]
    private IEnumerable<RecommendedNewsInfo> recommendedNewsInfos;

    public async Task Initialize()
    {
        IsLoading = true;
        ErrorVisibility = Visibility.Collapsed;


        try
        {
            RecommendedNewsInfos = await NewsService.GetRecommendedNews();
            ErrorVisibility = Visibility.Collapsed;
        }
        catch (HttpRequestException ex)
        {
            ShowInternetError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowInternetError(HttpRequestException ex)
    {
        ErrorVisibility = Visibility.Visible;
        ErrorInfo = new ErrorInfo()
        {
            Title = "ErrorOccurred".GetLocalized(),
            Message = "InternetErrorMessage".GetLocalized(),
            Exception = ex
        };
    }
}