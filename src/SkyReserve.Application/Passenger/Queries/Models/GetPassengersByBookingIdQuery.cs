using MediatR;
using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Passenger.Queries.Models
{
    public class GetPassengersByBookingIdQuery : IRequest<IEnumerable<PassengerDto>>
    {
        public int BookingId { get; set; }
    }
}