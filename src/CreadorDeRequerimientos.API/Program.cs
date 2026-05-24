using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.AppCore.Workspace.Formatting;
using CreadorDeRequerimientos.AppCore.Workspace.Mapping;
using CreadorDeRequerimientos.AppCore.Workspace.Normalization;
using CreadorDeRequerimientos.AppCore.Workspace.Participants;
using CreadorDeRequerimientos.AppCore.Workspace.Templates;
using CreadorDeRequerimientos.API.Auth;
using CreadorDeRequerimientos.Contracts;
using CreadorDeRequerimientos.Infrastructure.Workspace;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var assignedPort = builder.Configuration["PORT"];
if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]) &&
    !string.IsNullOrWhiteSpace(assignedPort) &&
    int.TryParse(assignedPort, out var parsedPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{parsedPort}");
}

var dataFile = builder.Configuration["Workspace:DataFile"];
if (string.IsNullOrWhiteSpace(dataFile))
{
    dataFile = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data", "workspace.json"));
}

builder.Services.Configure<WorkspaceAuthOptions>(builder.Configuration.GetSection(WorkspaceAuthOptions.SectionName));
builder.Services.AddSingleton<WorkspaceAuthService>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "cr-auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<RequirementWorkspaceService>();
builder.Services.AddSingleton<DefaultSurveyTemplateFactory>();
builder.Services.AddSingleton<WorkspaceNormalizer>();
builder.Services.AddSingleton<SurveyParticipantFactory>();
builder.Services.AddSingleton<SurveyTranscriptFormatter>();
builder.Services.AddSingleton<WorkspaceResponseMapper>();
builder.Services.AddSingleton<IRequirementWorkspaceStore>(_ => new JsonRequirementWorkspaceStore(dataFile));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

var authApi = app.MapGroup("/api/auth");
var secureApi = app.MapGroup("/api").RequireAuthorization();

authApi.MapGet("/status", (HttpContext httpContext, WorkspaceAuthService authService) =>
    Results.Ok(new AuthStatusResponse(
        authService.IsEnabled,
        httpContext.User.Identity?.IsAuthenticated == true,
        httpContext.User.Identity?.IsAuthenticated == true ? httpContext.User.Identity?.Name : null)));

authApi.MapPost("/login", async (LoginRequest request, WorkspaceAuthService authService, HttpContext httpContext) =>
{
    if (!authService.IsEnabled)
    {
        return Results.BadRequest(new { message = "La autenticacion no esta configurada." });
    }

    if (!authService.ValidateCredentials(request.Username, request.Password))
    {
        return Results.Unauthorized();
    }

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, authService.Username)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Ok(new AuthStatusResponse(true, true, authService.Username));
});

authApi.MapPost("/logout", async (HttpContext httpContext, WorkspaceAuthService authService) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new AuthStatusResponse(authService.IsEnabled, false, null));
});

secureApi.MapGet("/projects", async (RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetProjectsAsync(cancellationToken)));

secureApi.MapGet("/templates/system", async (RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetSystemTemplatesAsync(cancellationToken)));

secureApi.MapPost("/templates/system", async (CreateTemplateRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.CreateSystemTemplateAsync(request, cancellationToken)));

