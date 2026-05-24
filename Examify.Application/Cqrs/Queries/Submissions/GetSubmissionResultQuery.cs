// Examify.Application/Cqrs/Queries/Submissions/GetSubmissionResultQuery.cs
using MediatR;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Queries.Submissions;

public record GetSubmissionResultQuery(Guid SubmissionId) : IRequest<SubmissionResultDto>;