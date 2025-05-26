using Identity.Identity.Dtos;

namespace Identity.Identity.Features.SwitchTenant;

public class SwitchTenantResponse
{
    public string AccessToken { get; set; }
    public TenantContext CurrentTenant { get; set; }
} 