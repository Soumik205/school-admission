using System.Net;

namespace SchoolAdmission.Api.Tests;

[Collection(ApiCollection.Name)]
public class ConcurrencyTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Parallel_holds_from_many_students_never_exceed_total_seats()
    {
        var schoolClass = await _client.CreateClassAsync(totalSeats: 10);

        var responses = await Task.WhenAll(Enumerable.Range(0, 200)
            .Select(_ => _client.HoldAsync(schoolClass.Id, TestApi.NewStudentId())));

        Assert.Equal(10, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        var rejected = responses.Where(r => r.StatusCode != HttpStatusCode.Created).ToList();
        Assert.Equal(190, rejected.Count);
        foreach (var response in rejected)
        {
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("CLASS_FULL", (await response.ReadAsync<ProblemDto>()).Code);
        }

        var after = await _client.GetClassAsync(schoolClass.Id);
        Assert.Equal(10, after.HeldSeats);
        Assert.Equal(0, after.AvailableSeats);
    }

    [Fact]
    public async Task Parallel_holds_from_one_student_create_exactly_one_hold()
    {
        var schoolClass = await _client.CreateClassAsync(totalSeats: 50);
        var studentId = TestApi.NewStudentId();

        var responses = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => _client.HoldAsync(schoolClass.Id, studentId)));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        foreach (var response in responses.Where(r => r.StatusCode != HttpStatusCode.Created))
        {
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("ALREADY_APPLIED", (await response.ReadAsync<ProblemDto>()).Code);
        }

        Assert.Equal(1, (await _client.GetClassAsync(schoolClass.Id)).HeldSeats);
    }
}
