using MongoDB.Bson.IO;
using System.Net;
using Newtonsoft.Json;

namespace DAIKIN.CheckSheetPortal.API.Setup
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;
        private readonly IWebHostEnvironment _env;
        public ErrorHandlingMiddleware(RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment env)
        {
            this.next = next;
            this.logger = logger;
            _env = env;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            const HttpStatusCode code = HttpStatusCode.InternalServerError;

            var error = $"There is an Internal server error. Please try again or contact the administrator.";
            if (_env.IsEnvironment("Development") || _env.IsEnvironment("local") || _env.IsEnvironment("Uat"))
            {
                error = exception.Message;
            }
            var result = Newtonsoft.Json.JsonConvert.SerializeObject(new { error = error });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var UserId = 0;//context.User.GetUserId();
            logger.LogError(exception, "{Message} {UserId}", exception.Message, UserId);

            return context.Response.WriteAsync(result);
        }
    }
}
