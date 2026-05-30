using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Mobile.Pages;
using CreadorDeRequerimientos.Mobile.Services;

namespace CreadorDeRequerimientos.Mobile.ViewModels;

public partial class ProjectDetailViewModel(
    IProjectService projectService,
    ISurveyService surveyService,
    IRequirementService requirementService) : ObservableObject, IQueryAttributable
{
    private Guid projectId;

    public ObservableCollection<SurveyResponse> Surveys { get; } = [];
    public ObservableCollection<RequirementDocumentResponse> Requirements { get; } = [];

    [ObservableProperty]
    public partial ProjectDetailResponse? Project { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string NewSurveyTitle { get; set; } = "Entrevista nueva";

    [ObservableProperty]
    public partial string NewSurveyInterviewee { get; set; } = "Entrevistado";

    [ObservableProperty]
    public partial string NewSurveyObjective { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("projectId", out var value) &&
            Guid.TryParse(value?.ToString(), out var parsedProjectId))
        {
            projectId = parsedProjectId;
            LoadCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || projectId == Guid.Empty)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Project = await projectService.GetProjectAsync(projectId, CancellationToken.None);
            RefreshCollections(Project);
            StatusMessage = Project is null ? "Proyecto no encontrado." : "Proyecto actualizado.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo cargar el proyecto: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateSurveyAsync()
    {
        if (IsBusy || projectId == Guid.Empty)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var request = new CreateSurveyRequest(
                NewSurveyTitle.Trim(),
                NewSurveyInterviewee.Trim(),
                NewSurveyObjective.Trim(),
                null,
                null,
                null,
                null,
                null,
                null,
                false);

            Project = await surveyService.CreateSurveyAsync(projectId, request, CancellationToken.None);
            RefreshCollections(Project);
            StatusMessage = "Encuesta creada.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo crear la encuesta: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateRequirementDraftAsync()
    {
        if (IsBusy || projectId == Guid.Empty)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var title = Project is null ? "Borrador" : $"Borrador - {Project.Name}";
            Project = await requirementService.CreateDraftAsync(
                projectId,
                new CreateDraftRequirementRequest("toma", title, null),
                CancellationToken.None);
            RefreshCollections(Project);
            StatusMessage = "Borrador de requerimiento generado.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo generar el borrador: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task OpenSurveyAsync(SurveyResponse? survey)
    {
        if (survey is null || projectId == Guid.Empty)
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(SurveyCapturePage)}?projectId={projectId}&surveyId={survey.Id}");
    }

    private void RefreshCollections(ProjectDetailResponse? project)
    {
        Surveys.Clear();
        Requirements.Clear();
        if (project is null)
        {
            return;
        }

        foreach (var survey in project.Surveys.OrderByDescending(survey => survey.UpdatedAt))
        {
            Surveys.Add(survey);
        }

        foreach (var requirement in project.Requirements.OrderByDescending(requirement => requirement.UpdatedAt))
        {
            Requirements.Add(requirement);
        }
    }
}
