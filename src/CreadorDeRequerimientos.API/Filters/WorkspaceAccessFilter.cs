using CreadorDeRequerimientos.API.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CreadorDeRequerimientos.API.Filters;

public sealed class WorkspaceAccessFilter(WorkspaceAuthService authService) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!authService.IsEnabled || context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return Task.CompletedTask;
        }

        context.Result = new UnauthorizedResult();
        return Task.CompletedTask;
    }
}
