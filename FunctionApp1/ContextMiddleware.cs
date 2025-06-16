using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Primitives;

namespace Sunstealer.FunctionApp1;

public class ContextMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        StringValues flowId = string.Empty;
        StringValues spanId = string.Empty;

        context.GetHttpContext()?.Request.Headers.TryGetValue(Flow.FlowIdName, out flowId);
        context.GetHttpContext()?.Request.Headers.TryGetValue(Flow.SpanIdName, out spanId);

        Flow.SetContext(flowId, parentId: spanId);

        context.GetHttpContext()?.Response.OnStarting(state =>
        {
            foreach (var item in Flow.GetContextAsDictionary())
            {
                context.GetHttpContext()?.Response.Headers.Append(item.Key, item.Value);
            }
            return Task.CompletedTask;
        }, context);

        await next(context);
    }
}