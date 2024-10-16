using System.ComponentModel.DataAnnotations;

namespace App.DTO.Public.v1
{
    public class RoomAvailabilityRequest
    {
        public int? GuestCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Guid? CurrentBookingId { get; set; }

        [CustomValidation(typeof(RoomAvailabilityRequest), nameof(ValidateDates))]
        public static ValidationResult? ValidateDates(RoomAvailabilityRequest request, ValidationContext context)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate)
            {
                return new ValidationResult("End date cannot be earlier than start date.", new[] { nameof(EndDate) });
            }
            return ValidationResult.Success;
        }
    }
}
