using System.Diagnostics;

namespace ParcelTrack.Middleware
{
    // Part B: gives every request a short id, puts it on the response as X-Request-Id,
    // and writes one console line with id, method, path, status and elapsed time.
    public class RequestLogMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Short id before the request is passed on (headers can't change once the response has started).
            string requestId = Guid.NewGuid().ToString("N")[..8];
            context.Response.Headers["X-Request-Id"] = requestId;

            Stopwatch stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            // One console line, including 401s and 403s.
            Console.WriteLine(
                $"[{requestId}] {context.Request.Method} {context.Request.Path} -> {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
