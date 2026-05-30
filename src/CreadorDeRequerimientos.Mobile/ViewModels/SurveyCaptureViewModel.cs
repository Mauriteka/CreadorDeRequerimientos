using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Mobile.Services;

namespace CreadorDeRequerimientos.Mobile.ViewModels;

public partial class SurveyCaptureViewModel : ObservableObject, IQueryAttributable
{
    private readonly IProjectService projectService;
    private readonly ISurveyService surveyService;
    private readonly ISpeechCaptureService speechCaptureService;
    private Guid projectId;
    private Guid surveyId;

    public ObservableCollection<SurveyParticipantResponse> Participants { get; } = [];
    public ObservableCollection<QuestionPromptItem> Questions { get; } = [];
    public ObservableCollection<TranscriptTurnResponse> TranscriptTurns { get; } = [];

    [ObservableProperty]
    public partial ProjectDetailResponse? Project { get; set; }

    [ObservableProperty]
    public partial SurveyResponse? Survey { get; set; }

    [ObservableProperty]
    public partial SurveyParticipantResponse? SelectedParticipant { get; set; }

    [ObservableProperty]
    public partial QuestionPromptItem? SelectedQuestion { get; set; }

    [ObservableProperty]
    public partial string CapturedText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool Important { get; set; }

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial bool IsListening { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Captura manual disponible.";

    public string CaptureMode => speechCaptureService.IsContinuousCaptureSupported
        ? "Voz continua"
        : speechCaptureService.IsNativeCaptureAvailable ? "Voz nativa" : "Captura manual";

    public string CaptureButtonText => speechCaptureService.IsNativeCaptureAvailable ? "Dictar" : "Capturar";

    public string ContinuousCaptureButtonText => IsListening ? "Detener grabacion" : "Iniciar grabacion";

    public bool CanUseContinuousCapture => speechCaptureService.IsContinuousCaptureSupported;

    public SurveyCaptureViewModel(
        IProjectService projectService,
        ISurveyService surveyService,
        ISpeechCaptureService speechCaptureService)
    {
        this.projectService = projectService;
        this.surveyService = surveyService;
        this.speechCaptureService = speechCaptureService;
        speechCaptureService.TextReceived += OnSpeechTextReceived;
        speechCaptureService.ListeningChanged += OnSpeechListeningChanged;
        IsListening = speechCaptureService.IsListening;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("projectId", out var projectValue) &&
            Guid.TryParse(projectValue?.ToString(), out var parsedProjectId))
        {
            projectId = parsedProjectId;
        }

        if (query.TryGetValue("surveyId", out var surveyValue) &&
            Guid.TryParse(surveyValue?.ToString(), out var parsedSurveyId))
        {
            surveyId = parsedSurveyId;
        }

        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || projectId == Guid.Empty || surveyId == Guid.Empty)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Project = await projectService.GetProjectAsync(projectId, CancellationToken.None);
            Survey = Project?.Surveys.FirstOrDefault(survey => survey.Id == surveyId);
            RefreshSurveyState();
            StatusMessage = Survey is null ? "Encuesta no encontrada." : "Encuesta lista para capturar.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo cargar la encuesta: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CaptureAsync()
    {
        CapturedText = await speechCaptureService.CaptureAsync(CapturedText, CancellationToken.None);
        StatusMessage = speechCaptureService.IsNativeCaptureAvailable
            ? "Captura recibida."
            : "Escribe o pega la mencion y guardala como turno.";
    }

    [RelayCommand]
    private async Task ToggleContinuousCaptureAsync()
    {
        if (!speechCaptureService.IsContinuousCaptureSupported)
        {
            StatusMessage = "La captura continua no esta disponible en esta plataforma.";
            return;
        }

        if (IsListening)
        {
            await speechCaptureService.StopContinuousCaptureAsync();
            StatusMessage = "Grabacion detenida.";
            return;
        }

        await speechCaptureService.StartContinuousCaptureAsync(CancellationToken.None);
        StatusMessage = speechCaptureService.IsListening
            ? "Grabacion continua activa. Cambia participante o pregunta sin apagar el micro."
            : "No se pudo iniciar el microfono.";
    }

