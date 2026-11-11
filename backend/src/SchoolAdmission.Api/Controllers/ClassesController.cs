using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Contracts;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Controllers;

[ApiController]
[Route("classes")]
public class ClassesController(ClassService classes) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ClassResponse>> Create(CreateClassRequest request)
    {
        var created = await classes.CreateAsync(request);
        return Created($"/classes/{created.Id}", created);
    }

    [HttpGet]
    public Task<List<ClassResponse>> List() => classes.ListAsync();

    [HttpGet("{classId}")]
    public Task<ClassResponse> Get(string classId) => classes.GetAsync(classId);
}
