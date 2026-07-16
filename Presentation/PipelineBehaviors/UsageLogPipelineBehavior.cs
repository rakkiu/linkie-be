using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Domain.Entity;
using Domain.Interface;
using MediatR;

namespace Presentation.PipelineBehaviors
{
    public class UsageLogPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUsageLogRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsageLogPipelineBehavior(IUsageLogRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (ShouldSkipLogging(request))
                return await next();

            var httpContext = _httpContextAccessor.HttpContext;
            var userIdClaim = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return await next();

            var log = new UsageLog
            {
                UserId = Guid.Parse(userIdClaim),
                Action = ExtractActionName(request),
                EntityType = ExtractEntityType(request),
                EntityId = ExtractEntityId(request),
                Metadata = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                }),
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            try
            {
                var result = await next();

                await _repo.AddAsync(log, cancellationToken);
                await _repo.SaveChangesAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                log.Action = log.Action + "Fail";
                var errorMetadata = new
                {
                    Error = ex.Message,
                    OriginalRequest = request
                };
                log.Metadata = JsonSerializer.Serialize(errorMetadata, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });

                await _repo.AddAsync(log, cancellationToken);
                await _repo.SaveChangesAsync(cancellationToken);
                
                throw;
            }
        }

        private static bool ShouldSkipLogging(TRequest request)
        {
            var name = typeof(TRequest).Name;
            return name.EndsWith("Query");
        }

        private static string ExtractActionName(TRequest request)
        {
            var name = typeof(TRequest).Name;
            name = name.Replace("Command", "");
            name = name.Replace("Async", "");
            return name;
        }

        private static string? ExtractEntityType(TRequest request)
        {
            var props = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (prop.Name.EndsWith("Id") && prop.PropertyType == typeof(Guid))
                    return prop.Name.Replace("Id", "");
            }
            return null;
        }

        private static string? ExtractEntityId(TRequest request)
        {
            var props = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (prop.Name.EndsWith("Id") && prop.PropertyType == typeof(Guid))
                    return prop.GetValue(request)?.ToString();
            }
            return null;
        }
    }
}
