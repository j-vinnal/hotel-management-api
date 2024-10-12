namespace App.DTO.Public.v1
{
    public class RoomAvailabilityRequest
    {
        public int? GuestCount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}