    [RelayCommand]
    private async Task SaveTurnAsync()
    {
        if (IsBusy || SelectedParticipant is null || string.IsNullOrWhiteSpace(CapturedText))
        {
            StatusMessage = "Selecciona participante y escribe una mencion.";
            return;
        }

        await SaveTurnSnapshotAsync(SelectedParticipant, SelectedQuestion, CapturedText, "Turno guardado.");
    }

    private async Task SaveTurnSnapshotAsync(
        SurveyParticipantResponse participant,
        QuestionPromptItem? question,
        string text,
        string successMessage)
    {
        if (projectId == Guid.Empty || surveyId == Guid.Empty || string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        try
        {
            IsBusy = true;
            var request = new AddTranscriptTurnRequest(
                participant.Id,
                text.Trim(),
                question?.Tag ?? "mobile:manual",
                Important,
                "mobile");

            Project = await surveyService.AddTranscriptTurnAsync(projectId, surveyId, request, CancellationToken.None);
            Survey = Project?.Surveys.FirstOrDefault(survey => survey.Id == surveyId);
            if (string.Equals(CapturedText, text, StringComparison.Ordinal))
            {
                CapturedText = string.Empty;
            }

            Important = false;
            RefreshSurveyState();
            StatusMessage = successMessage;
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo guardar el turno: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefreshSurveyState()
    {
        Participants.Clear();
        Questions.Clear();
        TranscriptTurns.Clear();

        if (Survey is null)
        {
            return;
        }

        foreach (var participant in Survey.Participants.OrderBy(participant => participant.SortOrder))
        {
            Participants.Add(participant);
        }

        SelectedParticipant ??= Participants.FirstOrDefault();

        if (Survey.AppliedTemplate is not null)
        {
            for (var sectionIndex = 0; sectionIndex < Survey.AppliedTemplate.InterviewSections.Count; sectionIndex++)
            {
                var section = Survey.AppliedTemplate.InterviewSections[sectionIndex];
                for (var questionIndex = 0; questionIndex < section.Questions.Count; questionIndex++)
                {
                    Questions.Add(new QuestionPromptItem(
                        section.Title,
                        section.Questions[questionIndex],
                        $"question:{sectionIndex}:{questionIndex}"));
                }
            }
        }

        SelectedQuestion ??= Questions.FirstOrDefault();

        foreach (var turn in Survey.TranscriptTurns.OrderBy(turn => turn.CreatedAt))
        {
            TranscriptTurns.Add(turn);
        }
    }

    partial void OnIsListeningChanged(bool value)
    {
        OnPropertyChanged(nameof(ContinuousCaptureButtonText));
    }

    partial void OnSelectedParticipantChanging(SurveyParticipantResponse? oldValue, SurveyParticipantResponse? newValue)
    {
        if (oldValue is not null && !string.IsNullOrWhiteSpace(CapturedText))
        {
            var text = CapturedText;
            CapturedText = string.Empty;
            _ = SaveTurnSnapshotAsync(oldValue, SelectedQuestion, text, "Turno anterior guardado al cambiar participante.");
        }
    }

    partial void OnSelectedQuestionChanging(QuestionPromptItem? oldValue, QuestionPromptItem? newValue)
    {
        if (SelectedParticipant is not null && oldValue is not null && !string.IsNullOrWhiteSpace(CapturedText))
        {
            var text = CapturedText;
            CapturedText = string.Empty;
            _ = SaveTurnSnapshotAsync(SelectedParticipant, oldValue, text, "Turno anterior guardado al cambiar pregunta.");
        }
    }

    private void OnSpeechListeningChanged(object? sender, SpeechListeningChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsListening = e.IsListening;
            StatusMessage = e.IsListening
                ? "Escuchando..."
                : speechCaptureService.IsContinuousCaptureSupported ? "Reiniciando escucha..." : StatusMessage;
        });
    }

    private void OnSpeechTextReceived(object? sender, SpeechTextReceivedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.Text))
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CapturedText = string.IsNullOrWhiteSpace(CapturedText)
                ? e.Text.Trim()
                : $"{CapturedText.TrimEnd()} {e.Text.Trim()}";
            StatusMessage = "Texto dictado agregado al turno actual.";
        });
    }
}

public sealed record QuestionPromptItem(string SectionTitle, string Text, string Tag)
{
    public string DisplayText => string.IsNullOrWhiteSpace(SectionTitle)
        ? Text
        : $"{SectionTitle}: {Text}";
}
