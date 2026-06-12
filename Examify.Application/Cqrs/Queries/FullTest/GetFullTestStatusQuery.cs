using Examify.Application.DTOs.FullTest;
using MediatR;


namespace Examify.Application.Cqrs.Queries.FullTest
{
  public record GetFullTestStatusQuery(Guid FullTestId, Guid UserId): IRequest<FullTestStatusDto>;
}
