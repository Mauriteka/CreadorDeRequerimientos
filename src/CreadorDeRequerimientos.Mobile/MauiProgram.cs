using CommunityToolkit.Maui;
using CreadorDeRequerimientos.Mobile.Pages;
using CreadorDeRequerimientos.Mobile.Services;
using CreadorDeRequerimientos.Mobile.ViewModels;
#if ANDROID
using CreadorDeRequerimientos.Mobile.Platforms.Android;
#endif

namespace CreadorDeRequerimientos.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        builder.Services.AddSingleton(ApiEndpointOptions.CreateFromEnvironment());
        builder.Services.AddHttpClient<CreadorApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<ApiEndpointOptions>();
            client.BaseAddress = options.BaseUri;
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        builder.Services.AddSingleton<IProjectService, ProjectService>();
        builder.Services.AddSingleton<ISurveyService, SurveyService>();
        builder.Services.AddSingleton<IRequirementService, RequirementService>();
#if ANDROID
        builder.Services.AddSingleton<ISpeechCaptureService, AndroidSpeechCaptureService>();
#else
        builder.Services.AddSingleton<ISpeechCaptureService, ManualSpeechCaptureService>();
#endif

        builder.Services.AddTransient<ProjectsViewModel>();
        builder.Services.AddTransient<ProjectDetailViewModel>();
        builder.Services.AddTransient<SurveyCaptureViewModel>();

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<ProjectsPage>();
        builder.Services.AddTransient<ProjectDetailPage>();
        builder.Services.AddTransient<SurveyCapturePage>();

        return builder.Build();
    }
}
