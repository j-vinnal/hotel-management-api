using System.ComponentModel.DataAnnotations;
using Base.Contracts;

namespace App.DTO.Public.v1;

public class Booking : IEntityId
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public int? RoomNumber { get; set; } 
    public string? QuestFirstName { get; set; }
    public string? QuestLastName { get; set; }
    
    public Guid QuestId { get; set; }
    
    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
    public bool IsCancelled { get; set; } = false;

    [CustomValidation(typeof(Booking), nameof(ValidateDates))]
    public static ValidationResult? ValidateDates(Booking booking, ValidationContext context)
    {
        if (booking.EndDate < booking.StartDate)
        {
            return new ValidationResult("End date cannot be earlier than start date.", new[] { nameof(EndDate) });
        }
        return ValidationResult.Success;
    }
}
