// Examify.Core/Interfaces/ITokenService.cs
using Examify.Core.Entities;

namespace Examify.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, List<string> roles);
}