using MediatR;

namespace SkyReserve.Application.Passenger.Commands.Models
{
    public class DeletePassengerCommand : IRequest<bool>
    {
        public int PassengerId { get; set; }
    }
}