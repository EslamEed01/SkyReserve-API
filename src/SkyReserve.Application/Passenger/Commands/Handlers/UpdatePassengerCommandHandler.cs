using MediatR;
using SkyReserve.Application.Passenger.Commands.Models;
using SkyReserve.Application.Passenger.DTOS;
using SkyReserve.Application.Repository;

namespace SkyReserve.Application.Passenger.Commands.Handlers
{
    public class UpdatePassengerCommandHandler : IRequestHandler<UpdatePassengerCommand, PassengerDto>
    {
        private readonly IPassengerRepository _passengerRepository;

        public UpdatePassengerCommandHandler(IPassengerRepository passengerRepository)
        {
            _passengerRepository = passengerRepository;
        }

        public async Task<PassengerDto> Handle(UpdatePassengerCommand request, CancellationToken cancellationToken)
        {
            var updatePassengerDto = new UpdatePassengerDto
            {
                PassengerId = request.PassengerId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                PassportNumber = request.PassportNumber,
                Nationality = request.Nationality
            };

            return await _passengerRepository.UpdateAsync(updatePassengerDto);
        }
    }
}