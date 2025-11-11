using Elsa.Expressions.Models;
using Elsa.Extensions;
using Fluid;
using Fluid.Values;

namespace Elsa.Liquid.Helpers;

/// <summary>
/// Provides access to activity outputs through Liquid templating.
/// Supports syntax like {{ OutputFrom.activityId.outputName }}
/// </summary>
public class ActivityOutputAccessor
{
    private readonly ExpressionExecutionContext _context;
    private readonly string _activityId;
    private readonly TemplateOptions _options;

    public ActivityOutputAccessor(ExpressionExecutionContext context, string activityId, TemplateOptions options)
    {
        _context = context;
        _activityId = activityId;
        _options = options;
    }

    public Task<FluidValue> GetOutputAsync(string outputName)
    {
        var workflowExecutionContext = _context.GetWorkflowExecutionContext();
        var output = workflowExecutionContext.GetOutputByActivityId(_activityId, outputName);
        return Task.FromResult(output == null ? NilValue.Instance : FluidValue.Create(output, _options));
    }
}
