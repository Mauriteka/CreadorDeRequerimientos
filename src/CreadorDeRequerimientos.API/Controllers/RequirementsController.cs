using CreadorDeRequerimientos.API.Controllers.Base;
using CreadorDeRequerimientos.API.Filters;
using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CreadorDeRequerimientos.API.Controllers;

[Route("api/projects/{projectId:guid}/requirements")]
[ServiceFilter(typeof(WorkspaceAccessFilter))]
public sealed class RequirementsController(RequirementWorkspaceService service) : ApiController
{
    [HttpPost]
    public async Task<ActionResult<ProjectDetailResponse>> Upsert(Guid projectId, UpsertRequirementRequest request, CancellationToken cancellationToken)
    {
        var project = await service.UpsertRequirementAsync(projectId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpDelete("{requirementId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> Delete(Guid projectId, Guid requirementId, CancellationToken cancellationToken)
    {
        var project = await service.DeleteRequirementAsync(projectId, requirementId, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost("draft")]
    public async Task<ActionResult<ProjectDetailResponse>> CreateDraft(Guid projectId, CreateDraftRequirementRequest request, CancellationToken cancellationToken)
    {
        var project = await service.CreateDraftRequirementAsync(projectId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }
}
