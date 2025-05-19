using System.Net.Http;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace MonsterSiren.Uwp;

partial class App
{

    private void RegisterBackgroundTask()
    {
        // qwq
    }

    protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
    {
        IBackgroundTaskInstance taskInstance = args.TaskInstance;
        BackgroundTaskDeferral taskDeferral = taskInstance.GetDeferral();

        taskInstance.Canceled += (instance, reason) => taskDeferral.Complete();

        if (taskInstance.TriggerDetails is AppServiceTriggerDetails details)
        {
            if (details.Name == CommonValues.CortanaAppService)
            {
                VoiceCommandServiceConnection voiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(details);

                try
                {
                    voiceServiceConnection.VoiceCommandCompleted += (s, e) => taskDeferral.Complete();

                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                    switch (voiceCommand.CommandName)
                    {
                        case "baka-eureka":
                            {
                                await SendBakaEurekaMessageToCortana();
                                break;
                            }
                        case "queryRecentAlbums":
                            {
                                await SendRecentAlbumsQueryToCortana();
                            }
                            break;
                        default:
                            await SendLaunchAppInForegroundMessageToCortana();
                            break;
                    }
                }
                finally
                {
                    taskDeferral.Complete();
                }

                #region Local Methods
                async Task SendLaunchAppInForegroundMessageToCortana()
                {
                    VoiceCommandUserMessage userMessage = new()
                    {
                        SpokenMessage = "CortanaService_OpenForegroundApp".GetLocalized()
                    };

                    VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage);
                    await voiceServiceConnection.RequestAppLaunchAsync(response);
                }

                async Task SendBakaEurekaMessageToCortana()
                {
                    VoiceCommandUserMessage userMessage = new()
                    {
                        DisplayMessage = "CortanaService_BakaEureka_DisplayMessage".GetLocalized(),
                        SpokenMessage = "CortanaService_BakaEureka_SpokenMessage".GetLocalized()
                    };

                    IEnumerable<VoiceCommandContentTile> destinationsContentTiles = [
                        new()
                        {
                            ContentTileType = VoiceCommandContentTileType.TitleWithText,
                            AppLaunchArgument = CommonValues.BakaEurekaAppLaunchArgument,
                            Title = "尤里卡~尤里卡~",
                            TextLine1 = "我所挚爱的你——Baka632",
                    }];

                    VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);
                    response.AppLaunchArgument = CommonValues.BakaEurekaAppLaunchArgument;

                    await voiceServiceConnection.ReportSuccessAsync(response);
                }

                async Task SendRecentAlbumsQueryToCortana()
                {
                    VoiceCommandUserMessage progressMessage = new();
                    progressMessage.DisplayMessage = progressMessage.SpokenMessage = "CortanaService_QueryRecentAlbums_Querying".GetLocalized();

                    VoiceCommandResponse progressResponse = VoiceCommandResponse.CreateResponse(progressMessage);
                    await voiceServiceConnection.ReportProgressAsync(progressResponse);

                    try
                    {
                        IEnumerable<AlbumInfo> albumInfos = (await CommonValues.GetOrFetchAlbums()).CollectionSource.AlbumInfos;
                        VoiceCommandUserMessage recentAlbumsMessage = new();
                        recentAlbumsMessage.SpokenMessage = recentAlbumsMessage.DisplayMessage = "CortanaService_QueryRecentAlbums_Success".GetLocalized();

                        int maxTiles = (int)VoiceCommandResponse.MaxSupportedVoiceCommandContentTiles;
                        List<VoiceCommandContentTile> albumTiles = new(maxTiles);

                        int i = 1;
                        foreach (AlbumInfo info in albumInfos)
                        {
                            if (i > maxTiles)
                            {
                                break;
                            }

                            //StorageFile imageFile;

                            //Uri fileCoverUri = await FileCacheHelper.GetAlbumCoverUriAsync(info);
                            //if (fileCoverUri is not null)
                            //{
                            //    imageFile = await StorageFile.GetFileFromApplicationUriAsync(fileCoverUri);
                            //}
                            //else
                            //{
                            //    Uri coverUri = new(info.CoverUrl, UriKind.Absolute);
                            //    IRandomAccessStreamReference thumbnail = RandomAccessStreamReference.CreateFromUri(coverUri);
                            //    imageFile = await StorageFile.CreateStreamedFileFromUriAsync($"{info.Cid}.jpg", coverUri, thumbnail);
                            //}

                            VoiceCommandContentTile tile = new()
                            {
                                AppContext = info,
                                Title = info.Name,
                                TextLine1 = string.Join(" / ", info.Artistes),
                                AppLaunchArgument = $"{CommonValues.AlbumAppLaunchArgumentHeader}={info.Cid}",
                                ContentTileType = VoiceCommandContentTileType.TitleWithText
                            };

                            albumTiles.Add(tile);
                            i++;
                        }

                        VoiceCommandUserMessage recentAlbumsRepeatMessage = new();
                        recentAlbumsRepeatMessage.DisplayMessage = recentAlbumsRepeatMessage.SpokenMessage = "CortanaService_QueryRecentAlbums_Success_Repeat".GetLocalized();

                        VoiceCommandResponse recentAlbumsResponse = VoiceCommandResponse.CreateResponseForPrompt(recentAlbumsMessage, recentAlbumsRepeatMessage, albumTiles);
                        VoiceCommandDisambiguationResult result = await voiceServiceConnection.RequestDisambiguationAsync(recentAlbumsResponse);

                        if (result != null)
                        {
                            AlbumInfo info = (AlbumInfo)result.SelectedItem.AppContext;

                            VoiceCommandUserMessage openForegroundMessage = new();
                            openForegroundMessage.SpokenMessage = openForegroundMessage.DisplayMessage = "CortanaService_OpenForegroundApp".GetLocalized();
                            VoiceCommandResponse openForegroundResponse = VoiceCommandResponse.CreateResponse(openForegroundMessage);
                            openForegroundResponse.AppLaunchArgument = $"{CommonValues.AlbumAppLaunchArgumentHeader}={info.Cid}";
                            
                            await voiceServiceConnection.RequestAppLaunchAsync(openForegroundResponse);
                        }
                    }
                    catch (HttpRequestException)
                    {
                        VoiceCommandUserMessage noInternetMessage = new();
                        noInternetMessage.DisplayMessage = noInternetMessage.SpokenMessage = "CortanaService_NoInternetMessage".GetLocalized();

                        VoiceCommandResponse errorResponse = VoiceCommandResponse.CreateResponse(noInternetMessage);
                        await voiceServiceConnection.ReportFailureAsync(errorResponse);
                    }
                    catch (ArgumentException argumentEx)
                    {
                        VoiceCommandUserMessage errorMessage = new();
                        errorMessage.DisplayMessage = errorMessage.SpokenMessage = argumentEx.Message;

                        VoiceCommandResponse errorResponse = VoiceCommandResponse.CreateResponse(errorMessage);
                        await voiceServiceConnection.ReportFailureAsync(errorResponse);
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(ex);
#endif
                        VoiceCommandUserMessage errorMessage = new();
                        errorMessage.DisplayMessage = errorMessage.SpokenMessage = "CortanaService_ErrorOccurredMessage".GetLocalized();

                        VoiceCommandResponse errorResponse = VoiceCommandResponse.CreateResponse(errorMessage);
                        await voiceServiceConnection.ReportFailureAsync(errorResponse);
                    }
                }
                #endregion
            }
            else
            {
                // In case of new app service

                //AppServiceConnection connection = details.AppServiceConnection;
                //connection.RequestReceived += ...;
                //connection.ServiceClosed += ...;
            }
        }
    }
}
