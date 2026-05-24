using CreadorDeRequerimientos.AppCore.Workspace.Templates;
using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace.Normalization;

public sealed class WorkspaceNormalizer(DefaultSurveyTemplateFactory templateFactory)
{
    public bool NormalizeWorkspace(RequirementWorkspace workspace)
    {
        var changed = false;
        workspace.SystemTemplates ??= [];
        changed |= EnsureDefaultSystemTemplate(workspace);

        foreach (var project in workspace.Projects)
        {
            project.ProjectTemplates ??= [];
            foreach (var requirement in project.Requirements)
            {
                requirement.SurveyIds ??= [];
                requirement.RelatedRequirementIds ??= [];
            }

            foreach (var survey in project.Surveys)
            {
                changed |= NormalizeSurvey(survey);
            }
        }

        return changed;
    }

    public void SyncParticipantEmails(UserSurvey survey)
    {
        var self = survey.Participants.FirstOrDefault(item => item.RoleType == "Self");
        if (self is not null)
        {
            self.Email = survey.OwnerEmail;
        }

        var firstGuest = survey.Participants.FirstOrDefault(item => item.RoleType != "Self");
        if (firstGuest is not null)
        {
            survey.IntervieweeEmail = firstGuest.Email;
        }
    }

    private bool EnsureDefaultSystemTemplate(RequirementWorkspace workspace)
    {
        var changed = false;
        foreach (var seedTemplate in templateFactory.CreateDefaultSystemTemplates())
        {
            var existing = workspace.SystemTemplates.FirstOrDefault(item =>
                item.Scope == "system" &&
                string.Equals(item.Name, seedTemplate.Name, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                workspace.SystemTemplates.Add(seedTemplate);
                changed = true;
            }
        }

        var defaultTemplate = workspace.SystemTemplates.FirstOrDefault(item =>
            item.Scope == "system" &&
            string.Equals(item.Name, "Levantamiento base de requerimientos", StringComparison.OrdinalIgnoreCase));

        if (defaultTemplate is not null && defaultTemplate.MinuteSections.Count < 8)
        {
            var upgraded = templateFactory.CreateDefaultSystemTemplate();
            defaultTemplate.Update(
                upgraded.Name,
                upgraded.Description,
                "system",
                upgraded.InterviewSections,
                upgraded.MinuteSections);
            changed = true;
        }

        return changed;
    }

    private bool NormalizeSurvey(UserSurvey survey)
    {
        var changed = false;
        survey.Mentions ??= [];
        survey.Participants ??= [];
        survey.TranscriptTurns ??= [];
        survey.OwnerEmail ??= string.Empty;
        survey.IntervieweeEmail ??= string.Empty;
        survey.ExtraEmails ??= string.Empty;
        survey.MinuteDraft ??= string.Empty;
        foreach (var participant in survey.Participants)
        {
            participant.Email ??= string.Empty;
        }

        if (survey.Participants.Count == 0)
        {
            survey.Participants.Add(new SurveyParticipant
            {
                DisplayName = "Yo",
                Email = survey.OwnerEmail,
                RoleType = "Self",
                SortOrder = 0
            });
            changed = true;
        }
        else if (survey.Participants.All(item => item.RoleType != "Self"))
        {
            survey.Participants.Insert(0, new SurveyParticipant
            {
                DisplayName = "Yo",
                Email = survey.OwnerEmail,
                RoleType = "Self",
                SortOrder = 0
            });
            changed = true;
        }

        if (survey.Participants.All(item => item.RoleType == "Self"))
        {
            survey.Participants.Add(new SurveyParticipant
            {
                DisplayName = "Persona 1",
                Email = string.Empty,
                RoleType = "GuestPlaceholder",
                SortOrder = survey.Participants.Count
            });
            changed = true;
        }

        if (survey.TranscriptTurns.Count == 0 && survey.Mentions.Count > 0)
        {
            var speaker = survey.Participants.FirstOrDefault(item => item.RoleType != "Self");
            if (speaker is null)
            {
                speaker = new SurveyParticipant
                {
                    DisplayName = "Persona 1",
                    Email = string.Empty,
                    RoleType = "GuestPlaceholder",
                    SortOrder = survey.Participants.Count
                };
                survey.Participants.Add(speaker);
            }

            foreach (var mention in survey.Mentions.OrderBy(item => item.CreatedAt))
            {
                survey.TranscriptTurns.Add(new SurveyTranscriptTurn
                {
                    SpeakerId = speaker.Id,
                    Text = mention.Text,
                    Tag = mention.Tag,
                    Important = mention.Important,
                    SourceType = "Legacy",
                    CreatedAt = mention.CreatedAt,
                    UpdatedAt = mention.CreatedAt
                });
            }

            survey.Mentions.Clear();
            changed = true;
        }

        var orderedParticipants = survey.Participants.OrderBy(item => item.SortOrder).ThenBy(item => item.CreatedAt).ToList();
        for (var index = 0; index < orderedParticipants.Count; index++)
        {
            if (orderedParticipants[index].SortOrder != index)
            {
                orderedParticipants[index].SortOrder = index;
                changed = true;
            }
        }

        survey.Participants = orderedParticipants;
        SyncParticipantEmails(survey);
        return changed;
    }
}
