namespace SchoolAdmission.Api.Errors;

public static class ErrorCodes
{
    public const string InvalidStudentId = "INVALID_STUDENT_ID";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string ClassNotFound = "CLASS_NOT_FOUND";
    public const string HoldNotFound = "HOLD_NOT_FOUND";
    public const string HoldNotOwned = "HOLD_NOT_OWNED";
    public const string ClassFull = "CLASS_FULL";
    public const string AlreadyApplied = "ALREADY_APPLIED";
    public const string HoldExpired = "HOLD_EXPIRED";
    public const string HoldCancelled = "HOLD_CANCELLED";
    public const string HoldAlreadyConfirmed = "HOLD_ALREADY_CONFIRMED";
    public const string NotFound = "NOT_FOUND";
    public const string BadRequest = "BAD_REQUEST";
    public const string InternalError = "INTERNAL_ERROR";
}
