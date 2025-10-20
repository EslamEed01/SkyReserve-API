using MediatR;
using SkyReserve.Application.Passenger.DTOS;

namespace SkyReserve.Application.Passenger.Queries.Models
{
    public class GetPassengersByPassportNumberQuery : IRequest<IEnumerable<PassengerDto>>
    {
        public string PassportNumber { get; set; } = string.Empty;
    }
}