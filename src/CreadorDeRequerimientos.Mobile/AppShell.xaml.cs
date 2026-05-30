using CreadorDeRequerimientos.Mobile.Pages;

namespace CreadorDeRequerimientos.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ProjectDetailPage), typeof(ProjectDetailPage));
        Routing.RegisterRoute(nameof(SurveyCapturePage), typeof(SurveyCapturePage));
    }
}
