using CreadorDeRequerimientos.API.Controllers.Base;
using CreadorDeRequerimientos.API.Filters;
using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CreadorDeRequerimientos.API.Controllers;

[Route("api/projects/{projectId:guid}/surveys")]
[ServiceFilter(typeof(WorkspaceAccessFilter))]
public sealed class SurveysController(RequirementWorkspaceService service) : ApiController
{
    [HttpPost]
    public async Task<ActionResult<ProjectDetailResponse>> Create(Guid projectId, CreateSurveyRequest request, CancellationToken cancellationToken)
    {
        var project = await service.CreateSurveyAsync(projectId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPut("{surveyId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> Update(Guid projectId, Guid surveyId, UpdateSurveyRequest request, CancellationToken cancellationToken)
    {
        var project = await service.UpdateSurveyAsync(projectId, surveyId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpDelete("{surveyId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> Delete(Guid projectId, Guid surveyId, CancellationToken cancellationToken)
    {
        var project = await service.DeleteSurveyAsync(projectId, surveyId, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost("{surveyId:guid}/participants")]
    public async Task<ActionResult<ProjectDetailResponse>> CreateParticipant(Guid projectId, Guid surveyId, CreateSurveyParticipantRequest request, CancellationToken cancellationToken)
    {
        var project = await service.CreateParticipantAsync(projectId, surveyId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPut("{surveyId:guid}/participants/{participantId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> RenameParticipant(Guid projectId, Guid surveyId, Guid participantId, RenameParticipantRequest request, CancellationToken cancellationToken)
    {
        var project = await service.RenameParticipantAsync(projectId, surveyId, participantId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpDelete("{surveyId:guid}/participants/{participantId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> DeleteParticipant(Guid projectId, Guid surveyId, Guid participantId, CancellationToken cancellationToken)
    {
        var project = await service.DeleteParticipantAsync(projectId, surveyId, participantId, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost("{surveyId:guid}/turns")]
    public async Task<ActionResult<ProjectDetailResponse>> AddTurn(Guid projectId, Guid surveyId, AddTranscriptTurnRequest request, CancellationToken cancellationToken)
    {
        var project = await service.AddTranscriptTurnAsync(projectId, surveyId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPut("{surveyId:guid}/turns/{turnId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> UpdateTurn(Guid projectId, Guid surveyId, Guid turnId, UpdateTranscriptTurnRequest request, CancellationToken cancellationToken)
    {
        var project = await service.UpdateTranscriptTurnAsync(projectId, surveyId, turnId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpDelete("{surveyId:guid}/turns/{turnId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> DeleteTurn(Guid projectId, Guid surveyId, Guid turnId, CancellationToken cancellationToken)
    {
        var project = await service.DeleteTranscriptTurnAsync(projectId, surveyId, turnId, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }
}
