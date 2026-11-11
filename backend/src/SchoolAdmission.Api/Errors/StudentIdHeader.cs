namespace SchoolAdmission.Api.Errors;

public static class StudentIdHeader
{
    public const string Name = "X-Student-Id";

    public static string Validate(string? value)
    {
        var trimmed = value?.Trim();
        if (trimmed is not { Length: 17 } || !trimmed.All(char.IsAsciiDigit))
        {
            throw new ApiException(StatusCodes.Status400BadRequest, ErrorCodes.InvalidStudentId,
                "Invalid student id", $"The {Name} header must be exactly 17 digits.");
        }
        return trimmed;
    }
}
