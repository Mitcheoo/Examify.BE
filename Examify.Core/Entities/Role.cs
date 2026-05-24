// Examify.Core/Entities/Role.cs
using Microsoft.AspNetCore.Identity;

namespace Examify.Core.Entities;

public class Role : IdentityRole<Guid>
{
    public string? Description { get; set; }
}