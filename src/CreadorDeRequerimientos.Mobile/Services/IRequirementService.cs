using CreadorDeRequerimientos.Contracts;

namespace CreadorDeRequerimientos.Mobile.Services;

public interface IRequirementService
{
    Task<ProjectDetailResponse?> CreateDraftAsync(Guid projectId, CreateDraftRequirementRequest request, CancellationToken cancellationToken);
}
