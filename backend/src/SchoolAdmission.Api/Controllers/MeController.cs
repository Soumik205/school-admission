using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Api.Errors;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController(HoldService holds) : ControllerBase
{
    [HttpGet("holds")]
    public Task<List<HoldResponse>> Holds([FromHeader(Name = StudentIdHeader.Name)] string? studentId) =>
        holds.ListForStudentAsync(StudentIdHeader.Validate(studentId));
}
