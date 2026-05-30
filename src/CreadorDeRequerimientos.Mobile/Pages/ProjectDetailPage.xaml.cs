using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Mobile.ViewModels;

namespace CreadorDeRequerimientos.Mobile.Pages;

public partial class ProjectDetailPage : ContentPage
{
    private readonly ProjectDetailViewModel viewModel;

    public ProjectDetailPage(ProjectDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    private async void OnSurveySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var survey = e.CurrentSelection.FirstOrDefault() as SurveyResponse;
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        await viewModel.OpenSurveyAsync(survey);
    }
}
