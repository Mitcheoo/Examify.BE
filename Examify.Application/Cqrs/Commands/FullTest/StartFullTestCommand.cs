// Examify.Application/Cqrs/Commands/FullTest/StartFullTestCommand.cs
using MediatR;
using Examify.Application.DTOs.FullTest;

namespace Examify.Application.Cqrs.Commands.FullTest;

// ✅ THÊM FullTestId
public record StartFullTestCommand(Guid UserId, Guid FullTestId) : IRequest<StartFullTestResponse>;