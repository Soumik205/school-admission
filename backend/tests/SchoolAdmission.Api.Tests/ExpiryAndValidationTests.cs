using System.Net;

namespace SchoolAdmission.Api.Tests;

[Collection(ApiCollection.Name)]
public class ExpiryAndValidationTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Expired_hold_frees_its_seat_without_any_api_call()
    {
        var schoolClass = await _client.CreateClassAsync(totalSeats: 1);
        var studentId = TestApi.NewStudentId();
        var hold = await (await _client.HoldAsync(schoolClass.Id, studentId)).ReadAsync<HoldDto>();
        Assert.Equal(0, (await _client.GetClassAsync(schoolClass.Id)).AvailableSeats);

        factory.Time.Advance(TimeSpan.FromSeconds(601));

        Assert.Equal("EXPIRED", (await _client.GetHoldAsync(hold.HoldId)).Status);
        Assert.Equal(1, (await _client.GetClassAsync(schoolClass.Id)).AvailableSeats);

        var confirm = await _client.ConfirmAsync(hold.HoldId, studentId);
        Assert.Equal(HttpStatusCode.Conflict, confirm.StatusCode);
        Assert.Equal("HOLD_EXPIRED", (await confirm.ReadAsync<ProblemDto>()).Code);

        var again = await _client.HoldAsync(schoolClass.Id, studentId);
        Assert.Equal(HttpStatusCode.Created, again.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234567890123456")]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456a")]
    public async Task Missing_or_malformed_student_id_is_rejected(string? studentId)
    {
        var schoolClass = await _client.CreateClassAsync(totalSeats: 1);

        var response = await _client.HoldAsync(schoolClass.Id, studentId);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("INVALID_STUDENT_ID", (await response.ReadAsync<ProblemDto>()).Code);
        Assert.Equal(1, (await _client.GetClassAsync(schoolClass.Id)).AvailableSeats);
    }
}
