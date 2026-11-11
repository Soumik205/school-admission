using System.Net;

namespace SchoolAdmission.Api.Tests;

[Collection(ApiCollection.Name)]
public class ConfirmTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Repeated_parallel_confirms_count_the_seat_once()
    {
        var schoolClass = await _client.CreateClassAsync(totalSeats: 5);
        var studentId = TestApi.NewStudentId();
        var hold = await (await _client.HoldAsync(schoolClass.Id, studentId)).ReadAsync<HoldDto>();

        var responses = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(_ => _client.ConfirmAsync(hold.HoldId, studentId)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        foreach (var response in responses)
        {
            var body = await response.ReadAsync<HoldDto>();
            Assert.Equal(hold.HoldId, body.HoldId);
            Assert.Equal("CONFIRMED", body.Status);
        }

        var after = await _client.GetClassAsync(schoolClass.Id);
        Assert.Equal(1, after.ConfirmedSeats);
        Assert.Equal(0, after.HeldSeats);
        Assert.Equal(4, after.AvailableSeats);
    }

    [Fact]
    public async Task Another_student_cannot_confirm_a_hold()
    {
        var schoolClass = await _client.CreateClassAsync(totalSeats: 5);
        var hold = await (await _client.HoldAsync(schoolClass.Id, TestApi.NewStudentId())).ReadAsync<HoldDto>();

        var response = await _client.ConfirmAsync(hold.HoldId, TestApi.NewStudentId());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("HOLD_NOT_OWNED", (await response.ReadAsync<ProblemDto>()).Code);
        Assert.Equal("HELD", (await _client.GetHoldAsync(hold.HoldId)).Status);
        Assert.Equal(0, (await _client.GetClassAsync(schoolClass.Id)).ConfirmedSeats);
    }
}
