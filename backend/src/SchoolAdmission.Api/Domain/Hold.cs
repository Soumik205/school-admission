namespace SchoolAdmission.Api.Domain;

public class Hold
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public SchoolClass Class { get; set; } = null!;
    public string StudentId { get; set; } = "";
    public HoldStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public HoldStatus EffectiveStatus(DateTime now) =>
        Status == HoldStatus.Held && ExpiresAt <= now ? HoldStatus.Expired : Status;
}
