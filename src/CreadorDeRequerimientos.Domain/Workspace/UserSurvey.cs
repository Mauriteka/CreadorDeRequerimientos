namespace CreadorDeRequerimientos.Domain.Workspace;

public sealed class UserSurvey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Interviewee { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string OwnerEmail { get; set; } = string.Empty;
    public string IntervieweeEmail { get; set; } = string.Empty;
    public string ExtraEmails { get; set; } = string.Empty;
    public string MinuteDraft { get; set; } = string.Empty;
    public bool IsFinalized { get; set; }
    public DateTimeOffset? FinalizedAt { get; set; }
    public SurveyTemplateSnapshot? AppliedTemplate { get; set; }
    public List<SurveyParticipant> Participants { get; set; } = [];
    public List<SurveyTranscriptTurn> TranscriptTurns { get; set; } = [];
    public List<SurveyMention> Mentions { get; set; } = [];

    public static UserSurvey Create(string title, string interviewee, string objective, SurveyTemplateSnapshot? appliedTemplate)
    {
        var survey = new UserSurvey
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Encuesta sin titulo" : title.Trim(),
            Interviewee = interviewee.Trim(),
            Objective = objective.Trim(),
            AppliedTemplate = appliedTemplate
        };

        survey.Participants.Add(new SurveyParticipant
        {
            DisplayName = "Yo",
            RoleType = "Self",
            SortOrder = 0
        });

        survey.Participants.Add(new SurveyParticipant
        {
            DisplayName = "Persona 1",
            RoleType = "GuestPlaceholder",
            SortOrder = 1
        });

        return survey;
    }

    public void Update(
        string title,
        string interviewee,
        string objective,
        string ownerEmail,
        string intervieweeEmail,
        string extraEmails,
        string minuteDraft,
        bool isFinalized)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Encuesta sin titulo" : title.Trim();
        Interviewee = interviewee.Trim();
        Objective = objective.Trim();
        OwnerEmail = ownerEmail.Trim();
        IntervieweeEmail = intervieweeEmail.Trim();
        ExtraEmails = extraEmails.Trim();
        MinuteDraft = minuteDraft.Trim();
        if (isFinalized && !IsFinalized)
        {
            FinalizedAt = DateTimeOffset.UtcNow;
        }
        else if (!isFinalized)
        {
            FinalizedAt = null;
        }

        IsFinalized = isFinalized;
        Touch();
    }

    public SurveyTranscriptTurn AddTranscriptTurn(Guid speakerId, string text, string tag, bool important, string sourceType)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("El turno no puede estar vacio.", nameof(text));
        }

        var turn = new SurveyTranscriptTurn();
        turn.Update(speakerId, text, tag, important, sourceType);
        turn.CreatedAt = turn.UpdatedAt;

        TranscriptTurns.Add(turn);
        Touch();
        return turn;
    }

    public SurveyMention AddMention(string text, string tag, bool important)
    {
        var mention = new SurveyMention
        {
            Text = text.Trim(),
            Tag = tag.Trim(),
            Important = important
        };

        Mentions.Add(mention);
        Touch();
        return mention;
    }

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
