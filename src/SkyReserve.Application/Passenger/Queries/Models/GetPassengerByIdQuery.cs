using MediatR;
using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Passenger.Queries.Models
{
    public class GetPassengerByIdQuery : IRequest<PassengerDto?>
    {
        public int PassengerId { get; set; }
    }
}