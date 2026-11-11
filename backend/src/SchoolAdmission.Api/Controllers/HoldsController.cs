using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Api.Errors;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
public class HoldsController(HoldService holds) : ControllerBase
{
    [HttpPost("classes/{classId}/holds")]
    public async Task<ActionResult<HoldCreatedResponse>> Create(
        string classId,
        [FromHeader(Name = StudentIdHeader.Name)] string? studentId)
    {
        var created = await holds.CreateAsync(classId, StudentIdHeader.Validate(studentId));
        return Created($"/holds/{created.HoldId}", created);
    }

    [HttpGet("holds/{holdId}")]
    public Task<HoldResponse> Get(string holdId) => holds.GetAsync(holdId);

    [HttpPost("holds/{holdId}/confirm")]
    public Task<HoldResponse> Confirm(string holdId, [FromHeader(Name = StudentIdHeader.Name)] string? studentId) =>
        holds.ConfirmAsync(holdId, StudentIdHeader.Validate(studentId));

    [HttpDelete("holds/{holdId}")]
    public async Task<IActionResult> Cancel(string holdId, [FromHeader(Name = StudentIdHeader.Name)] string? studentId)
    {
        await holds.CancelAsync(holdId, StudentIdHeader.Validate(studentId));
        return NoContent();
    }
}
