namespace SchoolAdmission.Api.Domain;

public class SchoolClass
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int TotalSeats { get; set; }
    public DateTime CreatedAt { get; set; }
}
