namespace Sunstealer.FunctionApp1;

public static class Flow
{
    public const string FlowIdName = "flowId";
    public const string SpanIdName = "spanId";
    public const string ParentIdName = "parentId";

    public static string CreateFlowId()
    {
        return Guid.NewGuid().ToString("N");
    }

    public static string CreateSpanId()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }

    public static (string flowId, string? parentId, string spanId) SetContext(string? flowId = null, string? parentId = null)
    {
        flowId ??= CreateFlowId();
        var spanId = CreateSpanId();

        CallContext.SetData(FlowIdName, flowId);
        CallContext.SetData(SpanIdName, spanId);

        if (parentId != null)
        {
            CallContext.SetData(ParentIdName, parentId);
        }
        return (flowId, parentId, spanId);
    }

    public static (string? flowId, string? parentId, string? spanId) GetContext()
    {
        return (
            CallContext.GetData(FlowIdName)?.ToString(),
            CallContext.GetData(ParentIdName)?.ToString(),
            CallContext.GetData(SpanIdName)?.ToString());
    }

    public static Dictionary<string, string?> GetContextAsDictionary()
    {
        var (contextFlowId, contextParentId, contextSpanId) = GetContext();
        return new Dictionary<string, string?>
        {
            { FlowIdName, contextFlowId },
            { ParentIdName, contextParentId },
            { SpanIdName, contextSpanId }
        }
        .Where(d => d.Value != null)
        .ToDictionary(k => k.Key, v => v.Value);

    }
}