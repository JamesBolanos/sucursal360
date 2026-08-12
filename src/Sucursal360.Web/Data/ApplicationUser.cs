using Microsoft.AspNetCore.Identity;
using Sucursal360.Web.Domain.Entities;

namespace Sucursal360.Web.Data;

public class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; } = true;

    public Guid? AssignedBranchId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Branch? AssignedBranch { get; set; }
}
