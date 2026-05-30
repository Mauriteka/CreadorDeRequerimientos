using CreadorDeRequerimientos.API.Auth;
using CreadorDeRequerimientos.AppCore.Workspace;
using CreadorDeRequerimientos.AppCore.Workspace.Formatting;
using CreadorDeRequerimientos.AppCore.Workspace.Mapping;
using CreadorDeRequerimientos.AppCore.Workspace.Normalization;
using CreadorDeRequerimientos.AppCore.Workspace.Participants;
using CreadorDeRequerimientos.AppCore.Workspace.Templates;
using CreadorDeRequerimientos.API.Filters;
using CreadorDeRequerimientos.Infrastructure.Workspace;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CreadorDeRequerimientos.API.Configuration;

public static class ApplicationServiceExtensions
{
    public const string LocalClientsCorsPolicy = "LocalClients";

    public static WebApplicationBuilder ConfigureHostUrls(this WebApplicationBuilder builder)
    {
        var assignedPort = builder.Configuration["PORT"];
        if (string.IsNullOrWhiteSpace(builder.Configuration["urls"]) &&
            !string.IsNullOrWhiteSpace(assignedPort) &&
            int.TryParse(assignedPort, out var parsedPort))
        {
            builder.WebHost.UseUrls($"http://0.0.0.0:{parsedPort}");
        }

        return builder;
    }

    public static IServiceCollection AddWorkspaceApplication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var dataFile = ResolveWorkspacePath(configuration["Workspace:DataFile"], environment, "workspace.json");
        var databaseFile = ResolveWorkspacePath(configuration["Workspace:DatabaseFile"], environment, "workspace.db");
        var workspaceStorage = configuration["Workspace:Storage"]?.Trim().ToLowerInvariant();

        services.AddSingleton<RequirementWorkspaceService>();
        services.AddSingleton<DefaultSurveyTemplateFactory>();
        services.AddSingleton<WorkspaceNormalizer>();
        services.AddSingleton<SurveyParticipantFactory>();
        services.AddSingleton<SurveyTranscriptFormatter>();
        services.AddSingleton<WorkspaceResponseMapper>();
        services.AddScoped<WorkspaceAccessFilter>();
        services.AddSingleton<IRequirementWorkspaceStore>(_ => workspaceStorage switch
        {
            "json" => new JsonRequirementWorkspaceStore(dataFile),
            "sqlite" or null or "" => new SqliteRequirementWorkspaceStore(databaseFile, dataFile),
            _ => throw new InvalidOperationException($"Workspace:Storage '{workspaceStorage}' no es compatible. Usa 'sqlite' o 'json'.")
        });

        return services;
    }

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        return services;
    }

    public static IServiceCollection AddWorkspaceAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WorkspaceAuthOptions>(configuration.GetSection(WorkspaceAuthOptions.SectionName));
        services.AddSingleton<WorkspaceAuthService>();
        services
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
        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddLocalClientCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(LocalClientsCorsPolicy, policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    private static string ResolveWorkspacePath(string? configuredPath, IHostEnvironment environment, string fileName)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "..", "data", fileName));
        }

        return Path.IsPathFullyQualified(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configuredPath));
    }
}
