using System.Net;
using System.Net.Http.Json;

namespace SchoolAdmission.Api.Tests;

[Collection(ApiCollection.Name)]
public class ClassTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Created_class_is_returned_with_zero_seat_usage()
    {
        var response = await _client.PostAsJsonAsync("/classes", new { name = "  Class 6 - Morning Shift  ", totalSeats = 120 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.ReadAsync<ClassDto>();
        Assert.Equal("Class 6 - Morning Shift", created.Name);
        Assert.Equal(120, created.TotalSeats);

        var fetched = await _client.GetClassAsync(created.Id);
        Assert.Equal(new ClassDto(created.Id, "Class 6 - Morning Shift", 120, 0, 0, 120), fetched);
    }
}
