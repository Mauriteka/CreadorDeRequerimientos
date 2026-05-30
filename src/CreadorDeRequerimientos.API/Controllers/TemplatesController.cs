using CreadorDeRequerimientos.API.Controllers.Base;
using CreadorDeRequerimientos.API.Filters;
using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CreadorDeRequerimientos.API.Controllers;

[Route("api/templates")]
[ServiceFilter(typeof(WorkspaceAccessFilter))]
public sealed class TemplatesController(RequirementWorkspaceService service) : ApiController
{
    [HttpGet("system")]
    public async Task<ActionResult<IReadOnlyList<TemplateSummaryResponse>>> GetSystem(CancellationToken cancellationToken)
        => Ok(await service.GetSystemTemplatesAsync(cancellationToken));

    [HttpPost("system")]
    public async Task<ActionResult<TemplateDetailResponse>> CreateSystem(CreateTemplateRequest request, CancellationToken cancellationToken)
        => Ok(await service.CreateSystemTemplateAsync(request, cancellationToken));

    [HttpPut("system/{templateId:guid}")]
    public async Task<ActionResult<TemplateDetailResponse>> UpdateSystem(Guid templateId, UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await service.UpdateSystemTemplateAsync(templateId, request, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpDelete("system/{templateId:guid}")]
    public async Task<IActionResult> DeleteSystem(Guid templateId, CancellationToken cancellationToken)
        => await service.DeleteSystemTemplateAsync(templateId, cancellationToken) ? NoContent() : NotFound();
}
