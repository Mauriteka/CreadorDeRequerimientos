using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Mobile.Pages;
using CreadorDeRequerimientos.Mobile.Services;

namespace CreadorDeRequerimientos.Mobile.ViewModels;

public partial class ProjectsViewModel(IProjectService projectService, ApiEndpointOptions apiOptions) : ObservableObject
{
    public ObservableCollection<ProjectSummaryResponse> Projects { get; } = [];

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = $"API: {apiOptions.BaseUri}";

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Conectando a {apiOptions.BaseUri}";
            var projects = await projectService.GetProjectsAsync(CancellationToken.None);
            Projects.Clear();
            foreach (var project in projects.OrderByDescending(project => project.UpdatedAt))
            {
                Projects.Add(project);
            }

            StatusMessage = Projects.Count == 0
                ? "No hay proyectos todavia."
                : $"{Projects.Count} proyecto(s) cargado(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo cargar proyectos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task OpenProjectAsync(ProjectSummaryResponse? project)
    {
        if (project is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(ProjectDetailPage)}?projectId={project.Id}");
    }
}
