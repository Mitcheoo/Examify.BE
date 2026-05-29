// Examify.Application/Cqrs/Queries/Submissions/GetSubmissionResultQueryHandler.cs
using AutoMapper;
using Examify.Application.DTOs.Submissions;
using Examify.Core.Exceptions;
using Examify.Core.Interfaces;
using MediatR;

namespace Examify.Application.Cqrs.Queries.Submissions;

public class GetSubmissionResultQueryHandler : IRequestHandler<GetSubmissionResultQuery, SubmissionResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSubmissionResultQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SubmissionResultDto> Handle(GetSubmissionResultQuery request, CancellationToken cancellationToken)
    {
        var submission = await _unitOfWork.Submissions.GetByIdAsync(request.SubmissionId);

        if (submission == null)
            throw new NotFoundException("Submission not found");

        // ✅ Map thêm UserId
        var result = _mapper.Map<SubmissionResultDto>(submission);
        result.UserId = submission.UserId;

        return result;
    }
}