using MediatR;
using SkyReserve.Application.Booking.DTOS;
using System.ComponentModel.DataAnnotations;

namespace SkyReserve.Application.Booking.Commands.Models
{
    public class CreateBookingCommand : IRequest<BookingDto>
    {
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public int FlightId { get; set; }
        
        [Required]
        public string FareClass { get; set; } = "Economy";
        
        [Required]
        public PassengerInfo Passenger { get; set; } = new();
    }

    public class PassengerInfo
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string PassportNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Nationality { get; set; } = string.Empty;
    }
}