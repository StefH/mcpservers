using System.Reflection;

namespace ModelContextProtocolServer.Stdio;

public abstract class BaseServerOptions
{
    private readonly Lazy<Assembly> _entryAssembly = new(() => Assembly.GetEntryAssembly()!);

    private string? _name;
    private string? _version;

    /// <summary>
    /// Gets or sets the unique identifier for this item.
    /// </summary>
    public string Name
    {
        get => _name ??= GetDefaultApplicationName();
        set => _name = value;
    }

    /// <summary>
    /// Gets or sets the version of the implementation.
    /// </summary>
    /// <remarks>
    /// The version is used during client-server handshake to identify implementation versions,
    /// which can be important for troubleshooting compatibility issues or when reporting bugs.
    /// </remarks>
    public string Version
    {
        get => _version ??= GetDefaultVersion();
        set => _version = value;
    }

    /// <summary>
    /// Gets or sets optional server instructions to send to clients.
    /// </summary>
    /// <remarks>
    /// These instructions are sent to clients during the initialization handshake and provide
    /// guidance on how to effectively use the server's capabilities. They should focus on
    /// information that helps models use the server effectively and should not duplicate
    /// tool, prompt, or resource descriptions already exposed elsewhere.
    /// Client applications typically use these instructions as system messages for LLM interactions
    /// to provide context about available functionality.
    /// </remarks>
    public string? ServerInstructions { get; set; }

    /// <summary>
    /// The type of the server implementation, which is "sse" or "stdio".
    /// </summary>
    protected abstract string Type { get; }

    private string GetDefaultApplicationName()
    {
        var applicationName = _entryAssembly.Value?.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;

        return string.IsNullOrWhiteSpace(applicationName) ? $"mcpserver.{Guid.NewGuid()}{(string.IsNullOrEmpty(Type) ? string.Empty : $".{Type}")}" : applicationName;
    }

    private string GetDefaultVersion()
    {
        var informationalVersion = _entryAssembly.Value?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        return string.IsNullOrWhiteSpace(informationalVersion) ? "1.0.0" : informationalVersion.Split('+')[0];
    }
}
