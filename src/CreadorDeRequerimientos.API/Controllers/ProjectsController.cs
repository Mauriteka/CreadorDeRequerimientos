using CreadorDeRequerimientos.API.Controllers.Base;
using CreadorDeRequerimientos.API.Filters;
using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CreadorDeRequerimientos.API.Controllers;

[Route("api/projects")]
[ServiceFilter(typeof(WorkspaceAccessFilter))]
public sealed class ProjectsController(RequirementWorkspaceService service) : ApiController
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectSummaryResponse>>> Get(CancellationToken cancellationToken)
        => Ok(await service.GetProjectsAsync(cancellationToken));

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> GetById(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await service.GetProjectAsync(projectId, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDetailResponse>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await service.CreateProjectAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId = project.Id }, project);
    }

    [HttpPut("{projectId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> Update(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await service.UpdateProjectAsync(projectId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken cancellationToken)
        => await service.DeleteProjectAsync(projectId, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("{projectId:guid}/templates")]
    public async Task<ActionResult<ProjectDetailResponse>> CreateTemplate(Guid projectId, CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var project = await service.CreateProjectTemplateAsync(projectId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPut("{projectId:guid}/templates/{templateId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> UpdateTemplate(Guid projectId, Guid templateId, UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        var project = await service.UpdateProjectTemplateAsync(projectId, templateId, request, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpDelete("{projectId:guid}/templates/{templateId:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> DeleteTemplate(Guid projectId, Guid templateId, CancellationToken cancellationToken)
    {
        var project = await service.DeleteProjectTemplateAsync(projectId, templateId, cancellationToken);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost("{projectId:guid}/templates/{templateId:guid}/export")]
    public async Task<ActionResult<ExportProjectTemplateResponse>> ExportTemplate(Guid projectId, Guid templateId, CancellationToken cancellationToken)
    {
        var exportResult = await service.ExportProjectTemplateAsync(projectId, templateId, cancellationToken);
        return exportResult is null ? NotFound() : Ok(exportResult);
    }
}
