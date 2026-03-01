using Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Auth.Attributes;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(Permission permission) 
        : base(policy: permission.ToString()) { }
}