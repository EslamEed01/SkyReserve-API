using MediatR;
using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Passenger.Queries.Models
{
    public class GetAllPassengersQuery : IRequest<IEnumerable<PassengerDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}