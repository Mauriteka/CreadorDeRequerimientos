using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Mobile.ViewModels;

namespace CreadorDeRequerimientos.Mobile.Pages;

public partial class ProjectsPage : ContentPage
{
    private readonly ProjectsViewModel viewModel;

    public ProjectsPage(ProjectsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (viewModel.Projects.Count == 0)
        {
            viewModel.LoadCommand.Execute(null);
        }
    }

    private async void OnProjectSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var project = e.CurrentSelection.FirstOrDefault() as ProjectSummaryResponse;
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }

        await viewModel.OpenProjectAsync(project);
    }
}
