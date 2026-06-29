namespace Jarvis.Core.Framework.Workflow;

/// <summary>
/// Validates workflow structure before execution.
/// </summary>
public sealed class WorkflowValidator
{
    /// <summary>
    /// Validates that a workflow can be executed.
    /// </summary>
    /// <param name="workflow">The workflow to validate.</param>
    public void Validate(Workflow workflow)
    {
        ArgumentNullException.ThrowIfNull(workflow);

        if (workflow.Steps.Count == 0)
        {
            throw new InvalidOperationException("Workflow must contain at least one step.");
        }

        var ids = new HashSet<string>(workflow.Steps.Select(step => step.Id), StringComparer.OrdinalIgnoreCase);
        if (ids.Count != workflow.Steps.Count)
        {
            throw new InvalidOperationException("Workflow step identifiers must be unique.");
        }

        ValidateSteps(workflow.Steps, ids);
        ValidateBranches(workflow, ids);
        EnsureNoDependencyCycles(workflow.Steps);
    }

    private static void ValidateSteps(IEnumerable<WorkflowStep> steps, HashSet<string> ids)
    {
        foreach (var step in steps)
        {
            if (string.IsNullOrWhiteSpace(step.TaskType))
            {
                throw new InvalidOperationException($"Workflow step '{step.Id}' must define a task type.");
            }

            foreach (var dependency in step.Dependencies)
            {
                if (!ids.Contains(dependency))
                {
                    throw new InvalidOperationException($"Workflow step '{step.Id}' depends on unknown step '{dependency}'.");
                }
            }
        }
    }

    private static void ValidateBranches(Workflow workflow, HashSet<string> stepIds)
    {
        var branchIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var branch in workflow.Branches)
        {
            if (!branchIds.Add(branch.Id))
            {
                throw new InvalidOperationException("Workflow branch identifiers must be unique.");
            }

            if (!stepIds.Contains(branch.Condition.SourceStepId))
            {
                throw new InvalidOperationException($"Workflow branch '{branch.Id}' depends on unknown source step '{branch.Condition.SourceStepId}'.");
            }

            foreach (var branchStepId in branch.IfStepIds.Concat(branch.ElseStepIds))
            {
                if (!stepIds.Contains(branchStepId))
                {
                    throw new InvalidOperationException($"Workflow branch '{branch.Id}' references unknown step '{branchStepId}'.");
                }
            }

            if (!string.IsNullOrWhiteSpace(branch.EndStepId) && !stepIds.Contains(branch.EndStepId))
            {
                throw new InvalidOperationException($"Workflow branch '{branch.Id}' references unknown end step '{branch.EndStepId}'.");
            }
        }
    }

    private static void EnsureNoDependencyCycles(IReadOnlyCollection<WorkflowStep> steps)
    {
        var byId = steps.ToDictionary(step => step.Id, StringComparer.OrdinalIgnoreCase);
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var step in steps)
        {
            Visit(step.Id);
        }

        void Visit(string stepId)
        {
            if (visited.Contains(stepId))
            {
                return;
            }

            if (!visiting.Add(stepId))
            {
                throw new InvalidOperationException("Workflow dependencies contain a cycle.");
            }

            foreach (var dependency in byId[stepId].Dependencies)
            {
                Visit(dependency);
            }

            visiting.Remove(stepId);
            visited.Add(stepId);
        }
    }
}
