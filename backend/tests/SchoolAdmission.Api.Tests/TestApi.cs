using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SchoolAdmission.Api.Tests;

public record ClassDto(string Id, string Name, int TotalSeats, int HeldSeats, int ConfirmedSeats, int AvailableSeats);

public record HoldDto(string HoldId, string ClassId, string Status, DateTime ExpiresAt);

public record ProblemDto(int Status, string Code);

public static class TestApi
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static async Task<ClassDto> CreateClassAsync(this HttpClient client, int totalSeats)
    {
        var response = await client.PostAsJsonAsync("/classes", new { name = $"Class {Guid.NewGuid()}", totalSeats });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClassDto>(Json))!;
    }

    public static async Task<ClassDto> GetClassAsync(this HttpClient client, string classId) =>
        (await client.GetFromJsonAsync<ClassDto>($"/classes/{classId}", Json))!;

    public static async Task<HoldDto> GetHoldAsync(this HttpClient client, string holdId) =>
        (await client.GetFromJsonAsync<HoldDto>($"/holds/{holdId}", Json))!;

    public static Task<HttpResponseMessage> HoldAsync(this HttpClient client, string classId, string? studentId) =>
        client.SendAsync(WithStudent(HttpMethod.Post, $"/classes/{classId}/holds", studentId));

    public static Task<HttpResponseMessage> ConfirmAsync(this HttpClient client, string holdId, string? studentId) =>
        client.SendAsync(WithStudent(HttpMethod.Post, $"/holds/{holdId}/confirm", studentId));

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    // Distinct, valid 17-digit birth registration numbers so tests never collide with each other.
    public static string NewStudentId() =>
        "2015" + Random.Shared.NextInt64(1_000_000_000_000, 9_999_999_999_999).ToString();

    private static HttpRequestMessage WithStudent(HttpMethod method, string url, string? studentId)
    {
        var request = new HttpRequestMessage(method, url);
        if (studentId is not null)
        {
            request.Headers.Add("X-Student-Id", studentId);
        }
        return request;
    }
}
