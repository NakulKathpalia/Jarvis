namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Represents a reusable workflow definition.
/// </summary>
public sealed class WorkflowTemplate
{
    /// <summary>
    /// Gets or sets the template identifier.
    /// </summary>
    public string TemplateId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Gets or sets the template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the template description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reusable workflow definition.
    /// </summary>
    public Workflow Workflow { get; set; } = new();

    /// <summary>
    /// Gets or sets the template version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";
}
