/*// Examify.Application/Cqrs/Queries/Submissions/GetMySubmissionsQuery.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Queries.Submissions;

// ✅ Thêm SkillType vào record
public record GetMySubmissionsQuery(Guid UserId, int SkillType) : IRequest<List<MySubmissionItemDto>>;*/

using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Queries.Submissions;

// ✅ SỬA: SkillType là optional, có thể lọc hoặc lấy tất cả
public record GetMySubmissionsQuery(
    Guid UserId,
    int? SkillType = null,  // ✅ Optional
    int? Limit = null,      // ✅ Giới hạn số lượng
    int? Offset = null      // ✅ Phân trang
) : IRequest<List<MySubmissionItemDto>>;