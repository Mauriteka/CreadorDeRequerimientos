namespace CreadorDeRequerimientos.Mobile.Services;

public sealed record ApiEndpointOptions(Uri BaseUri)
{
    public static ApiEndpointOptions CreateFromEnvironment()
    {
        var configured = Environment.GetEnvironmentVariable("CREADOR_API_BASE_URL");
        if (Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
        {
            return new ApiEndpointOptions(EnsureTrailingSlash(configuredUri));
        }

#if ANDROID
        return new ApiEndpointOptions(new Uri("http://10.0.2.2:5046/"));
#elif WINDOWS
        return new ApiEndpointOptions(new Uri("http://localhost:5046/"));
#else
        return new ApiEndpointOptions(new Uri("http://localhost:5046/"));
#endif
    }

    private static Uri EnsureTrailingSlash(Uri uri)
    {
        var value = uri.ToString();
        return value.EndsWith("/", StringComparison.Ordinal) ? uri : new Uri(value + "/");
    }
}
