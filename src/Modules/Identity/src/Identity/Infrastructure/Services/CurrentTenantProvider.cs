using Microsoft.AspNetCore.Http;

namespace Identity.Infrastructure.Services;

public class CurrentTenantProvider : ICurrentTenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string TenantIdKey = "X-TenantId";
    private const string TenantKeyHeader = "X-TenantKey";
    private const string UserIdKey = "X-UserId";

    private long? _overrideTenantId;
    private string _overrideTenantKey;
    private long? _overrideUserId;

    public CurrentTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? TenantId
    {
        get
        {
            if (_overrideTenantId.HasValue)
                return _overrideTenantId;

            if (_httpContextAccessor.HttpContext == null)
                return null;

            // Try to get from headers
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(TenantIdKey, out var tenantIdHeader))
            {
                if (long.TryParse(tenantIdHeader, out var tenantId))
                    return tenantId;
            }

            // Try to get from claims
            var tenantIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("tenantId");
            if (tenantIdClaim != null && long.TryParse(tenantIdClaim.Value, out var claimTenantId))
                return claimTenantId;

            return null;
        }
    }

    public string TenantKey
    {
        get
        {
            if (!string.IsNullOrEmpty(_overrideTenantKey))
                return _overrideTenantKey;

            if (_httpContextAccessor.HttpContext == null)
                return null;

            // Try to get from headers
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(TenantKeyHeader, out var tenantKeyHeader))
            {
                return tenantKeyHeader;
            }

            // Try to get from claims
            var tenantKeyClaim = _httpContextAccessor.HttpContext.User.FindFirst("tenantKey");
            if (tenantKeyClaim != null)
                return tenantKeyClaim.Value;

            return null;
        }
    }

    public long? UserId
    {
        get
        {
            if (_overrideUserId.HasValue)
                return _overrideUserId;

            if (_httpContextAccessor.HttpContext == null)
                return null;

            // Try to get from headers
            if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(UserIdKey, out var userIdHeader))
            {
                if (long.TryParse(userIdHeader, out var userId))
                    return userId;
            }

            // Try to get from claims
            var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirst("sub") ?? 
                              _httpContextAccessor.HttpContext.User.FindFirst("userId");
                              
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var claimUserId))
                return claimUserId;

            return null;
        }
    }

    public void SetTenant(long tenantId, string tenantKey)
    {
        _overrideTenantId = tenantId;
        _overrideTenantKey = tenantKey;
    }

    public void SetUser(long userId)
    {
        _overrideUserId = userId;
    }

    public void Clear()
    {
        _overrideTenantId = null;
        _overrideTenantKey = null;
        _overrideUserId = null;
    }
} 