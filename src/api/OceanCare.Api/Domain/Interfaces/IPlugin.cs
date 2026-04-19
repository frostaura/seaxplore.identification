namespace OceanCare.Api.Domain.Interfaces;

/// <summary>
/// Base interface for the OceanCare plugin architecture.
/// All plugins must implement this interface.
/// </summary>
public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
}
