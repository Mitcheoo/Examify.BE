// Examify.Application/Cqrs/Queries/Submissions/GetMySubmissionsQuery.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Queries.Submissions;

// ✅ Thêm SkillType vào record
public record GetMySubmissionsQuery(Guid UserId, int SkillType) : IRequest<List<MySubmissionItemDto>>;