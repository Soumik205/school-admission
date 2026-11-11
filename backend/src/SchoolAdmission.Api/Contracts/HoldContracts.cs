using SchoolAdmission.Api.Domain;

namespace SchoolAdmission.Api.Contracts;

public record HoldCreatedResponse(
    Guid HoldId,
    Guid ClassId,
    string ClassName,
    string StudentId,
    HoldStatus Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);
