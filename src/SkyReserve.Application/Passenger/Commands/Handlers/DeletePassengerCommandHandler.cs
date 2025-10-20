using MediatR;
using SkyReserve.Application.Passenger.Commands.Models;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Commands.Handlers
{
    public class DeletePassengerCommandHandler : IRequestHandler<DeletePassengerCommand, bool>
    {
        private readonly IPassengerRepository _passengerRepository;

        public DeletePassengerCommandHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<bool> Handle(DeletePassengerCommand request, CancellationToken cancellationToken)
        {
            return await _passengerRepository.DeleteAsync(request.PassengerId);
        }
    }
}