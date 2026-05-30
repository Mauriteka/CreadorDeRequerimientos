using CreadorDeRequerimientos.Mobile.ViewModels;

namespace CreadorDeRequerimientos.Mobile.Pages;

public partial class SurveyCapturePage : ContentPage
{
    public SurveyCapturePage(SurveyCaptureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
