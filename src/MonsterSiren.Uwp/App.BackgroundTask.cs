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
                                SendBakaEurekaMessageToCortana();
                                break;
                            }
                        default:
                            SendLaunchAppInForegroundMessageToCortana();
                            break;
                    }
                }
                finally
                {
                    taskDeferral.Complete();
                }

                async void SendLaunchAppInForegroundMessageToCortana()
                {
                    VoiceCommandUserMessage userMessage = new()
                    {
                        SpokenMessage = "CortanaService_OpenForegroundApp".GetLocalized()
                    };

                    VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage);
                    await voiceServiceConnection.RequestAppLaunchAsync(response);
                }

                async void SendBakaEurekaMessageToCortana()
                {
                    VoiceCommandUserMessage userMessage = new()
                    {
                        DisplayMessage = "CortanaService_BakaEureka_DisplayMessage".GetLocalized(),
                        SpokenMessage = "CortanaService_BakaEureka_SpokenMessage".GetLocalized()
                    };

                    IEnumerable<VoiceCommandContentTile> destinationsContentTiles = [
                        new()
                        {
                            ContentTileType = VoiceCommandContentTileType.TitleWith68x68IconAndText,
                            AppLaunchArgument = CommonValues.BakaEurekaArgument,
                            Title = "尤里卡~尤里卡~",
                            TextLine1 = "我所挚爱的你——Baka632",
                    }];

                    VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);
                    response.AppLaunchArgument = CommonValues.BakaEurekaArgument;

                    await voiceServiceConnection.ReportSuccessAsync(response);
                }
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
