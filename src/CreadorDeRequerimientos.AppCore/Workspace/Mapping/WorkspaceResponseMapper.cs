using CreadorDeRequerimientos.AppCore.Workspace.Formatting;
using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace.Mapping;

public sealed class WorkspaceResponseMapper(SurveyTranscriptFormatter formatter)
{
    public ProjectSummaryResponse ToSummary(RequirementProject project) =>
        new(project.Id, project.Name, project.FeatureName, project.Surveys.Count, project.Requirements.Count, project.ProjectTemplates.Count, project.UpdatedAt);

    public ProjectDetailResponse ToDetail(RequirementProject project) =>
        new(
            project.Id,
            project.Name,
            project.FeatureName,
            project.Notes,
            project.CreatedAt,
            project.UpdatedAt,
            project.Surveys.OrderByDescending(item => item.UpdatedAt).Select(ToSurvey).ToList(),
            project.Requirements.OrderByDescending(item => item.UpdatedAt).Select(item => ToRequirement(item, project)).ToList(),
            project.ProjectTemplates.OrderBy(item => item.Name).Select(ToTemplateDetail).ToList());

    public TemplateDetailResponse ToTemplateDetail(SurveyTemplate template) =>
        new(
            template.Id,
            template.Name,
            template.Description,
            template.Scope,
            template.CreatedAt,
            template.UpdatedAt,
            template.InterviewSections.Select(ToInterviewSectionResponse).ToList(),
            template.MinuteSections.Select(ToMinuteSectionResponse).ToList());

    public AppliedTemplateResponse ToAppliedTemplate(SurveyTemplateSnapshot template) =>
        new(
            template.SourceTemplateId,
            template.SourceScope,
            template.Name,
            template.Description,
            template.CapturedAt,
            template.InterviewSections.Select(ToInterviewSectionResponse).ToList(),
            template.MinuteSections.Select(ToMinuteSectionResponse).ToList());

    private SurveyResponse ToSurvey(UserSurvey survey)
    {
        var speakers = survey.Participants.ToDictionary(item => item.Id, item => item.DisplayName);
        var suggestedMinute = formatter.BuildSurveyMinuteDraft(survey);
        var conversationCopy = formatter.BuildSurveyConversationCopy(survey);
        return new SurveyResponse(
            survey.Id,
            survey.Title,
            survey.Interviewee,
            survey.Objective,
            survey.OwnerEmail,
            survey.IntervieweeEmail,
            survey.ExtraEmails,
            survey.MinuteDraft,
            survey.IsFinalized,
            survey.FinalizedAt,
            survey.CreatedAt,
            survey.UpdatedAt,
            suggestedMinute,
            conversationCopy,
            survey.AppliedTemplate is null ? null : ToAppliedTemplate(survey.AppliedTemplate),
            survey.Participants.OrderBy(item => item.SortOrder).Select(ToParticipant).ToList(),
            survey.TranscriptTurns.OrderBy(item => item.CreatedAt).Select(turn => ToTurn(turn, speakers)).ToList(),
            survey.Mentions.OrderByDescending(item => item.CreatedAt).Select(ToMention).ToList());
    }

    private SurveyParticipantResponse ToParticipant(SurveyParticipant participant) =>
        new(participant.Id, participant.DisplayName, participant.Email, participant.RoleType, participant.SortOrder, participant.CreatedAt);

    private TranscriptTurnResponse ToTurn(SurveyTranscriptTurn turn, IReadOnlyDictionary<Guid, string> speakers) =>
        new(
            turn.Id,
            turn.SpeakerId,
            NormalizeSpeakerName(speakers.TryGetValue(turn.SpeakerId, out var speakerName) ? speakerName : "Persona"),
            turn.Text,
            turn.Tag,
            turn.Important,
            turn.SourceType,
            turn.CreatedAt,
            turn.UpdatedAt);

    private MentionResponse ToMention(SurveyMention mention) =>
        new(mention.Id, mention.Text, mention.Tag, mention.Important, mention.CreatedAt);

    private RequirementDocumentResponse ToRequirement(RequirementDocument requirement, RequirementProject project) =>
        new(
            requirement.Id,
            requirement.Stage,
            requirement.Title,
            requirement.Summary,
            requirement.Content,
            requirement.SurveyIds,
            requirement.RelatedRequirementIds,
            project.Surveys
                .Where(survey => requirement.SurveyIds.Contains(survey.Id))
                .OrderBy(survey => survey.Title)
                .Select(survey => new LinkedSurveySummaryResponse(survey.Id, survey.Title, survey.Interviewee))
                .ToList(),
            project.Requirements
                .Where(item => requirement.RelatedRequirementIds.Contains(item.Id))
                .OrderBy(item => item.Title)
                .Select(item => new LinkedRequirementSummaryResponse(item.Id, item.Title, item.Stage))
                .ToList(),
            requirement.CreatedAt,
            requirement.UpdatedAt);

    private static TemplateInterviewSectionResponse ToInterviewSectionResponse(TemplateInterviewSection section) =>
        new(section.Title, section.Prompt, section.Questions);

    private static TemplateMinuteSectionResponse ToMinuteSectionResponse(TemplateMinuteSection section) =>
        new(section.Title, section.Prompt);

    private static string NormalizeSpeakerName(string? speakerName)
    {
        if (string.IsNullOrWhiteSpace(speakerName))
        {
            return "Persona";
        }

        return string.Equals(speakerName.Trim(), "Yo", StringComparison.OrdinalIgnoreCase)
            ? "Entrevistador"
            : speakerName.Trim();
    }
}
