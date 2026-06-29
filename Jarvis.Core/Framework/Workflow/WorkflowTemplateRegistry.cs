namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Provides an in-memory registry for reusable workflow templates.
/// </summary>
public sealed class WorkflowTemplateRegistry
{
    private readonly Dictionary<string, WorkflowTemplate> templates = new(StringComparer.OrdinalIgnoreCase);
    private readonly object gate = new();

    /// <summary>
    /// Registers or replaces a workflow template.
    /// </summary>
    /// <param name="template">The template to register.</param>
    public void Register(WorkflowTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (string.IsNullOrWhiteSpace(template.TemplateId))
        {
            throw new ArgumentException("Template identifier is required.", nameof(template));
        }

        lock (gate)
        {
            templates[template.TemplateId] = template;
        }
    }

    /// <summary>
    /// Attempts to get a workflow template.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="template">The template when found.</param>
    /// <returns><c>true</c> when the template exists.</returns>
    public bool TryGet(string templateId, out WorkflowTemplate? template)
    {
        lock (gate)
        {
            return templates.TryGetValue(templateId, out template);
        }
    }

    /// <summary>
    /// Lists registered workflow templates.
    /// </summary>
    /// <returns>The registered templates.</returns>
    public IReadOnlyList<WorkflowTemplate> List()
    {
        lock (gate)
        {
            return templates.Values.OrderBy(template => template.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
