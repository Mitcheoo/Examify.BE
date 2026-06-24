// Examify.Application/Cqrs/Queries/Submissions/GetMySubmissionsQueryHandler.cs
using MediatR;
using AutoMapper;
using Examify.Core.Interfaces;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetMySubmissionsQueryHandler : IRequestHandler<GetMySubmissionsQuery, List<MySubmissionItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMySubmissionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<MySubmissionItemDto>> Handle(GetMySubmissionsQuery request, CancellationToken cancellationToken)
    {
        var submissions = await _unitOfWork.Submissions
            .FindAsync(s => s.UserId == request.UserId
                         && s.SkillType == request.SkillType
                         && s.IsGraded);

        var submissionList = submissions.OrderByDescending(s => s.SubmittedAt).ToList();

        return _mapper.Map<List<MySubmissionItemDto>>(submissionList);
    }
}