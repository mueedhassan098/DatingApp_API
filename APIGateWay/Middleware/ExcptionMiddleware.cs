using APIGateWay.Errors;
using System.Net;
using System.Text.Json;

namespace APIGateWay.Middleware
{
    public class ExcptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExcptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExcptionMiddleware(RequestDelegate next, ILogger<ExcptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex,ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode=(int)HttpStatusCode.InternalServerError;

                var response = _environment.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) : 
                    new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");
                var options=new JsonSerializerOptions { PropertyNamingPolicy=JsonNamingPolicy.CamelCase };
                var json=JsonSerializer.Serialize(response, options);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
