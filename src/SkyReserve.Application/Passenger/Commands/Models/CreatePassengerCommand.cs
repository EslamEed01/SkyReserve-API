using MediatR;
using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Passenger.Commands.Models
{
    public class CreatePassengerCommand : IRequest<PassengerDto>
    {
        public int BookingId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
    }
}