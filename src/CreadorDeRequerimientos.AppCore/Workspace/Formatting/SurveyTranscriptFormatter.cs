using System.Text;
using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace.Formatting;

public sealed class SurveyTranscriptFormatter
{
    public string BuildProjectDraft(RequirementProject project, RequirementDocument? requirement)
    {
        var text = new StringBuilder();
        text.AppendLine($"# {project.Name}");
        if (!string.IsNullOrWhiteSpace(project.FeatureName))
        {
            text.AppendLine($"Funcionalidad: {project.FeatureName}");
        }

        text.AppendLine();
        text.AppendLine("## Contexto");
        text.AppendLine(string.IsNullOrWhiteSpace(project.Notes) ? "Pendiente de redactar." : project.Notes);
        if (requirement is not null)
        {
            text.AppendLine();
            text.AppendLine("## Requerimiento");
            text.AppendLine($"Titulo: {requirement.Title}");
            if (!string.IsNullOrWhiteSpace(requirement.Summary))
            {
                text.AppendLine(requirement.Summary);
            }
        }

        text.AppendLine();
        text.AppendLine("## Entrevistas");

        var surveys = requirement is null
            ? project.Surveys.OrderBy(item => item.CreatedAt).ToList()
            : project.Surveys.Where(item => requirement.SurveyIds.Contains(item.Id)).OrderBy(item => item.CreatedAt).ToList();

        foreach (var survey in surveys)
        {
            text.AppendLine();
            text.AppendLine($"### {survey.Title}");
            if (!string.IsNullOrWhiteSpace(survey.Interviewee))
            {
                text.AppendLine($"Usuario entrevistado: {survey.Interviewee}");
            }

            if (survey.AppliedTemplate is not null)
            {
                text.AppendLine($"Plantilla aplicada: {survey.AppliedTemplate.Name}");
            }

            text.AppendLine();
            text.AppendLine("Transcript:");
            text.AppendLine(BuildSurveyConversationCopy(survey, includeHeader: false));

            if (survey.AppliedTemplate is not null && survey.AppliedTemplate.MinuteSections.Count > 0)
            {
                text.AppendLine();
                text.AppendLine("Minuta sugerida:");
                foreach (var section in survey.AppliedTemplate.MinuteSections)
                {
                    text.AppendLine($"#### {section.Title}");
                    text.AppendLine(string.IsNullOrWhiteSpace(section.Prompt) ? "Pendiente de completar." : section.Prompt);
                    text.AppendLine();
                }
            }
        }

        text.AppendLine("## Requerimiento propuesto");
        text.AppendLine("- Como usuario, necesito...");
        text.AppendLine("- Criterios de aceptacion:");
        text.AppendLine("  - Dado...");
        text.AppendLine("  - Cuando...");
        text.AppendLine("  - Entonces...");
        return text.ToString().Trim();
    }

    public string BuildSurveyConversationCopy(UserSurvey survey, bool includeHeader = true)
    {
        var text = new StringBuilder();
        if (includeHeader)
        {
            text.AppendLine($"Encuesta: {survey.Title}");
            if (!string.IsNullOrWhiteSpace(survey.Interviewee))
            {
                text.AppendLine($"Entrevistado: {survey.Interviewee}");
            }

            if (!string.IsNullOrWhiteSpace(survey.Objective))
            {
                text.AppendLine($"Objetivo: {survey.Objective}");
            }
        }

        foreach (var group in GroupTurnsByQuestion(survey))
        {
            text.AppendLine();
            text.AppendLine($"## {group.QuestionLabel}");
            foreach (var turn in group.Turns)
            {
                var speaker = NormalizeSpeakerName(survey.Participants.FirstOrDefault(item => item.Id == turn.SpeakerId)?.DisplayName);
                var marker = turn.Important ? " [importante]" : string.Empty;
                text.AppendLine($"- [{turn.CreatedAt.ToLocalTime():HH:mm:ss}] {speaker}: {turn.Text}{marker}");
            }
        }

        return text.ToString().Trim();
    }

    public string BuildSurveyMinuteDraft(UserSurvey survey)
    {
        var text = new StringBuilder();
        text.AppendLine($"Minuta de {survey.Title}");
        if (!string.IsNullOrWhiteSpace(survey.Interviewee))
        {
            text.AppendLine($"Entrevistado: {survey.Interviewee}");
        }

        foreach (var group in GroupTurnsByQuestion(survey))
        {
            text.AppendLine();
            text.AppendLine($"### {group.QuestionLabel}");
            foreach (var turn in group.Turns)
            {
                var speaker = NormalizeSpeakerName(survey.Participants.FirstOrDefault(item => item.Id == turn.SpeakerId)?.DisplayName);
                text.AppendLine($"- [{turn.CreatedAt.ToLocalTime():HH:mm:ss}] {speaker}: {turn.Text}");
            }
        }

        if (survey.AppliedTemplate?.MinuteSections.Count > 0)
        {
            text.AppendLine();
            text.AppendLine("Notas sugeridas:");
            foreach (var section in survey.AppliedTemplate.MinuteSections)
            {
                text.AppendLine($"- {section.Title}: {section.Prompt}");
            }
        }

        return text.ToString().Trim();
    }

    public IReadOnlyList<QuestionTurnGroup> GroupTurnsByQuestion(UserSurvey survey)
    {
        return survey.TranscriptTurns
            .OrderBy(item => item.CreatedAt)
            .GroupBy(turn => ParseQuestionTag(turn.Tag)?.QuestionKey ?? "opening")
            .Select(group =>
            {
                var parsed = ParseQuestionTag(group.First().Tag);
                var questionLabel = ResolveQuestionLabel(survey, parsed);
                return new QuestionTurnGroup(
                    group.Key,
                    questionLabel,
                    group.ToList());
            })
            .ToList();
    }

    private static string ResolveQuestionLabel(UserSurvey survey, ParsedQuestionTag? parsed)
    {
        if (parsed is null)
        {
            return "Apertura de entrevista";
        }

        if (!string.IsNullOrWhiteSpace(parsed.QuestionLabel) &&
            !string.Equals(parsed.QuestionLabel, "undefined", StringComparison.OrdinalIgnoreCase))
        {
            return parsed.QuestionLabel;
        }

        var parts = parsed.QuestionKey.Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], out var sectionIndex) ||
            !int.TryParse(parts[1], out var questionIndex))
        {
            return "Pregunta sin titulo";
        }

        var section = survey.AppliedTemplate?.InterviewSections.ElementAtOrDefault(sectionIndex);
        var question = section?.Questions.ElementAtOrDefault(questionIndex);
        if (section is null)
        {
            return "Pregunta sin titulo";
        }

        return string.IsNullOrWhiteSpace(question)
            ? (string.IsNullOrWhiteSpace(section.Title) ? "Pregunta sin titulo" : section.Title)
            : $"{section.Title} - {question}";
    }

    private ParsedQuestionTag? ParseQuestionTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || !tag.StartsWith("question:", StringComparison.Ordinal))
        {
            return null;
        }

        var separatorIndex = tag.IndexOf('|');
        if (separatorIndex < 0)
        {
            return null;
        }

        return new ParsedQuestionTag(
            tag["question:".Length..separatorIndex],
            tag[(separatorIndex + 1)..]);
    }

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

public sealed record ParsedQuestionTag(string QuestionKey, string QuestionLabel);
public sealed record QuestionTurnGroup(string QuestionKey, string QuestionLabel, IReadOnlyList<SurveyTranscriptTurn> Turns);