secureApi.MapPut("/templates/system/{templateId:guid}", async (Guid templateId, UpdateTemplateRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.UpdateSystemTemplateAsync(templateId, request, cancellationToken) is { } template ? Results.Ok(template) : Results.NotFound());

secureApi.MapDelete("/templates/system/{templateId:guid}", async (Guid templateId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.DeleteSystemTemplateAsync(templateId, cancellationToken) ? Results.NoContent() : Results.NotFound());

secureApi.MapGet("/projects/{projectId:guid}", async (Guid projectId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.GetProjectAsync(projectId, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPost("/projects", async (CreateProjectRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
{
    var project = await service.CreateProjectAsync(request, cancellationToken);
    return Results.Created($"/api/projects/{project.Id}", project);
});

secureApi.MapPut("/projects/{projectId:guid}", async (Guid projectId, UpdateProjectRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.UpdateProjectAsync(projectId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapDelete("/projects/{projectId:guid}", async (Guid projectId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.DeleteProjectAsync(projectId, cancellationToken) ? Results.NoContent() : Results.NotFound());

secureApi.MapPost("/projects/{projectId:guid}/templates", async (Guid projectId, CreateTemplateRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.CreateProjectTemplateAsync(projectId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPut("/projects/{projectId:guid}/templates/{templateId:guid}", async (Guid projectId, Guid templateId, UpdateTemplateRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.UpdateProjectTemplateAsync(projectId, templateId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapDelete("/projects/{projectId:guid}/templates/{templateId:guid}", async (Guid projectId, Guid templateId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.DeleteProjectTemplateAsync(projectId, templateId, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPost("/projects/{projectId:guid}/templates/{templateId:guid}/export", async (Guid projectId, Guid templateId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.ExportProjectTemplateAsync(projectId, templateId, cancellationToken) is { } exportResult ? Results.Ok(exportResult) : Results.NotFound());

secureApi.MapPost("/projects/{projectId:guid}/surveys", async (Guid projectId, CreateSurveyRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.CreateSurveyAsync(projectId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPut("/projects/{projectId:guid}/surveys/{surveyId:guid}", async (Guid projectId, Guid surveyId, UpdateSurveyRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.UpdateSurveyAsync(projectId, surveyId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapDelete("/projects/{projectId:guid}/surveys/{surveyId:guid}", async (Guid projectId, Guid surveyId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.DeleteSurveyAsync(projectId, surveyId, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPost("/projects/{projectId:guid}/surveys/{surveyId:guid}/participants", async (Guid projectId, Guid surveyId, CreateSurveyParticipantRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.CreateParticipantAsync(projectId, surveyId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPut("/projects/{projectId:guid}/surveys/{surveyId:guid}/participants/{participantId:guid}", async (Guid projectId, Guid surveyId, Guid participantId, RenameParticipantRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.RenameParticipantAsync(projectId, surveyId, participantId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapDelete("/projects/{projectId:guid}/surveys/{surveyId:guid}/participants/{participantId:guid}", async (Guid projectId, Guid surveyId, Guid participantId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.DeleteParticipantAsync(projectId, surveyId, participantId, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPost("/projects/{projectId:guid}/surveys/{surveyId:guid}/turns", async (Guid projectId, Guid surveyId, AddTranscriptTurnRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.AddTranscriptTurnAsync(projectId, surveyId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPut("/projects/{projectId:guid}/surveys/{surveyId:guid}/turns/{turnId:guid}", async (Guid projectId, Guid surveyId, Guid turnId, UpdateTranscriptTurnRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.UpdateTranscriptTurnAsync(projectId, surveyId, turnId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapDelete("/projects/{projectId:guid}/surveys/{surveyId:guid}/turns/{turnId:guid}", async (Guid projectId, Guid surveyId, Guid turnId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.DeleteTranscriptTurnAsync(projectId, surveyId, turnId, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPost("/projects/{projectId:guid}/requirements", async (Guid projectId, UpsertRequirementRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.UpsertRequirementAsync(projectId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapDelete("/projects/{projectId:guid}/requirements/{requirementId:guid}", async (Guid projectId, Guid requirementId, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.DeleteRequirementAsync(projectId, requirementId, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

secureApi.MapPost("/projects/{projectId:guid}/requirements/draft", async (Guid projectId, CreateDraftRequirementRequest request, RequirementWorkspaceService service, CancellationToken cancellationToken) =>
    await service.CreateDraftRequirementAsync(projectId, request, cancellationToken) is { } project ? Results.Ok(project) : Results.NotFound());

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapFallbackToFile("index.html");

app.Run();
