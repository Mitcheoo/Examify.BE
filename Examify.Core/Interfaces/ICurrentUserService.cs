// Examify.Core/Interfaces/ICurrentUserService.cs
namespace Examify.Core.Interfaces;

public interface ICurrentUserService
{
    /// <summary>
    /// ID của người dùng hiện tại
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Email của người dùng hiện tại
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Username của người dùng hiện tại
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Kiểm tra xem người dùng đã đăng nhập chưa
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Kiểm tra xem người dùng có role Admin không
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Lấy danh sách roles của người dùng hiện tại
    /// </summary>
    IList<string> GetRoles();
}