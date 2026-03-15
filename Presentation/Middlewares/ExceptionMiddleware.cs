using FluentValidation;
using Presentation.Common;
using System.Net;
using System.Text.Json;

namespace Presentation.Middlewares // Updated namespace
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionMiddleware
    {
        /// <summary>
        /// The next
        /// </summary>
        private readonly RequestDelegate _next;
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        /// <param name="logger">The logger.</param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed");

                var response = new ApiResponse<string[]>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Validation failed",
                    Data = vex.Errors.Select(e => e.ErrorMessage).ToArray(),
                    ResponsedAt = DateTime.UtcNow
                };

                await WriteResponseAsync(context, response, HttpStatusCode.BadRequest);
            }
            catch (UnauthorizedAccessException uex)
            {
                _logger.LogWarning(uex, "Unauthorized");

                var response = new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = uex.Message,
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                };

                await WriteResponseAsync(context, response, HttpStatusCode.Unauthorized);
            }
            catch (KeyNotFoundException knfex)
            {
                _logger.LogWarning(knfex, "Resource not found");

                var response = new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = knfex.Message,
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                };

                await WriteResponseAsync(context, response, HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                var response = new ApiResponse<object>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = ex.Message,
                    Data = null,
                    ResponsedAt = DateTime.UtcNow
                };

                await WriteResponseAsync(context, response, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Writes the response asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">The context.</param>
        /// <param name="response">The response.</param>
        /// <param name="statusCode">The status code.</param>
        private static async Task WriteResponseAsync<T>(HttpContext context, ApiResponse<T> response, HttpStatusCode statusCode)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
