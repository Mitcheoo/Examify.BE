// Examify.API/Controllers/ReadingController.cs
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Examify.Application.Cqrs.Commands.Reading;
using Examify.Application.Cqrs.Queries.Exercises;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Submissions;

namespace Examify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReadingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetExercise(Guid id)
    {
        var query = new GetExerciseQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<SubmissionResultDto>> Submit(SubmitReadingCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}