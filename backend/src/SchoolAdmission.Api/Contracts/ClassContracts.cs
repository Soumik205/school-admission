using System.ComponentModel.DataAnnotations;

namespace SchoolAdmission.Api.Contracts;

public record CreateClassRequest(
    [Required(ErrorMessage = "Class name is required.")]
    [StringLength(100, ErrorMessage = "Class name must be at most 100 characters.")]
    string? Name,
    [Required(ErrorMessage = "Total seats is required.")]
    [Range(1, 10000, ErrorMessage = "Total seats must be a whole number from 1 to 10000.")]
    int? TotalSeats);

public record ClassResponse(
    Guid Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats,
    int AvailableSeats);
