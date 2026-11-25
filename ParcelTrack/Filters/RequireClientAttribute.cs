using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ParcelTrack.Filters
{
    // Part C: every write must say which program is calling (X-Client header).
    // No header or empty → 400 and the action does not run.
    // Otherwise logs before and after the action.
    public class RequireClientAttribute : ActionFilterAttribute
    {
        private const string SkippedKey = "RequireClient.Skipped";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string? client = context.HttpContext.Request.Headers["X-Client"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(client))
            {
                context.HttpContext.Items[SkippedKey] = true;
                context.Result = new BadRequestObjectResult(new
                {
                    message = "Say which program is calling: add an X-Client header"
                });
                return;
            }

            string actionName = context.ActionDescriptor.RouteValues["action"] ?? "action";
            Console.WriteLine($"X-Client: {client} -> {actionName}");
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Items.ContainsKey(SkippedKey))
            {
                return;
            }

            string actionName = context.ActionDescriptor.RouteValues["action"] ?? "action";
            Console.WriteLine($"{actionName} finished");
        }
    }
}
