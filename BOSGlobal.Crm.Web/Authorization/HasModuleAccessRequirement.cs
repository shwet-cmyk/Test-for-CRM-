using Microsoft.AspNetCore.Authorization;

namespace BOSGlobal.Crm.Web.Authorization;

public class HasModuleAccessRequirement : IAuthorizationRequirement
{
    public HasModuleAccessRequirement(string moduleKey)
    {
        ModuleKey = moduleKey;
    }

    public string ModuleKey { get; }
}
