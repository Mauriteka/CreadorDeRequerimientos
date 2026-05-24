using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Domain.Workspace;

namespace CreadorDeRequerimientos.AppCore.Workspace.Participants;

public sealed class SurveyParticipantFactory
{
    public SurveyParticipant CreateFromRequest(UserSurvey survey, CreateSurveyParticipantRequest request)
    {
        var roleType = string.IsNullOrWhiteSpace(request.RoleType) ? "GuestPlaceholder" : request.RoleType.Trim();
        var name = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = roleType == "Self" ? "Yo" : $"Persona {survey.Participants.Count(item => item.RoleType != "Self") + 1}";
        }

        return new SurveyParticipant
        {
            DisplayName = name,
            Email = string.Empty,
            RoleType = roleType,
            SortOrder = survey.Participants.Count
        };
    }
}
