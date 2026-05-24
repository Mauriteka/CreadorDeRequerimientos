using CreadorDeRequerimientos.AppCore.Workspace.Formatting;
using CreadorDeRequerimientos.AppCore.Workspace.Mapping;
using CreadorDeRequerimientos.AppCore.Workspace.Normalization;
using CreadorDeRequerimientos.AppCore.Workspace.Participants;
using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace;

public sealed class RequirementWorkspaceService(
    IRequirementWorkspaceStore store,
    WorkspaceNormalizer normalizer,
    SurveyParticipantFactory participantFactory,
    SurveyTranscriptFormatter formatter,
    WorkspaceResponseMapper mapper)
{
    public async Task<IReadOnlyList<ProjectSummaryResponse>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        return workspace.Projects
            .OrderByDescending(project => project.UpdatedAt)
            .Select(mapper.ToSummary)
            .ToList();
    }

    public async Task<IReadOnlyList<TemplateDetailResponse>> GetSystemTemplatesAsync(CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        return workspace.SystemTemplates
            .OrderBy(template => template.Name)
            .Select(mapper.ToTemplateDetail)
            .ToList();
    }

    public async Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        return workspace.Projects.FirstOrDefault(project => project.Id == projectId) is { } project
            ? mapper.ToDetail(project)
            : null;
    }

    public async Task<ProjectDetailResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = RequirementProject.Create(request.Name, request.FeatureName, request.Notes);
        workspace.Projects.Add(project);
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null)
        {
            return null;
        }

        project.Rename(request.Name, request.FeatureName, request.Notes);
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var removed = workspace.Projects.RemoveAll(item => item.Id == projectId) > 0;
        if (removed)
        {
            await store.SaveAsync(workspace, cancellationToken);
        }

        return removed;
    }

    public async Task<TemplateDetailResponse> CreateSystemTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var template = SurveyTemplate.Create(
            request.Name,
            request.Description,
            "system",
            request.InterviewSections.Select(ToInterviewSection),
            request.MinuteSections.Select(ToMinuteSection));
        workspace.SystemTemplates.Add(template);
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToTemplateDetail(template);
    }

    public async Task<TemplateDetailResponse?> UpdateSystemTemplateAsync(Guid templateId, UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var template = workspace.SystemTemplates.FirstOrDefault(item => item.Id == templateId);
        if (template is null)
        {
            return null;
        }

        template.Update(
            request.Name,
            request.Description,
            "system",
            request.InterviewSections.Select(ToInterviewSection),
            request.MinuteSections.Select(ToMinuteSection));
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToTemplateDetail(template);
    }

    public async Task<bool> DeleteSystemTemplateAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var removed = workspace.SystemTemplates.RemoveAll(item => item.Id == templateId) > 0;
        if (removed)
        {
            await store.SaveAsync(workspace, cancellationToken);
        }

        return removed;
    }

    public async Task<ProjectDetailResponse?> CreateProjectTemplateAsync(Guid projectId, CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null)
        {
            return null;
        }

        var template = SurveyTemplate.Create(
            request.Name,
            request.Description,
            "project",
            request.InterviewSections.Select(ToInterviewSection),
            request.MinuteSections.Select(ToMinuteSection));
        project.ProjectTemplates.Add(template);
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> UpdateProjectTemplateAsync(Guid projectId, Guid templateId, UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var template = project?.ProjectTemplates.FirstOrDefault(item => item.Id == templateId);
        if (project is null || template is null)
        {
            return null;
        }

        template.Update(
            request.Name,
            request.Description,
            "project",
            request.InterviewSections.Select(ToInterviewSection),
            request.MinuteSections.Select(ToMinuteSection));
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> DeleteProjectTemplateAsync(Guid projectId, Guid templateId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null || project.ProjectTemplates.RemoveAll(item => item.Id == templateId) == 0)
        {
            return null;
        }

        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ExportProjectTemplateResponse?> ExportProjectTemplateAsync(Guid projectId, Guid templateId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var template = project?.ProjectTemplates.FirstOrDefault(item => item.Id == templateId);
        if (template is null)
        {
            return null;
        }

        var clone = template.CloneAs("system");
        workspace.SystemTemplates.Add(clone);
        await store.SaveAsync(workspace, cancellationToken);
        return new ExportProjectTemplateResponse(clone.Id, clone.Name, clone.Scope);
    }

    public async Task<ProjectDetailResponse?> CreateSurveyAsync(Guid projectId, CreateSurveyRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null)
        {
            return null;
        }

        var snapshot = ResolveTemplateSnapshot(workspace, project, request.TemplateId, request.TemplateScope);
        var survey = project.AddSurvey(request.Title, request.Interviewee, request.Objective, snapshot);
        survey.Update(
            survey.Title,
            request.Interviewee,
            request.Objective,
            request.OwnerEmail ?? string.Empty,
            request.IntervieweeEmail ?? string.Empty,
            request.ExtraEmails ?? string.Empty,
            request.MinuteDraft ?? string.Empty,
            request.IsFinalized);
        normalizer.SyncParticipantEmails(survey);
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> UpdateSurveyAsync(Guid projectId, Guid surveyId, UpdateSurveyRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var survey = project?.Surveys.FirstOrDefault(item => item.Id == surveyId);
        if (project is null || survey is null)
        {
            return null;
        }

        if (request.TemplateId.HasValue &&
            survey.AppliedTemplate is null &&
            survey.TranscriptTurns.Count == 0 &&
            survey.Mentions.Count == 0)
        {
            survey.AppliedTemplate = ResolveTemplateSnapshot(workspace, project, request.TemplateId, request.TemplateScope);
        }

        survey.Update(
            request.Title,
            request.Interviewee,
            request.Objective,
            request.OwnerEmail ?? survey.OwnerEmail,
            request.IntervieweeEmail ?? survey.IntervieweeEmail,
            request.ExtraEmails ?? survey.ExtraEmails,
            request.MinuteDraft ?? survey.MinuteDraft,
            request.IsFinalized);
        normalizer.SyncParticipantEmails(survey);
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> DeleteSurveyAsync(Guid projectId, Guid surveyId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null || project.Surveys.RemoveAll(item => item.Id == surveyId) == 0)
        {
            return null;
        }

        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> CreateParticipantAsync(Guid projectId, Guid surveyId, CreateSurveyParticipantRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var survey = project?.Surveys.FirstOrDefault(item => item.Id == surveyId);
        if (project is null || survey is null)
        {
            return null;
        }

        survey.Participants.Add(participantFactory.CreateFromRequest(survey, request));
        survey.Touch();
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> RenameParticipantAsync(Guid projectId, Guid surveyId, Guid participantId, RenameParticipantRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var survey = project?.Surveys.FirstOrDefault(item => item.Id == surveyId);
        var participant = survey?.Participants.FirstOrDefault(item => item.Id == participantId);
        if (project is null || survey is null || participant is null)
        {
            return null;
        }

        participant.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? participant.DisplayName
            : request.DisplayName.Trim();
        participant.Email = request.Email?.Trim() ?? participant.Email;

        survey.Touch();
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> DeleteParticipantAsync(Guid projectId, Guid surveyId, Guid participantId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var survey = project?.Surveys.FirstOrDefault(item => item.Id == surveyId);
        var participant = survey?.Participants.FirstOrDefault(item => item.Id == participantId);
        if (project is null || survey is null || participant is null)
        {
            return null;
        }

        if (participant.RoleType == "Self")
        {
            throw new InvalidOperationException("No puedes eliminar tu propio participante.");
        }

        if (survey.TranscriptTurns.Any(item => item.SpeakerId == participantId))
        {
            throw new InvalidOperationException("No puedes eliminar un participante que ya tiene transcript.");
        }

        survey.Participants.RemoveAll(item => item.Id == participantId);
        survey.Touch();
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> AddTranscriptTurnAsync(Guid projectId, Guid surveyId, AddTranscriptTurnRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var survey = project?.Surveys.FirstOrDefault(item => item.Id == surveyId);
        if (project is null || survey is null || survey.Participants.All(item => item.Id != request.SpeakerId))
        {
            return null;
        }

        survey.AddTranscriptTurn(request.SpeakerId, request.Text, request.Tag, request.Important, request.SourceType);
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> UpdateTranscriptTurnAsync(Guid projectId, Guid surveyId, Guid turnId, UpdateTranscriptTurnRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var survey = project?.Surveys.FirstOrDefault(item => item.Id == surveyId);
        var turn = survey?.TranscriptTurns.FirstOrDefault(item => item.Id == turnId);
        if (project is null || survey is null || turn is null || survey.Participants.All(item => item.Id != request.SpeakerId))
        {
            return null;
        }

        turn.Update(request.SpeakerId, request.Text, request.Tag, request.Important, request.SourceType);
        survey.Touch();
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> DeleteTranscriptTurnAsync(Guid projectId, Guid surveyId, Guid turnId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        var survey = project?.Surveys.FirstOrDefault(item => item.Id == surveyId);
        if (project is null || survey is null || survey.TranscriptTurns.RemoveAll(item => item.Id == turnId) == 0)
        {
            return null;
        }

        survey.Touch();
        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> UpsertRequirementAsync(Guid projectId, UpsertRequirementRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null)
        {
            return null;
        }

        var surveyIds = (request.SurveyIds ?? []).Distinct().ToList();
        var relatedRequirementIds = (request.RelatedRequirementIds ?? []).Distinct().ToList();
        if (surveyIds.Any(id => project.Surveys.All(survey => survey.Id != id)))
        {
            throw new InvalidOperationException("Solo puedes vincular encuestas del mismo proyecto.");
        }

        if (relatedRequirementIds.Any(id => project.Requirements.All(requirement => requirement.Id != id)))
        {
            throw new InvalidOperationException("Solo puedes vincular requerimientos del mismo proyecto.");
        }

        if (request.Id.HasValue && relatedRequirementIds.Contains(request.Id.Value))
        {
            throw new InvalidOperationException("Un requerimiento no puede apoyarse en si mismo.");
        }

        project.UpsertRequirement(
            request.Id,
            request.Stage,
            request.Title,
            request.Summary,
            request.Content,
            surveyIds,
            relatedRequirementIds);
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> DeleteRequirementAsync(Guid projectId, Guid requirementId, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null || project.Requirements.RemoveAll(item => item.Id == requirementId) == 0)
        {
            return null;
        }

        foreach (var requirement in project.Requirements)
        {
            requirement.RelatedRequirementIds.RemoveAll(id => id == requirementId);
        }

        project.Touch();
        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    public async Task<ProjectDetailResponse?> CreateDraftRequirementAsync(Guid projectId, CreateDraftRequirementRequest request, CancellationToken cancellationToken)
    {
        var workspace = await LoadWorkspaceAsync(cancellationToken);
        var project = workspace.Projects.FirstOrDefault(item => item.Id == projectId);
        if (project is null)
        {
            return null;
        }

        var requirement = request.RequirementId.HasValue
            ? project.Requirements.FirstOrDefault(item => item.Id == request.RequirementId.Value)
            : null;
        var content = formatter.BuildProjectDraft(project, requirement);
        if (requirement is null)
        {
            project.UpsertRequirement(null, request.Stage, request.Title, string.Empty, content, [], []);
        }
        else
        {
            project.UpsertRequirement(
                requirement.Id,
                request.Stage,
                requirement.Title,
                requirement.Summary,
                content,
                requirement.SurveyIds,
                requirement.RelatedRequirementIds);
        }

        await store.SaveAsync(workspace, cancellationToken);
        return mapper.ToDetail(project);
    }

    private async Task<RequirementWorkspace> LoadWorkspaceAsync(CancellationToken cancellationToken)
    {
        var workspace = await store.LoadAsync(cancellationToken);
        if (normalizer.NormalizeWorkspace(workspace))
        {
            await store.SaveAsync(workspace, cancellationToken);
        }

        return workspace;
    }

    private static SurveyTemplateSnapshot? ResolveTemplateSnapshot(RequirementWorkspace workspace, RequirementProject project, Guid? templateId, string? templateScope)
    {
        if (!templateId.HasValue)
        {
            return null;
        }

        var scope = string.IsNullOrWhiteSpace(templateScope) ? "project" : templateScope.Trim().ToLowerInvariant();
        var template = scope == "system"
            ? workspace.SystemTemplates.FirstOrDefault(item => item.Id == templateId.Value)
            : project.ProjectTemplates.FirstOrDefault(item => item.Id == templateId.Value);

        return template?.ToSnapshot();
    }

    private static TemplateInterviewSection ToInterviewSection(TemplateInterviewSectionRequest request)
    {
        return new TemplateInterviewSection
        {
            Title = request.Title,
            Prompt = request.Prompt,
            Questions = request.Questions.Where(question => !string.IsNullOrWhiteSpace(question)).Select(question => question.Trim()).ToList()
        };
    }

    private static TemplateMinuteSection ToMinuteSection(TemplateMinuteSectionRequest request)
    {
        return new TemplateMinuteSection
        {
            Title = request.Title,
            Prompt = request.Prompt
        };
    }
}
